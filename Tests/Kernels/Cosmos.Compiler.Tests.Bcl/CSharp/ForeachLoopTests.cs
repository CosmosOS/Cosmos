using System;
using System.Collections.Generic;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.CSharp
{
    public static class ForeachLoopTests
    {
        public static void Execute()
        {
            bool xResult;

            xResult = false;
            xResult = DoReturnFromForeachArray();
            Assert.IsTrue(xResult, "After return from foreach loop on array. Did not return true");

            xResult = false;
            xResult = DoReturnFromForeachList();
            Assert.IsTrue(xResult, "After return from foreach loop on list. Did not return true");

            xResult = false;
            xResult = DoBreakFromForeachArray();
            Assert.IsTrue(xResult, "After break from foreach loop on array. Did not return true");

            xResult = false;
            xResult = DoBreakFromForeachList();
            Assert.IsTrue(xResult, "After break from foreach loop on list. Did not return true");
        }

        private static bool DoReturnFromForeachArray()
        {
            int xFindMe = 3;
            int[] xArray = {1, 2, 3, 4, 5};

            foreach (int i in xArray)
            {
                if (i == xFindMe)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool DoReturnFromForeachList()
        {
            int xFindMe = 3;
            var xList = new List<int> {1, 2, 3, 4, 5};

            foreach (int i in xList)
            {
                if (i == xFindMe)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool DoBreakFromForeachArray()
        {
            bool xResult = false;
            int xFindMe = 3;
            int[] xArray = {1, 2, 3, 4, 5};

            foreach (int i in xArray)
            {
                if (i == xFindMe)
                {
                    xResult = true;
                    break;
                }

                xResult = false;
            }

            return xResult;
        }

        private static bool DoBreakFromForeachList()
        {
            bool xResult = false;
            int xFindMe = 3;
            var xList = new List<int> {1, 2, 3, 4, 5};

            foreach (int i in xList)
            {
                if (i == xFindMe)
                {
                    xResult = true;
                    break;
                }

                xResult = false;
            }

            return xResult;
        }
    }
}
