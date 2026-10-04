// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Matches a HID boot keyboard interface at priority 1, so the kit offers it
/// the node ahead of the shipped <c>UsbKeyboardDriver</c>, which matches the
/// same three fields at priority 0. The probe opens the interface's
/// interrupt IN pipe and declines: the kit must close that pipe, and count
/// it as released with the offer, before the shipped driver's probe opens
/// the same endpoint and binds.
/// </summary>
[Driver]
public sealed class UsbDeclineDriver : RecordingDriver
{
    /// <summary>The reason the driver declines with.</summary>
    public const string Reason = "declined after opening a pipe";

    /// <summary>The priority the driver claims; anything above the default wins the tie on specificity.</summary>
    public const int ClaimedPriority = 1;

    /// <summary>bInterfaceClass: HID.</summary>
    private const byte HidClass = 0x03;

    /// <summary>bInterfaceSubClass: the boot interface subclass.</summary>
    private const byte BootInterfaceSubclass = 0x01;

    /// <summary>bInterfaceProtocol: keyboard.</summary>
    private const byte KeyboardProtocol = 0x01;

    private readonly DeviceMatch[] _matches =
    [
        new UsbMatch(interfaceClass: HidClass, interfaceSubclass: BootInterfaceSubclass, interfaceProtocol: KeyboardProtocol),
    ];

    private volatile int _pipeOpens;
    private volatile int _reports;

    /// <inheritdoc/>
    public override string Name => nameof(UsbDeclineDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    public override int Priority => ClaimedPriority;

    /// <summary>How many interrupt pipes the probes opened before declining.</summary>
    public int PipeOpens => _pipeOpens;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        UsbAccess usb = binding.Node.Access<UsbAccess>();
        UsbEndpoint? endpoint = usb.FindEndpoint(UsbEndpointType.Interrupt, isIn: true);
        if (endpoint is null)
        {
            return ProbeResult.Declined("no interrupt IN endpoint");
        }

        if (!usb.OpenInterruptPipe(binding, endpoint, OnReport, out _))
        {
            return ProbeResult.Failed("the interrupt pipe did not open");
        }

        _pipeOpens++;
        return ProbeResult.Declined(Reason);
    }

    /// <summary>Counts a report; a pipe the kit closed on the decline delivers none.</summary>
    private void OnReport(ReadOnlySpan<byte> report) => _reports++;
}
