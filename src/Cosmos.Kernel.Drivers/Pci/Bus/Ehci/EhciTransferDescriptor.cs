// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// A queue element transfer descriptor, qTD (EHCI 1.0 section 3.5), in
/// wire order: the next and alternate next qTD pointers, the token and
/// five buffer pointers. 32 bytes, 32-byte aligned, held in a 64-byte
/// descriptor slot and reached through <c>MemoryMarshal.AsRef</c> over the
/// slot's DMA page, never a pointer. The token is the hand-over: software
/// fills the other fields, issues a write barrier and writes the token
/// with Active set last; the controller writes the token and buffer
/// pointer 0 back when the qTD retires.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct EhciTransferDescriptor
{
    /// <summary>Bytes of one qTD.</summary>
    public const int Size = 32;

    /// <summary>Dword 0: the next qTD pointer with its T bit.</summary>
    public uint Next;

    /// <summary>Dword 1: the alternate next qTD pointer with its T bit, followed on a short packet.</summary>
    public uint AlternateNext;

    /// <summary>Dword 2: the token.</summary>
    public uint Token;

    /// <summary>Dword 3: buffer pointer 0 with the offset in bits 11:0.</summary>
    public uint Buffer0;

    /// <summary>Dword 4: buffer pointer 1, page aligned.</summary>
    public uint Buffer1;

    /// <summary>Dword 5: buffer pointer 2, page aligned.</summary>
    public uint Buffer2;

    /// <summary>Dword 6: buffer pointer 3, page aligned.</summary>
    public uint Buffer3;

    /// <summary>Dword 7: buffer pointer 4, page aligned.</summary>
    public uint Buffer4;
}
