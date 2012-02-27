////////////////////////////////////////
//  Add you name here when you change to add to this class
//
//  Class made by: Craig Lee Mark Adams
//  
//  


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.Managed_Memory_System
{

    // Some code come from Cosmos.Core.Heap. Use to give each processes private heap.  
    // Object are limit in size. This inforce object-oriented design, can be change at cost of add over-head when allocating and deallocating.

    class ProcessHeap
    {
        public bool EnableDebug = true;
        protected uint mStart;
        protected uint mStartAddress;
        protected uint mLength;
        protected uint mEndOfRam;
        protected List<ObjectMemory> memory = new List<ObjectMemory>();
        protected bool full = false;

        public ProcessHeap(uint aStartAddress, uint aEndOfRam, uint aObjectSizes)
        {
            DoInitialize(aStartAddress,aEndOfRam,aObjectSizes);
        }

        public void DoInitialize(uint aStartAddress, uint aEndOfRam, uint aObjectSizes)
        {
            mStart = mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
            mLength = aEndOfRam - aStartAddress;
            mLength = (mLength / 4) * 4;
            mEndOfRam = aEndOfRam;
            mStartAddress = (mStartAddress / 4) * 4;

            uint thisAddress = mStartAddress;

            while (thisAddress < mEndOfRam)
            {
                if ((thisAddress + aObjectSizes) > (mStart + mLength))
                {
                    memory.Add(new ObjectMemory(thisAddress, aEndOfRam,true)); //This allocation the lest bit of memory.
                }
                else
                {
                    memory.Add(new ObjectMemory(thisAddress, aObjectSizes,false));
                }

                thisAddress = thisAddress + aObjectSizes + 1; //space of 1 between memory areas  
            }

        }
        private void ClearMemory(uint aStartAddress, uint aLength)
        {
            //TODO: Move to memory. Internal access only...
            Global.CPU.ZeroFill(aStartAddress, aLength);
        }
        private void WriteNumber(uint aNumber, byte aBits)
        {
            uint xValue = aNumber;
            byte xCurrentBits = aBits;
            Console.Write("0x");
            while (xCurrentBits >= 4)
            {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit)
                {
                    case 0:
                        xDigitString = "0";
                        goto default;
                    case 1:
                        xDigitString = "1";
                        goto default;
                    case 2:
                        xDigitString = "2";
                        goto default;
                    case 3:
                        xDigitString = "3";
                        goto default;
                    case 4:
                        xDigitString = "4";
                        goto default;
                    case 5:
                        xDigitString = "5";
                        goto default;
                    case 6:
                        xDigitString = "6";
                        goto default;
                    case 7:
                        xDigitString = "7";
                        goto default;
                    case 8:
                        xDigitString = "8";
                        goto default;
                    case 9:
                        xDigitString = "9";
                        goto default;
                    case 10:
                        xDigitString = "A";
                        goto default;
                    case 11:
                        xDigitString = "B";
                        goto default;
                    case 12:
                        xDigitString = "C";
                        goto default;
                    case 13:
                        xDigitString = "D";
                        goto default;
                    case 14:
                        xDigitString = "E";
                        goto default;
                    case 15:
                        xDigitString = "F";
                        goto default;
                    default:
                        if (xDigitString == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        Console.Write(xDigitString);
                        break;
                }
            }
        }
        public uint MemAlloc(uint aLength)
        {
            bool found = false;
            uint allocationStartArea = 0; 

            for (int i = 0; i > (memory.Count - 1) || found == true ; i++)
            {
                if (memory[i].MemoryFull == false)
                {
                    if (memory[i].MemorySmall == false)
                    {
                        allocationStartArea = memory[i].ObjectStart;
                        found = true;
                        full = true; // Should be the lest bit of memory in this heap.
                    }
                    else
                    {
                        uint thisLength = memory[i].ObjectEndAddress - memory[i].ObjectStart;
                        thisLength = (thisLength / 4) * 4;
                        if (aLength <= thisLength)
                        {
                            allocationStartArea = memory[i].ObjectStart;
                            found = true; 
                        }
                    }

                }
                if (found == false)
                {
                    full = true; 
                    return 0;
                }
            }
            ClearMemory(allocationStartArea, aLength);
            return allocationStartArea;
        }
        public void DeleteObject(uint aStartAddress, uint aObjectSizes)
        {
            bool found = false;
            for (int i = 0; i > (memory.Count - 1) || found == true; i++)
            {
                if (memory[i].ObjectStart == aStartAddress)
                {
                    memory[i].MemoryFull = false;
                    found = true;
                    full = false;
                }
            }
            if (found == true)
            {
                ClearMemory(aStartAddress, aObjectSizes);
            }
        }
        public bool HeapFull
        {
            get
            {
                return full;
            }
            set
            {
                full = value;
            }
        }
        public uint HeapStart
        {
            get
            {
                return mStart;
            }
        }
        public uint HeapEnd
        {
            get
            {
                return mEndOfRam;
            }
        }
    }
}
