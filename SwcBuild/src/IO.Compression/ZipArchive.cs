using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SwcBuild.IO.Compression
{
	[__DynamicallyInvokable]
	public class ZipArchive : IDisposable
	{
	    private byte[] _archiveComment;
	    private BinaryReader _archiveReader;
	    private Stream _archiveStream;
	    private ZipArchiveEntry _archiveStreamOwner;
	    private Stream _backingStream;
	    private long _centralDirectoryStart;
	    private List<ZipArchiveEntry> _entries;
	    private ReadOnlyCollection<ZipArchiveEntry> _entriesCollection;
	    private Dictionary<string, ZipArchiveEntry> _entriesDictionary;
	    private Encoding _entryNameEncoding;
	    private long _expectedNumberOfEntries;
	    private bool _isDisposed;
	    private bool _leaveOpen;
	    private ZipArchiveMode _mode;
	    private uint _numberOfThisDisk;
	    private bool _readEntries;

		[__DynamicallyInvokable]
		public ZipArchive(Stream stream) : this(stream, ZipArchiveMode.Read, false, null) { }

		[__DynamicallyInvokable]
		public ZipArchive(Stream stream, ZipArchiveMode mode) : this(stream, mode, false, null) { }

		[__DynamicallyInvokable]
		public ZipArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen) : this(stream, mode, leaveOpen, null) { }

		[__DynamicallyInvokable]
		public ZipArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen, Encoding entryNameEncoding)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			EntryNameEncoding = entryNameEncoding;
			Init(stream, mode, leaveOpen);
		}

		internal void AcquireArchiveStream(ZipArchiveEntry entry)
		{
			if (_archiveStreamOwner != null)
			{
				if (_archiveStreamOwner.EverOpenedForWrite)
					throw new IOException(Messages.CreateModeCreateEntryWhileOpen);
				
				_archiveStreamOwner.WriteAndFinishLocalEntry();
			}
			_archiveStreamOwner = entry;
		}

	    private void AddEntry(ZipArchiveEntry entry)
		{
			_entries.Add(entry);
			string fullName = entry.FullName;
			if (!_entriesDictionary.ContainsKey(fullName))
			{
				_entriesDictionary.Add(fullName, entry);
			}
		}

	    private void CloseStreams()
		{
			if (!_leaveOpen)
			{
				ArchiveStream1.Close();
				if (_backingStream != null)
				{
					_backingStream.Close();
				}
				if (_archiveReader != null)
				{
					_archiveReader.Close();
				}
			}
			else if (_backingStream != null)
			{
				ArchiveStream1.Close();
			}
		}

		[__DynamicallyInvokable]
		public ZipArchiveEntry CreateEntry(string entryName)
		{
			return DoCreateEntry(entryName, null);
		}

		[__DynamicallyInvokable]
		public ZipArchiveEntry CreateEntry(string entryName, CompressionLevel compressionLevel)
		{
			return DoCreateEntry(entryName, new CompressionLevel?(compressionLevel));
		}

		[__DynamicallyInvokable]
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[__DynamicallyInvokable]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				switch (_mode)
				{
					case ZipArchiveMode.Read:
						break;

					default:
						try
						{
							WriteFile();
						}
						catch (InvalidDataException)
						{
							CloseStreams();
							_isDisposed = true;
							throw;
						}
						break;
				}
				CloseStreams();
				_isDisposed = true;
			}
		}

	    private ZipArchiveEntry DoCreateEntry(string entryName, CompressionLevel? compressionLevel)
		{
			if (entryName == null)
			{
				throw new ArgumentNullException("entryName");
			}
			if (string.IsNullOrEmpty(entryName))
			{
				throw new ArgumentException(Messages.CannotBeEmpty, "entryName");
			}
			if (_mode == ZipArchiveMode.Read)
			{
				throw new NotSupportedException(Messages.CreateInReadMode);
			}
			ThrowIfDisposed();
			ZipArchiveEntry entry = compressionLevel.HasValue ? new ZipArchiveEntry(this, entryName, compressionLevel.Value) : new ZipArchiveEntry(this, entryName);
			AddEntry(entry);
			return entry;
		}

	    private void EnsureCentralDirectoryRead()
		{
			if (!_readEntries)
			{
				ReadCentralDirectory();
				_readEntries = true;
			}
		}

		[__DynamicallyInvokable]
		public ZipArchiveEntry GetEntry(string entryName)
		{
			ZipArchiveEntry entry;
			if (entryName == null)
			{
				throw new ArgumentNullException("entryName");
			}
			if (_mode == ZipArchiveMode.Create)
			{
				throw new NotSupportedException(Messages.EntriesInCreateMode);
			}
			EnsureCentralDirectoryRead();
			_entriesDictionary.TryGetValue(entryName, out entry);
			return entry;
		}

	    private void Init(Stream stream, ZipArchiveMode mode, bool leaveOpen)
		{
			Stream stream2 = null;
			try
			{
				_backingStream = null;
				switch (mode)
				{
					case ZipArchiveMode.Read:
						if (!stream.CanRead)
						{
							throw new ArgumentException(Messages.ReadModeCapabilities);
						}
						break;

					case ZipArchiveMode.Create:
						if (!stream.CanWrite)
						{
							throw new ArgumentException(Messages.CreateModeCapabilities);
						}
						goto Label_00A1;

					case ZipArchiveMode.Update:
						if ((!stream.CanRead || !stream.CanWrite) || !stream.CanSeek)
						{
							throw new ArgumentException(Messages.UpdateModeCapabilities);
						}
						goto Label_00A1;

					default:
						throw new ArgumentOutOfRangeException("mode");
				}
				if (!stream.CanSeek)
				{
					_backingStream = stream;
					stream2 = stream = new MemoryStream();
					_backingStream.CopyTo(stream);
					stream.Seek(0L, SeekOrigin.Begin);
				}
			Label_00A1:
				_mode = mode;
				ArchiveStream1 = stream;
				_archiveStreamOwner = null;
				if (mode == ZipArchiveMode.Create)
				{
					_archiveReader = null;
				}
				else
				{
					_archiveReader = new BinaryReader(stream);
				}
				_entries = new List<ZipArchiveEntry>();
				_entriesCollection = new ReadOnlyCollection<ZipArchiveEntry>(_entries);
				_entriesDictionary = new Dictionary<string, ZipArchiveEntry>();
				_readEntries = false;
				_leaveOpen = leaveOpen;
				_centralDirectoryStart = 0L;
				_isDisposed = false;
				_numberOfThisDisk = 0;
				_archiveComment = null;
				switch (mode)
				{
					case ZipArchiveMode.Read:
						ReadEndOfCentralDirectory();
						return;

					case ZipArchiveMode.Create:
						_readEntries = true;
						return;
				}
				if (ArchiveStream1.Length == 0)
				{
					_readEntries = true;
				}
				else
				{
					ReadEndOfCentralDirectory();
					EnsureCentralDirectoryRead();
					using (List<ZipArchiveEntry>.Enumerator enumerator = _entries.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							enumerator.Current.ThrowIfNotOpenable(false, true);
						}
					}
				}
			}
			catch
			{
				if (stream2 != null)
				{
					stream2.Close();
				}
				throw;
			}
		}

		internal bool IsStillArchiveStreamOwner(ZipArchiveEntry entry)
		{
			return (_archiveStreamOwner == entry);
		}

	    private void ReadCentralDirectory()
		{
			try
			{
				ZipCentralDirectoryFileHeader header;
				ArchiveStream1.Seek(_centralDirectoryStart, SeekOrigin.Begin);
				long num = 0L;
				bool saveExtraFieldsAndComments = Mode == ZipArchiveMode.Update;
				while (ZipCentralDirectoryFileHeader.TryReadBlock(_archiveReader, saveExtraFieldsAndComments, out header))
				{
					AddEntry(new ZipArchiveEntry(this, header));
					num += 1L;
				}
				if (num != _expectedNumberOfEntries)
				{
					throw new InvalidDataException(Messages.NumEntriesWrong);
				}
			}
			catch (EndOfStreamException exception)
			{
				throw new InvalidDataException(Messages.CentralDirectoryInvalid, exception);
			}
		}

	    private void ReadEndOfCentralDirectory()
		{
			try
			{
				ZipEndOfCentralDirectoryBlock block;
				ArchiveStream1.Seek(-18L, SeekOrigin.End);
				if (!ZipHelper.SeekBackwardsToSignature(ArchiveStream1, 0x6054b50))
				{
					throw new InvalidDataException(Messages.EOCDNotFound);
				}
				long position = ArchiveStream1.Position;
				ZipEndOfCentralDirectoryBlock.TryReadBlock(_archiveReader, out block);
				if (block.NumberOfThisDisk != block.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
				{
					throw new InvalidDataException(Messages.SplitSpanned);
				}
				_numberOfThisDisk = block.NumberOfThisDisk;
				_centralDirectoryStart = block.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
				if (block.NumberOfEntriesInTheCentralDirectory != block.NumberOfEntriesInTheCentralDirectoryOnThisDisk)
				{
					throw new InvalidDataException(Messages.SplitSpanned);
				}
				_expectedNumberOfEntries = block.NumberOfEntriesInTheCentralDirectory;
				if (_mode == ZipArchiveMode.Update)
				{
					_archiveComment = block.ArchiveComment;
				}
				if (((block.NumberOfThisDisk == 0xffff) || (block.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber == uint.MaxValue)) || (block.NumberOfEntriesInTheCentralDirectory == 0xffff))
				{
					ArchiveStream1.Seek(position - 0x10L, SeekOrigin.Begin);
					if (ZipHelper.SeekBackwardsToSignature(ArchiveStream1, 0x7064b50))
					{
						Zip64EndOfCentralDirectoryLocator locator;
						Zip64EndOfCentralDirectoryRecord record;
						Zip64EndOfCentralDirectoryLocator.TryReadBlock(_archiveReader, out locator);
						if (locator.OffsetOfZip64EOCD > 0x7fffffffffffffffL)
						{
							throw new InvalidDataException(Messages.FieldTooBigOffsetToZip64EOCD);
						}
						long offset = (long) locator.OffsetOfZip64EOCD;
						ArchiveStream1.Seek(offset, SeekOrigin.Begin);
						if (!Zip64EndOfCentralDirectoryRecord.TryReadBlock(_archiveReader, out record))
						{
							throw new InvalidDataException(Messages.Zip64EOCDNotWhereExpected);
						}
						_numberOfThisDisk = record.NumberOfThisDisk;
						if (record.NumberOfEntriesTotal > 0x7fffffffffffffffL)
						{
							throw new InvalidDataException(Messages.FieldTooBigNumEntries);
						}
						if (record.OffsetOfCentralDirectory > 0x7fffffffffffffffL)
						{
							throw new InvalidDataException(Messages.FieldTooBigOffsetToCD);
						}
						if (record.NumberOfEntriesTotal != record.NumberOfEntriesOnThisDisk)
						{
							throw new InvalidDataException(Messages.SplitSpanned);
						}
						_expectedNumberOfEntries = (long) record.NumberOfEntriesTotal;
						_centralDirectoryStart = (long) record.OffsetOfCentralDirectory;
					}
				}
				if (_centralDirectoryStart > ArchiveStream1.Length)
				{
					throw new InvalidDataException(Messages.FieldTooBigOffsetToCD);
				}
			}
			catch (EndOfStreamException exception)
			{
				throw new InvalidDataException(Messages.CDCorrupt, exception);
			}
			catch (IOException exception2)
			{
				throw new InvalidDataException(Messages.CDCorrupt, exception2);
			}
		}

		internal void ReleaseArchiveStream(ZipArchiveEntry entry)
		{
			_archiveStreamOwner = null;
		}

		internal void RemoveEntry(ZipArchiveEntry entry)
		{
			_entries.Remove(entry);
			_entriesDictionary.Remove(entry.FullName);
		}

		internal void ThrowIfDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

	    private void WriteArchiveEpilogue(long startOfCentralDirectory, long sizeOfCentralDirectory)
		{
			bool flag = false;
			if (((startOfCentralDirectory >= 0xffffffffL) || (sizeOfCentralDirectory >= 0xffffffffL)) || (_entries.Count >= 0xffff))
			{
				flag = true;
			}
			if (flag)
			{
				long position = ArchiveStream1.Position;
				Zip64EndOfCentralDirectoryRecord.WriteBlock(ArchiveStream1, (long) _entries.Count, startOfCentralDirectory, sizeOfCentralDirectory);
				Zip64EndOfCentralDirectoryLocator.WriteBlock(ArchiveStream1, position);
			}
			ZipEndOfCentralDirectoryBlock.WriteBlock(ArchiveStream1, (long) _entries.Count, startOfCentralDirectory, sizeOfCentralDirectory, _archiveComment);
		}

	    private void WriteFile()
		{
			List<ZipArchiveEntry>.Enumerator enumerator;
			if (_mode == ZipArchiveMode.Update)
			{
				List<ZipArchiveEntry> list = new List<ZipArchiveEntry>();
				foreach (ZipArchiveEntry entry in _entries)
				{
					if (!entry.LoadLocalHeaderExtraFieldAndCompressedBytesIfNeeded())
					{
						list.Add(entry);
					}
				}
				using (enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						enumerator.Current.Delete();
					}
				}
				ArchiveStream1.Seek(0L, SeekOrigin.Begin);
				ArchiveStream1.SetLength(0L);
			}
			using (enumerator = _entries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.WriteAndFinishLocalEntry();
				}
			}
			long position = ArchiveStream1.Position;
			using (enumerator = _entries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.WriteCentralDirectoryFileHeader();
				}
			}
			long sizeOfCentralDirectory = ArchiveStream1.Position - position;
			WriteArchiveEpilogue(position, sizeOfCentralDirectory);
		}

		internal BinaryReader ArchiveReader => _archiveReader;

		internal Stream ArchiveStream => ArchiveStream1;

		[__DynamicallyInvokable]
		public ReadOnlyCollection<ZipArchiveEntry> Entries
		{
			[__DynamicallyInvokable]
			get
			{
				if (_mode == ZipArchiveMode.Create)
				{
					throw new NotSupportedException(Messages.EntriesInCreateMode);
				}
				ThrowIfDisposed();
				EnsureCentralDirectoryRead();
				return _entriesCollection;
			}
		}

		internal Encoding EntryNameEncoding
		{
			get
			{
				return _entryNameEncoding;
			}
			set
			{
				if ((value != null) && ((value.Equals(Encoding.BigEndianUnicode) || value.Equals(Encoding.Unicode)) || (value.Equals(Encoding.UTF32) || value.Equals(Encoding.UTF7))))
				{
					throw new ArgumentException(Messages.EntryNameEncodingNotSupported, "entryNameEncoding");
				}
				_entryNameEncoding = value;
			}
		}

		[__DynamicallyInvokable]
		public ZipArchiveMode Mode
		{
			[__DynamicallyInvokable]
			get
			{
				return _mode;
			}
		}

		internal uint NumberOfThisDisk => _numberOfThisDisk;

		public Stream ArchiveStream1
		{
			get
			{
				return _archiveStream;
			}

			set
			{
				_archiveStream = value;
			}
		}
	}
}
