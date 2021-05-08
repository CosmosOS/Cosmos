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
                TestController.Debugger.Send($"Expected value: '{expected}'");
                TestController.Debugger.Send($"Actual value: '{actual}'");
            }
            IsTrue(xResult, message, file, line);
        }

        public static void AreEqual(long expected, long actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            var xResult = expected == actual;
            if (!xResult)
            {
                TestController.Debugger.Send($"Expected value: '{expected}'");
                TestController.Debugger.Send($"Actual value: '{actual}'");
            }
            IsTrue(xResult, message, file, line);
        }

        public static void AreEqual(string[] expected, string[] actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if(expected.Length != actual.Length)
            {
                TestController.Debugger.Send($"Array lengths differ: Expected: {expected.Length} Actual: {actual.Length}");
                if (actual.Length < 32 && expected.Length < 32)
                {
                    TestController.Debugger.Send("Values in Expected:");
                    for (int i = 0; i < expected.Length; i++)
                    {
                        TestController.Debugger.Send(expected[i]);
                    }
                    TestController.Debugger.Send("Values in Actual:");
                    for (int i = 0; i < actual.Length; i++)
                    {
                        TestController.Debugger.Send(actual[i]);
                    }
                }
                IsTrue(false, message, file, line);
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                if(expected[i] != actual[i])
                {
                    TestController.Debugger.Send($"Values differ in row {i}");
                    TestController.Debugger.Send($"Expected value: '{expected[i]}'");
                    TestController.Debugger.Send($"Actual value: '{actual[i]}'");
                    IsTrue(false, message, file, line);
                    return;
                }
            }
            IsTrue(true, message, file, line);
        }
    }
}
