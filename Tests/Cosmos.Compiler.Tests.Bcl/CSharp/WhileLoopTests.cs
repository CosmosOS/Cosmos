using System;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.CSharp
{
    public static class WhileLoopTests
    {
        public static void Execute()
        {
            DoInt32Condition();
            DoObjectCondition();
        }

        private static void DoInt32Condition()
        {
            var xTimesInLoop = 0;
            var xIterator = 3;
            while (xIterator > 0)
            {
                xTimesInLoop++;
                xIterator--;
            }

            Assert.AreEqual(xTimesInLoop, 3, "After while loop with int32 condition, TimesInLoop is not 3!");
            Assert.AreEqual(xIterator, 0, "After while loop with int32 condition, Iterator is not zero!");
        }

        private static void DoObjectCondition()
        {
            var xTimesInLoop = 0;
            var xCondition = new Object();
            while (xCondition != null)
            {
                xTimesInLoop++;
                xCondition = null;
            }

            Assert.AreEqual(xTimesInLoop, 1, "After while loop with int32 condition, TimesInLoop is not 1!");
        }
    }
}