// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// General transfer descriptor: one packet sequence of up to two pages on
/// an endpoint (OHCI 1.0a section 4.3.1), 16 bytes, 16-byte aligned, in
/// wire order. The controller writes CBP, the CC and toggle fields of
/// dword 0 and NextTD as it retires the TD onto the done queue; software
/// reaches the slot through <c>MemoryMarshal.Cast</c>, never a pointer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct OhciTransferDescriptor
{
    /// <summary>Bytes of one TD.</summary>
    public const int Size = 16;

    /// <summary>Dword 0: R, DP, DI, T, EC and CC.</summary>
    public uint Control;

    /// <summary>Dword 1: CBP, the next byte to transfer, 0 once the buffer is done.</summary>
    public uint CurrentBufferPointer;

    /// <summary>Dword 2: NextTD on the queue, or the next retired TD on the done queue.</summary>
    public uint NextTransfer;

    /// <summary>Dword 3: BE, the address of the buffer's last byte.</summary>
    public uint BufferEnd;
}
