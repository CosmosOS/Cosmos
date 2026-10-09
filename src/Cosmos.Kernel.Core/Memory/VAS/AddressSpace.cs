using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.Memory.VAS;

/// <summary>
/// Provides the architecture-specific page-table mapper used by <see cref="AddressSpace"/>.
/// Set during platform initialization from the architecture-specific Core package.
/// </summary>
internal static class VirtualMemoryProvider
{
    /// <summary>
    /// The current architecture's page-table mapper. Null until platform initialization.
    /// </summary>
    public static IVirtualMemoryMapper? Mapper { get; set; }
}

/// <summary>
/// Represents a virtual address space (page table root + mapping state).
/// Kernel code uses the singleton <see cref="KernelSpace"/>. Each user process
/// has its own address space that shares the kernel/higher-half mappings.
/// </summary>
internal class AddressSpace
{
    /// <summary>
    /// The kernel's address space, initialized from the bootloader page tables.
    /// </summary>
    public static AddressSpace? KernelSpace { get; private set; }

    /// <summary>
    /// Physical address of the root page table (CR3 on x64, TTBR0/TTBR1 on ARM64).
    /// </summary>
    public ulong PageTableRoot { get; }

    /// <summary>
    /// Number of live references to this address space. Used to decide when the
    /// root page table and private tables can be freed.
    /// </summary>
    public int ReferenceCount { get; private set; }

    private AddressSpace(ulong pageTableRoot)
    {
        PageTableRoot = pageTableRoot;
        ReferenceCount = 1;
    }

    /// <summary>
    /// Initializes the kernel address space from the current architecture page-table root.
    /// Must be called exactly once during early boot.
    /// </summary>
    public static void InitializeKernelSpace()
    {
        if (KernelSpace != null)
        {
            return;
        }

        IVirtualMemoryMapper? mapper = VirtualMemoryProvider.Mapper;
        if (mapper == null)
        {
            throw new InvalidOperationException("VirtualMemoryProvider.Mapper not set before AddressSpace.InitializeKernelSpace");
        }

        ulong root = mapper.ReadRoot();
        KernelSpace = new AddressSpace(root);
    }

    /// <summary>
    /// Creates a new address space that shares the kernel/higher-half mappings
    /// of the current kernel space but has no user-space mappings.
    /// </summary>
    public static AddressSpace? CloneHigherHalf()
    {
        if (KernelSpace == null)
        {
            return null;
        }

        IVirtualMemoryMapper? mapper = VirtualMemoryProvider.Mapper;
        if (mapper == null)
        {
            return null;
        }

        ulong newRoot = mapper.CloneHigherHalf(KernelSpace.PageTableRoot);
        return new AddressSpace(newRoot);
    }

    /// <summary>
    /// Maps consecutive 4 KiB pages into this address space.
    /// </summary>
    public void Map(ulong virtualAddress, ulong physicalAddress, ulong pageCount, PageFlags flags)
    {
        IVirtualMemoryMapper? mapper = VirtualMemoryProvider.Mapper;
        if (mapper == null)
        {
            throw new InvalidOperationException("VirtualMemoryProvider.Mapper not set");
        }

        mapper.MapPages(PageTableRoot, virtualAddress, physicalAddress, pageCount, flags);
    }

    /// <summary>
    /// Unmaps consecutive 4 KiB pages from this address space.
    /// </summary>
    public void Unmap(ulong virtualAddress, ulong pageCount)
    {
        IVirtualMemoryMapper? mapper = VirtualMemoryProvider.Mapper;
        if (mapper == null)
        {
            throw new InvalidOperationException("VirtualMemoryProvider.Mapper not set");
        }

        mapper.UnmapPages(PageTableRoot, virtualAddress, pageCount);
    }

    /// <summary>
    /// Retains a reference to this address space.
    /// </summary>
    public void AddReference()
    {
        ReferenceCount++;
    }

    /// <summary>
    /// Releases a reference to this address space.
    /// </summary>
    public void ReleaseReference()
    {
        ReferenceCount--;
        if (ReferenceCount <= 0)
        {
            // TODO: free page-table pages when refcount reaches zero.
        }
    }
}
