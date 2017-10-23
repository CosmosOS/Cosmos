using System;

namespace Cosmos.Core
{
    public unsafe class ManagedMemoryBlock
    {
        private byte[] memory;

        public UInt32 Offset;
        public UInt32 Size;

        /// <summary>
        /// Create a new buffer with the given size, not aligned
        /// </summary>
        /// <param name="size">Size of buffer</param>
        public ManagedMemoryBlock(UInt32 size)
            : this(size, 1, false)
        { }

        /// <summary>
        /// Create a new buffer with the given size, aligned on the byte boundary specified
        /// </summary>
        /// <param name="size">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        public ManagedMemoryBlock(UInt32 size, byte alignment)
            : this(size, alignment, true)
        { }

        /// <summary>
        /// Create a new buffer with the given size, and aligned on the byte boundary if align is true
        /// </summary>
        /// <param name="size">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        /// <param name="align">true if buffer should be aligned, false otherwise</param>
        public ManagedMemoryBlock(UInt32 size, byte alignment, bool align)
        {
            memory = new byte[size + alignment - 1];
            fixed (byte* bodystart = memory)
            {
                Offset = (UInt32)bodystart;
                Size = size;
            }
            if (align == true)
            {
                while (this.Offset % alignment != 0)
                {
                    this.Offset++;
                }
            }
        }

        /// <summary>
        /// Get or set the byte at the given offset
        /// </summary>
        /// <param name="offset">Address Offset</param>
        /// <returns>Byte value at given offset</returns>
        public byte this[uint offset]
        {
            get
            {
                if (offset > Size)
                    throw new ArgumentOutOfRangeException("offset");
                return *(byte*)(this.Offset + offset);
            }
            set
            {
                if (offset < 0 || offset > Size)
                    throw new ArgumentOutOfRangeException("offset");
                (*(byte*)(this.Offset + offset)) = value;
            }
        }
        public UInt16 Read16(uint offset)
        {
            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt16*)(this.Offset + offset);
        }

        public void Write16(uint offset, UInt16 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            (*(UInt16*)(this.Offset + offset)) = value;
        }
        public UInt32 Read32(uint offset)
        {
            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt32*)(this.Offset + offset);
        }
        public void Write32(uint offset, UInt32 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            (*(UInt32*)(this.Offset + offset)) = value;
        }
    }
}
