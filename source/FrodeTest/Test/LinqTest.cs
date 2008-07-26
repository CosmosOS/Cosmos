using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public static class LinqTest
    {
        public static void RunTest()
        {
            Console.WriteLine(" -- Testing LINQ --");

            string[] text = new string[] { "hello", "world", "LinqWorks", "!", };

            var result = (from word in text
                          where word == "LinqWorks"
                          select word);

            Console.WriteLine(result.Last());
        }
    }
}