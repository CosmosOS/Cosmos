/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network dictionary
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.HAL;

namespace Cosmos.System.Network.Config
{
    public class NetworkConfig
    {
        public static List<NetworkDevice> Keys = new List<NetworkDevice>();
        public static List<IPConfig> Values = new List<IPConfig>();
        public static KeyValuePair<NetworkDevice, IPConfig> CurrentConfig;

        public IPConfig this[NetworkDevice key]
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
            foreach (var device in Keys)
            {
                if (k == device)
                {
                    return true;
                }
            }
            return false;
        }

        public static IPConfig Get(NetworkDevice key)
        {
            int index = 0;

            foreach (var device in Keys)
            {
                if (key == device)
                {
                    break;
                }
                index++;
            }

            return Values[index];
        }

        public static void Add(NetworkDevice key, IPConfig value)
        {
            Keys.Add(key);
            Values.Add(value);
        }

        public static NetworkDevice[] GetKeys()
        {
            return Keys.ToArray();
        }

        public static IPConfig[] GetValues()
        {
            return Values.ToArray();
        }

        public static NetworkDevice GetKeyByValue(IPConfig value)
        {
            var x = Values.IndexOf(value);
            var x_ = Keys[x];
            return x_;
        }

        public static void Remove(NetworkDevice key)
        {
            int index = 0;

            foreach (var device in Keys)
            {
                if (key == device)
                {
                    break;
                }
                index++;
            }
            Keys.RemoveAt(index);
            Values.RemoveAt(index);
        }

        public static void Clear()
        {
            Keys = new List<NetworkDevice>();
            Values = new List<IPConfig>();
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
