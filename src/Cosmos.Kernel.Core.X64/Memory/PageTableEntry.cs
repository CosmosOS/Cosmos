using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// Shared 64-bit x64 page-table entry layout used by PML4, PDPT, PD, and PT.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct PageTableEntry
{
    [FieldOffset(0)]
    public ulong RawValue;

    /// <summary>Gets or sets the present (valid) bit.</summary>
    public bool Present
    {
        readonly get => (RawValue & PresentMask) != 0;
        set => SetBit(PresentMask, value);
    }

    /// <summary>Gets or sets the read/write bit.</summary>
    public bool Writable
    {
        readonly get => (RawValue & WritableMask) != 0;
        set => SetBit(WritableMask, value);
    }

    /// <summary>Gets or sets the user/supervisor bit.</summary>
    public bool User
    {
        readonly get => (RawValue & UserMask) != 0;
        set => SetBit(UserMask, value);
    }

    /// <summary>Gets or sets the page-level write-through bit.</summary>
    public bool WriteThrough
    {
        readonly get => (RawValue & WriteThroughMask) != 0;
        set => SetBit(WriteThroughMask, value);
    }

    /// <summary>Gets or sets the page-level cache disable bit.</summary>
    public bool CacheDisable
    {
        readonly get => (RawValue & CacheDisableMask) != 0;
        set => SetBit(CacheDisableMask, value);
    }

    /// <summary>Gets or sets the accessed bit.</summary>
    public bool Accessed
    {
        readonly get => (RawValue & AccessedMask) != 0;
        set => SetBit(AccessedMask, value);
    }

    /// <summary>Gets or sets the dirty bit.</summary>
    public bool Dirty
    {
        readonly get => (RawValue & DirtyMask) != 0;
        set => SetBit(DirtyMask, value);
    }

    /// <summary>
    /// Gets or sets the page size bit. Set on huge pages (PD/PDPT block entries);
    /// clear on table descriptors and 4 KiB PTEs.
    /// </summary>
    public bool PageSize
    {
        readonly get => (RawValue & PageSizeMask) != 0;
        set => SetBit(PageSizeMask, value);
    }

    /// <summary>Gets or sets the global bit.</summary>
    public bool Global
    {
        readonly get => (RawValue & GlobalMask) != 0;
        set => SetBit(GlobalMask, value);
    }

    /// <summary>Gets or sets the execute-disable bit.</summary>
    public bool NoExecute
    {
        readonly get => (RawValue & NoExecuteMask) != 0;
        set => SetBit(NoExecuteMask, value);
    }

    /// <summary>
    /// Gets or sets the physical address field (bits 51:12).
    /// </summary>
    public ulong PhysicalAddress
    {
        readonly get => RawValue & AddressMask;
        set => RawValue = (RawValue & ~AddressMask) | (value & AddressMask);
    }

    /// <summary>
    /// Creates a table descriptor pointing to the next-level page table.
    /// </summary>
    public static PageTableEntry Table(ulong physicalAddress, bool user = false, bool writable = true)
    {
        PageTableEntry entry = default;
        entry.PhysicalAddress = physicalAddress;
        entry.Present = true;
        entry.Writable = writable;
        entry.User = user;
        entry.Accessed = true;
        return entry;
    }

    /// <summary>
    /// Creates a 4 KiB page descriptor.
    /// </summary>
    public static PageTableEntry Page(ulong physicalAddress, PageFlags flags)
    {
        PageTableEntry entry = default;
        entry.SetPage(physicalAddress, flags);
        return entry;
    }

    /// <summary>
    /// Configures this entry as a 4 KiB page descriptor.
    /// </summary>
    public void SetPage(ulong physicalAddress, PageFlags flags)
    {
        RawValue = 0;
        PhysicalAddress = physicalAddress;
        Present = true;
        Accessed = true;
        Writable = true;
        Dirty = true;

        if ((flags & PageFlags.User) != 0)
        {
            User = true;
        }

        if ((flags & PageFlags.Global) != 0)
        {
            Global = true;
        }

        if ((flags & PageFlags.CacheDisable) != 0)
        {
            CacheDisable = true;
        }

        if ((flags & PageFlags.WriteThrough) != 0)
        {
            WriteThrough = true;
        }

        if ((flags & PageFlags.Write) == 0)
        {
            Writable = false;
            Dirty = false;
        }

        if ((flags & PageFlags.Execute) == 0)
        {
            NoExecute = true;
        }
    }

    private void SetBit(ulong mask, bool value)
    {
        if (value)
        {
            RawValue |= mask;
        }
        else
        {
            RawValue &= ~mask;
        }
    }

    private const ulong PresentMask = 1UL << 0;
    private const ulong WritableMask = 1UL << 1;
    private const ulong UserMask = 1UL << 2;
    private const ulong WriteThroughMask = 1UL << 3;
    private const ulong CacheDisableMask = 1UL << 4;
    private const ulong AccessedMask = 1UL << 5;
    private const ulong DirtyMask = 1UL << 6;
    private const ulong PageSizeMask = 1UL << 7;
    private const ulong GlobalMask = 1UL << 8;
    private const ulong NoExecuteMask = 1UL << 63;

    /// <summary>
    /// Mask for the physical address field (bits 51:12).
    /// </summary>
    public const ulong AddressMask = 0x000F_FFFF_FFFF_F000UL;
}
