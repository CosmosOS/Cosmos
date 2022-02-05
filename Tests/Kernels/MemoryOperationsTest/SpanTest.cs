using Cosmos.TestRunner;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryOperationsTest
{
    public class SpanTest
    {
        static ref byte GetValue(byte[] array)
        {
            return ref array[3];
        }

        public static void RefTests()
        {
            byte[] data = new byte[] {0, 1, 2, 3, 4 };
            ref byte b = ref data[2];
            Assert.AreEqual(2, b, "Retrieving from ref local variables works");
            b += 2;
            Assert.AreEqual(4, b, "Adding to ref local variables works");
            ref byte b2 = ref data[2];
            b = 6;
            Assert.AreEqual(6, b, "Setting value to ref local variables works");
            Assert.AreEqual(6, b2, "Setting value to indirect ref local variables works");
            Assert.AreEqual(3, GetValue(data), "Setting value to indirect ref local variables works");
            ref byte b3 = ref GetValue(data);
            data[3] *= 2;
            Assert.AreEqual(6, b3, "Setting value to indirect ref local variables works");

            ref byte a = ref data[0];
            Assert.AreEqual(0, a, "Correctly intialises ref type");
            a = ref Unsafe.Add(ref a, 1);
            Assert.AreEqual(1, a, "Unsafe.Add works for ref types");
            a = ref Unsafe.Add(ref a, 3);
            Assert.AreEqual(4, a, "Unsafe.Add works for ref types");

            int[] iData = new int[] { 0, 100, 200, 300, 400, 500, 600 };
            ref int iPointer = ref iData[2];
            Assert.AreEqual(200, iPointer, "Using ref types on int works as well");
            iPointer = ref Unsafe.Add(ref iPointer, 4);
            Assert.AreEqual(600, iPointer, "Unsafe.Add works for ref int");
            IntPtr intPtr = new IntPtr(2);
            iPointer = ref Unsafe.Subtract(ref iPointer, intPtr);
            Assert.AreEqual(400, iPointer, "Unsafe.Subtract works for ref int and IntPtr");
        }

        public static void MemoryMarshalTest()
        {
            var arr = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            ref byte a = ref MemoryMarshal.GetArrayDataReference(arr);
            Assert.AreEqual(0, a, "MemoryMarshal returns correct array data reference");
            arr[0] = 10;
            Assert.AreEqual(10, a, "The reference is updated");
            Assert.AreEqual(Unsafe.Add(ref a, 1), 1, "Unsafe.Add 1 works for array ref");
            Assert.AreEqual(Unsafe.Add(ref a, 5), 5, "Unsafe.Add 5 works for array ref");
            
        }

        public static void ImplicitSpanTest(ReadOnlySpan<char> aSpan)
        {
            Assert.AreEqual(aSpan[0], 'H', "Implicit array to span has correct value at index 0");
            Assert.AreEqual(aSpan[3], 'p', "Implicit array to span has correct value at index 3");
        }

        public static void Execute()
        {
            RefTests();
            MemoryMarshalTest();
            var arr = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Span<byte> span = arr;
            Assert.IsTrue(span != null, "Can create span from array");
            Assert.AreEqual(10, span.Length, "Span has correct length");
            Assert.AreEqual(2, arr[2], "Array has correct values");
            Assert.AreEqual(2, span[2], "Span has correct values");
            Assert.AreEqual(8, arr[8], "Array has correct values");
            Assert.AreEqual(8, span[8], "Span has correct values");
            span[3] = 20;
            Assert.AreEqual(20, span[3], "Span can change value");
            Assert.AreEqual(20, arr[3], "Array can change value");

            Span<byte> slice = span.Slice(start: 5, length: 3);
            Assert.IsTrue(slice != null, "Spans can be sliced");
            Assert.AreEqual(3, slice.Length, "Sliced Span has correct length");

            var intArr = new int[] { 0, 10, 100 };
            Span<int> intSpan = new(intArr);
            Assert.AreEqual(0, intSpan[0], "Int Span get_Item works at index 0");
            Assert.AreEqual(100, intSpan[2], "Int Span get_Item works at index 2");
            Assert.IsFalse(intSpan.IsEmpty, "Int Span is not empty");

            var charArr = new char[] { 'H', 'e', 'l', 'p' };
            ImplicitSpanTest(charArr);
        }
    }
}
