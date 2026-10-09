using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// Page Table entry — a 4 KiB page descriptor.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct PtEntry
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
    /// Creates a 4 KiB page descriptor.
    /// </summary>
    public static PtEntry Page(ulong physicalAddress, PageFlags flags)
    {
        PtEntry entry = default;
        entry._entry = PageTableEntry.Page(physicalAddress, flags);
        return entry;
    }

    /// <summary>
    /// Configures this entry as a 4 KiB page descriptor.
    /// </summary>
    public void SetPage(ulong physicalAddress, PageFlags flags)
    {
        _entry.SetPage(physicalAddress, flags);
    }
}
