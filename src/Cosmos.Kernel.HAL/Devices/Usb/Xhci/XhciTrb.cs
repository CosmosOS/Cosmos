// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// Transfer Request Block: the 16-byte unit of every xHCI ring (xHCI 1.2
/// §4.11, §6.4). The meaning of each field depends on the TRB type; the
/// helpers below decode the fields shared by the event TRBs.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct XhciTrb
{
    public const int Size = 16;

    // Control dword bits shared across TRB types (xHCI 1.2 §6.4).
    public const uint Cycle = 1u << 0;

    /// <summary>Link TRB: toggle the consumer cycle state when following this link.</summary>
    public const uint ToggleCycle = 1u << 1;

    public const uint InterruptOnShortPacket = 1u << 2;
    public const uint Chain = 1u << 4;
    public const uint InterruptOnCompletion = 1u << 5;
    public const uint ImmediateData = 1u << 6;

    /// <summary>Data/Status Stage TRB direction: set for IN.</summary>
    public const uint DirectionIn = 1u << 16;

    /// <summary>Setup Stage TRB transfer type field (bits 17:16): 0 no data, 2 OUT data, 3 IN data.</summary>
    public const int TransferTypeShift = 16;
    public const uint TransferTypeOut = 2;
    public const uint TransferTypeIn = 3;

    public const int SlotIdShift = 24;
    public const int EndpointIdShift = 16;

    private const int TypeShift = 10;
    private const uint TypeMask = 0x3F;
    private const uint EndpointIdMask = 0x1F;
    private const int CompletionCodeShift = 24;
    private const uint TransferLengthMask = 0xFFFFFF;
    private const int PortIdShift = 24;
    private const ulong PortIdMask = 0xFF;

    public ulong Parameter;
    public uint Status;
    public uint Control;

    public readonly XhciTrbType Type => (XhciTrbType)((Control >> TypeShift) & TypeMask);
    public readonly byte SlotId => (byte)(Control >> SlotIdShift);

    /// <summary>Transfer Event: the Device Context Index of the endpoint.</summary>
    public readonly byte EndpointId => (byte)((Control >> EndpointIdShift) & EndpointIdMask);

    public readonly XhciCompletionCode CompletionCode => (XhciCompletionCode)(Status >> CompletionCodeShift);

    /// <summary>Transfer Event: bytes NOT transferred of the TRB it points to.</summary>
    public readonly uint ResidualLength => Status & TransferLengthMask;

    /// <summary>Port Status Change Event: the 1-based root port.</summary>
    public readonly byte PortId => (byte)((Parameter >> PortIdShift) & PortIdMask);

    /// <summary>Encodes <paramref name="type"/> into its control dword field.</summary>
    public static uint TypeField(XhciTrbType type) => (uint)type << TypeShift;
}
