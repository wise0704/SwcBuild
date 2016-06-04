using System;
using System.IO;

namespace SwcBuild.IO.Compression
{
    internal class SubReadStream : Stream
	{
	    private bool _canRead;
	    private readonly long _endInSuperStream;
	    private bool _isDisposed;
	    private long _positionInSuperStream;
	    private readonly long _startInSuperStream;
	    private readonly Stream _superStream;

		public SubReadStream(Stream superStream, long startPosition, long maxLength)
		{
			_startInSuperStream = startPosition;
			_positionInSuperStream = startPosition;
			_endInSuperStream = startPosition + maxLength;
			_superStream = superStream;
			_canRead = true;
			_isDisposed = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				_canRead = false;
				_isDisposed = true;
			}
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			ThrowIfDisposed();
			throw new NotSupportedException(Messages.WritingNotSupported);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			ThrowIfCantRead();
			if (_superStream.Position != _positionInSuperStream)
			{
				_superStream.Seek(_positionInSuperStream, SeekOrigin.Begin);
			}
			if ((_positionInSuperStream + count) > _endInSuperStream)
			{
				count = (int) (_endInSuperStream - _positionInSuperStream);
			}
			int num = _superStream.Read(buffer, offset, count);
			_positionInSuperStream += num;
			return num;
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

        private void ThrowIfCantRead()
		{
			if (!CanRead)
			{
				throw new NotSupportedException(Messages.ReadingNotSupported);
			}
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
			ThrowIfDisposed();
			throw new NotSupportedException(Messages.WritingNotSupported);
		}

		public override bool CanRead => (_superStream.CanRead && _canRead);

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length
		{
			get
			{
				ThrowIfDisposed();
				return (_endInSuperStream - _startInSuperStream);
			}
		}

		public override long Position
		{
			get
			{
				ThrowIfDisposed();
				return (_positionInSuperStream - _startInSuperStream);
			}
			set
			{
				ThrowIfDisposed();
				throw new NotSupportedException(Messages.SeekingNotSupported);
			}
		}
	}
}
