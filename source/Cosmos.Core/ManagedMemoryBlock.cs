using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    /// <summary>
    /// ManagedMemoryBlock class. Used to read and write a managed memory block.
    /// </summary>
    public unsafe class ManagedMemoryBlock
    {
        public byte[] memory;

        /// <summary>
        /// Offset.
        /// </summary>
        public uint Offset;
        /// <summary>
        /// Size.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Create a new buffer with the given size, not aligned
        /// </summary>
        /// <param name="size">Size of buffer</param>
        public ManagedMemoryBlock(uint size)
            : this(size, 1, false)
        { }

        /// <summary>
        /// Create a new buffer with the given size, aligned on the byte boundary specified
        /// </summary>
        /// <param name="size">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        public ManagedMemoryBlock(uint size, int alignment)
            : this(size, alignment, true)
        { }

        /// <summary>
        /// Create a new buffer with the given size, and aligned on the byte boundary if align is true
        /// </summary>
        /// <param name="aSize">Size of buffer</param>
        /// <param name="aAlignment">Byte Boundary alignment</param>
        /// <param name="aAlign">true if buffer should be aligned, false otherwise</param>
        public ManagedMemoryBlock(uint aSize, int aAlignment, bool aAlign)
        {
            memory = new byte[aSize + aAlignment - 1];
            fixed (byte* bodystart = memory)
            {
                Offset = (uint)bodystart;
                Size = aSize;
            }
            if (aAlign == true)
            {
                while (Offset % aAlignment != 0)
                {
                    Offset++;
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
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                return *((byte*)Offset + offset);
            }
            set
            {
                if (offset < 0 || offset > Size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                *((byte*)Offset + offset) = value;
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
            uint* xDest = (uint*)Offset + aStart;
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill memory block with integer value
        /// </summary>
        /// <param name="aStart">A starting position in the memory block. This is integer indexing based</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        public unsafe void Fill(int aStart, int aCount, int aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            fixed (byte* aArrayPtr = memory)
            {
                Debugger.DoSendNumber((uint)aArrayPtr);
                MemoryOperations.Fill(aArrayPtr + 4 * aStart, aData, aCount * 4);
            }
        }

        /// <summary>
        /// Fill memory block.
        /// </summary>
        /// <param name="aData">A data to fill.</param>
        public void Fill(uint aData)
        {
            fixed (byte* destPtr = memory)
            {
                MemoryOperations.Fill(destPtr, (int)aData, (int)Size);
            }
        }

        public unsafe void Copy(int aStart, byte[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            byte* xDest;
            fixed (byte* aArrayPtr = this.memory)
            {
                xDest = (byte*)(aArrayPtr + aStart);
            }
            fixed (byte* aDataPtr = aData)
            {
                MemoryOperations.Copy(xDest, aDataPtr + aIndex, aCount);
            }
        }

        public unsafe void Copy(int aStart, int[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest;
            fixed (byte* aArrayPtr = memory)
            {
                xDest = (int*)aArrayPtr + aStart;
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
            byte* xDest = (byte*)(Offset);
            byte* aDataPtr = (byte*)block.Base;

            MemoryOperations.Copy(xDest, aDataPtr, (int)block.Size);
        }

        /// <summary>
        /// Write 8-bit to the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write8(uint offset, byte value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            (*(byte*)(this.Offset + offset)) = value;
        }

        /// <summary>
        /// Read 16-bit from the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <returns>UInt16 value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size.</exception>
        public ushort Read16(uint offset)
        {
            if (offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return *((ushort*)Offset + offset);
        }

        /// <summary>
        /// Write 16-bit to the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write16(uint offset, ushort value)
        {
            if (offset < 0 || offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            *((ushort*)Offset + offset) = value;
        }

        /// <summary>
        /// Read 32-bit from the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <returns>UInt32 value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size.</exception>
        public uint Read32(uint offset)
        {
            if (offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return *((uint*)Offset + offset);
        }

        /// <summary>
        /// Write 32-bit to the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write32(uint offset, uint value)
        {
            if (offset < 0 || offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            *((uint*)Offset + offset) = value;
        }

        /// <summary>
        /// Write string to the memory block.
        /// </summary>
        /// <param name="offset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void WriteString(uint offset, string value)
        {
            if (offset < 0 || offset > Size)
                throw new ArgumentOutOfRangeException("offset");
            if (value.Length + offset > Size)
                throw new ArgumentOutOfRangeException("value");
            for (int index = 0; index < value.Length; index++)
            {
                memory[offset + index] = (byte)value[index];
            }
        }
    }
}
