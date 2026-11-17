// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// A queue head (EHCI 1.0 section 3.6), in wire order: the horizontal
/// link, the endpoint characteristics and capabilities, the current qTD
/// pointer and the transfer overlay the controller works in (next and
/// alternate next qTD, the token and the five buffer pointers). 48 bytes,
/// 32-byte aligned, held in a 64-byte descriptor slot and reached through
/// <c>MemoryMarshal.AsRef</c> over the slot's DMA page, never a pointer.
/// Software writes dwords 1 and 2 only while the queue head is unlinked,
/// and the overlay only while it is unlinked or, at build time, before it
/// is linked.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct EhciQueueHead
{
    /// <summary>Bytes of one queue head.</summary>
    public const int Size = 48;

    /// <summary>Dword 0: the horizontal link pointer with its T bit and Typ field.</summary>
    public uint HorizontalLink;

    /// <summary>Dword 1: the endpoint characteristics.</summary>
    public uint Characteristics;

    /// <summary>Dword 2: the endpoint capabilities.</summary>
    public uint Capabilities;

    /// <summary>Dword 3: the current qTD pointer, written by the controller.</summary>
    public uint CurrentTransfer;

    /// <summary>Dword 4, the overlay: the next qTD pointer.</summary>
    public uint NextTransfer;

    /// <summary>Dword 5, the overlay: the alternate next qTD pointer.</summary>
    public uint AlternateNextTransfer;

    /// <summary>Dword 6, the overlay: the token.</summary>
    public uint Token;

    /// <summary>Dword 7, the overlay: buffer pointer 0 with the current offset.</summary>
    public uint Buffer0;

    /// <summary>Dword 8, the overlay: buffer pointer 1.</summary>
    public uint Buffer1;

    /// <summary>Dword 9, the overlay: buffer pointer 2.</summary>
    public uint Buffer2;

    /// <summary>Dword 10, the overlay: buffer pointer 3.</summary>
    public uint Buffer3;

    /// <summary>Dword 11, the overlay: buffer pointer 4.</summary>
    public uint Buffer4;
}
