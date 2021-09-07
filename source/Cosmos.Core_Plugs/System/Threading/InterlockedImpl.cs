using System;
using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(Target = typeof(Interlocked))]
    public static class InterlockedImpl
    {
        public static int Decrement(ref int aData)
        {
            return aData -= 1;
        }
        public static long Decrement(ref long aData)
        {
            return aData -= 1;
        }
        public static int Increment(ref int aData)
        {
            return aData += 1;
        }
        public static long Increment(ref long aData)
        {
            return aData += 1;
        }
        public static int Exchange(ref int loc1, int aData)
        {
            int orgValue = loc1;
            loc1 = aData;
            return orgValue;
        }
        public static long Exchange(ref long loc1, long aData)
        {
            long orgValue = loc1;
            loc1 = aData;
            return orgValue;
        }
        public static float Exchange(ref float loc1, float aData)
        {
            float orgValue = loc1;
            loc1 = aData;
            return orgValue;
        }
        public static double Exchange(ref double loc1, double aData)
        {
            double orgValue = loc1;
            loc1 = aData;
            return orgValue;
        }
        public static Object Exchange(ref Object loc1, Object aData)
        {
            Object orgValue = loc1;
            loc1 = aData;
            return orgValue;
        }
        public static IntPtr Exchange(ref IntPtr loc1, IntPtr aData)
        {
            IntPtr orgValue = loc1;
            loc1 = aData;
            return orgValue;
        }
        public static EventHandler CompareExchange(ref EventHandler loc1, EventHandler val, EventHandler comparand)
        {
            EventHandler old = loc1;
            if (loc1 == comparand)
            {
                loc1 = val;
            }
            return old;
        }
    }
}
