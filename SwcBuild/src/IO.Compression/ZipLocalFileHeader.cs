using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct ZipLocalFileHeader
	{
		public const uint DataDescriptorSignature = 0x8074b50;
		public const uint SignatureConstant = 0x4034b50;
		public const int OffsetToCrcFromHeaderStart = 14;
		public const int OffsetToBitFlagFromHeaderStart = 6;
		public const int SizeOfLocalHeader = 30;
		public static List<ZipGenericExtraField> GetExtraFields(BinaryReader reader)
		{
			List<ZipGenericExtraField> list;
			reader.BaseStream.Seek(0x1aL, SeekOrigin.Current);
			ushort num = reader.ReadUInt16();
			ushort num2 = reader.ReadUInt16();
			reader.BaseStream.Seek((long) num, SeekOrigin.Current);
			using (Stream stream = new SubReadStream(reader.BaseStream, reader.BaseStream.Position, (long) num2))
			{
				list = ZipGenericExtraField.ParseExtraField(stream);
			}
			Zip64ExtraField.RemoveZip64Blocks(list);
			return list;
		}

		public static bool TrySkipBlock(BinaryReader reader)
		{
			if (reader.ReadUInt32() != 0x4034b50)
			{
				return false;
			}
			if (reader.BaseStream.Length < (reader.BaseStream.Position + 0x16L))
			{
				return false;
			}
			reader.BaseStream.Seek(0x16L, SeekOrigin.Current);
			ushort num = reader.ReadUInt16();
			ushort num2 = reader.ReadUInt16();
			if (reader.BaseStream.Length < ((reader.BaseStream.Position + num) + num2))
			{
				return false;
			}
			reader.BaseStream.Seek((long) (num + num2), SeekOrigin.Current);
			return true;
		}
	}
}
