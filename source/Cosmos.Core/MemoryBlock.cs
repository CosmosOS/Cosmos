//#define COSMOSDEBUG
using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    public class MemoryBlock
    {
        public readonly uint Base;
        public readonly uint Size;

        public readonly MemoryBlock08 Bytes;
        public readonly MemoryBlock16 Words;
        public readonly MemoryBlock32 DWords;

        /// <summary>
        /// Create new inctanse of the <see cref="MemoryBlock"/> class.
        /// </summary>
        /// <param name="aBase">A base.</param>
        /// <param name="aSize">A size.</param>
        public MemoryBlock(uint aBase, uint aSize)
        {
            Base = aBase;
            Size = aSize;
            Bytes = new MemoryBlock08(aBase, aSize);
            Words = new MemoryBlock16(aBase, aSize);
            DWords = new MemoryBlock32(aBase, aSize);
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
        /// <param name="aStart">A start.</param>
        /// <param name="aCount">A count.</param>
        /// <param name="aData">A data.</param>
        [DebugStub(Off = true)]
        public unsafe void Fill(uint aStart, uint aCount, uint aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            uint* xDest = (uint*)(Base + aStart);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aStart">A starting position in the memory block.</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        [DebugStub(Off = true)]
        public unsafe void Fill(int aStart, int aCount, int aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest = (int*)(Base + aStart);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
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
        /// <param name="aStart">A starting position in the memory block.</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        [DebugStub(Off = true)]
        public unsafe void Fill(uint aStart, uint aCount, ushort aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            ushort* xDest = (ushort*)(Base + aStart);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        /// <summary>
        /// Fill data to memory block.
        /// </summary>
        /// <param name="aStart">A starting position in the memory block.</param>
        /// <param name="aCount">Data size.</param>
        /// <param name="aData">A data to fill memory block with.</param>
        [DebugStub(Off = true)]
        public unsafe void Fill(uint aStart, uint aCount, byte aData)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            byte* xDest = (byte*)(Base + aStart);
            MemoryOperations.Fill(xDest, aData, (int)aCount);
        }

        [DebugStub(Off = true)]
        public unsafe void Copy(uint[] aData)
        {
            Copy(0, aData, 0, aData.Length);
        }

        unsafe public void Copy(uint aStart, uint[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            uint* xDest = (uint*)(Base + aStart);

            Global.mDebugger.SendInternal($"Base is {Base} xDest is {(uint)xDest}");
            fixed (uint* aDataPtr = aData) {
                MemoryOperations.Copy(xDest, aDataPtr + aIndex, aCount);
            }
        }

        public void Copy(byte[] aData)
        {
            Copy(0, aData, 0, aData.Length);
        }

        public unsafe void Copy(int aStart, byte[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest = (int*)(Base + aStart);
            fixed (byte* aDataPtr = aData)
            {
                MemoryOperations.Copy((byte*)xDest, aDataPtr + aIndex, aCount);
            }
        }

        public void Copy(int[] aData)
        {
            Copy(0, aData, 0, aData.Length);
        }

        public void Copy(int []aData, int aIndex, int aCount)
        {
            Copy(0, aData, aIndex, aData.Length);
        }

        public unsafe void Copy(int aStart, int[] aData, int aIndex, int aCount)
        {
            // TODO thow exception if aStart and aCount are not in bound. I've tried to do this but Bochs dies :-(
            int* xDest = (int*)(Base + aStart);
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
            byte* xDest = (byte*)(Base);
            byte* aDataPtr = (byte*)block.Offset;

            MemoryOperations.Copy(xDest, aDataPtr, (int)block.Size);
        }

        /// <summary>
        /// Move bytes array down the memory block.
        /// </summary>
        /// <param name="aDest">Destination location.</param>
        /// <param name="aSrc">Sourcs location.</param>
        /// <param name="aCount">Number of bytes to move.</param>
        [DebugStub(Off = true)]
        public unsafe void MoveDown(uint aDest, uint aSrc, uint aCount)
        {
            byte* xDest = (byte*)(Base + aDest);
            byte* xSrc = (byte*)(Base + aSrc);
            MemoryOperations.Copy(xDest, xSrc, (int)aCount);
        }

        public void MoveUp(uint aDest, uint aSrc, uint aCount)
        {
            throw new Exception("TODO");
        }
        
        #region ReadWrite
        public unsafe void Read8(Byte[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
                aBuffer[i] = (*(Byte*)(Base + i));
        }

        public unsafe void Write8(Byte[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length; i++)
                (*(Byte*)(Base + i)) = aBuffer[i];
        }

        public unsafe void Read16(ushort[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length / 2; i++)
            {
                aBuffer[i] = (*(ushort*)(Base + i));
            }
        }

        public unsafe void Write16(ushort[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length / sizeof(ushort); i++)
            {
                (*(ushort*)(Base + i)) = aBuffer[i];
            }
        }

        public unsafe void Read32(uint[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length / sizeof(uint); i++)
                aBuffer[i] = (*(uint*)(Base + i));
        }

        public unsafe void Write32(uint[] aBuffer)
        {
            if(aBuffer.Length >= Size)
            {
                throw new Exception("Memory access violation");
            }
            for (int i = 0; i < aBuffer.Length / sizeof(uint); i++)
                (*(uint*)(Base + i)) = aBuffer[i];
        }
        #endregion ReadWrite
            
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
        /// Create new inctanse of the <see cref="MemoryBlock08"/> class.
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
                    // Also this exception gets eaten?
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
        /// Create new inctanse of the <see cref="MemoryBlock16"/> class.
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
        /// <param name="aByteOffset">A byte offset.</param>
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
        /// Create new inctanse of the <see cref="MemoryBlock32"/> class.
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
