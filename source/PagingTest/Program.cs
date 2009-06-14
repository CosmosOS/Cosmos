using System;

using Cosmos.Kernel;
using Cosmos.Sys;

using Cosmos.Compiler.Builder;

namespace CosmosBoot
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        #endregion

        // Main entry point of the kernel
        public static void Init()
        {
            new Boot().Execute();

            unsafe
            {
                //Align With Page Boundry
                uint CurAlign = Heap.MemAlloc(0);
                uint NewAlign = ((CurAlign + ((uint)0x1000)) & ((uint)0xFFFFF000));
                Heap.MemAlloc(NewAlign - CurAlign);

                //Allocate Space for PageDefinitionTable and 2 PageTables
                uint aPDT = Heap.MemAlloc(4096);
                uint aPGT = Heap.MemAlloc(4096);
                uint aPGT2 = Heap.MemAlloc(4096);

                //Setup Pointers
                PageDirectoryEntry* PDT = (PageDirectoryEntry*)aPDT;
                PageTableEntry* PGT = (PageTableEntry*)aPGT;
                PageTableEntry* PGT2 = (PageTableEntry*)aPGT2;

                //Init first entry of PageDefinitionTable to point to PageTable
                PDT[0].PDEValue = 0;
                PDT[0].PageTableAddress = aPGT;
                PDT[0].Present = true;


                //Init second entry of PageDefinitionTable to point to PageTable 2
                PDT[1].PDEValue = 0;
                PDT[1].PageTableAddress = aPGT2;
                PDT[1].Present = true;


                //Identity Map 0x00000000 through EndOfHeap (assume smaller than 4MB)
                uint EndOfHeap = Heap.MemAlloc(0);
                int PGTindx = 0;
                uint Addr = 0;
                for (;Addr < EndOfHeap;Addr += 0x1000)
                {
                    PGT[PGTindx].PTEValue = 0;
                    PGT[PGTindx].PhysicalAddress = Addr;
                    PGT[PGTindx].Present = true;
                    PGTindx++;
                }

                uint aPHYSa = Heap.MemAlloc(4096);
                byte* PHYSa = (byte*)aPHYSa;
                for (uint i = 0;i < 4096;i += 4)
                {
                    PHYSa[i] = 0xAA;
                    PHYSa[i + 1] = 0xAA;
                    PHYSa[i + 2] = 0xC0;
                    PHYSa[i + 3] = 0xDE;
                }
                uint aPHYSb = Heap.MemAlloc(4096);
                byte* PHYSb = (byte*)aPHYSb;
                for (uint i = 0;i < 4096;i += 4)
                {
                    PHYSb[i] = 0xBB;
                    PHYSb[i + 1] = 0xBB;
                    PHYSb[i + 2] = 0xC0;
                    PHYSb[i + 3] = 0xDE;
                }

                //Map aPHYSb to where aPHYSa normally is.
                PGT[PGTindx].PTEValue = 0;
                PGT[PGTindx].PhysicalAddress = aPHYSb;
                PGT[PGTindx].Present = true;

                //Map aPHYSa to where aPHYSb normally is.
                PGTindx++;
                PGT[PGTindx].PTEValue = 0;
                PGT[PGTindx].PhysicalAddress = aPHYSa;
                PGT[PGTindx].Present = true;
                PGTindx++;

                //Identity Map up to 4MB
                for (Addr = Heap.MemAlloc(0);Addr < 4194304;Addr += 0x1000)
                {
                    PGT[PGTindx].PTEValue = 0;
                    PGT[PGTindx].PhysicalAddress = Addr;
                    PGT[PGTindx].Present = true;
                    PGTindx++;
                }

                //Identity up to 8MB
                for (int PGT2indx = 0;Addr < 8388608;Addr += 0x1000)
                {
                    PGT2[PGT2indx].PTEValue = 0;
                    PGT2[PGT2indx].PhysicalAddress = Addr;
                    PGT2[PGT2indx].Present = true;
                    PGT2indx++;
                }

                //Debug Stuff
                Console.WriteLine("Amount Of Memory:       " + (CPU.AmountOfMemory * 1024 * 1024).ToHex());
                Console.WriteLine("Amount Of Paged Memory: " + (Addr + 0x1000).ToHex());
                Console.WriteLine();
                Console.WriteLine("Page Definition Table @ " + aPDT.ToHex());
                Console.WriteLine("Page Table #1   (4KB) @ " + aPGT.ToHex());
                Console.WriteLine("Page Table #2   (4KB) @ " + aPGT2.ToHex());
                Console.WriteLine();
                Console.WriteLine("Memory Block A   <-B] @ " + aPHYSa.ToHex());
                Console.WriteLine("Memory Block B   <-A] @ " + aPHYSb.ToHex());
                Console.WriteLine();
                Console.WriteLine("Press Any Key to View Pre-Paging Memory Layout ...");
                Console.Read();
                Cosmos.Debug.Debugger.ViewMemory();

                Console.WriteLine("Press any key to set page directory...");
                Console.Read();
                PagingUtility.SetPageDirectory(aPDT);

                Console.WriteLine("Press any key to enable paging...");
                Console.Read();
                PagingUtility.EnablePaging();
                Console.WriteLine();
                Console.WriteLine("Paging Enabled!");
            }

            Console.WriteLine("Press Any Key to View Post-Paging Memory Layout ...");
            Console.Read();

            Cosmos.Debug.Debugger.ViewMemory();
            Deboot.Reboot();
        }
    }
}