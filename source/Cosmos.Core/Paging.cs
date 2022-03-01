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
            PML4 = (uint*)0xEEEEEEEE;// new ManagedMemoryBlock(0x1000, 4096).Offset;

            while ((uint)PML4 % 4096 != 0)
            {
                PML4++;
            }

            Map(0, 0);

            Map(0xB8000, 0xB8000);

            DoEnable((uint)PML4);
            var buf = (byte*)0xB8000;
            buf[0] = 0x4D;
            buf[1] = 4;
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
