using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// Shared 64-bit ARM64 translation-table descriptor layout.
/// Used by L0/L1/L2 table descriptors and by L1/L2 block and L3 page descriptors.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct PageTableEntry
{
    [FieldOffset(0)]
    public ulong RawValue;

    /// <summary>Gets or sets the valid bit (bit 0).</summary>
    public bool Valid
    {
        readonly get => (RawValue & ValidMask) != 0;
        set => SetBit(ValidMask, value);
    }

    /// <summary>
    /// Gets or sets the table/block bit (bit 1).
    /// 1 = table descriptor, 0 = block or page descriptor.
    /// </summary>
    public bool IsTable
    {
        readonly get => (RawValue & TableMask) != 0;
        set => SetBit(TableMask, value);
    }

    /// <summary>
    /// Gets or sets the MAIR attribute index (bits 4:2).
    /// </summary>
    public byte AttrIndx
    {
        readonly get => (byte)((RawValue >> AttrIndxShift) & 0x7);
        set => RawValue = (RawValue & ~AttrIndxMask) | ((ulong)(value & 0x7) << AttrIndxShift);
    }

    /// <summary>
    /// Gets or sets whether EL0 (user) access is permitted (AP[1], bit 6).
    /// </summary>
    public bool UserAccessible
    {
        readonly get => (RawValue & ApUserMask) != 0;
        set => SetBit(ApUserMask, value);
    }

    /// <summary>
    /// Gets or sets whether the entry is read-only (AP[2], bit 7).</summary>
    public bool ReadOnly
    {
        readonly get => (RawValue & ApReadOnlyMask) != 0;
        set => SetBit(ApReadOnlyMask, value);
    }

    /// <summary>Gets or sets the access flag (bit 9).</summary>
    public bool AccessFlag
    {
        readonly get => (RawValue & AccessFlagMask) != 0;
        set => SetBit(AccessFlagMask, value);
    }

    /// <summary>
    /// Gets or sets the not-global bit (bit 10).
    /// Set for user/process pages, clear for global kernel pages.
    /// </summary>
    public bool NotGlobal
    {
        readonly get => (RawValue & NotGlobalMask) != 0;
        set => SetBit(NotGlobalMask, value);
    }

    /// <summary>Gets or sets the privileged execute-never bit (bit 53).</summary>
    public bool PrivilegedExecuteNever
    {
        readonly get => (RawValue & PxnMask) != 0;
        set => SetBit(PxnMask, value);
    }

    /// <summary>Gets or sets the unprivileged execute-never bit (bit 54).</summary>
    public bool UnprivilegedExecuteNever
    {
        readonly get => (RawValue & UxnMask) != 0;
        set => SetBit(UxnMask, value);
    }

    /// <summary>
    /// Gets or sets the physical address field (bits 47:12).
    /// Block descriptors at larger granularities must mask additional low bits.
    /// </summary>
    public ulong PhysicalAddress
    {
        readonly get => RawValue & AddressMask;
        set => RawValue = (RawValue & ~AddressMask) | (value & AddressMask);
    }

    /// <summary>
    /// Creates a table descriptor pointing to the next-level table.
    /// </summary>
    public static PageTableEntry Table(ulong physicalAddress, bool userAccessible = false, bool writable = true)
    {
        PageTableEntry entry = default;
        entry.PhysicalAddress = physicalAddress;
        entry.Valid = true;
        entry.IsTable = true;
        entry.AccessFlag = true;
        entry.UserAccessible = userAccessible;
        entry.ReadOnly = !writable;
        return entry;
    }

    /// <summary>
    /// Creates a 4 KiB page descriptor.
    /// </summary>
    public static PageTableEntry Page(ulong physicalAddress, PageFlags flags, byte normalAttrIndx, byte deviceAttrIndx)
    {
        PageTableEntry entry = default;
        entry.SetPage(physicalAddress, flags, normalAttrIndx, deviceAttrIndx);
        return entry;
    }

    /// <summary>
    /// Configures this entry as a 4 KiB page descriptor.
    /// </summary>
    public void SetPage(ulong physicalAddress, PageFlags flags, byte normalAttrIndx, byte deviceAttrIndx)
    {
        RawValue = 0;
        PhysicalAddress = physicalAddress;
        Valid = true;
        AccessFlag = true;

        if ((flags & PageFlags.User) != 0)
        {
            UserAccessible = true;
        }

        if ((flags & PageFlags.Write) == 0)
        {
            ReadOnly = true;
        }

        if ((flags & PageFlags.Global) == 0)
        {
            NotGlobal = true;
        }

        if ((flags & PageFlags.Execute) == 0)
        {
            PrivilegedExecuteNever = true;
            UnprivilegedExecuteNever = true;
        }

        bool cacheDisable = (flags & PageFlags.CacheDisable) != 0;
        bool writeThrough = (flags & PageFlags.WriteThrough) != 0;

        if (cacheDisable)
        {
            // Use Device-nGnRE for uncacheable/device mappings.
            AttrIndx = deviceAttrIndx;
        }
        else if (writeThrough)
        {
            // Write-through is rare on ARM64; map to Normal WB if no WT index is known.
            AttrIndx = normalAttrIndx;
        }
        else
        {
            AttrIndx = normalAttrIndx;
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

    /// <summary>
    /// Mask for the physical address field (bits 47:12).
    /// </summary>
    public const ulong AddressMask = 0x0000_FFFF_FFFF_F000UL;

    private const ulong ValidMask = 1UL << 0;
    private const ulong TableMask = 1UL << 1;
    private const ulong AttrIndxMask = 0x7UL << 2;
    private const int AttrIndxShift = 2;
    private const ulong ApUserMask = 1UL << 6;
    private const ulong ApReadOnlyMask = 1UL << 7;
    private const ulong AccessFlagMask = 1UL << 9;
    private const ulong NotGlobalMask = 1UL << 10;
    private const ulong PxnMask = 1UL << 53;
    private const ulong UxnMask = 1UL << 54;
}
