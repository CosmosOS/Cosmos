using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;


namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Generic
{
    public static class DictionaryTest
    {
        public static void Execute()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"a", "a"},
                {"b", "b" },
                {"c", "c"}
            };

            Assert.IsTrue(dictionary.ContainsKey("a"), "Dictionary ContainsKey does not work1");
            Assert.IsFalse(dictionary.ContainsKey("d"), "Dictionary ContainsKey does not work 2");
            


            //String test
            Assert.IsTrue(dictionary["a"] == "a", "Dictionary string not work");
            dictionary["b"] = "d";
            Assert.IsTrue(dictionary["b"] == "d", "Dictionary string been reset not working");
        }
    }
}
