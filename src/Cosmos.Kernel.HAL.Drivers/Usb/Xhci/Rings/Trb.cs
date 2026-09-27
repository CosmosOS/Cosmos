// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

/// <summary>
/// Transfer Request Block: the 16-byte unit of every xHCI ring (xHCI 1.2
/// §4.11, §6.4), as read back from a ring's DMA page. The meaning of each
/// field depends on the TRB type; the members below decode the fields the
/// event TRBs share, and the constants encode the control dword of the TRBs
/// the driver queues. The static helpers read and write one in place, and
/// allocate nothing, so the interrupt handler may use them. Both
/// architectures are little-endian, as every TRB field is.
/// </summary>
internal readonly struct Trb
{
    internal const int Size = 16;

    // Control dword bits shared across TRB types (xHCI 1.2 §6.4).
    internal const uint Cycle = 1u << 0;

    /// <summary>Link TRB: toggle the consumer cycle state when following this link.</summary>
    internal const uint ToggleCycle = 1u << 1;

    internal const uint InterruptOnShortPacket = 1u << 2;
    internal const uint Chain = 1u << 4;
    internal const uint InterruptOnCompletion = 1u << 5;
    internal const uint ImmediateData = 1u << 6;

    /// <summary>Data/Status Stage TRB direction: set for IN.</summary>
    internal const uint DirectionIn = 1u << 16;

    /// <summary>Setup Stage TRB transfer type field (bits 17:16): 0 no data, 2 OUT data, 3 IN data.</summary>
    internal const int TransferTypeShift = 16;
    internal const uint TransferTypeOut = 2;
    internal const uint TransferTypeIn = 3;

    internal const int SlotIdShift = 24;
    internal const int EndpointIdShift = 16;

    private const int StatusOffset = 8;
    private const int ControlOffset = 12;

    private const int TypeShift = 10;
    private const uint TypeMask = 0x3F;
    private const uint EndpointIdMask = 0x1F;
    private const int CompletionCodeShift = 24;
    private const uint TransferLengthMask = 0xFFFFFF;
    private const int PortIdShift = 24;
    private const ulong PortIdMask = 0xFF;

    internal ulong Parameter { get; }
    internal uint Status { get; }
    internal uint Control { get; }

    internal TrbType Type => (TrbType)((Control >> TypeShift) & TypeMask);
    internal byte SlotId => (byte)(Control >> SlotIdShift);

    /// <summary>Transfer Event: the Device Context Index of the endpoint.</summary>
    internal byte EndpointId => (byte)((Control >> EndpointIdShift) & EndpointIdMask);

    internal CompletionCode CompletionCode => (CompletionCode)(Status >> CompletionCodeShift);

    /// <summary>Transfer Event: bytes NOT transferred of the TRB it points to.</summary>
    internal uint ResidualLength => Status & TransferLengthMask;

    /// <summary>Port Status Change Event: the 1-based root port.</summary>
    internal byte PortId => (byte)((Parameter >> PortIdShift) & PortIdMask);

    private Trb(ulong parameter, uint status, uint control)
    {
        Parameter = parameter;
        Status = status;
        Control = control;
    }

    /// <summary>Encodes <paramref name="type"/> into its control dword field.</summary>
    internal static uint TypeField(TrbType type) => (uint)type << TypeShift;

    /// <summary>
    /// Reads the TRB at the start of <paramref name="trb"/>, the control
    /// dword first and volatile: the one a consumer polls, which the
    /// producer writes last. A caller that polls reads the control dword on
    /// its own, then this behind a read barrier.
    /// </summary>
    internal static Trb Read(ReadOnlySpan<byte> trb)
    {
        uint control = ReadControl(trb);
        return new Trb(BinaryPrimitives.ReadUInt64LittleEndian(trb), BinaryPrimitives.ReadUInt32LittleEndian(trb[StatusOffset..]), control);
    }

    /// <summary>
    /// Reads the control dword of the TRB at the start of
    /// <paramref name="trb"/>. Volatile, because a poll reads it again and
    /// again with nothing in between the compiler would treat as a write to
    /// it: a plain load could be hoisted out of the loop.
    /// </summary>
    internal static uint ReadControl(ReadOnlySpan<byte> trb)
    {
        ref byte field = ref MemoryMarshal.GetReference(trb[ControlOffset..]);
        return Volatile.Read(ref Unsafe.As<byte, uint>(ref field));
    }

    /// <summary>Writes the parameter and status of the TRB at the start of <paramref name="trb"/>, leaving its control dword alone.</summary>
    internal static void WriteBody(Span<byte> trb, ulong parameter, uint status)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(trb, parameter);
        BinaryPrimitives.WriteUInt32LittleEndian(trb[StatusOffset..], status);
    }

    /// <summary>
    /// Writes the control dword of the TRB at the start of
    /// <paramref name="trb"/>. Volatile, so it is one store the compiler
    /// neither splits nor moves ahead of the body: its cycle bit is what
    /// hands the TRB to the controller.
    /// </summary>
    internal static void WriteControl(Span<byte> trb, uint control)
    {
        ref byte field = ref MemoryMarshal.GetReference(trb[ControlOffset..]);
        Volatile.Write(ref Unsafe.As<byte, uint>(ref field), control);
    }
}
