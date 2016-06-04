using System;
using System.IO;

namespace SwcBuild.IO.Compression
{
    internal static class ZipHelper
	{
	    private const int BackwardsSeekingBufferSize = 0x20;
	    private static readonly DateTime InvalidDateIndicator = new DateTime(0x7bc, 1, 1, 0, 0, 0);
		internal const ushort Mask16Bit = 0xffff;
		internal const uint Mask32Bit = uint.MaxValue;
		internal const int ValidZipDate_YearMax = 0x83b;
		internal const int ValidZipDate_YearMin = 0x7bc;

		internal static void AdvanceToPosition(this Stream stream, long position)
		{
			int num3;
			for (long i = position - stream.Position; i != 0; i -= num3)
			{
				int count = (i > 0x40L) ? 0x40 : ((int) i);
				num3 = stream.Read(new byte[0x40], 0, count);
				if (num3 == 0)
				{
					throw new IOException(Messages.UnexpectedEndOfStream);
				}
			}
		}

		internal static uint DateTimeToDosTime(DateTime dateTime)
		{
			return (uint) ((((((((((((dateTime.Year - 0x7bc) & 0x7f) << 4) + dateTime.Month) << 5) + dateTime.Day) << 5) + dateTime.Hour) << 6) + dateTime.Minute) << 5) + (dateTime.Second / 2));
		}

		internal static DateTime DosTimeToDateTime(uint dateTime)
		{
			int year = 0x7bc + ((int) (dateTime >> 0x19));
			int month = ((int) (dateTime >> 0x15)) & 15;
			int day = ((int) (dateTime >> 0x10)) & 0x1f;
			int hour = ((int) (dateTime >> 11)) & 0x1f;
			int minute = ((int) (dateTime >> 5)) & 0x3f;
			int second = (int) ((dateTime & 0x1f) * 2);
			try
			{
				return new DateTime(year, month, day, hour, minute, second, 0);
			}
			catch (ArgumentOutOfRangeException)
			{
				return InvalidDateIndicator;
			}
			catch (ArgumentException)
			{
				return InvalidDateIndicator;
			}
		}

		internal static bool EndsWithDirChar(string test)
		{
			return (Path.GetFileName(test) == "");
		}

		internal static void ReadBytes(Stream stream, byte[] buffer, int bytesToRead)
		{
			int count = bytesToRead;
			int offset = 0;
			while (count > 0)
			{
				int num3 = stream.Read(buffer, offset, count);
				if (num3 == 0)
				{
					throw new IOException(Messages.UnexpectedEndOfStream);
				}
				offset += num3;
				count -= num3;
			}
		}

		internal static bool RequiresUnicode(string test)
		{
			string str = test;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] > '\x007f')
				{
					return true;
				}
			}
			return false;
		}

        private static bool SeekBackwardsAndRead(Stream stream, byte[] buffer, out int bufferPointer)
		{
			if (stream.Position >= buffer.Length)
			{
				stream.Seek((long) -buffer.Length, SeekOrigin.Current);
				ReadBytes(stream, buffer, buffer.Length);
				stream.Seek((long) -buffer.Length, SeekOrigin.Current);
				bufferPointer = buffer.Length - 1;
				return false;
			}
			int position = (int) stream.Position;
			stream.Seek(0L, SeekOrigin.Begin);
			ReadBytes(stream, buffer, position);
			stream.Seek(0L, SeekOrigin.Begin);
			bufferPointer = position - 1;
			return true;
		}

		internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind)
		{
			int bufferPointer = 0;
			uint num2 = 0;
			byte[] buffer = new byte[0x20];
			bool flag = false;
			bool flag2 = false;
			while (!flag2 && !flag)
			{
				flag = SeekBackwardsAndRead(stream, buffer, out bufferPointer);
				while ((bufferPointer >= 0) && !flag2)
				{
					num2 = (num2 << 8) | buffer[bufferPointer];
					if (num2 == signatureToFind)
					{
						flag2 = true;
					}
					else
					{
						bufferPointer--;
					}
				}
			}
			if (!flag2)
			{
				return false;
			}
			stream.Seek((long) bufferPointer, SeekOrigin.Current);
			return true;
		}
	}
}
