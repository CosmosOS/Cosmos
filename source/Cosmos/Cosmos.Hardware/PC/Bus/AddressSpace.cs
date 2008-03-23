using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus
{
    public  abstract class AddressSpace
    {
        public UInt32 Offset;
        public UInt32 Size;

        public AddressSpace(UInt32 offset, UInt32 size)
        {
            this.Offset = offset;
            this.Size = size;
        }
    }

    public unsafe class MemoryAddressSpace : AddressSpace
    {
        public MemoryAddressSpace(UInt32 offset, UInt32 size)
            : base(offset, size)
        {
        }

        public byte Read8(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(byte*)(this.Offset + offset);
        }

        public UInt16 Read16(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt16*)(this.Offset + offset);
        }

        public UInt32 Read32(UInt32 offset)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt32*)(this.Offset + offset);
        }

        public byte Read8Unchecked(UInt32 offset)
        {
            return *(byte*)(this.Offset + offset);
        }

        public UInt16 Read16Unchecked(UInt32 offset)
        {
            return *(UInt16*)(this.Offset + offset);
        }

        public UInt32 Read32Unchecked(UInt32 offset)
        {
            return *(UInt32*)(this.Offset + offset);
        }

        public void Write8(UInt32 offset, byte value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset"); 
            (*(byte*)(this.Offset + offset)) = value;
        }

        public void Write16(UInt32 offset, UInt16 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset"); 
            (*(UInt16*)(this.Offset + offset)) = value;
        }

        public void Write32(UInt32 offset, UInt32 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset"); 
            (*(UInt32*)(this.Offset + offset)) = value;
        }

        public void Write8Unchecked(UInt32 offset, byte value)
        {
            (*(byte*)(this.Offset + offset)) = value;
        }

        public void Write16Unchecked(UInt32 offset, UInt16 value)
        {
            (*(UInt16*)(this.Offset + offset)) = value;
        }

        public void Write32Unchecked(UInt32 offset, UInt32 value)
        {
            (*(UInt32*)(this.Offset + offset)) = value;
        }
    }

    public class IOAddressSpace : AddressSpace
    {
        public IOAddressSpace(UInt32 offset, UInt32 size)
            : base(offset, size)
        {
        }
    }
}
