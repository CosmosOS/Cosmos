using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 3 table: 512 4 KiB page entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct L3Table
{
    public fixed ulong RawEntries[512];

    public L3Entry* Entries => (L3Entry*)Unsafe.AsPointer(ref this);
}
