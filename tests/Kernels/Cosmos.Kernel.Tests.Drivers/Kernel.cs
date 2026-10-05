// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Drivers;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;
using Cosmos.Kernel.HAL.DriverKit.Usb;
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
/// the E1000E tests skip on arm64. The usb-kbd cell adds a qemu-xhci
/// controller with a usb-kbd plugged in at boot: the USB keyboard group
/// proves the kit's Usb bus kind over it, the decline-after-open
/// fall-through to the shipped <see cref="UsbKeyboardDriver"/>, and the
/// unplug and replug the engine performs over QMP when a test asks; the
/// group skips on the bare cell, which has no controller. q35's built-in
/// 8042 carries a keyboard and a mouse on both x64 cells: the PS/2 group
/// proves the kit's Ps2 bus kind over it, with the key and the pointer
/// events the engine injects over QMP when a test asks; virt has no 8042
/// and the group skips on arm64. The virtio-blk-pci cell (both arches) and
/// the virtio-blk-mmio cell (arm64) attach one virtio-blk disk: the
/// virtio-blk group proves the library's <see cref="VirtioBlkDriver"/>
/// over the kit's Virtio bus kind under either transport, publishing the
/// disk to the ring's storage manager; the group skips on the other cells.
/// </para>
/// <para>
/// The suite is two projects. This kernel is the harness: it holds an
/// <c>InternalsVisibleTo</c> grant from <c>Cosmos.Kernel.HAL</c> for one
/// purpose, installing <see cref="TestKeyboardConsumer"/> and
/// <see cref="TestPointerConsumer"/> through the internal
/// <see cref="DeviceRegistry"/>. The drivers it drives live in
/// <c>Cosmos.Kernel.Tests.Drivers.Library</c>, a driver assembly with no
/// grant at all, written over the public seam only, so their compiling is
/// the proof that a third party can write every one of them.
/// </para>
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests: 6 manifest, 2 engine, 5 arbitration, 7 keyboard device, 1 display device, 1 block device, 3 retract, 3 children, 1 diagnostics, 5 hardware, 6 USB keyboard, 7 PS/2, 6 virtio-blk.</summary>
    private const int ExpectedTestCount = 53;

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

    /// <summary>Bus name of the interface nodes the kit's Usb bus kind publishes.</summary>
    private const string UsbBusName = "usb";

    /// <summary>The end of a function node's description for an xHCI controller: class 0C, subclass 03, programming interface 30.</summary>
    private const string XhciClassDescription = "class 0c.03.30";

    /// <summary>The end of an interface node's description for a HID boot keyboard: interface class 03, subclass 01, protocol 01.</summary>
    private const string UsbKeyboardDescriptionSuffix = "class 03.01.01";

    /// <summary>The name the shipped driver publishes its keyboard under.</summary>
    private const string UsbKeyboardName = "usb-keyboard";

    /// <summary>Asks the engine to pull the cell's USB keyboard out (see TR.RequestHost).</summary>
    private const string UsbKeyboardUnplugRequest = "usb-kbd-unplug";

    /// <summary>Asks the engine to plug it back in.</summary>
    private const string UsbKeyboardPlugRequest = "usb-kbd-plug";

    /// <summary>
    /// Longest wait for the hot-plug thread and the kit worker to follow a
    /// plug or an unplug. Well inside the engine's stall window: 10 s
    /// without a protocol message and it kills the guest.
    /// </summary>
    private const int HotPlugTimeoutMilliseconds = 8000;

    /// <summary>How long a hot-plug wait sleeps between looks, so those threads get to run.</summary>
    private const int HotPlugPollMilliseconds = 50;

    /// <summary>Skip reason of the USB keyboard tests on a cell whose bus carries no xHCI controller.</summary>
    private const string SkipNoXhci = "no xHCI controller on this cell";

    /// <summary>The HID output report lighting Num Lock (bit 0) and Caps Lock (bit 1).</summary>
    private const byte NumLockCapsLockReport = 0x03;

    /// <summary>The HID output report lighting Scroll Lock (bit 2).</summary>
    private const byte ScrollLockReport = 0x04;

    /// <summary>Specificity of a USB match on the interface class, subclass and protocol: the shipped keyboard driver's and the declining driver's.</summary>
    private const int UsbKeyboardMatchSpecificity = 3;

    /// <summary>Bus name of the port nodes the shipped 8042 driver publishes.</summary>
    private const string Ps2BusName = "ps2";

    /// <summary>The 8042 node's compatible string, which the x64 machine description publishes it under.</summary>
    private const string I8042Compatible = "pnp0303";

    /// <summary>The 8042 node's description: its one compatible string.</summary>
    private const string I8042Description = "compatible pnp0303";

    /// <summary>The 8042 node's path, asserted: the machine description names it.</summary>
    private const string I8042Path = "platform:i8042@60";

    /// <summary>The keyboard port's node path.</summary>
    private const string Ps2KeyboardPath = "ps2:kbd";

    /// <summary>The auxiliary port's node path.</summary>
    private const string Ps2MousePath = "ps2:aux";

    /// <summary>The keyboard port's node description.</summary>
    private const string Ps2KeyboardDescription = "port kbd";

    /// <summary>The auxiliary port's node description.</summary>
    private const string Ps2MouseDescription = "port aux";

    /// <summary>The name the shipped driver publishes the PS/2 keyboard under.</summary>
    private const string Ps2KeyboardName = "ps2-keyboard";

    /// <summary>The name the shipped driver publishes the PS/2 mouse under.</summary>
    private const string Ps2MouseName = "ps2-mouse";

    /// <summary>Asks the engine to press and release a key (see TR.RequestHost): QEMU's qcode a, which the controller's translation delivers as set 1 make 0x1E and break 0x9E.</summary>
    private const string Ps2KeyRequest = "key-press a";

    /// <summary>Asks the engine to move the mouse 10 units right and none down.</summary>
    private const string Ps2MouseMoveRequest = "mouse-move 10 0";

    /// <summary>Asks the engine to press the left mouse button.</summary>
    private const string Ps2MouseButtonDownRequest = "mouse-button left down";

    /// <summary>Asks the engine to release it.</summary>
    private const string Ps2MouseButtonUpRequest = "mouse-button left up";

    /// <summary>The horizontal movement the move request asks for.</summary>
    private const int Ps2MoveDeltaX = 10;

    /// <summary>0xED's byte: num lock bit 1, caps lock bit 2.</summary>
    private const byte NumLockCapsLockLedByte = 0x06;

    /// <summary>Resources the 8042's binding holds when both lines are routed: two port windows and two line handles; the lock is not counted.</summary>
    private const int I8042InterruptDrivenHeldResources = 4;

    /// <summary>Skip reason of the PS/2 tests on a cell whose machine has no 8042.</summary>
    private const string SkipNo8042 = "no 8042 on this cell";

    /// <summary>Skip reason of the key injection test on a cell with a USB keyboard, which QEMU hands the key to.</summary>
    private const string SkipUsbKeyboardTakesKeys = "a usb-kbd on this cell takes the host's keys";

    /// <summary>Bus name of the device nodes the virtio transport drivers publish.</summary>
    private const string VirtioBusName = "virtio";

    /// <summary>The start of a virtio node's description for a block device; the trailing space keeps type 2x out.</summary>
    private const string VirtioBlkDescriptionPrefix = "type 2 ";

    /// <summary>Start of a function node's description for a transitional virtio-blk-pci function (vendor 1af4, device 1001), on a root bus.</summary>
    private const string TransitionalBlkFunctionPrefix = "1af4:1001";

    /// <summary>Start of a function node's description for a modern-only virtio-blk-pci function (vendor 1af4, device 1042), behind a PCI Express port.</summary>
    private const string ModernBlkFunctionPrefix = "1af4:1042";

    /// <summary>The start of every name the virtio-blk driver publishes a disk under.</summary>
    private const string VirtioBlkNamePrefix = "vblk";

    /// <summary>The engine's 256 MiB sparse image.</summary>
    private const long VirtioBlkImageBytes = 268435456L;

    /// <summary>Bytes per sector of the engine's image, the device's block size.</summary>
    private const int VirtioBlkSectorBytes = 512;

    /// <summary>The block the round trip test saves, overwrites and restores: well past any partition table and inside the image.</summary>
    private const ulong VirtioBlkProbeLba = 200000UL;

    /// <summary>More than the driver moves in one request: 130 blocks of 512 bytes is 66560, above the 65536-byte bounce block.</summary>
    private const int VirtioBlkSpanBlocks = 130;

    /// <summary>Skip reason of the virtio-blk tests on a cell that attaches no virtio-blk disk.</summary>
    private const string SkipNoVirtioBlk = "no virtio-blk device on this cell";

    /// <summary>The start of a virtio node's path under the PCI transport.</summary>
    private const string VirtioPciPathPrefix = "virtio:pci:";

    /// <summary>The start of a virtio node's path under the MMIO transport.</summary>
    private const string VirtioMmioPathPrefix = "virtio:mmio:";

    private readonly TestKeyboardConsumer _keyboardConsumer = new();
    private readonly TestPointerConsumer _pointerConsumer = new();
    private readonly DeviceNode _bootNode;
    private DeviceNode? _keyboardNode;
    private KeyboardState? _keyboardState;
    private SyntheticAccess? _keyboardAccess;
    private DeviceNode? _busNode;
    private BusState? _busState;
    private string? _e1000ePath;
    private string? _xhciPath;
    private string? _usbKeyboardPath;
    private DeviceNode? _usbKeyboardNode;
    private string? _i8042Path;
    private string? _virtioBlkPath;

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

        // ... and for the ring's mouse manager, so the PS/2 mouse's reports land here.
        DeviceRegistry.SetConsumer(DeviceKind.Pointer, _pointerConsumer);

        TR.Start("Driver Kit Tests", expectedTests: ExpectedTestCount);

        // ==================== Manifest ====================
        TR.Run("Manifest_HighPriorityDriver_Present", TestManifestHighPriorityDriverPresent);
        TR.Run("Manifest_FatFeatureDriver_Absent", TestManifestFatFeatureDriverAbsent);
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

        // ==================== USB keyboard ====================
        // The usb-kbd cell carries a qemu-xhci controller with a usb-kbd plugged
        // in at boot; the engine unplugs and replugs it over QMP when asked. On
        // the bare cell there is no controller and the group skips.
        _xhciPath = FindXhciPath();
        bool hasXhci = _xhciPath is not null;
        TR.RunIf(hasXhci, "Usb_XhciHost_Bound", TestUsbXhciHostBound, SkipNoXhci);
        TR.RunIf(hasXhci, "Usb_Keyboard_BoundAfterDecline", TestUsbKeyboardBoundAfterDecline, SkipNoXhci);
        TR.RunIf(hasXhci, "Usb_Keyboard_SetLedsRoundTrip", TestUsbKeyboardSetLedsRoundTrip, SkipNoXhci);
        TR.RunIf(hasXhci, "Usb_Keyboard_Unplug_RetractsNode", TestUsbKeyboardUnplugRetractsNode, SkipNoXhci);
        TR.RunIf(hasXhci, "Usb_Keyboard_Replug_PublishesAgain", TestUsbKeyboardReplugPublishesAgain, SkipNoXhci);
        TR.RunIf(hasXhci, "Usb_Keyboard_Replug_SetLeds", TestUsbKeyboardReplugSetLeds, SkipNoXhci);

        // ==================== PS/2 ====================
        // q35 has an 8042 with a keyboard and a mouse built in, on both x64
        // cells; virt has none and the group skips. The host injects a key and
        // pointer events over QMP when asked; with a usb-kbd present QEMU hands
        // the key to it, so the key test runs on the bare cell only.
        _i8042Path = FindI8042Path();
        bool has8042 = _i8042Path is not null;
        TR.RunIf(has8042, "Ps2_Controller_Bound", TestPs2ControllerBound, SkipNo8042);
        TR.RunIf(has8042, "Ps2_Keyboard_Bound", TestPs2KeyboardBound, SkipNo8042);
        TR.RunIf(has8042 && !hasXhci, "Ps2_Keyboard_KeyInjected", TestPs2KeyboardKeyInjected, has8042 ? SkipUsbKeyboardTakesKeys : SkipNo8042);
        TR.RunIf(has8042, "Ps2_Keyboard_SetLedsRoundTrip", TestPs2KeyboardSetLedsRoundTrip, SkipNo8042);
        TR.RunIf(has8042, "Ps2_Mouse_Bound", TestPs2MouseBound, SkipNo8042);
        TR.RunIf(has8042, "Ps2_Mouse_MovementInjected", TestPs2MouseMovementInjected, SkipNo8042);
        TR.RunIf(has8042, "Ps2_Mouse_ButtonInjected", TestPs2MouseButtonInjected, SkipNo8042);

        // ==================== virtio-blk ====================
        // The virtio-blk-pci and virtio-blk-mmio cells attach one virtio-blk disk;
        // bare and usb-kbd attach none and the group skips. The driver under test
        // is the library's own in this commit and the shipped one after the
        // promotion, over the same six assertions.
        _virtioBlkPath = FindVirtioBlkPath();
        bool hasVirtioBlk = _virtioBlkPath is not null;
        TR.RunIf(hasVirtioBlk, "VirtioBlk_Bound", TestVirtioBlkBound, SkipNoVirtioBlk);
        TR.RunIf(hasVirtioBlk, "VirtioBlk_TransportMatchesCell", TestVirtioBlkTransportMatchesCell, SkipNoVirtioBlk);
        TR.RunIf(hasVirtioBlk, "VirtioBlk_CapacityMatchesImage", TestVirtioBlkCapacityMatchesImage, SkipNoVirtioBlk);
        TR.RunIf(hasVirtioBlk, "VirtioBlk_ReadWriteRoundTrip", TestVirtioBlkReadWriteRoundTrip, SkipNoVirtioBlk);
        TR.RunIf(hasVirtioBlk, "VirtioBlk_FlushCompletes", TestVirtioBlkFlushCompletes, SkipNoVirtioBlk);
        TR.RunIf(hasVirtioBlk, "VirtioBlk_InterruptOrPolled", TestVirtioBlkInterruptOrPolled, SkipNoVirtioBlk);

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

    // The driver is tied to the FAT feature and the project turns the
    // feature off, so the generated manifest guards its registration behind
    // KernelFeatures.Fat and the guard folds to nothing.
    private static void TestManifestFatFeatureDriverAbsent()
    {
        Assert.True(FindDriverIndex(nameof(FatFeatureDriver)) < 0, "a driver tied to a feature that is off should not be in the manifest");
        Assert.Null(RecordingDriver.Find<FatFeatureDriver>(), "the registry should hold no FatFeatureDriver");
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
        int nodesBefore = DriverInfo.NodeCount;

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

        Assert.False(TryFindNode(node.Path, out _), "a retracted node leaves the tree");
        Assert.Equal(nodesBefore - 1, DriverInfo.NodeCount, "the node count should drop by one");
        Assert.True(node.State == NodeState.Retracted, "the node should be retracted");
        Assert.True(node.Binding is { Driver.Name: nameof(KeyboardDriver) }, "the node still names the driver that held it last");
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
        Assert.True(node.LeakedResourceCount == 0, "a thread that stopped in time leaks nothing");
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
        int nodesBefore = DriverInfo.NodeCount;

        SyntheticBus.Retract(bus);
        SyntheticBus.WaitForQueuedJobs();

        Assert.True(child is not null && child.DetachCount == 1, "the child driver should be detached once");
        Assert.True(child is not null && child.LastDetachReason.Cause == DetachCause.ParentRetracted, "the child should be told its parent went away");
        Assert.True(child is not null && !child.LastDetachReason.HardwarePresent, "the child inherits the parent's hardware state");
        Assert.True(busDriver is not null && busDriver.DetachCount == 1 && busDriver.LastDetachReason.Cause == DetachCause.Retracted, "the bus driver should see its own retraction");

        Assert.False(TryFindNode(busState.Child.Path, out _) || TryFindNode(busState.Orphan.Path, out _) || TryFindNode(bus.Path, out _), "a retracted parent and its children leave the tree");
        Assert.Equal(nodesBefore - 3, DriverInfo.NodeCount, "the three nodes should leave the count");
        Assert.True(busState.Child.State == NodeState.Retracted && busState.Orphan.State == NodeState.Retracted && bus.State == NodeState.Retracted, "all three should be retracted");
        Assert.Equal(2, bus.Children.Count, "children retracted with their parent stay on the parent");
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

    // ==================== USB keyboard ====================
    //
    // The kit's Usb bus kind over the cell's qemu-xhci controller: the host
    // driver bound and running its hot-plug thread, the keyboard interface
    // bound by the shipped driver after the suite's higher-priority driver
    // opened its pipe and declined, an LED write through the state object,
    // then one unplug and one replug the engine performs over QMP. The
    // shipped drivers are no RecordingDriver, so their detach shows through
    // the node, the kit's counts and the suite's consumer.

    private void TestUsbXhciHostBound()
    {
        if (!TryGetXhci(out string? path, out DeviceNodeInfo info))
        {
            return;
        }

        Assert.True(info.State == DeviceNodeState.Bound, "the xHCI driver should hold the controller");
        Assert.True(info.DriverName == nameof(XhciDriver), "XhciDriver should hold the controller");
        Assert.True(info.ChildCount >= 1, "the controller should have published the keyboard's interface node under it");
        Assert.True(info.HeldResourceCount >= 1, "the binding should hold the controller's register window at least");

        XhciState? state = FindDriverState<XhciState>(path);
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        Assert.True(state.HotPlugRunning, "TryStartThread needs the scheduler");
        Assert.True(state.HasInterrupt != state.IsPolling, "the controller takes its events from a message interrupt or polls, never both or neither");
        Assert.True(state.Bus.DeviceCount >= 1, "the bus should carry the keyboard plugged in at boot");
        Assert.Equal(1, state.Bus.Ordinal, "the cell's one controller is bus 1");
    }

    // The keyboard published at boot went to the ring's consumer, so the
    // keyboard manager holds it; the suite's consumer, installed afterwards,
    // never saw it and its published count is not read here.
    private void TestUsbKeyboardBoundAfterDecline()
    {
        string? path = FindUsbKeyboardPath();
        Assert.NotNull(path, "the usb-kbd cell should carry a HID boot keyboard interface");
        if (path is null)
        {
            return;
        }

        _usbKeyboardPath = path;
        _usbKeyboardNode = FindNode(path);
        Assert.NotNull(_usbKeyboardNode, "the keyboard's node should be in the tree");

        Assert.True(TryFindNode(path, out DeviceNodeInfo info), "the keyboard node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the keyboard interface should be bound");
        Assert.True(info.DriverName == nameof(UsbKeyboardDriver), "UsbKeyboardDriver should hold the keyboard interface");
        Assert.True(info.ParentPath == _xhciPath, "the interface node should sit under the controller's node");
        Assert.True(info.BusName == UsbBusName, "the interface node sits on the usb bus");
        Assert.Equal(0, info.ResourceCount, "a USB interface node carries no resources");
        Assert.Equal(0, info.InterruptCount, "a USB interface node carries no interrupt sources");
        Assert.Equal(2, info.OfferCount, "the declining driver and then the shipped driver should have been offered the node");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should publish one keyboard");

        Assert.True(TryFindOffer(path, 0, out DeviceOfferInfo declined), "the declined offer should be recorded");
        Assert.True(declined.DriverName == nameof(UsbDeclineDriver), "the higher priority driver should be offered first");
        Assert.Equal(UsbDeclineDriver.ClaimedPriority, declined.Priority, "the offer should record the declining driver's priority");
        Assert.Equal(UsbKeyboardMatchSpecificity, declined.Specificity, "the offer should record the three-field match");
        Assert.True(declined.Outcome == DeviceOfferOutcome.Declined, "the first offer should be declined");
        Assert.True(declined.Reason == UsbDeclineDriver.Reason, "the offer should carry the driver's reason");
        Assert.Equal(1, declined.ReleasedResourceCount, "the kit should have closed the pipe the declining probe opened");

        Assert.True(TryFindOffer(path, 1, out DeviceOfferInfo bound), "the bound offer should be recorded");
        Assert.True(bound.DriverName == nameof(UsbKeyboardDriver), "the shipped driver should be offered second");
        Assert.True(bound.Outcome == DeviceOfferOutcome.Bound, "the second offer should be bound");

        UsbDeclineDriver? declining = RecordingDriver.Find<UsbDeclineDriver>();
        Assert.True(declining is not null && declining.ProbeCount >= 1 && declining.PipeOpens >= 1, "the declining driver should have been probed and opened the pipe");

        int deviceIndex = FindDeviceIndex(UsbKeyboardName);
        Assert.True(deviceIndex >= 0, "the keyboard should be in the published list");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.Kind == PublishedDeviceKind.Keyboard, "the published device should be a keyboard");
            Assert.True(device.IsConsumed, "the keyboard consumer should have taken the keyboard");
            Assert.False(device.IsWithdrawn, "the keyboard should still be published");
            Assert.True(device.DriverName == nameof(UsbKeyboardDriver), "the published device should name its driver");
            Assert.True(device.NodePath == path, "the published device should name the interface node");
        }
    }

    private void TestUsbKeyboardSetLedsRoundTrip()
    {
        if (!TryGetUsbKeyboardState(out UsbKeyboardState? state))
        {
            return;
        }

        int writes = state.LedWrites;
        state.SetLeds(KeyboardLeds.NumLock | KeyboardLeds.CapsLock);

        Assert.Equal(writes + 1, state.LedWrites, "SetLeds should write one output report");
        Assert.Equal(NumLockCapsLockReport, state.LastLedReport, "the report should carry the HID Num Lock and Caps Lock bits");
        Assert.True(state.LastLedStatus == UsbTransferStatus.Success, "the keyboard should accept the output report");
    }

    // The node leaves the tree on the kit worker, in the teardown's last
    // step; the bus releases the device on the hot-plug thread, after the
    // slot is disabled. The wait covers both.
    private void TestUsbKeyboardUnplugRetractsNode()
    {
        if (!TryGetXhci(out string? xhciPath, out DeviceNodeInfo hostBefore) || !TryGetUsbKeyboard(out string? path, out DeviceNode? node))
        {
            return;
        }

        XhciState? state = FindDriverState<XhciState>(xhciPath);
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        int childrenBefore = hostBefore.ChildCount;
        int nodesBefore = DriverInfo.NodeCount;
        int devicesBefore = DriverInfo.DeviceCount;
        int withdrawnBefore = _keyboardConsumer.WithdrawnCount;

        TR.RequestHost(UsbKeyboardUnplugRequest);

        Assert.True(WaitUntil(() => !TryFindNode(path, out _) && state.Bus.DeviceCount == 0), "the keyboard node should leave the tree and the bus should release its device after the unplug");
        Assert.True(node.State == NodeState.Retracted, "the node should be retracted");
        Assert.True(node.Binding is { IsDetaching: true }, "the binding should be flagged as detaching");
        Assert.True(FindDeviceIndex(UsbKeyboardName) < 0, "the withdrawn keyboard should have left the published list");
        Assert.Equal(devicesBefore - 1, DriverInfo.DeviceCount, "the published list should have shrunk by one");
        Assert.Equal(withdrawnBefore + 1, _keyboardConsumer.WithdrawnCount, "the consumer should be told the keyboard is gone");
        Assert.Equal(nodesBefore - 1, DriverInfo.NodeCount, "the node count should drop by one");
        Assert.True(TryFindNode(xhciPath, out DeviceNodeInfo hostAfter), "the controller's node should still be in the tree");
        Assert.Equal(childrenBefore - 1, hostAfter.ChildCount, "a retracted child leaves its parent's count");
        Assert.Equal(0, state.Bus.DeviceCount, "the bus should carry no device once the keyboard is gone");
        Assert.True(state.PipesClosed >= 1, "the controller should have closed the keyboard's report pipe");
    }

    // The root port QEMU picks for the replug is its own (the next free
    // one), so the new path is logged, not asserted.
    private void TestUsbKeyboardReplugPublishesAgain()
    {
        if (!TryGetXhci(out string? xhciPath, out DeviceNodeInfo hostBefore))
        {
            return;
        }

        XhciState? state = FindDriverState<XhciState>(xhciPath);
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        int publishedBefore = _keyboardConsumer.PublishedCount;
        int nodesBefore = DriverInfo.NodeCount;
        int childrenBefore = hostBefore.ChildCount;

        TR.RequestHost(UsbKeyboardPlugRequest);

        Assert.True(WaitUntil(() => FindUsbKeyboardPath() is { } found && TryFindNode(found, out DeviceNodeInfo foundInfo) && foundInfo.State == DeviceNodeState.Bound && state.Bus.DeviceCount == 1), "the keyboard should be bound again after the replug");

        DeviceNode? oldNode = _usbKeyboardNode;
        string? path = FindUsbKeyboardPath();
        Assert.NotNull(path, "the replugged keyboard's interface should be in the tree");
        if (path is null)
        {
            return;
        }

        _usbKeyboardPath = path;
        _usbKeyboardNode = FindNode(path);
        Assert.True(_usbKeyboardNode is not null && !ReferenceEquals(_usbKeyboardNode, oldNode), "the replug should publish a new node");
        Log.WriteString("[DriversTests] replugged keyboard at ");
        Log.WriteString(path);
        Log.WriteString("\n");

        Assert.True(TryFindNode(path, out DeviceNodeInfo info), "the new keyboard node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the new keyboard interface should be bound");
        Assert.True(info.DriverName == nameof(UsbKeyboardDriver), "UsbKeyboardDriver should hold the new interface");
        Assert.Equal(2, info.OfferCount, "the declining driver and then the shipped driver should have been offered the new node");
        Assert.True(TryFindOffer(path, 0, out DeviceOfferInfo declined), "the declined offer should be recorded");
        Assert.True(declined.DriverName == nameof(UsbDeclineDriver) && declined.Outcome == DeviceOfferOutcome.Declined, "the declining driver should have declined the new node first");

        Assert.Equal(publishedBefore + 1, _keyboardConsumer.PublishedCount, "the suite's consumer should be handed the replugged keyboard");
        Assert.True(_keyboardConsumer.LastPublished is { Device: IKeyboard keyboard } && keyboard.Name == UsbKeyboardName, "the published device should be the shipped driver's keyboard");
        Assert.Equal(nodesBefore + 1, DriverInfo.NodeCount, "the node count should grow by one");
        Assert.True(TryFindNode(xhciPath, out DeviceNodeInfo hostAfter), "the controller's node should still be in the tree");
        Assert.Equal(childrenBefore + 1, hostAfter.ChildCount, "the controller should count its child again");
        Assert.True(FindDeviceIndex(UsbKeyboardName) >= 0, "the keyboard should be back in the published list");
    }

    private void TestUsbKeyboardReplugSetLeds()
    {
        if (!TryGetUsbKeyboardState(out UsbKeyboardState? state))
        {
            return;
        }

        state.SetLeds(KeyboardLeds.ScrollLock);

        Assert.Equal(ScrollLockReport, state.LastLedReport, "the report should carry the HID Scroll Lock bit");
        Assert.True(state.LastLedStatus == UsbTransferStatus.Success, "the replugged keyboard should accept the output report");
        Assert.Equal(1, state.LedWrites, "a fresh state should count this write alone");
    }

    // ==================== PS/2 ====================
    //
    // The kit's Ps2 bus kind over q35's built-in 8042: the controller node
    // the x64 machine description publishes, bound by the shipped
    // I8042Driver at the driver stage, the two port nodes it published,
    // bound by the shipped keyboard and mouse drivers, an LED write through
    // the keyboard's state object, then a key press and pointer events the
    // engine injects over QMP. The shipped drivers are no RecordingDriver,
    // so their work shows through the nodes, the states' counters and the
    // suite's two consumers.

    private void TestPs2ControllerBound()
    {
        if (!TryGetI8042(out string? path, out DeviceNodeInfo info))
        {
            return;
        }

        Assert.True(info.Path == I8042Path, "the machine description names the controller platform:i8042@60");
        Assert.True(info.State == DeviceNodeState.Bound, "the 8042 driver should hold the controller");
        Assert.True(info.DriverName == nameof(I8042Driver), "I8042Driver should hold the controller");
        Assert.True(info.Description == I8042Description, "the node's one compatible string is pnp0303");
        Assert.Equal(2, info.ResourceCount, "the data port and the status and command port");
        Assert.Equal(2, info.InterruptCount, "lines 1 and 12");
        Assert.Equal(2, info.ChildCount, "q35's 8042 is dual channel and both ports pass");
        Assert.Equal(0, info.PublishedDeviceCount, "the controller publishes nodes, not devices");

        I8042State? state = FindDriverState<I8042State>(path);
        Assert.NotNull(state);
        if (state is null)
        {
            return;
        }

        Assert.True(state.IsDualChannel, "q35's 8042 has a second port");
        Assert.True(state.InterruptDriven, "the I/O APIC routes lines 1 and 12 on q35");
        Assert.False(state.PolledPeriodically, "a controller with both lines routed needs no periodic drain");
        Assert.Equal(I8042InterruptDrivenHeldResources, info.HeldResourceCount, "two port windows and two line handles");
        Assert.Equal(0, state.StrayBytes, "no byte should have arrived while the ports were between probes");
    }

    // The keyboard published at the driver stage went to the ring's
    // consumer, so the keyboard manager holds it; the suite's consumer,
    // installed afterwards, never saw it and its published count is not
    // read here.
    private void TestPs2KeyboardBound()
    {
        if (!TryGetI8042(out string? path, out _))
        {
            return;
        }

        Assert.True(TryFindNode(Ps2KeyboardPath, out DeviceNodeInfo info), "the keyboard port's node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the keyboard port should be bound");
        Assert.True(info.DriverName == nameof(Ps2KeyboardDriver), "Ps2KeyboardDriver should hold the keyboard port");
        Assert.True(info.BusName == Ps2BusName, "the port node sits on the ps2 bus");
        Assert.True(info.Description == Ps2KeyboardDescription, "the port node describes itself as port kbd");
        Assert.True(info.ParentPath == path, "the port node should sit under the controller's node");
        Assert.Equal(0, info.ResourceCount, "a port node carries no resources");
        Assert.Equal(1, info.InterruptCount, "a port node carries the port's interrupt source");
        Assert.Equal(1, info.OfferCount, "the shipped driver should have bound on the first offer");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should publish one keyboard");
        Assert.Equal(1, info.HeldResourceCount, "the port's interrupt handle");

        int deviceIndex = FindDeviceIndex(Ps2KeyboardName);
        Assert.True(deviceIndex >= 0, "the keyboard should be in the published list");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.Kind == PublishedDeviceKind.Keyboard, "the published device should be a keyboard");
            Assert.True(device.IsConsumed, "the keyboard consumer should have taken the keyboard");
            Assert.False(device.IsWithdrawn, "the keyboard should still be published");
            Assert.True(device.DriverName == nameof(Ps2KeyboardDriver), "the published device should name its driver");
            Assert.True(device.NodePath == Ps2KeyboardPath, "the published device should name the port node");
        }

        if (!TryGetPs2KeyboardState(out Ps2KeyboardState? state))
        {
            return;
        }

        Assert.False(state.IsAtKeyboard, "QEMU's keyboard answers identify with AB 41");
    }

    // QMP send-key releases the key after its default 100 ms hold time, so
    // the test waits for the make and the break.
    private void TestPs2KeyboardKeyInjected()
    {
        if (!TryGetPs2KeyboardState(out Ps2KeyboardState? state))
        {
            return;
        }

        int keysBefore = _keyboardConsumer.KeyCount;
        int eventsBefore = state.KeyEvents;

        TR.RequestHost(Ps2KeyRequest);

        Assert.True(WaitUntil(() => _keyboardConsumer.KeyCount >= keysBefore + 2), "the make and the break of the injected key should reach the consumer");
        Assert.Equal(TestScanCode, _keyboardConsumer.LastPressedScanCode, "qcode a is set 1 make code 0x1E");
        Assert.True(_keyboardConsumer.LastScanCode == TestScanCode && _keyboardConsumer.LastReleased, "the last report is the release, 0x9E on the wire");
        Assert.True(_keyboardConsumer.LastKeyDevice is { Device: IKeyboard keyboard } && keyboard.Name == Ps2KeyboardName, "the key came from the PS/2 keyboard");
        Assert.True(state.KeyEvents >= eventsBefore + 2, "the driver should have decoded the make and the break");
    }

    // Runs on the boot thread after the driver stage returned, when no
    // worker job sends to either port; the exchange's event polls its latch
    // with interrupts enabled and the acknowledgements arrive on IRQ 1.
    private void TestPs2KeyboardSetLedsRoundTrip()
    {
        if (!TryGetPs2KeyboardState(out Ps2KeyboardState? state))
        {
            return;
        }

        int writes = state.LedWrites;
        state.SetLeds(KeyboardLeds.NumLock | KeyboardLeds.CapsLock);

        Assert.Equal(writes + 1, state.LedWrites, "SetLeds should run one indicator exchange");
        Assert.Equal(NumLockCapsLockLedByte, state.LastLedByte, "the byte should carry the num lock and caps lock bits");
        Assert.True(state.LastLedAcknowledged, "QEMU's keyboard acknowledges 0xED and its byte");
    }

    // The mouse published at the driver stage went to the ring's consumer,
    // as the keyboard did; the suite's pointer consumer never saw it.
    private void TestPs2MouseBound()
    {
        if (!TryGetI8042(out string? path, out _))
        {
            return;
        }

        Assert.True(TryFindNode(Ps2MousePath, out DeviceNodeInfo info), "the auxiliary port's node should be in the tree");
        Assert.True(info.State == DeviceNodeState.Bound, "the auxiliary port should be bound");
        Assert.True(info.DriverName == nameof(Ps2MouseDriver), "Ps2MouseDriver should hold the auxiliary port");
        Assert.True(info.BusName == Ps2BusName, "the port node sits on the ps2 bus");
        Assert.True(info.Description == Ps2MouseDescription, "the port node describes itself as port aux");
        Assert.True(info.ParentPath == path, "the port node should sit under the controller's node");
        Assert.Equal(0, info.ResourceCount, "a port node carries no resources");
        Assert.Equal(1, info.InterruptCount, "a port node carries the port's interrupt source");
        Assert.Equal(1, info.OfferCount, "the shipped driver should have bound on the first offer");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should publish one pointer");
        Assert.Equal(1, info.HeldResourceCount, "the port's interrupt handle");

        int deviceIndex = FindDeviceIndex(Ps2MouseName);
        Assert.True(deviceIndex >= 0, "the mouse should be in the published list");
        if (DriverInfo.TryGetDevice(deviceIndex, out PublishedDeviceInfo device))
        {
            Assert.True(device.Kind == PublishedDeviceKind.Pointer, "the published device should be a pointer");
            Assert.True(device.IsConsumed, "the pointer consumer should have taken the mouse");
            Assert.False(device.IsWithdrawn, "the mouse should still be published");
            Assert.True(device.DriverName == nameof(Ps2MouseDriver), "the published device should name its driver");
            Assert.True(device.NodePath == Ps2MousePath, "the published device should name the port node");
        }

        if (!TryGetPs2MouseState(out Ps2MouseState? state))
        {
            return;
        }

        Assert.True(state.HasWheel, "QEMU's mouse answers 3 after the 200, 100, 80 knock");
        Assert.Equal(4, state.PacketBytes, "a wheel mouse sends four byte packets");
    }

    // The engine sends the move as one input-send-event with one sync, and
    // QEMU's PS/2 mouse splits a delta over several packets only beyond 127
    // units, so a 10 unit move is one packet and one report. The totals are
    // what the test reads.
    private void TestPs2MouseMovementInjected()
    {
        if (!TryGetPs2MouseState(out Ps2MouseState? state))
        {
            return;
        }

        int reportsBefore = _pointerConsumer.RelativeCount;
        int totalXBefore = _pointerConsumer.TotalDeltaX;
        int totalYBefore = _pointerConsumer.TotalDeltaY;
        int packetsBefore = state.PacketsReported;

        TR.RequestHost(Ps2MouseMoveRequest);

        Assert.True(WaitUntil(() => _pointerConsumer.RelativeCount >= reportsBefore + 1), "the movement should reach the pointer consumer");
        Assert.Equal(totalXBefore + Ps2MoveDeltaX, _pointerConsumer.TotalDeltaX, "the packets of a 10 unit move sum to 10");
        Assert.Equal(totalYBefore, _pointerConsumer.TotalDeltaY, "a horizontal move has no vertical part");
        Assert.True(_pointerConsumer.LastButtons == PointerButtons.None, "no button is down during the move");
        Assert.Equal(0, _pointerConsumer.LastWheel, "the wheel did not turn");
        Assert.True(_pointerConsumer.LastDevice is { Device: IPointer pointer } && pointer.Name == Ps2MouseName, "the movement came from the PS/2 mouse");
        Assert.True(state.PacketsReported >= packetsBefore + 1, "the driver should have reported at least one packet");
        Assert.Equal(0, state.ResyncDrops, "every byte should have landed in a packet");
    }

    private void TestPs2MouseButtonInjected()
    {
        if (!TryGetPs2MouseState(out _))
        {
            return;
        }

        int reportsBefore = _pointerConsumer.RelativeCount;

        TR.RequestHost(Ps2MouseButtonDownRequest);

        Assert.True(WaitUntil(() => _pointerConsumer.RelativeCount > reportsBefore && (_pointerConsumer.LastButtons & PointerButtons.Left) != 0), "the button press should reach the pointer consumer");
        Assert.True(_pointerConsumer.LastButtons == PointerButtons.Left, "only the left button is down");

        reportsBefore = _pointerConsumer.RelativeCount;

        TR.RequestHost(Ps2MouseButtonUpRequest);

        Assert.True(WaitUntil(() => _pointerConsumer.RelativeCount > reportsBefore && _pointerConsumer.LastButtons == PointerButtons.None), "the button release should reach the pointer consumer");
    }

    // ==================== virtio-blk ====================
    //
    // The kit's Virtio bus kind over the cell's virtio-blk disk, under the
    // PCI transport or the virt machine's MMIO window: the library's driver
    // bound and its disk consumed by the ring's storage manager, the
    // geometry of the engine's image, a round trip that crosses the
    // per-request bound, a flush, and the completion mode the transport
    // gave the queue. The state is read through the node's binding.

    private void TestVirtioBlkBound()
    {
        if (!TryGetVirtioBlk(out string? path, out DeviceNodeInfo info))
        {
            return;
        }

        VirtioBlkState? state = FindDriverState<VirtioBlkState>(path);
        Assert.NotNull(state, "the virtio-blk node should be bound by VirtioBlkDriver");
        if (state is null)
        {
            return;
        }

        Assert.True(info.State == DeviceNodeState.Bound, "the virtio-blk node should be bound");
        Assert.True(info.DriverName == nameof(VirtioBlkDriver), "VirtioBlkDriver should hold the virtio-blk node");
        Assert.Equal(1, info.PublishedDeviceCount, "the binding should hold one published disk");
        Assert.True(info.BusName == VirtioBusName, "the node should be on the virtio bus");

        int index = FindBlockDeviceIndex(path);
        Assert.True(index >= 0, "the disk should be in the published list under its node");
        if (DriverInfo.TryGetDevice(index, out PublishedDeviceInfo device))
        {
            Assert.True(device.IsConsumed, "the storage manager should have consumed the disk");
            Assert.False(device.IsWithdrawn, "a published disk is not withdrawn");
            Assert.True(device.DriverName == nameof(VirtioBlkDriver), "the published device should name its driver");
            Assert.True(device.Name == state.Name, "the published device should carry the disk's name");
        }

        Assert.True(state.Name.StartsWith(VirtioBlkNamePrefix, StringComparison.Ordinal), "the disk should be named vblk<n>");
        Assert.True(HoldsDevice(StorageManager.Devices, state), "the storage manager should list the disk");
        Assert.Equal(0u, state.Index, "the cell's one disk takes index 0");
    }

    private void TestVirtioBlkTransportMatchesCell()
    {
        if (!TryGetVirtioBlk(out string? path, out DeviceNodeInfo info))
        {
            return;
        }

        VirtioBlkState? state = FindDriverState<VirtioBlkState>(path);
        Assert.NotNull(state, "the virtio-blk node should be bound by VirtioBlkDriver");
        if (state is null)
        {
            return;
        }

        bool pci = FindNodePathOnBus(PciBusName, TransitionalBlkFunctionPrefix) is not null || FindNodePathOnBus(PciBusName, ModernBlkFunctionPrefix) is not null;
        string expectedPrefix = pci ? VirtioPciPathPrefix : VirtioMmioPathPrefix;
        string expectedTransport = pci ? nameof(VirtioPciTransportDriver) : nameof(VirtioMmioTransportDriver);
        Assert.True(path.StartsWith(expectedPrefix, StringComparison.Ordinal), "the node's path should name the transport the cell attaches the disk on");

        string? parentPath = info.ParentPath;
        Assert.NotNull(parentPath, "the virtio node should have its transport's node as parent");
        if (parentPath is null)
        {
            return;
        }

        Assert.True(TryFindNode(parentPath, out DeviceNodeInfo parent), "the transport's node should be in the tree");
        Assert.True(parent.State == DeviceNodeState.Bound, "the transport's node should be bound");
        Assert.True(parent.DriverName == expectedTransport, "the transport driver should match the cell's bus");
        Assert.Equal(1, parent.ChildCount, "the transport publishes one virtio node");
    }

    private void TestVirtioBlkCapacityMatchesImage()
    {
        if (!TryGetVirtioBlk(out string? path, out _))
        {
            return;
        }

        VirtioBlkState? state = FindDriverState<VirtioBlkState>(path);
        Assert.NotNull(state, "the virtio-blk node should be bound by VirtioBlkDriver");
        if (state is null)
        {
            return;
        }

        Assert.Equal<ulong>(VirtioBlkSectorBytes, state.BlockSize, "the engine's image has 512-byte blocks");
        Assert.Equal<ulong>((ulong)VirtioBlkImageBytes / VirtioBlkSectorBytes, state.BlockCount, "the capacity should be the engine's 256 MiB image");
        Assert.False(state.IsReadOnly, "the engine's image is writable");
        Assert.True(state.MaxTransferBytes >= (int)state.BlockSize, "one request should move at least one block");
    }

    // The span crosses the per-request bound, so the chunking runs; both
    // ranges are put back as they were.
    private void TestVirtioBlkReadWriteRoundTrip()
    {
        if (!TryGetVirtioBlk(out string? path, out _))
        {
            return;
        }

        VirtioBlkState? state = FindDriverState<VirtioBlkState>(path);
        Assert.NotNull(state, "the virtio-blk node should be bound by VirtioBlkDriver");
        if (state is null)
        {
            return;
        }

        int blockBytes = (int)state.BlockSize;
        int spanBytes = VirtioBlkSpanBlocks * blockBytes;
        ulong spanLba = VirtioBlkProbeLba + 1;
        byte[] savedBlock = new byte[blockBytes];
        byte[] savedSpan = new byte[spanBytes];
        byte[] block = new byte[blockBytes];
        byte[] span = new byte[spanBytes];
        byte[] readBlock = new byte[blockBytes];
        byte[] readSpan = new byte[spanBytes];
        for (int i = 0; i < blockBytes; i++)
        {
            block[i] = (byte)(i * 7 + 3);
        }

        for (int i = 0; i < spanBytes; i++)
        {
            span[i] = (byte)(i * 13 + 5);
        }

        try
        {
            state.ReadBlock(VirtioBlkProbeLba, 1, savedBlock);
            state.ReadBlock(spanLba, VirtioBlkSpanBlocks, savedSpan);

            state.WriteBlock(VirtioBlkProbeLba, 1, block);
            state.WriteBlock(spanLba, VirtioBlkSpanBlocks, span);
            state.ReadBlock(VirtioBlkProbeLba, 1, readBlock);
            state.ReadBlock(spanLba, VirtioBlkSpanBlocks, readSpan);
            Assert.Equal(block, readBlock, "the single block should read back as written");
            Assert.Equal(span, readSpan, "the span should read back as written across the per-request bound");

            state.WriteBlock(VirtioBlkProbeLba, 1, savedBlock);
            state.WriteBlock(spanLba, VirtioBlkSpanBlocks, savedSpan);
            state.ReadBlock(VirtioBlkProbeLba, 1, readBlock);
            state.ReadBlock(spanLba, VirtioBlkSpanBlocks, readSpan);
            Assert.Equal(savedBlock, readBlock, "the single block should read back as restored");
            Assert.Equal(savedSpan, readSpan, "the span should read back as restored");
        }
        catch (Exception exception)
        {
            Assert.Fail("the round trip threw: " + exception.Message);
        }
    }

    private void TestVirtioBlkFlushCompletes()
    {
        if (!TryGetVirtioBlk(out string? path, out _))
        {
            return;
        }

        VirtioBlkState? state = FindDriverState<VirtioBlkState>(path);
        Assert.NotNull(state, "the virtio-blk node should be bound by VirtioBlkDriver");
        if (state is null)
        {
            return;
        }

        int before = state.RequestsCompleted;
        try
        {
            state.Flush();
        }
        catch (Exception exception)
        {
            Assert.Fail("the flush threw: " + exception.Message);
            return;
        }

        Assert.Equal(before + (state.FlushNegotiated ? 1 : 0), state.RequestsCompleted, "a negotiated flush is one request, an unnegotiated one none");
        Log.WriteString(state.FlushNegotiated ? "[DriversTests] virtio-blk flush negotiated\n" : "[DriversTests] virtio-blk flush not negotiated\n");
    }

    // The group's cells carry no GIC modifier, so the mode is not pinned.
    private void TestVirtioBlkInterruptOrPolled()
    {
        if (!TryGetVirtioBlk(out string? path, out _))
        {
            return;
        }

        VirtioBlkState? state = FindDriverState<VirtioBlkState>(path);
        Assert.NotNull(state, "the virtio-blk node should be bound by VirtioBlkDriver");
        if (state is null)
        {
            return;
        }

        Assert.True(state.HasInterrupt != state.IsPolling, "the driver takes its completions from the queue interrupt or polls, never both or neither");
        if (state.HasInterrupt)
        {
            Assert.True(state.InterruptCount >= 1, "the round trip should have raised the queue interrupt");
        }

        Log.WriteString(state.HasInterrupt ? "[DriversTests] virtio-blk interrupt\n" : "[DriversTests] virtio-blk polling\n");
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

    /// <summary>
    /// Finds the path of the first function node describing an xHCI
    /// controller, whatever its state: the USB tests decide from the
    /// hardware's presence, as the E1000E tests do, so a probe that failed
    /// shows up as a failed test rather than a skip.
    /// </summary>
    /// <returns>The node's path, or null when no such function is on the bus.</returns>
    private static string? FindXhciPath()
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == PciBusName
                && info.Description.Contains(XhciClassDescription, StringComparison.Ordinal))
            {
                return info.Path;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the path of the first usb node describing a HID boot keyboard
    /// interface that is not retracted; compared ordinally, since the kernel
    /// runtime does not plug the culture-sensitive comparisons.
    /// </summary>
    /// <returns>The node's path, or null when no live keyboard interface is in the tree.</returns>
    private static string? FindUsbKeyboardPath()
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == UsbBusName
                && info.Description.EndsWith(UsbKeyboardDescriptionSuffix, StringComparison.Ordinal)
                && info.State != DeviceNodeState.Retracted)
            {
                return info.Path;
            }
        }

        return null;
    }

    /// <summary>Hands back the controller's path and a fresh snapshot of its node, or fails the test when BeforeRun found none.</summary>
    /// <param name="path">The node's path.</param>
    /// <param name="info">The node's snapshot.</param>
    /// <returns>True when the node is in the tree.</returns>
    private bool TryGetXhci([NotNullWhen(true)] out string? path, out DeviceNodeInfo info)
    {
        path = _xhciPath;
        if (path is null || !TryFindNode(path, out info))
        {
            Assert.Fail("the xHCI controller's node was not found by BeforeRun");
            info = default;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Finds the path of the 8042's platform node: the node on the platform
    /// bus whose description (the identity's compatible strings) names
    /// pnp0303, compared ordinally, whatever its state.
    /// </summary>
    /// <returns>The node's path, or null when the machine description published none.</returns>
    private static string? FindI8042Path()
    {
        int count = DriverInfo.NodeCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo info)
                && info.BusName == PlatformBusName
                && info.Description.Contains(I8042Compatible, StringComparison.Ordinal))
            {
                return info.Path;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the path of the first virtio node describing a block device,
    /// whatever its state: the virtio-blk tests decide from the hardware's
    /// presence, as the E1000E tests do, so a probe that failed shows up as
    /// a failed test rather than a skip.
    /// </summary>
    /// <returns>The node's path, or null when no virtio-blk device is in the tree.</returns>
    private static string? FindVirtioBlkPath() => FindNodePathOnBus(VirtioBusName, VirtioBlkDescriptionPrefix);

    /// <summary>Finds the path of the first node on <paramref name="busName"/> whose description starts with <paramref name="descriptionPrefix"/>, compared ordinally, whatever its state.</summary>
    /// <param name="busName">The bus the node is on.</param>
    /// <param name="descriptionPrefix">The start of the node's description.</param>
    /// <returns>The node's path, or null when no such node is in the tree.</returns>
    private static string? FindNodePathOnBus(string busName, string descriptionPrefix)
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

    /// <summary>Hands back the virtio-blk node's path and a fresh snapshot of it, or fails the test when BeforeRun found none.</summary>
    /// <param name="path">The node's path.</param>
    /// <param name="info">The node's snapshot.</param>
    /// <returns>True when the node is in the tree.</returns>
    private bool TryGetVirtioBlk([NotNullWhen(true)] out string? path, out DeviceNodeInfo info)
    {
        path = _virtioBlkPath;
        if (path is null || !TryFindNode(path, out info))
        {
            Assert.Fail("the virtio-blk node was not found by BeforeRun");
            info = default;
            return false;
        }

        return true;
    }

    /// <summary>Hands back the 8042 node's path and a fresh snapshot of it, or fails the test when BeforeRun found none.</summary>
    /// <param name="path">The node's path.</param>
    /// <param name="info">The node's snapshot.</param>
    /// <returns>True when the node is in the tree.</returns>
    private bool TryGetI8042([NotNullWhen(true)] out string? path, out DeviceNodeInfo info)
    {
        path = _i8042Path;
        if (path is null || !TryFindNode(path, out info))
        {
            Assert.Fail("the 8042 node was not found by BeforeRun");
            info = default;
            return false;
        }

        return true;
    }

    /// <summary>Hands back the shipped keyboard driver's state on the keyboard port, or fails the test.</summary>
    /// <param name="state">The driver state when found.</param>
    /// <returns>True when the port is bound by the shipped driver.</returns>
    private static bool TryGetPs2KeyboardState([NotNullWhen(true)] out Ps2KeyboardState? state)
    {
        state = FindDriverState<Ps2KeyboardState>(Ps2KeyboardPath);
        Assert.NotNull(state, "the keyboard port should be bound by Ps2KeyboardDriver");
        return state is not null;
    }

    /// <summary>Hands back the shipped mouse driver's state on the auxiliary port, or fails the test.</summary>
    /// <param name="state">The driver state when found.</param>
    /// <returns>True when the port is bound by the shipped driver.</returns>
    private static bool TryGetPs2MouseState([NotNullWhen(true)] out Ps2MouseState? state)
    {
        state = FindDriverState<Ps2MouseState>(Ps2MousePath);
        Assert.NotNull(state, "the auxiliary port should be bound by Ps2MouseDriver");
        return state is not null;
    }

    /// <summary>Hands back the keyboard interface's path and node as the bound test recorded them, or fails the test when it did not.</summary>
    /// <param name="path">The node's path.</param>
    /// <param name="node">The node itself.</param>
    /// <returns>True when both were recorded.</returns>
    private bool TryGetUsbKeyboard([NotNullWhen(true)] out string? path, [NotNullWhen(true)] out DeviceNode? node)
    {
        path = _usbKeyboardPath;
        node = _usbKeyboardNode;
        if (path is null || node is null)
        {
            Assert.Fail("the USB keyboard was not set up by the bound test");
            return false;
        }

        return true;
    }

    /// <summary>Hands back the shipped keyboard driver's state on the keyboard interface recorded last, or fails the test.</summary>
    /// <param name="state">The driver state when found.</param>
    /// <returns>True when the interface is bound by the shipped driver.</returns>
    private bool TryGetUsbKeyboardState([NotNullWhen(true)] out UsbKeyboardState? state)
    {
        state = null;
        if (!TryGetUsbKeyboard(out string? path, out _))
        {
            return false;
        }

        state = FindDriverState<UsbKeyboardState>(path);
        Assert.NotNull(state, "the keyboard interface should be bound by UsbKeyboardDriver");
        return state is not null;
    }

    /// <summary>Finds the node with the given path in the tree itself, through the HAL grant.</summary>
    /// <param name="path">The node's path.</param>
    /// <returns>The node, or null when no node has that path.</returns>
    private static DeviceNode? FindNode(string path)
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode node = nodes[i];
            if (node.Path == path)
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the driver state of the node with the given path in the tree
    /// itself, through the HAL grant: the counters the USB tests read are on
    /// the state, which no diagnostic snapshot carries.
    /// </summary>
    /// <typeparam name="TState">The driver state's class.</typeparam>
    /// <param name="path">The node's path.</param>
    /// <returns>The state, or null when the node is not bound by a driver keeping that state.</returns>
    private static TState? FindDriverState<TState>(string path) where TState : class => FindNode(path)?.Binding?.DriverState as TState;

    /// <summary>
    /// Waits for a hot-plug to show up, sleeping so the hot-plug thread and
    /// the kit worker get to run, for at most the hot-plug budget.
    /// </summary>
    /// <param name="condition">What the test waits for.</param>
    /// <returns>True when the condition held within the budget.</returns>
    private static bool WaitUntil(Func<bool> condition)
    {
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * HotPlugTimeoutMilliseconds / TR.MillisecondsPerSecond;
        while (!condition())
        {
            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            SysThread.Sleep(HotPlugPollMilliseconds);
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
