using System;
using System.Collections;
using System.Collections.Generic;
using Sys = Cosmos.System;

using Cosmos.TestRunner;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU;
using Cosmos.Core.Memory;

namespace Cosmos.Compiler.Tests.TypeSystem
{
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
        static int test = 0;
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

            Assert.AreEqual(new uint[] { 12, 20, 28, 36}, VTablesImpl.GetGCFieldOffsets(GCImplementation.GetType(tObj)), "GetGCFieldOffsets returns the correct values");
        }

        static object staticObject;
        private unsafe void TestGarbageCollector()
        {
            // allocating + freeing works on gc side
            int allocated = HeapSmall.GetAllocatedObjectCount();
            object c = new object();
            int nowAllocated = HeapSmall.GetAllocatedObjectCount();
            GCImplementation.Free(c);
            int afterFree = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated + 1, nowAllocated, "NewObj causes one object to be allocated");
            Assert.AreEqual(allocated, afterFree, "Free causes one object to be freed again");

            // stloc correctly updates ref counts
            object a = new object();
            var test = new TestType();
            test.FieldB = a;
            test.FieldC = "asd";
            uint* aPtr = GCImplementation.GetPointer(test);
            Assert.AreEqual(1, Heap.GetRefCount(aPtr), "Object has one ref count");
            Assert.AreEqual(2, Heap.GetRefCount(GCImplementation.GetPointer(test.FieldB)), "object a has 2 ref count currently");
            test.FieldB = new object();
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(test.FieldB)), "new object has 1 ref count currently");
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(a)), "object a has 1 ref count currently");

            // Manual ref counting works
            Assert.AreEqual(RAT.PageType.None, RAT.GetPageType(GCImplementation.GetPointer(test.FieldC)), "String is created statically and not managed by GC");
            Heap.IncRefCount(aPtr);
            Assert.AreEqual(2, Heap.GetRefCount(aPtr), "IncRefCount works");
            Heap.DecRefCount(aPtr, 0);
            Assert.AreEqual(1, Heap.GetRefCount(aPtr), "DecRefCount works");
            allocated = HeapSmall.GetAllocatedObjectCount();
            Heap.DecRefCount(aPtr, 0);
            afterFree = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated - 2, afterFree, "DecRefCount triggers free, which works recursivly");

            // Cleaning up locals test
            allocated = HeapSmall.GetAllocatedObjectCount();
            TestMethod1();
            nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated, nowAllocated, "All local objects in a method are cleaned up");

            // Return value test
            allocated = HeapSmall.GetAllocatedObjectCount();
            var counter = TestMethod2();
            nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated + 1, nowAllocated, "Method with return value only keeps return value alive");
            Assert.AreEqual(counter.x, 3, "Returned object is not freed");
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(counter)), "Return value has 1 ref from local variable");
            allocated = HeapSmall.GetAllocatedObjectCount();
            counter = TestMethod3();
            nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated, nowAllocated, "Recursive methof calling with return value only keeps return value alive");
            Assert.AreEqual(counter.x, 3, "Returned object is not freed");
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(counter)), "Return value has 1 ref from local variable");

            // Array GC Test
            allocated = HeapSmall.GetAllocatedObjectCount();
            Counter counter1 = new Counter();
            Counter[] counters = new Counter[3];
            counters[0] = counter1;
            counters[1] = new Counter();
            nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated + 3, nowAllocated, "Array test allocates 3 elements");
            Assert.AreEqual(2, Heap.GetRefCount(GCImplementation.GetPointer(counter1)), "Storing element in array increases refcount");
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(counters[1])), "Storing element in array increases refcount");
            allocated = HeapSmall.GetAllocatedObjectCount();
            Heap.DecRefCount(GCImplementation.GetPointer(counters), 0);
            nowAllocated = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated - 2, nowAllocated, "Decreasing ref count of array to zero frees it and propagates to elements");
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(counter1)), "Freeing array decreases ref count by 1 of elements");

            // stsfld GC test
            staticObject = new object();
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(staticObject)), "Storing object in static variable increases ref count by 1");
            object x = staticObject;
            staticObject = new object();
            Assert.AreEqual(1, Heap.GetRefCount(GCImplementation.GetPointer(staticObject)), "Replacing object in static variable decreases ref count by 1 of old object");
        }

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


        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                //object xString = "a";
                //string xString2 = "b";

                //Assert.IsTrue(xString.GetType() == typeof(string), "GetType or typeof() works for reference types!");
                //Assert.IsTrue(xString.GetType() == xString2.GetType(), "GetType or typeof() works for reference types!");
                //Assert.IsTrue(xString is ICloneable, "isinst works for interfaces on reference types!");
                //Assert.IsTrue(xString is IEnumerable<char>, "isinst works for generic interfaces on reference types!");
                //Assert.IsFalse(xString.GetType().IsValueType, "IsValueType works for reference types!");

                //IComparable<int> xNumber = 3;

                //Assert.IsTrue(xNumber.GetType() == typeof(int), "GetType or typeof() works for value types!");
                //Assert.IsTrue(xNumber is IConvertible, "isinst works for interfaces on value types!");
                //Assert.IsTrue(xNumber is IEquatable<int>, "isinst works for generic interfaces on value types!");

                //IEnumerable<string> xEnumerable = new List<string>();

                //Assert.IsTrue(xEnumerable.GetType() == typeof(List<string>), "GetType or typeof() works for reference types!");
                //Assert.IsTrue(xEnumerable is IEnumerable, "isinst works for interfaces on generic reference types!");
                //Assert.IsTrue(xEnumerable is IList<string>, "isinst works for generic interfaces on generic reference types!");

                //B b = new B();
                //Assert.IsTrue(b.GetType() == typeof(B), "GetType or typeof() works for custom types!");

                //Type baseType = b.GetType().BaseType;
                //Type objectType = typeof(A);
                //Assert.IsTrue(baseType == objectType, "BaseType works for custom reference types!");
                //Assert.IsTrue(b.GetType().BaseType == new B().GetType().BaseType, "BaseType works for custom reference types!");
                //Assert.IsTrue(b.GetType().IsSubclassOf(typeof(A)), "IsSubClassOf works for custom reference types!");

                //byte xByte = 1;
                //Assert.IsTrue(xByte.GetType() == typeof(byte), "GetType or typeof() works for value types!");
                //Assert.IsTrue(xByte.GetType().IsSubclassOf(typeof(ValueType)), "IsSubClassOf works for value types!");
                //Assert.IsTrue(xByte.GetType().IsValueType, "IsValueType works for value types!");

                //Action a = () => { };
                //Action<int> a1 = (i) => test++;
                //Assert.IsTrue(a != null , "Anonymous type for action is created correctly");
                //Assert.IsTrue(a1 != null, "Anonymous type for action<int> is created correctly");

                //var c = new { i = 1, n = "Test" };
                //Assert.IsTrue(c != null, "Anonymous types are created correctly");
                //Assert.IsTrue(c.i == 1 && c.n == "Test", "Anonymous types have correct values");

                TestVTablesImpl();
                TestGarbageCollector();

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
    }
}
