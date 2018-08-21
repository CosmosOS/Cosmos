using System;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class StringTest
    {
        public static void Execute()
        {
            string xTestStr = "Test";
            int xExpectedLength = 4;
            int xLength = xTestStr.Length;
            Assert.IsTrue(xLength == xExpectedLength, "String.Length is not returning the correct value.");

            Assert.IsTrue(("a" + "b") == "ab", "concatting 2 string using + doesn't work");
            Assert.IsTrue(("a" + 'b') == "ab", "concatting 1 string and 1 character doesn't work");

            string a = "a";
            string b = "b";
            string result = a + b;
            Assert.IsTrue(result == "ab", "concatting 2 string (not optimizable) using + doesn't work"); // Fails

            char x = 'a';
            string y = "a";
            Assert.IsTrue(x.ToString() == y, "String == operator ");

            string str = "Cosmos is awesome!";
            string expected = "Cosmos";
            string substr = str.Substring(0, 6);
            Assert.IsTrue((substr == expected), "Substring is not equal to the expected result, result should be \"Cosmos\". Substrings are broken or MichaelTheShifter made an off-by-one error.");

            int value1 = 1;
            string value2 = "4";
            string expected_res = "1 + 3 = 4";
            Assert.IsTrue(($"{value1} + 3 = {value2}" == expected_res), "String $ operator does not work.");

            string split_in = "ABC";
            var xResultArray = split_in.Split('B');
            Assert.IsTrue(xResultArray.Length == 2, "String.Split(char[]) doesn't work.");

            xResultArray = split_in.Split(new[] { "B" }, StringSplitOptions.None);
            Assert.IsTrue(xResultArray.Length == 2, "String.Split(string[], StringSplitOptions) doesn't work.");

            string test = "This is a test string.";
            Assert.IsTrue(test.Contains("test"), "String.Contains(string) doesn't find a substring that actually exists.");
            Assert.IsTrue(test.Contains("ing"), "String.Contains(string) doesn't find a substring that actually exists.");
            Assert.IsFalse(test.Contains("cosmos"), "String.Contains(string) found a substring that didn't actually exist in a string.");

            Assert.IsTrue(test.StartsWith("This"), "string.StartsWith(string) is reporting false even though the string does start with the supplied substring.");
            Assert.IsFalse(test.StartsWith("That"), "string.StartsWith(string) is reporting true even though the string doesn't start with the supplied substring.");

            Assert.IsTrue(test.EndsWith("string."), "string.EndsWith(string) is not reporting false even though the string actually does end with the substring.");
            Assert.IsFalse(test.EndsWith("sentence."), "string.EndsWith(string) is not reporting true even though the string actually doesn't end with the substring.");

            string lower_expected = "this is a test string.";
            string upper_expected = "THIS IS A TEST STRING.";
            Assert.IsTrue((test.ToLower() == lower_expected), "string.ToLower() does not work.");
            Assert.IsTrue((test.ToUpper() == upper_expected), "string.ToUpper() does not work.");

            string replace_test = "That is a test string.";
            Assert.IsTrue((test.Replace("This", "That") == replace_test), "string.Replace(string, string) does not work.");

            test = "000";
            replace_test = "000000000000";
            Assert.IsTrue((test.Replace("0", "0000") == replace_test), "string.Replace(string, string) is recursive.");

            string char_array_test = "char";
            char[] char_array_expected = { 'c', 'h', 'a', 'r' };
            Assert.IsTrue((char_array_test.ToCharArray().Length == 4), "string.ToCharArray() does not work.");

            string strA;
            string strB;
            int comparisionResult;

            strA = "Test";
            strB = "Test";

            comparisionResult = String.Compare(strA, 0, strB, 0, strA.Length, StringComparison.Ordinal);
            Assert.IsTrue(comparisionResult == 0, "String.Compare (same string) not working!");

            strA = "\x0041\x0042\x0043";
            strB = "\x0061\x0062\x0063";

            comparisionResult = String.Compare(strA, 0, strB, 0, strA.Length, StringComparison.Ordinal);
            Assert.IsTrue(comparisionResult == -32, "String.Compare (uppercase vs lowercase) not working!");

            strA = "\x0041\x0042\x0043";
            strB = "\x0041\x0062\x0063";

            comparisionResult = String.Compare(strA, 0, strB, 0, strA.Length, StringComparison.Ordinal);
            Assert.IsTrue(comparisionResult == -32, "String.Compare (first letter same) not working!");

            strA = "Horse";
            strB = "Morse"; /* . _ . */

            comparisionResult = String.Compare(strA, 1, strB, 1, strA.Length, StringComparison.Ordinal);
            Assert.IsTrue(comparisionResult == 0, "String.Compare (first letter different skipped) not working!");

            strA = "\x0041\x0042\x0043";
            strB = "\x0061\x0062\x0063";

            comparisionResult = String.Compare(strA, 0, strB, 0, strA.Length, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(comparisionResult == 0, "String.Compare (uppercase vs lowercase ignoring case) not working!");

            string stringToHash = "test";
            int hashCode = stringToHash.GetHashCode();

            Assert.IsTrue(hashCode == -354185609, "String.GetHashCode() not working!");
        }
    }
}
