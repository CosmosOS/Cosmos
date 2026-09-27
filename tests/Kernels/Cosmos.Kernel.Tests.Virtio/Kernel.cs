using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Mouse;
using Cosmos.Kernel.System.Network;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Virtio;

/// <summary>
/// Covers virtio device binding over both transports. The suite's profiles
/// attach a virtio NIC, keyboard and mouse on every cell, so the bind tests
/// are unconditional: if a device is missing, that is the regression this
/// suite exists to catch, not an environment condition.
///
/// Which transport a cell presents is a property of the QEMU profile, not of
/// the architecture: x64 runs PCI only (q35 has no virtio-mmio window) while
/// arm64 runs both an MMIO cell and a PCI one. The same kernel binary serves
/// every cell of an architecture, so the transport is detected at runtime.
///
/// The two transports are driven by different code since virtio-net and
/// virtio-input moved to the driver kit: a NIC, a keyboard or a mouse on the
/// PCI bus is bound by the built-in <c>virtio-net</c> or <c>virtio-input</c>
/// kit driver in <c>Cosmos.Kernel.HAL.Drivers</c>, during the driver pass,
/// while one on the virt machine's MMIO window is still HAL's own
/// <c>VirtioNet</c>, <c>VirtioKeyboard</c> or <c>VirtioMouse</c>, bound
/// during HAL bring-up. So what each cell can be asked differs, and the
/// tests are in three groups: what both must end in (a NIC registered with
/// the network manager, addressed and up), what only the MMIO cell has
/// (HAL's driver objects), and what only the PCI cell has (owned, claimed
/// PCI functions with MSI-X enabled on the NIC's, and the input devices in
/// the kernel's managers).
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 13;

    /// <summary>Reason surfaced for the PCI-transport tests when the cell runs virtio over MMIO.</summary>
    private const string SkipNotPci = "this cell presents virtio over MMIO";

    /// <summary>Reason surfaced for the MMIO-transport tests when the cell runs virtio over PCI.</summary>
    private const string SkipNotMmio = "this cell presents virtio over PCI, where the kit's drivers own the devices";

    /// <summary>Input functions the virtio-pci profile attaches: one keyboard and one mouse.</summary>
    private const int PciInputFunctions = 2;

    /// <summary>Transport name the MMIO transport reports.</summary>
    private const string MmioTransportName = "MMIO";

    /// <summary>The name HAL's own virtio-net driver registers itself under.</summary>
    private const string MmioDeviceName = "VirtioNet";

    // Owner names, spelled out rather than read from PciOwner so a renamed
    // constant is caught instead of compared against itself.
    private const string VirtioNetOwner = "virtio-net";
    private const string VirtioInputOwner = "virtio-input";

    /// <summary>
    /// What the kit names a link it publishes: the driver's name, a space,
    /// then the device's path. Only the prefix is asserted here; that the
    /// suffix is the function's path is the Drivers suite's ground.
    /// </summary>
    private const string KitLinkNamePrefix = VirtioNetOwner + " ";

    // MSI-X capability fields (PCI 3.0 §6.8.2), read to show a driver enabled
    // message-signalled interrupts on the function.
    private const byte MsiXMessageControlOffset = 0x02;
    private const ushort MsiXEnableBit = 0x8000;

    // True when this cell put a virtio function on the PCI bus.
    //
    // Deliberately derived from PCI *enumeration*, not from the bound driver:
    // gating on the driver would mean a device that failed to bind takes its
    // own PCI tests down with it into a green skip, which is precisely the
    // regression this suite exists to catch. Keyed off the hardware, a bind
    // failure leaves the PCI tests running — and failing.
    private static bool s_isPciCell;

    // Captured once in BeforeRun so a state change between tests cannot show
    // up as cross-test interference.
    private static VirtioNet? s_net;
    private static IKeyboardDevice[] s_keyboards = Array.Empty<IKeyboardDevice>();
    private static IMouseDevice[] s_mice = Array.Empty<IMouseDevice>();
    private static PciDevice? s_virtioNetFunction;
    private static NetworkAdapter s_adapter;

    protected override void BeforeRun()
    {
        Log.WriteString("[Virtio] BeforeRun() reached!\n");

        TR.Start("Virtio Device Tests", expectedTests: ExpectedTestCount);

        s_net = VirtioDevice.GetDevice<VirtioNet>();
        s_keyboards = VirtioDevice.GetKeyboards();
        s_mice = VirtioDevice.GetMice();
        s_virtioNetFunction = FindVirtioFunction(VirtioTransport.DeviceTypeNetwork);
        s_isPciCell = s_virtioNetFunction is not null;
        s_adapter = NetworkManager.Primary;

        // ==================== The NIC, either transport ====================
        TR.Run("Net_AdapterRegistered", TestNet_AdapterRegistered);
        TR.Run("Net_AdapterReady", TestNet_AdapterReady);
        TR.Run("Net_LinkUp", TestNet_LinkUp);
        TR.Run("Net_MacAddressProgrammed", TestNet_MacAddressProgrammed);

        // ==================== MMIO transport ====================
        TR.RunIf(!s_isPciCell, "Mmio_DriverBound",         TestMmio_DriverBound,         SkipNotMmio);
        TR.RunIf(!s_isPciCell, "Mmio_Version1Negotiated",  TestMmio_Version1Negotiated,  SkipNotMmio);
        TR.RunIf(!s_isPciCell, "Mmio_KeyboardBound",       TestMmio_KeyboardBound,       SkipNotMmio);
        TR.RunIf(!s_isPciCell, "Mmio_MouseBound",          TestMmio_MouseBound,          SkipNotMmio);

        // ==================== PCI transport ====================
        TR.RunIf(s_isPciCell, "Pci_FunctionClaimed",       TestPci_FunctionClaimed,       SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_MsiXEnabled",           TestPci_MsiXEnabled,           SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_NetOwnerRecorded",      TestPci_NetOwnerRecorded,      SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_InputOwnerRecorded",    TestPci_InputOwnerRecorded,    SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_InputDevicesPublished", TestPci_InputDevicesPublished, SkipNotPci);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== The NIC, either transport ====================
    //
    // Whichever code drove it, a virtio NIC has to come out as a device in the
    // network manager, and the name says which code that was: the kit names a
    // link it publishes after the driver and the function, while HAL's own
    // driver reports its type name. So this also catches a NIC bound over the
    // wrong path — a PCI cell whose function HAL claimed, or an MMIO one that
    // somehow came up through the kit.

    private static void TestNet_AdapterRegistered()
    {
        Assert.Equal(1, NetworkManager.DeviceCount, "the cell's virtio NIC should be the one network device");

        string? name = s_adapter.Name;
        if (name is null)
        {
            Assert.Fail("the registered network device should have a name");
            return;
        }

        Log.WriteString("[Test] Network device: ");
        Log.WriteString(name);
        Log.WriteString("\n");

        if (s_isPciCell)
        {
            Assert.True(name.StartsWith(KitLinkNamePrefix, StringComparison.Ordinal),
                $"a NIC on the PCI bus should be the kit driver's link, named \"{KitLinkNamePrefix}<path>\", is \"{name}\"");
            return;
        }

        Assert.Equal(MmioDeviceName, name);
    }

    // Ready is only set once the driver has its queues configured, its receive
    // buffers posted and an interrupt path secured, and for a kit link only
    // once the pass delivered it to the network manager.
    private static void TestNet_AdapterReady()
    {
        Assert.True(s_adapter.Ready, "the virtio NIC should report ready once registered");
    }

    private static void TestNet_LinkUp()
    {
        // QEMU's user-mode backend brings the link up immediately, so a down
        // link means the status field was read from the wrong config offset.
        Assert.True(s_adapter.LinkUp, "the virtio NIC's link should be up with QEMU user networking");
    }

    // The MAC is read byte-by-byte out of the device configuration region,
    // which on PCI is a window located through its own vendor capability. An
    // all-zero address means those reads landed nowhere.
    private static void TestNet_MacAddressProgrammed()
    {
        MACAddress? mac = s_adapter.MacAddress;
        if (mac is null)
        {
            Assert.Fail("the registered network device should have a MAC address");
            return;
        }

        Log.WriteString("[Test] MAC: ");
        Log.WriteString(mac.ToString());
        Log.WriteString("\n");

        Assert.False(mac.Equals(MACAddress.None), "the MAC address read from device config should not be all zeros");
    }

    // ==================== MMIO transport ====================

    // The registry only accepts a device once Initialize() succeeded, so a
    // null here means the whole probe → transport → driver chain failed, not
    // merely that the NIC is unhappy. On a PCI cell there is nothing for this
    // path to find: the kit's driver owns that function, and HAL's virtio
    // scan leaves every network function to it.
    private static void TestMmio_DriverBound()
    {
        if (s_net is null)
        {
            Assert.Fail("HAL's virtio-net driver should have bound the NIC on the MMIO window");
            return;
        }

        Log.WriteString("[Test] Transport: ");
        Log.WriteString(s_net.Transport.TransportName);
        Log.WriteString("\n");

        Assert.Equal(MmioTransportName, s_net.Transport.TransportName);
        Assert.True(s_net.Ready, "the MMIO virtio-net device should report ready after initialization");
    }

    // A modern virtio device must land on VERSION_1, which is what selects the
    // 12-byte net header. Negotiating it away while the driver still sized the
    // header for it would corrupt every frame. The kit's driver requires the
    // feature and fails its probe without it, so on the PCI cell a bound,
    // owned function is the same statement — which Pci_NetOwnerRecorded makes.
    private static void TestMmio_Version1Negotiated()
    {
        if (s_net is null)
        {
            Assert.Fail("no virtio-net device bound over MMIO");
            return;
        }

        Assert.True(s_net.Transport.Version1Negotiated, "the MMIO transport should negotiate VIRTIO_F_VERSION_1");
    }

    // Both profiles attach a virtio keyboard and a mouse, and binding them
    // exercises the event-type probe that tells the two apart: it reads the
    // input config select/subsel window, a different device-config access
    // pattern from the NIC's flat MAC read. On the MMIO window that probe is
    // HAL's own registry, so the devices it built are what there is to find.
    // The PCI cell's are the kit driver's, which publishes them straight to
    // the managers: Pci_InputDevicesPublished.

    private static void TestMmio_KeyboardBound()
    {
        Assert.True(s_keyboards.Length > 0, "a virtio keyboard should have bound on the MMIO window");
    }

    private static void TestMmio_MouseBound()
    {
        Assert.True(s_mice.Length > 0, "a virtio mouse should have bound on the MMIO window");
    }

    // ==================== PCI transport ====================

    // Claimed is set when a driver takes the function: the kit does it for
    // every device it binds. If the function is enumerated but unclaimed, the
    // kit driver declined or failed it — capability parsing rejected the
    // device, or the device refused a step of the handshake.
    private static void TestPci_FunctionClaimed()
    {
        // Non-null is the gate for this test, so asserting it here would be a
        // tautology; ownership is the real claim. A function that enumerates
        // but stays unclaimed also leaves Net_AdapterRegistered failing, so
        // the two together separate "the driver rejected it" from "it was
        // never there".
        Assert.True(s_virtioNetFunction!.Claimed, "the virtio-net PCI function should be claimed by the driver");
    }

    // MSI-X is the interrupt path the kit routes where the platform can, and
    // the driver points the device's configuration and queue vectors at the
    // entry the kit programmed. The bit being set is that path in place; the
    // kit falls back to polling the handler from the timer where it is not,
    // which is why this asserts the function, not the frames.
    //
    // This is the assertion the arm64 PCI cell exists for: the same MsiRouting
    // call lands on the LAPIC on x64 and on the GICv3 ITS on arm64, and only
    // this cell covers the latter.
    private static void TestPci_MsiXEnabled()
    {
        PciDevice function = s_virtioNetFunction!;
        byte capability = function.FindCapability(MsiX.CapId);
        if (capability == 0)
        {
            Assert.Fail("the virtio-net PCI function should have an MSI-X capability");
            return;
        }

        ushort control = function.ReadRegister16((byte)(capability + MsiXMessageControlOffset));
        Assert.True((control & MsiXEnableBit) != 0, "MSI-X should be enabled for the virtio-net PCI function");
    }

    // The owner names the driver, not just the fact of a claim, and virtio
    // names each device type on its own: the NIC and the input functions
    // must not end up under one shared name. For the NIC the name is the kit
    // registration's, which is reserved so nothing else can record it.
    private static void TestPci_NetOwnerRecorded()
    {
        Assert.True(s_virtioNetFunction?.Owner == VirtioNetOwner, "the virtio-net PCI function should be owned by virtio-net");
    }

    // Keyboard and mouse are both virtio-input functions, and one kit driver
    // binds either, so both carry its registration's name. The owner is only
    // recorded once its Probe returned Bound, so a claimed, owned function is
    // a device the driver brought up, and two of them are both of the cell's.
    private static void TestPci_InputOwnerRecorded()
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            Assert.Fail("PCI was never enumerated");
            return;
        }

        int inputFunctions = 0;
        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice device = devices[i];
            if (device.VendorId != VirtioPciTransport.VirtioVendorId || VirtioPciTransport.GetDeviceType(device) != VirtioTransport.DeviceTypeInput)
            {
                continue;
            }

            inputFunctions++;
            Assert.True(device.Claimed, "every virtio-input PCI function should be claimed by the driver that bound it");
            Assert.True(device.Owner == VirtioInputOwner, "every virtio-input PCI function should be owned by virtio-input");
        }

        Assert.Equal(PciInputFunctions, inputFunctions, "the virtio-pci cell attaches a keyboard and a mouse, and both should have bound");
    }

    // What the two functions were published as, which is the one thing the
    // owner does not say: the driver reads the event types each device
    // reports and publishes a keyboard for one and a mouse for the other. On
    // arm64 the counts are exactly those two, since the virt machine has no
    // PS/2 controller; on x64 q35's PS/2 keyboard and mouse are registered
    // beside them, which is why this asks for at least one of each rather
    // than exactly one.
    private static void TestPci_InputDevicesPublished()
    {
        Assert.True(KeyboardManager.DeviceCount > 0, "the virtio keyboard should have reached the keyboard manager");
        Assert.True(MouseManager.DeviceCount > 0, "the virtio mouse should have reached the mouse manager");
    }

    // ==================== Helpers ====================

    /// <summary>
    /// Finds the first enumerated PCI function that is a virtio device of the
    /// given type, or null when none is present (the MMIO cell).
    /// </summary>
    private static PciDevice? FindVirtioFunction(uint deviceType)
    {
        if (PciManager.Devices == null)
        {
            return null;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice device = PciManager.Devices[i];
            if (device.VendorId == VirtioPciTransport.VirtioVendorId && VirtioPciTransport.GetDeviceType(device) == deviceType)
            {
                return device;
            }
        }

        return null;
    }
}
