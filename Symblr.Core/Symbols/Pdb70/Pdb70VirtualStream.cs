using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    partial class Pdb70File
    {
        /// <summary>
        /// Represents a stream that transparently accesses a PDB 7.00 stream.
        /// </summary>
        sealed class Pdb70VirtualStream : Stream
        {
            /// <summary>
            /// Gets a value indicating whether the current stream supports reading.
            /// </summary>
            public override bool CanRead
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the current stream supports seeking.
            /// </summary>
            public override bool CanSeek
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the current stream supports writing.
            /// </summary>
            public override bool CanWrite
            {
                get { return true; }
            }

            /// <summary>
            /// Gets the length in bytes of the stream.
            /// </summary>
            public override long Length
            {
                get { return _stream.StreamLength; }
            }

            /// <summary>
            /// Gets or sets the position within the current stream.
            /// </summary>
            public override long Position
            {
                get { return _position; }
                set { _position = value; }
            }

            private readonly Pdb70File _file;
            private readonly Pdb70StreamInfo _stream;
            private readonly byte[] _buffer;

            private long _position;
            private int _currentIndex;
            private bool _pendingWrites;

            /// <summary>
            /// Initializes a new instance of the <see cref="Pdb70VirtualStream"/> class.
            /// </summary>
            /// <param name="file">The file.</param>
            /// <param name="stream">The stream.</param>
            public Pdb70VirtualStream(Pdb70File file, Pdb70StreamInfo stream)
            {
                _file = file;
                _stream = stream;
                _buffer = new byte[_file._header.PageSize];
                _currentIndex = -1;
            }

            /// <summary>
            /// Ensures that the correct page is loaded.
            /// </summary>
            private void EnsurePage(bool isWriting)
            {
                var index = (int)(_position / _file._header.PageSize);
                if (index == _currentIndex) return;

                _file._streamLock.Wait();
                try
                {
                    // Write any pending changes out.
                    Flush(false);

                    // Read in the page.
                    _file._stream.Position = _stream[index] * _file._header.PageSize;
                    _currentIndex = index;

                    var count = _file._header.PageSize;
                    var offset = 0;
                    while (count > 0)
                    {
                        var read = _file._stream.Read(_buffer, offset, count);
                        if (read == 0)
                        {
                            if (isWriting) break;
                            throw new Pdb70LoadException(Pdb70LoadErrorCode.AssumedCorrupt);
                        }
                        offset += read;
                        count -= read;
                    }
                }
                finally
                {
                    _file._streamLock.Release();
                }
            }

            /// <summary>
            /// Ensures that the correct page is loaded.
            /// </summary>
            /// <param name="isWriting">if set to <c>true</c> data is being written.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// A <see cref="Task" /> that represents the asynchronous ensures operation.
            /// </returns>
            /// <exception cref="System.IO.EndOfStreamException"></exception>
            private async Task EnsurePageAsync(bool isWriting, CancellationToken cancellationToken)
            {
                var index = (int)(_position / _file._header.PageSize);
                if (index == _currentIndex) return;

                await _file._streamLock.WaitAsync(cancellationToken);
                try
                {
                    // Write any pending changes out.
                    await FlushAsync(false, cancellationToken);

                    // Read in the page.
                    _file._stream.Position = _stream[index] * _file._header.PageSize;
                    _currentIndex = index;

                    var count = _file._header.PageSize;
                    var offset = 0;
                    while (count > 0)
                    {
                        var read = await _file._stream.ReadAsync(_buffer, offset, count, cancellationToken);
                        if (read == 0)
                        {
                            if (isWriting) break;
                            throw new Pdb70LoadException(Pdb70LoadErrorCode.AssumedCorrupt);
                        }
                        offset += read;
                        count -= read;
                    }
                }
                finally
                {
                    _file._streamLock.Release();
                }
            }

            /// <summary>
            /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
            /// </summary>
            public override void Flush()
            {
                Flush(true);
            }

            /// <summary>
            /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
            /// </summary>
            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
            /// <returns>
            /// A task that represents the asynchronous flush operation.
            /// </returns>
            public override Task FlushAsync(System.Threading.CancellationToken cancellationToken)
            {
                return FlushAsync(true, cancellationToken);
            }

            /// <summary>
            /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
            /// </summary>
            /// <param name="acquireLock">if set to <c>true</c> a lock will be acquired for the flush.</param>
            private void Flush(bool acquireLock)
            {
                if (!_pendingWrites || _currentIndex < 0) return;

                if (acquireLock) _file._streamLock.Wait();
                try
                {
                    _file._stream.Position = _stream[_currentIndex] * _file._header.PageSize;
                    _file._stream.Write(_buffer, 0, _file._header.PageSize);
                    _pendingWrites = false;
                }
                finally
                {
                    if (acquireLock) _file._streamLock.Release();
                }
            }

            /// <summary>
            /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
            /// </summary>
            /// <param name="acquireLock">if set to <c>true</c> a lock will be acquired for the flush.</param>
            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
            /// <returns>
            /// A task that represents the asynchronous flush operation.
            /// </returns>
            private async Task FlushAsync(bool acquireLock, CancellationToken cancellationToken)
            {
                if (!_pendingWrites || _currentIndex < 0) return;

                if (acquireLock) await _file._streamLock.WaitAsync();
                try
                {
                    _file._stream.Position = _stream[_currentIndex] * _file._header.PageSize;
                    await _file._stream.WriteAsync(_buffer, 0, _file._header.PageSize, cancellationToken);
                    _pendingWrites = false;
                }
                finally
                {
                    if (acquireLock) _file._streamLock.Release();
                }
            }

            /// <summary>
            /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
            /// </summary>
            /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
            /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
            /// <returns>
            /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
            /// </returns>
            public override int Read(byte[] buffer, int offset, int count)
            {
                count = (int)Math.Min(count, Length - _position);
                var totalRead = 0;
                while (count > 0)
                {
                    EnsurePage(false);

                    var pageOffset = (int)(_position % _file._header.PageSize);
                    var pageRemainder = _file._header.PageSize - pageOffset;
                    var toRead = Math.Min(count, pageRemainder);

                    Buffer.BlockCopy(_buffer, pageOffset, buffer, offset, toRead);

                    offset += toRead;
                    totalRead += toRead;
                    count -= toRead;
                    _position += toRead;
                }
                return totalRead;
            }

            /// <summary>
            /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
            /// </summary>
            /// <param name="buffer">The buffer to write the data into.</param>
            /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
            /// <param name="count">The maximum number of bytes to read.</param>
            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
            /// <returns>
            /// A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
            /// </returns>
            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                count = (int)Math.Min(count, Length - _position);
                var totalRead = 0;
                while (count > 0)
                {
                    await EnsurePageAsync(false, cancellationToken);

                    var pageOffset = (int)(_position % _file._header.PageSize);
                    var pageRemainder = _file._header.PageSize - pageOffset;
                    var toRead = Math.Min(count, pageRemainder);

                    Buffer.BlockCopy(_buffer, pageOffset, buffer, offset, toRead);

                    offset += toRead;
                    totalRead += toRead;
                    count -= toRead;
                    _position += toRead;
                }
                return totalRead;
            }

            /// <summary>
            /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
            /// </summary>
            /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
            /// <param name="count">The number of bytes to be written to the current stream.</param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                SetLength(Math.Max(Length, _position + count));
                while (count > 0)
                {
                    EnsurePage(true);

                    var pageOffset = (int)(_position % _file._header.PageSize);
                    var pageRemainder = _file._header.PageSize - pageOffset;
                    var toWrite = Math.Min(count, pageRemainder);

                    Buffer.BlockCopy(buffer, offset, _buffer, pageOffset, toWrite);

                    offset += toWrite;
                    count -= toWrite;
                    _position += toWrite;
                    _pendingWrites = true;
                }
            }

            /// <summary>
            /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
            /// </summary>
            /// <param name="buffer">The buffer to write data from.</param>
            /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
            /// <param name="count">The maximum number of bytes to write.</param>
            /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
            /// <returns>
            /// A task that represents the asynchronous write operation.
            /// </returns>
            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                SetLength(Math.Max(Length, _position + count));
                while (count > 0)
                {
                    await EnsurePageAsync(true, cancellationToken);

                    var pageOffset = (int)(_position % _file._header.PageSize);
                    var pageRemainder = _file._header.PageSize - pageOffset;
                    var toWrite = Math.Min(count, pageRemainder);

                    Buffer.BlockCopy(buffer, offset, _buffer, pageOffset, toWrite);

                    offset += toWrite;
                    count -= toWrite;
                    _position += toWrite;
                    _pendingWrites = true;
                }
            }

            /// <summary>
            /// Sets the position within the current stream.
            /// </summary>
            /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
            /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
            /// <returns>
            /// The new position within the current stream.
            /// </returns>
            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin: _position = offset; break;
                    case SeekOrigin.Current: _position += offset; break;
                    case SeekOrigin.End: _position = Length - offset; break;
                }
                return _position;
            }

            /// <summary>
            /// Sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            public override void SetLength(long value)
            {
                var numPages = (int)(value + _file._header.PageSize - 1) / _file._header.PageSize;
                while (_stream.Count > numPages)
                {
                    _file._allocationTable.Deallocate(_stream[_stream.Count - 1]);
                    _stream.RemoveAt(_stream.Count - 1);
                }
                while (_stream.Count < numPages)
                {
                    _stream.Add(_file._allocationTable.Allocate());
                }
                _stream.StreamLength = (int)value;
                if (_stream == _file._indexStream) _file._header.IndexBytes = (int)value;
                _position = Math.Min(_position, Length);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Flush();
                }
                base.Dispose(disposing);
            }
        }
    }
}
