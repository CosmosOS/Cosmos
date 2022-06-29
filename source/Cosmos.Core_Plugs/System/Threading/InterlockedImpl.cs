using System;
using System.Threading;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(Target = typeof(Interlocked))]
    public static class InterlockedImpl
    {
        public static object CompareExchange(ref object location1, object value, object comparand)
        {
            var original = location1;

            if (location1 == comparand)
            {
                location1 = value;
            }

            return original;
        }

        public static int CompareExchange(ref int location1, int value, int comparand)
        {
            var original = location1;

            if (location1 == comparand)
            {
                location1 = value;
            }

            return original;
        }

        public static long CompareExchange(ref long location1, long value, long comparand)
        {
            var original = location1;

            if (location1 == comparand)
            {
                location1 = value;
            }

            return original;
        }

        public static int Decrement(ref int aData)
        {
            return aData -= 1;
        }

        public static int Exchange(ref int location1, int value)
        {
            var original = location1;
            location1 = value;

            return original;
        }

        public static long Exchange(ref long aLocation1, long aValue)
        {
            var original = aLocation1;
            aLocation1 = aValue;

            return original;
        }
        public static object Exchange(ref object aObject, object aValue)
        {
            var toReturn = aObject;
            aObject = aValue;
            return toReturn;
        }

        public static int ExchangeAdd(ref int aLocation, int aValue)
        {
            var toReturn = aLocation;
            aLocation += aValue;
            return toReturn;
        }

        public static IntPtr Exchange(ref IntPtr aLocation, IntPtr aValue)
        {
            IntPtr toReturn = aLocation;
            aLocation = aValue;
            return toReturn;
        }

        public static long ExchangeAdd(ref long aLocation, long aValue)
        {
            long toReturn = aLocation;
            aLocation += aValue;
            return toReturn;
        }
        public static void MemoryBarrier() { }
    }
}
