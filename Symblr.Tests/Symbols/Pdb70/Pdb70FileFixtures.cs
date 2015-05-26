using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    [TestClass]
    public class Pdb70FileFixtures
    {
        [TestMethod, TestCategory("PDB70")]
        public async Task When_reading_a_PDB_with_no_source_server_stream()
        {
            var stream = new TestStream(Pdbs.NoSrcSrv);
            using (var file = await Pdb70File.TryOpenAsync(stream))
            {
                Assert.AreEqual(1, file.Age, "it should read the correct age.");
                Assert.AreEqual(0x555CE245, file.Signature, "it should read the correct signature.");
                Assert.AreEqual(20000404, file.Version, "it should read the correct version.");
                Assert.AreEqual(Guid.Parse("{b59bba63-2d99-42fc-9f6b-cc52135dbb09}"), file.Guid, "it should read the correct GUID.");
                Assert.IsFalse(file.StreamExists("SRCSRV"), "the SRCSRV stream is not present.");
                Assert.IsTrue(file.StreamExists(1), "the index stream exists.");
                ArrayAssert.AnyIsTrue(file.StreamNames, x => x.StartsWith("/src/files/"), "it should read all stream names.");
                ArrayAssert.AnyIsTrue(file.StreamNames, x => x.StartsWith("/LinkInfo"), "it should read all stream names.");
            }

            Assert.IsTrue(stream.IsDisposed, "it should dispose the file.");
            Assert.IsTrue(stream.DidRead, "it should read bytes from the file.");
            Assert.IsFalse(stream.DidWrite, "it should not write bytes to the file.");
        }

        [TestMethod, TestCategory("PDB70")]
        public async Task When_writing_to_a_PDB_with_no_source_server_stream()
        {
            var longString = new StringBuilder();
            for (var i = 0; i < 1000; i++)
                longString.AppendFormat("Line {0}", i).AppendLine();

            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.NoSrcSrv, 0, Pdbs.NoSrcSrv.Length);
            stream.Position = 0;

            using (var file = await Pdb70File.TryOpenAsync(stream))
            {
                using (var srcsrv = file.GetStream("SRCSRV"))
                using (var writer = new StreamWriter(srcsrv))
                {
                    await writer.WriteAsync(longString.ToString());
                    await writer.FlushAsync();
                }

                await file.SaveAsync();

                stream = new TestStream(ms.ToArray());
                stream.Position = 0;
            }

            using (var file = await Pdb70File.TryOpenAsync(stream))
            {
                using (var srcsrv = file.GetStream("SRCSRV"))
                using (var reader = new StreamReader(srcsrv))
                {
                    var data = await reader.ReadToEndAsync();
                    Assert.AreEqual(longString.ToString(), data, "it should write out the SRCSRV entry.");
                }
            }
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_syncrhonously_writing_to_a_PDB_with_no_source_server_stream()
        {
            var longString = new StringBuilder();
            for (var i = 0; i < 1000; i++)
                longString.AppendFormat("Line {0}", i).AppendLine();

            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            stream.Write(Pdbs.NoSrcSrv, 0, Pdbs.NoSrcSrv.Length);
            stream.Position = 0;

            using (var file = Pdb70File.TryOpenAsync(stream).Result)
            {
                using (var srcsrv = file.GetStream("SRCSRV"))
                using (var writer = new StreamWriter(srcsrv))
                {
                    writer.Write(longString.ToString());
                    writer.Flush();
                }

                file.SaveAsync().Wait();

                stream = new TestStream(ms.ToArray());
                stream.Position = 0;
            }

            using (var file = Pdb70File.TryOpenAsync(stream).Result)
            {
                using (var srcsrv = file.GetStream("SRCSRV"))
                using (var reader = new StreamReader(srcsrv))
                {
                    var data = reader.ReadToEnd();
                    Assert.AreEqual(longString.ToString(), data, "it should write out the SRCSRV entry.");
                }
            }
        }

        [TestMethod, TestCategory("PDB70")]
        public async Task When_reading_a_PDB_with_a_source_server_stream()
        {
            // NB: if you change this resource file do so with PDBSTR and not
            // this library. Microsoft tools are the source of truth.
            // "C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\pdbstr.exe" -w -p:SrcSrv.pdb -i:srcsrv.txt -s:SRCSRV

            var longString = new StringBuilder();
            for (var i = 0; i < 1000; i++)
                longString.AppendFormat("PDBSTR {0}", i).AppendLine();

            var stream = new TestStream(Pdbs.SrcSrv);
            using (var file = await Pdb70File.TryOpenAsync(stream))
            {
                Assert.AreEqual(2, file.Age, "it should read the correct age.");
                Assert.AreEqual(0x555CE245, file.Signature, "it should read the correct signature.");
                Assert.AreEqual(20000404, file.Version, "it should read the correct version.");
                Assert.AreEqual(Guid.Parse("{b59bba63-2d99-42fc-9f6b-cc52135dbb09}"), file.Guid, "it should read the correct GUID.");
                Assert.IsTrue(file.StreamExists("SRCSRV"), "the SRCSRV stream is not present.");
                Assert.IsTrue(file.StreamExists(1), "the index stream exists.");
                ArrayAssert.AnyIsTrue(file.StreamNames, x => x.StartsWith("/src/files/"), "it should read all stream names.");
                ArrayAssert.AnyIsTrue(file.StreamNames, x => x.StartsWith("/LinkInfo"), "it should read all stream names.");

                using (var srcsrv = file.GetStream("SRCSRV"))
                using (var reader = new StreamReader(srcsrv))
                {
                    var data = await reader.ReadToEndAsync();
                    Assert.AreEqual(longString.ToString(), data, "it should read in the SRCSRV entry.");
                }
            }

            Assert.IsTrue(stream.IsDisposed, "it should dispose the file.");
            Assert.IsTrue(stream.DidRead, "it should read bytes from the file.");
            Assert.IsFalse(stream.DidWrite, "it should not write bytes to the file.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_synchronously_reading_a_PDB_with_a_source_server_stream()
        {
            // NB: if you change this resource file do so with PDBSTR and not
            // this library. Microsoft tools are the source of truth.
            // "C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\pdbstr.exe" -w -p:SrcSrv.pdb -i:srcsrv.txt -s:SRCSRV

            var longString = new StringBuilder();
            for (var i = 0; i < 1000; i++)
                longString.AppendFormat("PDBSTR {0}", i).AppendLine();

            var stream = new TestStream(Pdbs.SrcSrv);
            using (var file = Pdb70File.TryOpenAsync(stream).Result)
            {
                Assert.AreEqual(2, file.Age, "it should read the correct age.");
                Assert.AreEqual(0x555CE245, file.Signature, "it should read the correct signature.");
                Assert.AreEqual(20000404, file.Version, "it should read the correct version.");
                Assert.AreEqual(Guid.Parse("{b59bba63-2d99-42fc-9f6b-cc52135dbb09}"), file.Guid, "it should read the correct GUID.");
                Assert.IsTrue(file.StreamExists("SRCSRV"), "the SRCSRV stream is not present.");
                Assert.IsTrue(file.StreamExists(1), "the index stream exists.");
                ArrayAssert.AnyIsTrue(file.StreamNames, x => x.StartsWith("/src/files/"), "it should read all stream names.");
                ArrayAssert.AnyIsTrue(file.StreamNames, x => x.StartsWith("/LinkInfo"), "it should read all stream names.");

                using (var srcsrv = file.GetStream("SRCSRV"))
                using (var reader = new StreamReader(srcsrv))
                {
                    var data = reader.ReadToEnd();
                    Assert.AreEqual(longString.ToString(), data, "it should read in the SRCSRV entry.");
                }
            }

            Assert.IsTrue(stream.IsDisposed, "it should dispose the file.");
            Assert.IsTrue(stream.DidRead, "it should read bytes from the file.");
            Assert.IsFalse(stream.DidWrite, "it should not write bytes to the file.");
        }

        [TestMethod, TestCategory("PDB70")]
        [ExpectedException(typeof(Pdb70LoadException), "it should throw a load exception.")]
        public async Task When_reading_a_truncated_PDB()
        {
            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.NoSrcSrv, 0, Pdbs.NoSrcSrv.Length / 2);
            stream.Position = 0;

            try
            {
                await Pdb70File.TryOpenAsync(stream);
            }
            catch (Pdb70LoadException e)
            {
                Assert.AreEqual(Pdb70LoadErrorCode.AssumedCorrupt, e.ErrorCode, "it should have the correct error code");
                throw;
            }
        }

        [TestMethod, TestCategory("PDB70")]
        [ExpectedException(typeof(Pdb70LoadException), "it should throw a load exception.")]
        public async Task When_reading_a_truncated_PDB_with_a_valid_index()
        {
            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            var didReadStream = false;
            await stream.WriteAsync(Pdbs.NoSrcSrv, 0, Pdbs.NoSrcSrv.Length);
            stream.Position = 0;

            try
            {
                using (var f = await Pdb70File.TryOpenAsync(stream))
                {
                    didReadStream = true;

                    // Now that the index has been read cause the next stream
                    // to fail to read.
                    stream.SetLength(stream.Length / 2);

                    for (var i = 0; i < f.StreamCount; i++)
                    {
                        using (var s = f.GetStream(i))
                        {
                            var b = new byte[s.Length];
                            await s.ReadAsync(b, 0, b.Length);
                        }
                    }
                }
            }
            catch (Pdb70LoadException e)
            {
                if (!didReadStream) Assert.Inconclusive("The PDB read failed at the incorrect time.");
                Assert.AreEqual(Pdb70LoadErrorCode.AssumedCorrupt, e.ErrorCode, "it should have the correct error code");
                throw;
            }
        }

        [TestMethod, TestCategory("PDB70")]
        public async Task When_reading_a_non_PDB()
        {
            var stream = new TestStream(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            var v = await Pdb70File.TryOpenAsync(stream);
            Assert.IsNull(v, "it should return null.");
        }

        [TestMethod, TestCategory("PDB70")]
        [ExpectedException(typeof(Pdb70LoadException), "it should throw an exception.")]
        public async Task When_reading_a_PDB_with_a_deleted_header_entry()
        {
            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.DeletedBitSet, 0, Pdbs.DeletedBitSet.Length);
            stream.Position = 0;

            try
            {
                await Pdb70File.TryOpenAsync(stream);
            }
            catch (Pdb70LoadException e)
            {
                Assert.AreEqual(Pdb70LoadErrorCode.UnsupportedFeature, e.ErrorCode, "it should have the correct error code.");
                throw;
            }
        }

        [TestMethod, TestCategory("PDB70")]
        [ExpectedException(typeof(Pdb70LoadException), "it should throw an exception.")]
        public async Task When_an_exception_is_thrown_during_reading()
        {
            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.SrcSrv, 0, Pdbs.SrcSrv.Length);
            stream.Position = 0;
            stream.ThrowAfter = 1024;

            try
            {
                await Pdb70File.TryOpenAsync(stream);
            }
            catch (Pdb70LoadException e)
            {
                Assert.AreEqual(Pdb70LoadErrorCode.Unknown, e.ErrorCode, "it should have the correct error code.");
                Assert.IsNotNull(e.InnerException, "it should have an inner exception.");
                Assert.AreEqual(e.InnerException.Message, "Exception from the stream.", "it should have the correct inner exception.");
                throw;
            }
        }
    }
}
