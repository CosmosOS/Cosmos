using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class ArrayTests
    {
        public static unsafe void Execute()
        {
            byte[] xEmptyByteArray = Array.Empty<byte>();
            object[] xEmptyObjectArray = Array.Empty<object>();

            Assert.IsTrue(xEmptyByteArray.Length == 0, "Array.Empty<byte> should return an empty array!");
            Assert.IsTrue(xEmptyObjectArray.Length == 0, "Array.Empty<object> should return an empty array!");

            byte[] xByteResult = { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] xByteExpectedResult = { 1, 2, 3, 4, 5, 6, 7, 1 };
            byte[] xByteSource = { 1 };

            Assert.IsTrue(xByteExpectedResult.Length == 8, "Array length is stored correctly");
            Assert.IsTrue(xByteResult.GetLowerBound(0) == 0, "Array.GetLowerBound works");
            //xByteResult.SetValue(1, 0);
            Assert.IsTrue((int)xByteResult.GetValue(0) == 1, "Array.GetValue works for first element");
            Assert.IsTrue((int)xByteResult.GetValue(1) == 2, "Array.GetValue works for element in middle");
            Assert.IsTrue((int)xByteResult.GetValue(7) == 8, "Array.GetValue works at end");

            Array.Copy(xByteSource, 0, xByteResult, 7, 1);

            Assert.IsTrue((xByteResult[7] == xByteExpectedResult[7]), "Array.Copy doesn't work: xResult[7] =  " + (uint)xByteResult[7] + " != " + (uint)xByteExpectedResult[7]);
            Array.Clear(xByteResult, 0, xByteResult.Length);
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(xByteResult[i] == 0, "Array.Clear(byte[], int, int) works");
            }
            xByteResult[1] = 1;
            Assert.IsTrue(xByteResult[1] == 1, "Array.Clear does not break the array");

            xByteResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Assert.AreEqual(2, xByteResult[1], "Setting byte array to new array object works");
            Array.Clear(xByteResult);
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(0, xByteResult[i], "Array.Clear(byte[]) works");
            }

            // Single[] Test
            float[] xSingleResult = { 1.25f, 2.50f, 3.51f, 4.31f, 9.28f, 18.56f };
            float[] xSingleExpectedResult = { 1.25f, 2.598f, 5.39f, 4.31f, 9.28f, 18.56f };
            float[] xSingleSource = { 0.49382f, 1.59034f, 2.598f, 5.39f, 7.48392f, 4.2839f };

            xSingleResult[1] = xSingleSource[2];
            xSingleResult[2] = xSingleSource[3];

            Assert.IsTrue(((xSingleResult[1] + xSingleResult[2]) == (xSingleExpectedResult[1] + xSingleExpectedResult[2])), "Assinging values to single array elements doesn't work: xResult[1] =  " + (uint)xSingleResult[1] + " != " + (uint)xSingleExpectedResult[1] + " and xResult[2] =  " + (uint)xSingleResult[2] + " != " + (uint)xSingleExpectedResult[2]);

            // Double[] Test
            double[] xDoubleResult = { 0.384, 1.5823, 2.5894, 2.9328539, 3.9201, 4.295 };
            double[] xDoubleExpectedResult = { 0.384, 1.5823, 2.5894, 95.32815, 3.9201, 4.295 };
            double[] xDoubleSource = { 95.32815 };

            xDoubleResult[3] = xDoubleSource[0];

            Assert.IsTrue(xDoubleResult[3] == xDoubleExpectedResult[3], "Assinging values to double array elements doesn't work: xResult[1] =  " + (uint)xDoubleResult[3] + " != " + (uint)xDoubleExpectedResult[3]);

            //Test array indexes
            int y = 0;
            int[] x = new int[5] { 1, 2, 3, 4, 5 };
            bool error = false;
            try
            {
                y = x[1];
                y = x[7];
            }
            catch (IndexOutOfRangeException)
            {
                error = true;
            }
            Assert.IsTrue(error && y == 2, "Index out of range exception works correctly for too large positions.");
            error = false;
            try
            {
                y = x[-1];
            }
            catch (IndexOutOfRangeException)
            {
                error = true;
            }
            Assert.IsTrue(error && y == 2, "Index out of range exception works correctly for too small positions.");

            fixed (int* val = x)
            {
                Assert.AreEqual(1, val[0], "Accessing values using pointer works at offset 0");
                Assert.AreEqual(2, val[1], "Accessing values using pointer works at offset 1");
                Assert.AreEqual(4, val[3], "Accessing values using pointer works at offset 3");
                Assert.AreEqual(5, val[4], "Accessing values using pointer works at offset 4");
            }
            int[] arr = new int[] { 0, 1, 2, 3, 4 };

            arr[2] = 6;

            // Try reading the values from the array via a pointer
            fixed (int* val = arr)
            {
                Assert.AreEqual(0, val[0], "Accessing values using pointer works at offset 0");
                Assert.AreEqual(1, val[1], "Accessing values using pointer works at offset 1");
                Assert.AreEqual(3, val[3], "Accessing values using pointer works at offset 3");
                Assert.AreEqual(4, val[4], "Accessing values using pointer works at offset 4");
            }


            char[] charArray = new char[] { 'A', 'a', 'Z', 'z', 'l' };
            charArray[2] = 'k';
            fixed (char* val = charArray)
            {
                Assert.AreEqual('A', val[0], "Accessing values using pointer works at offset 0");
                Assert.AreEqual('a', val[1], "Accessing values using pointer works at offset 1");
                Assert.AreEqual('z', val[3], "Accessing values using pointer works at offset 3");
                Assert.AreEqual('l', val[4], "Accessing values using pointer works at offset 4");
            }

            string[] stringArray = new string[] { "ABC", "BAB", "TAT", "A", "", "LA" };
            Array.Resize(ref stringArray, 3);
            Assert.AreEqual(new string[] { "ABC", "BAB", "TAT" }, stringArray, "Array.Resize works");

            stringArray = new string[10];
            stringArray[0] += "asd";
            Assert.AreEqual(stringArray[0], "asd", "Adding directly to array works");
            
            // Lets test the normal interface methods
            Assert.AreEqual(5, x.Length, "Length of array is correct");
            var objEnumerator = x.GetEnumerator();
            bool moved = objEnumerator.MoveNext();
            Assert.IsTrue(moved, "Enumerator can move into first state");
            int current = (int)objEnumerator.Current;
            Assert.AreEqual(x[0], current, "Getting enumerator directly from array works");
            
            // Lets test the generic interface methods implemented via SZArrayImpl and callvirt + vmtable trickery

            IEnumerator<int> enumerator = (x as IEnumerable<int>).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext(), "Getting enumerator from array as IEnumerable<int> works");
            Assert.AreEqual(x[0], enumerator.Current, "Getting enumerator from array as enumerable<int> works");
            enumerator.MoveNext();
            Assert.AreEqual(x[1], enumerator.Current, "Getting enumerator from array as enumerable<in> works after second move");

            IList<int> list = x;
            //Assert.AreEqual(0, list.IndexOf(1), "Calling IndexOf on array as IList<int> works"); - broken until .Net 5.0 changes fixed
            //Assert.AreEqual(1, list.IndexOf(2), "Calling IndexOf on array as IList<int> works");
            Assert.AreEqual(1, list[0], "Getting item from array as IList<int> works");
            Assert.AreEqual(3, list[2], "Getting item from array as IList<int> works");

            ICollection<int> collection = x;
            Assert.AreEqual(5, collection.Count, "Getting Count from array as ICollection<int> works");
            Assert.IsTrue(collection.IsReadOnly, "Getting IsReadOnly from array as ICollection<int> works");
            //Assert.IsTrue(collection.Contains(2), "Calling Contains on array as ICollection<int> works"); - broken until .Net 5.0 changes fixed
            //Assert.IsFalse(collection.Contains(6), "Calling Contains on array as ICollection<int> works");
            int[] newArray = new int[5];
            collection.CopyTo(newArray, 0);
            bool areEqual = true;
            for (int i = 0; i < x.Length; i++)
            {
                if(x[i] != newArray[i])
                {
                    areEqual = false;
                    break;
                }
            }
            Assert.IsTrue(areEqual, "Calling CopyTo on array as ICollection<int> works");

            IReadOnlyList<int> readOnlyList = x;
            Assert.AreEqual(x[0], readOnlyList[0], "Getting item from array as IReadOnlyList<int> works");
            Assert.AreEqual(x[3], readOnlyList[3], "Getting item from array as IReadOnlyList<int> works");

            IReadOnlyCollection<int> readOnlyCollection = x;
            Assert.AreEqual(5, readOnlyCollection.Count, "Getting Count from array as IReadOnlyCollection<int> works");
        }
    }
}
