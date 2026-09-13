using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
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
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 10;

    /// <summary>Reason surfaced for the PCI-transport tests when the cell runs virtio over MMIO.</summary>
    private const string SkipNotPci = "this cell presents virtio over MMIO";

    /// <summary>Transport name the PCI transport reports.</summary>
    private const string PciTransportName = "PCI";

    /// <summary>Transport name the MMIO transport reports.</summary>
    private const string MmioTransportName = "MMIO";

    // True when this cell put a virtio function on the PCI bus.
    //
    // Deliberately derived from PCI *enumeration*, not from the bound driver:
    // gating on the driver would mean a device that failed to bind takes its
    // own PCI tests down with it into a green skip, which is precisely the
    // regression this suite exists to catch. Keyed off the hardware, a bind
    // failure leaves the PCI tests running — and failing.
    private static bool s_isPciCell;

    /// <summary>Bytes in a MAC address.</summary>
    private const int MacAddressLength = 6;

    // Captured once in BeforeRun so a state change between tests cannot show
    // up as cross-test interference.
    private static VirtioNet? s_net;
    private static IKeyboardDevice[] s_keyboards = Array.Empty<IKeyboardDevice>();
    private static IMouseDevice[] s_mice = Array.Empty<IMouseDevice>();
    private static PciDevice? s_virtioNetFunction;

    protected override void BeforeRun()
    {
        Log.WriteString("[Virtio] BeforeRun() reached!\n");

        TR.Start("Virtio Device Tests", expectedTests: ExpectedTestCount);

        s_net = VirtioDevice.GetDevice<VirtioNet>();
        s_keyboards = VirtioDevice.GetKeyboards();
        s_mice = VirtioDevice.GetMice();
        s_virtioNetFunction = FindVirtioFunction(VirtioTransport.DeviceTypeNetwork);
        s_isPciCell = s_virtioNetFunction != null;

        // ==================== Binding ====================
        TR.Run("Net_DriverBound", TestNet_DriverBound);
        TR.Run("Net_TransportMatchesCell", TestNet_TransportMatchesCell);
        TR.Run("Net_DeviceReady", TestNet_DeviceReady);
        TR.Run("Net_LinkUp", TestNet_LinkUp);
        TR.Run("Net_MacAddressProgrammed", TestNet_MacAddressProgrammed);

        // ==================== Input ====================
        TR.Run("Input_KeyboardBound", TestInput_KeyboardBound);
        TR.Run("Input_MouseBound", TestInput_MouseBound);

        // ==================== PCI transport ====================
        TR.RunIf(s_isPciCell, "Pci_FunctionClaimed",     TestPci_FunctionClaimed,     SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_MsiXActive",          TestPci_MsiXActive,          SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_Version1Negotiated",  TestPci_Version1Negotiated,  SkipNotPci);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Binding ====================

    // The registry only accepts a device once Initialize() succeeded, so a
    // null here means the whole probe → transport → driver chain failed, not
    // merely that the NIC is unhappy.
    private static void TestNet_DriverBound()
    {
        Assert.NotNull(s_net, "virtio-net driver should have bound to the attached NIC");
    }

    // Cross-checks the bus against the driver: if this cell put a virtio
    // function on the PCI bus, the driver must have come up over the PCI
    // transport, and otherwise over MMIO. Catches a silent fallback to the
    // wrong transport, which would otherwise look like a healthy device
    // while none of the PCI-specific paths were ever exercised.
    private static void TestNet_TransportMatchesCell()
    {
        if (s_net == null)
        {
            Assert.Fail("no virtio-net device bound");
            return;
        }

        Log.WriteString("[Test] Transport: ");
        Log.WriteString(s_net.Transport.TransportName);
        Log.WriteString("\n");

        string expected = s_isPciCell ? PciTransportName : MmioTransportName;
        Assert.Equal(expected, s_net.Transport.TransportName);
    }

    // Ready is only set after queues are configured, buffers are posted and an
    // interrupt source was secured — the driver now refuses to come up
    // without one, so this also covers MSI-X binding on the PCI cell.
    private static void TestNet_DeviceReady()
    {
        if (s_net == null)
        {
            Assert.Fail("no virtio-net device bound");
            return;
        }

        Assert.True(s_net.Ready, "virtio-net should report ready after initialization");
    }

    private static void TestNet_LinkUp()
    {
        if (s_net == null)
        {
            Assert.Fail("no virtio-net device bound");
            return;
        }

        // QEMU's user-mode backend brings the link up immediately, so a down
        // link means the status field was read from the wrong config offset.
        Assert.True(s_net.LinkUp, "virtio-net link should be up with QEMU user networking");
    }

    // The MAC is read byte-by-byte out of the device configuration region,
    // which on PCI is a separate BAR window located through its own vendor
    // capability. An all-zero address means those reads landed nowhere.
    private static void TestNet_MacAddressProgrammed()
    {
        if (s_net == null)
        {
            Assert.Fail("no virtio-net device bound");
            return;
        }

        MACAddress mac = s_net.MacAddress;
        Assert.NotNull(mac);

        bool anyNonZero = false;
        for (int i = 0; i < MacAddressLength; i++)
        {
            if (mac._bytes[i] != 0)
            {
                anyNonZero = true;
                break;
            }
        }

        Log.WriteString("[Test] MAC: ");
        Log.WriteString(mac.ToString());
        Log.WriteString("\n");

        Assert.True(anyNonZero, "MAC address read from device config should not be all zeros");
    }

    // ==================== Input ====================
    //
    // Both profiles attach a virtio keyboard and mouse. Binding them exercises
    // the event-type probe in the registry, which reads the input config
    // select/subsel window — a different device-config access pattern from the
    // NIC's flat MAC read.

    private static void TestInput_KeyboardBound()
    {
        Assert.True(s_keyboards.Length > 0, "a virtio keyboard should have bound");
    }

    private static void TestInput_MouseBound()
    {
        Assert.True(s_mice.Length > 0, "a virtio mouse should have bound");
    }

    // ==================== PCI transport ====================

    // Claimed is set by the virtio PCI scan when it takes ownership. If the
    // function is enumerated but unclaimed, capability parsing rejected the
    // device and the driver never saw it.
    private static void TestPci_FunctionClaimed()
    {
        // Non-null is the gate for this test, so asserting it here would be a
        // tautology; ownership is the real claim. A function that enumerates
        // but stays unclaimed still fails Net_DriverBound, so the two together
        // separate "capability parsing rejected it" from "it was never there".
        Assert.True(s_virtioNetFunction!.Claimed, "the virtio-net PCI function should be claimed by the driver");
    }

    // MSI-X is the only interrupt path the PCI transport offers (there is no
    // INTx fallback — PCI interrupt lines are level-low and shared, which the
    // available line routing cannot express), so this failing means the device
    // would run blind even if everything else bound.
    //
    // This is the assertion the arm64 PCI cell exists for: the same MsiRouting
    // call lands on the LAPIC on x64 and on the GICv3 ITS on arm64, and only
    // this cell covers the latter.
    private static void TestPci_MsiXActive()
    {
        VirtioPciTransport? transport = s_net?.Transport as VirtioPciTransport;
        Assert.NotNull(transport, "virtio-net should be running over the PCI transport");
        if (transport != null)
        {
            Assert.True(transport.MsiXActive, "MSI-X should be enabled for the virtio-net PCI function");
        }
    }

    // Modern virtio-pci must land on VERSION_1, which is what selects the
    // 12-byte net header. Negotiating it away here while the driver still
    // sized the header for it would corrupt every frame.
    private static void TestPci_Version1Negotiated()
    {
        if (s_net == null)
        {
            Assert.Fail("no virtio-net device bound");
            return;
        }

        Assert.True(s_net.Transport.Version1Negotiated, "virtio-pci should negotiate VIRTIO_F_VERSION_1");
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
