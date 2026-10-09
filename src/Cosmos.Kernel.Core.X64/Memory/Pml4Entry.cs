using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// Page Map Level 4 entry.
/// Always a table descriptor pointing to a PDPT.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct Pml4Entry
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

    public bool NoExecute
    {
        readonly get => _entry.NoExecute;
        set => _entry.NoExecute = value;
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
    /// Creates a PML4 table descriptor pointing to a PDPT.
    /// </summary>
    public static Pml4Entry Table(ulong physicalAddress, bool user = false, bool writable = true)
    {
        Pml4Entry entry = default;
        entry._entry = PageTableEntry.Table(physicalAddress, user, writable);
        return entry;
    }
}
