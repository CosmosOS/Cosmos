using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public class SpanTest
    {
        public static void Execute()
        {
            var arr = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                arr[i] = (byte)i;
            }
            Span<byte> span = arr;
            Assert.IsTrue(span != null, "Can create span from array");
            Assert.AreEqual(10, span.Length, "Span has correct length");
            Assert.AreEqual(2, span[2], "Span has correct values");
            Assert.AreEqual(8, span[8], "Span has correct values");
            span[3] = 20;
            Assert.AreEqual(20, span[3], "Span can change value");

            Span<byte> slice = span.Slice(start: 5, length: 3);
            Assert.IsTrue(slice != null, "Spans can be splived");
            Assert.AreEqual(3, slice.Length, "Sliced Span has correct length");
        }
    }
}
