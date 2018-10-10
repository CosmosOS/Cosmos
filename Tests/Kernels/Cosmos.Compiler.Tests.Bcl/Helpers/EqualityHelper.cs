using System;

namespace Cosmos.Compiler.Tests.Bcl
{
    internal static class EqualityHelper
    {
        public static bool SinglesAreEqual(float left, float right)
        {
            // Define the tolerance for variation in their values
            float difference = Math.Abs(left * .00001F);
            return Math.Abs(left - right) <= difference;
        }

        public static bool DoublesAreEqual(double left, double right)
        {
            // Define the tolerance for variation in their values
            double difference = Math.Abs(left * .00001);
            return Math.Abs(left - right) <= difference;
        }

        /// <summary>
        /// Utility method to test Byte[] equality.
        /// </summary>
        /// <param name="a1">Byte array.</param>
        /// <param name="a2">Byte array.</param>
        /// <returns>True if the elements in the arrays are equal otherwise false.</returns>
        public static bool ByteArrayAreEquals(byte[] a1, byte[] a2)
        {
            if (ReferenceEquals(a1, a2))
            {
                //mDebugger.Send("a1 and a2 are the same Object");
                return true;
            }

            if (a1 == null || a2 == null)
            {
                //mDebugger.Send("a1 or a2 is null so are different");
                return false;
            }

            if (a1.Length != a2.Length)
            {
                //mDebugger.Send("a1.Length != a2.Length so are different");
                return false;
            }

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    //mDebugger.Send("In position " + i + " a byte is different");
                    return false;
                }
            }

            return true;
        }
    }
}
