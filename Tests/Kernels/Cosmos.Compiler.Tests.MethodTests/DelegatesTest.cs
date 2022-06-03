using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.MethodTests
{

    public class DelegatesTest
    {
        private static int mCount;

        private static void IncreaseCounterOnce()
        {
            mCount++;
        }

        public static int WithReturnValue()
        {
            return 1;
        }

        public static string WithStringReturnValue()
        {
            return "Hello World";
        }

        private void IncreaseCounterTwiceFromInstanceMethod()
        {
            mCount += 2;
        }

        public static void Execute()
        {
            Func<int> funcInt = WithReturnValue;
            int val = funcInt();
            Assert.AreEqual(1, val, "Func<int> works");
            Func<string> funcString = WithStringReturnValue;
            string retString = funcString();
            Assert.AreEqual("Hello World", retString, "Func<string> works");
            TestDelegateWithoutArguments();
            TestDelegateWithArguments();
            TestMulticastDelegateWithoutArguments();
        }

        private static void TestDelegateWithoutArguments()
        {
            mCount = 0;
            Action xDelegate = IncreaseCounterOnce;
            xDelegate();
            Assert.AreEqual(1, mCount, "After calling delegate once, Count != 1");

            mCount = 0;
            var xTestInstance = new DelegatesTest();
            xDelegate = xTestInstance.IncreaseCounterTwiceFromInstanceMethod;
            xDelegate();
            Assert.AreEqual(2, mCount, "After calling delegate second time, Count != 2");
        }

        private static void TestMulticastDelegateWithoutArguments()
        {
            var xDebugger = new Debugger("Test", "Delegates");
            xDebugger.Send("Start MulticastDelegate test");
            mCount = 0;
            Action xDelegate = IncreaseCounterOnce;
            xDebugger.Send("Adding second handler now");
            xDelegate += IncreaseCounterOnce;
            xDelegate();
            Assert.AreEqual(2, mCount, "After calling multicast delegate once, Count != 2");
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

            mCount = 0;
            var xTestInstance = new DelegatesTest();
            xDelegate = xTestInstance.IncreaseCounterFromInstanceMethod;
            xDelegate(3);
            Assert.AreEqual(3, mCount, "After calling delegate second time, Count != 3");
        }
    }
}
