using System;
using System.IO;

namespace Symblr
{
    /// <summary>
    /// Represents a stream that will read a maximum of 
    /// <see cref="MaxBytes"/> at once.
    /// </summary>
    class TestStream : Stream
    {
        public const int MaxBytes = 3;

        public override bool CanRead
        {
            get { return _underlyingStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _underlyingStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _underlyingStream.CanWrite; }
        }

        public override bool CanTimeout
        {
            get { return _underlyingStream.CanTimeout; }
        }

        public override long Length
        {
            get { return _underlyingStream.Length; }
        }

        public override long Position
        {
            get { return _underlyingStream.Position; }
            set { _underlyingStream.Position = value; }
        }

        public override int ReadTimeout
        {
            get { return _underlyingStream.ReadTimeout; }
            set { _underlyingStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _underlyingStream.WriteTimeout; }
            set { _underlyingStream.WriteTimeout = value; }
        }

        private readonly Stream _underlyingStream;

        public bool DidRead { get; private set; }
        public bool DidWrite { get; private set; }
        public bool IsDisposed { get; private set; }

        public TestStream(Stream underlyingStream)
        {
            _underlyingStream = underlyingStream;
        }

        public TestStream(params byte[] data)
            : this(new MemoryStream(data))
        {

        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            DidRead = true;
            return _underlyingStream.BeginRead(buffer, offset, Math.Min(MaxBytes, count), callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            // Writes are not limited for sane reasons.
            DidWrite = true;
            return _underlyingStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            IsDisposed = true;
            _underlyingStream.Close();
        }

        public override System.Threading.Tasks.Task CopyToAsync(Stream destination, int bufferSize, System.Threading.CancellationToken cancellationToken)
        {
            DidRead = true;
            return _underlyingStream.CopyToAsync(destination, Math.Min(MaxBytes, bufferSize), cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            if (disposing) _underlyingStream.Dispose();
            base.Dispose(disposing);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _underlyingStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _underlyingStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _underlyingStream.Flush();
        }

        public override System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken)
        {
            return _underlyingStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            DidRead = true;
            return _underlyingStream.Read(buffer, offset, Math.Min(MaxBytes, count));
        }

        public override System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            DidRead = true;
            return _underlyingStream.ReadAsync(buffer, offset, Math.Min(MaxBytes, count), cancellationToken);
        }

        public override int ReadByte()
        {
            DidRead = true;
            return _underlyingStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _underlyingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _underlyingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            DidWrite = true;
            _underlyingStream.Write(buffer, offset, count);
        }

        public override System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            DidWrite = true;
            return _underlyingStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            DidWrite = true;
            _underlyingStream.WriteByte(value);
        }
    }
}
