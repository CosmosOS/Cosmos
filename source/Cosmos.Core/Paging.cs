using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    public unsafe class Paging
    {
        internal static uint* PML4;
        public static void Enable()
        {
            PML4 = (uint*)new ManagedMemoryBlock(0x1000, 4096).Offset;
            for (uint i = 0; i < 0x1000; i++)
            {
                // This sets the following flags to the pages:
                //   Supervisor: Only kernel-mode can access them
                //   Write Enabled: It can be both read from and written to
                //   Not Present: The page table is not present


                PML4[i] = 0;
            }

            uint* PageTable = (uint*)new ManagedMemoryBlock(1024 * 4, 4096).Offset;

            //Map the 1st 4mb
            for (uint i = 0; i < 1024; i++)
            {
                // As the address is page aligned, it will always leave 12 bits zeroed.
                // Those bits are used by the attributes ;)
                PageTable[i] = (i * 0x1000) | 3; // attributes: supervisor level, read/write, present.
            }

            // attributes: supervisor level, read/write, present
            PML4[0] = ((uint)PageTable) | 3;

            uint* PageTable2 = (uint*)new ManagedMemoryBlock(1024 * 4, 4096).Offset;

            //Map the 1st 4mb
            for (uint i = 0; i < 1024; i++)
            {
                // As the address is page aligned, it will always leave 12 bits zeroed.
                // Those bits are used by the attributes ;)
                PageTable2[i] = (i * 0x1000) | 3; // attributes: supervisor level, read/write, present.
            }

            // attributes: supervisor level, read/write, present
            PML4[1] = ((uint)PageTable2) | 3;

            Map(0, 0);

            Map(0xB8000, 0xB8000);

            DoEnable((uint)PML4);
            while (true) { }
        }
        //plugged
        internal static void DoEnable(uint pagedir)
        {
            throw new NotImplementedException();
        }

        internal static void RefreshPages()
        {
            throw new NotImplementedException();
        }


        public static void Map(ulong PhysicalAddr, ulong virtualAddr)
        {
            var pdindex = (ulong)virtualAddr >> 22;
            var ptindex = (ulong)virtualAddr >> 12 & 0x03FF;

            var pageDir = (uint)PML4;
            var pageTable = (uint*)PML4 + (0x400 * pdindex);
            uint flags = 0;

            pageTable[ptindex] = (uint)((uint)PhysicalAddr | (flags & 0xFFF) | 0x01); // Present;

            RefreshPages();
        }
    }
}
