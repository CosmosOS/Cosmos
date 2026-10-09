// This code is licensed under MIT license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory.VAS;

internal static partial class VirtualMemoryMapper
{

    public const ulong UserSpaceStart = 0;

    public static partial void AddTable(PageDirectory directory, PageTableAddress table);

    public static partial void MapPages(PageTableAddress table, VirtualAddress virtualAddress, PhysicalAddress physicalAddress, ulong pageCount, PageFlags flags);

    public static partial void MapPages(PageTableAddress table, VirtualAddress virtualAddress, ulong pageCount);

    public static partial void MapHigherHalf(PageTableAddress table);

    public static partial void InvalidatePage(VirtualAddress virtualAddress);

    public static partial ref PageDirectory ReadRoot();
    public static partial ref PageDirectory ReadRoot(PageDirectoryAddress directory);

    public static partial void WriteRoot(PageDirectoryAddress directory);
}
