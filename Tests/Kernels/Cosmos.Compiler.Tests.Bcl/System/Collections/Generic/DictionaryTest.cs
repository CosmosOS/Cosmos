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

            Assert.IsTrue(dictionary.ContainsKey("a"),  "Dictionary<string, string> ContainsKey does not work1");
            Assert.IsFalse(dictionary.ContainsKey("d"), "Dictionary<string, string> ContainsKey does not work 2");

            //String test
            Assert.IsTrue(dictionary["a"] == "a", "Dictionary<string, string> [] operator (get) does not work");
            dictionary["b"] = "d";
            Assert.IsTrue(dictionary["b"] == "d", "Dictionary<string, string> [] operator (set existing) does not work");

            Assert.IsTrue(dictionary.Count == 3, "Dictionary<string, string>.Count does not work");
            dictionary["d"] = "d";
            Assert.IsTrue(dictionary["d"] == "d", "Dictionary<string, string> [] operator (set not existing) does not work");

            /* We added another key so now Count should be 4 */ 
            Assert.IsTrue(dictionary.Count == 4, "Dictionary<string, string>.Count (after new key) does not work");

            Dictionary<string, string>.KeyCollection keyColl = dictionary.Keys;

            foreach (string key in keyColl)
            {
                Assert.IsTrue(key == "a" || key == "b" || key == "c" || key == "d", "Dictionary<string, string>.Keys returns invalid key");
            }

            dictionary.Add("e", "e");
            /* We added another key so now Count should be 5 */
            Assert.IsTrue(dictionary.Count == 5, "Dictionary<string, string>.Count (after Added key) does not work");

            /* Now we remove "e" key, the operation should succeed and Count should be 4 again */
            Assert.IsTrue(dictionary.Remove("e"), "Dictionary<string, string>.Remove() of existing key does not work");
            Assert.IsTrue(dictionary.Count == 4, "Dictionary<string, string>.Count (after Removed key) does not work");

            /* Now we remove "f" key, the operation should fail as there is not "f" key */
            Assert.IsFalse(dictionary.Remove("f"), "Dictionary<string, string>.Remove() of not existing key does not work");

            Assert.IsTrue(dictionary.TryGetValue("a", out string val), "Dictionary<string, string>.TryGetValue() of existing key does not work");
            Assert.IsFalse(dictionary.TryGetValue("f", out string val2), "Dictionary<string, string>.TryGetValue() of not existing key does not work");

            var dictionary2 = new Dictionary<string, int>
            {
                { "One", 1 },
                { "Two",  2},
                { "Three", 3 },
            };

            Assert.IsTrue(dictionary2.ContainsKey("One"), "Dictionary<string, int> ContainsKey does not work1");
            Assert.IsFalse(dictionary2.ContainsKey("Four"), "Dictionary<string, int> ContainsKey does not work2");

            Assert.IsTrue(dictionary2["One"] == 1, "Dictionary<string, int> operator [] does not work");
            dictionary2["Two"] = 22;
            Assert.IsTrue(dictionary2["Two"] == 22, "Dictionary<string, int> [] operator (set existing) does not work");

            Assert.IsTrue(dictionary2.Count == 3, "Dictionary<string, int>.Count does not work");
            dictionary2["Four"] = 4;
            Assert.IsTrue(dictionary2["Four"] == 4, "Dictionary<string, int> [] operator (set not existing) does not work");

            /* We added another key so now Count should be 4 */
            Assert.IsTrue(dictionary2.Count == 4, "Dictionary<string, int>.Count (after new key) does not work");

            Dictionary<string, int>.KeyCollection keyColl2 = dictionary2.Keys;

            foreach (string key in keyColl2)
            {
                Assert.IsTrue(key == "One" || key == "Two" || key == "Three" || key == "Four", "Dictionary<string, int>.Keys returns invalid key");
            }

            dictionary2.Add("Five", 5);
            /* We added another key so now Count should be 5 */
            Assert.IsTrue(dictionary2.Count == 5, "Dictionary<string, int>.Count (after Added key) does not work");

            /* Now we remove "Five" key, the operation should succeed and Count should be 4 again */
            Assert.IsTrue(dictionary2.Remove("Five"), "Dictionary<string, int>.Remove() of existing key does not work");
            Assert.IsTrue(dictionary2.Count == 4, "Dictionary<string, int>.Count (after Removed key) does not work");

            /* Now we remove "Six" key, the operation should fail as there is not "Six" key */
            Assert.IsFalse(dictionary2.Remove("Six"), "Dictionary<string, string>.Remove() of not existing key does not work");

            Assert.IsTrue(dictionary2.TryGetValue("One", out int val3), "Dictionary<string, string>.TryGetValue() of existing key does not work");
            Assert.IsFalse(dictionary2.TryGetValue("Six", out int val4), "Dictionary<string, string>.TryGetValue() of not existing key does not work");
#if false
            var dictionary3 = new Dictionary<int, string>
            {
                { 1, "One"},
                { 2, "Two"},
                { 3, "Three"},
            };

            Assert.IsTrue(dictionary3.ContainsKey(1), "Dictionary<int, string> ContainsKey does not work1");
            Assert.IsFalse(dictionary3.ContainsKey(4), "Dictionary<int, string> ContainsKey does not work2");

            Assert.IsTrue(dictionary3[1] == "One", "Dictionary<int, string> operator [] does not work");
#endif
        }
    }
}
