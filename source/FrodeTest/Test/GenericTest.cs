using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    class GenericTest
    {
        public static void RunTest()
        {
            Check.SetHeadingText("Testing System.Collections.Generic");

            Check.Text = "Dictionary<UInt32, string>";
            Check.Validate(TestDictionary() == "Hello");
            Check.Text = "List<string>.Count";
            Check.Validate(TestListCount() == 2);
        }

        public static string TestDictionary()
        {
            var x = new Dictionary<UInt32, string>();
            x.Add(1000, "Hello");
            return x[1000];
        }

        public static int TestListCount()
        {
            List<string> list = new List<string>();
            list.Add("Hello");
            list.Add("World");

            return list.Count;
        }
    }
}
