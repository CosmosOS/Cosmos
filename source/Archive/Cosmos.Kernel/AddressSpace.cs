using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 

namespace Cosmos.Kernel
{
    public abstract class AddressSpace
    {
        public UInt32 Offset;
        public UInt32 Size;

        public AddressSpace( UInt32 offset, UInt32 size )
        {
            Offset = offset;
            Size = size;
        }

        /// <summary>
        /// Reads 8 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>8 bits of data read</returns>
        public abstract byte Read8( UInt32 offset );

        /// <summary>
        /// Reads 16 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>16 bits of data read</returns>
        public abstract UInt16 Read16( UInt32 offset );

        /// <summary>
        /// Reads 32 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>32 bits of data read</returns>
        public abstract UInt32 Read32( UInt32 offset );

        /// <summary>
        /// Reads 64 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>32 bits of data read</returns>
        public abstract UInt64 Read64( UInt32 offset );

        /// <summary>
        /// Reads 8 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>8 bits of data read</returns>
        public abstract byte Read8Unchecked( UInt32 offset );

        /// <summary>
        /// Reads 16 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>16 bits of data read</returns>
        public abstract UInt16 Read16Unchecked( UInt32 offset );

        /// <summary>
        /// Reads 32 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>32 bits of data read</returns>
        public abstract UInt32 Read32Unchecked( UInt32 offset );

        /// <summary>
        /// Reads 64 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>64 bits of data read</returns>
        public abstract UInt64 Read64Unchecked( UInt32 offset );

        /// <summary>
        /// Writes 8 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write8( UInt32 offset, byte value );

        /// <summary>
        /// Writes 16 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write16( UInt32 offset, UInt16 value );

        /// <summary>
        /// Writes 32 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write32( UInt32 offset, UInt32 value );

        /// <summary>
        /// Writes 64 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write64( UInt32 offset, UInt64 value );

        /// <summary>
        /// Writes 8 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write8Unchecked( UInt32 offset, byte value );

        /// <summary>
        /// Writes 16 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write16Unchecked( UInt32 offset, UInt16 value );

        /// <summary>
        /// Writes 32 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write32Unchecked( UInt32 offset, UInt32 value );

        /// <summary>
        /// Writes 64 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write64Unchecked( UInt32 offset, UInt64 value );
    }
}
