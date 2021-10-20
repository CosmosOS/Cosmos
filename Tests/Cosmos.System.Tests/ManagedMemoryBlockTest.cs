using Cosmos.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Tests
{
    class ManagedMemoryBlockTest
    {
        [Test]
        public void TestBlock()
        {
            var memoryBlock = new ManagedMemoryBlock(128);
            memoryBlock.Write32(0, 1);
            Assert.AreEqual(1, memoryBlock[0]);
            Assert.AreEqual(1, memoryBlock.Read32(0), "ManagedMemoryBlock read/write at index 0 works");
            memoryBlock.Write32(1, 101);
            Assert.AreEqual(101, memoryBlock[1], "ManagedMemoryBlock read/write at index 1 works");
            Assert.AreEqual(25857, memoryBlock.Read32(0), "ManagedMemoryBlock read int at index 0 works");
            memoryBlock.Write32(2, 2 ^ 16 + 2);
            Assert.AreEqual(16, memoryBlock[2], "ManagedMemoryBlock write int at index 2 works");
            Assert.AreEqual(0, memoryBlock[3], "ManagedMemoryBlock write int at index 2 works");
            Assert.AreEqual(1074433, memoryBlock.Read32(0), "ManagedMemoryBlock read int at index 0 works");
            memoryBlock.Write32(3, int.MaxValue);
            Assert.AreEqual(255, memoryBlock[3], "ManagedMemoryBlock write int at index 3 works");
            Assert.AreEqual(0xFF106501, memoryBlock.Read32(0), "ManagedMemoryBlock read int at index 0 works");
            Assert.AreEqual(0xFFFF1065, memoryBlock.Read32(1), "ManagedMemoryBlock read int at index 1 works");
            Assert.AreEqual(0xFFFFFF10, memoryBlock.Read32(2), "ManagedMemoryBlock read int at index 2 works");
            Assert.AreEqual(int.MaxValue, memoryBlock.Read32(3), "ManagedMemoryBlock read/write at index 3 works");

            memoryBlock.Fill(101);
            Assert.AreEqual(101, memoryBlock.Read32(0), "ManagedMemoryBlock fill works at index 0");
            Assert.AreEqual(0, memoryBlock[1], "ManagedMemoryBlock fill fills entire ints");
            Assert.AreEqual(6619136, memoryBlock.Read32(10), "ManagedMemoryBlock fill works at index 10");

            memoryBlock.Write8(0, 101);
            Assert.AreEqual(101, memoryBlock[0], "ManagedMemoryBlock write byte works at index 0");
            memoryBlock.Fill(1, 1, 987893745);
            Assert.AreEqual(101, memoryBlock[0], "ManagedMemoryBlock Fill(1, int, int) skips index 0");
            Assert.AreEqual(987893745, memoryBlock.Read32(1), "ManagedMemoryBlock Fill(int, int, int) works at index 1");
        }
    }
}
