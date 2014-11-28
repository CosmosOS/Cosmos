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

    // Some code come from Cosmos.Core.Heap.

    // Paging should be made later.
    // The process should not know the different between page and real memory. This would stop process heap from not braking. 


    public static class ManagedMemory
    {
        public static bool EnableDebug = true;
        private static uint mStart;
        private static uint mStartAddress;
        private static uint mLength;
        private static uint mEndOfRam;

        private static List<ObjectMemory> memoryAreas = new List<ObjectMemory>();
        private static List<ObjectMemory> kernalMemoryArea = new List<ObjectMemory>();


        private static uint mAmountHeapAlloc = 500;     //set the amount of memory that can be allocate in one go.

        private static uint memoryAreaSize = 500;

        private static uint kernalOjectAreaSize = 5;

        private static uint memorySetUpAreaEnd;   //Area that is use for memory set up objects.
                                                     
        private static uint rootKernalEnd;      //Use to say where object can be made to for process to work.

        private static void DoInitialize(uint aStartAddress, uint aEndOfRam)
        {
            mStart = mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
            mLength = aEndOfRam - aStartAddress;
            mLength = (mLength / 4) * 4; 
            mStartAddress += 1024;
            mEndOfRam = aEndOfRam;
            mStartAddress = (mStartAddress / 4) * 4;
            mLength -= 1024;
            memorySetUpAreaEnd = mStartAddress + 5; // My need changing
            rootKernalEnd = rootKernalEnd + memorySetUpAreaEnd;
            
            UpdateDebugDisplay();
        }

        private static bool mInitialized = false;
        internal static void Initialize()
        {
            if (!mInitialized)
            {
                mInitialized = true;
                DoInitialize(CPU.GetEndOfKernel(), (CPU.GetAmountOfRAM() - 1) * 1024 * 1024);
            }
        }

        private static void ClearMemory(uint aStartAddress, uint aLength)
        {
            //TODO: Move to memory. Internal access only...
            Global.CPU.ZeroFill(aStartAddress, aLength);
        }

        private static void WriteNumber(uint aNumber, byte aBits)
        {
            uint xValue = aNumber;
            byte xCurrentBits = aBits;
            Console.Write("0x");
            while (xCurrentBits >= 4) {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit) {
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
                        if (xDigitString == null) {
                            System.Diagnostics.Debugger.Break();
                        }
                        Console.Write(xDigitString);
                        break;
                }
            }
        }

        private static bool mDebugDisplayInitialized = false;

        // This method displays the used/total memory of the heap on the first line of the text screen
        private static void UpdateDebugDisplay()
        {
            //if (EnableDebug)
            //{
            //    if (!mDebugDisplayInitialized)
            //    {
            //        mDebugDisplayInitialized = true;
            //        int xOldPositionLeft = Console.CursorLeft;
            //        int xOldPositionTop = Console.CursorTop;
            //        Console.CursorLeft = 0;
            //        Console.CursorTop = 0;
            //        Console.Write("[Heap Usage: ");
            //        WriteNumber(mStartAddress,
            //                    32);
            //        Console.Write("/");
            //        WriteNumber(mEndOfRam,
            //                    32);
            //        Console.Write("] bytes");
            //        while (Console.CursorLeft < (Console.WindowWidth-1))
            //        {
            //            Console.Write(" ");
            //        }
            //        Console.CursorLeft = xOldPositionLeft;
            //        Console.CursorTop = xOldPositionTop;
            //    }
            //    else
            //    {
            //        int xOldPositionLeft = Console.CursorLeft;
            //        int xOldPositionTop = Console.CursorTop;
            //        Console.CursorLeft = 13;
            //        Console.CursorTop = 0;
            //        WriteNumber(mStartAddress,
            //                    32);
            //        Console.CursorLeft = xOldPositionLeft;
            //        Console.CursorTop = xOldPositionTop;
            //    }
            //}
        }
        
        // This should get call after initualize is done 
        public static void SetUpMemoryArea()
        {
            uint temp = memorySetUpAreaEnd + 1; //space of 1 between memory areas 

            while (temp < rootKernalEnd)
            {
                if ((temp + memoryAreaSize) > (mStart + mLength))
                {
                    kernalMemoryArea.Add(new ObjectMemory(temp, rootKernalEnd, true));
                }
                else
                {
                    kernalMemoryArea.Add(new ObjectMemory(temp, kernalOjectAreaSize, false));
                }
                temp = temp + memoryAreaSize + 1; //space of 1 between memory areas 
            }

            temp = rootKernalEnd + 1; //space of 1 between memory areas 
            while (temp < mEndOfRam)
            {
                if ((temp + memoryAreaSize) > (mStart + mLength))
                {
                    memoryAreas.Add(new ObjectMemory(temp, mLength, true)); //This allocation the lest bit of memory.
                }
                else
                {
                    memoryAreas.Add(new ObjectMemory(temp, memoryAreaSize, false));
                }

                temp = temp + memoryAreaSize + 1; //space of 1 between memory areas 
            }


        }
         
        public static uint SetUpMemoryAlloc(uint aLength)
        {
            Initialize();
            uint xTemp = mStartAddress;

            if ((xTemp + aLength) > (mStart + memorySetUpAreaEnd))
            {
                Console.WriteLine("Too large memory block allocated!");
                return 0; //error 
            }
            mStartAddress += aLength;
            UpdateDebugDisplay();
            ClearMemory(xTemp, aLength);
            return xTemp;
        }

        // Allocation objects that is need for process to work 
        public static uint KernelMemAlloc(uint aLength)
            {
                bool found = false;
                uint allocationStartArea = 0;

                for (int i = 0; i > (kernalMemoryArea.Count - 1) || found == true; i++)
                {
                    if (kernalMemoryArea[i].MemoryFull == false)
                    {
                        uint thisLength = kernalMemoryArea[i].ObjectEndAddress - kernalMemoryArea[i].ObjectStart;
                        thisLength = (thisLength / 4) * 4;
                        if (aLength <= thisLength)
                        {
                            allocationStartArea = kernalMemoryArea[i].ObjectStart;
                            kernalMemoryArea[i].MemoryFull = true;
                            found = true;
                        }
                    }
                }
                if (found == true)
                {
                    ClearMemory(allocationStartArea, aLength);
                }
                return allocationStartArea;
            }

        public static uint HeapAlloc()
        {
            bool found = false;
            uint allocationStartArea = 0;

            for (int i = 0; i > (memoryAreas.Count - 1) || found == true; i++)
            {
                if (memoryAreas[i].MemoryFull == false)
                {
                    if (memoryAreas[i].MemorySmall == false)
                    {
                        allocationStartArea = memoryAreas[i].ObjectStart;
                        memoryAreas[i].MemoryFull = true;
                        found = true;
                    }
                }
            }
            if (found == true)
            {
                ClearMemory(allocationStartArea, mAmountHeapAlloc);
            }
            return allocationStartArea;
            
        }

        public unsafe static uint* StackAlloc(uint stackSize)
        {
            bool found = false;
            uint* allocationStartArea = stackalloc uint[2]; //pointer is use so that objects are not put on the heap.
            allocationStartArea[0] = 0;
            allocationStartArea[1] = 0;

            for (int i = 0; i > (memoryAreas.Count - 1) || found == true; i++)
            {
                if (memoryAreas[i].MemoryFull == false)
                {
                    if (memoryAreas[i].MemorySmall == false)
                    {
                        if (stackSize <= memoryAreaSize)
                        {

                            allocationStartArea[0] = memoryAreas[i].ObjectStart;
                            allocationStartArea[1] = memoryAreas[i].ObjectEndAddress;
                            memoryAreas[i].MemoryFull = true;
                            found = true;
                        }
                        else
                        {
                            bool thisFound = false;

                            for (int stacks = 1; thisFound == true; stacks++)
                            {
                                if (memoryAreas[i + stacks].MemoryFull == false)
                                {
                                    if (stackSize <= (memoryAreaSize * (stacks + 1)))
                                    {
                                        allocationStartArea[0] = memoryAreas[i].ObjectStart;
                                        allocationStartArea[1] = memoryAreas[i + stacks].ObjectEndAddress;
                                        for (int eachMemoryArea = i; eachMemoryArea > stacks; eachMemoryArea++)
                                        {
                                            memoryAreas[eachMemoryArea].MemoryFull = true;
                                        }


                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                         uint thisLength = memoryAreas[i].ObjectEndAddress - memoryAreas[i].ObjectStart;
                         thisLength = (thisLength / 4) * 4;
                         if (stackSize < thisLength)
                         {

                             allocationStartArea[0] = memoryAreas[i].ObjectStart;
                             allocationStartArea[1] = memoryAreas[i].ObjectEndAddress;
                             memoryAreas[i].MemoryFull = true;
                             found = true;
                         }
                    }
                }
            }
            if (found == true)
            {
                ClearMemory(allocationStartArea[0], allocationStartArea[1]);
            }
            return allocationStartArea;

        }

        public static void KernalMemDeallocate(uint address)
        {
            bool found = false;
            for (int i = 0; i > (kernalMemoryArea.Count - 1) || found == true; i++)
            {
                if (kernalMemoryArea[i].ObjectStart < address && kernalMemoryArea[i].ObjectEndAddress > address)
                {
                    kernalMemoryArea[i].MemoryFull = false;
                    found = true;
                }
            }
        }

        public static void MemDeallocate(uint startAddress, uint endAddress)
        {
            bool found = false;
            for (int i = 0; i > (memoryAreas.Count - 1) || found == true; i++)
            {
                if (memoryAreas[i].ObjectStart == startAddress)
                {
                    if (memoryAreas[i].ObjectEndAddress >= endAddress)
                    {
                        memoryAreas[i].MemoryFull = false;
                    }
                    else
                    {
                        bool thisFound = false;
                        for (int stacks = 1; thisFound == true; stacks++)
                        {
                            if (endAddress <= memoryAreas[i + stacks].ObjectEndAddress)
                            {
                                memoryAreas[i].MemoryFull = false;
                                thisFound = true;
                            }
                            else
                            {
                                memoryAreas[i].MemoryFull = false;
                            }
                        }
                    }
                    kernalMemoryArea[i].MemoryFull = false;
                    found = true;
                }
            }
        }

    }
}
