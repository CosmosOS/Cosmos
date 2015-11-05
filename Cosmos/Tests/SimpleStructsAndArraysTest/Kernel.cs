﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace SimpleStructsAndArraysTest
{
    public class Kernel: Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
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

        private static T GetValue<T>(T[] arr, int index)
        {
            return arr[index];
        }

        private static void TestStep1()
        {
            //var xItem = xQueue.Dequeue();
            //Console.Write("Char: ");
            //Console.WriteLine(xResult.KeyChar);
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
            Console.Write("A: ");
            Console.WriteLine(xItem.A);
            Assert.IsTrue(xItem.B == 2, "xItem.B == 2");
            Console.Write("B: ");
            Console.WriteLine(xItem.B);
            Assert.IsTrue(xItem.C == 3, "xItem.C == 3");
            Console.Write("C: ");
            Console.WriteLine(xItem.C);
            Assert.IsTrue(xItem.D == 4, "xItem.D == 4");
            Console.Write("D: ");
            Console.WriteLine(xItem.D);
            Assert.IsTrue(xItem.E == 5, "xItem.E == 5");
            Console.Write("E: ");
            Console.WriteLine(xItem.E);

            //xItem = new MyStruct(6, 7, 8, 9, 10);

            Console.WriteLine("Next: ");
            //xItem = xQueue.Dequeue();
            //Console.Write("Char: ");
            //Console.WriteLine(xResult.KeyChar);

            //var xArray = new MyStruct[0];
            //xArray[0] = new MyStruct(1, 2, 3, 4, 5);

            var xItem2 = GetValue(xArray, 0);
            Assert.IsTrue(xItem2.A == 1, "xItem2.A == 1");
            Console.Write("A: ");
            Console.WriteLine(xItem2.A);
            Assert.IsTrue(xItem2.B == 2, "xItem2.B == 2");
            Console.Write("B: ");
            Console.WriteLine(xItem2.B);
            Assert.IsTrue(xItem2.C == 3, "xItem2.C == 3");
            Console.Write("C: ");
            Console.WriteLine(xItem2.C);
            Assert.IsTrue(xItem2.D == 4, "xItem2.D == 4");
            Console.Write("D: ");
            Console.WriteLine(xItem2.D);
            Assert.IsTrue(xItem2.E == 5, "xItem2.E == 5");
            Console.Write("E: ");
            Console.WriteLine(xItem2.E);
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
                this._items = new T[0];
            }
            public void Add(T item)
            {

                if (this._size == this._items.Length)
                {
                    this.EnsureCapacity(this._size + 1);
                }
                T[] arg_36_0 = this._items;
                int size = this._size;
                this._size = size + 1;
                arg_36_0[size] = item;
                this._version++;
            }

            private void EnsureCapacity(int min)
            {
                if (this._items.Length < min)
                {
                    int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
                    if (num > 2146435071)
                    {
                        num = 2146435071;
                    }
                    if (num < min)
                    {
                        num = min;
                    }
                    this.Capacity = num;
                }
            }

            public int Capacity
            {
                get
                {
                    return this._items.Length;
                }
                set
                {
                    if (value < this._size)
                    {
                        throw new Exception("Capacity is smaller than size!");
                    }
                    if (value != this._items.Length)
                    {
                        if (value > 0)
                        {
                            T[] array = new T[value];
                            if (this._size > 0)
                            {
                                Array.Copy(this._items, 0, array, 0, this._size);
                            }
                            this._items = array;
                            return;
                        }
                        this._items = new T[0];
                    }
                }
            }

            public T this[int index]
            {
                get
                {
                    Assert.IsTrue(index == ExpectedIndex, "index == " + ExpectedIndex);
                    if (index >= this._size)
                    {
                        throw new Exception("Out of range!");
                    }
                    return this._items[index];
                }
            }

            public static int ExpectedIndex;

        }

        protected static void TestOurList()
        {
            Assert.IsTrue(true, "Start of test");
            var xListClasses = new OurList<KVPClass>();
            var xListStructs = new OurList<KVPStruct>();

            xListClasses.Add(new KVPClass {Key = 1, Value = 2});
            xListClasses.Add(new KVPClass {Key = 2, Value = 5});

            OurList<KVPClass>.ExpectedIndex = 0;
            var xListItem = xListClasses[0];
            Assert.AreEqual(1, xListItem.Key, "xListClasses[0].Key == 1");
            Assert.AreEqual(2, xListItem.Value, "xListClasses[0].Value == 2");
            OurList<KVPClass>.ExpectedIndex = 1;
            xListItem = xListClasses[1];
            Assert.AreEqual(2, xListItem.Key,   "xListClasses[1].Key == 2");
            Assert.AreEqual(5, xListItem.Value, "xListClasses[1].Value == 5");

            xListStructs.Add(new KVPStruct {Key = 1, Value = 2});
            xListStructs.Add(new KVPStruct {Key = 2, Value = 5});

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

        protected override void Run()
        {
            TestStep1();
            TestOurList();
            Assert.IsTrue(true, "After TestOurList");
            TestStandardList();
            Assert.IsTrue(true, "After TestStandardList");
            TestController.Completed();
        }
    }
}
