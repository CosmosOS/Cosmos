//#define COSMOSDEBUG
using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    /// <summary>
    /// MemoryBlock class. Used to read and write to memory blocks.
    /// </summary>
    public class MemoryBlock
    {
        /// <summary>
        /// Memory block base address.
        /// </summary>
        public readonly uint Base;
        /// <summary>
        /// Memory block size.
        /// </summary>
        public readonly uint Size;

        /// <summary>
        /// Bytes memory block.
        /// </summary>
        public readonly MemoryBlock08 Bytes;
        /// <summary>
        /// Words memory block.
        /// </summary>
        public readonly MemoryBlock16 Words;
        /// <summary>
        /// DWords memory block.
        /// </summary>
        public readonly MemoryBlock32 DWords;

        /// <summary>
        /// Create new instance of the <see cref="MemoryBlock"/> class.
        /// </summary>
        /// <param name="aBase">A base.</param>
        /// <param name="aByteSize">A size.</param>
        public MemoryBlock(uint aBase, uint aByteSize)
        {
            Base = aBase;
            Size = aByteSize;
            Bytes = new MemoryBlock08(aBase, aByteSize);
            Words = new MemoryBlock16(aBase, aByteSize);
            DWords = new MemoryBlock32(aBase, aByteSize);
        }

        //TODO: Fill all these methods with fast ASM
        //TODO: Make an attribute that can be applied to methods to tell the copmiler to inline them to save
        // the overhead of a call on operations like this.
        // Need to check bounds for 16 and 32 better so offset cannot be size - 1 and then the 4 bytes write past the end
        /// <summary>
        /// Get and set memory block.
        /// </summary>
        /// <param name="aByteOffset">A byte offset.</param>
        /// <returns>uint value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe uint this[uint aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(uint*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(uint*)(Base + aByteOffset)) = value;
            }
        }

        /// <summary>
        /// Fill memory block.
        /// </summary>
        /// <param name="aData">A data to fill.</param>
        public void Fill(uint aData)
        {
            //Fill(0, Size / 4, aData);
            Fill(0, Size, aData);
        }

        /// <summary>
        /// Fill memory block.
        /// </summary>
        /// <param name="aByteOffset">A start.</param>
        /// <param name="aCount">A count.</param>
        /// <param name="aData">A data.</param>
        [global::IL2CPU.API.Attribs.DebugStub(Off = true)]
        public unsafe void Fill(uint aByteOffset, uint aCount, uint aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            uint* xDest = (uint*)(Base + aByteOffset);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill memory block with a value. The filling is integer aligned
        /// </summary>
        /// <param name="aByteOffset">Byte offset from Base</param>
        /// <param name="aCount">Number of integers to fill</param>
        /// <param name="aData">Value to fill memory block with</param>
        [global::IL2CPU.API.Attribs.DebugStub(Off = true)]
        public unsafe void Fill(int aByteOffset, int aCount, int aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest = (int*)(Base + aByteOffset);
            MemoryOperations.Fill(xDest, aData, aCount);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aData">Data to fill the memory block with.</param>
        public void Fill(byte aData)
        {
            Fill(0, Size, aData);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aData">Data to fill the memory block with.</param>
        public void Fill(ushort aData)
        {
            Fill(0, Size, aData);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aByteOffset">Starting point offset in bytes</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        [global::IL2CPU.API.Attribs.DebugStub(Off = true)]
        public unsafe void Fill(uint aByteOffset, uint aCount, ushort aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            ushort* xDest = (ushort*)(Base + aByteOffset);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aByteOffset">Starting point offset in bytes</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        [global::IL2CPU.API.Attribs.DebugStub(Off = true)]
        public unsafe void Fill(uint aByteOffset, uint aCount, byte aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            byte* xDest = (byte*)(Base + aByteOffset);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Copy data from the buffer to the start of the memory block.
        /// </summary>
        /// <param name="aData">A data buffer array.</param>
        /// <exception cref="OverflowException">Thrown if aData length in greater then Int32.MaxValue.</exception>
        [global::IL2CPU.API.Attribs.DebugStub(Off = true)]
        public unsafe void Copy(uint[] aData)
        {
            Copy(0, aData, 0, aData.Length);
        }

        /// <summary>
        /// Copy data from the buffer array to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Starting point offset in bytes</param>
        /// <param name="aData">A data buffer array.</param>
        /// <param name="aIndex">A staring index in the source data buffer array.</param>
        /// <param name="aCount">Number of bytes to copy.</param>
        unsafe public void Copy(uint aByteOffset, uint[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            uint* xDest = (uint*)(Base + aByteOffset);

            fixed (uint* aDataPtr = aData) {
                MemoryOperations.Copy(xDest, aDataPtr + aIndex, aCount);
            }
        }

        /// <summary>
        /// Copy data from the buffer to the start of the memory block.
        /// </summary>
        /// <param name="aData">A data buffer array.</param>
        /// <exception cref="OverflowException">Thrown if aData length in greater then Int32.MaxValue.</exception>
        public void Copy(byte[] aData)
        {
            Copy(0, aData, 0, aData.Length);
        }

        /// <summary>
        /// Copy data from the buffer array to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Starting point offset in bytes</param>
        /// <param name="aData">A data buffer array.</param>
        /// <param name="aIndex">A staring index in the source data buffer array.</param>
        /// <param name="aCount">Number of bytes to copy.</param>
        public unsafe void Copy(int aByteOffset, byte[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest = (int*)(Base + aByteOffset);
            fixed (byte* aDataPtr = aData)
            {
                MemoryOperations.Copy((byte*)xDest, aDataPtr + aIndex, aCount);
            }
        }

        /// <summary>
        /// Copy data from the buffer to the start of the memory block.
        /// </summary>
        /// <param name="aData">A data buffer array.</param>
        /// <exception cref="OverflowException">Thrown if aData length in greater then Int32.MaxValue.</exception>
        public void Copy(int[] aData)
        {
            Copy(0, aData, 0, aData.Length);
        }

        /// <summary>
        /// Copy data from the buffer array to the start of the memory block.
        /// </summary>
        /// <param name="aData">A data buffer array.</param>
        /// <param name="aIndex">A staring index in the source data buffer array.</param>
        /// <param name="aCount">Number of integers to copy.</param>
        /// <exception cref="OverflowException">Thrown if aData length in greater then Int32.MaxValue.</exception>
        public void Copy(int[] aData, int aIndex, int aCount)
        {
            if(aData.Length < aCount)
            {
                throw new IndexOutOfRangeException();
            }
            Copy(0, aData, aIndex, aCount);
        }

        /// <summary>
        /// Copy data from the buffer array to the memory block.
        /// </summary>
        /// <param name="aByteOffset">Starting point offset in bytes</param>
        /// <param name="aData">A data buffer array.</param>
        /// <param name="aIndex">A staring index in the source data buffer array.</param>
        /// <param name="aCount">Number of integers to copy.</param>
        public unsafe void Copy(int aByteOffset, int[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest = (int*)(Base + aByteOffset);
            fixed (int* aDataPtr = aData)
            {
                MemoryOperations.Copy(xDest, aDataPtr + aIndex, aCount);
            }
        }

        /// <summary>
        /// Copy ManagedMemoryBlock into MemoryBlock
        /// </summary>
        /// <param name="block">ManagedMemoryBlock to copy.</param>
        public unsafe void Copy(ManagedMemoryBlock block)
        {
            byte* xDest = (byte*)Base;
            byte* aDataPtr = (byte*)block.Offset;

            MemoryOperations.Copy(xDest, aDataPtr, (int)block.Size);
        }

        /// <summary>
        /// Move bytes array down the memory block.
        /// </summary>
        /// <param name="aDest">Destination location.</param>
        /// <param name="aSrc">Sourcs location.</param>
        /// <param name="aCount">Number of bytes to move.</param>
        [global::IL2CPU.API.Attribs.DebugStub(Off = true)]
        public unsafe void MoveDown(uint aDest, uint aSrc, uint aCount)
        {
            byte* xDest = (byte*)(Base + aDest);
            byte* xSrc = (byte*)(Base + aSrc);
            MemoryOperations.Copy(xDest, xSrc, (int)aCount);
        }

        /// <summary>
        /// Move data inside the block. Undone.
        /// </summary>
        /// <remarks>Always throw. Yet to be done.</remarks>
        /// <param name="aDest">A destination address.</param>
        /// <param name="aSrc">A source address.</param>
        /// <param name="aCount">Number of bytes to move.</param>
        /// <exception cref="Exception">Thrown always.</exception>
        public void MoveUp(uint aDest, uint aSrc, uint aCount)
        {
            throw new Exception("TODO");
        }

        #region ReadWrite
        /// <summary>
        /// Read 8-bit from the memory block.
        /// </summary>
        /// <param name="aBuffer">A buffer to write the data to.</param>
        /// <exception cref="OverflowException">Thrown if aBuffer length in greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe void Read8(byte[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
            {
                aBuffer[i] = *((byte*)Base + i);
            }
        }

        /// <summary>
        /// Write 8-bit to the memory block.
        /// </summary>
        /// <param name="aBuffer">A buffer to be written to the memory block.</param>
        /// <exception cref="OverflowException">Thrown if aBuffer length in greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe void Write8(byte[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
            {
                *((byte*)Base + i) = aBuffer[i];
            }
        }

        /// <summary>
        /// Read 16-bit from the memory block.
        /// </summary>
        /// <param name="aBuffer">A buffer to write the data to.</param>
        /// <exception cref="OverflowException">Thrown if aBuffer length in greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe void Read16(ushort[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
            {
                aBuffer[i] = *((ushort*)Base + i);
            }
        }

        /// <summary>
        /// Write 16-bit to the memory block.
        /// </summary>
        /// <param name="aBuffer">A buffer to be written to the memory block.</param>
        /// <exception cref="OverflowException">Thrown if aBuffer length in greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe void Write16(ushort[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
            {
                *((ushort*)Base + i) = aBuffer[i];
            }
        }

        /// <summary>
        /// Read 32-bit from the memory block.
        /// </summary>
        /// <param name="aBuffer">A buffer to write the data to.</param>
        /// <exception cref="OverflowException">Thrown if aBuffer length in greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe void Read32(uint[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
            {
                aBuffer[i] = *((uint*)Base + i);
            }
        }

        /// <summary>
        /// Write 32-bit to the memory block.
        /// </summary>
        /// <param name="aBuffer">A buffer to be written to the memory block.</param>
        /// <exception cref="OverflowException">Thrown if aBuffer length in greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe void Write32(uint[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
            {
                *((uint*)Base + i) = aBuffer[i];
            }
        }
        #endregion ReadWrite

        /// <summary>
        /// Convert part for the memory block to array.
        /// </summary>
        /// <param name="aStart">A starting position of the data at the source memory block.</param>
        /// <param name="aIndex">A index to be the staring index at the destination array.</param>
        /// <param name="aCount">Number of bytes to get.</param>
        /// <returns>uint array.</returns>
        public unsafe uint[] ToArray(int aStart, int aIndex, int aCount)
        {
            uint* xDest = (uint*)(Base + aStart);
            uint[] array = new uint[aCount];
            fixed (uint* aArrayPtr = array)
            {
                MemoryOperations.Copy(aArrayPtr + aIndex, xDest, aCount);
            }
            return array;
        }

        /// <summary>
        /// Convert the memory block to array.
        /// </summary>
        /// <returns>uint array.</returns>
        public uint[] ToArray()
        {
            return ToArray(0, 0, (int)Size);
        }

    }

    /// <summary>
    /// MemoryBlock08 class.
    /// </summary>
    public class MemoryBlock08
    {
        /// <summary>
        /// Base.
        /// </summary>
        public readonly uint Base;
        /// <summary>
        /// Size.
        /// </summary>
        public readonly uint Size;

        /// <summary>
        /// Create new instance of the <see cref="MemoryBlock08"/> class.
        /// </summary>
        /// <param name="aBase">A base.</param>
        /// <param name="aSize">A size.</param>
        internal MemoryBlock08(uint aBase, uint aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        /// <summary>
        /// Get and set memory block.
        /// </summary>
        /// <param name="aByteOffset">A byte offset.</param>
        /// <returns>byte value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe byte this[uint aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(byte*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(byte*)(Base + aByteOffset)) = value;
            }
        }
    }

    /// <summary>
    /// MemoryBlock16 class.
    /// </summary>
    public class MemoryBlock16
    {
        /// <summary>
        /// Base.
        /// </summary>
        public readonly uint Base;
        /// <summary>
        /// Size.
        /// </summary>
        public readonly uint Size;

        /// <summary>
        /// Create new instance of the <see cref="MemoryBlock16"/> class.
        /// </summary>
        /// <param name="aBase">A base.</param>
        /// <param name="aSize">A size.</param>
        internal MemoryBlock16(uint aBase, uint aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        /// <summary>
        /// Get and set memory block.
        /// </summary>
        /// <param name="aByteOffset">Byte offset from start of memory block</param>
        /// <returns>ushort value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe ushort this[uint aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(ushort*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(ushort*)(Base + aByteOffset)) = value;
            }
        }

    }

    /// <summary>
    /// MemoryBlock32 class.
    /// </summary>
    public class MemoryBlock32
    {
        /// <summary>
        /// Base.
        /// </summary>
        public readonly uint Base;
        /// <summary>
        /// Size.
        /// </summary>
        public readonly uint Size;

        /// <summary>
        /// Create new instance of the <see cref="MemoryBlock32"/> class.
        /// </summary>
        /// <param name="aBase">A base.</param>
        /// <param name="aSize">A size.</param>
        internal MemoryBlock32(uint aBase, uint aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        /// <summary>
        /// Get and set memory block.
        /// </summary>
        /// <param name="aByteOffset">A byte offset.</param>
        /// <returns>uint value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public unsafe uint this[uint aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(uint*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(uint*)(Base + aByteOffset)) = value;
            }
        }
    }
}
