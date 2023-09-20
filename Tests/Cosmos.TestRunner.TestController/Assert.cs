using System;
using System.Runtime.CompilerServices;

namespace Cosmos.TestRunner
{
    public static class Assert
    {
        public static void Succeed(string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            TestController.Debugger.Send("Assertion succeeded:");
            TestController.Debugger.Send(message);
            TestController.AssertionSucceeded();
        }
        public static void Fail(string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            TestController.Debugger.Send("Assertion failed:");
            TestController.Debugger.Send("File: " + file);
            TestController.Debugger.Send("Line number: " + line);
            TestController.Debugger.Send(message);
            TestController.Failed();
            throw new Exception("Assertion failed!");
        }

        public static void IsTrue(bool condition, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (condition)
            {
                Succeed(message, file, line);
            }
            else
            {
                Fail(message, file, line);
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
                TestController.Debugger.SendNumber(expected);
                TestController.Debugger.Send("Actual value");
                TestController.Debugger.SendNumber(actual);

                TestController.Debugger.Send("Expected value");
                TestController.Debugger.SendNumber(expected);
                TestController.Debugger.Send("Actual value");
                TestController.Debugger.SendNumber(actual);

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

        public static void AreEqual(double expected, double actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            var xResult = Math.Abs(expected - actual) < 0.0001;
            if (!xResult)
            {
                TestController.Debugger.Send($"Expected value: '{expected}' " + BitConverter.ToString(BitConverter.GetBytes(expected)));
                TestController.Debugger.Send($"Actual value: '{actual}' " + BitConverter.ToString(BitConverter.GetBytes(actual)));
                TestController.Debugger.Send($"Diff: {xResult}");
            }
            IsTrue(xResult, message, file, line);
        }

        public static void AreEqual(string[] expected, string[] actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if(expected.Length != actual.Length)
            {
                TestController.Debugger.Send($"Array lengths differ: Expected: {expected.Length} Actual: {actual.Length}");
                if(actual.Length < 32)
                {
                    TestController.Debugger.Send("Actual Content:");
                    for (int i = 0; i < actual.Length; i++)
                    {
                        TestController.Debugger.Send(actual[i]);
                    }
                }
                Fail(message, file, line);
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                if(expected[i] != actual[i])
                {
                    TestController.Debugger.Send($"Values differ in row {i}");
                    TestController.Debugger.Send($"Expected value: '{expected[i]}'");
                    TestController.Debugger.Send($"Actual value: '{actual[i]}'");
                    Fail(message, file, line);
                    return;
                }
            }
            Succeed(message, file, line);
        }

        public static void AreNotEqual(uint expected, uint actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            var xResult = expected != actual;
            if (!xResult)
            {
                TestController.Debugger.Send("Expected value");
                TestController.Debugger.SendNumber((uint)expected);
                TestController.Debugger.Send("Actual value");
                TestController.Debugger.SendNumber((uint)actual);

                TestController.Debugger.Send("Expected value");
                TestController.Debugger.SendNumber(expected);
                TestController.Debugger.Send("Actual value");
                TestController.Debugger.SendNumber(actual);

                TestController.Debugger.Send("Numbers sent!");
            }
            IsTrue(xResult, message, file, line);
        }

        public static void AreEqual(byte[] expected, byte[] actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (expected.Length != actual.Length)
            {
                TestController.Debugger.Send($"Array lengths differ: Expected: {expected.Length} Actual: {actual.Length}");
                Fail(message, file, line);
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    TestController.Debugger.Send($"Values differ in row {i}");
                    TestController.Debugger.Send($"Expected value: '{expected[i]}'");
                    TestController.Debugger.Send($"Actual value: '{actual[i]}'");
                    TestController.Debugger.Send(BitConverter.ToString(actual));
                    Fail(message, file, line);
                    return;
                }
            }
            Succeed(message, file, line);
        }

        public static void AreEqual(uint[] expected, uint[] actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (expected.Length != actual.Length)
            {
                TestController.Debugger.Send($"Array lengths differ: Expected: {expected.Length} Actual: {actual.Length}");
                Fail(message, file, line);
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    TestController.Debugger.Send($"Values differ in row {i}");
                    TestController.Debugger.Send($"Expected value: '{expected[i]}'");
                    TestController.Debugger.Send($"Actual value: '{actual[i]}'");
                    Fail(message, file, line);
                    return;
                }
            }
            Succeed(message, file, line);
        }

        public static void AreEqual(int[] expected, int[] actual, string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (expected.Length != actual.Length)
            {
                TestController.Debugger.Send($"Array lengths differ: Expected: {expected.Length} Actual: {actual.Length}");
                Fail(message, file, line);
                return;
            }
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    TestController.Debugger.Send($"Values differ in row {i}");
                    TestController.Debugger.Send($"Expected value: '{expected[i]}'");
                    TestController.Debugger.Send($"Actual value: '{actual[i]}'");
                    Fail(message, file, line);
                    return;
                }
            }
            Succeed(message, file, line);
        }
    }
}
