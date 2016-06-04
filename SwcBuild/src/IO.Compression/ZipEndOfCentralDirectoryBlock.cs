using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ZipEndOfCentralDirectoryBlock
	{
		public const uint SignatureConstant = 0x6054b50;
		public const int SizeOfBlockWithoutSignature = 0x12;
		public uint Signature;
		public ushort NumberOfThisDisk;
		public ushort NumberOfTheDiskWithTheStartOfTheCentralDirectory;
		public ushort NumberOfEntriesInTheCentralDirectoryOnThisDisk;
		public ushort NumberOfEntriesInTheCentralDirectory;
		public uint SizeOfCentralDirectory;
		public uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
		public byte[] ArchiveComment;
		public static void WriteBlock(Stream stream, long numberOfEntries, long startOfCentralDirectory, long sizeOfCentralDirectory, byte[] archiveComment)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			ushort num = (numberOfEntries > 0xffffL) ? ((ushort) 0xffff) : ((ushort) numberOfEntries);
			uint num2 = (startOfCentralDirectory > 0xffffffffL) ? uint.MaxValue : ((uint) startOfCentralDirectory);
			uint num3 = (sizeOfCentralDirectory > 0xffffffffL) ? uint.MaxValue : ((uint) sizeOfCentralDirectory);
			writer.Write((uint) 0x6054b50);
			writer.Write((ushort) 0);
			writer.Write((ushort) 0);
			writer.Write(num);
			writer.Write(num);
			writer.Write(num3);
			writer.Write(num2);
			writer.Write((archiveComment != null) ? ((ushort) archiveComment.Length) : ((ushort) 0));
			if (archiveComment != null)
			{
				writer.Write(archiveComment);
			}
		}

		public static bool TryReadBlock(BinaryReader reader, out ZipEndOfCentralDirectoryBlock eocdBlock)
		{
			eocdBlock = new ZipEndOfCentralDirectoryBlock();
			if (reader.ReadUInt32() != 0x6054b50)
			{
				return false;
			}
			eocdBlock.Signature = 0x6054b50;
			eocdBlock.NumberOfThisDisk = reader.ReadUInt16();
			eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = reader.ReadUInt16();
			eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = reader.ReadUInt16();
			eocdBlock.NumberOfEntriesInTheCentralDirectory = reader.ReadUInt16();
			eocdBlock.SizeOfCentralDirectory = reader.ReadUInt32();
			eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = reader.ReadUInt32();
			ushort count = reader.ReadUInt16();
			eocdBlock.ArchiveComment = reader.ReadBytes(count);
			return true;
		}
	}
}
