using System;

namespace Cosmos.Core
{
    /// <summary>
    /// ManagedMemoryBlock class. Used to read and write a managed memory block.
    /// </summary>
    public unsafe class ManagedMemoryBlock
    {
        private byte[] memory;

        /// <summary>
        /// Offset.
        /// </summary>
        public UInt32 Offset;
        /// <summary>
        /// Size.
        /// </summary>
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
        public ManagedMemoryBlock(UInt32 size, int alignment)
            : this(size, alignment, true)
        { }

        /// <summary>
        /// Create a new buffer with the given size, and aligned on the byte boundary if align is true
        /// </summary>
        /// <param name="size">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        /// <param name="align">true if buffer should be aligned, false otherwise</param>
        public ManagedMemoryBlock(UInt32 size, int alignment, bool align)
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid offset.</exception>
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

        /// <summary>
        /// Fill memory block.
        /// </summary>
        /// <param name="aStart">A start.</param>
        /// <param name="aCount">A count.</param>
        /// <param name="aData">A data.</param>
        public unsafe void Fill(uint aStart, uint aCount, uint aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            uint* xDest = (uint*)(this.Offset + aStart);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aStart">A starting position in the memory block.</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        public unsafe void Fill(int aStart, int aCount, int aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            fixed (byte* aArrayPtr = this.memory)
            {
                MemoryOperations.Fill(aArrayPtr + aStart, aData, (int)aCount);
            }
        }

        /// <summary>
        /// Fill memory block.
        /// </summary>
        /// <param name="aData">A data to fill.</param>
        public void Fill(uint aData)
        {
            fixed (byte* destPtr = this.memory)
            {
                MemoryOperations.Fill(destPtr, (int)aData, (int)this.Size);
            }
        }

        public unsafe void Copy(int aStart, int[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest;
            fixed (byte* aArrayPtr = this.memory)
            {
                xDest = (int*)(aArrayPtr + aStart);
            }
            fixed (int* aDataPtr = aData)
            {
                MemoryOperations.Copy(xDest, aDataPtr + aIndex, aCount);
            }
        }

        /// <summary>
        /// Copy MemoryBlock into ManagedMemoryBlock
        /// </summary>
        /// <param name="block">MemoryBlock to copy.</param>
        public unsafe void Copy(MemoryBlock block)
        {
            byte* xDest = (byte*)(this.Offset);
            byte* aDataPtr = (byte*)block.Base;

            MemoryOperations.Copy(xDest, aDataPtr, (int)block.Size);
        }

        /// <summary>
        /// Read 16-bit from the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <returns>UInt16 value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size.</exception>
        public UInt16 Read16(uint offset)
        {
            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt16*)(this.Offset + offset);
        }

        /// <summary>
        /// Write 16-bit to the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write16(uint offset, UInt16 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            (*(UInt16*)(this.Offset + offset)) = value;
        }

        /// <summary>
        /// Read 32-bit from the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <returns>UInt32 value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size.</exception>
        public UInt32 Read32(uint offset)
        {
            if (offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            return *(UInt32*)(this.Offset + offset);
        }

        /// <summary>
        /// Write 32-bit to the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write32(uint offset, UInt32 value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            (*(UInt32*)(this.Offset + offset)) = value;
        }
    }
}
