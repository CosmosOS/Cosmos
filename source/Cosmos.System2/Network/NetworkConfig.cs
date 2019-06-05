/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network dictionary
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network
{
    public class NetworkConfig
    {
        public static List<NetworkDevice> Keys = new List<NetworkDevice>();
        public static List<Config> Values = new List<Config>();

        public Config this[NetworkDevice key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Values[Keys.IndexOf(key)] = value;
            }
        }

        public static int Count
        {
            get
            {
                return Keys.Count;
            }
        }

        public static bool ContainsKey(NetworkDevice k)
        {
            return Keys.Contains(k);
        }

        public static Config Get(NetworkDevice key)
        {
            int index = Keys.IndexOf(key);
            return Values[index];
        }

        public static void Add(NetworkDevice key, Config value)
        {
            Keys.Add(key);
            Values.Add(value);
        }

        public static NetworkDevice[] GetKeys()
        {
            return Keys.ToArray();
        }

        public static Config[] GetValues()
        {
            return Values.ToArray();
        }

        public static NetworkDevice GetKeyByValue(Config value)
        {
            var x = Values.IndexOf(value);
            var x_ = Keys[x];
            return x_;
        }

        public static void Remove(NetworkDevice key)
        {
            int index = Keys.IndexOf(key);
            Keys.RemoveAt(index);
            Values.RemoveAt(index);
        }

        public static void Clear()
        {
            Keys = new List<NetworkDevice>();
            Values = new List<Config>();
        }

        /// <summary>
        /// Get Values
        /// </summary>
        /// <returns></returns>
        public static IEnumerator GetEnumerator_V()
        {
            return ((IEnumerable)Values).GetEnumerator();
        }

        /// <summary>
        /// Default GetEnumerator (Keys)
        /// </summary>
        /// <returns></returns>
        public static IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Keys).GetEnumerator();
        }

    }
}
