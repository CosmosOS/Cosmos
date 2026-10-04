// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Transfer Request Block: the 16-byte unit of every xHCI ring (xHCI 1.2
/// sections 4.11 and 6.4), in wire order. The meaning of each field
/// depends on the TRB type; the decoders below read the fields the event
/// TRBs share. Reached through <c>MemoryMarshal.Cast</c> over a ring's DMA
/// page, never through a pointer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct XhciTrb
{
    /// <summary>Bytes of one TRB.</summary>
    public const int Size = 16;

    /// <summary>Control dword bit 0: the cycle bit, which hands the TRB over.</summary>
    public const uint Cycle = 1u << 0;

    /// <summary>Link TRB: toggle the consumer cycle state when following this link.</summary>
    public const uint ToggleCycle = 1u << 1;

    /// <summary>Interrupt on a short packet too.</summary>
    public const uint InterruptOnShortPacket = 1u << 2;

    /// <summary>The next TRB belongs to the same TD.</summary>
    public const uint Chain = 1u << 4;

    /// <summary>Raise a Transfer Event when this TRB completes.</summary>
    public const uint InterruptOnCompletion = 1u << 5;

    /// <summary>The Parameter holds the data itself (a Setup Stage TRB).</summary>
    public const uint ImmediateData = 1u << 6;

    /// <summary>Data and Status Stage TRB direction: set for IN.</summary>
    public const uint DirectionIn = 1u << 16;

    /// <summary>Setup Stage TRB transfer type (bits 17:16): 0 no data, 2 OUT data, 3 IN data.</summary>
    public const int TransferTypeShift = 16;

    /// <summary>Setup Stage TRB transfer type: an OUT Data Stage follows.</summary>
    public const uint TransferTypeOut = 2;

    /// <summary>Setup Stage TRB transfer type: an IN Data Stage follows.</summary>
    public const uint TransferTypeIn = 3;

    /// <summary>Command and event TRBs: the slot id in bits 31:24.</summary>
    public const int SlotIdShift = 24;

    /// <summary>Endpoint commands and Transfer Events: the endpoint's DCI in bits 20:16.</summary>
    public const int EndpointIdShift = 16;

    private const int TypeShift = 10;
    private const uint TypeMask = 0x3F;
    private const uint EndpointIdMask = 0x1F;
    private const int CompletionCodeShift = 24;
    private const uint TransferLengthMask = 0xFFFFFF;
    private const int PortIdShift = 24;
    private const ulong PortIdMask = 0xFF;

    /// <summary>Dwords 0 and 1: a pointer, immediate data, or an event's TRB address.</summary>
    public ulong Parameter;

    /// <summary>Dword 2: the transfer length, or an event's completion code and residual.</summary>
    public uint Status;

    /// <summary>Dword 3: the cycle bit, the flags, the type and the slot and endpoint ids.</summary>
    public uint Control;

    /// <summary>The TRB type, control bits 15:10.</summary>
    public readonly XhciTrbType Type => (XhciTrbType)((Control >> TypeShift) & TypeMask);

    /// <summary>Event TRBs: the slot the event is for.</summary>
    public readonly byte SlotId => (byte)(Control >> SlotIdShift);

    /// <summary>Transfer Event: the Device Context Index of the endpoint.</summary>
    public readonly byte EndpointId => (byte)((Control >> EndpointIdShift) & EndpointIdMask);

    /// <summary>Event TRBs: the completion code, status bits 31:24.</summary>
    public readonly XhciCompletionCode CompletionCode => (XhciCompletionCode)(Status >> CompletionCodeShift);

    /// <summary>Transfer Event: bytes not transferred of the TRB it points to.</summary>
    public readonly uint ResidualLength => Status & TransferLengthMask;

    /// <summary>Port Status Change Event: the 1-based root port, parameter bits 31:24.</summary>
    public readonly byte PortId => (byte)((Parameter >> PortIdShift) & PortIdMask);

    /// <summary>Encodes <paramref name="type"/> into its control dword field.</summary>
    /// <param name="type">The TRB type.</param>
    public static uint TypeField(XhciTrbType type) => (uint)type << TypeShift;
}
