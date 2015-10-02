using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public class DelegatesTest
    {
        private static int mCount;

        private static void IncreaseCounterOnce()
        {
            mCount++;
        }


        private void IncreaseCounterTwiceFromInstanceMethod()
        {
            mCount += 2;
        }

        public static void Execute()
        {
            TestDelegateWithoutArguments();
            TestDelegateWithArguments();
        }

        private static void TestDelegateWithoutArguments()
        {
            mCount = 0;
            Action xDelegate = IncreaseCounterOnce;

            xDelegate();
            Assert.AreEqual(1, mCount, "After calling delegate once, Count != 1");
            var xTestInstance = new DelegatesTest();
            xDelegate = xTestInstance.IncreaseCounterTwiceFromInstanceMethod;
            mCount = 0;
            xDelegate();
            Assert.AreEqual(2, mCount, "After calling delegate second time, Count != 2");
        }

        private static void IncreaseCounter(int number)
        {
            mCount += number;
        }

        private void IncreaseCounterFromInstanceMethod(int number)
        {
            mCount += number;
        }

        private static void TestDelegateWithArguments()
        {
            mCount = 0;
            Action<int> xDelegate = IncreaseCounter;

            xDelegate(2);
            Assert.AreEqual(2, mCount, "After calling delegate once, Count != 2");
            var xTestInstance = new DelegatesTest();
            xDelegate = xTestInstance.IncreaseCounterFromInstanceMethod;
            mCount = 0;
            xDelegate(3);
            Assert.AreEqual(3, mCount, "After calling delegate second time, Count != 3");
        }
    }
}
