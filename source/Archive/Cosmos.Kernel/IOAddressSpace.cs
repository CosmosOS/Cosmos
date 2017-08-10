using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 

namespace Cosmos.Kernel
{
    public class IOAddressSpace : AddressSpace
    {
        public IOAddressSpace( UInt32 offset, UInt32 size )
            : base( offset, size )
        {
            if( offset > 0xffff || offset + size > 0xffff )
                throw new ArgumentOutOfRangeException( "offset or size" );
        }

        public override string ToString()
        {
            return ""; // String.Concat("IOAddressSpace, offset = ", Offset.ToHex(), ", size = ", Size.ToHex());
        }

        public override byte Read8( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return Kernel.CPUBus.Read8( ( UInt16 )( this.Offset + offset ) );
        }
        public override UInt16 Read16( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return Kernel.CPUBus.Read16( ( UInt16 )( this.Offset + offset ) );
        }
        public override UInt32 Read32( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return Kernel.CPUBus.Read32( ( UInt16 )( this.Offset + offset ) );
        }
        public override UInt64 Read64( UInt32 offset )
        {
            throw new NotImplementedException();
        }

        public override byte Read8Unchecked( UInt32 offset )
        {
            return Kernel.CPUBus.Read8( ( UInt16 )( this.Offset + offset ) );
        }
        public override UInt16 Read16Unchecked( UInt32 offset )
        {
            return Kernel.CPUBus.Read16( ( UInt16 )( this.Offset + offset ) );
        }
        public override UInt32 Read32Unchecked( UInt32 offset )
        {
            return Kernel.CPUBus.Read32( ( UInt16 )( this.Offset + offset ) );
        }
        public override UInt64 Read64Unchecked( UInt32 offset )
        {
            throw new NotImplementedException();
        }

        public override void Write8( UInt32 offset, byte value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            Kernel.CPUBus.Write8( ( UInt16 )( this.Offset + offset ), value );
        }
        public override void Write16( UInt32 offset, UInt16 value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            Kernel.CPUBus.Write16( ( UInt16 )( this.Offset + offset ), value );
        }
        public override void Write32( UInt32 offset, UInt32 value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            Kernel.CPUBus.Write32( ( UInt16 )( this.Offset + offset ), value );
        }
        public override void Write64( UInt32 offset, UInt64 value )
        {
            throw new NotImplementedException();
        }
        public override void Write8Unchecked( UInt32 offset, byte value )
        {
            Kernel.CPUBus.Write8( ( UInt16 )( this.Offset + offset ), value );
        }
        public override void Write16Unchecked( UInt32 offset, UInt16 value )
        {
            Kernel.CPUBus.Write16( ( UInt16 )( this.Offset + offset ), value );
        }
        public override void Write32Unchecked( UInt32 offset, UInt32 value )
        {
            Kernel.CPUBus.Write32( ( UInt16 )( this.Offset + offset ), value );
        }
        public override void Write64Unchecked( UInt32 offset, UInt64 value )
        {
            throw new NotImplementedException();
        }
    }
}
