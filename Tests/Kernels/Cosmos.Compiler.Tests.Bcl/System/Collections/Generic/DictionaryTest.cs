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
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"a", "a"},
                    {"b", "b" },
                    {"c", "c"}
                };

                Assert.IsTrue(dictionary.ContainsKey("a"), "Dictionary<string, string> ContainsKey does not work1");
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
            }

            {
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
                Assert.IsFalse(dictionary2.Remove("Six"), "Dictionary<string, int>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary2.TryGetValue("One", out int val3), "Dictionary<string, int>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary2.TryGetValue("Six", out int val4), "Dictionary<string, int>.TryGetValue() of not existing key does not work");
            }

            //#region "Dictionary<char, char> Tests"
            //{
            //    var dictionary = new Dictionary<char, char>
            //    {
            //        { 'a', 'a' },
            //        { 'b', 'b' },
            //        { 'c', 'c' },
            //    };

            //    Assert.IsTrue(dictionary.ContainsKey('a'), "Dictionary<char, char> ContainsKey does not work1");
            //    Assert.IsFalse(dictionary.ContainsKey('d'), "Dictionary<char, char> ContainsKey does not work2");

            //    Assert.IsTrue(dictionary['a'] == 'a', "Dictionary<char, char> operator [] does not work");
            //    dictionary['b'] = 'v';
            //    Assert.IsTrue(dictionary['b'] == 'v', "Dictionary<char, char> [] operator (set existing) does not work");

            //    Assert.IsTrue(dictionary.Count == 4, "Dictionary<char, char>.Count does not work");
            //    dictionary['d'] = 'd';
            //    Assert.IsTrue(dictionary['d'] == 'd', "Dictionary<char, char> [] operator (set not existing) does not work");

            //    /* We added another key so now Count should be 4 */
            //    Assert.IsTrue(dictionary.Count == 4, "Dictionary<char, char>.Count (after new key) does not work");

            //    Dictionary<char, char>.KeyCollection keyColl = dictionary.Keys;

            //    foreach (var key in keyColl)
            //    {
            //        Assert.IsTrue(key == 'a' || key == 'b' || key == 'c' || key == 'd', "Dictionary<char, char>.Keys returns invalid key");
            //    }

            //    dictionary.Add('e', 'e');
            //    /* We added another key so now Count should be 5 */
            //    Assert.IsTrue(dictionary.Count == 5, "Dictionary<char, char>.Count (after Added key) does not work");

            //    /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
            //    Assert.IsTrue(dictionary.Remove('e'), "Dictionary<char, char>.Remove() of existing key does not work");
            //    Assert.IsTrue(dictionary.Count == 4, "Dictionary<char, char>.Count (after Removed key) does not work");

            //    /* Now we remove "6" key, the operation should fail as there is not "6" key */
            //    Assert.IsFalse(dictionary.Remove('f'), "Dictionary<char, char>.Remove() of not existing key does not work");

            //    Assert.IsTrue(dictionary.TryGetValue('a', out char val1), "Dictionary<char, char>.TryGetValue() of existing key does not work");
            //    Assert.IsFalse(dictionary.TryGetValue('f', out char val2), "Dictionary<char, char>.TryGetValue() of not existing key does not work");
            //}
            //#endregion

            #region "Dictionary<sbyte, sbyte> Tests"
            {
                var dictionary = new Dictionary<sbyte, sbyte>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<sbyte, sbyte> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<sbyte, sbyte> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<sbyte, sbyte> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<sbyte, sbyte> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<sbyte, sbyte>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<sbyte, sbyte> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<sbyte, sbyte>.Count (after new key) does not work");

                Dictionary<sbyte, sbyte>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<sbyte, sbyte>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<sbyte, sbyte>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<sbyte, sbyte>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<sbyte, sbyte>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<sbyte, sbyte>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out sbyte val1), "Dictionary<sbyte, sbyte>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out sbyte val2), "Dictionary<sbyte, sbyte>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<byte, byte> Tests"
            {
                var dictionary = new Dictionary<byte, byte>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<byte, byte> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<byte, byte> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<byte, byte> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<byte, byte> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<byte, byte>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<byte, byte> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<byte, byte>.Count (after new key) does not work");

                Dictionary<byte, byte>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<byte, byte>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<byte, byte>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<byte, byte>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<byte, byte>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<byte, byte>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out byte val1), "Dictionary<byte, byte>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out byte val2), "Dictionary<byte, byte>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<short, short> Tests"
            {
                var dictionary = new Dictionary<short, short>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<short, short> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<short, short> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<short, short> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<short, short> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<short, short>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<short, short> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<short, short>.Count (after new key) does not work");

                Dictionary<short, short>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<short, short>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<short, short>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<short, short>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<short, short>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<short, short>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out short val1), "Dictionary<short, short>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out short val2), "Dictionary<short, short>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<ushort, ushort> Tests"
            {
                var dictionary = new Dictionary<ushort, ushort>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<ushort, ushort> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<ushort, ushort> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<ushort, ushort> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<ushort, ushort> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<ushort, ushort>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<ushort, ushort> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<ushort, ushort>.Count (after new key) does not work");

                Dictionary<ushort, ushort>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<ushort, ushort>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<ushort, ushort>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<ushort, ushort>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<ushort, ushort>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<ushort, ushort>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out ushort val1), "Dictionary<ushort, ushort>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out ushort val2), "Dictionary<ushort, ushort>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<int, int> Tests"
            {
                var dictionary = new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<int, int> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<int, int> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<int, int> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<int, int> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<int, int>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<int, int> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<int, int>.Count (after new key) does not work");

                Dictionary<int, int>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<int, int>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<int, int>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<int, int>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<int, int>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<int, int>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out int val1), "Dictionary<int, int>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out int val2), "Dictionary<int, int>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<uint, uint> Tests"
            {
                var dictionary = new Dictionary<uint, uint>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<uint, uint> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<uint, uint> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<uint, uint> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<uint, uint> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<uint, uint>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<uint, uint> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<uint, uint>.Count (after new key) does not work");

                Dictionary<uint, uint>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<uint, uint>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<uint, uint>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<uint, uint>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<uint, uint>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<uint, uint>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out uint val1), "Dictionary<uint, uint>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out uint val2), "Dictionary<uint, uint>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<long, long> Tests"
            {
                var dictionary = new Dictionary<long, long>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<long, long> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<long, long> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<long, long> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<long, long> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<long, long>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<long, long> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<long, long>.Count (after new key) does not work");

                Dictionary<long, long>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<long, long>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<long, long>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<long, long>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<long, long>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<long, long>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out long val1), "Dictionary<long, long>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out long val2), "Dictionary<long, long>.TryGetValue() of not existing key does not work");
            }
            #endregion

            #region "Dictionary<ulong, ulong> Tests"
            {
                var dictionary = new Dictionary<ulong, ulong>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                };

                Assert.IsTrue(dictionary.ContainsKey(1), "Dictionary<ulong, ulong> ContainsKey does not work1");
                Assert.IsFalse(dictionary.ContainsKey(4), "Dictionary<ulong, ulong> ContainsKey does not work2");

                Assert.IsTrue(dictionary[1] == 1, "Dictionary<ulong, ulong> operator [] does not work");
                dictionary[2] = 22;
                Assert.IsTrue(dictionary[2] == 22, "Dictionary<ulong, ulong> [] operator (set existing) does not work");

                Assert.IsTrue(dictionary.Count == 3, "Dictionary<ulong, ulong>.Count does not work");
                dictionary[4] = 4;
                Assert.IsTrue(dictionary[4] == 4, "Dictionary<ulong, ulong> [] operator (set not existing) does not work");

                /* We added another key so now Count should be 4 */
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<ulong, ulong>.Count (after new key) does not work");

                Dictionary<ulong, ulong>.KeyCollection keyColl = dictionary.Keys;

                foreach (var key in keyColl)
                {
                    Assert.IsTrue(key == 1 || key == 2 || key == 3 || key == 4, "Dictionary<ulong, ulong>.Keys returns invalid key");
                }

                dictionary.Add(5, 5);
                /* We added another key so now Count should be 5 */
                Assert.IsTrue(dictionary.Count == 5, "Dictionary<ulong, ulong>.Count (after Added key) does not work");

                /* Now we remove "5" key, the operation should succeed and Count should be 4 again */
                Assert.IsTrue(dictionary.Remove(5), "Dictionary<ulong, ulong>.Remove() of existing key does not work");
                Assert.IsTrue(dictionary.Count == 4, "Dictionary<ulong, ulong>.Count (after Removed key) does not work");

                /* Now we remove "6" key, the operation should fail as there is not "6" key */
                Assert.IsFalse(dictionary.Remove(6), "Dictionary<ulong, ulong>.Remove() of not existing key does not work");

                Assert.IsTrue(dictionary.TryGetValue(1, out ulong val1), "Dictionary<ulong, ulong>.TryGetValue() of existing key does not work");
                Assert.IsFalse(dictionary.TryGetValue(6, out ulong val2), "Dictionary<ulong, ulong>.TryGetValue() of not existing key does not work");
            }
            #endregion

            //TODO: Add GUID test once newGUID returns something other than a zero initialized guid.

        }
    }
}
