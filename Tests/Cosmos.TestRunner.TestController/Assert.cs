using System;
using System.Runtime.CompilerServices;

namespace Cosmos.TestRunner
{
    public static class Assert
    {
        public static void IsTrue(bool condition, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (condition)
            {
                TestController.Debugger.Send("Assertion succeeded:");
                TestController.Debugger.Send(message);
                TestController.AssertionSucceeded();
            }
            else
            {
                TestController.Debugger.Send("Assertion failed:");
                TestController.Debugger.Send("File: " + file);
                TestController.Debugger.Send("Line number: " + line);
                TestController.Debugger.Send(message);
                TestController.Failed();
                throw new Exception("Assertion failed!");
            }
        }

        public static void IsFalse(bool condition, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            IsTrue(!condition, message, file, line);
        }

        public static void AreEqual(int expected, int actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            var xResult = expected == actual;
            if (!xResult)
            {
                TestController.Debugger.Send("Expected value");
                TestController.Debugger.SendNumber((uint) expected);
                TestController.Debugger.Send("Actual value");
                TestController.Debugger.SendNumber((uint)actual);

                TestController.Debugger.SendNumber("TestAssertion", "Expected", (uint)expected, 32);
                TestController.Debugger.SendNumber("TestAssertion", "Actual", (uint)actual, 32);

                TestController.Debugger.Send("Numbers sent!");
            }
            IsTrue(xResult, message, file, line);
        }

        public static void AreEqual(string expected, string actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            var xResult = expected == actual;
            if (!xResult)
            {
                TestController.Debugger.Send("Expected value");
                TestController.Debugger.Send(expected);
                TestController.Debugger.Send("Actual value");
                TestController.Debugger.Send(actual);
            }
            IsTrue(xResult, message, file, line);
        }

    }
}
