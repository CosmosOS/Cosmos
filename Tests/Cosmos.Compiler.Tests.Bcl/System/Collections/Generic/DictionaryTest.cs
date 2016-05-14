using Cosmos.TestRunner;
using System.Collections.Generic;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Generic
{
    public static class DictionaryTest
    {
        public static void Execute()
        {
            var xDictionary = new Dictionary<string, string>();
            xDictionary.Add("ValueOne", "One");
            xDictionary.Add("ValueTwo", "Two");
            xDictionary.Add("ValueThree", "Three");
            Assert.AreEqual(3, xDictionary.Count, "After adding 3 items, count != 3");
            Assert.IsTrue(xDictionary["ValueOne"] == "One", "Dictionary['ValueOne'] != 'One'");
            Assert.IsTrue(xDictionary["ValueTwo"] == "Two", "Dictionary['ValueTwo'] != 'Two'");
            Assert.IsTrue(xDictionary["ValueTwo"] == "THree", "Dictionary['ValueThree'] != 'Three'");
        }
    }
}
