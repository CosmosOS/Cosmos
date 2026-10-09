namespace Cosmos.Kernel.Core.Memory.VAS;

/// <summary>
/// Architecture-specific page-table operations used by <see cref="AddressSpace"/>.
/// Implementations live in Cosmos.Kernel.Core.X64 and Cosmos.Kernel.Core.ARM64.
/// </summary>
internal unsafe interface IVirtualMemoryMapper
{
    /// <summary>
    /// Maps <paramref name="pageCount"/> consecutive 4 KiB pages.
    /// </summary>
    /// <param name="pageTableRoot">Physical address of the root page table (CR3 / TTBR0 / TTBR1).</param>
    /// <param name="virtualAddress">Start virtual address. Must be 4 KiB aligned.</param>
    /// <param name="physicalAddress">Start physical address. Must be 4 KiB aligned.</param>
    /// <param name="pageCount">Number of pages to map.</param>
    /// <param name="flags">Page permissions and attributes.</param>
    void MapPages(ulong pageTableRoot, ulong virtualAddress, ulong physicalAddress, ulong pageCount, PageFlags flags);

    /// <summary>
    /// Unmaps <paramref name="pageCount"/> consecutive 4 KiB pages.
    /// </summary>
    /// <param name="pageTableRoot">Physical address of the root page table.</param>
    /// <param name="virtualAddress">Start virtual address. Must be 4 KiB aligned.</param>
    /// <param name="pageCount">Number of pages to unmap.</param>
    void UnmapPages(ulong pageTableRoot, ulong virtualAddress, ulong pageCount);

    /// <summary>
    /// Creates a new root page table that shares the kernel/higher-half mappings
    /// of the current address space but has no user-space mappings.
    /// </summary>
    /// <param name="sourceRoot">Physical address of the source root page table.</param>
    /// <returns>Physical address of the new root page table.</returns>
    ulong CloneHigherHalf(ulong sourceRoot);

    /// <summary>
    /// Invalidates the TLB entry for a single page.
    /// </summary>
    /// <param name="virtualAddress">Virtual address whose TLB entry to invalidate.</param>
    void InvalidatePage(ulong virtualAddress);

    /// <summary>
    /// Reads the architecture-specific page-table root register.
    /// </summary>
    /// <returns>Physical address of the current root page table.</returns>
    ulong ReadRoot();

    /// <summary>
    /// Writes the architecture-specific page-table root register.
    /// </summary>
    /// <param name="pageTableRoot">Physical address of the new root page table.</param>
    void WriteRoot(ulong pageTableRoot);
}
