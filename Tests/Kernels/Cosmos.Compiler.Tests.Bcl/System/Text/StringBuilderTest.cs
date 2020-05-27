using System;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class StringBuilderTest
    {
        public static void Execute()
        {
            StringBuilder sb = new StringBuilder();

            Assert.IsTrue(sb.Capacity == 16, "StringBuilder.Capacity is wrong");

            Assert.IsTrue(sb.MaxCapacity == Int32.MaxValue, "StringBuilder.MaxCapacity is wrong");

            Assert.IsTrue(sb.Length == 0, "StringBuilder.MaxCapacity is wrong");

            sb.Append("This ");
            sb.Append("is ");
            sb.Append("a test");

            /* Now sb.Lenght should be 14 (the leng of 'This is a test') */
            Assert.IsTrue(sb.Length == 14, "After Append StringBuilder.Lenght is wrong");

            Assert.IsTrue(sb.ToString() == "This is a test", "StringBuilder.Append() does not work");

            sb.Append("...again");

            Assert.IsTrue(sb.Capacity == 32, "StringBuilder.Capacity is wrong (not doubled!)");

            Assert.IsTrue(sb.Length == 22, "After Append StringBuilder.Lenght is wrong");

            Assert.IsTrue(sb.ToString() == "This is a test...again", "StringBuilder.Append() again does not work");

            sb.Clear();

            // Capacity does not change after Clear
            Assert.IsTrue(sb.Capacity == 32, "StringBuilder.Capacity after Clear is wrong)");

            // ... but Lenght should be 0 again
            Assert.IsTrue(sb.Length == 0, "After Clear StringBuilder.Lenght is wrong");

            Assert.IsTrue(sb.ToString() == String.Empty, "StringBuilder.ToString() is not empty after Clear()");

            /* This is required NumberBuffer to work it will be 90% managed code in Net Core 2.1 so better to wait */
#if false
            int var1 = 111;
            float var2 = 2.22F;
            string var3 = "abcd";
            object[] var4 = { 3, 4.4, 'X' };

            sb.AppendFormat($"1) {var1}");
            Assert.IsTrue(sb.ToString() == "1) 111", "StringBuilder.AppendFormat() with 1 arg does not work");
            sb.Length = 0; // Same of sb.Clear() maybe faster

            sb.AppendFormat("2) {0}, {1}", var1, var2);
            Assert.IsTrue(sb.ToString() == "2) 111, 2.22", "StringBuilder.AppendFormat() with 2 args does not work");
            sb.Length = 0; // Same of sb.Clear() maybe faster

            sb.AppendFormat("3) {0}, {1}, {2}", var1, var2, var3);
            Assert.IsTrue(sb.ToString() == "111, 2.22, abcd", "StringBuilder.AppendFormat() with 3 args does not work");
            sb.Length = 0; // Same of sb.Clear() maybe faster

            sb.AppendFormat("4) {0}, {1}, {2}", var4);
            Assert.IsTrue(sb.ToString() == "111, 2.22, abcd", "StringBuilder.AppendFormat() with arg array does not work");
            sb.Length = 0; // Same of sb.Clear() maybe faster
#endif

            sb.Append("This is a test");
            sb.Insert("This is".Length, " indeed");

            Assert.IsTrue(sb.ToString() == "This is indeed a test", $"StringBuilder.Insert() does not work: {sb.ToString()}");

            string initialValue = "--[]--";

            sb = new StringBuilder(initialValue);

            Assert.IsTrue(sb.ToString() == initialValue, "Stribuilder(string) does not work");

            string xyz = "xyz";
            char[] abc = { 'a', 'b', 'c' };
            char star = '*';
            Object obj = 0;

            bool xBool = true;
            byte xByte = 1;
            short xInt16 = 2;
            int xInt32 = 3;
            long xInt64 = 4;
            //Decimal xDecimal = 5;
            float xSingle = 6.6F;
            double xDouble = 7.7;

            // The following types are not CLS-compliant.
            ushort xUInt16 = 8;
            uint xUInt32 = 9;
            ulong xUInt64 = 10;
            sbyte xSByte = -11;

            sb.Insert(3, xyz, 2);
            Assert.IsTrue(sb.ToString() == "--[xyzxyz]--", "Insert #1 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, star);
            Assert.IsTrue(sb.ToString() == "--[*]--", "Insert #2 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, abc);
            Assert.IsTrue(sb.ToString() == "--[abc]--", "Insert #3 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, abc, 1, 2);
            Assert.IsTrue(sb.ToString() == "--[bc]--", "Insert #4 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xBool);
            Assert.IsTrue(sb.ToString() == "--[True]--", "Insert #5 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, obj);
            Assert.IsTrue(sb.ToString() == "--[0]--", "Insert #6 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xByte);
            Assert.IsTrue(sb.ToString() == "--[1]--", "Insert #7 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xInt16);
            Assert.IsTrue(sb.ToString() == "--[2]--", "Insert #8 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xInt32);
            Assert.IsTrue(sb.ToString() == "--[3]--", "Insert #9 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xInt64);
            Assert.IsTrue(sb.ToString() == "--[4]--", "Insert #10 does not work");
            sb = new StringBuilder(initialValue);

            // Decimal has a totally managed implementation in .Net Core 2.1, let's wait for that
#if false
            sb.Insert(3, xDecimal);
            Assert.IsTrue(sb.ToString() == "--[5]--", "Insert #11 does not work");
            sb = new StringBuilder(initialValue);
#endif
            sb.Insert(3, xSingle);
            Assert.IsTrue(sb.ToString() == "--[6.6]--", "Insert #11 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xDouble);
            Assert.IsTrue(sb.ToString() == "--[7.7]--", "Insert #12 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xUInt16);
            Assert.IsTrue(sb.ToString() == "--[8]--", "Insert #13 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xUInt32);
            Assert.IsTrue(sb.ToString() == "--[9]--", "Insert #14 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xUInt64);
            Assert.IsTrue(sb.ToString() == "--[10]--", "Insert #15 does not work");
            sb = new StringBuilder(initialValue);

            sb.Insert(3, xSByte);
            Assert.IsTrue(sb.ToString() == "--[-11]--", "Insert #16 does not work");

            sb.Clear();

            sb = new StringBuilder("The quick brown fox jumps over the lazy dog.");

            sb.Remove(10, 6); // Remove "brown "

            Assert.IsTrue(sb.ToString() == "The quick fox jumps over the lazy dog.", "Remove does not work");

            sb = new StringBuilder("The quick br!wn d#g jumps #ver the lazy cat.");

            sb.Replace('#', '!', 15, 29);        // Some '#' -> '!'
            Assert.IsTrue(sb.ToString() == "The quick br!wn d!g jumps !ver the lazy cat.", "Replace #1 does not work");

            sb.Replace('!', 'o'); // All '!' -> 'o'
            Assert.IsTrue(sb.ToString() == "The quick brown dog jumps over the lazy cat.", "Replace #2 does not work");

            sb.Replace("cat", "dog");            // All "cat" -> "dog"
            Assert.IsTrue(sb.ToString() == "The quick brown dog jumps over the lazy dog.", "Replace #3 does not work");

            sb.Replace("dog", "fox", 15, 20);    // Some "dog" -> "fox"
            Assert.IsTrue(sb.ToString() == "The quick brown fox jumps over the lazy dog.", "Replace #4 does not work");

            sb = new StringBuilder("This is a simple sentence.");

            Assert.IsTrue(sb[2] == 'i', "Index get operator does not work");

            sb[2] = '1';

            Assert.IsTrue(sb.ToString() == "Th1s is a simple sentence.", "Index set operator does not work");
        }
    }
}
