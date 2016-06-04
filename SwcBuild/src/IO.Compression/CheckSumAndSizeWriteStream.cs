using System;
using System.IO;

namespace SwcBuild.IO.Compression
{
    internal class CheckSumAndSizeWriteStream : Stream
	{
	    private readonly Stream _baseBaseStream;
	    private readonly Stream _baseStream;
	    private bool _canWrite;
	    private uint _checksum;
	    private bool _everWritten;
	    private long _initialPosition;
	    private bool _isDisposed;
	    private readonly bool _leaveOpenOnClose;
	    private long _position;
	    private readonly Action<long, long, uint> _saveCrcAndSizes;

		public CheckSumAndSizeWriteStream(Stream baseStream, Stream baseBaseStream, bool leaveOpenOnClose, Action<long, long, uint> saveCrcAndSizes)
		{
			_baseStream = baseStream;
			_baseBaseStream = baseBaseStream;
			_position = 0L;
			_checksum = 0;
			_leaveOpenOnClose = leaveOpenOnClose;
			_canWrite = true;
			_isDisposed = false;
			_initialPosition = 0L;
			_saveCrcAndSizes = saveCrcAndSizes;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				if (!_everWritten)
				{
					_initialPosition = _baseBaseStream.Position;
				}
				if (!_leaveOpenOnClose)
				{
					_baseStream.Close();
				}
				if (_saveCrcAndSizes != null)
				{
					_saveCrcAndSizes(_initialPosition, Position, _checksum);
				}
				_isDisposed = true;
			}
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			ThrowIfDisposed();
			_baseStream.Flush();
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
					_initialPosition = _baseBaseStream.Position;
					_everWritten = true;
				}
				_checksum = Crc32Helper.UpdateCrc32(_checksum, buffer, offset, count);
				_baseStream.Write(buffer, offset, count);
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
}
