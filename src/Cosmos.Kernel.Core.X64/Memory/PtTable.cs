using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// x64 Page Table: 512 4 KiB page entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct PtTable
{
    public fixed ulong RawEntries[512];

    public PtEntry* Entries => (PtEntry*)Unsafe.AsPointer(ref this);
}
