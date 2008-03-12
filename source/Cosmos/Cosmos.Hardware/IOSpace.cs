using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware
{
    /// <summary>
    /// The IOSpace class is used to access memory directly.
    /// </summary>
    public static class IOSpace
    {
        #region Read from IO space
        public static unsafe byte Read8(uint address)
        {
            byte* pointer = (byte*)address;
            byte data = *pointer;
            return data;
        }

        public static unsafe UInt16 Read16(uint address)
        {
            UInt16* pointer = (UInt16*)address;
            UInt16 data = *pointer;
            return data;
        }

        public static unsafe UInt32 Read32(uint address)
        {
            UInt32* pointer = (UInt32*)address;
            UInt32 data = *pointer;
            return data;
        }

        public static unsafe UInt64 Read64(uint address)
        {
            UInt64* pointer = (UInt64*)address;
            UInt64 data = *pointer;
            return data;
        }

        #endregion

        #region Write to IO space
        public static unsafe void Write8(uint address, byte data)
        {
            ushort* pointer = (ushort*)address;
            *pointer = data;
        }

        public static unsafe void Write16(uint address, UInt16 data)
        {
            UInt16* pointer = (UInt16*)address;
            *pointer = data;
        }

        public static unsafe void Write32(uint address, UInt32 data)
        {
            UInt32* pointer = (UInt32*)address;
            *pointer = data;
        }

        public static unsafe void Write64(uint address, UInt64 data)
        {
            UInt64* pointer = (UInt64*)address;
            *pointer = data;
        }

        #endregion

        

    }
}
