using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.IO
{
    /// <summary>
    /// Represents a <see cref="BinaryWriter"/> that has async methods.
    /// </summary>
    internal sealed class AsyncBinaryWriter : BinaryWriter
    {
        private readonly byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public AsyncBinaryWriter(Stream output)
            : this(output, new UTF8Encoding(false, true))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public AsyncBinaryWriter(Stream output, Encoding encoding)
            : this(output, encoding, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">
        /// <c>true </c>to leave the stream open after the <see cref="T:System.IO.BinaryWriter" /> object is disposed; otherwise,
        /// <c>false</c>.</param>
        public AsyncBinaryWriter(Stream output, Encoding encoding, bool leaveOpen)
            : base(output, encoding, leaveOpen)
        {
            _buffer = new byte[Math.Max(16, encoding.GetMaxByteCount(1))];
        }

        /// <summary>
        /// Asynchronously writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write operation.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        public Task WriteAsync(int value, CancellationToken cancellationToken)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            return OutStream.WriteAsync(this._buffer, 0, 4, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a structure to the current stream.
        /// </summary>
        /// <typeparam name="T">The type of structure to write.</typeparam>
        /// <param name="value">The structure.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> that represents the asynchronous write operation.
        /// </returns>
        public Task WriteStructureAsync<T>(T value, CancellationToken cancellationToken)
            where T : struct
        {
            var bytes = new byte[Marshal.SizeOf(typeof(T))];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
                return OutStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
