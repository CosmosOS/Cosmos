﻿#define OLD_HEAP
// BE CAREFUL: enabling/disabling the OLD_HEAP define must happen in HMI.cs as well!

#if !OLD_HEAP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    // This class must be static, as for creating objects, we needd the hea
    public static class Heap
    {
        public static bool EnableDebug = true;  
        //GC variables

        public static bool SkipRalloc = true;      
        public static uint LastMallocAddr;
        

        private static uint mStartAddress;
        private static uint mEndOfRam;
        internal static uint xAddr;
        internal static bool IsInitalised = false;

        public static uint GetMemoryUse()
        {
           return mStartAddress;
        }

        public static void DecreaseMemoryUse(uint aSize)
        {

            mStartAddress -= aSize;
            if (mStartAddress < HMI.ProtectArea)
            {
                HMI.CauseProgramFault("[ Heap Error: Memory Access Violation ]");
            }
            else
            {
                LastMallocAddr = mStartAddress;
            }
        }


        internal static void Initialize()
        {
            if (IsInitalised == false)
            {
                mEndOfRam = ((Cosmos.Core.CPU.GetAmountOfRAM() - 1) * 1048576);
                mStartAddress = Cosmos.Core.CPU.GetEndOfKernel();
                mStartAddress += 1024; // leave at 1024 bytes between  kernel and User space;
                IsInitalised = true;
            }
        }

        public static uint MemAlloc(uint aLength)
        {
            Initialize();
            xAddr = 0;

            if (SkipRalloc == false)
            {
                xAddr = HMI.ReAlloc(aLength);
            }

            if (xAddr == 0)
            {
                xAddr = mStartAddress;
             
                if (mStartAddress >= mEndOfRam)
                {
                    HMI.HeapOutOfMemory();
                }

                else
                {
                    mStartAddress += aLength;
                    Cosmos.Core.Global.CPU.ZeroFill(xAddr, aLength);
                    if (HMI.IsInitalised == true)//Prevent it from interfering with unmanaged memory before boot
                    {
                        HMI.AddBlock(xAddr, aLength);
                    }
                    if (SkipRalloc == false)
                    {
                        LastMallocAddr = xAddr;//Update this when we allocate a new block not extend an old block
                    }
                    SkipRalloc = false; 
                   
                }
          
            }
            
            return xAddr;
        }

        public static void xHeapExcessUseFault()
        {
            HMI.HeapExcessUseFault();
        }

    }
}
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core
{
    // This class must be static, as for creating objects, we needd the hea
    public static class Heap
    {
        public static bool EnableDebug = true;
        private static uint mStart;
        private static uint mStartAddress;
        private static uint mLength;
        private static uint mEndOfRam;
        
        private static void DoInitialize(uint aStartAddress, uint aEndOfRam)
        {
            mStart = mStartAddress = aStartAddress + (4 - (aStartAddress % 4));
            mLength = aEndOfRam - aStartAddress;
            mLength = (mLength / 4) * 4;
            ClearMemory(aStartAddress, mLength);
           
            mStartAddress += 1024;
            mEndOfRam = aEndOfRam;
            mStartAddress = (mStartAddress / 4) * 4;
            mLength -= 1024;
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
            CPU.ZeroFill(aStartAddress, aLength);
        }

        private static void WriteNumber(uint aNumber, byte aBits)
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

        //private static bool mDebugDisplayInitialized = false;

        // this method displays the used/total memory of the heap on the first line of the text screen
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

        public static uint MemAlloc(uint aLength)
        {
            Initialize();
            uint xTemp = mStartAddress;

            if ((xTemp + aLength) > (mStart + mLength))
            {
                Console.WriteLine("Too large memory block allocated!");
                WriteNumber(aLength, 32);
                while (true)
                    ;
            }
            mStartAddress += aLength;
            UpdateDebugDisplay();
            ClearMemory(xTemp, aLength);
            return xTemp;
        }
    }
}
#endif