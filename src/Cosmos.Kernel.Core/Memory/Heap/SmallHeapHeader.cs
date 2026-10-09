// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Memory.Heap;

[StructLayout(LayoutKind.Sequential)]
internal struct SmallHeapHeader
{
    public ushort Size;
}
