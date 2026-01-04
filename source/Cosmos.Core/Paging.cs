using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.Memory;
using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    public unsafe class Paging
    {
        internal static ulong* PageDirectoryPointerTable;
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
            video[0] = (byte)'!';
            video[1] = 0x0f;

            PageDirectoryPointerTable = (ulong*)new ManagedMemoryBlock(4 * sizeof(long), 0x20, true).Offset;

            // identity map kernel
            for (ulong i = 0x2000000; i < 0x2500000; i += 0x200000)
            {
                Map(i, i, PageSize._2MB, PageFlags.ReadWrite);
            }

            // identity map RAT
            for (ulong i = (ulong)RAT.mRAT; i < (ulong)RAT.HeapEnd; i += 0x200000)
            {
                Map(i, i, PageSize._2MB, PageFlags.ReadWrite);
            }

            // identity map console
            Map(0xB8000, 0xB8000, PageSize._2MB, PageFlags.ReadWrite);

            // Enable paging
            video[0] = (byte)'E';
            video[1] = 0x0f;

            DoEnable((uint)PageDirectoryPointerTable);

            // Print C for now as memory manager would need rework
            video[0] = (byte)':';
            video[1] = 0x0f;
            video[1] = (byte)')';
            video[2] = 0x0f;
        }
        private static ulong* GetNextLevel(ulong* topLevel, ulong index, bool allocate, ulong flags = 3)
        {
            if ((topLevel[index] & 1) != 0)
            {
                return (ulong*)((topLevel[index] & ~((ulong)0xFFF)));
            }

            if (!allocate)
            {
                return null;
            }

            var nextLevel = (ulong*)new ManagedMemoryBlock(512 * sizeof(ulong), 0x1000, true).Offset;
            topLevel[index] = (ulong)nextLevel | flags;
            return nextLevel;
        }
        public static void Map(ulong PhysicalAddress, ulong VirtualAddress, PageSize size, PageFlags flags)
        {
            var pdpteIndex = (VirtualAddress >> 30) & 0x03; // 2 bits for PDPT
            var pdeIndex = (VirtualAddress >> 21) & 0x1FF;  // 9 bits for PD
            var pteIndex = (VirtualAddress >> 12) & 0x1FF;  // 9 bits for PT

            var pdpte = GetNextLevel(PageDirectoryPointerTable, pdpteIndex, true, 1);
            if (size == PageSize._2MB)
            {
                pdpte[pdeIndex] = (ulong)(PhysicalAddress | ((ulong)3 | (1 << 7)));
            }
            else if (size == PageSize._4KB)
            {
                var pde = GetNextLevel(pdpte, pdeIndex, true);
                var pt = GetNextLevel(pde, pteIndex, true);
                pt[pteIndex] = (ulong)(PhysicalAddress | (ulong)(3));
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
        _2MB,
        _4KB,
    }
    [Flags]
    public enum PageFlags
    {
        Present = 1 << 0,
        ReadWrite = 1 << 1,
        Supervisor = 1 << 2
    }
}