using System;

namespace Cosmos.System
{
    /// <summary>
    /// MathEx class. Provides additional math methods.
    /// </summary>
    public static class MathEx
    {
        /// <summary>
        /// Get the remainder on division of a in b.
        /// </summary>
        /// <param name="a">Divided number.</param>
        /// <param name="b">Divider.</param>
        /// <returns>long value.</returns>
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
