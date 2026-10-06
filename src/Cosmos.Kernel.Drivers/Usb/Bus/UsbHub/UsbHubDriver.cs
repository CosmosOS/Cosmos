// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers.Usb.Bus.UsbHub;

/// <summary>
/// The hub class driver (USB 2.0 chapter 11, USB 3.2 chapter 10) over the
/// driver kit: binds every interface of the hub class, reads the hub
/// descriptor, registers the hub with the host controller through the kit,
/// powers and resets every port and attaches the devices behind them as
/// children of its own node, then opens the status change pipe and starts
/// the <c>usb-hub</c> thread that follows the ports plugged or pulled
/// afterwards. Everything it holds for one hub lives on a
/// <see cref="UsbHubState"/> in <see cref="DeviceBinding.DriverState"/>.
/// <see cref="Probe"/> runs in thread context on the kit worker; the detach
/// hook does nothing, since the pipe is closed by the ledger, the children
/// by the kit's child step and the slots by the host.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class UsbHubDriver : Driver
{
    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new UsbMatch(interfaceClass: UsbClassCode.Hub),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(UsbHubDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the hub up and attaches what is plugged into it. Thread
    /// context on the kit worker; a declined or failed result makes the kit
    /// release everything acquired here, the children attached from here
    /// included (their device states stay under the hub's state until the
    /// hub itself is detached, the controller releases everything, or a
    /// later probe of the hub node attaches the same port and releases the
    /// stale state first). The port scan runs here with waits of up to 500 ms
    /// per port reset and the power-good delay, so the driver stage
    /// lengthens by that.
    /// </summary>
    /// <param name="binding">The hub interface's node and the kit facilities for it.</param>
    /// <returns>Bound with the ports scanned and the status change pipe open; declined when the interface has no status change endpoint; failed when the hub did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The access and the status change endpoint.
        UsbAccess usb = binding.Node.Access<UsbAccess>();
        UsbEndpoint? endpoint = usb.FindEndpoint(UsbEndpointType.Interrupt, isIn: true);
        if (endpoint is null)
        {
            return ProbeResult.Declined("no status change endpoint");
        }

        // 2. The hub descriptor, through bPwrOn2PwrGood.
        bool superSpeed = usb.Speed >= UsbSpeed.Super;
        UsbDescriptorType descriptorType = superSpeed ? UsbDescriptorType.SuperSpeedHub : UsbDescriptorType.Hub;
        Span<byte> descriptor = stackalloc byte[UsbHubProtocol.HubDescriptorLength];
        UsbTransferStatus status = usb.ControlIn(UsbHubProtocol.HubRecipient, (byte)UsbStandardRequest.GetDescriptor,
            (ushort)((byte)descriptorType << 8), 0, descriptor);
        if (status != UsbTransferStatus.Success)
        {
            return ProbeResult.Failed("could not read the hub descriptor");
        }

        byte portCount = descriptor[UsbHubProtocol.PortCountOffset];
        int characteristics = descriptor[UsbHubProtocol.CharacteristicsOffset] | (descriptor[UsbHubProtocol.CharacteristicsOffset + 1] << 8);
        byte thinkTime = superSpeed ? (byte)0 : (byte)((characteristics >> UsbHubProtocol.ThinkTimeShift) & UsbHubProtocol.ThinkTimeMask);

        // 3. The host routes through the hub from here.
        if (!usb.ConfigureAsHub(portCount, thinkTime))
        {
            return ProbeResult.Failed("the host controller refused the hub configuration");
        }

        // 4. A SuperSpeed hub routes by the route-string nibble of its own
        //    tier, which it has to be told before any downstream traffic.
        if (superSpeed
            && usb.ControlOut(UsbHubProtocol.HubRecipient, UsbHubProtocol.SetHubDepthRequest, (ushort)usb.HubDepth, 0) != UsbTransferStatus.Success)
        {
            return ProbeResult.Failed("SET_HUB_DEPTH failed");
        }

        // 5. The state.
        UsbHubState state = new(binding, usb, portCount, superSpeed, binding.CreateEvent());
        binding.DriverState = state;
        binding.Log($"{portCount} port(s)");

        // 6. Power.
        state.PowerPorts(descriptor[UsbHubProtocol.PowerOnToPowerGoodOffset] * UsbHubProtocol.PowerOnToPowerGoodUnitMs);

        // 7. The ports: every device behind the hub is attached from here.
        state.ProbePorts();

        // 8. Listening only after the scan keeps the changes it made (the
        //    resets, the connections it cleared) from coming back as reports;
        //    a device plugged in meanwhile leaves its change bit set, which
        //    the hub reports as soon as the endpoint is polled.
        if (!usb.OpenInterruptPipe(binding, endpoint, state.OnStatusChange, out _))
        {
            return ProbeResult.Failed("could not open the status change pipe");
        }

        // 9. The thread; without a scheduler, the boot-time scan only.
        state.HotPlugRunning = binding.TryStartThread("usb-hub", state.ThreadMain, out _);
        if (!state.HotPlugRunning)
        {
            binding.Log("hot-plug off (no scheduler)");
        }

        return ProbeResult.Bound;
    }
}
