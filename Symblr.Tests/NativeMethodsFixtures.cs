using System;
using Xunit;

namespace Symblr
{
    public class NativeMethodsFixtures
    {
        [Fact]
        public void When_comparing_two_null_arrays()
        {
            var val = NativeMethods.MemoryEquals(null, null);
            Assert.True(val);
        }

        [Fact]
        public void When_comparing_a_null_array_to_a_non_null_array()
        {
            var val = NativeMethods.MemoryEquals(null, new byte[0]);
            Assert.False(val);
        }

        [Fact]
        public void When_comparing_a_non_null_array_to_a_null_array()
        {
            var val = NativeMethods.MemoryEquals(new byte[0], null);
            Assert.False(val);
        }

        [Fact]
        public void When_comparing_differently_sized_arrays()
        {
            var val = NativeMethods.MemoryEquals(new byte[0], new byte[1]);
            Assert.False(val);
        }

        [Fact]
        public void When_comparing_two_zero_sized_arrays()
        {
            var val = NativeMethods.MemoryEquals(new byte[0], new byte[0]);
            Assert.True(val);
        }

        [Fact]
        public void When_comparing_two_arrays_with_a_different_first_value()
        {
            var val = NativeMethods.MemoryEquals(new byte[] { 1 }, new byte[] { 2 });
            Assert.False(val);
        }

        [Fact]
        public void When_comparing_two_very_different_arrays()
        {
            var val = NativeMethods.MemoryEquals(new byte[] { 1, 1 }, new byte[] { 1, 2 });
            Assert.False(val);
        }

        [Fact]
        public void When_comparing_two_identical_arrays()
        {
            var val = NativeMethods.MemoryEquals(new byte[] { 1, 2 }, new byte[] { 1, 2 });
            Assert.True(val);
        }

        [Fact]
        public void When_comparing_an_array_to_itself()
        {
            var arr = new byte[] { 1, 2 };
            var val = NativeMethods.MemoryEquals(arr, arr);
            Assert.True(val);
        }

        [Fact]
        public void When_setting_memory_to_zero()
        {
            var arr = new byte[] { 1, 2 };
            NativeMethods.MemorySet(arr);
            Assert.Equal(new byte[] { 0, 0 }, arr);
        }

        [Fact]
        public void When_setting_memory_to_20()
        {
            var arr = new byte[] { 1, 2 };
            NativeMethods.MemorySet(arr, 20);
            Assert.Equal(new byte[] { 20, 20 }, arr);
        }

        [Fact]
        public void When_setting_null_memory()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                NativeMethods.MemorySet(null);
            });
        }
    }
}
