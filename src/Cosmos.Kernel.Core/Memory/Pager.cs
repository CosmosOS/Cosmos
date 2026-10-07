// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// Platform-agnostic paging: describes address spaces and installs them into
/// whichever page tables the running architecture uses.
/// </summary>
/// <remarks>
/// This is the kernel's paging API and it is deliberately not a user-space
/// feature. Nothing here is reachable from an application today — there is no
/// ring-3 transition in the kernel (every x64 IDT gate is DPL 0) and no
/// syscall path on either architecture — but the surface is shaped for it:
/// a descriptor names an address space independently of any CPU, and
/// <see cref="PageFlags.User"/> exists but is not yet exercised.
/// <para>
/// Lifecycle is <see cref="Create"/> → <see cref="Add"/> (repeatedly) →
/// <see cref="Commit"/>. Committing is idempotent per region and installs the
/// whole set, so adding a region and committing again is the way to grow a
/// space. <see cref="Translate(AddressSpaceDescriptor*, VirtualAddress, out PhysicalAddress)"/>
/// answers whether an address is backed.
/// </para>
/// <para>
/// All descriptors are allocated from the kernel heap and handed out as raw
/// pointers. This is safe despite passing a pointer around because the kernel's
/// collector is non-moving: an object's address never changes. It also means
/// callers own the allocation — there is no finalizer, and nothing frees a
/// descriptor.
/// </para>
/// </remarks>
internal static unsafe class Pager
{
    /// <summary>
    /// Page granularity of every mapping this class installs. Mirrors
    /// <see cref="PageAllocator.PageSize"/> rather than repeating the literal.
    /// </summary>
    public const ulong PageSize = PageAllocator.PageSize;

    private static IPagerBackend? s_backend;

    /// <summary>
    /// Registers the architecture-specific backend. Called once at boot by the
    /// platform initializer, following the same self-registration pattern as
    /// <c>MsiRouting.RegisterBinder</c>.
    /// </summary>
    public static void RegisterBackend(IPagerBackend backend)
    {
        s_backend = backend;
    }

    /// <summary>
    /// Allocates an empty address space, rooted at whatever tables the CPU is
    /// currently using.
    /// </summary>
    /// <remarks>
    /// The descriptor's <see cref="AddressSpaceDescriptor.RootFrame"/> starts at
    /// the live root rather than at zero. A new space therefore begins as a
    /// copy-by-reference of the kernel's own mappings, which is what keeps the
    /// kernel image and HHDM aliases reachable under it — see
    /// <see cref="Commit"/>.
    /// </remarks>
    /// <returns>
    /// A heap-allocated descriptor the caller owns, or null when no backend is
    /// registered or the allocation failed.
    /// </returns>
    public static AddressSpaceDescriptor* Create()
    {
        if (s_backend is not { IsAvailable: true })
        {
            Serial.WriteString("[Pager] Create: no backend registered\n");
            return null;
        }

        uint size = (uint)sizeof(AddressSpaceDescriptor);
        byte* memory = Heap.Heap.Alloc(size);
        if (memory is null)
        {
            Serial.WriteString("[Pager] Create: out of memory\n");
            return null;
        }

        AddressSpaceDescriptor* space = (AddressSpaceDescriptor*)memory;
        *space = default;

        // Seed from the live root so kernel mappings stay valid in this space.
        space->RootFrame = s_backend.CurrentRootFrame();
        space->RegionCount = 0;
        space->IsCommitted = false;

        return space;
    }

    /// <summary>
    /// Appends a mapping to a space. The change is not visible until
    /// <see cref="Commit"/> runs.
    /// </summary>
    /// <returns>
    /// False when the space is full (<see cref="AddressSpaceDescriptor.MaxRegionCount"/>
    /// regions) or the descriptor is null. The region is not stored in that
    /// case — silently dropping it would leave a caller believing a mapping
    /// exists that was never installed, which is far worse than a loud refusal.
    /// </returns>
    public static bool Add(AddressSpaceDescriptor* space, VirtualAddress virt, PhysicalAddress phys,
        ulong pageCount, PageFlags flags)
    {
        if (space is null)
        {
            return false;
        }

        if (space->RegionCount >= AddressSpaceDescriptor.MaxRegionCount)
        {
            Serial.WriteString("[Pager] Add: region capacity reached (");
            Serial.WriteNumber(AddressSpaceDescriptor.MaxRegionCount);
            Serial.WriteString(")\n");
            return false;
        }

        ref AddressSpaceRegion region = ref space->NextRegion;
        region.Virt = virt;
        region.Phys = phys;
        region.PageCount = pageCount;
        region.Flags = flags;

        space->RegionCount++;
        return true;
    }

    /// <summary>
    /// Installs every region of <paramref name="space"/> into its page tables.
    /// </summary>
    /// <remarks>
    /// Safe to call repeatedly: regions already installed are recognised and
    /// left alone unless their attributes changed, so a caller can commit after
    /// each <see cref="Add"/> without special-casing the first one.
    /// </remarks>
    /// <exception cref="PlatformNotSupportedException">
    /// No backend is registered. Committing is the one operation with no
    /// managed fallback, so it fails rather than pretending to succeed.
    /// </exception>
    public static void Commit(AddressSpaceDescriptor* space)
    {
        if (s_backend is not { IsAvailable: true })
        {
            throw new PlatformNotSupportedException("Pager backend not registered");
        }

        if (space is null)
        {
            return;
        }

        s_backend.Commit(space);
        space->IsCommitted = true;
    }

    /// <summary>
    /// Resolves a virtual address within a specific space.
    /// </summary>
    /// <returns>
    /// False when the space is not committed, the address is unmapped, or it is
    /// not page-aligned. Unaligned addresses are rejected rather than rounded:
    /// the answer would be ambiguous, and a caller asking about a byte address
    /// probably has an arithmetic bug.
    /// </returns>
    public static bool Translate(AddressSpaceDescriptor* space, VirtualAddress virt, out PhysicalAddress phys)
    {
        phys = PhysicalAddress.None;

        if (s_backend is not { IsAvailable: true } || space is null)
        {
            return false;
        }

        return s_backend.Translate(space, virt, out phys);
    }

    /// <summary>
    /// Resolves a virtual address against the tables the CPU is using right
    /// now, without naming a space.
    /// </summary>
    /// <remarks>
    /// This is the form to use from a fault handler or a driver: it resolves
    /// against whatever is actually installed, so it stays correct across a
    /// root swap without the caller tracking which space is live.
    /// </remarks>
    public static bool Translate(VirtualAddress virt, out PhysicalAddress phys)
    {
        phys = PhysicalAddress.None;

        if (s_backend is not { IsAvailable: true })
        {
            return false;
        }

        return s_backend.TranslateCurrent(virt, out phys);
    }
}