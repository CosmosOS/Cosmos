using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// Page Directory Pointer Table entry.
/// Can be a table descriptor to a PD or a 1 GiB page/block descriptor.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct PdptEntry
{
    [FieldOffset(0)]
    private PageTableEntry _entry;

    public bool Present
    {
        readonly get => _entry.Present;
        set => _entry.Present = value;
    }

    public bool Writable
    {
        readonly get => _entry.Writable;
        set => _entry.Writable = value;
    }

    public bool User
    {
        readonly get => _entry.User;
        set => _entry.User = value;
    }

    public bool PageSize
    {
        readonly get => _entry.PageSize;
        set => _entry.PageSize = value;
    }

    public bool Accessed
    {
        readonly get => _entry.Accessed;
        set => _entry.Accessed = value;
    }

    public bool Dirty
    {
        readonly get => _entry.Dirty;
        set => _entry.Dirty = value;
    }

    public bool Global
    {
        readonly get => _entry.Global;
        set => _entry.Global = value;
    }

    public bool NoExecute
    {
        readonly get => _entry.NoExecute;
        set => _entry.NoExecute = value;
    }

    public bool CacheDisable
    {
        readonly get => _entry.CacheDisable;
        set => _entry.CacheDisable = value;
    }

    public bool WriteThrough
    {
        readonly get => _entry.WriteThrough;
        set => _entry.WriteThrough = value;
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
    /// Creates a PDPT table descriptor pointing to a PD.
    /// </summary>
    public static PdptEntry Table(ulong physicalAddress, bool user = false, bool writable = true)
    {
        PdptEntry entry = default;
        entry._entry = PageTableEntry.Table(physicalAddress, user, writable);
        return entry;
    }

    /// <summary>
    /// Creates a 1 GiB page/block descriptor.
    /// </summary>
    public static PdptEntry Page(ulong physicalAddress, PageFlags flags)
    {
        PdptEntry entry = default;
        entry._entry = PageTableEntry.Page(physicalAddress, flags);
        entry.PageSize = true;
        return entry;
    }
}
