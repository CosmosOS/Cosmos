using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 

namespace Cosmos.Kernel
{
    public unsafe class MemoryAddressSpace : AddressSpace
    {
        public MemoryAddressSpace( UInt32 offset, UInt32 size )
            : base( offset, size )
        { }

        public override string ToString()
        {
            return "";// String.Concat("MemoryAddressSpace, offset = ", Offset.ToHex(), ", size = ", Size.ToHex());
        }

        public override byte Read8( UInt32 offset )
        {
            if( offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return *( byte* )( this.Offset + offset );
        }
        public override UInt16 Read16( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return *( UInt16* )( this.Offset + offset );
        }
        public override UInt32 Read32( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return *( UInt32* )( this.Offset + offset );
        }

        public override UInt64 Read64( UInt32 offset )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            return *( UInt64* )( this.Offset + offset );
        }

        public override byte Read8Unchecked( UInt32 offset )
        {
            return *( byte* )( this.Offset + offset );
        }
        public override UInt16 Read16Unchecked( UInt32 offset )
        {
            return *( UInt16* )( this.Offset + offset );
        }
        public override UInt32 Read32Unchecked( UInt32 offset )
        {
            return *( UInt32* )( this.Offset + offset );
        }

        public override UInt64 Read64Unchecked( UInt32 offset )
        {
            return *( UInt64* )( this.Offset + offset );
        }

        public override void Write8( UInt32 offset, byte value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            ( *( byte* )( this.Offset + offset ) ) = value;
        }
        public override void Write16( UInt32 offset, UInt16 value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            ( *( UInt16* )( this.Offset + offset ) ) = value;
        }
        public override void Write32( UInt32 offset, UInt32 value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            ( *( UInt32* )( this.Offset + offset ) ) = value;
        }
        public override void Write64( UInt32 offset, UInt64 value )
        {
            if( offset < 0 || offset > Size )
                throw new ArgumentOutOfRangeException( "offset" );
            ( *( UInt64* )( this.Offset + offset ) ) = value;
        }
        public override void Write8Unchecked( UInt32 offset, byte value )
        {
            ( *( byte* )( this.Offset + offset ) ) = value;
        }
        public override void Write16Unchecked( UInt32 offset, UInt16 value )
        {
            ( *( UInt16* )( this.Offset + offset ) ) = value;
        }
        public override void Write32Unchecked( UInt32 offset, UInt32 value )
        {
            ( *( UInt32* )( this.Offset + offset ) ) = value;
        }
        public override void Write64Unchecked( UInt32 offset, UInt64 value )
        {
            ( *( UInt64* )( this.Offset + offset ) ) = value;
        }

        public void CopyFrom( MemoryAddressSpace src )
        {
            for( uint x = 0; x < src.Size; x++ )
            {
                ( *( byte* )( this.Offset + x ) ) = *( byte* )( src.Offset + x );
            }
        }

        public void CopyFrom( MemoryAddressSpace src, uint srcOffset, uint dstOffset, uint bytes )
        {
            for( uint x = 0; x < bytes; x++ )
            {
                ( *( byte* )( this.Offset + dstOffset + x ) ) = *( byte* )( src.Offset + srcOffset + x );
            }
        }

        public void SetMem( byte data )
        {
            for( uint x = 0; x < this.Size; x++ )
            {
                ( *( byte* )( this.Offset + x ) ) = data;
            }
        }
    }
}
