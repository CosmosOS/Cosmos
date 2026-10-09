using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// x64 Page Map Level 4 table: 512 PML4 entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct Pml4Table
{
    public fixed ulong RawEntries[512];

    public Pml4Entry* Entries => (Pml4Entry*)Unsafe.AsPointer(ref this);
}
