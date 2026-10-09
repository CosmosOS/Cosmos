using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// x64 Page Directory: 512 PD entries.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct PdTable
{
    public fixed ulong RawEntries[512];

    public PdEntry* Entries => (PdEntry*)Unsafe.AsPointer(ref this);
}
