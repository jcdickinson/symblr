using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.Symbols
{
    public class SourceInformationFixtures
    {
        [Fact]
        public void When_constructing_with_a_null_original_file()
        {
            Assert.Throws<ArgumentNullException>(()=>
            {
                new SourceInformation(null);
            });
        }

        [Fact]
        public void When_constructing_with_a_null_original_file_and_server_file()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SourceInformation(null, null);
            });
        }

        [Fact]
        public void When_constructing_with_a_null_server_file()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SourceInformation("test", null);
            });
        }

        [Fact]
        public void When_converting_an_empty_SourceInformation_to_a_string()
        {
            var sut = default(SourceInformation).ToString();
            Assert.Equal("*", sut);
        }
    }
}
