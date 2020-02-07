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

            Assert.IsTrue(test.IndexOf(string.Empty, 10) == 10, "string.IndexOf currectly returns for empty string with start index");
            Assert.IsTrue(test.IndexOf(string.Empty, 10, 10) == 10, "string.IndexOf currectly returns for empty string with start index and count");
            Assert.IsTrue(test.IndexOf('T') == 0, "string.IndexOf finds the only occurance of a letter");
            Assert.IsTrue(test.IndexOf('A') == -1, "string.IndexOf correctly returns when it does not find something");
            Assert.IsTrue(test.IndexOf("ABCDE") == -1, "string.IndexOf correctly returns when it does not find something");
            Assert.IsTrue(test.IndexOf('.') == test.Length - 1, "string.IndexOf finds the only occurance of a letter at the end of the string");
            Assert.IsTrue(test.IndexOf('i') == 2, "string.IndexOf finds the first of multiple occurances of a letter");
            Assert.IsTrue(test.IndexOf('i', 8) == 18, "string.IndexOf with start point finds the first of multiple occurances of a letter");
            Assert.IsTrue(test.IndexOf("is") == 2, "string.IndexOf finds the first of multiple occurances of a string");
            Assert.IsTrue(test.IndexOf("is", 3) == 5, "string.IndexOf with start point finds the first of multiple occurances of a string");
            Assert.IsTrue(test.IndexOf("is", 3, 5) == 5, "string.IndexOf with start point and count finds the first of multiple occurances of a string");
            Assert.IsTrue(test.IndexOf("is", 3, 1) == -1, "string.IndexOf with start point and count correctly returns if it does not find something");

            Assert.IsTrue(test.IndexOfAny(new[] { 'T', 'h', 'i', 's' }) == 0, "string.IndexOfAny finds the first one");
            Assert.IsTrue(test.IndexOfAny(new[] { 'A', 'B', 'C' }) == -1, "string.IndexOfAny finds none if none are present");

            Assert.IsTrue(test.LastIndexOf(string.Empty, 100) == test.Length, "string.LastIndexOf handles empty correctly");
            Assert.IsTrue(test.LastIndexOf('T') == 0, "string.LastIndexOf finds the only occurance of a letter");
            Assert.IsTrue(test.LastIndexOf('.') == test.Length - 1, "string.LastIndexOf finds the only occurance of a letter at the end of the string");
            Assert.IsTrue(test.LastIndexOf('i') == test.Length - 4, "string.IndexOf finds the last of multiple occurances of a letter");

            Assert.IsTrue(test.LastIndexOfAny(new[] { 'T', 'h', 'i', 's' }) == 18, "string.LastIndexOfAny finds the first one");
            Assert.IsTrue(test.LastIndexOfAny(new[] { 'A', 'B', 'C' }) == -1, "string.LastIndexOfAny finds none if none are present");


            Assert.IsTrue(test.Insert(0, "A") != test, "string.Insert creates a new instance");
            Assert.IsTrue(test.Insert(1, "A") == "TAhis is a test string.", "string.Insert correctly inserts a single character");
            Assert.IsTrue(test.Insert(2, "ABCDE F") == "ThABCDE Fis is a test string.", "string.Insert correctly adds multiple characters");
            Assert.IsTrue(test.Insert(test.Length, "END") == "This is a test string.END", "string.Insert correctly inserts at the end of the string");

            Assert.IsTrue(test.Remove(1) == "T", "string.Remove correctly removes all other characters");
            Assert.IsTrue(test.Remove(0) == "", "string.Remove correctly removes all characters");
            Assert.IsTrue(test.Remove(0, 2) == "is is a test string.", "string.Remove works with count");

            Assert.IsTrue("    a     ".Trim() == "a", "string.Trim trims both front and back");
            Assert.IsTrue("abababababa".Trim(new[] { 'a', 'b' }) == "", "string.Trim works with custom chars");
            Assert.IsTrue("abCababababa".Trim(new[] { 'a', 'b' }) == "C", "string.Trim works with custom chars");
            Assert.IsTrue("a".Trim() == "a", "string.Trim trims both front and back");
            Assert.IsTrue("  a     ".TrimStart() == "a     ", "string.TrimStart trims front");
            Assert.IsTrue("a".TrimStart() == "a", "string.Trim trims front");
            Assert.IsTrue("    a     ".TrimEnd() == "    a", "string.TrimEnd trims back");
            Assert.IsTrue("a".TrimEnd() == "a", "string.TrimEnd trims back");


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
