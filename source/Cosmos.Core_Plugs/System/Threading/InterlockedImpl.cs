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

        public static void MemoryBarrier() { }
    }
}
