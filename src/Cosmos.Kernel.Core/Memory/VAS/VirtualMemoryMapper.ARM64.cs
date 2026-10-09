// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory.VAS;

// The ARM64 half of the compile-time mapper. Nothing calls it yet: AddressSpace
// still maps through IVirtualMemoryMapper, which Core.ARM64 implements.
internal static partial class VirtualMemoryMapper
{
    public static partial void AddTable(PageDirectory directory, PageTableAddress table)
    {
        throw new NotImplementedException();
    }

    public static partial void MapPages(PageTableAddress table, VirtualAddress virtualAddress, PhysicalAddress physicalAddress, ulong pageCount, PageFlags flags)
    {
        throw new NotImplementedException();
    }

    public static partial void MapPages(PageTableAddress table, VirtualAddress virtualAddress, ulong pageCount)
    {
        throw new NotImplementedException();
    }

    public static partial void MapHigherHalf(PageTableAddress table)
    {
        throw new NotImplementedException();
    }

    public static partial void InvalidatePage(VirtualAddress virtualAddress)
    {
        throw new NotImplementedException();
    }

    public static partial ref PageDirectory ReadRoot()
    {
        throw new NotImplementedException();
    }

    public static partial ref PageDirectory ReadRoot(PageDirectoryAddress directory)
    {
        throw new NotImplementedException();
    }

    public static partial void WriteRoot(PageDirectoryAddress directory)
    {
        throw new NotImplementedException();
    }
}
