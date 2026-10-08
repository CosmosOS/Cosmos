// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Usb.Input.UsbMouse;

/// <summary>
/// Everything <see cref="UsbMouseDriver"/> holds for one bound mouse
/// interface, hung off <see cref="DeviceBinding.DriverState"/>, and the
/// pointer it publishes: a HID mouse driven in boot protocol (HID 1.11
/// appendix B.2), whose reports carry a button bitmap and signed 8-bit X
/// and Y deltas in their first three bytes, each forwarded to the sink as
/// one relative movement. A fourth byte, which the boot protocol leaves to
/// the device, is the wheel on every wheel mouse, so it is read as one.
/// <see cref="OnReport"/> runs in the <see cref="UsbReportHandler"/>
/// contexts and is allocation-free.
/// </summary>
internal sealed class UsbMouseState : IPointer
{
    // --- Constants ---

    /// <summary>SET_IDLE (HID 1.11 section 7.2.4).</summary>
    internal const byte SetIdleRequest = 0x0A;

    /// <summary>SET_PROTOCOL (HID 1.11 section 7.2.6).</summary>
    internal const byte SetProtocolRequest = 0x0B;

    /// <summary>SET_PROTOCOL wValue for the boot protocol.</summary>
    internal const ushort BootProtocol = 0;

    /// <summary>Bytes every boot protocol mouse report carries: the buttons, X and Y.</summary>
    internal const int BootReportLength = 3;

    /// <summary>The button bitmap's offset in the report.</summary>
    internal const int ButtonsOffset = 0;

    /// <summary>The X delta's offset in the report.</summary>
    internal const int DeltaXOffset = 1;

    /// <summary>The Y delta's offset in the report.</summary>
    internal const int DeltaYOffset = 2;

    /// <summary>The wheel delta's offset, in a report longer than the boot part.</summary>
    internal const int WheelOffset = 3;

    /// <summary>Bit 0 of the button bitmap: the left button.</summary>
    private const byte LeftButtonBit = 0x01;

    /// <summary>Bit 1 of the button bitmap: the right button.</summary>
    private const byte RightButtonBit = 0x02;

    /// <summary>Bit 2 of the button bitmap: the middle button.</summary>
    private const byte MiddleButtonBit = 0x04;

    // --- Private fields ---

    private volatile int _reportCount;
    private volatile int _shortReportDrops;
    private volatile int _lastReportLength;

    // --- IPointer ---

    /// <inheritdoc/>
    public string Name => "usb-mouse";

    // --- Properties the suites read ---

    /// <summary>The sink movements go to, set by the probe before the report pipe opens; a report before the consumer registered it is dropped by the sink. Any context.</summary>
    public PointerSink? Sink { get; internal set; }

    /// <summary>How many reports the handler forwarded to the sink. Any context.</summary>
    public int ReportCount => _reportCount;

    /// <summary>How many reports were dropped for being shorter than the boot part. Any context.</summary>
    public int ShortReportDrops => _shortReportDrops;

    /// <summary>The length of the last report forwarded, 0 before the first: 3 for a mouse without a wheel, 4 or more with one. Any context.</summary>
    public int LastReportLength => _lastReportLength;

    // --- The report handler ---

    /// <summary>
    /// The report pipe's handler: a report shorter than the boot part is
    /// dropped; otherwise the buttons and the deltas are forwarded as one
    /// relative movement, the Y axis as is since HID points it down as the
    /// ring does, and the wheel negated, since HID turns it positive away
    /// from the user and the ring positive toward. The
    /// <see cref="UsbReportHandler"/> contexts: interrupt context on a
    /// controller with a message interrupt, otherwise whichever thread drains
    /// the controller's events under its lock. Allocation-free; calls only
    /// the sink.
    /// </summary>
    /// <param name="report">The bytes the mouse sent in this transfer.</param>
    internal void OnReport(ReadOnlySpan<byte> report)
    {
        if (report.Length < BootReportLength)
        {
            _shortReportDrops++;
            return;
        }

        byte flags = report[ButtonsOffset];
        PointerButtons buttons = ((flags & LeftButtonBit) != 0 ? PointerButtons.Left : PointerButtons.None)
            | ((flags & RightButtonBit) != 0 ? PointerButtons.Right : PointerButtons.None)
            | ((flags & MiddleButtonBit) != 0 ? PointerButtons.Middle : PointerButtons.None);
        int deltaX = (sbyte)report[DeltaXOffset];
        int deltaY = (sbyte)report[DeltaYOffset];
        int wheel = report.Length > WheelOffset ? -(sbyte)report[WheelOffset] : 0;

        _lastReportLength = report.Length;
        _reportCount++;
        PointerSink? sink = Sink;
        sink?.ReportRelative(deltaX, deltaY, buttons, wheel);
    }
}
