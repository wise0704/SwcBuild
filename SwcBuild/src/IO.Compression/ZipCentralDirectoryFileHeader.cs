using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipCentralDirectoryFileHeader
	{
		public const uint SignatureConstant = 0x2014b50;
		public ushort VersionMadeBy;
		public ushort VersionNeededToExtract;
		public ushort GeneralPurposeBitFlag;
		public ushort CompressionMethod;
		public uint LastModified;
		public uint Crc32;
		public long CompressedSize;
		public long UncompressedSize;
		public ushort FilenameLength;
		public ushort ExtraFieldLength;
		public ushort FileCommentLength;
		public int DiskNumberStart;
		public ushort InternalFileAttributes;
		public uint ExternalFileAttributes;
		public long RelativeOffsetOfLocalHeader;
		public byte[] Filename;
		public byte[] FileComment;
		public List<ZipGenericExtraField> ExtraFields;
		public static bool TryReadBlock(BinaryReader reader, bool saveExtraFieldsAndComments, out ZipCentralDirectoryFileHeader header)
		{
			Zip64ExtraField field;
			header = new ZipCentralDirectoryFileHeader();
			if (reader.ReadUInt32() != 0x2014b50)
			{
				return false;
			}
			header.VersionMadeBy = reader.ReadUInt16();
			header.VersionNeededToExtract = reader.ReadUInt16();
			header.GeneralPurposeBitFlag = reader.ReadUInt16();
			header.CompressionMethod = reader.ReadUInt16();
			header.LastModified = reader.ReadUInt32();
			header.Crc32 = reader.ReadUInt32();
			uint num = reader.ReadUInt32();
			uint num2 = reader.ReadUInt32();
			header.FilenameLength = reader.ReadUInt16();
			header.ExtraFieldLength = reader.ReadUInt16();
			header.FileCommentLength = reader.ReadUInt16();
			ushort num3 = reader.ReadUInt16();
			header.InternalFileAttributes = reader.ReadUInt16();
			header.ExternalFileAttributes = reader.ReadUInt32();
			uint num4 = reader.ReadUInt32();
			header.Filename = reader.ReadBytes(header.FilenameLength);
			bool readUncompressedSize = num2 == uint.MaxValue;
			bool readCompressedSize = num == uint.MaxValue;
			bool readLocalHeaderOffset = num4 == uint.MaxValue;
			bool readStartDiskNumber = num3 == 0xffff;
			long position = reader.BaseStream.Position + header.ExtraFieldLength;
			using (Stream stream = new SubReadStream(reader.BaseStream, reader.BaseStream.Position, (long) header.ExtraFieldLength))
			{
				if (saveExtraFieldsAndComments)
				{
					header.ExtraFields = ZipGenericExtraField.ParseExtraField(stream);
					field = Zip64ExtraField.GetAndRemoveZip64Block(header.ExtraFields, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber);
				}
				else
				{
					header.ExtraFields = null;
					field = Zip64ExtraField.GetJustZip64Block(stream, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber);
				}
			}
			reader.BaseStream.AdvanceToPosition(position);
			if (saveExtraFieldsAndComments)
			{
				header.FileComment = reader.ReadBytes(header.FileCommentLength);
			}
			else
			{
				Stream baseStream = reader.BaseStream;
				baseStream.Position += header.FileCommentLength;
				header.FileComment = null;
			}
			header.UncompressedSize = !field.UncompressedSize.HasValue ? ((long) num2) : field.UncompressedSize.Value;
			header.CompressedSize = !field.CompressedSize.HasValue ? ((long) num) : field.CompressedSize.Value;
			header.RelativeOffsetOfLocalHeader = !field.LocalHeaderOffset.HasValue ? ((long) num4) : field.LocalHeaderOffset.Value;
			header.DiskNumberStart = !field.StartDiskNumber.HasValue ? num3 : field.StartDiskNumber.Value;
			return true;
		}
	}
}
