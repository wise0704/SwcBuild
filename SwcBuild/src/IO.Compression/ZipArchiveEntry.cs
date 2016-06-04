using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SwcBuild.IO.Compression
{
	[__DynamicallyInvokable]
	public class ZipArchiveEntry
	{
	    private ZipArchive _archive;
	    private List<ZipGenericExtraField> _cdUnknownExtraFields;
	    private byte[] _compressedBytes;
	    private long _compressedSize;
	    private CompressionLevel? _compressionLevel;
	    private uint _crc32;
	    private bool _currentlyOpenForWrite;
	    private readonly int _diskNumberStart;
	    private bool _everOpenedForWrite;
	    private byte[] _fileComment;
	    private BitFlagValues _generalPurposeBitFlag;
	    private DateTimeOffset _lastModified;
	    private List<ZipGenericExtraField> _lhUnknownExtraFields;
	    private long _offsetOfLocalHeader;
	    private readonly bool _originallyInArchive;
	    private Stream _outstandingWriteStream;
	    private CompressionMethodValues _storedCompressionMethod;
	    private string _storedEntryName;
	    private byte[] _storedEntryNameBytes;
	    private long? _storedOffsetOfCompressedData;
	    private MemoryStream _storedUncompressedData;
	    private long _uncompressedSize;
	    private ZipVersionNeededValues _versionToExtract;
	    private const ushort DefaultVersionToExtract = 10;

		internal ZipArchiveEntry(ZipArchive archive, ZipCentralDirectoryFileHeader cd)
		{
			_archive = archive;
			_originallyInArchive = true;
			_diskNumberStart = cd.DiskNumberStart;
			_versionToExtract = (ZipVersionNeededValues) cd.VersionNeededToExtract;
			_generalPurposeBitFlag = (BitFlagValues) cd.GeneralPurposeBitFlag;
			CompressionMethod = (CompressionMethodValues) cd.CompressionMethod;
			_lastModified = new DateTimeOffset(ZipHelper.DosTimeToDateTime(cd.LastModified));
			_compressedSize = cd.CompressedSize;
			_uncompressedSize = cd.UncompressedSize;
			_offsetOfLocalHeader = cd.RelativeOffsetOfLocalHeader;
			_storedOffsetOfCompressedData = null;
			_crc32 = cd.Crc32;
			_compressedBytes = null;
			_storedUncompressedData = null;
			_currentlyOpenForWrite = false;
			_everOpenedForWrite = false;
			_outstandingWriteStream = null;
			FullName = DecodeEntryName(cd.Filename);
			_lhUnknownExtraFields = null;
			_cdUnknownExtraFields = cd.ExtraFields;
			_fileComment = cd.FileComment;
			_compressionLevel = null;
		}

		internal ZipArchiveEntry(ZipArchive archive, string entryName)
		{
			_archive = archive;
			_originallyInArchive = false;
			_diskNumberStart = 0;
			_versionToExtract = ZipVersionNeededValues.Default;
			_generalPurposeBitFlag = 0;
			CompressionMethod = CompressionMethodValues.Deflate;
			_lastModified = DateTimeOffset.Now;
			_compressedSize = 0L;
			_uncompressedSize = 0L;
			_offsetOfLocalHeader = 0L;
			_storedOffsetOfCompressedData = null;
			_crc32 = 0;
			_compressedBytes = null;
			_storedUncompressedData = null;
			_currentlyOpenForWrite = false;
			_everOpenedForWrite = false;
			_outstandingWriteStream = null;
			FullName = entryName;
			_cdUnknownExtraFields = null;
			_lhUnknownExtraFields = null;
			_fileComment = null;
			_compressionLevel = null;
			if (_storedEntryNameBytes.Length > 0xffff)
			{
				throw new ArgumentException(Messages.EntryNamesTooLong);
			}
			if (_archive.Mode == ZipArchiveMode.Create)
			{
				_archive.AcquireArchiveStream(this);
			}
		}

		internal ZipArchiveEntry(ZipArchive archive, ZipCentralDirectoryFileHeader cd, CompressionLevel compressionLevel) : this(archive, cd)
		{
			_compressionLevel = new CompressionLevel?(compressionLevel);
		}

		internal ZipArchiveEntry(ZipArchive archive, string entryName, CompressionLevel compressionLevel) : this(archive, entryName)
		{
			_compressionLevel = new CompressionLevel?(compressionLevel);
		}

	    private void CloseStreams()
		{
			if (_outstandingWriteStream != null)
			{
				_outstandingWriteStream.Close();
			}
		}

	    private string DecodeEntryName(byte[] entryNameBytes)
		{
			// This item is obfuscated and can not be translated.
			Encoding encoding;
			if (((int) (_generalPurposeBitFlag & BitFlagValues.UnicodeFileName)) == 0)
			{
				Encoding expressionStack_33_0;
				if (_archive != null)
				{
					Encoding expressionStack_24_0;
					Encoding entryNameEncoding = _archive.EntryNameEncoding;
					if (entryNameEncoding != null)
					{
						encoding = Encoding.GetEncoding(0);
						goto Label_003C;
					}
					else
					{
						expressionStack_24_0 = entryNameEncoding;
					}
					expressionStack_24_0 = Encoding.GetEncoding(0);
					expressionStack_33_0 = Encoding.GetEncoding(0);
				}
				else
				{
					expressionStack_33_0 = Encoding.GetEncoding(0);
				}
				encoding = expressionStack_33_0;
			}
			else
			{
				encoding = Encoding.UTF8;
			}
		Label_003C:
			return new string(encoding.GetChars(entryNameBytes));
		}

		[__DynamicallyInvokable]
		public void Delete()
		{
			if (_archive != null)
			{
				if (_currentlyOpenForWrite)
				{
					throw new IOException(Messages.DeleteOpenEntry);
				}
				if (_archive.Mode != ZipArchiveMode.Update)
				{
					throw new NotSupportedException(Messages.DeleteOnlyInUpdate);
				}
				_archive.ThrowIfDisposed();
				_archive.RemoveEntry(this);
				_archive = null;
				UnloadStreams();
			}
		}

	    private byte[] EncodeEntryName(string entryName, out bool isUTF8)
		{
			Encoding entryNameEncoding;
			if ((_archive != null) && (_archive.EntryNameEncoding != null))
			{
				entryNameEncoding = _archive.EntryNameEncoding;
			}
			else
			{
				entryNameEncoding = ZipHelper.RequiresUnicode(entryName) ? Encoding.UTF8 : Encoding.GetEncoding(0);
			}
			isUTF8 = (entryNameEncoding is UTF8Encoding) && entryNameEncoding.Equals(Encoding.UTF8);
			return entryNameEncoding.GetBytes(entryName);
		}

	    private CheckSumAndSizeWriteStream GetDataCompressor(Stream backingStream, bool leaveBackingStreamOpen, EventHandler onClose)
		{
			bool flag = true;
			return new CheckSumAndSizeWriteStream(_compressionLevel.HasValue ? new DeflateStream(backingStream, CompressionMode.Compress, leaveBackingStreamOpen) : new DeflateStream(backingStream, CompressionMode.Compress, leaveBackingStreamOpen), backingStream, leaveBackingStreamOpen && !flag, delegate (long initialPosition, long currentPosition, uint checkSum)
			{
				_crc32 = checkSum;
				_uncompressedSize = currentPosition;
				_compressedSize = backingStream.Position - initialPosition;
				if (onClose != null)
				{
					onClose(this, EventArgs.Empty);
				}
			});
		}

	    private Stream GetDataDecompressor(Stream compressedStreamToRead)
		{
			CompressionMethodValues compressionMethod = CompressionMethod;
			if ((compressionMethod != CompressionMethodValues.Stored) && (compressionMethod == CompressionMethodValues.Deflate))
			{
				return new DeflateStream(compressedStreamToRead, CompressionMode.Decompress);
			}
			return compressedStreamToRead;
		}

	    private bool IsOpenable(bool needToUncompress, bool needToLoadIntoMemory, out string message)
		{
			message = null;
			if (_originallyInArchive)
			{
				if ((needToUncompress && (CompressionMethod != CompressionMethodValues.Stored)) && (CompressionMethod != CompressionMethodValues.Deflate))
				{
					message = Messages.UnsupportedCompression;
					return false;
				}
				if (_diskNumberStart != _archive.NumberOfThisDisk)
				{
					message = Messages.SplitSpanned;
					return false;
				}
				if (_offsetOfLocalHeader > _archive.ArchiveStream.Length)
				{
					message = Messages.LocalFileHeaderCorrupt;
					return false;
				}
				_archive.ArchiveStream.Seek(_offsetOfLocalHeader, SeekOrigin.Begin);
				if (!ZipLocalFileHeader.TrySkipBlock(_archive.ArchiveReader))
				{
					message = Messages.LocalFileHeaderCorrupt;
					return false;
				}
				if ((OffsetOfCompressedData + _compressedSize) > _archive.ArchiveStream.Length)
				{
					message = Messages.LocalFileHeaderCorrupt;
					return false;
				}
				if (needToLoadIntoMemory && (_compressedSize > 0x7fffffffL))
				{
					message = Messages.EntryTooLarge;
					return false;
				}
			}
			return true;
		}

		internal bool LoadLocalHeaderExtraFieldAndCompressedBytesIfNeeded()
		{
			if (_originallyInArchive)
			{
				_archive.ArchiveStream.Seek(_offsetOfLocalHeader, SeekOrigin.Begin);
				_lhUnknownExtraFields = ZipLocalFileHeader.GetExtraFields(_archive.ArchiveReader);
			}
			if (!_everOpenedForWrite && _originallyInArchive)
			{
				_compressedBytes = new byte[_compressedSize];
				_archive.ArchiveStream.Seek(OffsetOfCompressedData, SeekOrigin.Begin);
				ZipHelper.ReadBytes(_archive.ArchiveStream, _compressedBytes, (int) _compressedSize);
			}
			return true;
		}

		[__DynamicallyInvokable]
		public Stream Open()
		{
			ThrowIfInvalidArchive();
			switch (_archive.Mode)
			{
				case ZipArchiveMode.Read:
					return OpenInReadMode(true);

				case ZipArchiveMode.Create:
					return OpenInWriteMode();
			}
			return OpenInUpdateMode();
		}

	    private Stream OpenInReadMode(bool checkOpenable)
		{
			if (checkOpenable)
			{
				ThrowIfNotOpenable(true, false);
			}
			Stream compressedStreamToRead = new SubReadStream(_archive.ArchiveStream, OffsetOfCompressedData, _compressedSize);
			return GetDataDecompressor(compressedStreamToRead);
		}

	    private Stream OpenInUpdateMode()
		{
			if (_currentlyOpenForWrite)
			{
				throw new IOException(Messages.UpdateModeOneStream);
			}
			ThrowIfNotOpenable(true, true);
			_everOpenedForWrite = true;
			_currentlyOpenForWrite = true;
			UncompressedData.Seek(0L, SeekOrigin.Begin);
			return new WrappedStream(UncompressedData, delegate (object o, EventArgs e)
			{
				_currentlyOpenForWrite = false;
			});
		}

	    private Stream OpenInWriteMode()
		{
			if (_everOpenedForWrite)
			{
				throw new IOException(Messages.CreateModeWriteOnceAndOneEntryAtATime);
			}
			_everOpenedForWrite = true;
			CheckSumAndSizeWriteStream crcSizeStream = GetDataCompressor(_archive.ArchiveStream, true, delegate (object o, EventArgs e)
			{
				_archive.ReleaseArchiveStream(this);
				_outstandingWriteStream = null;
			});
			_outstandingWriteStream = new DirectToArchiveWriterStream(crcSizeStream, this);
			return new WrappedStream(_outstandingWriteStream, delegate (object o, EventArgs e)
			{
				_outstandingWriteStream.Close();
			});
		}

	    private bool SizesTooLarge()
		{
			if (_compressedSize <= 0xffffffffL)
			{
				return (_uncompressedSize > 0xffffffffL);
			}
			return true;
		}

	    private void ThrowIfInvalidArchive()
		{
			if (_archive == null)
			{
				throw new InvalidOperationException(Messages.DeletedEntry);
			}
			_archive.ThrowIfDisposed();
		}

		internal void ThrowIfNotOpenable(bool needToUncompress, bool needToLoadIntoMemory)
		{
			string str;
			if (!IsOpenable(needToUncompress, needToLoadIntoMemory, out str))
			{
				throw new InvalidDataException(str);
			}
		}

		[__DynamicallyInvokable]
		public override string ToString()
		{
			return FullName;
		}

	    private void UnloadStreams()
		{
			if (_storedUncompressedData != null)
			{
				_storedUncompressedData.Close();
			}
			_compressedBytes = null;
			_outstandingWriteStream = null;
		}

	    private void VersionToExtractAtLeast(ZipVersionNeededValues value)
		{
			if (_versionToExtract < value)
			{
				_versionToExtract = value;
			}
		}

		internal void WriteAndFinishLocalEntry()
		{
			CloseStreams();
			WriteLocalFileHeaderAndDataIfNeeded();
			UnloadStreams();
		}

		internal void WriteCentralDirectoryFileHeader()
		{
			uint maxValue;
			uint num2;
			uint num3;
			ushort num5;
			BinaryWriter writer = new BinaryWriter(_archive.ArchiveStream);
			Zip64ExtraField field = new Zip64ExtraField();
			bool flag = false;
			if (SizesTooLarge())
			{
				flag = true;
				maxValue = uint.MaxValue;
				num2 = uint.MaxValue;
				field.CompressedSize = new long?(_compressedSize);
				field.UncompressedSize = new long?(_uncompressedSize);
			}
			else
			{
				maxValue = (uint) _compressedSize;
				num2 = (uint) _uncompressedSize;
			}
			if (_offsetOfLocalHeader > 0xffffffffL)
			{
				flag = true;
				num3 = uint.MaxValue;
				field.LocalHeaderOffset = new long?(_offsetOfLocalHeader);
			}
			else
			{
				num3 = (uint) _offsetOfLocalHeader;
			}
			if (flag)
			{
				VersionToExtractAtLeast(ZipVersionNeededValues.Zip64);
			}
			int num4 = (flag ? field.TotalSize : 0) + ((_cdUnknownExtraFields != null) ? ZipGenericExtraField.TotalSize(_cdUnknownExtraFields) : 0);
			if (num4 > 0xffff)
			{
				num5 = flag ? field.TotalSize : ((ushort) 0);
				_cdUnknownExtraFields = null;
			}
			else
			{
				num5 = (ushort) num4;
			}
			writer.Write((uint) 0x2014b50);
			writer.Write((ushort) _versionToExtract);
			writer.Write((ushort) _versionToExtract);
			writer.Write((ushort) _generalPurposeBitFlag);
			writer.Write((ushort) CompressionMethod);
			writer.Write(ZipHelper.DateTimeToDosTime(_lastModified.DateTime));
			writer.Write(_crc32);
			writer.Write(maxValue);
			writer.Write(num2);
			writer.Write((ushort) _storedEntryNameBytes.Length);
			writer.Write(num5);
			writer.Write((_fileComment != null) ? ((ushort) _fileComment.Length) : ((ushort) 0));
			writer.Write((ushort) 0);
			writer.Write((ushort) 0);
			writer.Write((uint) 0);
			writer.Write(num3);
			writer.Write(_storedEntryNameBytes);
			if (flag)
			{
				field.WriteBlock(_archive.ArchiveStream);
			}
			if (_cdUnknownExtraFields != null)
			{
				ZipGenericExtraField.WriteAllBlocks(_cdUnknownExtraFields, _archive.ArchiveStream);
			}
			if (_fileComment != null)
			{
				writer.Write(_fileComment);
			}
		}

	    private void WriteCrcAndSizesInLocalHeader(bool zip64HeaderUsed)
		{
			long position = _archive.ArchiveStream.Position;
			BinaryWriter writer = new BinaryWriter(_archive.ArchiveStream);
			bool flag1 = SizesTooLarge();
			bool flag = flag1 && !zip64HeaderUsed;
			uint num2 = flag1 ? uint.MaxValue : ((uint) _compressedSize);
			uint num3 = flag1 ? uint.MaxValue : ((uint) _uncompressedSize);
			if (flag)
			{
				_generalPurposeBitFlag |= BitFlagValues.DataDescriptor;
				_archive.ArchiveStream.Seek(_offsetOfLocalHeader + 6L, SeekOrigin.Begin);
				writer.Write((ushort) _generalPurposeBitFlag);
			}
			_archive.ArchiveStream.Seek(_offsetOfLocalHeader + 14L, SeekOrigin.Begin);
			if (!flag)
			{
				writer.Write(_crc32);
				writer.Write(num2);
				writer.Write(num3);
			}
			else
			{
				writer.Write((uint) 0);
				writer.Write((uint) 0);
				writer.Write((uint) 0);
			}
			if (zip64HeaderUsed)
			{
				_archive.ArchiveStream.Seek(((_offsetOfLocalHeader + 30L) + _storedEntryNameBytes.Length) + 4L, SeekOrigin.Begin);
				writer.Write(_uncompressedSize);
				writer.Write(_compressedSize);
				_archive.ArchiveStream.Seek(position, SeekOrigin.Begin);
			}
			_archive.ArchiveStream.Seek(position, SeekOrigin.Begin);
			if (flag)
			{
				writer.Write(_crc32);
				writer.Write(_compressedSize);
				writer.Write(_uncompressedSize);
			}
		}

	    private void WriteDataDescriptor()
		{
			BinaryWriter writer = new BinaryWriter(_archive.ArchiveStream);
			writer.Write((uint) 0x8074b50);
			writer.Write(_crc32);
			if (SizesTooLarge())
			{
				writer.Write(_compressedSize);
				writer.Write(_uncompressedSize);
			}
			else
			{
				writer.Write((uint) _compressedSize);
				writer.Write((uint) _uncompressedSize);
			}
		}

	    private bool WriteLocalFileHeader(bool isEmptyFile)
		{
			uint maxValue;
			uint num2;
			ushort num4;
			BinaryWriter writer = new BinaryWriter(_archive.ArchiveStream);
			Zip64ExtraField field = new Zip64ExtraField();
			bool flag = false;
			if (isEmptyFile)
			{
				CompressionMethod = CompressionMethodValues.Stored;
				maxValue = 0;
				num2 = 0;
			}
			else if (((_archive.Mode == ZipArchiveMode.Create) && !_archive.ArchiveStream.CanSeek) && !isEmptyFile)
			{
				_generalPurposeBitFlag |= BitFlagValues.DataDescriptor;
				flag = false;
				maxValue = 0;
				num2 = 0;
			}
			else if (SizesTooLarge())
			{
				flag = true;
				maxValue = uint.MaxValue;
				num2 = uint.MaxValue;
				field.CompressedSize = new long?(_compressedSize);
				field.UncompressedSize = new long?(_uncompressedSize);
				VersionToExtractAtLeast(ZipVersionNeededValues.Zip64);
			}
			else
			{
				flag = false;
				maxValue = (uint) _compressedSize;
				num2 = (uint) _uncompressedSize;
			}
			_offsetOfLocalHeader = writer.BaseStream.Position;
			int num3 = (flag ? field.TotalSize : 0) + ((_lhUnknownExtraFields != null) ? ZipGenericExtraField.TotalSize(_lhUnknownExtraFields) : 0);
			if (num3 > 0xffff)
			{
				num4 = flag ? field.TotalSize : ((ushort) 0);
				_lhUnknownExtraFields = null;
			}
			else
			{
				num4 = (ushort) num3;
			}
			writer.Write((uint) 0x4034b50);
			writer.Write((ushort) _versionToExtract);
			writer.Write((ushort) _generalPurposeBitFlag);
			writer.Write((ushort) CompressionMethod);
			writer.Write(ZipHelper.DateTimeToDosTime(_lastModified.DateTime));
			writer.Write(_crc32);
			writer.Write(maxValue);
			writer.Write(num2);
			writer.Write((ushort) _storedEntryNameBytes.Length);
			writer.Write(num4);
			writer.Write(_storedEntryNameBytes);
			if (flag)
			{
				field.WriteBlock(_archive.ArchiveStream);
			}
			if (_lhUnknownExtraFields != null)
			{
				ZipGenericExtraField.WriteAllBlocks(_lhUnknownExtraFields, _archive.ArchiveStream);
			}
			return flag;
		}

	    private void WriteLocalFileHeaderAndDataIfNeeded()
		{
			if ((_storedUncompressedData != null) || (_compressedBytes != null))
			{
				if (_storedUncompressedData != null)
				{
					_uncompressedSize = _storedUncompressedData.Length;
					using (Stream stream = new DirectToArchiveWriterStream(GetDataCompressor(_archive.ArchiveStream, true, null), this))
					{
						_storedUncompressedData.Seek(0L, SeekOrigin.Begin);
						_storedUncompressedData.CopyTo(stream);
						_storedUncompressedData.Close();
						_storedUncompressedData = null;
						return;
					}
				}
				if (_uncompressedSize == 0)
				{
					CompressionMethod = CompressionMethodValues.Stored;
				}
				WriteLocalFileHeader(false);
				using (MemoryStream stream2 = new MemoryStream(_compressedBytes))
				{
					stream2.CopyTo(_archive.ArchiveStream);
					return;
				}
			}
			if ((_archive.Mode == ZipArchiveMode.Update) || !_everOpenedForWrite)
			{
				_everOpenedForWrite = true;
				WriteLocalFileHeader(true);
			}
		}

		[__DynamicallyInvokable]
		public ZipArchive Archive
		{
			[__DynamicallyInvokable]
			get
			{
				return _archive;
			}
		}

		[__DynamicallyInvokable]
		public long CompressedLength
		{
			[__DynamicallyInvokable]
			get
			{
				if (_everOpenedForWrite)
				{
					throw new InvalidOperationException(Messages.LengthAfterWrite);
				}
				return _compressedSize;
			}
		}

	    private CompressionMethodValues CompressionMethod
		{
			get
			{
				return _storedCompressionMethod;
			}
			set
			{
				if (value == CompressionMethodValues.Deflate)
				{
					VersionToExtractAtLeast(ZipVersionNeededValues.ExplicitDirectory);
				}
				_storedCompressionMethod = value;
			}
		}

		internal bool EverOpenedForWrite => _everOpenedForWrite;

		[__DynamicallyInvokable]
		public string FullName
		{
			[__DynamicallyInvokable]
			get
			{
				return _storedEntryName;
			}
			set
			{
				bool flag;
				if (value == null)
				{
					throw new ArgumentNullException("FullName");
				}
				_storedEntryNameBytes = EncodeEntryName(value, out flag);
				_storedEntryName = value;
				if (flag)
				{
					_generalPurposeBitFlag |= BitFlagValues.UnicodeFileName;
				}
				else
				{
					_generalPurposeBitFlag = ((BitFlagValues) ((int) _generalPurposeBitFlag)) & ((BitFlagValues) 0xf7ff);
				}
				if (ZipHelper.EndsWithDirChar(value))
				{
					VersionToExtractAtLeast(ZipVersionNeededValues.ExplicitDirectory);
				}
			}
		}

		[__DynamicallyInvokable]
		public DateTimeOffset LastWriteTime
		{
			[__DynamicallyInvokable]
			get
			{
				return _lastModified;
			}
			[__DynamicallyInvokable]
			set
			{
				ThrowIfInvalidArchive();
				if (_archive.Mode == ZipArchiveMode.Read)
				{
					throw new NotSupportedException(Messages.ReadOnlyArchive);
				}
				if ((_archive.Mode == ZipArchiveMode.Create) && _everOpenedForWrite)
				{
					throw new IOException(Messages.FrozenAfterWrite);
				}
				if ((value.DateTime.Year < 0x7bc) || (value.DateTime.Year > 0x83b))
				{
					throw new ArgumentOutOfRangeException("value", Messages.DateTimeOutOfRange);
				}
				_lastModified = value;
			}
		}

		[__DynamicallyInvokable]
		public long Length
		{
			[__DynamicallyInvokable]
			get
			{
				if (_everOpenedForWrite)
				{
					throw new InvalidOperationException(Messages.LengthAfterWrite);
				}
				return _uncompressedSize;
			}
		}

		[__DynamicallyInvokable]
		public string Name
		{
			[__DynamicallyInvokable]
			get
			{
				return Path.GetFileName(FullName);
			}
		}

	    private long OffsetOfCompressedData
		{
			get
			{
				if (!_storedOffsetOfCompressedData.HasValue)
				{
					_archive.ArchiveStream.Seek(_offsetOfLocalHeader, SeekOrigin.Begin);
					if (!ZipLocalFileHeader.TrySkipBlock(_archive.ArchiveReader))
					{
						throw new InvalidDataException(Messages.LocalFileHeaderCorrupt);
					}
					_storedOffsetOfCompressedData = new long?(_archive.ArchiveStream.Position);
				}
				return _storedOffsetOfCompressedData.Value;
			}
		}

	    private MemoryStream UncompressedData
		{
			get
			{
				if (_storedUncompressedData == null)
				{
					_storedUncompressedData = new MemoryStream((int) _uncompressedSize);
					if (_originallyInArchive)
					{
						Stream stream = OpenInReadMode(false);
						try
						{
							stream.CopyTo(_storedUncompressedData);
						}
						catch (InvalidDataException)
						{
							_storedUncompressedData.Dispose();
							_storedUncompressedData = null;
							_currentlyOpenForWrite = false;
							_everOpenedForWrite = false;
							throw;
						}
						finally
						{
							if (stream != null)
							{
								stream.Dispose();
							}
						}
					}
					CompressionMethod = CompressionMethodValues.Deflate;
				}
				return _storedUncompressedData;
			}
		}

		[Flags]
		private enum BitFlagValues : ushort
		{
			DataDescriptor = 8,
			UnicodeFileName = 0x800
		}

	    private enum CompressionMethodValues : ushort
		{
			Deflate = 8,
			Stored = 0
		}

	    private class DirectToArchiveWriterStream : Stream
		{
		    private bool _canWrite;
		    private CheckSumAndSizeWriteStream _crcSizeStream;
		    private ZipArchiveEntry _entry;
		    private bool _everWritten;
		    private bool _isDisposed;
		    private long _position = 0L;
		    private bool _usedZip64inLH;

			public DirectToArchiveWriterStream(CheckSumAndSizeWriteStream crcSizeStream, ZipArchiveEntry entry)
			{
				_crcSizeStream = crcSizeStream;
				_everWritten = false;
				_isDisposed = false;
				_entry = entry;
				_usedZip64inLH = false;
				_canWrite = true;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !_isDisposed)
				{
					_crcSizeStream.Close();
					if (!_everWritten)
					{
						_entry.WriteLocalFileHeader(true);
					}
					else if (_entry._archive.ArchiveStream.CanSeek)
					{
						_entry.WriteCrcAndSizesInLocalHeader(_usedZip64inLH);
					}
					else
					{
						_entry.WriteDataDescriptor();
					}
					_canWrite = false;
					_isDisposed = true;
				}
				base.Dispose(disposing);
			}

			public override void Flush()
			{
				ThrowIfDisposed();
				_crcSizeStream.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				ThrowIfDisposed();
				throw new NotSupportedException(Messages.ReadingNotSupported);
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				ThrowIfDisposed();
				throw new NotSupportedException(Messages.SeekingNotSupported);
			}

			public override void SetLength(long value)
			{
				ThrowIfDisposed();
				throw new NotSupportedException(Messages.SetLengthRequiresSeekingAndWriting);
			}

	        private void ThrowIfDisposed()
			{
				if (_isDisposed)
				{
					throw new ObjectDisposedException(base.GetType().Name, Messages.HiddenStreamName);
				}
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (buffer == null)
				{
					throw new ArgumentNullException("buffer");
				}
				if (offset < 0)
				{
					throw new ArgumentOutOfRangeException("offset", Messages.ArgumentNeedNonNegative);
				}
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException("count", Messages.ArgumentNeedNonNegative);
				}
				if ((buffer.Length - offset) < count)
				{
					throw new ArgumentException(Messages.OffsetLengthInvalid);
				}
				ThrowIfDisposed();
				if (count != 0)
				{
					if (!_everWritten)
					{
						_everWritten = true;
						_usedZip64inLH = _entry.WriteLocalFileHeader(false);
					}
					_crcSizeStream.Write(buffer, offset, count);
					_position += count;
				}
			}

			public override bool CanRead => false;

			public override bool CanSeek => false;

			public override bool CanWrite => _canWrite;

			public override long Length
			{
				get
				{
					ThrowIfDisposed();
					throw new NotSupportedException(Messages.SeekingNotSupported);
				}
			}

			public override long Position
			{
				get
				{
					ThrowIfDisposed();
					return _position;
				}
				set
				{
					ThrowIfDisposed();
					throw new NotSupportedException(Messages.SeekingNotSupported);
				}
			}
		}

	    private enum OpenableValues
		{
			Openable,
			FileNonExistent,
			FileTooLarge
		}
	}
}
