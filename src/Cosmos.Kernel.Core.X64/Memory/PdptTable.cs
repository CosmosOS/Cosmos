using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// x64 Page Directory Pointer Table: 512 PDPT entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct PdptTable
{
    public fixed ulong RawEntries[512];

    public PdptEntry* Entries => (PdptEntry*)Unsafe.AsPointer(ref this);
}
