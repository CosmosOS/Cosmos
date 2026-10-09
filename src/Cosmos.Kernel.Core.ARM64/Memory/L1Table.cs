using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 1 table: 512 L1 entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct L1Table
{
    public fixed ulong RawEntries[512];

    public L1Entry* Entries => (L1Entry*)Unsafe.AsPointer(ref this);
}
