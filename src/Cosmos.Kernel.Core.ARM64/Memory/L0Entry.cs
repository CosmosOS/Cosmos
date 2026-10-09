using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 0 entry. Always a table descriptor pointing to an L1 table.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct L0Entry
{
    [FieldOffset(0)]
    private PageTableEntry _entry;

    public bool Valid
    {
        readonly get => _entry.Valid;
        set => _entry.Valid = value;
    }

    public bool AccessFlag
    {
        readonly get => _entry.AccessFlag;
        set => _entry.AccessFlag = value;
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
    /// Creates an L0 table descriptor pointing to an L1 table.
    /// </summary>
    public static L0Entry Table(ulong physicalAddress, bool userAccessible = false, bool writable = true)
    {
        L0Entry entry = default;
        entry._entry = PageTableEntry.Table(physicalAddress, userAccessible, writable);
        return entry;
    }
}
