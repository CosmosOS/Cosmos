using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Tests.IO.System.IO
{
    class HelperMethods
    {
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

        /// <summary>
        /// Utility method to test string[] equality.
        /// </summary>
        /// <param name="a1">String array.</param>
        /// <param name="a2">String array.</param>
        /// <returns>True if the elements in the arrays are equal otherwise false.</returns>
        public static bool StringArrayAreEquals(string[] a1, string[] a2)
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
                    //mDebugger.Send("In position " + i + " a String is different");
                    return false;
                }
            }

            return true;
        }
    }
}
