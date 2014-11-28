using System;
using System.Collections.Generic;
using Cosmos.Common;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core
{
    public class MemoryBlock
    {
        public readonly UInt32 Base;
        public readonly UInt32 Size;

        public readonly MemoryBlock08 Bytes;
        public readonly MemoryBlock16 Words;
        public readonly MemoryBlock32 DWords;

        public MemoryBlock(UInt32 aBase, UInt32 aSize)
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
        public unsafe UInt32 this[UInt32 aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(UInt32*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(UInt32*)(Base + aByteOffset)) = value;
            }
        }

        public void Fill(UInt32 aData)
        {
            Fill(0, Size / 4, aData);
        }

        [DebugStub(Off = true)]
        public unsafe void Fill(UInt32 aStart, UInt32 aCount, UInt32 aData)
        {
            //TODO: before next step can at least check bounds here and do the addition just once to 
            //start the loop.
            //TODO - When asm can check count against size just one time and use a native fill asm op
            UInt32* xDest = (UInt32*)(this.Base + aStart);
            for (UInt32 i = 0; i < aCount; i++)
            {
                *xDest = aData;
                xDest++;
            }
        }
        public void Fill(byte aData)
        {
            Fill(0, Size, aData);
        }

        [DebugStub(Off = true)]
        public unsafe void Fill(UInt32 aStart, UInt32 aCount, byte aData)
        {
            //TODO: before next step can at least check bounds here and do the addition just once to 
            //start the loop.
            //TODO - When asm can check count against size just one time and use a native fill asm op
            byte* xDest = (byte*)(this.Base + aStart);
            for (UInt32 i = 0; i < aCount; i++)
            {
                *xDest = aData;
                xDest++;
            }
        }

        [DebugStub(Off = true)]
        public unsafe void MoveDown(UInt32 aDest, UInt32 aSrc, UInt32 aCount)
        {
            byte* xDest = (byte*)(this.Base + aDest);
            byte* xSrc = (byte*)(this.Base + aSrc);
            for (int i = 0; i < aCount; i++)
            {
                *xDest = *xSrc;
                xDest++;
                xSrc++;
            }
        }

        public void MoveUp(UInt32 aDest, UInt32 aSrc, UInt32 aCount)
        {
            throw new Exception("TODO");
        }
    }

    public class MemoryBlock08
    {
        public readonly UInt32 Base;
        public readonly UInt32 Size;

        internal MemoryBlock08(UInt32 aBase, UInt32 aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        public unsafe byte this[UInt32 aByteOffset]
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

    public class MemoryBlock16
    {
        public readonly UInt32 Base;
        public readonly UInt32 Size;

        internal MemoryBlock16(UInt32 aBase, UInt32 aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        public unsafe UInt16 this[UInt32 aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(UInt16*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(UInt16*)(Base + aByteOffset)) = value;
            }
        }

    }

    public class MemoryBlock32
    {
        public readonly UInt32 Base;
        public readonly UInt32 Size;

        internal MemoryBlock32(UInt32 aBase, UInt32 aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        public unsafe UInt32 this[UInt32 aByteOffset]
        {
            get
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(UInt32*)(Base + aByteOffset);
            }
            set
            {
                if (aByteOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(UInt32*)(Base + aByteOffset)) = value;
            }
        }

    }
}
