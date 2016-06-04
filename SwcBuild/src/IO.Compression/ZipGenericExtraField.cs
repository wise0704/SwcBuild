using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipGenericExtraField
	{
	    private const int SizeOfHeader = 4;
	    private ushort _tag;
	    private ushort _size;
	    private byte[] _data;
		public ushort Tag => _tag;
		public ushort Size => _size;
		public byte[] Data => _data;
		public void WriteBlock(Stream stream)
		{
			BinaryWriter writer1 = new BinaryWriter(stream);
			writer1.Write(Tag);
			writer1.Write(Size);
			writer1.Write(Data);
		}

		public static bool TryReadBlock(BinaryReader reader, long endExtraField, out ZipGenericExtraField field)
		{
			field = new ZipGenericExtraField();
			if ((endExtraField - reader.BaseStream.Position) < 4L)
			{
				return false;
			}
			field._tag = reader.ReadUInt16();
			field._size = reader.ReadUInt16();
			if ((endExtraField - reader.BaseStream.Position) < field._size)
			{
				return false;
			}
			field._data = reader.ReadBytes(field._size);
			return true;
		}

		public static List<ZipGenericExtraField> ParseExtraField(Stream extraFieldData)
		{
			List<ZipGenericExtraField> list = new List<ZipGenericExtraField>();
			using (BinaryReader reader = new BinaryReader(extraFieldData))
			{
				ZipGenericExtraField field;
				while (TryReadBlock(reader, extraFieldData.Length, out field))
				{
					list.Add(field);
				}
			}
			return list;
		}

		public static int TotalSize(List<ZipGenericExtraField> fields)
		{
			int num = 0;
			foreach (ZipGenericExtraField field in fields)
			{
				num += field.Size + 4;
			}
			return num;
		}

		public static void WriteAllBlocks(List<ZipGenericExtraField> fields, Stream stream)
		{
			foreach (ZipGenericExtraField field in fields)
			{
				field.WriteBlock(stream);
			}
		}
	}
}
