using System;
using System.IO;

namespace SwcBuild.IO.Compression
{
    internal class WrappedStream : Stream
	{
	    private readonly Stream _baseStream;
	    private bool _canRead;
	    private bool _canSeek;
	    private bool _canWrite;
	    private readonly bool _closeBaseStream;
	    private bool _isDisposed;
	    private readonly EventHandler _onClosed;

		internal WrappedStream(Stream baseStream, EventHandler onClosed) : this(baseStream, true, true, true, onClosed)
		{
		}

		internal WrappedStream(Stream baseStream, bool canRead, bool canWrite, bool canSeek, EventHandler onClosed) : this(baseStream, canRead, canWrite, canSeek, false, onClosed)
		{
		}

		internal WrappedStream(Stream baseStream, bool canRead, bool canWrite, bool canSeek, bool closeBaseStream, EventHandler onClosed)
		{
			_baseStream = baseStream;
			_onClosed = onClosed;
			_canRead = canRead;
			_canSeek = canSeek;
			_canWrite = canWrite;
			_isDisposed = false;
			_closeBaseStream = closeBaseStream;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				if (_onClosed != null)
				{
					_onClosed(this, null);
				}
				if (_closeBaseStream)
				{
					_baseStream.Dispose();
				}
				_canRead = false;
				_canWrite = false;
				_canSeek = false;
				_isDisposed = true;
			}
			base.Dispose(disposing);
		}

		public override void Flush()
		{
			ThrowIfDisposed();
			ThrowIfCantWrite();
			_baseStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			ThrowIfCantRead();
			return _baseStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			ThrowIfDisposed();
			ThrowIfCantSeek();
			return _baseStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			ThrowIfDisposed();
			ThrowIfCantSeek();
			ThrowIfCantWrite();
			_baseStream.SetLength(value);
		}

        private void ThrowIfCantRead()
		{
			if (!CanWrite)
			{
				throw new NotSupportedException(Messages.WritingNotSupported);
			}
		}

        private void ThrowIfCantSeek()
		{
			if (!CanSeek)
			{
				throw new NotSupportedException(Messages.SeekingNotSupported);
			}
		}

        private void ThrowIfCantWrite()
		{
			if (!CanWrite)
			{
				throw new NotSupportedException(Messages.WritingNotSupported);
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
			ThrowIfCantWrite();
			_baseStream.Write(buffer, offset, count);
		}

		public override bool CanRead => (_canRead && _baseStream.CanRead);

		public override bool CanSeek => (_canSeek && _baseStream.CanSeek);

		public override bool CanWrite => (_canWrite && _baseStream.CanWrite);

		public override long Length
		{
			get
			{
				ThrowIfDisposed();
				return _baseStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				ThrowIfDisposed();
				return _baseStream.Position;
			}
			set
			{
				ThrowIfDisposed();
				ThrowIfCantSeek();
				_baseStream.Position = value;
			}
		}
	}
}
