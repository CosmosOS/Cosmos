#define OLD_HEAP
// BE CAREFUL: enabling/disabling the OLD_HEAP define must happen in HMI.cs as well!

#if !OLD_HEAP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Common;

namespace Cosmos.Core
{
    //Coded by: Forest201

    //Heap Memory Interface
    //Should be Started after Boot
    public static class HMI
    {
        public static uint UsedMemory = 0;
        public static uint FreeMemory = 0;
        public static uint TotalMemory = 0;
        public static uint BlockReused = 0;  
        public static uint MemUsed = 0;

        internal static int BlockID = 0;
        internal static int StartID = 0;
        internal static uint LeftOver = 0;
        internal static int MaxID = 0;
        internal static int GCIndex = 0;
        internal static int GCDebugID = 0;
        internal static int GCObjCount = 0;
        internal static int GCRefObjID = 0;
        internal static uint ProtectArea = 0;

        //Resizable Variable
        //You can increase the size of these variables below if you like to suit your needs
        internal static int[] GCObjIDs = new int[256];
        internal static uint[] MemBases = new uint[256];
        internal static uint[] MemSizes = new uint[256];
        internal static uint[] MemHeaders = new uint[256];
        //Resizable Variables stops here


        internal static bool IsInitalised = false;
        internal static bool Monitor = false;
        internal static bool GCEnable = false;
        internal static uint MallocAddr = 0;
        internal static uint GCSavedAddr = 0;
        internal static uint GCNewAddr = 0;
        internal static uint JediReallocAddr = 0;
        internal static uint MaxSize = 0;
        internal static uint MemoryPointer = 0;

     

        public unsafe static void HandleGPF(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            Console.Clear();

            //Causes Include
            //Using SIMD instruction on unligned memory
            //Invalid Segment

            Console.WriteLine("[ Kernel Crashed: General Fault Protection ]", 1);

            uint* Stack = (uint*)aContext.ESP;
            uint ErrorCode = *Stack;

            if ((ErrorCode & 0x1) == 0x1)
            {
                Console.Write("External In ");
                if ((ErrorCode & 6) == 0x0)
                {
                    Console.Write("GDT -> ");
                }
                else if ((ErrorCode & 6) == 0x2)
                {
                    Console.Write("IDT -> ");
                }
                else if ((ErrorCode & 6) == 0x4)
                {
                    Console.Write("LDT -> ");
                }
                else if ((ErrorCode & 6) == 0x6)
                {
                    Console.Write("IDT -> ");
                }
                uint Err = ErrorCode & 0xFFF8;
                Console.WriteLine(Err.ToHex());
            }
            else
            {
                Console.Write("Internal In ");
                if ((ErrorCode & 6) == 0x0)
                {
                    Console.Write("GDT -> ");
                }
                else if ((ErrorCode & 6) == 0x2)
                {
                    Console.Write("IDT -> ");
                }
                else if ((ErrorCode & 6) == 0x4)
                {
                    Console.Write("LDT -> ");
                }
                else if ((ErrorCode & 6) == 0x6)
                {
                    Console.Write("IDT -> ");
                }
                uint Err = ErrorCode & 0xFFF8;
                Console.WriteLine(Err.ToHex());
            }


            while (true) ;
        }



        public static bool Init()
        {
            Cosmos.Core.INTs.GeneralProtectionFault = new Cosmos.Core.INTs.IRQDelegate(HandleGPF);
            ProtectArea = Cosmos.Core.CPU.GetEndOfKernel();

            TotalMemory = ((Cosmos.Core.CPU.GetAmountOfRAM() - 1) * 1048576);
            FreeMemory = TotalMemory - Cosmos.Core.CPU.GetEndOfKernel();
            UsedMemory = TotalMemory - FreeMemory;
            IsInitalised = true;
            MemUsed = 0;

            for (int i = 0; i < 256; i++)
            {
                MemBases[i] = 0x0000000;
                MemSizes[i] = 0x0000000;
                MemHeaders[i] = 0x00000000;
                GCObjIDs[i] = 0x0000000;
            }
            return IsInitalised;

        }
        //Update Memory Consumption variables
        public static void PrintMemoryUsed()
        {
            Console.WriteLine("Memory Used : " + MaxSize.ToString());
        }

        //Update Memory Consumption variables
        public static void Update()
        {
            UsedMemory = Cosmos.Core.Heap.GetMemoryUse();
            FreeMemory = TotalMemory - UsedMemory;
        }

        //Causes a Heap Memory Fault
        public static void HeapExcessUseFault()
        {
            Console.Clear();
            Console.WriteLine(" FAULT IN KERNEL HEAP : ILLEGAL MEMORY CONSUMPTION DETECTED ");
            while (true) ;
        }

        //Causes a Heap Fault
        public static void CauseProgramFault(string aFault)
        {
            Console.Clear();
            Console.WriteLine(aFault);
            while (true) ;
        }

        public static void HeapOutOfMemory()
        {
            Console.Clear();
            Console.WriteLine("FAULT IN KERNEL HEAP : OUT OF MEMORY ", 12);
            while (true) ;
        }

        //Monitors Code/Data consumption
        public static void GCMonitor()
        {
            MaxSize = 0;
            GCIndex = 0;
            GCRefObjID = 0;
            GCSavedAddr = Heap.GetMemoryUse();
            BlockReused = GCSavedAddr;
            Monitor = true;
            MemoryPointer = Heap.LastMallocAddr;

        }

        //Free All Objects in the GC List.
        public static void GCFreeAll()
        {
            GCNewAddr = Heap.GetMemoryUse();

            for (int u = 0; u < GCObjCount; u++)
            {
                GCIndex = GCObjIDs[u];
                MemHeaders[GCIndex] = 0x10000000;
            }
            GCEnable = true;

        }

        //
        public static uint MemAlloc(uint aSize)
        {
            MallocAddr = 0;
            if (IsInitalised == false)
            {
                Console.WriteLine("HMI Re-allocator not initialised.", 10);
            }
            else
            {
                MallocAddr = ReAlloc(aSize);
            }
            if (MallocAddr == 0)
            {
                MallocAddr = Heap.MemAlloc(aSize);
                AddBlock(MallocAddr, aSize);
                UsedMemory += aSize;
            }
            return MallocAddr;
        }


        //Reallocates And/Or Resizes Memory

        public static uint ReAlloc(uint aSize)
        {
            JediReallocAddr = 0;
            if (IsInitalised == true)
            {
                JediReallocate(aSize);   // Use Jedi Reallocate if it is Enable
                NormalReallocate(aSize); // If the Garbage Collector is disabled Use normal reallocate
                CheckForErrors(aSize);   // Check for Errors
            }
            return JediReallocAddr;
        }



        //Jedi Reallocator only
        //It is only used by Garbage Collector
        public static void JediReallocate(uint aSize)
        {
            //Use the Force Anican
            LeftOver = 0;
            if (GCEnable == true & GCObjCount > 0)
            {
                JediReallocAddr = MemoryPointer;

                MemoryPointer += aSize;

                GCIndex = GCObjIDs[GCRefObjID];

                MaxSize += aSize;

                if (aSize < MemSizes[GCIndex])
                {
                    //Don't do anything. Just update MaxSize
                    MaxSize -= LeftOver;//
                }
                else if (aSize > MemSizes[GCIndex])
                {
                    LeftOver = aSize - MemSizes[GCIndex];
                    Heap.SkipRalloc = true;//Skip Realloc for one inertia
                    Heap.MemAlloc(LeftOver);//Allocate the missing size 
                    MemSizes[GCIndex] += LeftOver;
                    MaxSize += LeftOver;
                }

                MemUsed = MaxSize;
                GCRefObjID++;
            }
        }


        //Reallocate Data that is freed by FreeMem(uint aSize) function

        public static void NormalReallocate(uint aSize)
        { //Normal Realloc
            if (JediReallocAddr == 0)
            {
                for (uint i = 0; i < BlockID; i++)
                {
                    if (MemHeaders[i] == 0x10)
                    {
                        JediReallocAddr = MemBases[i];
                        BlockReused = JediReallocAddr;
                        MemHeaders[i] = 0xF0000000;

                        if (MemSizes[i] == aSize)
                        {
                            Cosmos.Core.Global.CPU.ZeroFill(JediReallocAddr, MemSizes[GCIndex]);
                            GCDebugID = 0x0005;
                            break;
                        }
                        else if (MemSizes[i] > aSize)
                        {
                            SplitBlock(i, aSize);
                            Cosmos.Core.Global.CPU.ZeroFill(JediReallocAddr, MemSizes[GCIndex]);
                            GCDebugID = 0x0006;
                            break;
                        }
                        else if (aSize > MemSizes[i])
                        {
                            if (MemBases[i] == Heap.LastMallocAddr)
                            {
                                LeftOver = aSize - MemSizes[i];
                                MemSizes[i] = aSize;
                                Heap.SkipRalloc = true;//Skip Ralloc for one inertia
                                Heap.MemAlloc(LeftOver);//Allocate the missing size
                                Cosmos.Core.Global.CPU.ZeroFill(JediReallocAddr, MemSizes[GCIndex]);
                                StartID = GCIndex + 1;
                                MaxID = GCObjCount - StartID;

                                if (StartID < GCObjCount)
                                {
                                    for (int p = StartID; p < MaxID; p++)
                                    {
                                        MemBases[p] += LeftOver;
                                    }
                                }
                                GCDebugID = 0x0007;
                            }
                        }
                    }
                }
            }

        }

        public static void CheckForErrors(uint aSize)
        {

            //Check if address is Zero
            if (JediReallocAddr == 0 & GCEnable == true & GCObjCount > 0)
            {
                Console.Clear();
                Console.Write("Debug ID: ");
                Console.WriteLine(GCDebugID.ToHex());

                Console.Write("Index ID: ");
                Console.WriteLine(GCIndex.ToHex());

                Console.Write("GCMax ID: ");
                Console.WriteLine(GCObjIDs[(GCObjCount - 1)].ToHex());

                Console.WriteLine("Realloc Error: Base Memory Address is Zero");
                Console.Write("Requested Memory: ");
                Console.WriteLine(aSize.ToString());

                if (GCObjCount == 0)
                {
                    Console.WriteLine("No objects in GC List.");
                }

                int aIndex = GCObjIDs[GCIndex];
                uint DataSize = MemSizes[aIndex];
                Console.Write("Block# ");
                Console.Write(aIndex.ToString() + ": ");
                Console.WriteLine(DataSize.ToString());
                while (true) { };
            }
            //Cause a Heap Fault if an object illegally accesses Kernel Memory Area
            else if (JediReallocAddr != 0 & JediReallocAddr < ProtectArea & GCEnable == true)
            {

                HMI.CauseProgramFault("[ HMI Error: Kernel Memory Access Violation ]");
                while (true) ;
            }
        }

        //Add a Block to Manage Memory
        public static void AddBlock(uint DAddr, uint dataSize)
        {
            MemBases[BlockID] = DAddr;
            MemSizes[BlockID] = dataSize;
            MemHeaders[BlockID] = 0xF0000000;
            if (Monitor == true)
            {
                GCObjIDs[GCObjCount] = BlockID;
                GCObjCount++;
            }
            BlockID = BlockID + 1;
        }

        //Split a memory block in two 
        public static void SplitBlock(uint id, uint usedsize)
        {
            uint newblocksize = 0;
            newblocksize = MemSizes[id] - usedsize;

            //Split the block into the required size for whatever requested it 
            MemSizes[id] = MemSizes[id] - newblocksize;

            //Register the remaider of the block as a new unused block
            MemBases[BlockID] = MemBases[id] + MemSizes[id];
            MemSizes[BlockID] = newblocksize;

            BlockID++;
        }

        //Free memory
        //Jedi Reallocate doesn't use it. 
        public static bool FreeMem(uint addr)
        {
            bool Status = false;
            for (int i = 0; i < BlockID; i++)
            {
                if (MemBases[i] == addr)
                {
                    MemHeaders[i] = 0x10000000;
                    UsedMemory -= MemSizes[i]; //Decrease Used memory
                    FreeMemory += MemSizes[i]; //Increase free memory
                    Cosmos.Core.Global.CPU.ZeroFill(addr, MemSizes[i]);
                    Status = true;
                    break;
                }
            }
            return Status;
        }


    

    }
}
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Common;

namespace Cosmos.Core
{
    //Coded by: Forest201

    //Heap Memory Interface
    //Should be Started after Boot
    public static class HMI
    {
        // dummy implementation
        public static void GCMonitor()
        {

        }

        public static void GCFreeAll()
        {

        }
    }
}

#endif