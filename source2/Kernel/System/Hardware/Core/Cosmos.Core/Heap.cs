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
                xAddr = HMI.RAlloc(aLength);
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