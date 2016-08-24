using System;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class StringTest
    {
        public static void Execute()
        {
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
            Assert.IsTrue(split_in.Split('B').Length == 2, "Split(char) doesn't return 2 element");
            Assert.IsTrue(split_in.Split(new[] { 'B'}).Length == 2, "String.Split(char[]) doesn't work.");
            Assert.IsTrue((split_in.Split(new[] { "B" }, StringSplitOptions.None).Length == 2), "String.Split(string[], StringSplitOptions) doesn't work.");

            string test = "This is a test string.";
            Assert.IsTrue(test.Contains("test"), "string.Contains(string) doesn't find a substring that actually exists.");
            Assert.IsFalse(test.Contains("cosmos"), "string.Contains(string) found a substring that didn't actually exist in a string.");

            //Assert.IsTrue(test.EndsWith("string."), "string.EndsWith(string) is not reporting false even though the string actually does end with the substring.");
            //Assert.IsFalse(test.EndsWith("sentence."), "string.EndsWith(string) is not reporting true even though the string actually doesn't end with the substring.");

            Assert.IsTrue(test.StartsWith("This"), "string.StartsWith(string) is reporting false even though the string does start with the supplied substring.");
            Assert.IsFalse(test.StartsWith("That"), "string.StartsWith(string) is reporting true even though the string doesn't start with the supplied substring.");

            string lower_expected = "this is a test string.";
            string upper_expected = "THIS IS A TEST STRING.";
            Assert.IsTrue((test.ToLower() == lower_expected), "string.ToLower() does not work.");
            Assert.IsTrue((test.ToUpper() == upper_expected), "string.ToUpper() does not work.");

            string replace_test = "That is a test string.";
            Assert.IsTrue((test.Replace("This", "That") == replace_test), "string.Replace(string, string) does not work.");

            string char_array_test = "char";
            char[] char_array_expected = { 'c', 'h', 'a', 'r' };
            Assert.IsTrue((char_array_test.ToCharArray().Length == 4), "string.ToCharArray() does not work.");

        }
    }
}
