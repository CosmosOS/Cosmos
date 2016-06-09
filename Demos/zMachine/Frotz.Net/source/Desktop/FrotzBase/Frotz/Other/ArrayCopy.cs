using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using zword = System.UInt16;
using zbyte = System.Byte;

namespace Frotz.Other
{
    internal static class ArrayCopy
    {
        internal static void Copy(ushort[] sourceArray, long sourceIndex, ushort[] destinationArray, long destinationIndex, long length)
        {
            for (long i = 0; i < length; i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
            }

#if SILVERLIGHT
            
#else
            // System.Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
#endif
        }

        internal static void Copy(zbyte[] sourceArray, long sourceIndex, zbyte[] destinationArray, long destinationIndex, long length)
        {
            for (long i = 0; i < length; i++)
            {
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
            }

#if SILVERLIGHT

#else
            // System.Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
#endif
        }
    }
}
