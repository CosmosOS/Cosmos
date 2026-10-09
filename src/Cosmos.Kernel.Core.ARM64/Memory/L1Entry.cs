using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 1 entry. Can be a table descriptor to an L2 table or a 1 GiB block descriptor.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct L1Entry
{
    [FieldOffset(0)]
    private PageTableEntry _entry;

    public bool Valid
    {
        readonly get => _entry.Valid;
        set => _entry.Valid = value;
    }

    public bool IsTable
    {
        readonly get => _entry.IsTable;
        set => _entry.IsTable = value;
    }

    public bool AccessFlag
    {
        readonly get => _entry.AccessFlag;
        set => _entry.AccessFlag = value;
    }

    public bool NotGlobal
    {
        readonly get => _entry.NotGlobal;
        set => _entry.NotGlobal = value;
    }

    public bool UserAccessible
    {
        readonly get => _entry.UserAccessible;
        set => _entry.UserAccessible = value;
    }

    public bool ReadOnly
    {
        readonly get => _entry.ReadOnly;
        set => _entry.ReadOnly = value;
    }

    public bool PrivilegedExecuteNever
    {
        readonly get => _entry.PrivilegedExecuteNever;
        set => _entry.PrivilegedExecuteNever = value;
    }

    public bool UnprivilegedExecuteNever
    {
        readonly get => _entry.UnprivilegedExecuteNever;
        set => _entry.UnprivilegedExecuteNever = value;
    }

    public byte AttrIndx
    {
        readonly get => _entry.AttrIndx;
        set => _entry.AttrIndx = value;
    }

    public ulong PhysicalAddress
    {
        readonly get => _entry.PhysicalAddress;
        set => _entry.PhysicalAddress = value;
    }

    public ulong RawValue
    {
        readonly get => _entry.RawValue;
        set => _entry.RawValue = value;
    }

    /// <summary>
    /// Creates an L1 table descriptor pointing to an L2 table.
    /// </summary>
    public static L1Entry Table(ulong physicalAddress, bool userAccessible = false, bool writable = true)
    {
        L1Entry entry = default;
        entry._entry = PageTableEntry.Table(physicalAddress, userAccessible, writable);
        return entry;
    }

    /// <summary>
    /// Creates a 1 GiB block descriptor.
    /// </summary>
    public static L1Entry Block(ulong physicalAddress, PageFlags flags, byte normalAttrIndx, byte deviceAttrIndx)
    {
        L1Entry entry = default;
        entry._entry = PageTableEntry.Page(physicalAddress, flags, normalAttrIndx, deviceAttrIndx);
        entry.IsTable = false;
        return entry;
    }
}
