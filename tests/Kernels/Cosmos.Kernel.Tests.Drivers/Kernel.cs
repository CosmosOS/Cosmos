// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Drivers;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.Tests.Drivers.Library;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using SysThread = System.Threading.Thread;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// Exercises the driver kit over the synthetic bus, with no hardware behind
/// any node: the manifest the build generated (content, policy and order),
/// arbitration, the unwinding of a declined or failed probe, publishing to a
/// consumer, interrupt delivery in a synthetic dispatch, deferred work,
/// periodic work, driver threads, teardown order and accounting, child
/// nodes, a display published to the ring's display manager, and a block
/// device published to the ring's storage manager. Every assertion reads
/// <see cref="DriverInfo"/>, the display manager, the storage manager or
/// the suite's own drivers and consumer, never the serial log. One node is published from
/// the constructor, before the driver stage, to cover the boot path; the
/// rest are published from the tests, which is the hot-plug path.
/// <para>
/// The hardware half runs the kit over the machine's real buses: the PCI
/// host node on both architectures, and on x64 the 82574L that q35 adds
/// when a cell names no NIC, bound by <see cref="E1000EDriver"/> from
/// <c>Cosmos.Kernel.Drivers</c> and published to the ring. virt's default
/// NIC is a virtio-net-pci function that the kit's VirtioNetDriver binds, so
/// the E1000E tests skip on arm64.
/// </para>
/// <para>
/// The suite is two projects. This kernel is the harness: it holds an
/// <c>InternalsVisibleTo</c> grant from <c>Cosmos.Kernel.HAL</c> for one
/// purpose, installing <see cref="TestKeyboardConsumer"/> through the
/// internal <see cref="DeviceRegistry"/>. The drivers it drives live in
/// <c>Cosmos.Kernel.Tests.Drivers.Library</c>, a driver assembly with no
/// grant at all, written over the public seam only, so their compiling is
/// the proof that a third party can write every one of them.
/// </para>
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests: 6 manifest, 2 engine, 5 arbitration, 7 keyboard device, 1 display device, 1 block device, 3 retract, 3 children, 1 diagnostics, 5 hardware.</summary>
    private const int ExpectedTestCount = 34;

    /// <summary>Key of the node the constructor publishes, before the engine starts.</summary>
    private const string BootKey = "boot";

    /// <summary>Firings the periodic test waits for.</summary>
    private const int PeriodicMinimumRuns = 3;

    /// <summary>How long the periodic test waits for them; the interval is 20 ms and the x64 tick about 55 ms.</summary>
    private const int PeriodicWindowMilliseconds = 500;

    /// <summary>How long a polling loop sleeps between looks.</summary>
    private const int PollSleepMilliseconds = 10;

    /// <summary>The scan code the interrupt test writes into the window.</summary>
    private const byte TestScanCode = 0x1E;

    /// <summary>A scan code reported after the retraction, which must never reach the consumer.</summary>
    private const byte StaleScanCode = 0x99;

    /// <summary>Flags register value for a key release.</summary>
    private const byte ReleasedFlag = 1;

    /// <summary>Bus name of the root nodes the machine description publishes, the PCI host among them.</summary>
    private const string PlatformBusName = "platform";

    /// <summary>Bus name of the function nodes the PCI host driver publishes.</summary>
    private const string PciBusName = "pci";

    /// <summary>Prefix shared by the host's compatible strings (pci-host-legacy on x64, pci-host-ecam-generic on arm64), which the platform node's description lists.</summary>
    private const string PciHostCompatiblePrefix = "pci-host-";

    /// <summary>Start of a function node's description for Intel's 82574L (vendor 8086, device 10d3), the e1000e q35 adds by default.</summary>
    private const string E1000EDescriptionPrefix = "8086:10d3";

    /// <summary>Skip reason of the E1000E tests on a machine whose bus carries no 82574L.</summary>
    private const string SkipNoE1000E = "no e1000e on this machine";

    /// <summary>
    /// Kit resources the E1000E probe holds at least: the BAR0 window (1),
    /// the two descriptor rings and their two buffer areas (4), the drain
    /// work item (1) and the periodic drain (1), which is 7; the interrupt
    /// handle makes 8 when the platform routed the function's line. The
    /// lock is recorded but not a resource.
    /// </summary>
    private const int E1000EMinimumHeldResourceCount = 7;

    /// <summary>Kit resources the E1000E probe holds when the platform routed the function's line: the seven above and the interrupt handle.</summary>
    private const int E1000ELineHeldResourceCount = E1000EMinimumHeldResourceCount + 1;

    /// <summary>How long the link test waits for the link to come up.</summary>
    private const int LinkWindowMilliseconds = 2000;

    /// <summary>Bytes of the smallest Ethernet frame without its checksum, which the controller appends.</summary>
    private const int MinimumFrameBytes = 60;

    /// <summary>Bytes of an Ethernet address.</summary>
    private const int MacAddressBytes = 6;

    /// <summary>Offset of the destination address in a frame.</summary>
    private const int DestinationOffset = 0;

    /// <summary>Offset of the source address in a frame.</summary>
    private const int SourceOffset = MacAddressBytes;

    /// <summary>Offset of the EtherType in a frame; its high byte comes first.</summary>
    private const int EtherTypeOffset = 2 * MacAddressBytes;

    /// <summary>Offset of the EtherType's low byte in a frame.</summary>
    private const int EtherTypeLowByteOffset = EtherTypeOffset + 1;

    /// <summary>The IEEE 802 local experimental EtherType 1, which no stack in the ring claims, so the frame goes out and nobody answers.</summary>
    private const ushort ExperimentalEtherType = 0x88B5;

    /// <summary>Bits per byte, for splitting the EtherType into its two bytes.</summary>
    private const int BitsPerByte = 8;

    /// <summary>Every byte of the broadcast address.</summary>
    private const byte BroadcastByte = 0xFF;

    /// <summary>Characters of an address in the text form MACAddress exposes: six hex pairs and five colons.</summary>
    private const int MacAddressTextLength = 17;

    /// <summary>Characters per address byte in that text form: the pair and its separator.</summary>
    private const int MacAddressTextStride = 3;

    /// <summary>Bits per hex digit.</summary>
    private const int BitsPerHexDigit = 4;

    /// <summary>Value of the first hex letter, a or A.</summary>
    private const int HexLetterBase = 10;

    private readonly TestKeyboardConsumer _keyboardConsumer = new();
    private readonly DeviceNode _bootNode;
    private DeviceNode? _keyboardNode;
    private KeyboardState? _keyboardState;
    private SyntheticAccess? _keyboardAccess;
    private DeviceNode? _busNode;
    private BusState? _busState;
    private string? _e1000ePath;

    /// <summary>
    /// Publishes the boot node. The constructor runs before
    /// <see cref="Sys.Kernel.Start"/>, so the engine is not started yet and
    /// the node waits in the queue for the driver stage to offer it.
    /// </summary>
    public Kernel()
    {
        _bootNode = SyntheticBus.Publish(BootKey, []);
    }

    /// <inheritdoc/>
    protected override void BeforeRun()
    {
        Log.WriteString("[DriversTests] BeforeRun() reached!\n");

        // The suite stands in for the ring's keyboard manager; installed
        // before any keyboard is published, as a manager would be.
        DeviceRegistry.SetConsumer(DeviceKind.Keyboard, _keyboardConsumer);

        TR.Start("Driver Kit Tests", expectedTests: ExpectedTestCount);

        // ==================== Manifest ====================
        TR.Run("Manifest_HighPriorityDriver_Present", TestManifestHighPriorityDriverPresent);
        TR.Run("Manifest_MouseFeatureDriver_Absent", TestManifestMouseFeatureDriverAbsent);
        TR.Run("Manifest_ExcludedDriver_Absent", TestManifestExcludedDriverAbsent);
        TR.Run("Manifest_OptInDriver_Present", TestManifestOptInDriverPresent);
        TR.Run("Manifest_OptOutDriver_Absent", TestManifestOptOutDriverAbsent);
        TR.Run("Manifest_Order_ReferencedByTypeName", TestManifestOrderReferencedByTypeName);

        // ==================== Engine ====================
        TR.Run("Engine_Started_WithWorker", TestEngineStartedWithWorker);
        TR.Run("BootPath_NodeFromConstructor_Bound", TestBootPathNodeFromConstructorBound);

        // ==================== Arbitration ====================
        TR.Run("Arbitration_ByPriority", TestArbitrationByPriority);
        TR.Run("Arbitration_BySpecificity", TestArbitrationBySpecificity);
        TR.Run("Arbitration_TieByManifestOrder", TestArbitrationTieByManifestOrder);
        TR.Run("Decline_UnwindsResources", TestDeclineUnwindsResources);
        TR.Run("ThrowingProbe_RecordedAsFailed", TestThrowingProbeRecordedAsFailed);

        // ==================== Keyboard device ====================
        TR.Run("Publish_ReachesConsumer", TestPublishReachesConsumer);
        TR.Run("WindowAndDma_Contents", TestWindowAndDmaContents);
        TR.Run("Interrupt_HandlerReadsWindow_ReportsKey", TestInterruptHandlerReadsWindowReportsKey);
        TR.Run("Interrupt_WorkItemDeferredToWorker", TestInterruptWorkItemDeferredToWorker);
        TR.Run("Interrupt_MaskUnmask", TestInterruptMaskUnmask);
        TR.Run("Periodic_FiresAtLeastThreeTimes", TestPeriodicFiresAtLeastThreeTimes);
        TR.Run("BlockingHandler_FaultRecorded", TestBlockingHandlerFaultRecorded);

        // ==================== Display device ====================
        TR.Run("Publish_Display_ReachesDisplayManager", TestPublishDisplayReachesDisplayManager);

        // ==================== Block device ====================
        TR.Run("Publish_Block_ReachesStorageManager", TestPublishBlockReachesStorageManager);

        // ==================== Retract ====================
        TR.Run("Retract_DetachOrderAndAccounting", TestRetractDetachOrderAndAccounting);
        TR.Run("Retract_DriverThreadExited", TestRetractDriverThreadExited);
        TR.Run("Retract_SinkReportDiscarded", TestRetractSinkReportDiscarded);

        // ==================== Children ====================
        TR.Run("Children_PublishedFromProbe", TestChildrenPublishedFromProbe);
        TR.Run("UnmatchedNode_UnboundWithNoOffers", TestUnmatchedNodeUnboundWithNoOffers);
        TR.Run("Children_RetractedWithParent", TestChildrenRetractedWithParent);

        // ==================== Diagnostics ====================
        TR.Run("DriverInfo_OutOfRange_ReturnsFalse", TestDriverInfoOutOfRangeReturnsFalse);

        // ==================== Hardware ====================
        // The host test is unconditional: the default cell on either arch
        // carries a PCI host. The E1000E tests need the 82574L q35 adds
        // when a cell names no NIC; virt's default NIC is a virtio-net-pci
        // function that the kit's VirtioNetDriver binds, so they skip there.
        // The node is looked up once, here, and read by path.
        _e1000ePath = FindE1000EPath();
        bool hasE1000E = _e1000ePath is not null;
        TR.Run("Hardware_PciHost_Bound", TestHardwarePciHostBound);
        TR.RunIf(hasE1000E, "Hardware_E1000E_NodeBound", TestHardwareE1000ENodeBound, SkipNoE1000E);
        TR.RunIf(hasE1000E, "Hardware_E1000E_DeviceConsumed", TestHardwareE1000EDeviceConsumed, SkipNoE1000E);
        TR.RunIf(hasE1000E, "Hardware_E1000E_LinkUp", TestHardwareE1000ELinkUp, SkipNoE1000E);
        TR.RunIf(hasE1000E, "Hardware_E1000E_Transmit", TestHardwareE1000ETransmit, SkipNoE1000E);

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

    // ==================== Manifest ====================

    private static void TestManifestHighPriorityDriverPresent()
    {
        int index = FindDriverIndex(nameof(HighPriorityDriver));
        Assert.True(index >= 0, "HighPriorityDriver should be in the manifest");
        if (DriverInfo.TryGetDriver(index, out DriverEntryInfo info))
        {
            Assert.Equal(HighPriorityDriver.ClaimedPriority, info.Priority, "the manifest entry should carry the driver's priority");
        }

        Assert.NotNull(RecordingDriver.Find<HighPriorityDriver>());
    }

    // The driver is tied to the mouse feature and the project turns the
    // feature off, so the generated manifest guards its registration behind
    // KernelFeatures.Mouse and the guard folds to nothing.
    private static void TestManifestMouseFeatureDriverAbsent()
    {
        Assert.True(FindDriverIndex(nameof(MouseFeatureDriver)) < 0, "a driver tied to a feature that is off should not be in the manifest");
        Assert.Null(RecordingDriver.Find<MouseFeatureDriver>(), "the registry should hold no MouseFeatureDriver");
    }

    private static void TestManifestExcludedDriverAbsent()
    {
        Assert.True(FindDriverIndex(nameof(ExcludedDriver)) < 0, "a driver named by a CosmosDriverExclude item should not be in the manifest");
        Assert.Null(RecordingDriver.Find<ExcludedDriver>(), "the registry should hold no ExcludedDriver");
    }

    private static void TestManifestOptInDriverPresent()
    {
        Assert.True(FindDriverIndex(nameof(OptInDriver)) >= 0, "a Default = false driver named by a CosmosDriverInclude item should be in the manifest");
        Assert.NotNull(RecordingDriver.Find<OptInDriver>());
    }

    private static void TestManifestOptOutDriverAbsent()
    {
        Assert.True(FindDriverIndex(nameof(OptOutDriver)) < 0, "a Default = false driver nobody asked for should not be in the manifest");
        Assert.Null(RecordingDriver.Find<OptOutDriver>(), "the registry should hold no OptOutDriver");
    }

    // The library's drivers are referenced-assembly drivers, which the
    // generator orders by assembly name and then by full type name, both
    // ordinal. Both tie drivers share the assembly and the namespace, so
    // only the type name decides, and TieFirstDriver sorts before
    // TieSecondDriver (F before S). Declaration order plays no part.
    private static void TestManifestOrderReferencedByTypeName()
    {
        int first = FindDriverIndex(nameof(TieFirstDriver));
        int second = FindDriverIndex(nameof(TieSecondDriver));
        Assert.True(first >= 0, "TieFirstDriver should be in the manifest");
        Assert.True(second >= 0, "TieSecondDriver should be in the manifest");
        Assert.True(string.CompareOrdinal(typeof(TieFirstDriver).FullName, typeof(TieSecondDriver).FullName) < 0, "the test relies on TieFirstDriver sorting before TieSecondDriver by full type name");
        Assert.True(first < second, "referenced drivers should sit in the manifest in ordinal order of their full type names");
    }

    // ==================== Engine ====================

    private static void TestEngineStartedWithWorker()
    {
        Assert.True(DriverInfo.IsStarted, "the driver stage should have run before BeforeRun");
        Assert.True(DriverInfo.HasWorker, "with the scheduler on, a worker thread should run the kit");
    }

    private void TestBootPathNodeFromConstructorBound()
    {
        Assert.True(TryFindNode(_bootNode.Path, out DeviceNodeInfo info), "the node published from the constructor should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the driver stage should have offered and bound the boot node");
        Assert.True(info.DriverName == nameof(AnyKeyDriver), "the catch-all driver should have bound the boot node");
        Assert.Equal(1, info.OfferCount, "the boot node should have been offered once");
        Assert.Null(info.ParentPath, "a bus node has no parent");
        Assert.True(info.BusName == "synthetic", "the boot node sits on the synthetic bus");
    }

    // ==================== Arbitration ====================

    // Two keyed drivers of equal specificity plus the catch-all: the one with
    // the higher priority is offered first and binds, and the other two are
    // never probed.
    private static void TestArbitrationByPriority()
    {
        LowPriorityDriver? low = RecordingDriver.Find<LowPriorityDriver>();
        DeviceNode node = SyntheticBus.Publish(HighPriorityDriver.Key, []);

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the prio node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the prio node should be bound");
        Assert.True(info.DriverName == nameof(HighPriorityDriver), "the higher priority driver should hold the node");
        Assert.Equal(1, info.OfferCount, "the first offer should have bound");
        Assert.True(TryFindOffer(node.Path, 0, out DeviceOfferInfo offer), "the first offer should be recorded");
        Assert.True(offer.DriverName == nameof(HighPriorityDriver), "the first offer should go to the higher priority driver");
        Assert.Equal(HighPriorityDriver.ClaimedPriority, offer.Priority, "the offer should record the driver's priority");
        Assert.True(offer.Outcome == DeviceOfferOutcome.Bound, "the first offer should be bound");
        Assert.Null(offer.Reason, "a bound offer carries no reason");
        Assert.True(low is not null && low.ProbeCount == 0, "the lower priority driver should never be probed");
    }

    // Equal priority: the keyed match (specificity 1) beats the catch-all
    // (specificity 0), which is not probed for this node.
    private static void TestArbitrationBySpecificity()
    {
        AnyKeyDriver? any = RecordingDriver.Find<AnyKeyDriver>();
        int anyProbesBefore = any?.ProbeCount ?? 0;
        DeviceNode node = SyntheticBus.Publish(ExactKeyDriver.Key, []);

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the spec node should be in the tree");
        Assert.True(info.DriverName == nameof(ExactKeyDriver), "the more specific match should hold the node");
        Assert.Equal(1, info.OfferCount, "the first offer should have bound");
        Assert.True(TryFindOffer(node.Path, 0, out DeviceOfferInfo offer), "the first offer should be recorded");
        Assert.True(offer.DriverName == nameof(ExactKeyDriver), "the first offer should go to the more specific match");
        Assert.Equal(1, offer.Specificity, "the offer should record the keyed match's specificity");
        Assert.True(any is not null && any.ProbeCount == anyProbesBefore, "the catch-all should not be probed for a node a keyed driver takes");
    }

    // Same priority, same specificity: the earlier manifest position wins.
    private static void TestArbitrationTieByManifestOrder()
    {
        TieSecondDriver? second = RecordingDriver.Find<TieSecondDriver>();
        DeviceNode node = SyntheticBus.Publish(TieDriver.Key, []);

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the tie node should be in the tree");
        Assert.True(info.DriverName == nameof(TieFirstDriver), "the driver earlier in the manifest should hold the node");
        Assert.True(TryFindOffer(node.Path, 0, out DeviceOfferInfo offer), "the first offer should be recorded");
        Assert.True(offer.DriverName == nameof(TieFirstDriver), "the first offer should go to the driver earlier in the manifest");
        Assert.Equal(0, offer.Priority, "both tie drivers keep the default priority");
        Assert.True(second is not null && second.ProbeCount == 0, "the driver later in the manifest should never be probed");
    }

    // The declining probe maps a window, allocates DMA, creates an event and
    // a work item. All four are released before the next offer, the offer
    // says so, and the kit holds no more than it did before.
    private static void TestDeclineUnwindsResources()
    {
        DecliningDriver? declining = RecordingDriver.Find<DecliningDriver>();
        int heldBefore = DriverInfo.GetTotalHeldResourceCount();
        DeviceNode node = SyntheticBus.Publish(DecliningDriver.Key, [], interruptCount: 0, windowBytes: DecliningDriver.WindowBytes);

        // Drains anything the declined probe left queued: a work item the
        // unwind failed to cancel would run here and show up below.
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(TryFindOffer(node.Path, 0, out DeviceOfferInfo offer), "the declined offer should be recorded");
        Assert.True(offer.DriverName == nameof(DecliningDriver), "the keyed driver should be offered first");
        Assert.True(offer.Outcome == DeviceOfferOutcome.Declined, "the offer should be recorded as declined");
        Assert.True(offer.Reason == DecliningDriver.Reason, "the offer should carry the driver's reason");
        Assert.Equal(DecliningDriver.AcquiredResourceCount, offer.ReleasedResourceCount, "the offer should count what the probe had acquired");
        Assert.Equal(heldBefore, DriverInfo.GetTotalHeldResourceCount(), "a declined probe should leave the held total unchanged");

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the decline node should be in the tree");
        Assert.Equal(2, info.OfferCount, "the catch-all should be offered after the decline");
        Assert.True(info.DriverName == nameof(AnyKeyDriver), "the catch-all should hold the node");
        Assert.True(info.HeldResourceCount == 0, "the catch-all acquires nothing");
        Assert.True(declining is not null && declining.ProbeCount == 1 && declining.WorkItemRuns == 0, "the declining driver's work item should never run");
    }

    private static void TestThrowingProbeRecordedAsFailed()
    {
        DeviceNode node = SyntheticBus.Publish(ThrowingDriver.Key, []);

        Assert.True(TryFindOffer(node.Path, 0, out DeviceOfferInfo offer), "the failed offer should be recorded");
        Assert.True(offer.DriverName == nameof(ThrowingDriver), "the keyed driver should be offered first");
        Assert.True(offer.Outcome == DeviceOfferOutcome.Failed, "an exception from the probe should be recorded as failed");
        Assert.True(offer.Reason == ThrowingDriver.Message, "the offer should carry the exception's message");
        Assert.Equal(0, offer.ReleasedResourceCount, "the throwing probe acquired nothing");

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the throw node should be in the tree");
        Assert.Equal(2, info.OfferCount, "the catch-all should be offered after the failure");
        Assert.True(info.State == DeviceNodeState.Bound, "the catch-all should hold the node");
    }

    // ==================== Keyboard device ====================

    private void TestPublishReachesConsumer()
    {
        int devicesBefore = DriverInfo.DeviceCount;
        DeviceNode node = SyntheticBus.Publish(KeyboardDriver.Key, [], interruptCount: 1, windowBytes: KeyboardState.WindowBytes);
        _keyboardNode = node;
        _keyboardAccess = node.Access<SyntheticAccess>();
        KeyboardState? state = node.Binding?.DriverState as KeyboardState;
        _keyboardState = state;
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the kbd node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the keyboard driver should hold the node");
        Assert.True(info.DriverName == nameof(KeyboardDriver), "the keyboard driver should hold the node");
        Assert.Equal(1, info.ResourceCount, "the node carries one RAM window");
        Assert.Equal(1, info.InterruptCount, "the node carries one interrupt source");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should hold one published device");
        Assert.Equal(state.ExpectedHeldResourceCount, info.HeldResourceCount, "the node should hold what the probe acquired");

        Assert.Equal(1, _keyboardConsumer.PublishedCount, "the consumer should have been handed the keyboard");
        Assert.True(_keyboardConsumer.LastPublished is { } published && published.Kind == DeviceKind.Keyboard && ReferenceEquals(published.Device, state), "the consumer should hold the driver's keyboard");
        Assert.Equal(devicesBefore + 1, DriverInfo.DeviceCount, "the published list should have grown by one");

        int deviceIndex = FindDeviceIndex(state.Name);
        Assert.True(deviceIndex >= 0, "the keyboard should be in the published list");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.Kind == PublishedDeviceKind.Keyboard, "the published device should be a keyboard");
            Assert.True(device.IsConsumed, "the published device should be consumed");
            Assert.True(device.NodePath == node.Path, "the published device should name its node");
            Assert.True(device.DriverName == nameof(KeyboardDriver), "the published device should name its driver");
        }
    }

    private void TestWindowAndDmaContents()
    {
        if (!TryGetKeyboard(out _, out KeyboardState? state, out SyntheticAccess? access))
        {
            return;
        }

        Assert.True(access.HasWindow, "the node should carry a window");
        Span<byte> window = access.Window;
        Assert.Equal(KeyboardState.WindowBytes, window.Length, "the bus should expose the whole window");
        for (int i = 0; i < KeyboardState.PatternLength; i++)
        {
            Assert.Equal((byte)(KeyboardState.PatternFirstByte + i), window[KeyboardState.PatternOffset + i], "the pattern the driver wrote through its register window should be visible in the page");
        }

        Assert.True(state.Dma.PhysicalAddress != 0, "a DMA buffer should have a physical address");
        Assert.Equal(KeyboardState.DmaBytes, state.Dma.Length, "the DMA buffer should have the requested length");
        Assert.True(state.DmaWasZeroed, "a DMA buffer should be zeroed at allocation");
        Span<byte> dma = state.Dma.Span;
        for (int i = 0; i < dma.Length; i++)
        {
            Assert.Equal((byte)(KeyboardState.DmaFirstByte + i), dma[i], "the pattern the driver wrote into DMA memory should read back");
        }
    }

    private void TestInterruptHandlerReadsWindowReportsKey()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out SyntheticAccess? access))
        {
            return;
        }

        int keysBefore = _keyboardConsumer.KeyCount;
        int interruptsBefore = state.InterruptCount;

        access.Window[KeyboardState.ScanCodeOffset] = TestScanCode;
        access.Window[KeyboardState.FlagsOffset] = 0;
        bool raised = SyntheticBus.RaiseInterrupt(node, 0);
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(raised, "a connected, unmasked source should run the handler");
        Assert.Equal(interruptsBefore + 1, state.InterruptCount, "the handler should have run once");
        Assert.Equal(keysBefore + 1, _keyboardConsumer.KeyCount, "the report should reach the consumer");
        Assert.Equal(TestScanCode, _keyboardConsumer.LastScanCode, "the handler should read the scan code the test wrote into the window");
        Assert.False(_keyboardConsumer.LastReleased, "a clear flags register is a key press");

        access.Window[KeyboardState.FlagsOffset] = ReleasedFlag;
        raised = SyntheticBus.RaiseInterrupt(node, 0);
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(raised, "the second raise should run the handler");
        Assert.True(_keyboardConsumer.LastReleased, "a set release flag is a key release");
        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info) && info.FaultCount == 0, "a well-behaved handler records no fault");
    }

    // A work item a handler schedules is deferred to the kit worker, never
    // run inside the dispatch. The handler notes the run count as it
    // returns, with interrupts still disabled, so that reading is exact:
    // unchanged means the item did not run inside RaiseInterrupt, and the
    // one run WaitForQueuedJobs then drains is the worker's.
    private void TestInterruptWorkItemDeferredToWorker()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out _))
        {
            return;
        }

        int runsBefore = state.KeyWorkRuns;
        Assert.True(SyntheticBus.RaiseInterrupt(node, 0), "the raise should run the handler");
        Assert.Equal(runsBefore, state.KeyWorkRunsAtHandlerExit, "the work item should not run inside RaiseInterrupt");

        SyntheticBus.WaitForQueuedJobs();
        Assert.Equal(runsBefore + 1, state.KeyWorkRuns, "the work item the handler scheduled should have run once, after WaitForQueuedJobs");
    }

    private void TestInterruptMaskUnmask()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out _))
        {
            return;
        }

        InterruptHandle? handle = state.Handle;
        Assert.NotNull(handle);
        if (handle is null)
        {
            return;
        }

        int interruptsBefore = state.InterruptCount;
        handle.Mask();
        Assert.True(handle.IsMasked, "the handle should report masked");
        Assert.False(SyntheticBus.RaiseInterrupt(node, 0), "a masked source is not delivered");
        Assert.Equal(interruptsBefore, state.InterruptCount, "the handler should not run while masked");

        handle.Unmask();
        Assert.False(handle.IsMasked, "the handle should report unmasked");
        Assert.True(SyntheticBus.RaiseInterrupt(node, 0), "an unmasked source is delivered again");
        SyntheticBus.WaitForQueuedJobs();
        Assert.Equal(interruptsBefore + 1, state.InterruptCount, "the handler should run once unmasked");
    }

    // The periodic work item is queued from the timer interrupt every 20 ms
    // (rounded up to the tick, about 55 ms on x64) and runs on the worker.
    private void TestPeriodicFiresAtLeastThreeTimes()
    {
        if (!TryGetKeyboard(out _, out KeyboardState? state, out _))
        {
            return;
        }

        Assert.True(state.PeriodicScheduled, "TrySchedulePeriodic needs the platform timer and the kit worker");

        int runsBefore = state.PeriodicRuns;
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * PeriodicWindowMilliseconds / TR.MillisecondsPerSecond;
        while (state.PeriodicRuns - runsBefore < PeriodicMinimumRuns && Stopwatch.GetTimestamp() < deadline)
        {
            SysThread.Sleep(PollSleepMilliseconds);
            SyntheticBus.WaitForQueuedJobs();
        }

        int runs = state.PeriodicRuns - runsBefore;
        Log.WriteString("[DriversTests] periodic runs within the window: ");
        Log.WriteNumber(runs);
        Log.WriteString("\n");
        Assert.True(runs >= PeriodicMinimumRuns, "the periodic work item should fire at least three times in 500 ms");
    }

    // A handler that sleeps through the binding hits the interrupt-context
    // guard. In a synthetic dispatch that is an exception the trampoline
    // records as a fault, masking the source so it cannot repeat.
    private static void TestBlockingHandlerFaultRecorded()
    {
        DeviceNode node = SyntheticBus.Publish(BlockingHandlerDriver.Key, [], interruptCount: 1);
        BlockingHandlerState? state = node.Binding?.DriverState as BlockingHandlerState;
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        Assert.True(SyntheticBus.RaiseInterrupt(node, 0), "the first raise should run the handler");
        SyntheticBus.WaitForQueuedJobs();

        Assert.Equal(1, state.HandlerRuns, "the handler should have been entered once");
        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the blocking node should be in the tree");
        Assert.Equal(1, info.FaultCount, "the guard's exception should be recorded as one fault");
        Assert.NotNull(info.LastFault);
        Assert.True(info.State == DeviceNodeState.Bound, "a fault does not tear the binding down");
        Assert.True(state.Handle is { IsMasked: true }, "a faulting handler leaves its source masked");
        Assert.False(SyntheticBus.RaiseInterrupt(node, 0), "the masked source is not delivered again");
        Assert.Equal(1, state.HandlerRuns, "the handler should not run again while masked");
    }

    // ==================== Display device ====================

    // The ring's display manager consumes the display kind: a display a
    // driver publishes joins its list ahead of the firmware framebuffer and
    // leaves it when the node is retracted. The state is read back through
    // the manager's handle, which keeps answering after the withdrawal.
    private static void TestPublishDisplayReachesDisplayManager()
    {
        int displaysBefore = DisplayManager.Count;
        DisplayDevice? primaryBefore = DisplayManager.Primary;
        Assert.True(primaryBefore is null || primaryBefore.IsFirmware, "before the publish only the firmware framebuffer, if any, is a display");

        DeviceNode node = SyntheticBus.Publish(DisplayDriver.Key, []);
        SyntheticBus.WaitForQueuedJobs();

        DisplayState? state = node.Binding?.DriverState as DisplayState;
        Assert.NotNull(state, "the display driver should hold the node");
        if (state is null)
        {
            SyntheticBus.Retract(node);
            SyntheticBus.WaitForQueuedJobs();
            return;
        }

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the display node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the display driver should hold the node");
        Assert.True(info.DriverName == nameof(DisplayDriver), "the display driver should hold the node");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should hold one published device");

        Assert.Equal(displaysBefore + 1, DisplayManager.Count, "the display manager should list one more display");
        DisplayDevice? display = FindDisplay(DisplayState.DisplayName);
        Assert.NotNull(display, "the manager should list the synthetic display by name");
        if (display is null)
        {
            SyntheticBus.Retract(node);
            SyntheticBus.WaitForQueuedJobs();
            return;
        }

        Assert.True(display.DriverName == nameof(DisplayDriver), "the display should name its driver");
        Assert.True(display.NodePath == node.Path, "the display should name its node");
        Assert.False(display.IsFirmware, "a driver's display is not the firmware one");
        Assert.False(display.IsWithdrawn, "a published display is not withdrawn");
        Assert.Equal(DisplayState.ModeWidth, display.Width, "the display reports the driver's width");
        Assert.Equal(DisplayState.ModeHeight, display.Height, "the display reports the driver's height");
        Assert.Equal(DisplayState.ModeBitsPerPixel, display.BitsPerPixel, "the display reports the driver's depth");
        Assert.Equal(DisplayState.ModePitch, display.Pitch, "the display reports the driver's pitch");
        Assert.True(ReferenceEquals(DisplayManager.Primary, display), "a driver display is preferred over the firmware one");
        Assert.True(display.TryGetFacet(out DisplayState? facet) && ReferenceEquals(facet, state), "the driver's state is found as a facet of the display");
        Assert.False(display.TryGetFacet<IDisplayModes>(out _), "a display in one fixed mode has no modes facet");

        int deviceIndex = FindDisplayDeviceIndex(node.Path);
        Assert.True(deviceIndex >= 0, "the display should be in the published list under its node");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.IsConsumed, "the display manager should have consumed the display");
            Assert.True(device.DriverName == nameof(DisplayDriver), "the published device should name its driver");
            Assert.True(device.Name == DisplayState.DisplayName, "the published device should carry the driver's name for it");
        }

        SyntheticBus.Retract(node);
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(display.IsWithdrawn, "the handle should report the withdrawal");
        Assert.Equal(0, display.Width, "a withdrawn display reports a zero mode");
        Assert.Equal(0, display.Height, "a withdrawn display reports a zero mode");
        Assert.Equal(0, display.BitsPerPixel, "a withdrawn display reports a zero mode");
        Assert.Equal(0, display.RefreshRate, "a withdrawn display reports no refresh rate");
        Assert.False(display.TryGetFacet<DisplayState>(out _), "a withdrawn display has no facet");
        Assert.Equal(displaysBefore, DisplayManager.Count, "the display manager should have dropped the display");
        Assert.Null(FindDisplay(DisplayState.DisplayName), "the withdrawn display should have left the manager's list");
        Assert.True(ReferenceEquals(DisplayManager.Primary, primaryBefore), "the primary should be the firmware display again, or none");
        Assert.True(FindDisplayDeviceIndex(node.Path) < 0, "the withdrawn display should have left the published list");
        Assert.Equal(0, state.FlushCount, "a display without a framebuffer receives no flush");
    }

    // ==================== Block device ====================

    // The ring's storage manager consumes the block kind: a disk a driver
    // publishes is registered and scanned inside the probe, takes its
    // place in the manager's tables (ahead of any hand-registered disk, so
    // it is the primary when nothing else is there) and leaves them when
    // the node is retracted. The UART carries the manager's registered and
    // unregistered lines for it; they are read from the log, not asserted.
    private static void TestPublishBlockReachesStorageManager()
    {
        int disksBefore = StorageManager.DeviceCount;
        int devicesBefore = DriverInfo.DeviceCount;
        IBlockDevice? primaryBefore = StorageManager.PrimaryDevice;
        BlockDriver? driver = RecordingDriver.Find<BlockDriver>();
        int probesBefore = driver?.ProbeCount ?? 0;
        int detachesBefore = driver?.DetachCount ?? 0;

        DeviceNode node = SyntheticBus.Publish(BlockDriver.Key, []);
        SyntheticBus.WaitForQueuedJobs();

        BlockState? state = node.Binding?.DriverState as BlockState;
        Assert.NotNull(state, "the block driver should hold the node");
        if (state is null)
        {
            SyntheticBus.Retract(node);
            SyntheticBus.WaitForQueuedJobs();
            return;
        }

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the block node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the block driver should hold the node");
        Assert.True(info.DriverName == nameof(BlockDriver), "the block driver should hold the node");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should hold one published device");

        Assert.Equal(disksBefore + 1, StorageManager.DeviceCount, "the storage manager should list one more device");
        Assert.True(HoldsDevice(StorageManager.Devices, state), "the storage manager should list the synthetic disk");
        Assert.Equal(0, StorageManager.GetPartitions(state).Count, "a blank disk has no partitions");
        if (disksBefore == 0)
        {
            Assert.True(ReferenceEquals(StorageManager.PrimaryDevice, state), "the only disk should be the primary device");
        }

        Assert.Equal(devicesBefore + 1, DriverInfo.DeviceCount, "the published list should hold one more device");
        int deviceIndex = FindBlockDeviceIndex(node.Path);
        Assert.True(deviceIndex >= 0, "the disk should be in the published list under its node");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.IsConsumed, "the storage manager should have consumed the disk");
            Assert.False(device.IsWithdrawn, "a published disk is not withdrawn");
            Assert.True(device.DriverName == nameof(BlockDriver), "the published device should name its driver");
            Assert.True(device.Name == BlockState.DeviceName, "the published device should carry the driver's name for it");
        }

        SyntheticBus.Retract(node);
        SyntheticBus.WaitForQueuedJobs();

        Assert.Equal(disksBefore, StorageManager.DeviceCount, "the storage manager should have dropped the disk");
        Assert.False(HoldsDevice(StorageManager.Devices, state), "the withdrawn disk should have left the manager's list");
        Assert.True(ReferenceEquals(StorageManager.PrimaryDevice, primaryBefore), "the primary device should be what it was before, or none");
        Assert.Equal(devicesBefore, DriverInfo.DeviceCount, "the withdrawn disk should have left the published list");
        Assert.True(FindBlockDeviceIndex(node.Path) < 0, "the withdrawn disk should have left the published list");
        Assert.True(driver is not null && driver.ProbeCount == probesBefore + 1, "the block driver should have been probed once");
        Assert.True(driver is not null && driver.DetachCount == detachesBefore + 1, "the block driver should have been detached once");
        Assert.Equal(0, state.FlushCount, "nothing writes to the synthetic disk, so nothing flushes it");
    }

    // ==================== Retract ====================

    // Teardown order: devices withdrawn (the consumer is told), interrupts
    // disconnected, threads joined, OnDetach with the bus's reason, then the
    // memory released and the node marked retracted with nothing held.
    private void TestRetractDetachOrderAndAccounting()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out SyntheticAccess? access))
        {
            return;
        }

        KeyboardDriver? driver = RecordingDriver.Find<KeyboardDriver>();
        int devicesBefore = DriverInfo.DeviceCount;
        int withdrawnBefore = _keyboardConsumer.WithdrawnCount;
        int heldBefore = DriverInfo.GetTotalHeldResourceCount();
        PublishedDevice? published = _keyboardConsumer.LastPublished;

        SyntheticBus.Retract(node);
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(driver is not null && driver.DetachCount == 1, "OnDetach should run once for the bound device");
        Assert.True(driver is not null && driver.LastDetachReason.Cause == DetachCause.Retracted, "the bus retracted the node itself");
        Assert.True(driver is not null && !driver.LastDetachReason.HardwarePresent, "a retraction without hardware says so");

        Assert.Equal(withdrawnBefore + 1, _keyboardConsumer.WithdrawnCount, "the consumer should be told the keyboard is gone");
        Assert.Null(_keyboardConsumer.LastPublished, "the consumer should hold no keyboard any more");
        Assert.True(published is not null && ReferenceEquals(_keyboardConsumer.LastWithdrawn, published), "the consumer should be handed the same device it was given");
        Assert.Equal(devicesBefore - 1, DriverInfo.DeviceCount, "the published list should have shrunk by one");
        Assert.True(FindDeviceIndex(state.Name) < 0, "the withdrawn keyboard should have left the published list");

        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the retracted node stays in the tree");
        Assert.True(info.State == DeviceNodeState.Retracted, "the node should be retracted");
        Assert.True(info.DriverName == nameof(KeyboardDriver), "the node still names the driver that held it last");
        Assert.Equal(0, info.HeldResourceCount, "a retracted node holds nothing");
        Assert.Equal(0, info.PublishedDeviceCount, "a retracted node publishes nothing");
        Assert.Equal(heldBefore - state.ExpectedHeldResourceCount, DriverInfo.GetTotalHeldResourceCount(), "the held total should drop by what the binding held");
        Assert.True(node.Binding is { IsDetaching: true }, "the binding should be flagged as detaching");
        Assert.True(access.Window.IsEmpty, "the RAM page should have gone back to the allocator");
    }

    private void TestRetractDriverThreadExited()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out _))
        {
            return;
        }

        Assert.True(state.Thread is not null, "TryStartThread needs the scheduler");
        Assert.True(state.ThreadStarted, "the driver thread should have run");
        Assert.True(state.EventWakes >= 1, "the handler's signal should have woken the driver thread at least once");
        Assert.True(state.ThreadExited, "the driver thread should have returned once the binding began detaching");
        Assert.True(state.ThreadExitedBeforeDetach, "the join should precede OnDetach");
        Assert.True(state.Thread is { HasExited: true }, "the kit should see the thread as exited");
        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info) && info.LeakedResourceCount == 0, "a thread that stopped in time leaks nothing");
    }

    private void TestRetractSinkReportDiscarded()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out _))
        {
            return;
        }

        int keysBefore = _keyboardConsumer.KeyCount;
        int interruptsBefore = state.InterruptCount;
        KeyboardSink? sink = state.Sink;
        Assert.NotNull(sink);
        sink?.Report(StaleScanCode, released: false);

        Assert.Equal(keysBefore, _keyboardConsumer.KeyCount, "a report through a withdrawn device's sink is discarded");
        Assert.True(_keyboardConsumer.LastScanCode != StaleScanCode, "the stale scan code should never reach the consumer");
        Assert.False(SyntheticBus.RaiseInterrupt(node, 0), "a retracted node has no connected handler");
        Assert.Equal(interruptsBefore, state.InterruptCount, "the handler should not run after the retraction");
        Assert.False(state.KeyWork.Schedule(), "a cancelled work item refuses to be scheduled");
        Assert.True(state.KeyEvent.IsCancelled, "the binding's events should be cancelled");
    }

    // ==================== Children ====================

    // The bus driver publishes two children from its probe; their offers
    // queue behind the probe, so the test waits for the engine to settle.
    private void TestChildrenPublishedFromProbe()
    {
        DeviceNode bus = SyntheticBus.Publish(BusDriver.Key, []);
        SyntheticBus.WaitForQueuedJobs();
        _busNode = bus;
        BusState? busState = bus.Binding?.DriverState as BusState;
        _busState = busState;
        Assert.NotNull(busState);
        if (busState is null)
        {
            return;
        }

        Assert.True(TryFindNode(bus.Path, out DeviceNodeInfo busInfo), "the bus node should be in the tree");
        Assert.True(busInfo.State == DeviceNodeState.Bound, "the bus driver should hold the node");
        Assert.True(busInfo.DriverName == nameof(BusDriver), "the bus driver should hold the node");
        Assert.Equal(2, busInfo.ChildCount, "the bus node should have two children");

        Assert.True(TryFindNode(busState.Child.Path, out DeviceNodeInfo childInfo), "the child should be in the tree");
        Assert.True(childInfo.State == DeviceNodeState.Bound, "the child driver should hold the child");
        Assert.True(childInfo.DriverName == nameof(ChildDriver), "the child driver should hold the child");
        Assert.True(childInfo.ParentPath == bus.Path, "the child should name the bus node as its parent");
        Assert.True(childInfo.BusName == ChildIdentity.Bus, "the child sits on the child bus");
    }

    private void TestUnmatchedNodeUnboundWithNoOffers()
    {
        BusState? busState = _busState;
        if (busState is null)
        {
            Assert.Fail("the bus node was not set up by the children test");
            return;
        }

        Assert.True(TryFindNode(busState.Orphan.Path, out DeviceNodeInfo info), "the orphan should be in the tree");
        Assert.True(info.State == DeviceNodeState.Unbound, "a node no driver matches is unbound");
        Assert.Equal(0, info.OfferCount, "a node no driver matches is offered to nobody");
        Assert.Null(info.DriverName, "an unbound node names no driver");
        Assert.True(info.ParentPath == _busNode?.Path, "the orphan should name the bus node as its parent");
    }

    private void TestChildrenRetractedWithParent()
    {
        DeviceNode? bus = _busNode;
        BusState? busState = _busState;
        if (bus is null || busState is null)
        {
            Assert.Fail("the bus node was not set up by the children test");
            return;
        }

        ChildDriver? child = RecordingDriver.Find<ChildDriver>();
        BusDriver? busDriver = RecordingDriver.Find<BusDriver>();
        Assert.True(child is { DetachCount: 0 }, "the child driver should still hold its device");

        SyntheticBus.Retract(bus);
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(child is not null && child.DetachCount == 1, "the child driver should be detached once");
        Assert.True(child is not null && child.LastDetachReason.Cause == DetachCause.ParentRetracted, "the child should be told its parent went away");
        Assert.True(child is not null && !child.LastDetachReason.HardwarePresent, "the child inherits the parent's hardware state");
        Assert.True(busDriver is not null && busDriver.DetachCount == 1 && busDriver.LastDetachReason.Cause == DetachCause.Retracted, "the bus driver should see its own retraction");

        Assert.True(TryFindNode(busState.Child.Path, out DeviceNodeInfo childInfo) && childInfo.State == DeviceNodeState.Retracted, "the child should be retracted");
        Assert.True(TryFindNode(busState.Orphan.Path, out DeviceNodeInfo orphanInfo) && orphanInfo.State == DeviceNodeState.Retracted, "the orphan should be retracted");
        Assert.True(TryFindNode(bus.Path, out DeviceNodeInfo busInfo) && busInfo.State == DeviceNodeState.Retracted, "the bus node should be retracted");
        Assert.Equal(2, busInfo.ChildCount, "retracted children stay counted");
    }

    // ==================== Diagnostics ====================

    private static void TestDriverInfoOutOfRangeReturnsFalse()
    {
        Assert.True(DriverInfo.NodeCount > 0, "the suite should have published nodes by now");
        Assert.False(DriverInfo.TryGetNode(DriverInfo.NodeCount, out _), "an index equal to the count is out of range");
        Assert.False(DriverInfo.TryGetNode(-1, out _), "a negative index is out of range");
        Assert.False(DriverInfo.TryGetDriver(DriverInfo.DriverCount, out _), "an index equal to the driver count is out of range");
        Assert.False(DriverInfo.TryGetDevice(DriverInfo.DeviceCount, out _), "an index equal to the device count is out of range");
        Assert.False(DriverInfo.TryGetOffer(DriverInfo.NodeCount, 0, out _), "a node index out of range yields no offer");
        Assert.False(DriverInfo.TryGetOffer(0, int.MaxValue, out _), "an offer index out of range yields no offer");
    }

    // ==================== Hardware ====================
    //
    // The kit over the machine's real buses, read through DriverInfo and
    // the ring like every other group; the transmit test alone reaches
    // into the tree through the HAL grant, for the driver's counters.

    private static void TestHardwarePciHostBound()
    {
        Assert.True(TryFindHostNode(out DeviceNodeInfo host), "a platform node whose description names a pci-host compatible should be in the tree");
        Assert.True(host.State == DeviceNodeState.Bound, "the host node should be bound");
        Assert.True(host.DriverName == nameof(PciHostDriver), "PciHostDriver should hold the host node");
    }

    private void TestHardwareE1000ENodeBound()
    {
        if (!TryGetE1000E(out string? path, out DeviceNodeInfo info))
        {
            return;
        }

        Assert.True(info.State == DeviceNodeState.Bound, "the E1000E driver should hold the 82574L");
        Assert.True(info.DriverName == nameof(E1000EDriver), "E1000EDriver should hold the 82574L");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should publish one network interface");
        Assert.True(info.HeldResourceCount >= E1000EMinimumHeldResourceCount, "the binding should hold the window, the four DMA buffers, the drain and the periodic drain at least");

        E1000EState? state = FindE1000EState(path);
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        int expectedHeld = state.HasLine ? E1000ELineHeldResourceCount : E1000EMinimumHeldResourceCount;
        Assert.Equal(expectedHeld, info.HeldResourceCount, "the binding should hold exactly the probe's resources, the interrupt handle among them when the line connected");
    }

    private void TestHardwareE1000EDeviceConsumed()
    {
        if (!TryGetE1000E(out string? path, out _))
        {
            return;
        }

        int deviceIndex = FindNetworkDeviceIndex(path);
        Assert.True(deviceIndex >= 0, "the 82574L's interface should be in the published list as a network device");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.IsConsumed, "the ring's network manager should have taken the interface");
            Assert.True(device.DriverName == nameof(E1000EDriver), "the published device should name its driver");
        }

        if (!TryGetPrimaryE1000EState(path, out E1000EState? state))
        {
            return;
        }

        Assert.True(NetworkManager.DeviceCount >= 1, "the network manager should hold at least the kit's interface");
        MACAddress? macAddress = NetworkManager.MacAddress;
        Assert.NotNull(macAddress);
        Assert.True(macAddress is not null && !macAddress.Equals(MACAddress.None), "the primary device's address should not be all zero");
        Assert.True(macAddress is not null && macAddress.Equals(state.MacAddress), "the primary device's address should be the 82574L's");
    }

    // The probe reads the link once; the drain, on the line and every 50 ms,
    // keeps it current. QEMU's e1000e reports the link up from the start,
    // so the window is slack for a slow first drain, not a wait for
    // auto-negotiation.
    private void TestHardwareE1000ELinkUp()
    {
        if (!TryGetE1000E(out string? path, out _) || !TryGetPrimaryE1000EState(path, out _))
        {
            return;
        }

        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * LinkWindowMilliseconds / TR.MillisecondsPerSecond;
        while (!NetworkManager.LinkUp && Stopwatch.GetTimestamp() < deadline)
        {
            SysThread.Sleep(PollSleepMilliseconds);
        }

        Assert.True(NetworkManager.LinkUp, "the primary device's link should be up within the window");
    }

    // A broadcast frame with the experimental EtherType goes out through
    // the ring's primary device, which on this cell is the kit's e1000e,
    // and lands on the driver's transmit counter.
    private void TestHardwareE1000ETransmit()
    {
        if (!TryGetE1000E(out string? path, out _))
        {
            return;
        }

        if (!TryGetPrimaryE1000EState(path, out E1000EState? state))
        {
            return;
        }

        byte[] frame = new byte[MinimumFrameBytes];
        Assert.True(TryBuildBroadcastFrame(state.MacAddress, frame), "the device's address should parse into the frame's source");

        int transmittedBefore = state.FramesTransmitted;
        bool sent = NetworkManager.Send(frame, frame.Length);
        Assert.True(sent, "the ring should queue the frame on the primary device");
        Assert.Equal(transmittedBefore + 1, state.FramesTransmitted, "the driver should count the one frame the ring queued");
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
    /// Finds the path of the first function node describing an 82574L,
    /// whatever its state: the E1000E tests decide from the hardware's
    /// presence, not from the binding, so a probe that failed shows up as
    /// a failed test rather than a skip.
    /// </summary>
    /// <returns>The node's path, or null when no such function is on the bus.</returns>
    private static string? FindE1000EPath()
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == PciBusName
                && info.Description.StartsWith(E1000EDescriptionPrefix, StringComparison.Ordinal))
            {
                return info.Path;
            }
        }

        return null;
    }

    /// <summary>Hands back the 82574L's path and a fresh snapshot of its node, or fails the test when BeforeRun found none.</summary>
    /// <param name="path">The node's path.</param>
    /// <param name="info">The node's snapshot.</param>
    /// <returns>True when the node is in the tree.</returns>
    private bool TryGetE1000E([NotNullWhen(true)] out string? path, out DeviceNodeInfo info)
    {
        path = _e1000ePath;
        if (path is null || !TryFindNode(path, out info))
        {
            Assert.Fail("the 82574L node was not found by BeforeRun");
            info = default;
            return false;
        }

        return true;
    }

    /// <summary>Finds the published network device whose node has the given path.</summary>
    /// <param name="nodePath">The path of the node whose driver published the device.</param>
    /// <returns>Its position in the published list, or -1.</returns>
    private static int FindNetworkDeviceIndex(string nodePath)
    {
        int count = DriverInfo.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetDevice(i, out PublishedDeviceInfo info) && info.Kind == PublishedDeviceKind.Network && info.NodePath == nodePath)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Finds the display the manager lists under the given name.</summary>
    /// <param name="name">The driver's name for the display.</param>
    /// <returns>The manager's handle, or null when no display of that name is listed.</returns>
    private static DisplayDevice? FindDisplay(string name)
    {
        int count = DisplayManager.Count;
        for (int i = 0; i < count; i++)
        {
            if (DisplayManager.TryGet(i, out DisplayDevice? display) && display.Name == name)
            {
                return display;
            }
        }

        return null;
    }

    /// <summary>Finds the published display device whose node has the given path.</summary>
    /// <param name="nodePath">The path of the node whose driver published the device.</param>
    /// <returns>Its position in the published list, or -1.</returns>
    private static int FindDisplayDeviceIndex(string nodePath)
    {
        int count = DriverInfo.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetDevice(i, out PublishedDeviceInfo info) && info.Kind == PublishedDeviceKind.Display && info.NodePath == nodePath)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Finds the published block device whose node has the given path.</summary>
    /// <param name="nodePath">The path of the node whose driver published the device.</param>
    /// <returns>Its position in the published list, or -1.</returns>
    private static int FindBlockDeviceIndex(string nodePath)
    {
        int count = DriverInfo.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetDevice(i, out PublishedDeviceInfo info) && info.Kind == PublishedDeviceKind.Block && info.NodePath == nodePath)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Whether <paramref name="devices"/> holds <paramref name="device"/> by reference.</summary>
    /// <param name="devices">The manager's list, read once.</param>
    /// <param name="device">The device looked for.</param>
    private static bool HoldsDevice(IReadOnlyList<IBlockDevice> devices, IBlockDevice device)
    {
        for (int i = 0; i < devices.Count; i++)
        {
            if (ReferenceEquals(devices[i], device))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds the driver state of the node with the given path in the tree
    /// itself, through the HAL grant: the counters the transmit test reads
    /// are on the state, which no diagnostic snapshot carries.
    /// </summary>
    /// <param name="path">The node's path.</param>
    /// <returns>The state, or null when the node is not bound by the E1000E driver.</returns>
    private static E1000EState? FindE1000EState(string path)
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode node = nodes[i];
            if (node.Path == path)
            {
                return node.Binding?.DriverState as E1000EState;
            }
        }

        return null;
    }

    /// <summary>
    /// Hands back the 82574L's driver state and checks that the ring's
    /// primary device is that interface, since the ring-facing tests read
    /// the primary's address and link and must be reading the driver under
    /// test. Fails the test when the state is missing or another device is
    /// primary.
    /// </summary>
    /// <param name="path">The node's path.</param>
    /// <param name="state">The driver state when found.</param>
    /// <returns>True when the state is there and its interface is the primary device.</returns>
    private static bool TryGetPrimaryE1000EState(string path, [NotNullWhen(true)] out E1000EState? state)
    {
        state = FindE1000EState(path);
        Assert.NotNull(state);
        if (state is null)
        {
            return false;
        }

        bool isPrimary = NetworkManager.Name == state.Name;
        Assert.True(isPrimary, "the kit's e1000e should be the ring's primary device on this cell");
        return isPrimary;
    }

    /// <summary>
    /// Fills a frame addressed to everyone: the broadcast destination, the
    /// device's own address as the source, the experimental EtherType and
    /// a zero payload. The payload is whatever length the buffer leaves.
    /// </summary>
    /// <param name="source">The address to send from.</param>
    /// <param name="frame">The buffer to fill, at least the header long.</param>
    /// <returns>True when the source address parsed and the frame is filled.</returns>
    private static bool TryBuildBroadcastFrame(MACAddress source, Span<byte> frame)
    {
        frame.Clear();
        frame.Slice(DestinationOffset, MacAddressBytes).Fill(BroadcastByte);
        if (!TryParseMacAddress(source.ToString(), frame.Slice(SourceOffset, MacAddressBytes)))
        {
            return false;
        }

        frame[EtherTypeOffset] = (byte)(ExperimentalEtherType >> BitsPerByte);
        frame[EtherTypeLowByteOffset] = unchecked((byte)ExperimentalEtherType);
        return true;
    }

    /// <summary>
    /// Decodes the text form of an address, six hex pairs joined by
    /// colons, into its bytes. MACAddress hands its bytes only to the HAL
    /// and the ring; the text form is the public one.
    /// </summary>
    /// <param name="text">The address as MACAddress prints it.</param>
    /// <param name="bytes">Where the six bytes go.</param>
    /// <returns>True when the text had the expected shape.</returns>
    private static bool TryParseMacAddress(string text, Span<byte> bytes)
    {
        if (text.Length != MacAddressTextLength || bytes.Length != MacAddressBytes)
        {
            return false;
        }

        for (int i = 0; i < MacAddressBytes; i++)
        {
            int offset = i * MacAddressTextStride;
            int high = HexDigitValue(text[offset]);
            int low = HexDigitValue(text[offset + 1]);
            if (high < 0 || low < 0)
            {
                return false;
            }

            bytes[i] = (byte)((high << BitsPerHexDigit) | low);
        }

        return true;
    }

    /// <summary>Value of one hex digit in either case, or -1 for any other character.</summary>
    /// <param name="digit">The character to decode.</param>
    private static int HexDigitValue(char digit)
    {
        if (digit >= '0' && digit <= '9')
        {
            return digit - '0';
        }

        if (digit >= 'a' && digit <= 'f')
        {
            return digit - 'a' + HexLetterBase;
        }

        if (digit >= 'A' && digit <= 'F')
        {
            return digit - 'A' + HexLetterBase;
        }

        return -1;
    }

    private bool TryGetKeyboard([NotNullWhen(true)] out DeviceNode? node, [NotNullWhen(true)] out KeyboardState? state, [NotNullWhen(true)] out SyntheticAccess? access)
    {
        node = _keyboardNode;
        state = _keyboardState;
        access = _keyboardAccess;
        if (node is null || state is null || access is null)
        {
            Assert.Fail("the keyboard device was not set up by the publish test");
            return false;
        }

        return true;
    }

    private static int FindDriverIndex(string name)
    {
        int count = DriverInfo.DriverCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetDriver(i, out DriverEntryInfo info) && info.Name == name)
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

    private static int FindDeviceIndex(string name)
    {
        int count = DriverInfo.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetDevice(i, out PublishedDeviceInfo info) && info.Name == name)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool TryFindNode(string path, out DeviceNodeInfo info) => DriverInfo.TryGetNode(FindNodeIndex(path), out info);

    private static bool TryFindOffer(string path, int offerIndex, out DeviceOfferInfo info) => DriverInfo.TryGetOffer(FindNodeIndex(path), offerIndex, out info);
}
