// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Endpoint descriptor: the 16-byte, 16-byte aligned head of one
/// endpoint's TD queue (OHCI 1.0a section 4.2), in wire order. The
/// controller reads dword 0 and the tail, and writes the head as it
/// retires TDs; software reaches the slot through
/// <c>MemoryMarshal.Cast</c>, never a pointer, and writes the head only
/// while the ED is skipped and settled.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct OhciEndpointDescriptor
{
    /// <summary>Bytes of one ED.</summary>
    public const int Size = 16;

    /// <summary>Dword 0: FA, EN, D, S, K, F and MPS.</summary>
    public uint Control;

    /// <summary>Dword 1: TailP, the dummy TD the queue ends at.</summary>
    public uint TailPointer;

    /// <summary>Dword 2: HeadP with H (bit 0) and C (bit 1); the controller advances it.</summary>
    public uint HeadPointer;

    /// <summary>Dword 3: NextED, the next ED of the list, 0 at the end.</summary>
    public uint NextEndpoint;
}
