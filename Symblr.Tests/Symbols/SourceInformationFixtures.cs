using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Symbols
{
    [TestClass]
    public class SourceInformationFixtures
    {
        [TestMethod, TestCategory("Symbols")]
        [ExpectedException(typeof(ArgumentNullException), "it should disallow a null original file.")]
        public void When_constructing_with_a_null_original_file()
        {
            new SourceInformation(null);
        }

        [TestMethod, TestCategory("Symbols")]
        [ExpectedException(typeof(ArgumentNullException), "it should disallow a null original file.")]
        public void When_constructing_with_a_null_original_file_and_server_file()
        {
            new SourceInformation(null, null);
        }

        [TestMethod, TestCategory("Symbols")]
        [ExpectedException(typeof(ArgumentNullException), "it should disallow a null original file.")]
        public void When_constructing_with_a_null_server_file()
        {
            new SourceInformation("test", null);
        }

        [TestMethod, TestCategory("Symbols")]
        public void When_converting_an_empty_SourceInformation_to_a_string()
        {
            var sut = default(SourceInformation).ToString();
            Assert.AreEqual<string>("*", sut, "it should convert correctly.");
        }
    }
}
