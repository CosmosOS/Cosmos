using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 3 entry — a 4 KiB page descriptor.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct L3Entry
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
    /// Creates a 4 KiB page descriptor.
    /// </summary>
    public static L3Entry Page(ulong physicalAddress, PageFlags flags, byte normalAttrIndx, byte deviceAttrIndx)
    {
        L3Entry entry = default;
        entry._entry = PageTableEntry.Page(physicalAddress, flags, normalAttrIndx, deviceAttrIndx);
        return entry;
    }

    /// <summary>
    /// Configures this entry as a 4 KiB page descriptor.
    /// </summary>
    public void SetPage(ulong physicalAddress, PageFlags flags, byte normalAttrIndx, byte deviceAttrIndx)
    {
        _entry.SetPage(physicalAddress, flags, normalAttrIndx, deviceAttrIndx);
    }
}
