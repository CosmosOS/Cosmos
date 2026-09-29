using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Pci;

public class Kernel : Sys.Kernel
{
    // First device PciManager enumerated. Used by every ConfigSpace_* test;
    // captured once at BeforeRun time so a transient state change between
    // tests can't show up as cross-test interference.
    private static PciDevice? s_firstDevice;

    // Reason string surfaced through TR.RunIf when a test depends on at
    // least one PCI device having been enumerated. A profile that disables
    // ACPI on arm64 (no MCFG, no FDT fallback for the ECAM base) lands
    // here and the device tests skip cleanly.
    private const string SkipNoDevice = "no PCI devices enumerated, host bridge / ECAM not discovered";

    /// <summary>Number of tests announced to the runner in TR.Start: 2 manager, 4 config space, 3 host.</summary>
    private const int ExpectedTestCount = 9;

    /// <summary>All-ones vendor/device id returned by an unmapped or empty config-space read (PCI spec: 0xFFFF = no device).</summary>
    private const ushort AllOnesId = 0xFFFF;

    /// <summary>All-zeros vendor id, the pattern seen when a config-space read hits stale/zeroed memory instead of the device.</summary>
    private const ushort AllZerosVendorId = 0x0000;

    /// <summary>Highest spec-defined PCI base class code (0x00..0x13 per PCI-SIG class code list; 0xFF is "unassigned").</summary>
    private const byte MaxDefinedClassCode = 0x13;

    /// <summary>Vendor ID register offset in PCI configuration space (16-bit, offset 0x00).</summary>
    private const byte VendorIdRegisterOffset = 0x00;

    /// <summary>Bus name of the root nodes the machine description publishes, the PCI host among them.</summary>
    private const string PlatformBusName = "platform";

    /// <summary>Bus name of the function nodes the PCI host driver publishes.</summary>
    private const string PciBusName = "pci";

    /// <summary>Name of the driver kit's PCI host driver, as DriverInfo reports it.</summary>
    private const string PciHostDriverName = "PciHostDriver";

    /// <summary>Prefix shared by the host's compatible strings (pci-host-legacy on x64, pci-host-ecam-generic on arm64), which the platform node's description lists.</summary>
    private const string PciHostCompatiblePrefix = "pci-host-";

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

        s_firstDevice = PciManager.Count > 0 ? PciManager.Devices![0] : null;
        bool anyDevice = s_firstDevice != null;

        // ==================== Manager ====================
        TR.Run("Manager_Initialized", TestManager_Initialized);
        // Unconditional on purpose: this suite's only cell is the default
        // q35/virt machine, where zero enumerated devices is an enumeration
        // regression, the one thing this suite exists to catch, not an
        // environment condition. Gating it on anyDevice (= Count > 0) made
        // it a tautology that converted such a regression into 5 skips and
        // a green CI. anyDevice keeps gating only the per-device
        // ConfigSpace spot-checks below.
        TR.Run("Manager_HasDevices", TestManager_HasDevices);

        // ==================== ConfigSpace ====================
        TR.RunIf(anyDevice, "ConfigSpace_VendorId_NotAllOnes", TestConfigSpace_VendorIdNotAllOnes, SkipNoDevice);
        TR.RunIf(anyDevice, "ConfigSpace_DeviceId_NotAllOnes", TestConfigSpace_DeviceIdNotAllOnes, SkipNoDevice);
        TR.RunIf(anyDevice, "ConfigSpace_ClassCode_InRange", TestConfigSpace_ClassCodeInRange, SkipNoDevice);
        TR.RunIf(anyDevice, "ConfigSpace_VendorRead_StableAcrossCalls", TestConfigSpace_VendorReadStable, SkipNoDevice);

        // ==================== Host ====================
        // Unconditional like Manager_HasDevices: the default cell on either
        // arch carries a PCI host (the port mechanism on q35, ECAM from
        // MCFG on virt with EDK2), so a missing or unbound host node is a
        // regression in the machine description or the host driver.
        TR.Run("Host_PlatformNode_BoundByPciHostDriver", TestHost_PlatformNodeBoundByPciHostDriver);
        TR.Run("Host_PublishesPciNodes", TestHost_PublishesPciNodes);
        TR.Run("Host_NodeCount_MatchesLegacyScan", TestHost_NodeCountMatchesLegacyScan);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Manager ====================

    // Devices array is allocated by PciManager.Setup() during boot, even when
    // zero devices end up being found. A null array means Setup never ran,
    // a kernel-init regression, not a "no PCI on this profile" condition.
    private static void TestManager_Initialized()
    {
        Assert.NotNull(PciManager.Devices);
    }

    private static void TestManager_HasDevices()
    {
        Assert.True(PciManager.Count > 0);
    }

    // ==================== ConfigSpace ====================
    //
    // Spot-checks against the first enumerated device. We don't pin any
    // particular vendor/device id because that varies by QEMU machine type
    // and version; what we assert is that the values look like a real
    // config-space response rather than the all-ones / all-zeros patterns
    // that surface when ECAM is unmapped or the bus is empty.

    private static void TestConfigSpace_VendorIdNotAllOnes()
    {
        Assert.True(s_firstDevice!.VendorId != AllOnesId && s_firstDevice.VendorId != AllZerosVendorId);
    }

    private static void TestConfigSpace_DeviceIdNotAllOnes()
    {
        Assert.True(s_firstDevice!.DeviceId != AllOnesId);
    }

    private static void TestConfigSpace_ClassCodeInRange()
    {
        // PCI base class codes 0x00..0x13 are spec-defined; 0xFF is
        // reserved for "unassigned". Anything outside means the byte we
        // read is not a real class code (typically a stale-cache or
        // unmapped read returning all-ones).
        Assert.True(s_firstDevice!.ClassCode <= MaxDefinedClassCode);
    }

    private static void TestConfigSpace_VendorReadStable()
    {
        // Two reads of the same offset must agree. Catches half-baked ECAM
        // mappings (one read hits cached zeros, the next hits real config),
        // and catches register-side-effect bugs in ReadRegister16 (it must
        // be a pure read, not advance any internal pointer).
        ushort first = s_firstDevice!.ReadRegister16(VendorIdRegisterOffset);
        ushort second = s_firstDevice.ReadRegister16(VendorIdRegisterOffset);
        Assert.Equal(first, second);
    }

    // ==================== Host ====================
    //
    // The driver kit's view of the same bus, enumerated through DriverInfo:
    // the platform node the machine description publishes for the PCI
    // host, the PciHostDriver that binds it, and the function nodes the
    // driver publishes beneath it. The per-function interrupt check reads
    // the kit node itself through the suite's HAL grant, since the expected
    // count depends on the function's MSI-X table, which the snapshot does
    // not carry. PciManager's legacy scan walks the same configuration
    // space, so the two enumerations are checked against each other.

    private static void TestHost_PlatformNodeBoundByPciHostDriver()
    {
        Assert.True(TryFindHostNode(out DeviceNodeInfo host), "a platform node whose description names a pci-host compatible should be in the tree");
        Assert.True(host.State == DeviceNodeState.Bound, "the host node should be bound");
        Assert.True(host.DriverName == PciHostDriverName, "PciHostDriver should hold the host node");
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

    private static void TestHost_NodeCountMatchesLegacyScan()
    {
        int pciNodes = CountNodesOnBus(PciBusName);
        uint legacyCount = PciManager.Count;
        Assert.True((uint)pciNodes >= legacyCount, "the kit should publish at least every function the legacy scan enumerated");
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

    /// <summary>Counts the nodes of the tree on the named bus, whatever their state.</summary>
    /// <param name="busName">The bus name to count, compared ordinally.</param>
    /// <returns>How many nodes sit on that bus.</returns>
    private static int CountNodesOnBus(string busName)
    {
        int matches = 0;
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info) && info.BusName == busName)
            {
                matches++;
            }
        }

        return matches;
    }
}
