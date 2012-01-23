using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cosmos.IL2CPU.X86;

namespace Cosmos.Assembler.x86.Tests
{
    [TestFixture]
    public class AssemblerNasmTests
    {
        [Test]
        public void Test()
        {
            // public static int Test()
            Assert.AreEqual(8, AppAssemblerNasm.GetResultCodeOffset(4, 0), "static int Test()");
            // public static int Test(int)
            Assert.AreEqual(8, AppAssemblerNasm.GetResultCodeOffset(4, 4), "static int Test(int)");
            // public static long Test()
            Assert.AreEqual(8, AppAssemblerNasm.GetResultCodeOffset(8, 0), "static long Test()");
            // public static long Test(int)
            Assert.AreEqual(8, AppAssemblerNasm.GetResultCodeOffset(8, 4), "static long Test(int)");
            // public static int Test(long)
            Assert.AreEqual(12, AppAssemblerNasm.GetResultCodeOffset(4, 8), "static int Test(long)");
            // public static int Test(long,long)
            Assert.AreEqual(20, AppAssemblerNasm.GetResultCodeOffset(4, 16), "static int Test(long,long)");
            // public static long Test(long,long)
            Assert.AreEqual(16, AppAssemblerNasm.GetResultCodeOffset(8, 16), "static long Test(long,long)");
        }
    }
}