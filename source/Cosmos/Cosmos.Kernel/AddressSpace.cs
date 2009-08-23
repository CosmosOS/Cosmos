using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel {
    public abstract class AddressSpace {
        public UInt32 Offset;
        public UInt32 Size;

        public AddressSpace(UInt32 offset, UInt32 size) {
            Offset = offset;
            Size = size;
        }

        /// <summary>
        /// Reads 8 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>8 bits of data read</returns>
        public abstract byte Read8(UInt32 offset);
        
        /// <summary>
        /// Reads 16 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>16 bits of data read</returns>
        public abstract UInt16 Read16(UInt32 offset);
        
        /// <summary>
        /// Reads 32 bits from a given offset if it within the valid range
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>32 bits of data read</returns>
        public abstract UInt32 Read32(UInt32 offset);

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
        public abstract byte Read8Unchecked(UInt32 offset);

        /// <summary>
        /// Reads 16 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>16 bits of data read</returns>
        public abstract UInt16 Read16Unchecked(UInt32 offset);

        /// <summary>
        /// Reads 32 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>32 bits of data read</returns>
        public abstract UInt32 Read32Unchecked(UInt32 offset);

        /// <summary>
        /// Reads 64 bits from a given offset. The offset is not checked. Use with caution
        /// </summary>
        /// <param name="offset">The offset to read from</param>
        /// <returns>64 bits of data read</returns>
        public abstract UInt64 Read64Unchecked( UInt64 offset );

        /// <summary>
        /// Writes 8 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write8(UInt32 offset, byte value);

        /// <summary>
        /// Writes 16 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write16(UInt32 offset, UInt16 value);

        /// <summary>
        /// Writes 32 bits from a given offset if it is within the valid range.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write32(UInt32 offset, UInt32 value);

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
        public abstract void Write8Unchecked(UInt32 offset, byte value);
        
        /// <summary>
        /// Writes 16 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write16Unchecked(UInt32 offset, UInt16 value);
        
        /// <summary>
        /// Writes 32 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write32Unchecked(UInt32 offset, UInt32 value);

        /// <summary>
        /// Writes 64 bits from a given offset. The offset is not checked. Use with caution.
        /// </summary>
        /// <param name="offset">The offset to write to</param>
        /// <param name="value">The data to write</param>
        public abstract void Write64Unchecked( UInt32 offset, UInt64 value ); 
    }

    public unsafe class MemoryAddressSpace : AddressSpace
    {
        public MemoryAddressSpace(UInt32 offset, UInt32 size) : base(offset, size)
        {   }

        public override byte Read8(UInt32 offset)
        {
            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(byte*)(this.Offset + offset);
        }
        public override UInt16 Read16(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt16*)(this.Offset + offset);
        }
        public override UInt32 Read32(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt32*)(this.Offset + offset);
        }

        public override UInt64 Read64( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return *( UInt64* )( this.Offset + offset );
        }

        public override byte Read8Unchecked(UInt32 offset)
        {
            return *(byte*)(this.Offset + offset);
        }
        public override UInt16 Read16Unchecked(UInt32 offset)
        {
            return *(UInt16*)(this.Offset + offset);
        }
        public override UInt32 Read32Unchecked(UInt32 offset)
        {
            return *(UInt32*)(this.Offset + offset);
        }

        public override UInt64 Read32Unchecked( UInt64 offset )
        {
            return *( UInt64* )( this.Offset + offset );
        }

        public override void Write8(UInt32 offset, byte value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset"); 
            (*(byte*)(this.Offset + offset)) = value;
        }
        public override void Write16(UInt32 offset, UInt16 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset"); 
            (*(UInt16*)(this.Offset + offset)) = value;
        }
        public override void Write32(UInt32 offset, UInt32 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset"); 
            (*(UInt32*)(this.Offset + offset)) = value;
        }
        public override void Write64( UInt32 offset, UInt64 value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            ( *( UInt64* )( this.Offset + offset ) ) = value;
        }        
        public override void Write8Unchecked(UInt32 offset, byte value)
        {
            (*(byte*)(this.Offset + offset)) = value;
        }
        public override void Write16Unchecked(UInt32 offset, UInt16 value)
        {
            (*(UInt16*)(this.Offset + offset)) = value;
        }
        public override void Write32Unchecked(UInt32 offset, UInt32 value)
        {
            (*(UInt32*)(this.Offset + offset)) = value;
        }
        public override void Write64Unchecked( UInt32 offset, UInt64 value )
        {
            ( *( UInt64* )( this.Offset + offset ) ) = value;
        }
    }

    public class IOAddressSpace : AddressSpace
    {
        public IOAddressSpace(UInt32 offset, UInt32 size)
            : base(offset, size)
        {
            if (offset > 0xffff || offset + size > 0xffff)
                throw new ArgumentOutOfRangeException("offset or size");
        }

        public override byte Read8(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return Kernel.CPUBus.Read8((UInt16)(this.Offset + offset));
        }
        public override UInt16 Read16(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return Kernel.CPUBus.Read16((UInt16)(this.Offset + offset));
        }
        public override UInt32 Read32(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return Kernel.CPUBus.Read32((UInt16)(this.Offset + offset));
        }
        public override UInt64 Read64( UInt32 offset )
        {
            throw new NotImplementedException(); 
        }

        public override byte Read8Unchecked(UInt32 offset)
        {
            return Kernel.CPUBus.Read8((UInt16)(this.Offset + offset));
        }
        public override UInt16 Read16Unchecked(UInt32 offset)
        {
            return Kernel.CPUBus.Read16((UInt16)(this.Offset + offset));
        }
        public override UInt32 Read32Unchecked(UInt32 offset)
        {
            return Kernel.CPUBus.Read32((UInt16)(this.Offset + offset));
        }
        public override UInt64 Read64Unchecked( UInt32 offset )
        {
            throw new NotImplementedException(); 
        }

        public override void Write8(UInt32 offset, byte value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            Kernel.CPUBus.Write8((UInt16)(this.Offset + offset), value);
        }
        public override void Write16(UInt32 offset, UInt16 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            Kernel.CPUBus.Write16((UInt16)(this.Offset + offset), value);
        }
        public override void Write32(UInt32 offset, UInt32 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            Kernel.CPUBus.Write32((UInt16)(this.Offset + offset), value);
        }
        public override void Write64( UInt32 offset, UInt64 value )
        {
            throw new NotImplementedException(); 
        }
        public override void Write8Unchecked(UInt32 offset, byte value)
        {
            Kernel.CPUBus.Write8((UInt16)(this.Offset + offset), value);
        }
        public override void Write16Unchecked(UInt32 offset, UInt16 value)
        {
            Kernel.CPUBus.Write16((UInt16)(this.Offset + offset), value);
        }
        public override void Write32Unchecked(UInt32 offset, UInt32 value)
        {
            Kernel.CPUBus.Write32((UInt16)(this.Offset + offset), value);
        }
        public override void Write64Unchecked( UInt32 offset, UInt64 value )
        {
            throw new NotImplementedException();
        }
    }
}
