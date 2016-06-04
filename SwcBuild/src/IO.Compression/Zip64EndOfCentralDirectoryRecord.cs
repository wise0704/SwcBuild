using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Zip64EndOfCentralDirectoryRecord
	{
	    private const uint SignatureConstant = 0x6064b50;
	    private const ulong NormalSize = 0x2cL;
		public ulong SizeOfThisRecord;
		public ushort VersionMadeBy;
		public ushort VersionNeededToExtract;
		public uint NumberOfThisDisk;
		public uint NumberOfDiskWithStartOfCD;
		public ulong NumberOfEntriesOnThisDisk;
		public ulong NumberOfEntriesTotal;
		public ulong SizeOfCentralDirectory;
		public ulong OffsetOfCentralDirectory;
		public static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
		{
			zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();
			if (reader.ReadUInt32() != 0x6064b50)
			{
				return false;
			}
			zip64EOCDRecord.SizeOfThisRecord = reader.ReadUInt64();
			zip64EOCDRecord.VersionMadeBy = reader.ReadUInt16();
			zip64EOCDRecord.VersionNeededToExtract = reader.ReadUInt16();
			zip64EOCDRecord.NumberOfThisDisk = reader.ReadUInt32();
			zip64EOCDRecord.NumberOfDiskWithStartOfCD = reader.ReadUInt32();
			zip64EOCDRecord.NumberOfEntriesOnThisDisk = reader.ReadUInt64();
			zip64EOCDRecord.NumberOfEntriesTotal = reader.ReadUInt64();
			zip64EOCDRecord.SizeOfCentralDirectory = reader.ReadUInt64();
			zip64EOCDRecord.OffsetOfCentralDirectory = reader.ReadUInt64();
			return true;
		}

		public static void WriteBlock(Stream stream, long numberOfEntries, long startOfCentralDirectory, long sizeOfCentralDirectory)
		{
			BinaryWriter writer1 = new BinaryWriter(stream);
			writer1.Write((uint) 0x6064b50);
			writer1.Write((ulong) 0x2cL);
			writer1.Write((ushort) 0x2d);
			writer1.Write((ushort) 0x2d);
			writer1.Write((uint) 0);
			writer1.Write((uint) 0);
			writer1.Write(numberOfEntries);
			writer1.Write(numberOfEntries);
			writer1.Write(sizeOfCentralDirectory);
			writer1.Write(startOfCentralDirectory);
		}
	}
}
