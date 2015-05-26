using Symblr.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents a PDB 7.00 file.
    /// </summary>
    sealed partial class Pdb70File : IDisposable
    {
        private static readonly Encoding Encoding = new UTF8Encoding(false, true);

        private readonly Stream _stream;
        private readonly SemaphoreSlim _streamLock;

        private Pdb70Header _header;
        private Pdb70BitSet _allocationTable;
        private Pdb70StreamInfo _indexStream;
        private Pdb70SignatureHeader _signature;
        private byte[] _stream1Footer;

        private readonly List<Pdb70StreamInfo> _streams;
        private readonly Dictionary<string, int> _namedStreams;

        /// <summary>
        /// Gets the version of the PDB.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public int Version { get { return _signature.Version; } }

        /// <summary>
        /// Gets the signature of the PDB (legacy).
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        public int Signature { get { return _signature.Signature; } }

        /// <summary>
        /// Gets the age of the PDB.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int Age { get { return _signature.Age; } }

        /// <summary>
        /// Gets the unique identifier of the PDB.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get { return _signature.Guid; } }

        /// <summary>
        /// Gets the stream names.
        /// </summary>
        /// <value>
        /// The stream names.
        /// </value>
        public ICollection<string> StreamNames { get { return _namedStreams.Keys; } }

        /// <summary>
        /// Gets the number of streams within the PDB.
        /// </summary>
        /// <value>
        /// The number of streams within the PDB.
        /// </value>
        public int StreamCount { get { return _streams.Count; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb70File"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        private Pdb70File(Stream stream)
        {
            _stream = stream;
            _streamLock = new SemaphoreSlim(1, 1);
            _streams = new List<Pdb70StreamInfo>();
            _namedStreams = new Dictionary<string, int>();
        }

        #region Open
        /// <summary>
        /// Asynchronously opens a stream as a PDB 7.00 file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{Pdb20File}" /> that represents the asynchronous open operation. If the file is not a
        /// supported PDB 7.00 file the result of the task will be <c>null</c>.
        /// </returns>
        public static async Task<Pdb70File> TryOpenAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            stream.Position = 0;
            using (var reader = new AsyncBinaryReader(stream, Encoding, true))
            {
                try
                {
                    // Unable to use reader.ReadBytes because it throws an exception.
                    var signature = new byte[Pdb70Header.Signature.Length];
                    var pos = 0;
                    while (pos < signature.Length)
                    {
                        var length = await stream.ReadAsync(signature, pos, signature.Length - pos, cancellationToken);
                        if (length == 0) break;
                        pos += length;
                    }
                    if (!NativeMethods.MemoryEquals(Pdb70Header.Signature, signature)) return null;

                    var result = new Pdb70File(stream);
                    await result.ReadHeaderAsync(reader, cancellationToken);
                    await result.ReadBitmapAsync(reader, cancellationToken);
                    await result.ReadIndexAsync(reader, cancellationToken);
                    await result.ReadPdbHeadersAsync(cancellationToken);

                    return result;
                }
                catch (EndOfStreamException e)
                {
                    throw new Pdb70LoadException(Pdb70LoadErrorCode.AssumedCorrupt, e);
                }
                catch (SymblrException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new Pdb70LoadException(Pdb70LoadErrorCode.Unknown, e);
                }
            }
        }

        /// <summary>
        /// Asynchronously reads the header.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous read operation.
        /// </returns>
        private async Task ReadHeaderAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
        {
            await _streamLock.WaitAsync(cancellationToken);
            try
            {
                reader.BaseStream.Position = Pdb70Header.Signature.Length;
                _header = await reader.ReadStructureAsync<Pdb70Header>(cancellationToken);
            }
            finally
            {
                _streamLock.Release();
            }
        }

        /// <summary>
        /// Asynchronously reads the allocation bitmap.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous read operation.
        /// </returns>
        private async Task ReadBitmapAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
        {
            _allocationTable = new Pdb70BitSet(_header.PageCount / 32);

            await _streamLock.WaitAsync(cancellationToken);
            try
            {
                reader.BaseStream.Position = _header.PageSize * _header.BitmapPage;
                var bytes = await reader.ReadBytesAsync(_allocationTable.Words.Length * sizeof(int), cancellationToken);
                Buffer.BlockCopy(bytes, 0, _allocationTable.Words, 0, bytes.Length);
                for (var i = 0; i < _header.BitmapPage; i++)
                    _allocationTable[i] = false;
            }
            finally
            {
                _streamLock.Release();
            }
        }

        /// <summary>
        /// Asynchronously reads the index.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous read operation.
        /// </returns>
        private async Task ReadIndexAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
        {
            await _streamLock.WaitAsync(cancellationToken);
            try
            {
                _stream.Position = _header.IndexPage * _header.PageSize;

                var pages = (_header.IndexBytes + _header.PageSize - 1) / _header.PageSize;
                var bytes = await reader.ReadBytesAsync(pages * sizeof(int), cancellationToken);

                _indexStream = new Pdb70StreamInfo(_header.IndexBytes);
                for (var i = 0; i < pages; i++)
                {
                    var index = BitConverter.ToInt32(bytes, i * sizeof(int));
                    _indexStream.Add(index);
                    _allocationTable[index] = false;
                }
            }
            finally
            {
                _streamLock.Release();
            }

            using (var indexStream = new Pdb70VirtualStream(this, _indexStream))
            using (var indexReader = new AsyncBinaryReader(indexStream))
            {
                var count = await indexReader.ReadInt32Async(cancellationToken);
                var bytes = await indexReader.ReadBytesAsync(count * sizeof(int), cancellationToken);
                for (var i = 0; i < count; i++)
                    _streams.Add(new Pdb70StreamInfo(BitConverter.ToInt32(bytes, i * sizeof(int))));

                for (var i = 0; i < _streams.Count; i++)
                {
                    var stream = _streams[i];
                    if (stream.StreamLength > 0)
                    {
                        count = (int)(stream.StreamLength + _header.PageSize - 1) / _header.PageSize;
                        bytes = await indexReader.ReadBytesAsync(count * sizeof(int), cancellationToken);
                        for (var j = 0; j < count; j++)
                        {
                            var index = BitConverter.ToInt32(bytes, j * sizeof(int));
                            stream.Add(index);
                            _allocationTable[index] = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously reads the headers.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous reads operation.
        /// </returns>
        private async Task ReadPdbHeadersAsync(CancellationToken cancellationToken)
        {
            using (var headerStream = new Pdb70VirtualStream(this, _streams[1]))
            using (var headerReader = new AsyncBinaryReader(headerStream))
            {
                _signature = await headerReader.ReadStructureAsync<Pdb70SignatureHeader>(cancellationToken);

                // Names of streams.
                if (headerStream.Position < headerStream.Length)
                {
                    var size = await headerReader.ReadInt32Async(cancellationToken);
                    var data = await headerReader.ReadBytesAsync(size, cancellationToken);

                    var count = await headerReader.ReadInt32Async(cancellationToken);
                    var max = await headerReader.ReadInt32Async(cancellationToken);

                    var present = await Pdb70BitSet.ReadAsync(headerReader, cancellationToken);
                    var deleted = await Pdb70BitSet.ReadAsync(headerReader, cancellationToken);

                    if (!deleted.IsEmpty) throw new Pdb70LoadException(Pdb70LoadErrorCode.UnsupportedFeature);

                    for (var i = 0; i < max; i++)
                    {
                        if (present[i])
                        {
                            var bytes = await headerReader.ReadBytesAsync(2 * sizeof(int), cancellationToken);
                            var ns = BitConverter.ToInt32(bytes, 0);
                            var ni = BitConverter.ToInt32(bytes, sizeof(int));

                            for (var len = ns; len < data.Length; len++)
                            {
                                if (data[len] == '\0')
                                {
                                    var name = Encoding.GetString(data, ns, len - ns);
                                    _namedStreams[name] = ni;
                                    break;
                                }
                            }
                        }
                    }

                    // This has been constant in all PDBs I've tried: {0, 20091201}
                    // Seems like it could be sentinel value. Yuck.
                    _stream1Footer = await headerReader.ReadBytesAsync((int)(headerStream.Length - headerStream.Position), cancellationToken);
                }
            }
        }
        #endregion

        #region Save
        /// <summary>
        /// Asynchronously saves the PDB 7.00 file to disk.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous save operation.
        /// </returns>
        public async Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var writer = new AsyncBinaryWriter(_stream, Encoding, true))
            {
                await WritePdbHeadersAsync(cancellationToken);
                await WriteIndexAsync(writer, cancellationToken);
                await WriteBitmapAsync(writer, cancellationToken);
                await WriteHeaderAsync(writer, cancellationToken);
            }
        }

        /// <summary>
        /// Asynchronously writes the the headers to the file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write operation.
        /// </returns>
        private async Task WritePdbHeadersAsync(CancellationToken cancellationToken)
        {
            using (var headerStream = new Pdb70VirtualStream(this, _streams[1]))
            using (var writer = new AsyncBinaryWriter(headerStream))
            {
                headerStream.SetLength(28);
                headerStream.Position = 28;

                using (var data = new MemoryStream())
                {
                    var dataValues = new List<Tuple<int, int>>();
                    var present = new Pdb70BitSet(0);
                    var deleted = new Pdb70BitSet(0);

                    var i = 0;
                    foreach (var item in _namedStreams)
                    {
                        dataValues.Add(Tuple.Create((int)data.Length, item.Value));
                        var bytes = Encoding.GetBytes(item.Key);
                        data.Write(bytes, 0, bytes.Length);
                        data.WriteByte(0);
                        present[i++] = true;
                    }

                    data.Position = 0;
                    await writer.WriteAsync((int)data.Length, cancellationToken);
                    await data.CopyToAsync(headerStream, Environment.SystemPageSize, cancellationToken);

                    await writer.WriteAsync(dataValues.Count, cancellationToken); // Count
                    await writer.WriteAsync(dataValues.Count, cancellationToken); // Max

                    await present.WriteAsync(writer, cancellationToken);
                    await deleted.WriteAsync(writer, cancellationToken);

                    foreach (var value in dataValues)
                    {
                        await writer.WriteAsync(value.Item1, cancellationToken); // ns
                        await writer.WriteAsync(value.Item2, cancellationToken); // ni
                    }

                    await headerStream.WriteAsync(_stream1Footer, 0, _stream1Footer.Length, cancellationToken);
                    await headerStream.FlushAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Asynchronously writes the index to the file.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous writes operation.
        /// </returns>
        private async Task WriteIndexAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
        {
            using (var indexStream = new Pdb70VirtualStream(this, _indexStream))
            using (var indexWriter = new AsyncBinaryWriter(indexStream))
            {
                indexStream.SetLength(0);

                await indexWriter.WriteAsync(_streams.Count, cancellationToken);
                for (var i = 0; i < _streams.Count; i++)
                {
                    await indexWriter.WriteAsync(_streams[i].StreamLength, cancellationToken);
                }

                for (var i = 0; i < _streams.Count; i++)
                {
                    var stream = _streams[i];
                    var pages = (int)(stream.StreamLength + _header.PageSize - 1) / _header.PageSize;
                    for (var j = 0; j < pages; j++)
                    {
                        await indexWriter.WriteAsync(stream[j], cancellationToken);
                    }
                }

                await indexStream.FlushAsync(cancellationToken);
            }

            await _streamLock.WaitAsync(cancellationToken);
            try
            {
                _stream.Position = _header.IndexPage * _header.PageSize;
                for (var i = 0; i < _indexStream.Count; i++)
                {
                    await writer.WriteAsync(_indexStream[i], cancellationToken);
                }
            }
            finally
            {
                _streamLock.Release();
            }
        }

        /// <summary>
        /// Asynchronously writes the allocation bitmap.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write operation.
        /// </returns>
        private async Task WriteBitmapAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
        {
            var bytes = new byte[_allocationTable.Words.Length * sizeof(int)];
            Buffer.BlockCopy(_allocationTable.Words, 0, bytes, 0, bytes.Length);

            await _streamLock.WaitAsync(cancellationToken);
            try
            {
                _stream.Position = _header.BitmapPage * _header.PageSize;
                await writer.BaseStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            }
            finally
            {
                _streamLock.Release();
            }
        }

        /// <summary>
        /// Asynchronously writes the overall file header.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write operation.
        /// </returns>
        private async Task WriteHeaderAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
        {
            await _streamLock.WaitAsync(cancellationToken);
            try
            {
                _header.PageCount = (int)(_stream.Length + _header.PageSize - 1) / _header.PageSize;
                _stream.Position = Pdb70Header.Signature.Length;
                await writer.WriteStructureAsync(_header, cancellationToken);
            }
            finally
            {
                _streamLock.Release();
            }
        }
        #endregion

        /// <summary>
        /// Gets a value indicating whether the specified stream exists.
        /// </summary>
        /// <param name="index">The index of the stream.</param>
        /// <returns>A value indicating whether the stream exists.</returns>
        public bool StreamExists(int index)
        {
            return index < _streams.Count;
        }

        /// <summary>
        /// Gets a value indicating whether the specified stream exists.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <returns>
        /// A value indicating whether the stream exists.
        /// </returns>
        public bool StreamExists(string name)
        {
            return _namedStreams.ContainsKey(name);
        }

        /// <summary>
        /// Gets or creates the stream with the specified index.
        /// </summary>
        /// <param name="index">The index of the stream to get.</param>
        /// <returns>The stream.</returns>
        public Stream GetStream(int index)
        {
            while (_streams.Count <= index)
                _streams.Add(new Pdb70StreamInfo());
            return new Pdb70VirtualStream(this, _streams[index]);
        }

        /// <summary>
        /// Gets or creates the stream with the specified name.
        /// </summary>
        /// <param name="name">The name of the stream to get.</param>
        /// <returns>The stream.</returns>
        public Stream GetStream(string name)
        {
            int index;
            if (!_namedStreams.TryGetValue(name, out index))
                _namedStreams.Add(name, index = _streams.Count);
            return GetStream(index);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
                _streamLock.Dispose();
            }
        }
    }
}
