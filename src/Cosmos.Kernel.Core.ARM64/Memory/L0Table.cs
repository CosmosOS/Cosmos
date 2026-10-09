using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 level 0 table: 512 L0 entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct L0Table
{
    public fixed ulong RawEntries[512];

    public L0Entry* Entries => (L0Entry*)Unsafe.AsPointer(ref this);
}
