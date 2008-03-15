using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Driver.RTL8139.Misc
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
    }
}
