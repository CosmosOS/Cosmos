using System;
using System.Linq;
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

            char x = 'a';
            string y = "a";
            Assert.IsTrue(x.ToString() == y, "String == operator ");

            string str = "Cosmos is awesome!";
            string expected = "Cosmos";
            string substr = str.Substring(0, 6);
            Assert.IsTrue((substr == expected), "Substring is not equal to the expected result, result should be \"Cosmos\". Substrings are broken or MichaelTheShifter made an off-by-one error.");


            int value1 = 1;
            string value2 = " = 4";
            string expected_res = "1 + 3 = 4";
            Assert.IsTrue(($"{value1} + 3 = {value2}" == expected_res), "String $ operator does not work.");

            string split_in = "Cosmos is the best.";
            string[] split_expected = { "Cosmos", "is", "the", "best" };
            Assert.IsTrue((split_in.Split(' ') == split_expected), "String.Split(char) doesn't work.");
            Assert.IsTrue((split_in.Split(new[] { ' '}) == split_expected), "String.Split(char[]) doesn't work.");
            Assert.IsTrue((split_in.Split(new[] { " " }, StringSplitOptions.None) == split_expected), "String.Split(string[], StringSplitOptions) doesn't work.");


        }
    }
}
