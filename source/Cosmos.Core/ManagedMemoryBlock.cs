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
        public ulong Offset;
        /// <summary>
        /// Size.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Create a new buffer with the given size, not aligned
        /// </summary>
        /// <param name="byteSize">Size of buffer</param>
        public ManagedMemoryBlock(uint aByteCount) : this(aByteCount, 1, false)
        {
        }

        /// <summary>
        /// Create a new buffer with the given size, aligned on the byte boundary specified
        /// </summary>
        /// <param name="byteCount">Size of buffer</param>
        /// <param name="alignment">Byte Boundary alignment</param>
        public ManagedMemoryBlock(uint aByteCount, int alignment) : this(aByteCount, alignment, true)
        {
        }

        /// <summary>
        /// Create a new buffer with the given size, and aligned on the byte boundary if align is true
        /// </summary>
        /// <param name="aByteCount">Size of buffer</param>
        /// <param name="aAlignment">Byte Boundary alignment</param>
        /// <param name="aAlign">true if buffer should be aligned, false otherwise</param>
        public ManagedMemoryBlock(uint aByteCount, int aAlignment, bool aAlign)
        {
            memory = new byte[aByteCount + aAlignment - 1];
            fixed (byte* bodystart = memory)
            {
                Offset = (ulong)bodystart;
                Size = aByteCount;
            }
            if (aAlign == true)
            {
                while (Offset % (ulong)aAlignment != 0)
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
        /// <param name="aByteOffset">A start.</param>
        /// <param name="aCount">A count.</param>
        /// <param name="aData">A data.</param>
        public unsafe void Fill(uint aByteOffset, uint aCount, uint aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            uint* xDest = (uint*)(Offset + aByteOffset);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill memory block with integer value
        /// </summary>
        /// <param name="aByteOffset">A starting position in the memory block. This is integer indexing based</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        public unsafe void Fill(int aByteOffset, int aCount, int aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            fixed (byte* aArrayPtr = memory)
            {
                MemoryOperations.Fill(aArrayPtr + aByteOffset, aData, aCount * 4);
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
            byte* xDest = (byte*)Offset;
            byte* aDataPtr = (byte*)block.Base;

            MemoryOperations.Copy(xDest, aDataPtr, (int)block.Size);
        }

        /// <summary>
        /// Write 8-bit to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write8(uint aByteOffset, byte value)
        {
            if (aByteOffset < 0 || aByteOffset > Size)
                throw new ArgumentOutOfRangeException("offset");
            (*(byte*)(Offset + aByteOffset)) = value;
        }

        /// <summary>
        /// Read 16-bit from the memory block.
        /// </summary>
        /// <param name="aByteOffset">Data offset.</param>
        /// <returns>UInt16 value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size.</exception>
        public ushort Read16(uint aByteOffset)
        {
            if (aByteOffset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(aByteOffset));
            }

            return *(ushort*)(Offset + aByteOffset);
        }

        /// <summary>
        /// Write 16-bit to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write16(uint aByteOffset, ushort value)
        {
            if (aByteOffset < 0 || aByteOffset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(aByteOffset));
            }
            *(ushort*)(Offset + aByteOffset) = value;
        }

        /// <summary>
        /// Read 32-bit from the memory block.
        /// </summary>
        /// <param name="aByteOffset">Data offset.</param>
        /// <returns>UInt32 value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size.</exception>
        public uint Read32(uint aByteOffset)
        {
            if (aByteOffset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(aByteOffset));
            }

            return *(uint*)(Offset + aByteOffset);
        }

        /// <summary>
        /// Write 32-bit to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void Write32(uint aByteOffset, uint value)
        {
            if (aByteOffset < 0 || aByteOffset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(aByteOffset));
            }
            *(uint*)(Offset + aByteOffset) = value;
        }

        /// <summary>
        /// Write string to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Data offset.</param>
        /// <param name="value">Value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset if bigger than memory block size or smaller than 0.</exception>
        public void WriteString(uint aByteOffset, string value)
        {
            if (aByteOffset < 0 || aByteOffset > Size)
                throw new ArgumentOutOfRangeException("offset");
            if (value.Length + aByteOffset > Size)
                throw new ArgumentOutOfRangeException("value");
            for (int index = 0; index < value.Length; index++)
            {
                memory[aByteOffset + index] = (byte)value[index];
            }
        }
    }
}
