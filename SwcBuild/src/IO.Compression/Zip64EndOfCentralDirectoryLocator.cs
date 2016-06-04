using System.IO;
using System.Runtime.InteropServices;

namespace SwcBuild.IO.Compression
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Zip64EndOfCentralDirectoryLocator
	{
		public const uint SignatureConstant = 0x7064b50;
		public const int SizeOfBlockWithoutSignature = 0x10;
		public uint NumberOfDiskWithZip64EOCD;
		public ulong OffsetOfZip64EOCD;
		public uint TotalNumberOfDisks;
		public static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
		{
			zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();
			if (reader.ReadUInt32() != 0x7064b50)
			{
				return false;
			}
			zip64EOCDLocator.NumberOfDiskWithZip64EOCD = reader.ReadUInt32();
			zip64EOCDLocator.OffsetOfZip64EOCD = reader.ReadUInt64();
			zip64EOCDLocator.TotalNumberOfDisks = reader.ReadUInt32();
			return true;
		}

		public static void WriteBlock(Stream stream, long zip64EOCDRecordStart)
		{
			BinaryWriter writer1 = new BinaryWriter(stream);
			writer1.Write((uint) 0x7064b50);
			writer1.Write((uint) 0);
			writer1.Write(zip64EOCDRecordStart);
			writer1.Write((uint) 1);
		}
	}
}
