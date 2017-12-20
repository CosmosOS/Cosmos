using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Generic
{
    public static class ListTest
    {
        public static void Execute()
        {
            var xList = new List<int>();

            Assert.AreEqual(0, xList.Count, "Count != 0 at start!");

            xList.Add(0);
            xList.Add(1);
            xList.Add(2);

            Assert.AreEqual(3, xList.Count, "After adding 3 items, count != 3");
            Assert.AreEqual(0, xList[0], "List<int>.Add: xList[0] != 0");
            Assert.AreEqual(1, xList[1], "List<int>.Add: xList[1] != 1");
            Assert.AreEqual(2, xList[2], "List<int>.Add: xList[2] != 2");

            var xArray = xList.ToArray();

            Assert.AreEqual(3, xArray.Length, "List<int>.ToArray: xArray.Length != 3");
            Assert.AreEqual(0, xArray[0], "List<int>.ToArray: xArray[0] != 0");
            Assert.AreEqual(1, xArray[1], "List<int>.ToArray: xArray[1] != 1");
            Assert.AreEqual(2, xArray[2], "List<int>.ToArray: xArray[2] != 2");

            xList.Insert(1, 5);

            Assert.AreEqual(0, xList[0], "List<int>.Insert: xList[0] != 0");
            Assert.AreEqual(5, xList[1], "List<int>.Insert: xList[1] != 5");
            Assert.AreEqual(1, xList[2], "List<int>.Insert: xList[2] != 1");
            Assert.AreEqual(2, xList[3], "List<int>.Insert: xList[3] != 2");

            xList.Insert(0, 7);

            Assert.AreEqual(7, xList[0], "List<int>.Insert: xList[0] != 7");
            Assert.AreEqual(0, xList[1], "List<int>.Insert: xList[1] != 0");
            Assert.AreEqual(5, xList[2], "List<int>.Insert: xList[2] != 5");
            Assert.AreEqual(1, xList[3], "List<int>.Insert: xList[3] != 1");
            Assert.AreEqual(2, xList[4], "List<int>.Insert: xList[4] != 2");

            xList.RemoveAt(2);

            Assert.AreEqual(7, xList[0], "List<int>.RemoveAt: xList[0] != 7");
            Assert.AreEqual(0, xList[1], "List<int>.RemoveAt: xList[1] != 0");
            Assert.AreEqual(1, xList[2], "List<int>.RemoveAt: xList[2] != 1");
            Assert.AreEqual(2, xList[3], "List<int>.RemoveAt: xList[3] != 2");

            xList.RemoveAt(0);

            Assert.AreEqual(0, xList[0], "List<int>.RemoveAt: xList[0] != 0");
            Assert.AreEqual(1, xList[1], "List<int>.RemoveAt: xList[1] != 1");
            Assert.AreEqual(2, xList[2], "List<int>.RemoveAt: xList[2] != 2");


            // Commented tests depend on #583

            //xList.AddRange(new List<int>() { 3, 4, 5 });

            //Assert.AreEqual(0, xList[0], "List<int>.AddRange: xList[0] != 0");
            //Assert.AreEqual(5, xList[1], "List<int>.AddRange: xList[1] != 5");
            //Assert.AreEqual(2, xList[2], "List<int>.AddRange: xList[2] != 2");
            //Assert.AreEqual(3, xList[2], "List<int>.AddRange: xList[3] != 3");
            //Assert.AreEqual(4, xList[2], "List<int>.AddRange: xList[4] != 4");
            //Assert.AreEqual(5, xList[2], "List<int>.AddRange: xList[5] != 5");

            //xList.RemoveRange(2, 2);

            //Assert.AreEqual(0, xList[0], "List<int>.RemoveRange: xList[0] != 0");
            //Assert.AreEqual(5, xList[1], "List<int>.RemoveRange: xList[1] != 5");
            //Assert.AreEqual(4, xList[2], "List<int>.RemoveRange: xList[2] != 4");
            //Assert.AreEqual(5, xList[2], "List<int>.RemoveRange: xList[3] != 5");

            //var xRange = xList.GetRange(1, 3);

            //Assert.AreEqual(5, xRange[0], "List<int>.GetRange: xRange[0] != 5");
            //Assert.AreEqual(4, xRange[1], "List<int>.GetRange: xRange[1] != 4");
            //Assert.AreEqual(5, xRange[2], "List<int>.GetRange: xRange[2] != 5");
        }
    }
}
