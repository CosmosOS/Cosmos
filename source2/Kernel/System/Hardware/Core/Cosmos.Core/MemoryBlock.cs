using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using System.Linq;
using System.Text;

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
            Bytes = new MemoryBlock08(aBase, aSize * 4);
            Words = new MemoryBlock16(aBase, aSize * 2);
            DWords = new MemoryBlock32(aBase, aSize);
        }

        //TODO: Fill all these methods with fast ASM
        //TODO: Make an attribute that can be applied to methods to tell the copmiler to inline them to save
        // the overhead of a call on operations like this.
        // Need to check bounds for 16 and 32 better so offset cannot be size - 1 and then the 4 bytes write past the end
        public unsafe UInt32 this[UInt32 aOffset]
        {
            get
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(UInt32*)(Base + aOffset * 4);
            }
            set
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(UInt32*)(Base + aOffset * 4)) = value;
            }
        }

        public void Fill(UInt32 aData)
        {
            Fill(0, Size, aData);
        }

        public void Fill(UInt32 aStart, UInt32 aCount, UInt32 aData)
        {
            //TODO: before next step can at least check bounds here and do the addition just once to 
            //start the loop.
            //TODO - When asm can check count against size just one time and use a native fill asm op
            for (UInt32 i = aStart; i < aStart + aCount; i++)
            {
                this[i] = aData;
            }
        }

        public void MoveDown(UInt32 aDest, UInt32 aSrc, UInt32 aCount)
        {
            UInt32 xDest = aDest;
            UInt32 xSrc = aSrc;
            for (int i = 1; i <= aCount; i++)
            {
                this[xDest] = this[xSrc];
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

        public unsafe byte this[UInt32 aOffset]
        {
            get
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(byte*)(Base + aOffset);
            }
            set
            {
                if (aOffset >= Size)
                {
                    // Also this exception gets eaten?
                    throw new Exception("Memory access violation");
                }
                (*(byte*)(Base + aOffset)) = value;
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

        public unsafe UInt16 this[UInt32 aOffset]
        {
            get
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(UInt16*)(Base + aOffset * 2);
            }
            set
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(UInt16*)(Base + aOffset * 2)) = value;
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

        public unsafe UInt32 this[UInt32 aOffset]
        {
            get
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                return *(UInt32*)(Base + aOffset);
            }
            set
            {
                if (aOffset >= Size)
                {
                    throw new Exception("Memory access violation");
                }
                (*(UInt32*)(Base + aOffset)) = value;
            }
        }

    }
}
