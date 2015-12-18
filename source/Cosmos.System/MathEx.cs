using System;

namespace Cosmos.System
{
    public static class MathEx
    {
        public static long Rem(long a, long b)
        {
            long result = a;
            while (result - b > 0)
            {
                result = result - b;
            }
            return result;
        }
    }
}
