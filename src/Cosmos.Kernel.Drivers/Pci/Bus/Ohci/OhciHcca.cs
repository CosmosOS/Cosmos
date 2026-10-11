// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Host controller communications area (OHCI 1.0a section 4.4): 256 bytes,
/// 256-byte aligned. The 32 interrupt list heads fill the first 128 bytes
/// and are reached as a <c>Span&lt;uint&gt;</c> over that range; the frame
/// number and the done head lie at fixed offsets behind them. The
/// controller writes the frame number every frame and the done head when
/// it delivers retired TDs; software reads the done head with
/// <c>Volatile</c> and writes 0 back.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 256)]
internal struct OhciHcca
{
    /// <summary>Bytes of the HCCA.</summary>
    public const int Size = 256;

    /// <summary>Entries of the interrupt table: frame number modulo 32 picks one.</summary>
    public const int InterruptTableEntries = 32;

    /// <summary>Bytes of the interrupt table, which opens the HCCA.</summary>
    public const int InterruptTableBytes = InterruptTableEntries * sizeof(uint);

    /// <summary>Offset 0x80: HccaFrameNumber in the low word, HccaPad1 in the high one.</summary>
    [FieldOffset(0x80)]
    public uint FrameNumberAndPad;

    /// <summary>Offset 0x84: HccaDoneHead, the newest retired TD, with bit 0 set when another interrupt was pending.</summary>
    [FieldOffset(0x84)]
    public uint DoneHead;
}
