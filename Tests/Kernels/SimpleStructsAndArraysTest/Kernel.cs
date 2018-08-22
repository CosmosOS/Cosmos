using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace SimpleStructsAndArraysTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
        }

        [StructLayout(LayoutKind.Explicit, Size = 10)]
        private struct MyStruct
        {
            public MyStruct(short a, short b, short c, short d, short e)
            {
                A = a;
                B = b;
                C = c;
                D = d;
                E = e;
            }

            [FieldOffset(0)]
            public short A;

            [FieldOffset(2)]
            public short B;

            [FieldOffset(4)]
            public short C;

            [FieldOffset(6)]
            public short D;

            [FieldOffset(8)]
            public short E;
        }

        private static void TestStep1()
        {
            var xItem = new MyStruct
            {
                A = 1,
                B = 2,
                C = 3,
                D = 4,
                E = 5
            };

            var xArray = new MyStruct[1];
            xArray[0] = xItem;
            //xArray[0] = new MyStruct(1, 2, 3, 4, 5);

            xItem = xArray[0];

            Assert.IsTrue(xItem.A == 1, "xItem.A == 1");
            Assert.IsTrue(xItem.B == 2, "xItem.B == 2");
            Assert.IsTrue(xItem.C == 3, "xItem.C == 3");
            Assert.IsTrue(xItem.D == 4, "xItem.D == 4");
            Assert.IsTrue(xItem.E == 5, "xItem.E == 5");

            //xItem = new MyStruct(6, 7, 8, 9, 10);

            //xItem = xQueue.Dequeue();
            //Console.Write("Char: ");

            //var xArray = new MyStruct[0];
            //xArray[0] = new MyStruct(1, 2, 3, 4, 5);

            var xItem2 = xArray[0];

            Assert.IsTrue(xItem2.A == 1, "xItem2.A == 1");
            Assert.IsTrue(xItem2.B == 2, "xItem2.B == 2");
            Assert.IsTrue(xItem2.C == 3, "xItem2.C == 3");
            Assert.IsTrue(xItem2.D == 4, "xItem2.D == 4");
            Assert.IsTrue(xItem2.E == 5, "xItem2.E == 5");
        }

        private class KVPClass
        {
            public int Key;
            public int Value;
        }

        private struct KVPStruct
        {
            public int Key;
            public int Value;
        }

        private class OurList<T>
        {
            private int _size;
            private int _version;
            private T[] _items;
            private object _syncRoot;

            public OurList()
            {
                _items = new T[0];
            }
            public void Add(T item)
            {

                if (_size == _items.Length)
                {
                    EnsureCapacity(_size + 1);
                }
                T[] arg_36_0 = _items;
                int size = _size;
                _size = size + 1;
                arg_36_0[size] = item;
                _version++;
            }

            private void EnsureCapacity(int min)
            {
                if (_items.Length < min)
                {
                    int num = (this._items.Length == 0) ? 4 : (_items.Length * 2);
                    if (num > 2146435071)
                    {
                        num = 2146435071;
                    }
                    if (num < min)
                    {
                        num = min;
                    }
                    Capacity = num;
                }
            }

            public int Capacity
            {
                get
                {
                    return _items.Length;
                }
                set
                {
                    if (value < _size)
                    {
                        throw new Exception("Capacity is smaller than size!");
                    }
                    if (value != _items.Length)
                    {
                        if (value > 0)
                        {
                            T[] array = new T[value];
                            if (_size > 0)
                            {
                                Array.Copy(_items, 0, array, 0, _size);
                            }
                            _items = array;
                            return;
                        }
                        _items = new T[0];
                    }
                }
            }

            public T this[int index]
            {
                get
                {
                    Assert.IsTrue(index == ExpectedIndex, "index == " + ExpectedIndex);
                    if (index >= _size)
                    {
                        throw new Exception("Out of range!");
                    }
                    return _items[index];
                }
            }

            public static int ExpectedIndex;

        }

        protected static void TestOurList()
        {
            Assert.IsTrue(true, "Start of test");
            var xListClasses = new OurList<KVPClass>();
            var xListStructs = new OurList<KVPStruct>();

            xListClasses.Add(new KVPClass { Key = 1, Value = 2 });
            xListClasses.Add(new KVPClass { Key = 2, Value = 5 });

            OurList<KVPClass>.ExpectedIndex = 0;
            var xListItem = xListClasses[0];
            Assert.AreEqual(1, xListItem.Key, "xListClasses[0].Key == 1");
            Assert.AreEqual(2, xListItem.Value, "xListClasses[0].Value == 2");
            OurList<KVPClass>.ExpectedIndex = 1;
            xListItem = xListClasses[1];
            Assert.AreEqual(2, xListItem.Key, "xListClasses[1].Key == 2");
            Assert.AreEqual(5, xListItem.Value, "xListClasses[1].Value == 5");

            xListStructs.Add(new KVPStruct { Key = 1, Value = 2 });
            xListStructs.Add(new KVPStruct { Key = 2, Value = 5 });

            OurList<KVPStruct>.ExpectedIndex = 0;
            var xStructItem = xListStructs[0];
            Assert.AreEqual(1, xStructItem.Key, "xListStructs[0].Key == 1");
            Assert.AreEqual(2, xStructItem.Value, "xListStructs[0].Value == 2");
            OurList<KVPStruct>.ExpectedIndex = 1;
            xStructItem = xListStructs[1];
            Assert.AreEqual(2, xStructItem.Key, "xListStructs[1].Key == 2");
            Assert.AreEqual(5, xStructItem.Value, "xListStructs[1].Value == 5");
        }

        protected static void TestStandardList()
        {
            Assert.IsTrue(true, "Start of test");
            var xListClasses = new List<KVPClass>();
            var xListStructs = new List<KVPStruct>();

            xListClasses.Add(new KVPClass { Key = 1, Value = 2 });
            xListClasses.Add(new KVPClass { Key = 2, Value = 5 });

            var xListItem = xListClasses[0];
            Assert.AreEqual(1, xListItem.Key, "xListClasses[0].Key == 1");
            Assert.AreEqual(2, xListItem.Value, "xListClasses[0].Value == 2");
            xListItem = xListClasses[1];
            Assert.AreEqual(2, xListItem.Key, "xListClasses[1].Key == 2");
            Assert.AreEqual(5, xListItem.Value, "xListClasses[1].Value == 5");

            xListStructs.Add(new KVPStruct { Key = 1, Value = 2 });
            xListStructs.Add(new KVPStruct { Key = 2, Value = 5 });

            var xStructItem = xListStructs[0];
            Assert.AreEqual(1, xStructItem.Key, "xListStructs[0].Key == 1");
            Assert.AreEqual(2, xStructItem.Value, "xListStructs[0].Value == 2");
            xStructItem = xListStructs[1];
            Assert.AreEqual(2, xStructItem.Key, "xListStructs[1].Key == 2");
            Assert.AreEqual(5, xStructItem.Value, "xListStructs[1].Value == 5");
        }

        //protected static void TestMultidimensionalArray()
        //{
        //    var xTestMultidimensionalArray = new int[2, 2];
        //    xTestMultidimensionalArray[0, 0] = 1;
        //    xTestMultidimensionalArray[0, 1] = 2;
        //    xTestMultidimensionalArray[1, 0] = 3;
        //    xTestMultidimensionalArray[1, 1] = 4;
        //    Assert.IsTrue(xTestMultidimensionalArray.Length == 4, "Size of array is 4.");
        //    Assert.IsTrue(xTestMultidimensionalArray[0, 0] == 1, "Index [0, 0] == 1");
        //    Assert.IsTrue(xTestMultidimensionalArray[0, 1] == 2, "Index [0, 1] == 2");
        //    Assert.IsTrue(xTestMultidimensionalArray[1, 0] == 3, "Index [1, 0] == 3");
        //    Assert.IsTrue(xTestMultidimensionalArray[1, 1] == 4, "Index [1, 1] == 4");
        //}

        protected override void Run()
        {
            TestStep1();
            TestOurList();
            Assert.IsTrue(true, "After TestOurList");
            TestStandardList();
            Assert.IsTrue(true, "After TestStandardList");
            //TestMultidimensionalArray();
            //Assert.IsTrue(true, "After TestMultidimensionalArray");
            ConstrainedTest.MutateStructTest();
            Assert.IsTrue(true, "After MutateTestStruct");

            TestController.Completed();
        }
    }
}
