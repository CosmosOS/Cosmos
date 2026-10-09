using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 2 table: 512 L2 entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct L2Table
{
    public fixed ulong RawEntries[512];

    public L2Entry* Entries => (L2Entry*)Unsafe.AsPointer(ref this);
}
