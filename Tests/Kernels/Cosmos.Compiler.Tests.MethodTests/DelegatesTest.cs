using System;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.MethodTests
{
    public struct TestStruct
    {
        public int A;
        public long B;
        public string C;
    }

    public class ClassWithDelegates
    {
        public int X = 0;
        public string Y = "";

        public int GetX()
        {
            return X;
        }

        public void IncX()
        {
            X++;
        }
    }

    public struct StructWithDelegates
    {
        public int X = 0;
        
        public StructWithDelegates()
        {

        }

        public int GetX()
        {
            return X;
        }

        public void IncX()
        {
            X++;
        }

        public int IncAndReturnX()
        {
            X++;
            return X;
        }
    }

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

        public static int Increment(int i)
        {
            return i + 1;
        }

        public static int Sub(int a, int b)
        {
            return a - b;
        }

        public static TestStruct LargerReturn(int a, string c)
        {
            return new TestStruct
            {
                A = a,
                B = 1,
                C = c
            };
        }

        public static TestStruct SmallerReturn(int a, long b, string c, int d)
        {
            return new TestStruct
            {
                A = a + d,
                B = b,
                C = c
            };
        }

        public static string WithStringReturnValue()
        {
            return "Hello World";
        }

        public static TestStruct WithStructReturnValue()
        {
            return new TestStruct
            {
                A = 5,
                B = 0x8888888,
                C = "Test"
            };
        }

        private void IncreaseCounterTwiceFromInstanceMethod()
        {
            mCount += 2;
        }

        public static void TestDelegateWithReturnValue()
        {
            Func<int> funcInt = WithReturnValue;
            int val = funcInt();
            Assert.AreEqual(1, val, "Func<int> works");
            Func<string> funcString = WithStringReturnValue;
            string retString = funcString();
            Assert.AreEqual("Hello World", retString, "Func<string> works");
            Func<TestStruct> funcStruct = WithStructReturnValue;
            TestStruct testStruct = funcStruct();
            Assert.AreEqual(5, testStruct.A, "Func<Struct> returns first value correctly");
            Assert.AreEqual(0x8888888, testStruct.B, "Func<Struct> returns second value correctly");
            Assert.AreEqual("Test", testStruct.C, "Func<Struct> returns third value correctly");
            Func<int, int> funcIntParam = Increment;
            val = funcIntParam(10);
            Assert.AreEqual(11, val, "Func<int, int> works");
            val = funcIntParam(funcIntParam(funcIntParam(0)));
            Assert.AreEqual(3, val, "Calling same delegate works");
            Func<int, int, int> funcSub = Sub;
            val = funcSub(100, 15);
            Assert.AreEqual(85, val, "Func<int, int, int> works");
            Debugger.DoBochsBreak();
            TestStruct testStructa = LargerReturn(33, "String");
            Func<int, string, TestStruct> largeReturn = LargerReturn;
            testStruct = largeReturn(33, "String");
            Assert.AreEqual(33, testStruct.A, "Func<int, string, TestStruct> returns first value correctly");
            Assert.AreEqual(1, testStruct.B, "Func<int, string, TestStruct> returns second value correctly");
            Assert.AreEqual("String", testStruct.C, "Func<int, string, TestStruct> returns third value correctly");
            Func<int, long, string, int, TestStruct> smallerReturn = SmallerReturn;
            testStruct = smallerReturn(33, 100, "String2", 37);
            Assert.AreEqual(70, testStruct.A, "Func<int, long, string, int, TestStruct> returns first value correctly");
            Assert.AreEqual(100, testStruct.B, "Func<int, long, string, int, TestStruct> returns second value correctly");
            Assert.AreEqual("String2", testStruct.C, "Func<int, long, string, int, TestStruct> returns third value correctly");
        }

        public static void TestDelegateWithTarget()
        {
            ClassWithDelegates class1 = new ();
            Func<int> getX1 = class1.GetX;
            Assert.AreEqual(0, getX1(), "Func<int> works with method");
            class1.X = 10;
            Assert.AreEqual(10, getX1(), "Func<int> works with method and gets current value");
            Action incX1 = class1.IncX;
            incX1();
            Assert.AreEqual(11, class1.X, "Action works with method");
        }

        public static void TestDelegateWithStructTarget()
        {
            StructWithDelegates struct1 = new();
            Func<int> getX1 = struct1.GetX;
            Assert.AreEqual(0, getX1(), "Func<int> works with method");
            struct1.X = 10;
            Assert.AreEqual(0, getX1(), "Func<int> works with method and gets current value of the boxed instance");
            Action incX1 = struct1.IncX;
            incX1();
            Assert.AreEqual(0, getX1(), "Action works with method but changes the value of a different instance");
            Func<int> incAndGetX1 = struct1.IncAndReturnX;
            Assert.AreEqual(11, incAndGetX1(), "Creating Func<int> correctly keeps value from before boxing");
            Assert.AreEqual(12, incAndGetX1(), "Func<int> works on the same instance");
        }

        public static void Execute()
        {
            TestDelegateWithoutArguments();
            TestDelegateWithArguments();
            TestDelegateWithReturnValue();
            TestDelegateWithTarget();
            TestDelegateWithStructTarget();
            //TestMulticastDelegateWithoutArguments();
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
            Debugger xDebugger = new("Delegates");
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
