using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Generic
{
    public static class ListTest
    {
        public enum Test
        {
            E1,
            E2
        }

        public enum LongTest : long
        {
            L1 = long.MaxValue - 1,
            L2 = long.MaxValue - 2,
            L3 = 0
        }
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

            //Test enums with lists
            List<Test> list = new List<Test>();
            list.Add(Test.E2);
            Assert.IsTrue(list[0] == Test.E2, "IL2CPU does not handle lists with Enums correctly");

            List<LongTest> list2 = new List<LongTest>();
            list2.Add(LongTest.L3);
            list2.Add(LongTest.L2);
            list2.Add(LongTest.L1);
            Assert.IsTrue(list2[2] == LongTest.L1, "Enums with underlying long type work");
            Assert.IsTrue(list2[1] == LongTest.L2, "Enums with underlying long type work");

        }
    }
}
