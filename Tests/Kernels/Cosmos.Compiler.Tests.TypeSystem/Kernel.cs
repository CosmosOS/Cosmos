using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU;
using Cosmos.TestRunner;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.TypeSystem
{
    static class StaticTestClass
    {
        internal static object A;
        internal static TestType B;
    }

    class A
    {

    }

    class B : A
    {

    }

    class TestType
    {
        public int FieldA;
        public object FieldB;
        public string FieldC;
        public List<int> FieldD;
        public object FieldE;
    }

    class Counter
    {
        public int x;
    }

    public class Kernel : Sys.Kernel
    {
        // When testing the changes locally alt tabbing on to bochs/sending key codes will cause more objects to be freed and
        // can cause the tests to fail if badly timed
        // so just let the tests run in peace :)

        private static int test = 0;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting Type tests now please wait...");
        }

        private void TestVTablesImpl()
        {
            object obj = new object();

            Assert.AreEqual(GCImplementation.GetType(obj), ((CosmosRuntimeType)obj.GetType()).mTypeId, "Both methods to get type id return the same value for object");

            string s = "a";
            Assert.AreEqual(GCImplementation.GetType(s), ((CosmosRuntimeType)s.GetType()).mTypeId, "Both methods to get type id return the same value for string");

            Assert.AreEqual(GCImplementation.GetType(s), ((CosmosRuntimeType)typeof(string)).mTypeId, "Methods and constato get type id return the same value for string");
            List<int> x = new List<int>();
            Assert.AreEqual(GCImplementation.GetType(x), ((CosmosRuntimeType)typeof(List<int>)).mTypeId, "Methods and constant get type id return the same value for List<int>");

            TestType tObj = new TestType();
            Assert.AreEqual(GCImplementation.GetType(tObj), ((CosmosRuntimeType)typeof(TestType)).mTypeId, "Methods and constant get type id return the same value for TestType");

            Assert.AreEqual(4, VTablesImpl.GetGCFieldCount(GCImplementation.GetType(tObj)), "TestType has 4 fields tracked by GC");

            var types = VTablesImpl.GetGCFieldTypes(GCImplementation.GetType(tObj));
            Assert.AreEqual(4, types.Length, "GetGCFieldTypes returns correct number of values");
            Assert.AreEqual(((CosmosRuntimeType)typeof(object)).mTypeId, types[0], "GetGCFieldTypes returns object at offset 0");
            Assert.AreEqual(((CosmosRuntimeType)typeof(List<int>)).mTypeId, types[1], "GetGCFieldTypes returns List<int> at offset 1");
            Assert.AreEqual(((CosmosRuntimeType)typeof(string)).mTypeId, types[2], "GetGCFieldTypes returns string at offset 2");
            Assert.AreEqual(((CosmosRuntimeType)typeof(object)).mTypeId, types[3], "GetGCFieldTypes returns object at offset 3");

            Assert.AreEqual(4, VTablesImpl.GetGCFieldOffsets(GCImplementation.GetType(tObj)).Length, "GetGCFieldOffsets returned the correct number of values");

            Assert.AreEqual(new uint[] { 12, 20, 28, 36 }, VTablesImpl.GetGCFieldOffsets(GCImplementation.GetType(tObj)), "GetGCFieldOffsets returns the correct values");

            ClassWithStruct classWithStruct = new ClassWithStruct();

            Assert.AreEqual(3, VTablesImpl.GetGCFieldCount(GCImplementation.GetType(classWithStruct)), "ClassWithStruct has 3 fields tracked by GC");
            types = VTablesImpl.GetGCFieldTypes(GCImplementation.GetType(classWithStruct));
            Assert.AreEqual(((CosmosRuntimeType)typeof(object)).mTypeId, types[0], "GetGCFieldTypes returns object at offset 0");
            Assert.AreEqual(((CosmosRuntimeType)typeof(TestStruct)).mTypeId, types[1], "GetGCFieldTypes returns TestStruct at offset 1");
            Assert.AreEqual(((CosmosRuntimeType)typeof(object)).mTypeId, types[2], "GetGCFieldTypes returns object at offset 2");

            // check that classes have the correct name
            Assert.AreEqual("Int32", ((CosmosRuntimeType)typeof(int)).Name, "Name of Int32 is correctly stored");
            Assert.AreEqual("Object", ((CosmosRuntimeType)typeof(object)).Name, "Name of Object is correctly stored");
        }

        private unsafe void TestGarbageCollectorMethods()
        {
            // allocating + freeing works on gc side
            int allocated = HeapSmall.GetAllocatedObjectCount();
            object c = new object();
            int nowAllocated = HeapSmall.GetAllocatedObjectCount();
            GCImplementation.Free(c);
            int afterFree = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated + 1, nowAllocated, "NewObj causes one object to be allocated");
            Assert.AreEqual(allocated, afterFree, "Free causes one object to be freed again");

            var testString = "asd";
            Assert.AreEqual((byte)RAT.PageType.Empty, (byte)RAT.GetPageType(GCImplementation.GetPointer(testString)), "String is created statically and not managed by GC");

            Assert.IsTrue(Heap.Collect() >= 0, "Running GC Collect first time does not crash and returns non-negative value");
        }

        private unsafe void TestGarbageCollector()
        {
            Heap.Collect();
            TestMethod1();
            int collected = Heap.Collect();
            Assert.AreEqual(2, collected, "GC Collect correctly cleans up locals from method");
            Heap.Collect();
            Counter a = TestMethod2();
            collected = Heap.Collect();
            Assert.AreEqual(3, a.x, "a is not freed and still contains value");
            Assert.AreEqual(1, collected, "GC Collect correctly keeps local element alive");
            Heap.Collect();
            Counter b = TestMethod3();
            collected = Heap.Collect();
            Assert.AreEqual(2, collected, "GC Collect collects unused locals from both methods");
            Heap.Collect();
            TestStruct testStruct = TestMethod4();
            collected = Heap.Collect();
            Assert.AreEqual(0, collected, "Storing elements in struct keeps them referenced");
            Heap.Collect();
            TestMethod5();
            collected = Heap.Collect();
            Assert.AreEqual(2, collected, "Objects once stored in struct are cleaned up");
            Heap.Collect();
            StaticTestClass.A = new object();
            StaticTestClass.B = new TestType();
            StaticTestClass.B.FieldA = 10;
            collected = Heap.Collect();
            Assert.AreEqual(0, collected, "Storing elements in static class keeps them referenced");
        }

        #region Test Methods
        public void TestMethod1()
        {
            object a = new object();
            int x = 1;
            TestType b = new TestType();
        }

        Counter TestMethod2()
        {
            Counter a = new Counter();
            a.x = 3;
            Counter b = new Counter();
            b.x = 4;
            return a;
        }

        Counter TestMethod3()
        {
            Counter x = new Counter();
            return TestMethod2();
        }

        struct TestStruct
        {
            public int a;
            public object b;
            public TestType c;
        }

        TestStruct TestMethod4()
        {
            TestStruct testStruct = new TestStruct();
            testStruct.a = 10;
            testStruct.b = new object();
            testStruct.c = new TestType();
            return testStruct;
        }

        void TestMethod5()
        {
            TestStruct testStruct = new TestStruct();
            testStruct.a = 10;
            testStruct.b = new object();
            testStruct.c = new TestType();
        }

        class ClassWithStruct
        {
            public int a;
            public object b;
            public TestStruct c;
            public object d;
        }

        public void RealMethodsTest()
        {
            Heap.Collect();
            int allocated = HeapSmall.GetAllocatedObjectCount();
            TestMethod6();
            Heap.Collect();
            int nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated, nowAllocated, "Concentating and writing strings does not leak objects");

            allocated = HeapSmall.GetAllocatedObjectCount();
            TestMethod7();
            Heap.Collect();
            nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated, nowAllocated, "TestMethod7 does not leak string objects");
        }

        void TestMethod6()
        {
            Console.WriteLine("Test: " + 3 + " vs " + 5);
        }

        void TestMethod7()
        {
            string o = "";
            for (int i = 0; i < 128; i++)
            {
                o += i + "|" + i * 2;
            }
        }

        #endregion

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                object xString = "a";
                string xString2 = "b";

                Assert.IsTrue(xString.GetType() == typeof(string), "GetType or typeof() works for reference types!");
                Assert.IsTrue(xString.GetType() == xString2.GetType(), "GetType or typeof() works for reference types!");
                Assert.IsTrue(xString is ICloneable, "isinst works for interfaces on reference types!");
                Assert.IsTrue(xString is IEnumerable<char>, "isinst works for generic interfaces on reference types!");
                Assert.IsFalse(xString.GetType().IsValueType, "IsValueType works for reference types!");

                IComparable<int> xNumber = 3;

                Assert.IsTrue(xNumber.GetType() == typeof(int), "GetType or typeof() works for value types!");
                Assert.IsTrue(xNumber is IConvertible, "isinst works for interfaces on value types!");
                Assert.IsTrue(xNumber is IEquatable<int>, "isinst works for generic interfaces on value types!");

                IEnumerable<string> xEnumerable = new List<string>();

                Assert.IsTrue(xEnumerable.GetType() == typeof(List<string>), "GetType or typeof() works for reference types!");
                Assert.IsTrue(xEnumerable is global::System.Collections.IEnumerable, "isinst works for interfaces on generic reference types!");
                Assert.IsTrue(xEnumerable is IList<string>, "isinst works for generic interfaces on generic reference types!");

                B b = new B();
                Assert.IsTrue(b.GetType() == typeof(B), "GetType or typeof() works for custom types!");

                Type baseType = b.GetType().BaseType;
                Type objectType = typeof(A);
                Assert.IsTrue(baseType == objectType, "BaseType works for custom reference types!");
                Assert.IsTrue(b.GetType().BaseType == new B().GetType().BaseType, "BaseType works for custom reference types!");
                Assert.IsTrue(b.GetType().IsSubclassOf(typeof(A)), "IsSubClassOf works for custom reference types!");

                byte xByte = 1;
                Assert.IsTrue(xByte.GetType() == typeof(byte), "GetType or typeof() works for value types!");
                Assert.IsTrue(xByte.GetType().IsSubclassOf(typeof(ValueType)), "IsSubClassOf works for value types!");
                Assert.IsTrue(xByte.GetType().IsValueType, "IsValueType works for value types!");

                Action a = () => { };
                Action<int> a1 = (i) => test++;
                Assert.IsTrue(a != null, "Anonymous type for action is created correctly");
                Assert.IsTrue(a1 != null, "Anonymous type for action<int> is created correctly");

                var c = new { i = 1, n = "Test" };
                Assert.IsTrue(c != null, "Anonymous types are created correctly");
                Assert.IsTrue(c.i == 1 && c.n == "Test", "Anonymous types have correct values");

                TestVTablesImpl();
                TestGarbageCollectorMethods();
                TestGarbageCollector();
                RealMethodsTest();
                TestReflection();

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);

                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.Message);

                TestController.Failed();
            }
        }

        private static void TestReflection()
        {
            Assert.AreEqual("Int32", typeof(int).Name, "Plug for Name of Int32 works");
            Assert.AreEqual("Object", typeof(object).Name, "Plug for Name of Object works");
            string intAQN = typeof(int).AssemblyQualifiedName;
            Assert.IsTrue(intAQN.StartsWith("System.Int32, System.Private.CoreLib"), $"Plug for AssemblyQualifiedName of Int32 works ({intAQN})");
            string objectAQN = typeof(object).AssemblyQualifiedName;
            Assert.IsTrue(objectAQN.StartsWith("System.Object, System.Private.CoreLib"), $"Plug for AssemblyQualifiedName of Object works ({objectAQN})");

            Assert.AreEqual("Int32", Type.GetType("Int32").Name, "GetType works on Int32");
            Assert.AreEqual("Int32", Type.GetType(typeof(int).AssemblyQualifiedName).Name, "GetType works on Int32 using assembly qualified name");
            Assert.AreEqual("Int32", Type.GetType("System.Int32, System.Private.CoreLib").Name, "GetType works on Int32 with shortened assembly qualified name");
            Assert.AreEqual("Int32", Type.GetType("System.Int32").Name, "GetType works on Int32 with shortened assembly qualified name");
        }
    }
}
