using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common;

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
            //UpdateDebugDisplay();
        }

        private static bool mInitialized = false;
        internal static void Initialize()
        {
            if (!mInitialized)
            {
                mInitialized = true;
                DoInitialize(CPU.GetEndOfKernel(), (CPU.GetAmountOfRAM() - 1) * 1024 * 1024);
                //DoInitialize(4 * 1024 * 1024, 16 * 1024 * 1024);
            }
        }

        private static void ClearMemory(uint aStartAddress, uint aLength)
        {
            //TODO: Move to memory. Internal access only...
            CPU.ZeroFill(aStartAddress, aLength);
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
            //        NumberHelper.WriteNumber(mStartAddress,
            //                    32);
            //        Console.Write("/");
            //        NumberHelper.WriteNumber(mEndOfRam,
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
            //        NumberHelper.WriteNumber(mStartAddress,
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
                NumberHelper.WriteNumber(aLength, 32);
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
