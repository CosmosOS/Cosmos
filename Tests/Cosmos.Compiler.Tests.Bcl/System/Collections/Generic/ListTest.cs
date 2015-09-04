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
            Assert.AreEqual(0, xList[0], "List[0] != 0");
            Assert.AreEqual(1, xList[1], "List[1] != 1");
            Assert.AreEqual(2, xList[2], "List[2] != 2");

            var xArray = xList.ToArray();

            Assert.AreEqual(3, xArray.Length, "xArray.Length != 3");
            Assert.AreEqual(0, xArray[0], "xArray[0] != 0");
            Assert.AreEqual(1, xArray[1], "xArray[1] != 1");
            Assert.AreEqual(2, xArray[2], "xArray[2] != 2");


        }
    }
}