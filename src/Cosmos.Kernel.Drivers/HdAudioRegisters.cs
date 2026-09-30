// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The register map of an Intel HD Audio controller, as the High Definition
/// Audio Specification 1.0a numbers it. Controller registers sit at fixed
/// offsets in BAR 0; a stream descriptor's registers are an offset from that
/// stream's own base, which <see cref="StreamBase"/> computes.
/// </summary>
internal static class HdAudioRegisters
{
    // --- Controller registers ---

    /// <summary>Global capabilities: how many streams of each kind the controller has.</summary>
    internal const ulong GlobalCapabilities = 0x00;

    /// <summary>Minor version.</summary>
    internal const ulong VersionMinor = 0x02;

    /// <summary>Major version.</summary>
    internal const ulong VersionMajor = 0x03;

    /// <summary>Global control; bit 0 is the controller reset.</summary>
    internal const ulong GlobalControl = 0x08;

    /// <summary>State change status: one bit per codec address that changed.</summary>
    internal const ulong StateChangeStatus = 0x0E;

    /// <summary>Interrupt control; bit 31 is the global enable.</summary>
    internal const ulong InterruptControl = 0x20;

    /// <summary>Interrupt status; one bit per stream plus the controller bit.</summary>
    internal const ulong InterruptStatus = 0x24;

    /// <summary>CORB base address, low half.</summary>
    internal const ulong CorbBaseLow = 0x40;

    /// <summary>CORB base address, high half.</summary>
    internal const ulong CorbBaseHigh = 0x44;

    /// <summary>CORB write pointer: the last command the driver wrote.</summary>
    internal const ulong CorbWritePointer = 0x48;

    /// <summary>CORB read pointer; bit 15 resets it.</summary>
    internal const ulong CorbReadPointer = 0x4A;

    /// <summary>CORB control; bit 1 runs the DMA engine.</summary>
    internal const ulong CorbControl = 0x4C;

    /// <summary>CORB size.</summary>
    internal const ulong CorbSize = 0x4E;

    /// <summary>RIRB base address, low half.</summary>
    internal const ulong RirbBaseLow = 0x50;

    /// <summary>RIRB base address, high half.</summary>
    internal const ulong RirbBaseHigh = 0x54;

    /// <summary>RIRB write pointer: the last response the controller wrote; bit 15 resets it.</summary>
    internal const ulong RirbWritePointer = 0x58;

    /// <summary>Response interrupt count: how many responses before the controller interrupts.</summary>
    internal const ulong RirbInterruptCount = 0x5A;

    /// <summary>RIRB control; bit 1 runs the DMA engine.</summary>
    internal const ulong RirbControl = 0x5C;

    /// <summary>RIRB status.</summary>
    internal const ulong RirbStatus = 0x5D;

    /// <summary>RIRB size.</summary>
    internal const ulong RirbSize = 0x5E;

    /// <summary>DMA position buffer base, low half.</summary>
    internal const ulong DmaPositionBaseLow = 0x70;

    /// <summary>DMA position buffer base, high half.</summary>
    internal const ulong DmaPositionBaseHigh = 0x74;

    /// <summary>Where the first stream descriptor begins.</summary>
    internal const ulong StreamDescriptorBase = 0x80;

    /// <summary>Bytes one stream descriptor spans.</summary>
    internal const ulong StreamDescriptorStride = 0x20;

    // --- Stream descriptor registers, offsets from the stream's own base ---

    /// <summary>Stream control, three bytes wide; read and written a byte at a time.</summary>
    internal const ulong StreamControlLow = 0x00;

    /// <summary>The third byte of stream control, holding the stream number.</summary>
    internal const ulong StreamControlHigh = 0x02;

    /// <summary>Stream status: the completion and error bits, cleared by writing them back.</summary>
    internal const ulong StreamStatus = 0x03;

    /// <summary>Link position in the cyclic buffer: how far the DMA engine has read.</summary>
    internal const ulong StreamLinkPosition = 0x04;

    /// <summary>Cyclic buffer length in bytes.</summary>
    internal const ulong StreamCyclicBufferLength = 0x08;

    /// <summary>Last valid index: the highest BDL entry the engine may use.</summary>
    internal const ulong StreamLastValidIndex = 0x0C;

    /// <summary>Stream format.</summary>
    internal const ulong StreamFormat = 0x12;

    /// <summary>BDL pointer, low half.</summary>
    internal const ulong StreamBdlLow = 0x18;

    /// <summary>BDL pointer, high half.</summary>
    internal const ulong StreamBdlHigh = 0x1C;

    // --- Bits ---

    /// <summary>Global control: the controller leaves reset when this is set.</summary>
    internal const uint GlobalControlReset = 1u << 0;

    /// <summary>Interrupt control: the global interrupt enable.</summary>
    internal const uint InterruptControlGlobalEnable = 1u << 31;

    /// <summary>Interrupt control: the controller interrupt enable.</summary>
    internal const uint InterruptControlControllerEnable = 1u << 30;

    /// <summary>CORB control: run the command DMA engine.</summary>
    internal const byte CorbControlRun = 1 << 1;

    /// <summary>CORB read pointer: assert to reset the pointer, then poll until it reads back.</summary>
    internal const ushort CorbReadPointerReset = 1 << 15;

    /// <summary>
    /// RIRB control: raise the response status bit once
    /// <see cref="RirbInterruptCount"/> responses have landed. Needed even by
    /// a driver that polls, because the controller only counts responses it
    /// has flagged, and the flag is what the acknowledgement clears; without
    /// it the count never returns to zero and the command ring stops after
    /// that many commands. Delivery to the CPU is a separate question, and
    /// <see cref="InterruptControlControllerEnable"/> is what decides it.
    /// </summary>
    internal const byte RirbControlResponseInterrupt = 1 << 0;

    /// <summary>RIRB control: run the response DMA engine.</summary>
    internal const byte RirbControlRun = 1 << 1;

    /// <summary>RIRB write pointer: write to reset the pointer.</summary>
    internal const ushort RirbWritePointerReset = 1 << 15;

    /// <summary>
    /// RIRB status: a response arrived. Write-to-clear, and clearing it is
    /// what lets the controller carry on: it counts responses against
    /// <see cref="RirbInterruptCount"/> and holds the command ring once that
    /// many are outstanding, whether or not anyone wanted the interrupt.
    /// </summary>
    internal const byte RirbStatusResponseInterrupt = 1 << 0;

    /// <summary>CORB and RIRB size field: 256 entries, the size every controller supports.</summary>
    internal const byte RingSize256 = 0x02;

    /// <summary>Stream control: the stream reset bit.</summary>
    internal const byte StreamControlReset = 1 << 0;

    /// <summary>Stream control: the run bit.</summary>
    internal const byte StreamControlRun = 1 << 1;

    /// <summary>Stream control: interrupt on buffer completion.</summary>
    internal const byte StreamControlInterruptOnCompletion = 1 << 2;

    /// <summary>Stream status: a descriptor with the interrupt flag completed.</summary>
    internal const byte StreamStatusBufferCompleted = 1 << 2;

    /// <summary>Stream status: the FIFO ran dry or overflowed.</summary>
    internal const byte StreamStatusFifoError = 1 << 3;

    /// <summary>Stream status: the engine could not read a descriptor.</summary>
    internal const byte StreamStatusDescriptorError = 1 << 4;

    /// <summary>The three status bits that are cleared by writing them back.</summary>
    internal const byte StreamStatusWriteClearMask =
        StreamStatusBufferCompleted | StreamStatusFifoError | StreamStatusDescriptorError;

    /// <summary>Global capabilities: shift of the output stream count.</summary>
    internal const int CapabilitiesOutputStreamsShift = 12;

    /// <summary>Global capabilities: shift of the input stream count.</summary>
    internal const int CapabilitiesInputStreamsShift = 8;

    /// <summary>Global capabilities: mask of a stream count field.</summary>
    internal const uint CapabilitiesStreamsMask = 0x0F;

    /// <summary>Where the registers of stream <paramref name="index"/> begin, counted over all streams.</summary>
    /// <param name="index">The stream's index among every stream the controller has.</param>
    internal static ulong StreamBase(int index) =>
        StreamDescriptorBase + ((ulong)index * StreamDescriptorStride);
}
