using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.IO
{
    /// <summary>
    /// Represents a <see cref="BinaryReader"/> that has asynchronous overloads.
    /// </summary>
    sealed class AsyncBinaryReader : BinaryReader
    {
        private readonly byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryReader"/> class.
        /// </summary>
        /// <param name="input">The input stream.</param>
        public AsyncBinaryReader(Stream input)
            : this(input, new UTF8Encoding(false, true), false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryReader"/> class.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public AsyncBinaryReader(Stream input, Encoding encoding)
            : this(input, encoding, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryReader"/> class.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after the <see cref="T:System.IO.BinaryReader" /> object is disposed; otherwise, false.</param>
        public AsyncBinaryReader(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
            _buffer = new byte[Math.Max(16, encoding.GetMaxByteCount(1))];
        }

        /// <summary>
        /// Asynchronously fills the internal buffer.
        /// </summary>
        /// <param name="numBytes">The number bytes to read.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{System.Byte[]}" /> that represents the asynchronous fill operation.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="numBytes" /> is an invalid value.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream was reached before the bytes could be read.</exception>
        private async Task<byte[]> FillBufferAsync(int numBytes, CancellationToken cancellationToken)
        {
            if (numBytes < 0 || numBytes > _buffer.Length)
                throw new ArgumentOutOfRangeException("numBytes");

            var offset = 0;
            while (numBytes > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var readBytes = await BaseStream.ReadAsync(_buffer, offset, numBytes, cancellationToken);
                if (readBytes == 0) throw new EndOfStreamException();
                offset += readBytes;
                numBytes -= readBytes;
            }
            return _buffer;
        }

        /// <summary>
        /// Asynchronously reads a number of bytes from the underlying stream.
        /// </summary>
        /// <param name="numBytes">The number of bytes to read.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{System.Byte[]}" /> that represents the asynchronous reads operation.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="numBytes" /> is an invalid value.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream was reached before the bytes could be read.</exception>
        public async Task<byte[]> ReadBytesAsync(int numBytes, CancellationToken cancellationToken)
        {
            if (numBytes < 0) throw new ArgumentOutOfRangeException("numBytes");

            var buffer = new byte[numBytes];
            var offset = 0;
            while (numBytes > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var readBytes = await BaseStream.ReadAsync(buffer, offset, numBytes, cancellationToken);
                if (readBytes == 0) throw new EndOfStreamException();
                offset += readBytes;
                numBytes -= readBytes;
            }
            return buffer;
        }

        /// <summary>
        /// Asynchronously reads a structure from the underlying stream.
        /// </summary>
        /// <typeparam name="T">The type of the structure to read.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{T}"/> that represents the asynchronous reads operation.
        /// </returns>
        public async Task<T> ReadStructureAsync<T>(CancellationToken cancellationToken)
            where T : struct
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = await ReadBytesAsync(Marshal.SizeOf(typeof(T)), cancellationToken);
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Asynchronously reads a 4-byte signed integer from the current stream and advances the position of the stream by four bytes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{System.Int32}"/> that represents the asynchronous read operation.
        /// </returns>
        public async Task<int> ReadInt32Async(CancellationToken cancellationToken)
        {
            return (int)(await ReadUInt32Async(cancellationToken));
        }

        /// <summary>
        /// Asynchronously reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{System.UInt32}"/> that represents the asynchronous read operation.
        /// </returns>
        public async Task<uint> ReadUInt32Async(CancellationToken cancellationToken)
        {
            await FillBufferAsync(4, cancellationToken);
            return (uint)_buffer[0] | (uint)_buffer[1] << 8 | (uint)_buffer[2] << 16 | (uint)_buffer[3] << 24;
        }
    }
}
