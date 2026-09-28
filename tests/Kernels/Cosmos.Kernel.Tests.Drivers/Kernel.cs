// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.Tests.Drivers.Drivers;
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
/// periodic work, driver threads, teardown order and accounting, and child
/// nodes. Every assertion reads <see cref="DriverInfo"/> or the suite's own
/// drivers and consumer, never the serial log. One node is published from
/// the constructor, before the driver stage, to cover the boot path; the
/// rest are published from the tests, which is the hot-plug path.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests: 6 manifest, 2 engine, 5 arbitration, 7 keyboard device, 3 retract, 3 children, 1 diagnostics.</summary>
    private const int ExpectedTestCount = 27;

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

    private readonly TestKeyboardConsumer _keyboardConsumer = new();
    private readonly DeviceNode _bootNode;
    private DeviceNode? _keyboardNode;
    private KeyboardState? _keyboardState;
    private SyntheticAccess? _keyboardAccess;
    private DeviceNode? _busNode;
    private BusState? _busState;

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
        TR.Run("Manifest_Order_FollowsDeclarationOrder", TestManifestOrderFollowsDeclarationOrder);

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
        TR.Run("Interrupt_WorkItemRunsOnWorker", TestInterruptWorkItemRunsOnWorker);
        TR.Run("Interrupt_MaskUnmask", TestInterruptMaskUnmask);
        TR.Run("Periodic_FiresAtLeastThreeTimes", TestPeriodicFiresAtLeastThreeTimes);
        TR.Run("BlockingHandler_FaultRecorded", TestBlockingHandlerFaultRecorded);

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

    // The two tie drivers are declared in one file, first before second, so
    // their manifest positions follow the declaration order whatever order
    // the compiler gives the files.
    private static void TestManifestOrderFollowsDeclarationOrder()
    {
        int first = FindDriverIndex(nameof(TieFirstDriver));
        int second = FindDriverIndex(nameof(TieSecondDriver));
        Assert.True(first >= 0, "TieFirstDriver should be in the manifest");
        Assert.True(second >= 0, "TieSecondDriver should be in the manifest");
        Assert.True(first < second, "manifest order should follow declaration order within a file");
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
        DriverEngine.WaitForQueuedJobs();

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
        DriverEngine.WaitForQueuedJobs();

        Assert.True(raised, "a connected, unmasked source should run the handler");
        Assert.Equal(interruptsBefore + 1, state.InterruptCount, "the handler should have run once");
        Assert.Equal(keysBefore + 1, _keyboardConsumer.KeyCount, "the report should reach the consumer");
        Assert.Equal(TestScanCode, _keyboardConsumer.LastScanCode, "the handler should read the scan code the test wrote into the window");
        Assert.False(_keyboardConsumer.LastReleased, "a clear flags register is a key press");

        access.Window[KeyboardState.FlagsOffset] = ReleasedFlag;
        raised = SyntheticBus.RaiseInterrupt(node, 0);
        DriverEngine.WaitForQueuedJobs();

        Assert.True(raised, "the second raise should run the handler");
        Assert.True(_keyboardConsumer.LastReleased, "a set release flag is a key release");
        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info) && info.FaultCount == 0, "a well-behaved handler records no fault");
    }

    private void TestInterruptWorkItemRunsOnWorker()
    {
        if (!TryGetKeyboard(out DeviceNode? node, out KeyboardState? state, out _))
        {
            return;
        }

        int runsBefore = state.KeyWorkRuns;
        Assert.True(SyntheticBus.RaiseInterrupt(node, 0), "the raise should run the handler");
        DriverEngine.WaitForQueuedJobs();

        Assert.Equal(runsBefore + 1, state.KeyWorkRuns, "the work item the handler scheduled should have run once");
        Assert.True(state.KeyWorkRanOnWorker, "the work item should run on the kit worker");
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
        DriverEngine.WaitForQueuedJobs();
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
            DriverEngine.WaitForQueuedJobs();
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
        DriverEngine.WaitForQueuedJobs();

        Assert.Equal(1, state.HandlerRuns, "the handler should have been entered once");
        Assert.True(TryFindNode(node.Path, out DeviceNodeInfo info), "the block node should be in the tree");
        Assert.Equal(1, info.FaultCount, "the guard's exception should be recorded as one fault");
        Assert.NotNull(info.LastFault);
        Assert.True(info.State == DeviceNodeState.Bound, "a fault does not tear the binding down");
        Assert.True(state.Handle is { IsMasked: true }, "a faulting handler leaves its source masked");
        Assert.False(SyntheticBus.RaiseInterrupt(node, 0), "the masked source is not delivered again");
        Assert.Equal(1, state.HandlerRuns, "the handler should not run again while masked");
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
        DriverEngine.WaitForQueuedJobs();

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
        DriverEngine.WaitForQueuedJobs();
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
        DriverEngine.WaitForQueuedJobs();

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

    // ==================== Helpers ====================

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
