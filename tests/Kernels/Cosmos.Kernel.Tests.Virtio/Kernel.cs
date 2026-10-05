// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Drivers;
using Cosmos.Kernel.HAL;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Virtio;

/// <summary>
/// Covers virtio device binding through the driver kit over both
/// transports. The suite's profiles attach a virtio NIC, keyboard and mouse
/// on every cell, so the bind tests are unconditional: if a device is
/// missing, that is the regression this suite exists to catch, not an
/// environment condition. Every assertion reads <see cref="DriverInfo"/>,
/// the ring's <see cref="NetworkManager"/> or the state a binding holds,
/// never the serial log.
/// <para>
/// Which transport a cell presents is a property of the QEMU profile, not of
/// the architecture: x64 runs PCI only (q35 has no virtio-mmio window) while
/// arm64 runs both an MMIO cell and a PCI one. The same kernel binary serves
/// every cell of an architecture, so the transport is detected at runtime
/// from the tree: on a PCI cell the function node is bound by
/// <see cref="VirtioPciTransportDriver"/>, on an MMIO cell the platform slot
/// by <see cref="VirtioMmioTransportDriver"/>, and either publishes one
/// virtio node per device whose path names the transport. The leaf drivers,
/// <see cref="VirtioNetDriver"/> and <see cref="VirtioInputDriver"/>, bind
/// that node the same way on both, which is what the shared tests prove.
/// </para>
/// <para>
/// The kernel holds an <c>InternalsVisibleTo</c> grant from
/// <c>Cosmos.Kernel.HAL</c> for two purposes: reaching a node's binding state
/// through <see cref="DriverEngine.Nodes"/>, for the flags
/// <see cref="VirtioNetState"/> records and no diagnostic snapshot carries,
/// and reading <see cref="PlatformHAL.Architecture"/> for the one cell whose
/// interrupt mode depends on it.
/// </para>
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests: 6 net, 2 input, 2 PCI transport, 2 MMIO transport.</summary>
    private const int ExpectedTestCount = 12;

    /// <summary>Reason surfaced for the PCI transport tests when the cell runs virtio over MMIO.</summary>
    private const string SkipNotPci = "this cell presents virtio over MMIO";

    /// <summary>Reason surfaced for the MMIO transport tests when the cell runs virtio over PCI.</summary>
    private const string SkipNotMmio = "this cell presents virtio over PCI";

    /// <summary>Bus name of the nodes a virtio transport driver publishes.</summary>
    private const string VirtioBusName = "virtio";

    /// <summary>Bus name of the function nodes the PCI host driver publishes.</summary>
    private const string PciBusName = "pci";

    /// <summary>Bus name of the root nodes the machine description publishes, the virtio-mmio slots among them.</summary>
    private const string PlatformBusName = "platform";

    /// <summary>Start of a virtio node's description for a network device (type 1); the trailing space keeps type 18 out.</summary>
    private const string NetDescriptionPrefix = "type 1 ";

    /// <summary>Start of a virtio node's description for an input device (type 18).</summary>
    private const string InputDescriptionPrefix = "type 18 ";

    /// <summary>Start of a function node's description for a modern virtio-net function (vendor 1af4, device 0x1040 + 1).</summary>
    private const string ModernNetFunctionPrefix = "1af4:1041";

    /// <summary>Start of a function node's description for a transitional virtio-net function (vendor 1af4, legacy device id 0x1000).</summary>
    private const string TransitionalNetFunctionPrefix = "1af4:1000";

    /// <summary>The compatible string a virtio-mmio slot's platform node lists in its description.</summary>
    private const string MmioCompatible = "virtio,mmio";

    /// <summary>Start of the path of a virtio node published by the PCI transport.</summary>
    private const string PciPathPrefix = "virtio:pci:";

    /// <summary>Start of the path of a virtio node published by the MMIO transport.</summary>
    private const string MmioPathPrefix = "virtio:mmio:";

    /// <summary>Children a transport node has: the one virtio node it publishes for its device.</summary>
    private const int TransportChildCount = 1;

    // True when this cell put a virtio-net function on the PCI bus.
    //
    // Deliberately derived from the function node's presence in the tree,
    // not from the bound driver: gating on the driver would mean a device
    // that failed to bind takes its own PCI tests down with it into a green
    // skip, which is precisely the regression this suite exists to catch.
    // Keyed off the hardware, a bind failure leaves the PCI tests running,
    // and failing.
    private static bool s_isPciCell;

    // Captured once in BeforeRun, as paths: a test takes a fresh snapshot of
    // its node, and nothing here retracts a node between tests.
    private static string? s_netPath;
    private static string[] s_inputPaths = [];
    private static string? s_pciFunctionPath;

    /// <inheritdoc/>
    protected override void BeforeRun()
    {
        Log.WriteString("[Virtio] BeforeRun() reached!\n");

        TR.Start("Virtio Device Tests", expectedTests: ExpectedTestCount);

        s_netPath = FindNodePath(VirtioBusName, NetDescriptionPrefix);
        s_inputPaths = FindInputPaths();
        s_pciFunctionPath = FindNodePath(PciBusName, ModernNetFunctionPrefix) ?? FindNodePath(PciBusName, TransitionalNetFunctionPrefix);
        s_isPciCell = s_pciFunctionPath is not null;

        // ==================== Binding ====================
        TR.Run("Net_DriverBound", TestNet_DriverBound);
        TR.Run("Net_TransportMatchesCell", TestNet_TransportMatchesCell);
        TR.Run("Net_DeviceReady", TestNet_DeviceReady);
        TR.Run("Net_LinkUp", TestNet_LinkUp);
        TR.Run("Net_MacAddressProgrammed", TestNet_MacAddressProgrammed);
        TR.Run("Net_InterruptModeMatchesCell", TestNet_InterruptModeMatchesCell);

        // ==================== Input ====================
        TR.Run("Input_KeyboardBound", TestInput_KeyboardBound);
        TR.Run("Input_MouseBound", TestInput_MouseBound);

        // ==================== PCI transport ====================
        TR.RunIf(s_isPciCell, "Pci_FunctionBoundByTransport", TestPci_FunctionBoundByTransport, SkipNotPci);
        TR.RunIf(s_isPciCell, "Pci_Version1Negotiated", TestPci_Version1Negotiated, SkipNotPci);

        // ==================== MMIO transport ====================
        TR.RunIf(!s_isPciCell, "Mmio_SlotBoundByTransport", TestMmio_SlotBoundByTransport, SkipNotMmio);
        TR.RunIf(!s_isPciCell, "Mmio_AnyLayoutNegotiated", TestMmio_AnyLayoutNegotiated, SkipNotMmio);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    /// <inheritdoc/>
    protected override void Run() => Stop();

    /// <inheritdoc/>
    protected override void AfterRun()
    {
        TR.Complete();
        Sys.Power.Halt();
    }

    // ==================== Binding ====================

    // The virtio node exists only once a transport driver bound the
    // function or the slot and published it, so a missing node means the
    // transport half failed; a node in any state but Bound means the leaf
    // probe did.
    private static void TestNet_DriverBound()
    {
        if (!TryGetNetNode(out string? path, out DeviceNodeInfo info))
        {
            return;
        }

        Log.WriteString("[Test] Net node: ");
        Log.WriteString(path);
        Log.WriteString("\n");

        Assert.True(info.State == DeviceNodeState.Bound, "the virtio-net node should be bound");
        Assert.True(info.DriverName == nameof(VirtioNetDriver), "VirtioNetDriver should hold the virtio-net node");
    }

    // Cross-checks the bus against the path: if this cell put a virtio
    // function on the PCI bus, the node must have been published by the PCI
    // transport, and otherwise by the MMIO one. Catches a silent fallback to
    // the wrong transport, which would otherwise look like a healthy device
    // while none of the PCI-specific paths were ever exercised.
    private static void TestNet_TransportMatchesCell()
    {
        if (!TryGetNetNode(out string? path, out _))
        {
            return;
        }

        string expected = s_isPciCell ? PciPathPrefix : MmioPathPrefix;
        Assert.True(path.StartsWith(expected, StringComparison.Ordinal), "the virtio-net node's path should name the transport this cell presents: " + path);
    }

    // The driver publishes its interface only after the queues are created,
    // the receive ring is posted, a wake was secured and DRIVER_OK was set,
    // and the ring's consumer takes it from there; so a consumed device that
    // the manager reports ready covers the whole bring-up on this cell.
    private static void TestNet_DeviceReady()
    {
        if (!TryGetNetNode(out string? path, out _))
        {
            return;
        }

        int deviceIndex = FindDeviceIndex(PublishedDeviceKind.Network, path);
        Assert.True(deviceIndex >= 0, "the virtio-net interface should be in the published list as a network device");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.IsConsumed, "the ring's network manager should have taken the interface");
        }

        Assert.True(NetworkManager.DeviceCount >= 1, "the network manager should hold at least the kit's interface");
        Assert.True(NetworkManager.Ready, "the primary device should report ready after the probe");
    }

    private static void TestNet_LinkUp()
    {
        // QEMU's user-mode backend brings the link up immediately, so a down
        // link means the status word was read from the wrong config offset.
        Assert.True(NetworkManager.LinkUp, "virtio-net link should be up with QEMU user networking");
    }

    // The MAC is read byte by byte out of the device-specific configuration
    // region, which on PCI is located through its own vendor capability (on
    // QEMU an offset inside the same BAR as the common configuration; the
    // transport reads 0 without that capability or past its length). An
    // all-zero address means those reads landed nowhere.
    private static void TestNet_MacAddressProgrammed()
    {
        MACAddress? mac = NetworkManager.MacAddress;
        Assert.NotNull(mac);
        if (mac is null)
        {
            return;
        }

        Log.WriteString("[Test] MAC: ");
        Log.WriteString(mac.ToString());
        Log.WriteString("\n");

        Assert.False(mac.Equals(MACAddress.None), "MAC address read from device config should not be all zeros");
    }

    // Every cell routes one wake for the receive queue but the arm64 PCI cell
    // under acpi-off: there the GIC comes up on the virt defaults without an
    // ITS, the transport publishes the device with no message entry, and the
    // driver falls back to the periodic drain. Over MMIO the GIC line routes
    // with or without ACPI; on x64 the LAPIC does too (acpi=off leaves the
    // MADT in place). A driver on the wrong side of that line passes every
    // other test here, so the flags are read off the binding's state.
    private static void TestNet_InterruptModeMatchesCell()
    {
        if (!TryGetNetState(out VirtioNetState? state))
        {
            return;
        }

        if (ExpectsInterrupt())
        {
            Assert.True(state.HasInterrupt, "the receive queue's source should be connected on this cell");
            Assert.False(state.IsPolling, "a driver with an interrupt should not run the periodic drain");
        }
        else
        {
            Assert.False(state.HasInterrupt, "the receive queue's source cannot be routed on this cell");
            Assert.True(state.IsPolling, "a driver without an interrupt should run the periodic drain");
        }
    }

    /// <summary>True on every cell but the arm64 PCI cell under acpi-off.</summary>
    private static bool ExpectsInterrupt() =>
        PlatformHAL.Architecture != PlatformArchitecture.ARM64 || !s_isPciCell || !TR.ProfileContains("acpi-off");

    // ==================== Input ====================
    //
    // Both profiles attach a virtio keyboard and mouse. Binding them
    // exercises the event-type probe, which reads the input config
    // select/subsel window, a different device-config access pattern from
    // the NIC's flat MAC read, and the ring's keyboard and pointer
    // consumers, which take what the driver publishes.

    private static void TestInput_KeyboardBound()
    {
        AssertInputBound(PublishedDeviceKind.Keyboard, "keyboard");
    }

    private static void TestInput_MouseBound()
    {
        AssertInputBound(PublishedDeviceKind.Pointer, "pointer");
    }

    // ==================== PCI transport ====================

    // The transport driver matches every virtio function; a function that
    // enumerates but is not bound by it means capability parsing rejected
    // the device, and the leaf never saw a node. The one child is that node.
    private static void TestPci_FunctionBoundByTransport()
    {
        if (!TryGetPciFunction(out DeviceNodeInfo info))
        {
            return;
        }

        Assert.True(info.State == DeviceNodeState.Bound, "the virtio-net function should be bound");
        Assert.True(info.DriverName == nameof(VirtioPciTransportDriver), "VirtioPciTransportDriver should hold the virtio-net function");
        Assert.Equal(TransportChildCount, info.ChildCount, "the transport should publish one virtio node for the function");
    }

    // Modern virtio-pci must land on VERSION_1, which is what selects the
    // 12-byte net header. Negotiating it away while the driver still sized
    // the header for it would corrupt every frame. QEMU's virtio-mmio is
    // legacy by default, so the MMIO cell does not assert it.
    private static void TestPci_Version1Negotiated()
    {
        if (!TryGetNetState(out VirtioNetState? state))
        {
            return;
        }

        Assert.True(state.Version1Negotiated, "virtio-pci should negotiate VIRTIO_F_VERSION_1");
    }

    // ==================== MMIO transport ====================

    // The machine description publishes one platform node per virtio-mmio
    // slot that presents a device; the transport driver binds it and
    // publishes the virtio node beneath. This is the assertion that catches
    // a lost MMIO window, which only this cell can see: with no slot in the
    // tree there is nothing for the net tests to fail on but the absence.
    private static void TestMmio_SlotBoundByTransport()
    {
        int count = DriverInfo.NodeCount;
        int slots = 0;
        for (int i = 0; i < count; i++)
        {
            if (!DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                || info.BusName != PlatformBusName
                || !info.Description.Contains(MmioCompatible, StringComparison.Ordinal))
            {
                continue;
            }

            slots++;
            Assert.True(info.State == DeviceNodeState.Bound, "a virtio-mmio slot should be bound: " + info.Path);
            Assert.True(info.DriverName == nameof(VirtioMmioTransportDriver), "VirtioMmioTransportDriver should hold the slot: " + info.Path);
            Assert.Equal(TransportChildCount, info.ChildCount, "the transport should publish one virtio node for the slot: " + info.Path);
        }

        Assert.True(slots > 0, "a platform node whose description names virtio,mmio should be in the tree");
    }

    // QEMU's virtio-mmio is legacy by default, and on the legacy interface
    // VIRTIO_F_ANY_LAYOUT (any_layout defaults on) is the only way the
    // driver keeps the net header and the frame in one descriptor; without
    // it the probe declines, so a bound node with the bit clear is a
    // negotiation bug.
    private static void TestMmio_AnyLayoutNegotiated()
    {
        if (!TryGetNetState(out VirtioNetState? state))
        {
            return;
        }

        Assert.True(state.AnyLayoutNegotiated, "the legacy MMIO device should negotiate VIRTIO_F_ANY_LAYOUT");
    }

    // ==================== Helpers ====================

    /// <summary>
    /// Asserts that one of the virtio-input nodes captured by BeforeRun is
    /// bound by <see cref="VirtioInputDriver"/> and published a device of
    /// the given kind that the ring consumed.
    /// </summary>
    /// <param name="kind">The kind the driver publishes for the device.</param>
    /// <param name="name">The kind in words, for the messages.</param>
    private static void AssertInputBound(PublishedDeviceKind kind, string name)
    {
        Assert.True(s_inputPaths.Length > 0, "virtio-input nodes should be in the tree");

        bool found = false;
        for (int i = 0; i < s_inputPaths.Length; i++)
        {
            string path = s_inputPaths[i];
            if (!TryFindNode(path, out DeviceNodeInfo info)
                || info.State != DeviceNodeState.Bound
                || info.DriverName != nameof(VirtioInputDriver))
            {
                continue;
            }

            int deviceIndex = FindDeviceIndex(kind, path);
            if (deviceIndex >= 0
                && DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device)
                && device.IsConsumed)
            {
                found = true;
                break;
            }
        }

        Assert.True(found, "a virtio-input node bound by VirtioInputDriver should publish a consumed " + name);
    }

    /// <summary>
    /// Finds the path of the first node on the given bus whose description
    /// starts with the given text, whatever its state. Compared ordinally,
    /// as every string in kernel test code is.
    /// </summary>
    /// <param name="busName">The bus the node sits on.</param>
    /// <param name="descriptionPrefix">Start of the node's description.</param>
    /// <returns>The node's path, or null when no such node is in the tree.</returns>
    private static string? FindNodePath(string busName, string descriptionPrefix)
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == busName
                && info.Description.StartsWith(descriptionPrefix, StringComparison.Ordinal))
            {
                return info.Path;
            }
        }

        return null;
    }

    /// <summary>
    /// Collects the paths of every virtio node describing an input device
    /// (type 18), whatever its state, in publication order.
    /// </summary>
    /// <returns>The paths; empty when no input device is in the tree.</returns>
    private static string[] FindInputPaths()
    {
        int count = DriverInfo.NodeCount;
        int matches = 0;
        for (int i = 0; i < count; i++)
        {
            if (IsInputNode(i))
            {
                matches++;
            }
        }

        string[] paths = new string[matches];
        int next = 0;
        for (int i = 0; i < count && next < matches; i++)
        {
            if (IsInputNode(i) && DriverInfo.TryGetNode(i, out DeviceNodeInfo info))
            {
                paths[next] = info.Path;
                next++;
            }
        }

        return paths;
    }

    /// <summary>Whether the node at the given position is a virtio node describing an input device.</summary>
    /// <param name="index">Publication position of the node.</param>
    /// <returns>True for a virtio node of type 18.</returns>
    private static bool IsInputNode(int index) =>
        DriverInfo.TryGetNode(index, out DeviceNodeInfo info)
        && info.BusName == VirtioBusName
        && info.Description.StartsWith(InputDescriptionPrefix, StringComparison.Ordinal);

    /// <summary>Hands back the virtio-net node's path and a fresh snapshot of it, or fails the test when BeforeRun found none.</summary>
    /// <param name="path">The node's path.</param>
    /// <param name="info">The node's snapshot.</param>
    /// <returns>True when the node is in the tree.</returns>
    private static bool TryGetNetNode([NotNullWhen(true)] out string? path, out DeviceNodeInfo info)
    {
        path = s_netPath;
        if (path is null || !TryFindNode(path, out info))
        {
            Assert.Fail("no virtio-net node was found by BeforeRun");
            info = default;
            return false;
        }

        return true;
    }

    /// <summary>Hands back a fresh snapshot of the virtio-net function's node, or fails the test when BeforeRun found none.</summary>
    /// <param name="info">The node's snapshot.</param>
    /// <returns>True when the node is in the tree.</returns>
    private static bool TryGetPciFunction(out DeviceNodeInfo info)
    {
        string? path = s_pciFunctionPath;
        if (path is null || !TryFindNode(path, out info))
        {
            Assert.Fail("no virtio-net PCI function was found by BeforeRun");
            info = default;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Hands back the virtio-net node's driver state, read from the tree
    /// itself through the HAL grant: the flags the interrupt and negotiation
    /// tests read are on the state, which no diagnostic snapshot carries.
    /// Fails the test when the node is missing or not bound by the net
    /// driver.
    /// </summary>
    /// <param name="state">The driver state when found.</param>
    /// <returns>True when the state is there.</returns>
    private static bool TryGetNetState([NotNullWhen(true)] out VirtioNetState? state)
    {
        state = null;
        if (!TryGetNetNode(out string? path, out _))
        {
            return false;
        }

        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode node = nodes[i];
            if (node.Path == path)
            {
                state = node.Binding?.DriverState as VirtioNetState;
                break;
            }
        }

        Assert.NotNull(state, "the virtio-net node's binding should hold a VirtioNetState");
        return state is not null;
    }

    /// <summary>Finds the published device of the given kind whose node has the given path.</summary>
    /// <param name="kind">The kind of device.</param>
    /// <param name="nodePath">The path of the node whose driver published the device.</param>
    /// <returns>Its position in the published list, or -1.</returns>
    private static int FindDeviceIndex(PublishedDeviceKind kind, string nodePath)
    {
        int count = DriverInfo.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetDevice(i, out PublishedDeviceInfo info) && info.Kind == kind && info.NodePath == nodePath)
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindNodeIndex(string path)
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info) && info.Path == path)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool TryFindNode(string path, out DeviceNodeInfo info) => DriverInfo.TryGetNode(FindNodeIndex(path), out info);
}
