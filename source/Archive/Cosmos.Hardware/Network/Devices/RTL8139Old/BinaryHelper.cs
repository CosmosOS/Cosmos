using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Network.Devices.RTL8139
{
    /// <summary>
    /// Contains various helpermethods to make bitfiddling easier.
    /// </summary>
    public class BinaryHelper
    {
        /// <summary>
        /// Bitwise checks it the given bit is set in the data.
        /// </summary>
        /// <param name="bit">The zero-based position of a bit. I.e. bit 1 is the second bit.</param>
        /// <returns>Returns TRUE if bit is set.</returns>
        public static bool CheckBit(UInt16 data, ushort bit)
        {
            //A single bit is LEFT SHIFTED the number a given number of bits.
            //and bitwise AND'ed together with the data.
            //So the initial value is   :       0000 0000.
            //Left shifting a bit 3 bits:       0000 0100
            //And'ed together with the data:    0101 0101 AND 0000 01000 => 0000 0100 (which is greater than zero - so bit is set).

            ushort mask = (ushort)(1 << (ushort)bit);
            return (data & mask) != 0;
        }

        public static bool CheckBit(UInt32 data, ushort bit)
        {
            UInt32 mask = (UInt32)(1 << (int)bit);
            return (data & mask) != 0;
        }

        public static bool CheckBit(byte data, byte bit)
        {
            byte mask = (byte)(1 << bit);
            return (data & mask) != 0;
        }

        /// <summary>
        /// Changes the value in the given position. Change bitvalue from low to high, or high to low.
        /// Returns the same byte, but with one bit changed.
        /// </summary>
        public static byte FlipBit(byte data, ushort bitposition)
        {
            byte mask = (byte)(1 << bitposition);
            if (CheckBit(data, bitposition))
                return (byte)(data & ~mask);
            else
                return (byte)(data | mask);
        }

        public static UInt32 FlipBit(UInt32 data, ushort bitposition)
        {
            UInt32 mask = (UInt32)(1 << bitposition);
            if (CheckBit(data, bitposition))
                return (UInt32)(data & ~mask);
            else
                return (UInt32)(data | mask);
        }


        /// <summary>
        /// Retrieves a byte of data from somewhere inside a 32 bit number. An offset is used to indicate where in
        /// the 32 bit number to start extracting 8 bits.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static byte GetByteFrom32bit(UInt32 data, byte offset)
        {
            if (offset > 24)
                throw new ArgumentException("Offset can not move outside the 32 bit range");

            data = data >> offset;
            return (byte)data;
        }

        /// <summary>
        /// Returns the HEX value of a given bitnumber
        /// </summary>
        [Flags]
        public enum BitPos : uint
        {
            BIT0 = 0x1,
            BIT1 = 0x2,
            BIT2 = 0x4,
            BIT3 = 0x8,
            BIT4 = 0x10,
            BIT5 = 0x20,
            BIT6 = 0x40,
            BIT7 = 0x80,
            BIT8 = 0x100,
            BIT9 = 0x200,
            BIT10 = 0x400,
            BIT11 = 0x800,
            BIT12 = 0x1000,
            BIT13 = 0x2000,
            BIT14 = 0x4000,
            BIT15 = 0x8000,
            BIT16 = 0x10000,
            BIT17 = 0x20000,
            BIT18 = 0x40000,
            BIT19 = 0x80000,
            BIT20 = 0x100000,
            BIT21 = 0x200000,
            BIT22 = 0x400000,
            BIT23 = 0x800000,
            BIT24 = 0x1000000,
            BIT25 = 0x2000000,
            BIT26 = 0x4000000,
            BIT27 = 0x8000000,
            BIT28 = 0x10000000,
            BIT29 = 0x20000000,
            BIT30 = 0x40000000,
            BIT31 = 0x80000000
        }
    }
}
