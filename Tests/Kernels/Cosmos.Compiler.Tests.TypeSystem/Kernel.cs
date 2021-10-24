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

            Assert.AreEqual(4, VTablesImpl.GetGCFieldCount(GCImplementation.GetType(tObj)), "TestType has 3 fields tracked by GC");

            var types = VTablesImpl.GetGCFieldTypes(GCImplementation.GetType(tObj));
            Assert.AreEqual(4, types.Length, "GetGCFieldTypes returns correct number of values");
            Assert.AreEqual(((CosmosRuntimeType)typeof(object)).mTypeId, types[0], "GetGCFieldTypes returns object at offset 0");
            Assert.AreEqual(((CosmosRuntimeType)typeof(string)).mTypeId, types[1], "GetGCFieldTypes returns string at offset 1");
            Assert.AreEqual(((CosmosRuntimeType)typeof(List<int>)).mTypeId, types[2], "GetGCFieldTypes returns List<int> at offset 2");
            Assert.AreEqual(((CosmosRuntimeType)typeof(object)).mTypeId, types[3], "GetGCFieldTypes returns object at offset 3");

            Assert.AreEqual(4, VTablesImpl.GetGCFieldOffsets(GCImplementation.GetType(tObj)).Length, "GetGCFieldOffsets returned the correct number of values");

            Assert.AreEqual(new uint[] { 4, 12, 20, 28}, VTablesImpl.GetGCFieldOffsets(GCImplementation.GetType(tObj)), "GetGCFieldOffsets returns the correct values");
        }

        private void TestGarbageCollector()
        {
            int allocated = HeapSmall.GetAllocatedObjectCount();
            object a = new object();
            int nowAllocated = HeapSmall.GetAllocatedObjectCount();
            GCImplementation.Free(a);
            int afterFree = HeapSmall.GetAllocatedObjectCount();
            Assert.AreEqual(allocated + 1, nowAllocated, "NewObj causes one object to be allocated");
            Assert.AreEqual(allocated, afterFree, "Free causes one object to be freed again");
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
