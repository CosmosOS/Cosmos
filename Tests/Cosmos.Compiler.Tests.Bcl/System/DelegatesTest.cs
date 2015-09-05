using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class DelegatesTest
    {
        private static int mCount;

        private static void IncreaseCounterOnce()
        {
            mCount++;
        }

        private static void IncreaseCounterTwice()
        {
            mCount++;
        }
        
        public static void Execute()
        {
            mCount = 0;
            Action xDelegate = IncreaseCounterOnce;

            xDelegate();
            Assert.AreEqual(1, mCount, "After calling delegate once, Count != 1");
            //xDelegate += IncreaseCounterTwice;
            //xDelegate();
            //Assert.AreEqual(4, mCount, "After calling delegate second time, Count != 4");

        }
    }
}