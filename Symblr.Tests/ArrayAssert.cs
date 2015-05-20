using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Symblr
{
    static class ArrayAssert
    {
        public static void All<T1, T2>(IEnumerable<T1> expected, IEnumerable<T2> actual, Action<T1, T2> each)
        {
            if (expected == null && actual == null) return;
            Assert.IsNotNull(expected, "it should be null.");
            Assert.IsNotNull(actual, "it should not be null.");

            using (var e = expected.GetEnumerator())
            using (var a = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    Assert.IsTrue(a.MoveNext(), "it should have the same number of items.");
                    each(e.Current, a.Current);
                }
                Assert.IsFalse(a.MoveNext(), "it should have the same number of items.");
            }
        }

        public static void AnyIsTrue<T>(IEnumerable<T> value, Func<T, bool> predicate, string message)
        {
            value = value ?? Enumerable.Empty<T>();
            Assert.IsTrue(value.Any(predicate), message);
        }
    }
}
