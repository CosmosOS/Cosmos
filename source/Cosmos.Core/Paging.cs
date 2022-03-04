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
        internal static ulong* PageDirPtrTable;
        public static void Enable()
        {

            //test
            var buf = (byte*)0xB8000;
            buf[0] = (byte)'A';
            buf[1] = 0x0f;

            //4 page directory entries
            PageDirPtrTable = (ulong*)0xDEADBF00;// new ManagedMemoryBlock(32, 0x20).Offset;

            for (int i = 0; i < 3; i++)
            {
                var PageDir = (ulong*)Align((ulong)(0xDEADBF00 + 32 + (i * 5)), 0x1000);//(ulong*)new ManagedMemoryBlock(4096, 0x1000, true).Offset;
                PageDir[0] = 0b10000011; //Address=0, 2MIB, RW and present

                // set the page directory into the PDPT and mark it present
                PageDirPtrTable[i] = (ulong)PageDir | 1;
            }


            //test
            buf[0] = (byte)'B';
            buf[1] = 0x0f;

            DoEnable();

            buf[0] = (byte)'C';
            buf[1] = 0x0f;
            while (true) { }

            //Map(0, 0);

            //Map(0xB8000, 0xB8000);


            ////map first 40mb
            //for (int i = 0; i < 10; i++)
            //{
            //    var pageTable = (uint*)PML4 + (0x400 * i);
            //    //Map the 1st 4mb
            //    for (uint i2 = 0; i2 < 1024; i2++)
            //    {
            //        pageTable[i2] = (i2 * 0x1000) | 3; // attributes: supervisor level, read/write, present.
            //    }

            //    // attributes: supervisor level, read/write, present
            //    PML4[i] = ((uint)pageTable) | 3;
            //}

            //

            ////test

        }
        private static ulong Align(ulong addr, ulong alligment)
        {
            while (addr % (ulong)alligment != 0)
            {
                addr++;
            }
            return addr;
        }
        //plugged
        internal static void DoEnable()
        {
            throw new NotImplementedException();
        }

        internal static void RefreshPages()
        {
            throw new NotImplementedException();
        }


        public static void Map(ulong PhysicalAddr, ulong virtualAddr)
        {
            //var pdindex = (ulong)virtualAddr >> 22;
            //var ptindex = (ulong)virtualAddr >> 12 & 0x03FF;

            //var pageDir = (uint)PML4;
            //var pageTable = (uint*)PML4 + (0x400 * pdindex);
            //uint flags = 0;

            //pageTable[ptindex] = (uint)((uint)PhysicalAddr | (flags & 0xFFF) | 0x01); // Present;

            //RefreshPages();
        }
    }
}
