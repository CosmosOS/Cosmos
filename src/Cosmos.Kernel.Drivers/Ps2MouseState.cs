// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Ps2;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="Ps2MouseDriver"/> holds for one bound auxiliary
/// port, hung off <see cref="DeviceBinding.DriverState"/>, and the pointer
/// it publishes: the port's access, the sink, the packet being assembled
/// and the counters the Drivers suite reads. <see cref="OnInterrupt"/> runs
/// in interrupt context, raised by the 8042 driver's delivery, and is
/// allocation-free: the packet buffer is allocated in the constructor.
/// </summary>
public sealed class Ps2MouseState : IPointer
{
    // --- Constants ---

    /// <summary>Bytes of a standard mouse's packet.</summary>
    private const int StandardPacketBytes = 3;

    /// <summary>Bytes of a wheel mouse's packet.</summary>
    private const int WheelPacketBytes = 4;

    /// <summary>Bit 3 of a packet's first byte, always set: a first byte without it is not a packet start.</summary>
    private const byte AlwaysSetBit = 0x08;

    /// <summary>Bit 0 of the first byte: the left button.</summary>
    private const byte LeftButtonBit = 0x01;

    /// <summary>Bit 1 of the first byte: the right button.</summary>
    private const byte RightButtonBit = 0x02;

    /// <summary>Bit 2 of the first byte: the middle button.</summary>
    private const byte MiddleButtonBit = 0x04;

    /// <summary>Bit 4 of the first byte: the X movement is negative.</summary>
    private const byte XSignBit = 0x10;

    /// <summary>Bit 5 of the first byte: the Y movement is negative.</summary>
    private const byte YSignBit = 0x20;

    // --- Private fields ---

    private readonly Ps2Access _access;
    private readonly byte[] _packet = new byte[WheelPacketBytes];
    private int _index;
    private volatile int _bytesReceived;
    private volatile int _packetsReported;
    private volatile int _resyncDrops;

    // --- Constructor ---

    /// <summary>Takes the access and whether the knock found a wheel. Thread context, from the probe.</summary>
    /// <param name="access">The auxiliary port's access.</param>
    /// <param name="hasWheel">True when the mouse answered the knock with id 0x03.</param>
    internal Ps2MouseState(Ps2Access access, bool hasWheel)
    {
        _access = access;
        HasWheel = hasWheel;
    }

    // --- IPointer ---

    /// <inheritdoc/>
    public string Name => "ps2-mouse";

    // --- Properties the suites read ---

    /// <summary>The sink movements go to, set by the probe before reporting is enabled. Any context.</summary>
    public PointerSink? Sink { get; internal set; }

    /// <summary>True when the mouse answered the IntelliMouse knock, so every packet carries a fourth byte for the wheel. Any context.</summary>
    public bool HasWheel { get; }

    /// <summary>Bytes of one packet: 4 with a wheel, 3 without. Any context.</summary>
    public int PacketBytes => HasWheel ? WheelPacketBytes : StandardPacketBytes;

    /// <summary>How many bytes the handler took off the port's ring. Any context.</summary>
    public int BytesReceived => _bytesReceived;

    /// <summary>How many whole packets the handler reported to the sink. Any context.</summary>
    public int PacketsReported => _packetsReported;

    /// <summary>How many bytes were dropped for not being a packet start. Any context.</summary>
    public int ResyncDrops => _resyncDrops;

    // --- The handler ---

    /// <summary>Drains the port's ring and collects each byte into the packet. Interrupt context, raised by the 8042 driver's delivery; allocation-free.</summary>
    /// <param name="context">The kit's handler context, unused.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The method has the InterruptHandler shape; the ring is drained and the sink called, nothing is scheduled.")]
    internal void OnInterrupt(InterruptContext context)
    {
        while (_access.TryReceive(out byte value))
        {
            _bytesReceived++;
            Collect(value);
        }
    }

    /// <summary>
    /// Collects one byte: a first byte whose bit 3 is clear is not a packet
    /// start and is dropped, so a lost or doubled byte costs one packet and
    /// not every packet after it; a whole packet is parsed and reported as
    /// one relative movement, the Y axis flipped since PS/2 points it up,
    /// the overflow bits 6 and 7 ignored. Handler context.
    /// </summary>
    /// <param name="value">The byte.</param>
    private void Collect(byte value)
    {
        if (_index == 0 && (value & AlwaysSetBit) == 0)
        {
            _resyncDrops++;
            return;
        }

        _packet[_index++] = value;
        if (_index < PacketBytes)
        {
            return;
        }

        _index = 0;
        byte flags = _packet[0];
        PointerButtons buttons = ((flags & LeftButtonBit) != 0 ? PointerButtons.Left : PointerButtons.None)
            | ((flags & RightButtonBit) != 0 ? PointerButtons.Right : PointerButtons.None)
            | ((flags & MiddleButtonBit) != 0 ? PointerButtons.Middle : PointerButtons.None);
        int deltaX = _packet[1];
        int deltaY = _packet[2];
        if ((flags & XSignBit) != 0)
        {
            deltaX |= unchecked((int)0xFFFFFF00);
        }

        if ((flags & YSignBit) != 0)
        {
            deltaY |= unchecked((int)0xFFFFFF00);
        }

        deltaY = -deltaY;
        int wheel = HasWheel ? (sbyte)_packet[3] : 0;
        _packetsReported++;
        PointerSink? sink = Sink;
        sink?.ReportRelative(deltaX, deltaY, buttons, wheel);
    }
}
