using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.TestRunner
{
    public static class Assert
    {
        public static void IsTrue(bool condition, string message)
        {
            if (condition)
            {
                TestController.Debugger.Send("Assertion succeeded:");
                TestController.Debugger.Send(message);
                TestController.AssertionSucceeded();
            }
            else
            {
                TestController.Debugger.Send("Assertion failed!:");
                TestController.Debugger.Send(message);
                TestController.Failed();
                throw new Exception("Assertion failed!");
            }
        }

        public static void IsFalse(bool condition, string message)
        {
            IsTrue(!condition, message);
        }

        public static void AreEqual(int expected, int actual, string message)
        {
            var xResult = expected == actual;
            if (!xResult)
            {
                Debugger.DoSend("Expected value");
                Debugger.DoSendNumber((uint) expected);
                Debugger.DoSend("Actual value");
                Debugger.DoSendNumber((uint)actual);
                TestController.Debugger.SendNumber("TestAssertion", "Expected", (uint)expected, 32);
                TestController.Debugger.SendNumber("TestAssertion", "Actual", (uint)actual, 32);
                TestController.Debugger.Send("Numbers sent!");
            }
            IsTrue(xResult, message);
        }
    }
}
