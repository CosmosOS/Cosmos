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
        internal static uint* PageDirectory;
        private static bool IsEnabled = false;
        public static void Init()
        {
            if (IsEnabled)
            {
                return;
            }
            IsEnabled = true;

            var video = (byte*)0xB8000;
            
            // Setup page tables
            video[0] = (byte)'A';
            video[1] = 0x0f;

            PageDirectory = (uint*)new ManagedMemoryBlock(4096, 0x400000, true).Offset;

            // identity map kernel
            for (ulong i = 0x2000000; i < 0x2500000; i += 0x400000)
            {
                Map(i, i, PageSize._4MB, PageFlags.ReadWrite);
            }

            // identity map console
            Map(0xB8000, 0xB8000, PageSize._4KB, PageFlags.ReadWrite);

            // Enable paging
            video[0] = (byte)'B';
            video[1] = 0x0f;

            DoEnable((uint)PageDirectory);

            // Print C for now as memory manager would need rework
            video[0] = (byte)'C';
            video[1] = 0x0f;
            while (true) { }
        }
        private static uint* GetNextLevel(uint* topLevel, ulong index, bool allocate)
        {
            if ((topLevel[index] & 1) != 0)
            {
                return (uint*)((topLevel[index] & ~((uint)0xFFF)));
            }

            if (!allocate)
            {
                return null;
            }

            var nextLevel = (uint*)new ManagedMemoryBlock(4096, 0x400000, true).Offset;
            topLevel[index] = (uint)nextLevel | 3;
            return nextLevel;
        }
        public static void Map(ulong PhysicalAddress, ulong VirtualAddress, PageSize size, PageFlags flags)
        {
            var pml2Entry = VirtualAddress >> 22;
            var pml1Entry = (VirtualAddress >> 12) & 0x03FF;

            if (size == PageSize._4MB)
            {
                PageDirectory[pml2Entry] = (uint)(PhysicalAddress | ((uint)PageFlags.Present | (uint)flags | (1 << 7)));
            }
            else if (size == PageSize._4KB)
            {
                var pd = GetNextLevel(PageDirectory, pml2Entry, true);
                //var pt = GetNextLevel(pd, pml1Entry, true);
                pd[pml1Entry] = (uint)(PhysicalAddress | (uint)(PageFlags.Present | flags));
            }
        }

        //plugged
        internal static void DoEnable(uint addr)
        {
            throw new NotImplementedException();
        }

        internal static void RefreshPages()
        {
            throw new NotImplementedException();
        }
    }

    public enum PageSize
    {
        _4MB,
        _4KB,
    }
    [Flags]
    public enum PageFlags {
        Present = 1,
        ReadWrite = 2,
    }
}