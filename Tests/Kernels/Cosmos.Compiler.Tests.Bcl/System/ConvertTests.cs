using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class ConvertTests
    {
        public static void Execute()
        {
            Assert.AreEqual("1010", Convert.ToString(10, 2), "Convert.ToString(int, 2) works");
            Assert.AreEqual("12", Convert.ToString(10, 8), "Convert.ToString(int, 8) works");
            Assert.AreEqual("10", Convert.ToString(10, 10), "Convert.ToString(int, 10) works");
            Assert.AreEqual("A", Convert.ToString(10, 16), "Convert.ToString(int, 16) works");
            Assert.AreEqual("11000100000", Convert.ToString(1568, 2), "Convert.ToString(int, 2) works");
            Assert.AreEqual("3040", Convert.ToString(1568, 8), "Convert.ToString(int, 8) works");
            Assert.AreEqual("1568", Convert.ToString(1568, 10), "Convert.ToString(int, 10) works");
            Assert.AreEqual("620", Convert.ToString(1568, 16), "Convert.ToString(int, 16) works");

        }
    }
}
