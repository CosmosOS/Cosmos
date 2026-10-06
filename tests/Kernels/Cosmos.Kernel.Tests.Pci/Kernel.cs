using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Platform;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Pci;

public class Kernel : Sys.Kernel
{
    // Every PCI function node in the kit's tree: a node whose identity is a
    // PciIdentity and whose access object is a PciAccess. Used by every
    // ConfigSpace_* test; captured once at BeforeRun time, after the engine
    // offered every node, so a transient state change between tests can't
    // show up as cross-test interference.
    private static DeviceNode[] s_functions = [];

    // Reason string surfaced through TR.RunIf when a test depends on at
    // least one PCI function node. The ARM64 description takes the ECAM
    // host from MCFG or, without ACPI, from the device tree, so only a
    // machine with no host at all (PCI compiled out, or neither source
    // names one) lands here and the config space tests skip cleanly; the
    // Host_* tests stay unconditional and fail there instead.
    private const string SkipNoFunction = "no PCI function node in the tree, the PCI host was not described or published nothing";

    /// <summary>Number of tests announced to the runner in TR.Start: 4 host, 4 config space.</summary>
    private const int ExpectedTestCount = 8;

    /// <summary>All-ones vendor/device id returned by an unmapped or empty config-space read (PCI spec: 0xFFFF = no device).</summary>
    private const ushort AllOnesId = 0xFFFF;

    /// <summary>All-zeros vendor id, the pattern seen when a config-space read hits stale/zeroed memory instead of the device.</summary>
    private const ushort AllZerosVendorId = 0x0000;

    /// <summary>Highest spec-defined PCI base class code (0x00..0x13 per PCI-SIG class code list; 0xFF is "unassigned").</summary>
    private const byte MaxDefinedClassCode = 0x13;

    /// <summary>Vendor ID register offset in PCI configuration space (16-bit, offset 0x00).</summary>
    private const ushort VendorIdRegisterOffset = 0x00;

    /// <summary>Device ID register offset in PCI configuration space (16-bit, offset 0x02).</summary>
    private const ushort DeviceIdRegisterOffset = 0x02;

    /// <summary>Base class code register offset in PCI configuration space (8-bit, offset 0x0B).</summary>
    private const ushort ClassCodeRegisterOffset = 0x0B;

    /// <summary>Header type register offset in PCI configuration space (8-bit, offset 0x0E).</summary>
    private const ushort HeaderTypeRegisterOffset = 0x0E;

    /// <summary>Header type bit 7: the device implements functions beyond 0.</summary>
    private const byte MultiFunctionBit = 0x80;

    /// <summary>Highest device number on a bus: the device field is five bits.</summary>
    private const byte MaxDevice = 31;

    /// <summary>Highest function number of a device: the function field is three bits.</summary>
    private const byte MaxFunction = 7;

    /// <summary>Bus name of the root nodes the machine description publishes, the PCI host among them.</summary>
    private const string PlatformBusName = "platform";

    /// <summary>Bus name of the function nodes the PCI host driver publishes.</summary>
    private const string PciBusName = "pci";

    /// <summary>Name of the driver kit's PCI host driver, as DriverInfo reports it.</summary>
    private const string PciHostDriverName = "PciHostDriver";

    /// <summary>Prefix shared by the host's compatible strings (pci-host-legacy on x64, pci-host-ecam-generic on arm64), which the platform node's description lists.</summary>
    private const string PciHostCompatiblePrefix = "pci-host-";

    /// <summary>Compatible string of the host over the x86 port mechanism (the q35 description).</summary>
    private const string LegacyHostCompatible = "pci-host-legacy";

    /// <summary>Compatible string of a host over an ECAM window (the virt description, from MCFG or the device tree).</summary>
    private const string EcamHostCompatible = "pci-host-ecam-generic";

    /// <summary>Bytes of configuration space per function through the port mechanism.</summary>
    private const int PortMechanismSize = 256;

    /// <summary>Bytes of configuration space per function through ECAM.</summary>
    private const int EcamMechanismSize = 4096;

    /// <summary>CONFIG_ADDRESS, the first port of the legacy host's one port range resource.</summary>
    private const ulong ConfigAddressPort = 0xCF8;

    /// <summary>Ports of the legacy host's resource: CONFIG_ADDRESS and CONFIG_DATA, four each.</summary>
    private const ulong ConfigPortCount = 8;

    /// <summary>Shift of the bus number in an ECAM address: 1 MiB of configuration space per bus.</summary>
    private const int EcamBusShift = 20;

    /// <summary>Resources every function node carries: one per base address register slot, assigned or not (PCI type 0 headers have six).</summary>
    private const int PciFunctionResourceCount = 6;

    /// <summary>Line interrupt sources every function node carries first: its legacy line, routed or not, before any message source.</summary>
    private const int PciFunctionLineCount = 1;

    /// <summary>Prefix of the description of a function's legacy line source ("line 11" or "line (none)").</summary>
    private const string LineSourcePrefix = "line";

    /// <summary>Prefix of the description of a function's first message source, completed by the size of its MSI-X table.</summary>
    private const string FirstMessageSourcePrefix = "message 0 of ";

    protected override void BeforeRun()
    {
        Log.WriteString("[Pci] BeforeRun() reached!\n");

        TR.Start("PCI Subsystem Tests", expectedTests: ExpectedTestCount);

        s_functions = CollectFunctionNodes();
        bool anyFunction = s_functions.Length > 0;

        // ==================== Host ====================
        // Unconditional: the default cell on either arch carries a PCI host
        // (the port mechanism on q35, ECAM from MCFG on virt with EDK2), so
        // a missing or unbound host node, or a host that published no
        // function, is a regression in the machine description or the host
        // driver, the one thing this suite exists to catch, not an
        // environment condition.
        TR.Run("Host_PlatformNode_BoundByPciHostDriver", TestHost_PlatformNodeBoundByPciHostDriver);
        TR.Run("Host_ConfigMechanism_MatchesCompatible", TestHost_ConfigMechanismMatchesCompatible);
        TR.Run("Host_PublishesPciNodes", TestHost_PublishesPciNodes);
        TR.Run("Host_NodeCount_MatchesConfigSpaceScan", TestHost_NodeCountMatchesConfigSpaceScan);

        // ==================== ConfigSpace ====================
        TR.RunIf(anyFunction, "ConfigSpace_VendorId_NotAllOnes", TestConfigSpace_VendorIdNotAllOnes, SkipNoFunction);
        TR.RunIf(anyFunction, "ConfigSpace_DeviceId_NotAllOnes", TestConfigSpace_DeviceIdNotAllOnes, SkipNoFunction);
        TR.RunIf(anyFunction, "ConfigSpace_ClassCode_InRange", TestConfigSpace_ClassCodeInRange, SkipNoFunction);
        TR.RunIf(anyFunction, "ConfigSpace_VendorRead_StableAcrossCalls", TestConfigSpace_VendorReadStable, SkipNoFunction);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Host ====================
    //
    // The driver kit's view of the bus, enumerated through DriverInfo: the
    // platform node the machine description publishes for the PCI host,
    // the PciHostDriver that binds it, and the function nodes the driver
    // publishes beneath it. The configuration mechanism, the per-function
    // interrupt check and the raw scan read the kit node itself through the
    // suite's HAL grant, since the host's PciHostAccess, a function's MSI-X
    // table and its mechanism are on the node, which no snapshot carries.

    private static void TestHost_PlatformNodeBoundByPciHostDriver()
    {
        Assert.True(TryFindHostNode(out DeviceNodeInfo host), "a platform node whose description names a pci-host compatible should be in the tree");
        Assert.True(host.State == DeviceNodeState.Bound, "the host node should be bound");
        Assert.True(host.DriverName == PciHostDriverName, "PciHostDriver should hold the host node");
    }

    // The host's platform node carries the mechanism its compatible string
    // names: the machine's one port mechanism (256 bytes per function,
    // resource CONFIG_ADDRESS..CONFIG_DATA) for pci-host-legacy, an ECAM
    // window (4096 bytes per function, resource spanning the buses the host
    // mapped) for pci-host-ecam-generic; and every function on the host's
    // buses is reached through that same mechanism.
    private static void TestHost_ConfigMechanismMatchesCompatible()
    {
        if (!TryFindHostKitNode(out DeviceNode? hostNode, out PciHostAccess? host))
        {
            Assert.Fail("no platform node carries a PciHostAccess");
            return;
        }

        if (hostNode.Identity is not PlatformIdentity identity)
        {
            Assert.Fail("the host node's identity should be a platform identity: " + hostNode.Path);
            return;
        }

        Assert.True(host.StartBus <= host.EndBus, "the host should decode at least one bus: " + hostNode.Path);
        PciConfigSpace mechanism = host.ConfigSpace;
        IReadOnlyList<DeviceResource> resources = hostNode.Resources;
        if (identity.IsCompatible(LegacyHostCompatible))
        {
            Assert.True(mechanism is PciPortConfigSpace, "the legacy host should reach configuration space through the port mechanism");
            Assert.True(ReferenceEquals(mechanism, PciConfigSpace.Ports), "the legacy host should share the machine's one port mechanism, its one latch and lock");
            Assert.Equal(PortMechanismSize, mechanism.Size, "the port mechanism reaches 256 bytes per function");
            Assert.True(resources.Count == 1
                && resources[0].Kind == DeviceResourceKind.PortRange
                && resources[0].Base == ConfigAddressPort
                && resources[0].Length == ConfigPortCount, "the legacy host's one resource should be the CONFIG_ADDRESS and CONFIG_DATA ports");
        }
        else if (identity.IsCompatible(EcamHostCompatible))
        {
            Assert.True(mechanism is PciEcamConfigSpace, "the ECAM host should reach configuration space through its ECAM window");
            Assert.Equal(EcamMechanismSize, mechanism.Size, "ECAM reaches 4096 bytes per function");
            ulong windowLength = (ulong)(host.EndBus - host.StartBus + 1) << EcamBusShift;
            Assert.True(resources.Count == 1
                && resources[0].Kind == DeviceResourceKind.MemoryWindow
                && resources[0].Length == windowLength, "the ECAM host's one resource should be the window over the buses it mapped");
        }
        else
        {
            Assert.Fail("the host node names neither pci-host-legacy nor pci-host-ecam-generic: " + hostNode.Path);
        }

        for (int i = 0; i < s_functions.Length; i++)
        {
            DeviceNode node = s_functions[i];
            if (node.Identity is PciIdentity function && IsOnHost(function, host))
            {
                Assert.True(ReferenceEquals(node.Access<PciAccess>().ConfigSpace, mechanism), "a function on the host's buses should be reached through the host's mechanism: " + node.Path);
            }
        }
    }

    private static void TestHost_PublishesPciNodes()
    {
        if (!TryFindHostNode(out DeviceNodeInfo host))
        {
            Assert.Fail("the host node was not found");
            return;
        }

        int published = 0;
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (!DriverInfo.TryGetNode(i, out DeviceNodeInfo info) || info.BusName != PciBusName || info.ParentPath != host.Path)
            {
                continue;
            }

            published++;
            Assert.Equal(PciFunctionResourceCount, info.ResourceCount, "a function node carries one resource per BAR slot: " + info.Path);

            if (!TryFindKitNode(info.Path, out DeviceNode? node))
            {
                Assert.Fail("the function node should be in the engine's tree: " + info.Path);
                continue;
            }

            PciAccess pci = node.Access<PciAccess>();
            int expectedInterrupts = PciFunctionLineCount
                + (pci.IsMsiXCapable ? Math.Min(pci.MessageInterruptCount, PciHostAccess.MaxDescribedMessages) : 0);
            Assert.Equal(expectedInterrupts, info.InterruptCount, "a function node carries its legacy line first, then one source per described message: " + info.Path);
            Assert.True(node.Interrupts[0].Describe().StartsWith(LineSourcePrefix, StringComparison.Ordinal), "the first source of a function node should be its legacy line: " + info.Path);
            if (pci.IsMsiXCapable)
            {
                Assert.True(string.Equals(node.Interrupts[1].Describe(), FirstMessageSourcePrefix + pci.MessageInterruptCount, StringComparison.Ordinal), "the second source of an MSI-X capable function should be its first message: " + info.Path);
            }
        }

        Assert.True(published >= 1, "the host driver should have published at least one function node under the host");
    }

    // An independent enumeration of the same configuration space: every
    // device of every bus the host decodes, read raw through the host's
    // PciHostAccess with the walk's presence rule (vendor id neither 0xFFFF
    // nor 0x0000; functions 1 to 7 only when function 0's header type has
    // the multi-function bit), against the function nodes on the host's
    // segment and buses. Equal, not at least: the default cells carry no
    // bus the host driver's walk cannot reach and no hot-plug slot, so a
    // function the walk missed or published twice is a regression.
    private static void TestHost_NodeCountMatchesConfigSpaceScan()
    {
        if (!TryFindHostKitNode(out DeviceNode? hostNode, out PciHostAccess? host))
        {
            Assert.Fail("no platform node carries a PciHostAccess");
            return;
        }

        int scanned = CountPresentFunctions(host);
        int published = 0;
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Identity is PciIdentity identity && IsOnHost(identity, host))
            {
                published++;
            }
        }

        Assert.True(scanned >= 1, "a raw scan of the host's buses should find at least one function: " + hostNode.Path);
        Assert.Equal(scanned, published, "the kit should publish one node per function a raw scan of the host's buses finds: " + hostNode.Path);
    }

    // ==================== ConfigSpace ====================
    //
    // Spot-checks against every function node, read live through the node's
    // own PciAccess. We don't pin any particular vendor/device id because
    // that varies by QEMU machine type and version; what we assert is that
    // the values look like a real config-space response rather than the
    // all-ones / all-zeros patterns that surface when ECAM is unmapped or
    // the bus is empty, and that they still agree with the identity the
    // host described at publish time.

    private static void TestConfigSpace_VendorIdNotAllOnes()
    {
        for (int i = 0; i < s_functions.Length; i++)
        {
            DeviceNode node = s_functions[i];
            PciIdentity identity = (PciIdentity)node.Identity;
            ushort vendorId = node.Access<PciAccess>().ReadConfig16(VendorIdRegisterOffset);
            Assert.True(vendorId != AllOnesId && vendorId != AllZerosVendorId, "the vendor id should read as a present function's: " + node.Path);
            Assert.Equal(identity.VendorId, vendorId, "the live vendor id should match the one the host described: " + node.Path);
        }
    }

    private static void TestConfigSpace_DeviceIdNotAllOnes()
    {
        for (int i = 0; i < s_functions.Length; i++)
        {
            DeviceNode node = s_functions[i];
            PciIdentity identity = (PciIdentity)node.Identity;
            ushort deviceId = node.Access<PciAccess>().ReadConfig16(DeviceIdRegisterOffset);
            Assert.True(deviceId != AllOnesId, "the device id should not read all ones: " + node.Path);
            Assert.Equal(identity.DeviceId, deviceId, "the live device id should match the one the host described: " + node.Path);
        }
    }

    private static void TestConfigSpace_ClassCodeInRange()
    {
        // PCI base class codes 0x00..0x13 are spec-defined; 0xFF is
        // reserved for "unassigned". Anything outside means the byte we
        // read is not a real class code (typically a stale-cache or
        // unmapped read returning all-ones).
        for (int i = 0; i < s_functions.Length; i++)
        {
            DeviceNode node = s_functions[i];
            PciIdentity identity = (PciIdentity)node.Identity;
            byte classCode = node.Access<PciAccess>().ReadConfig8(ClassCodeRegisterOffset);
            Assert.True(identity.ClassCode <= MaxDefinedClassCode, "the described class code should be a spec-defined one: " + node.Path);
            Assert.Equal(identity.ClassCode, classCode, "the live class code should match the one the host described: " + node.Path);
        }
    }

    private static void TestConfigSpace_VendorReadStable()
    {
        // Two reads of the same offset through the node's access must
        // agree, and agree with the host's raw read of the same register.
        // Catches half-baked ECAM mappings (one read hits cached zeros, the
        // next hits real config), register-side-effect bugs in ReadConfig16
        // (it must be a pure read, not advance any internal pointer), and
        // an access built over another mechanism or address than the host's.
        PciHostAccess? host = TryFindHostKitNode(out _, out PciHostAccess? found) ? found : null;
        for (int i = 0; i < s_functions.Length; i++)
        {
            DeviceNode node = s_functions[i];
            PciIdentity identity = (PciIdentity)node.Identity;
            PciAccess pci = node.Access<PciAccess>();
            ushort first = pci.ReadConfig16(VendorIdRegisterOffset);
            ushort second = pci.ReadConfig16(VendorIdRegisterOffset);
            Assert.Equal(first, second, "two reads of the vendor id through the node's access should agree: " + node.Path);
            if (host is not null && IsOnHost(identity, host))
            {
                ushort raw = host.ReadConfig16(identity.Bus, identity.Device, identity.Function, VendorIdRegisterOffset);
                Assert.Equal(first, raw, "the node's read and the host's raw read of the vendor id should agree: " + node.Path);
            }
        }
    }

    // ==================== Helpers ====================

    /// <summary>
    /// Finds the PCI host's platform node: the node on the platform bus
    /// whose description (the identity's compatible strings) names a
    /// pci-host compatible. Compared ordinally, as every string in kernel
    /// test code is.
    /// </summary>
    /// <param name="host">The host node's snapshot when found.</param>
    /// <returns>True when the node is in the tree.</returns>
    private static bool TryFindHostNode(out DeviceNodeInfo host)
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == PlatformBusName
                && info.Description.Contains(PciHostCompatiblePrefix, StringComparison.Ordinal))
            {
                host = info;
                return true;
            }
        }

        host = default;
        return false;
    }

    /// <summary>
    /// Finds the PCI host's platform node in the engine's tree itself,
    /// through the HAL grant: the first node on the platform bus whose
    /// access object is a <see cref="PciHostAccess"/>, which no diagnostic
    /// snapshot carries.
    /// </summary>
    /// <param name="node">The host node when found.</param>
    /// <param name="host">The node's host access when found.</param>
    /// <returns>True when such a node is in the tree.</returns>
    private static bool TryFindHostKitNode([NotNullWhen(true)] out DeviceNode? node, [NotNullWhen(true)] out PciHostAccess? host)
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode candidate = nodes[i];
            if (string.Equals(candidate.Identity.BusName, PlatformBusName, StringComparison.Ordinal)
                && candidate.TryGetAccess(out PciHostAccess? access))
            {
                node = candidate;
                host = access;
                return true;
            }
        }

        node = null;
        host = null;
        return false;
    }

    /// <summary>
    /// Finds the kit node with the given path in the engine's tree itself,
    /// through the HAL grant: the access object and the interrupt sources
    /// the host test reads are on the node, which no diagnostic snapshot
    /// carries. Paths are compared ordinally.
    /// </summary>
    /// <param name="path">The node's path, as DriverInfo reports it.</param>
    /// <param name="node">The node when found.</param>
    /// <returns>True when a node with that path is in the tree.</returns>
    private static bool TryFindKitNode(string path, [NotNullWhen(true)] out DeviceNode? node)
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode candidate = nodes[i];
            if (string.Equals(candidate.Path, path, StringComparison.Ordinal))
            {
                node = candidate;
                return true;
            }
        }

        node = null;
        return false;
    }

    /// <summary>Every node of the tree whose identity is a <see cref="PciIdentity"/> and whose access object is a <see cref="PciAccess"/>, in publication order.</summary>
    /// <returns>The function nodes; empty when the tree holds none.</returns>
    private static DeviceNode[] CollectFunctionNodes()
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        List<DeviceNode> functions = [];
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode node = nodes[i];
            if (node.Identity is PciIdentity && node.TryGetAccess<PciAccess>(out _))
            {
                functions.Add(node);
            }
        }

        return functions.ToArray();
    }

    /// <summary>True when the function sits on the host's segment and within the buses it decodes.</summary>
    /// <param name="identity">The function's identity.</param>
    /// <param name="host">The host's access.</param>
    private static bool IsOnHost(PciIdentity identity, PciHostAccess host) =>
        identity.Segment == host.Segment && identity.Bus >= host.StartBus && identity.Bus <= host.EndBus;

    /// <summary>
    /// Counts the functions present on every bus the host decodes, read raw
    /// through <see cref="PciHostAccess.ReadConfig16"/> and
    /// <see cref="PciHostAccess.ReadConfig8"/>: function 0 of each device,
    /// then functions 1 to 7 when its header type has the multi-function bit.
    /// </summary>
    /// <param name="host">The host's access.</param>
    /// <returns>How many functions answered.</returns>
    private static int CountPresentFunctions(PciHostAccess host)
    {
        int count = 0;
        for (int bus = host.StartBus; bus <= host.EndBus; bus++)
        {
            for (byte device = 0; device <= MaxDevice; device++)
            {
                if (!IsPresent(host, (byte)bus, device, 0))
                {
                    continue;
                }

                count++;
                bool multiFunction = (host.ReadConfig8((byte)bus, device, 0, HeaderTypeRegisterOffset) & MultiFunctionBit) != 0;
                if (!multiFunction)
                {
                    continue;
                }

                for (byte function = 1; function <= MaxFunction; function++)
                {
                    if (IsPresent(host, (byte)bus, device, function))
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    /// <summary>True when the function's vendor id reads neither 0xFFFF nor 0x0000.</summary>
    private static bool IsPresent(PciHostAccess host, byte bus, byte device, byte function)
    {
        ushort vendorId = host.ReadConfig16(bus, device, function, VendorIdRegisterOffset);
        return vendorId != AllOnesId && vendorId != AllZerosVendorId;
    }
}
