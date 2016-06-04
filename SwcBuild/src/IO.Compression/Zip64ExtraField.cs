using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Zip64ExtraField
	{
		public const int OffsetToFirstField = 4;
	    private const ushort TagConstant = 1;
	    private ushort _size;
	    private long? _uncompressedSize;
	    private long? _compressedSize;
	    private long? _localHeaderOffset;
	    private int? _startDiskNumber;
		public ushort TotalSize => (ushort) (_size + 4);
		public long? UncompressedSize
		{
			get
			{
				return _uncompressedSize;
			}
			set
			{
				_uncompressedSize = value;
				UpdateSize();
			}
		}
		public long? CompressedSize
		{
			get
			{
				return _compressedSize;
			}
			set
			{
				_compressedSize = value;
				UpdateSize();
			}
		}
		public long? LocalHeaderOffset
		{
			get
			{
				return _localHeaderOffset;
			}
			set
			{
				_localHeaderOffset = value;
				UpdateSize();
			}
		}
		public int? StartDiskNumber => _startDiskNumber;

	    private void UpdateSize()
		{
			_size = 0;
			if (_uncompressedSize.HasValue)
			{
				_size = (ushort) (_size + 8);
			}
			if (_compressedSize.HasValue)
			{
				_size = (ushort) (_size + 8);
			}
			if (_localHeaderOffset.HasValue)
			{
				_size = (ushort) (_size + 8);
			}
			if (_startDiskNumber.HasValue)
			{
				_size = (ushort) (_size + 4);
			}
		}

		public static Zip64ExtraField GetJustZip64Block(Stream extraFieldStream, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber)
		{
			using (BinaryReader reader = new BinaryReader(extraFieldStream))
			{
				ZipGenericExtraField field2;
				while (ZipGenericExtraField.TryReadBlock(reader, extraFieldStream.Length, out field2))
				{
					Zip64ExtraField field;
					if (TryGetZip64BlockFromGenericExtraField(field2, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out field))
					{
						return field;
					}
				}
			}
			return new Zip64ExtraField { _compressedSize = null, _uncompressedSize = null, _localHeaderOffset = null, _startDiskNumber = null };
		}

	    private static bool TryGetZip64BlockFromGenericExtraField(ZipGenericExtraField extraField, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber, out Zip64ExtraField zip64Block)
		{
			bool flag;
			zip64Block = new Zip64ExtraField();
			zip64Block._compressedSize = null;
			zip64Block._uncompressedSize = null;
			zip64Block._localHeaderOffset = null;
			zip64Block._startDiskNumber = null;
			if (extraField.Tag != 1)
			{
				return false;
			}
			MemoryStream input = null;
			try
			{
				input = new MemoryStream(extraField.Data);
				using (BinaryReader reader = new BinaryReader(input))
				{
					input = null;
					zip64Block._size = extraField.Size;
					ushort num = 0;
					if (readUncompressedSize)
					{
						num = (ushort) (num + 8);
					}
					if (readCompressedSize)
					{
						num = (ushort) (num + 8);
					}
					if (readLocalHeaderOffset)
					{
						num = (ushort) (num + 8);
					}
					if (readStartDiskNumber)
					{
						num = (ushort) (num + 4);
					}
					if (num != zip64Block._size)
					{
						return false;
					}
					if (readUncompressedSize)
					{
						zip64Block._uncompressedSize = new long?(reader.ReadInt64());
					}
					if (readCompressedSize)
					{
						zip64Block._compressedSize = new long?(reader.ReadInt64());
					}
					if (readLocalHeaderOffset)
					{
						zip64Block._localHeaderOffset = new long?(reader.ReadInt64());
					}
					if (readStartDiskNumber)
					{
						zip64Block._startDiskNumber = new int?(reader.ReadInt32());
					}
					long? nullable = zip64Block._uncompressedSize;
					long num2 = 0L;
					if ((nullable.GetValueOrDefault() < num2) ? nullable.HasValue : false)
					{
						throw new InvalidDataException(Messages.FieldTooBigUncompressedSize);
					}
					nullable = zip64Block._compressedSize;
					num2 = 0L;
					if ((nullable.GetValueOrDefault() < num2) ? nullable.HasValue : false)
					{
						throw new InvalidDataException(Messages.FieldTooBigCompressedSize);
					}
					nullable = zip64Block._localHeaderOffset;
					num2 = 0L;
					if ((nullable.GetValueOrDefault() < num2) ? nullable.HasValue : false)
					{
						throw new InvalidDataException(Messages.FieldTooBigLocalHeaderOffset);
					}
					int? nullable2 = zip64Block._startDiskNumber;
					int num3 = 0;
					if ((nullable2.GetValueOrDefault() < num3) ? nullable2.HasValue : false)
					{
						throw new InvalidDataException(Messages.FieldTooBigStartDiskNumber);
					}
					flag = true;
				}
			}
			finally
			{
				if (input != null)
				{
					input.Close();
				}
			}
			return flag;
		}

		public static Zip64ExtraField GetAndRemoveZip64Block(List<ZipGenericExtraField> extraFields, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber)
		{
			Zip64ExtraField field = new Zip64ExtraField
			{
				_compressedSize = null,
				_uncompressedSize = null,
				_localHeaderOffset = null,
				_startDiskNumber = null
			};
			List<ZipGenericExtraField> list = new List<ZipGenericExtraField>();
			bool flag = false;
			foreach (ZipGenericExtraField field2 in extraFields)
			{
				if (field2.Tag == 1)
				{
					list.Add(field2);
					if (!flag && TryGetZip64BlockFromGenericExtraField(field2, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out field))
					{
						flag = true;
					}
				}
			}
			foreach (ZipGenericExtraField field3 in list)
			{
				extraFields.Remove(field3);
			}
			return field;
		}

		public static void RemoveZip64Blocks(List<ZipGenericExtraField> extraFields)
		{
			List<ZipGenericExtraField> list = new List<ZipGenericExtraField>();
			foreach (ZipGenericExtraField field in extraFields)
			{
				if (field.Tag == 1)
				{
					list.Add(field);
				}
			}
			foreach (ZipGenericExtraField field2 in list)
			{
				extraFields.Remove(field2);
			}
		}

		public void WriteBlock(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write((ushort) 1);
			writer.Write(_size);
			if (_uncompressedSize.HasValue)
			{
				writer.Write(_uncompressedSize.Value);
			}
			if (_compressedSize.HasValue)
			{
				writer.Write(_compressedSize.Value);
			}
			if (_localHeaderOffset.HasValue)
			{
				writer.Write(_localHeaderOffset.Value);
			}
			if (_startDiskNumber.HasValue)
			{
				writer.Write(_startDiskNumber.Value);
			}
		}
	}
}
