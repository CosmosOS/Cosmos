using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace SimpleStructsAndArraysTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        [StructLayout(LayoutKind.Explicit, Size=10)]
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

        private static int Add(int a, int b)
        {
            return a + b;
        }

        protected override void Run()
        {
            Add(1, 2);
            //var xQueue = new Queue<MyStruct>(8);
            //xQueue.Enqueue(new MyStruct(1, 2, 3, 4, 5));
            //xQueue.Enqueue(new MyStruct(6, 7, 8, 9, 10));

            var xTest = 3 % 8;
            Console.Write("Test: ");
            Console.WriteLine(xTest.ToString());

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

            TestController.Completed();
        }
    }
}
