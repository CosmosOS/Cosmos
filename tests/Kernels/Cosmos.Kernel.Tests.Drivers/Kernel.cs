// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Build.API.Enum;
using Cosmos.Kernel.HAL;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Drivers.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Mouse;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.DHCP;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using SysThread = System.Threading.Thread;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The driver kit. Each cell attaches hardware that no built-in driver
/// claims: the PCI profiles (edu, rtl8139, e1000e-arm64, and nvme, which
/// this kernel leaves free by building without Storage) put one function on
/// the bus, and the usb-hid profile puts a HID boot mouse and a tablet
/// behind an xHCI controller, with a keyboard the built-in USB keyboard
/// driver takes. The kernel registers its test drivers, PCI
/// and USB, from its constructor, the driver pass in Global.StartKernel
/// offers them the free functions and interfaces, and the cells check what
/// came of it: the registration rules, the ranking, the teardown of failed
/// attempts, the interrupts, work items and events the kit hands out, the
/// mouse and network link drivers publish, on edu a driver that drives the
/// device's registers, DMA engine and polled interrupt, on the RTL8139 a NIC
/// driver whose link gets a DHCP lease through the kernel's network stack,
/// on NVMe a failed attempt with MSI-X followed by a driver that gets it
/// again and takes a real command's completion interrupt, and on USB a boot
/// mouse driver that binds after a device match declined and whose reports
/// reach it only once bound, and a tablet driver that opens its endpoint and
/// fails, which leaves the tablet to no other driver. The USB cells then
/// have the test engine move, pull out and plug back in the USB devices:
/// QEMU's pointer movement reaches the mouse manager through the boot mouse
/// driver, pulling the mouse out runs the unplug teardown and the driver's
/// Remove, plugging it back in binds a new driver on the hot-plug thread,
/// a tablet plugged back in goes to a driver whose network link leaves the
/// network manager when the tablet is pulled out again, and the keyboard
/// plugged back in goes to the built-in driver again.
///
/// One kernel binary serves every cell of an architecture, so the hardware a
/// cell presents is read from the profile name the engine puts on the kernel
/// command line. The gates key off that name, not off what was found: a
/// device that failed to enumerate must fail its cell, not skip it.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 80;

    // Base profile names, spelled as in tests/profiles.json. A cell name is
    // one of them followed by "+modifier" for each modifier composed onto it.
    private const string EduProfile = "edu";
    private const string Rtl8139Profile = "rtl8139";
    private const string E1000EProfile = "e1000e-arm64";
    private const string UsbHidProfile = "usb-hid";
    private const string NvmeProfile = "nvme";

    /// <summary>Separator between a cell's base profile and each modifier composed onto it.</summary>
    private const char ModifierSeparator = '+';

    /// <summary>The modifier that gives the arm64 virt machine a GICv3, whose ITS routes MSI-X.</summary>
    private const string Gicv3Modifier = "+gicv3";

    /// <summary>Reason surfaced for the PCI cells on a cell whose profile attaches USB hardware.</summary>
    private const string SkipNotPciCell = "this cell attaches no unclaimed PCI function";

    /// <summary>Reason surfaced for the USB cells on a cell whose profile attaches a PCI function.</summary>
    private const string SkipNotUsbCell = "this cell attaches no USB device";

    /// <summary>Reason surfaced for the cells a registered driver binds or probes on edu.</summary>
    private const string SkipNotEduCell = "this cell attaches no edu device";

    /// <summary>Reason surfaced for the class-ranking cell on a cell without an unclaimed NIC.</summary>
    private const string SkipNotNicCell = "this cell attaches no unclaimed Ethernet function";

    /// <summary>Reason surfaced for the NVMe cells on a cell without an NVMe controller.</summary>
    private const string SkipNotNvmeCell = "this cell attaches no NVMe controller";

    /// <summary>Reason surfaced for the fall-through ranking cell, which needs a function several drivers match by device.</summary>
    private const string SkipNotEduOrNvmeCell = "this cell attaches neither edu nor an NVMe controller";

    /// <summary>Reason surfaced for the 82574L interrupt cell elsewhere.</summary>
    private const string SkipNotE1000ECell = "this cell attaches no unclaimed 82574L";

    /// <summary>Reason surfaced for the RTL8139 cells elsewhere.</summary>
    private const string SkipNotRtl8139Cell = "this cell attaches no RTL8139";

    // QEMU's edu device. QEMU gives it class code 0x00ff (PCI_CLASS_OTHERS):
    // base class 0x00, which predates class codes, and subclass 0xff.
    private const byte UnclassifiedClassCode = 0x00;
    private const byte OtherSubclass = 0xFF;

    // Intel 82574L, the function QEMU's e1000e model presents.
    private const ushort IntelVendorId = 0x8086;
    private const ushort I82574LDeviceId = 0x10D3;

    private const byte NetworkClassCode = 0x02;
    private const byte EthernetSubclass = 0x00;
    private const byte EthernetProgIf = 0x00;

    // An NVMe controller: mass storage, non-volatile memory, NVM Express.
    private const byte MassStorageClassCode = 0x01;
    private const byte NonVolatileMemorySubclass = 0x08;
    private const byte NvmeProgIf = 0x02;

    // An xHCI controller: serial bus controller, USB, programming interface 0x30.
    private const byte SerialBusClassCode = 0x0C;
    private const byte UsbSubclass = 0x03;
    private const byte XhciProgIf = 0x30;

    // Owner names, spelled out rather than read from PciOwner so a renamed
    // constant is caught instead of compared against itself.
    private const string XhciOwner = "xhci";
    private const string GopOwner = "gop";

    // A HID interface that declares the boot mouse protocol (HID 1.11
    // §4.2-§4.3), as QEMU's usb-mouse does. QEMU's usb-tablet declares
    // neither a boot subclass nor a protocol.
    private const byte HidClass = UsbBootMouseDriver.HidClass;
    private const byte BootInterfaceSubclass = UsbBootMouseDriver.BootSubclass;
    private const byte MouseProtocol = UsbBootMouseDriver.MouseProtocol;
    private const byte TabletSubclass = UsbTabletFailingDriver.NoSubclass;
    private const byte TabletProtocol = UsbTabletFailingDriver.NoProtocol;

    /// <summary>bInterfaceProtocol of a boot keyboard (HID 1.11 §4.3), which QEMU's usb-kbd declares and the built-in keyboard driver takes.</summary>
    private const byte KeyboardProtocol = 0x01;

    // Entries of the usb-hid profile's "usb" list, in tests/profiles.json,
    // which the host requests name.
    private const int MouseUsbIndex = 0;
    private const int TabletUsbIndex = 1;
    private const int KeyboardUsbIndex = 2;

    // The built-in USB class drivers' names, spelled out for the same reason
    // as the owner names above.
    private const string HubName = "hub";
    private const string UsbKeyboardName = "HID boot keyboard";
    private const string MassStorageName = "mass storage";

    // The suite's registrations besides the edu driver and its two
    // edu-matching siblings. The class ones register before the ones that
    // must beat them, so only the kind of match can put them behind.
    private const string EduClassName = "edu-class";
    private const string NicClassName = "nic-class";
    private const string NicClassWithInterfaceName = "nic-class-progif";
    private const string LateName = "late";
    private const string LateUsbName = "late-usb";
    private const string InvalidName = "invalid";

    // The suite's USB registrations besides the boot mouse, the tablet and
    // the device-match drivers. The class-only match registers first and
    // would bind any HID interface it were offered, so only its rank keeps
    // it from the mouse and the tablet; the declining boot mouse match ties
    // with the boot mouse driver and registers before it.
    private const string UsbHidClassName = "usb-hid-class";
    private const string UsbMouseDeclinesName = "usb-mouse-declines";

    /// <summary>Registrations the constructor expects Register to accept.</summary>
    private const int AcceptedRegistrationCount = 10;

    /// <summary>USB registrations the constructor expects Register to accept.</summary>
    private const int AcceptedUsbRegistrationCount = 6;

    /// <summary>
    /// Order in which the mouse interface should have been offered: the
    /// device match, then the two boot mouse matches in registration order,
    /// up to the boot mouse driver, which binds it. Never the class match.
    /// </summary>
    private const string ExpectedMouseProbeOrder = $"{UsbHidDeviceDriver.Name},{UsbMouseDeclinesName},{UsbBootMouseDriver.Name}";

    /// <summary>
    /// Order in which the tablet interface should have been offered: the
    /// device match, which opens nothing and declines, then the tablet
    /// driver, which opens its endpoint and fails, and no one after it.
    /// </summary>
    private const string ExpectedTabletProbeOrder = $"{UsbHidDeviceDriver.Name},{UsbTabletFailingDriver.Name}";

    /// <summary>
    /// Order in which a tablet plugged back in should be offered: the device
    /// match and the tablet driver, both declining having opened nothing,
    /// then the link driver's class and subclass match, which binds it.
    /// </summary>
    private const string ExpectedTabletReplugProbeOrder = $"{ExpectedTabletProbeOrder},{UsbTabletLinkDriver.Name}";

    /// <summary>Order in which the pass should have probed the edu drivers: device matches in registration order, the class match never.</summary>
    private const string ExpectedEduProbeOrder = $"{ThrowingDriver.Name},{ReentrantDriver.Name},{EduDriver.Name}";

    /// <summary>
    /// Order in which the pass should have probed the 82574L: the class
    /// match with a programming interface first, then the class matches in
    /// registration order, up to the interrupt driver, which binds it.
    /// </summary>
    private const string ExpectedE1000EProbeOrder = $"{NicClassWithInterfaceName},{NicClassName},{E1000EInterruptDriver.Name}";

    /// <summary>
    /// Order in which the pass should have probed the RTL8139: as the
    /// 82574L, with the interrupt driver declining it, then the RTL8139
    /// driver, registered last, which binds it.
    /// </summary>
    private const string ExpectedRtl8139ProbeOrder = $"{ExpectedE1000EProbeOrder},{Rtl8139Driver.Name}";

    /// <summary>Order in which the pass should have probed the NVMe controller: the device match, registered last, first.</summary>
    private const string ExpectedNvmeProbeOrder = $"{FailingNvmeDriver.Name},{NvmeDriver.Name}";

    /// <summary>Command register bit 10, which the kit sets on every function it offers a driver.</summary>
    private const ushort InterruptDisableBit = 0x0400;

    /// <summary>Command register bit 2, which lets a function master the bus and send MSI-X messages.</summary>
    private const ushort BusMasterBit = 0x0004;

    /// <summary>Config offset of the Command register.</summary>
    private const ushort CommandOffset = 0x04;

    /// <summary>
    /// Longest wait for something an interrupt or the driver-work thread
    /// does, in milliseconds of Stopwatch time: dozens of polling periods on
    /// x64, where the timer ticks about every 55 ms.
    /// </summary>
    private const long InterruptWaitMilliseconds = 2000;

    /// <summary>How long the lock cell holds an IrqSafeLock: more than two x64 polling periods.</summary>
    private const long LockHoldMilliseconds = 150;

    private const long MillisecondsPerSecond = 1000;

    /// <summary>
    /// Pages the heap may take between the failed NVMe attempt's Probe and
    /// the next one: well below the <see cref="FailingNvmeDriver.LeakCheckPages"/>
    /// of DMA memory that attempt allocated, so a leak of it shows.
    /// </summary>
    private const long PageLeakSlack = 32;

    // Values raised through edu's interrupt raise register by the cells,
    // one bit each so the handler's acknowledgements tell them apart from
    // the raise the edu driver makes during Probe.
    private const uint ServiceRaise = 0x2;
    private const uint WorkRaise = 0x4;

    /// <summary>Capability ID of MSI, which edu has and which the config-space cell looks for.</summary>
    private const byte MsiCapabilityId = 0x05;

    // Where the mouse cells put the pointer before a report, far enough from
    // every screen edge that no report is clamped.
    private const int PointerStartX = 100;
    private const int PointerStartY = 100;

    /// <summary>First config offset the kit lets a driver write.</summary>
    private const ushort FirstDriverConfigOffset = 0x40;

    // What the USB mouse cell reports through the boot mouse driver's
    // published mouse.
    private const int UsbMouseDeltaX = 6;
    private const int UsbMouseDeltaY = 9;

    /// <summary>A boot mouse report: buttons, X and Y, then an optional wheel.</summary>
    private const int MinimumBootReportLength = 3;

    /// <summary>
    /// How long the teardown cell watches the failed tablet attempt's
    /// handler while the tablet sends a report every 4 ms: dozens of them,
    /// and several polls of a controller whose events nothing interrupts on.
    /// </summary>
    private const long TabletReportWatchMilliseconds = 600;

    /// <summary>
    /// Longest wait for what the USB hot-plug thread does after a host
    /// request: a device pulled out and its binding torn down, or plugged in,
    /// enumerated and offered, or a report QEMU sent. The Storage suite's
    /// bound for a stick plugged back in, which takes about 1.5 s on x64: a
    /// GICv2 cell, whose xHCI the hot-plug thread polls every 250 ms, has
    /// time, and the wait stays inside the test engine's 10 s window
    /// without a protocol message.
    /// </summary>
    private const long HotPlugWaitMilliseconds = 8000;

    // What the pointer cells ask QEMU to move the USB mouse by, and the
    // buttons the request holds: the bits of a boot mouse report's first
    // byte, 1 for the left button.
    private const int PointerMoveX = 12;
    private const int PointerMoveY = 7;
    private const int LeftButtonBit = 1;
    private const int NoButtons = 0;

    /// <summary>An Ethernet frame's minimum length without its CRC, which the link cells send.</summary>
    private const int MinimumFrameLength = 60;

    /// <summary>What the path of every USB interface in the device list starts with.</summary>
    private const string UsbPathPrefix = "usb/";

    /// <summary>
    /// Times each withdrawal race cell hands the CPU to its thread and acts
    /// when it gets it back. The thread is preempted wherever the timer
    /// finds it, so each round is one more chance for the race to show.
    /// </summary>
    private const int LinkRaceRounds = 100;

    /// <summary>How long a withdrawal race cell waits for its thread to return once told to stop.</summary>
    private const int RaceThreadJoinMilliseconds = 2000;

    // The hardware this cell's profile attaches, chosen in the constructor
    // from the profile name.
    private static bool s_isPciCell;
    private static bool s_isEduCell;
    private static bool s_isNicCell;
    private static bool s_isE1000ECell;
    private static bool s_isRtl8139Cell;
    private static bool s_isNvmeCell;
    private static bool s_isUsbCell;

    /// <summary>
    /// True where the platform routes MSI-X: x64, and arm64 with a GICv3,
    /// whose ITS does. Elsewhere the kit must poll.
    /// </summary>
    private static bool s_expectMsiX;

    /// <summary>The scheduler's ID for the boot thread, which runs the cells; work items must run elsewhere.</summary>
    private static uint s_bootThreadId;
    private static ushort s_expectedVendorId;
    private static ushort s_expectedDeviceId;
    private static byte s_expectedClassCode;
    private static byte s_expectedSubclass;

    // The profile's PCI function as the constructor found it, before the
    // driver pass ran.
    private static bool s_functionFoundBeforePass;
    private static string? s_ownerBeforePass;
    private static PciCommand s_commandBeforePass;

    /// <summary>
    /// Network devices registered when the kernel was constructed: the
    /// built-in drivers' ones. A published link joins after them.
    /// </summary>
    private static int s_networkDevicesBeforePass;

    // What Register answered in the constructor.
    private static int s_acceptedRegistrations;
    private static bool s_duplicateNameAccepted;
    private static bool s_builtInNameAccepted;
    private static bool s_bootDisplayNameAccepted;
    private static int s_acceptedUsbRegistrations;
    private static bool s_usbDuplicateNameAccepted;
    private static bool s_usbNamedAfterPciAccepted;
    private static bool s_pciNamedAfterUsbAccepted;
    private static bool s_usbHubNameAccepted;
    private static bool s_usbKeyboardNameAccepted;
    private static bool s_usbMassStorageNameAccepted;
    private static bool s_usbNamedAfterPciBuiltInAccepted;
    private static bool s_pciNamedAfterUsbBuiltInAccepted;

    // The USB interfaces of the usb-hid profile as the constructor found
    // them, before the driver pass ran.
    private static bool s_mouseFoundBeforePass;
    private static string? s_mouseOwnerBeforePass;
    private static bool s_tabletFoundBeforePass;
    private static string? s_tabletOwnerBeforePass;
    private static string? s_keyboardOwnerBeforePass;
    private static string? s_keyboardPathBeforePass;

    // The mouse's binding as the unplug cell captured it before pulling the
    // mouse out, for the cells after it.
    private static UsbDeviceContext? s_unpluggedMouseContext;
    private static MouseReporter? s_unpluggedMouse;
    private static DeviceWorkItem? s_unpluggedWorkItem;
    private static DeviceEvent? s_unpluggedEvent;
    private static UsbInterface? s_unpluggedMouseInterface;

    // The network manager as it stood before the tablet's link joined it.
    private static int s_networkDevicesBeforeLink;
    private static NetworkAdapter s_primaryBeforeLink;
    private static MACAddress? s_primaryAddressBeforeLink;

    // The first link, as the cell that pulls the tablet out captured it:
    // the handle to it, and the device behind it.
    private static NetworkAdapter s_staleLinkAdapter;
    private static PublishedNetworkDevice? s_staleLinkDevice;

    // Shared by the withdrawal race cells and the thread each one starts:
    // what the thread works on, what it saw, and when to stop.
    private static volatile bool s_raceStop;
    private static volatile PublishedNetworkDevice? s_raceLink;
    private static PublishedNetworkDevice? s_sendingLink;
    private static int s_raceTransmits;
    private static int s_transmitsAfterWithdraw;
    private static PublishedNetworkDevice? s_deliveringLink;
    private static int s_raceFrames;
    private static int s_framesAfterWithdraw;
    private static Address4? s_routeDestination;
    private static Address? s_routeSource;
    private static int s_routeLookups;
    private static int s_routeLookupFailures;
    private static string? s_routeLookupError;

    /// <summary>True in the x64 build, whose local APIC routes MSI-X on every cell and binds a dynamic vector per entry.</summary>
    private static bool IsX64 => PlatformHAL.Architecture == PlatformArchitecture.X64;

    /// <summary>
    /// Runs before Global.StartKernel, so before the driver pass: records the
    /// profile's PCI function as the built-in drivers left it, then registers
    /// the suite's drivers, the only point where registration is open.
    /// </summary>
    public Kernel()
    {
        SelectCellHardware();
        CaptureFunctionBeforePass();
        CaptureUsbInterfacesBeforePass();
        s_networkDevicesBeforePass = NetworkManager.DeviceCount;
        RegisterTestDrivers();
        RegisterUsbTestDrivers();
    }

    protected override void BeforeRun()
    {
        Log.WriteString("[Drivers] BeforeRun() reached!\n");

        KernelState.TryGetCurrentThread(out s_bootThreadId, out _);

        TR.Start("Driver Kit Tests", expectedTests: ExpectedTestCount);

        // ==================== Profile ====================
        TR.Run("Profile_Recognized", TestProfile_Recognized);

        // ==================== PCI ====================
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionEnumeratedOnce",    TestPci_ProfileFunctionEnumeratedOnce,    SkipNotPciCell);
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionClassMatches",      TestPci_ProfileFunctionClassMatches,      SkipNotPciCell);
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionUnownedBeforePass", TestPci_ProfileFunctionUnownedBeforePass, SkipNotPciCell);
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionOwnerAfterPass",    TestPci_ProfileFunctionOwnerAfterPass,    SkipNotPciCell);

        // ==================== USB ====================
        TR.RunIf(s_isUsbCell, "Usb_XhciOwnedByXhci",           TestUsb_XhciOwnedByXhci,           SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_MouseEnumeratedOnce",       TestUsb_MouseEnumeratedOnce,       SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_TabletEnumeratedOnce",      TestUsb_TabletEnumeratedOnce,      SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_MouseInterfaceBoundByPass", TestUsb_MouseInterfaceBoundByPass, SkipNotUsbCell);

        // ==================== Registration ====================
        TR.Run("Register_TakenNameRefused",           TestRegister_TakenNameRefused);
        TR.Run("Register_InvalidRegistrationThrows",  TestRegister_InvalidRegistrationThrows);
        TR.Run("Register_AfterPassThrows",            TestRegister_AfterPassThrows);
        TR.RunIf(s_isEduCell, "Register_FromDriverCallbackThrows", TestRegister_FromDriverCallbackThrows, SkipNotEduCell);
        TR.Run("Register_UsbTakenNameRefused",          TestRegister_UsbTakenNameRefused);
        TR.Run("Register_UsbInvalidRegistrationThrows", TestRegister_UsbInvalidRegistrationThrows);
        TR.Run("Register_UsbAfterPassThrows",           TestRegister_UsbAfterPassThrows);
        TR.RunIf(s_isUsbCell, "Register_FromUsbDriverCallbackThrows", TestRegister_FromUsbDriverCallbackThrows, SkipNotUsbCell);

        // ==================== Ranking ====================
        TR.RunIf(s_isEduCell, "Ranking_DeviceMatchBeatsClassMatch",       TestRanking_DeviceMatchBeatsClassMatch,       SkipNotEduCell);
        TR.RunIf(s_isEduCell || s_isNvmeCell, "Ranking_FailedAndDeclinedFallThrough", TestRanking_FailedAndDeclinedFallThrough, SkipNotEduOrNvmeCell);
        TR.RunIf(s_isNicCell, "Ranking_ClassWithInterfaceBeatsClass",     TestRanking_ClassWithInterfaceBeatsClass,     SkipNotNicCell);
        TR.RunIf(s_isUsbCell, "UsbRanking_DeviceMatchFallsThroughToBootMouse", TestUsbRanking_DeviceMatchFallsThroughToBootMouse, SkipNotUsbCell);

        // ==================== Teardown ====================
        TR.RunIf(s_isPciCell, "Teardown_RestoresCommandRegister",         TestTeardown_RestoresCommandRegister,         SkipNotPciCell);
        TR.RunIf(s_isEduCell, "Teardown_InvalidatesRegionAndBuffer",      TestTeardown_InvalidatesRegionAndBuffer,      SkipNotEduCell);

        // ==================== Context ====================
        TR.RunIf(s_isEduCell, "Context_ProbeOnlyMembersThrowAfterProbe",  TestContext_ProbeOnlyMembersThrowAfterProbe,  SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Context_WriteConfigSparesHeader",          TestContext_WriteConfigSparesHeader,          SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Function_ReadsConfigSpace",                TestFunction_ReadsConfigSpace,                SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Mmio_MapsWholeBar",                        TestMmio_MapsWholeBar,                        SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Mmio_RefusesBadAccesses",                  TestMmio_RefusesBadAccesses,                  SkipNotEduCell);

        // ==================== edu ====================
        TR.RunIf(s_isEduCell, "Edu_Identification", TestEdu_Identification, SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Edu_Liveness",       TestEdu_Liveness,       SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Edu_Factorial",      TestEdu_Factorial,      SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Edu_Dma",            TestEdu_Dma,            SkipNotEduCell);

        // ==================== Interrupts ====================
        TR.RunIf(s_isEduCell,     "Interrupts_RequestOncePerAttempt",     TestInterrupts_RequestOncePerAttempt,     SkipNotEduCell);
        TR.RunIf(s_isEduCell,     "Interrupts_HandlerIdleUntilBound",     TestInterrupts_HandlerIdleUntilBound,     SkipNotEduCell);
        TR.RunIf(s_isEduCell,     "Interrupts_PolledHandlerServicesDevice", TestInterrupts_PolledHandlerServicesDevice, SkipNotEduCell);
        TR.RunIf(s_isE1000ECell,  "Interrupts_E1000EModeFollowsGic",      TestInterrupts_E1000EModeFollowsGic,      SkipNotE1000ECell);

        // ==================== Work items and events ====================
        TR.RunIf(s_isEduCell, "WorkItem_ScheduledInProbeRunsAfterBound",       TestWorkItem_ScheduledInProbeRunsAfterBound,       SkipNotEduCell);
        TR.RunIf(s_isEduCell, "WorkItem_ScheduledFromHandlerRunsOnDriverWork", TestWorkItem_ScheduledFromHandlerRunsOnDriverWork, SkipNotEduCell);
        TR.RunIf(s_isEduCell, "WorkItem_DroppedWithFailedAttempt",             TestWorkItem_DroppedWithFailedAttempt,             SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Event_WaitThrowsInProbe",                       TestEvent_WaitThrowsInProbe,                       SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Event_TornDownWaitReturnsFalse",                TestEvent_TornDownWaitReturnsFalse,                SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Lock_IrqSafeLockMasksInterrupts",               TestLock_IrqSafeLockMasksInterrupts,               SkipNotEduCell);

        // ==================== Publications ====================
        TR.RunIf(s_isEduCell, "Publish_MouseMovesPointer",        TestPublish_MouseMovesPointer,        SkipNotEduCell);
        TR.RunIf(s_isEduCell, "Publish_DroppedWithFailedAttempt", TestPublish_DroppedWithFailedAttempt, SkipNotEduCell);

        // Last among the edu cells: by now the edu handler has run on many
        // ticks, which the failed attempt's handler must not have.
        TR.RunIf(s_isEduCell, "Interrupts_TornDownHandlerNeverRuns",           TestInterrupts_TornDownHandlerNeverRuns,           SkipNotEduCell);

        // ==================== NVMe ====================
        TR.RunIf(s_isNvmeCell, "Nvme_FailedAttemptRequestedInterrupts", TestNvme_FailedAttemptRequestedInterrupts, SkipNotNvmeCell);
        TR.RunIf(s_isNvmeCell, "Nvme_TeardownReleasedInterrupts",       TestNvme_TeardownReleasedInterrupts,       SkipNotNvmeCell);
        TR.RunIf(s_isNvmeCell, "Nvme_RebindGetsInterruptsAgain",        TestNvme_RebindGetsInterruptsAgain,        SkipNotNvmeCell);
        TR.RunIf(s_isNvmeCell, "Nvme_CompletionInterruptAfterBound",    TestNvme_CompletionInterruptAfterBound,    SkipNotNvmeCell);

        // ==================== RTL8139 ====================
        TR.RunIf(s_isRtl8139Cell, "Rtl8139_LinkRegistered",   TestRtl8139_LinkRegistered,   SkipNotRtl8139Cell);
        TR.RunIf(s_isRtl8139Cell, "Rtl8139_PolledHandlerRuns", TestRtl8139_PolledHandlerRuns, SkipNotRtl8139Cell);
        TR.RunIf(s_isRtl8139Cell, "Rtl8139_DhcpLease",        TestRtl8139_DhcpLease,        SkipNotRtl8139Cell);

        // ==================== USB binding ====================
        TR.RunIf(s_isUsbCell, "UsbBind_BootMouseProbeSucceeded",           TestUsbBind_BootMouseProbeSucceeded,           SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbContext_ControlInReadsDeviceDescriptor", TestUsbContext_ControlInReadsDeviceDescriptor, SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbContext_ProbeOnlyMembersThrowAfterProbe", TestUsbContext_ProbeOnlyMembersThrowAfterProbe, SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbContext_OpenRefusesOtherEndpoints",      TestUsbContext_OpenRefusesOtherEndpoints,      SkipNotUsbCell);

        // Before the mouse cell: it stops the idle reports this one needs.
        TR.RunIf(s_isUsbCell, "UsbInterrupts_ReportsOnlyAfterBound",       TestUsbInterrupts_ReportsOnlyAfterBound,       SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbBind_PublishedMouseMovesPointer",        TestUsbBind_PublishedMouseMovesPointer,        SkipNotUsbCell);

        // ==================== USB no fall-through ====================
        TR.RunIf(s_isUsbCell, "UsbNoFallThrough_TabletOfferingEnds",       TestUsbNoFallThrough_TabletOfferingEnds,       SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbTeardown_FailedAttemptInvalidated",      TestUsbTeardown_FailedAttemptInvalidated,      SkipNotUsbCell);

        // ==================== USB hot-plug ====================
        // After every cell on the state the pass left: these move, pull out
        // and plug back in the usb-hid profile's devices, in this order.
        TR.RunIf(s_isUsbCell, "UsbHotPlug_PointerMoveReachesBootMouseDriver",   TestUsbHotPlug_PointerMoveReachesBootMouseDriver,   SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbUnplug_RemoveRunsOnceAfterWithdrawal",        TestUsbUnplug_RemoveRunsOnceAfterWithdrawal,        SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbUnplug_MouseLeavesMouseManager",              TestUsbUnplug_MouseLeavesMouseManager,              SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbUnplug_StaleContextAnswersDisconnected",      TestUsbUnplug_StaleContextAnswersDisconnected,      SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbUnplug_WorkItemsAndEventsCancelled",          TestUsbUnplug_WorkItemsAndEventsCancelled,          SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbUnplug_InterfaceLeavesDeviceList",            TestUsbUnplug_InterfaceLeavesDeviceList,            SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbReplug_NewDriverBindsOnHotPlugThread",        TestUsbReplug_NewDriverBindsOnHotPlugThread,        SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbReplug_NewMouseRegisteredAndListed",          TestUsbReplug_NewMouseRegisteredAndListed,          SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbReplug_ReportsReachNewBinding",               TestUsbReplug_ReportsReachNewBinding,               SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbTabletUnplug_UnboundInterfaceLeavesDeviceList", TestUsbTabletUnplug_UnboundInterfaceLeavesDeviceList, SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbTabletReplug_LinkDriverBindsAfterDeclines",   TestUsbTabletReplug_LinkDriverBindsAfterDeclines,   SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbTabletUnplug_LinkLeavesNetworkManager",       TestUsbTabletUnplug_LinkLeavesNetworkManager,       SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbTabletReplug_StaleAdapterNamesNoNewLink",     TestUsbTabletReplug_StaleAdapterNamesNoNewLink,     SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "LinkWithdraw_SendInFlightNeverTransmits",        TestLinkWithdraw_SendInFlightNeverTransmits,        SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "LinkWithdraw_DeliverInFlightNeverReachesStack",  TestLinkWithdraw_DeliverInFlightNeverReachesStack,  SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "LinkWithdraw_RouteLookupSurvivesRemoval",        TestLinkWithdraw_RouteLookupSurvivesRemoval,        SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbKeyboardUnplug_BuiltInLetsGo",                TestUsbKeyboardUnplug_BuiltInLetsGo,                SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "UsbKeyboardReplug_BuiltInKeepsItsInterface",     TestUsbKeyboardReplug_BuiltInKeepsItsInterface,     SkipNotUsbCell);

        // ==================== Device list ====================
        TR.Run("DeviceList_MatchesEveryFunction",   TestDeviceList_MatchesEveryFunction);
        TR.Run("DeviceList_ListsEveryUsbInterface", TestDeviceList_ListsEveryUsbInterface);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Profile ====================

    // Every other cell is gated on the base profile, so a profile this suite
    // has no expectations for (renamed in profiles.json, or added to the
    // csproj alone) would skip all of them and report green having checked
    // nothing.
    private static void TestProfile_Recognized()
    {
        Assert.True(s_isPciCell || s_isUsbCell, "the cell's profile is not one this suite has expectations for");
    }

    // ==================== PCI ====================

    // Exactly once: the profile attaches the model once. Zero means
    // enumeration missed it; a second copy would be another function the
    // test drivers match, changing what the other cells test.
    private static void TestPci_ProfileFunctionEnumeratedOnce()
    {
        Assert.Equal(1, CountFunctions(s_expectedVendorId, s_expectedDeviceId),
            "the profile's PCI function should be enumerated exactly once");
    }

    // A driver can match on class instead of IDs, so the class and subclass
    // the kernel read are pinned too.
    private static void TestPci_ProfileFunctionClassMatches()
    {
        PciDevice? function = FindFunction(s_expectedVendorId, s_expectedDeviceId);
        if (function is null)
        {
            Assert.Fail("the profile's PCI function was not enumerated");
            return;
        }

        Assert.Equal(s_expectedClassCode, function.ClassCode, "the profile's PCI function reports the wrong base class");
        Assert.Equal(s_expectedSubclass, function.Subclass, "the profile's PCI function reports the wrong subclass");
    }

    // Free for the registered drivers when the pass began: no built-in took
    // the function, and it is not the boot display either. Read in the
    // constructor, since on edu the pass then binds it.
    private static void TestPci_ProfileFunctionUnownedBeforePass()
    {
        Assert.True(s_functionFoundBeforePass, "the profile's PCI function was not enumerated when the kernel was constructed");
        Assert.Null(s_ownerBeforePass, "no built-in driver should own the profile's PCI function");
    }

    // edu goes to the edu driver, NVMe to the driver that binds after the
    // failed attempt, the 82574L to the interrupt driver, and the RTL8139 to
    // the RTL8139 driver, once the other Ethernet class drivers declined it.
    private static void TestPci_ProfileFunctionOwnerAfterPass()
    {
        PciDevice? function = FindFunction(s_expectedVendorId, s_expectedDeviceId);
        if (function is null)
        {
            Assert.Fail("the profile's PCI function was not enumerated");
            return;
        }

        string expected = BoundDriverName();
        Assert.True(function.Owner == expected, $"the profile's function should be owned by {expected}, is {function.Owner ?? "unowned"}");
    }

    // ==================== USB ====================

    // The mouse sits behind the controller the profile adds, which the USB
    // stack brings up and records as the function's owner. Exactly one:
    // neither q35 nor virt has an xHCI controller of its own.
    private static void TestUsb_XhciOwnedByXhci()
    {
        Assert.Equal(1, CountXhciFunctions(), "the profile's xHCI controller should be enumerated exactly once");

        PciDevice? xhci = FindXhciFunction();
        if (xhci is null)
        {
            Assert.Fail("no xHCI controller enumerated");
            return;
        }

        Assert.True(xhci.Owner == XhciOwner, "the xHCI controller should be owned by xhci");
    }

    // Enumeration read the mouse's configuration. Exactly one boot mouse
    // interface: the profile's tablet is HID too, but no boot device.
    private static void TestUsb_MouseEnumeratedOnce()
    {
        Assert.Equal(1, CountInterfaces(HidClass, BootInterfaceSubclass, MouseProtocol), "exactly one USB device should present a HID boot mouse interface");
    }

    // Exactly one: the no fall-through cells need one interface the device
    // match and the tablet driver match, and the profile attaches one tablet.
    private static void TestUsb_TabletEnumeratedOnce()
    {
        Assert.Equal(1, CountInterfaces(HidClass, TabletSubclass, TabletProtocol), "exactly one USB device should present a HID interface with no boot subclass, the tablet's");
    }

    // No class driver takes a HID mouse: the keyboard driver matches the
    // keyboard protocol only, the hub and mass storage drivers other classes.
    // So the interface was free when the kernel was constructed, and the
    // pass then gave it to the boot mouse driver, through the kit's class
    // driver, which the USB stack hands it back to on disconnect.
    private static void TestUsb_MouseInterfaceBoundByPass()
    {
        Assert.True(s_mouseFoundBeforePass, "no HID boot mouse interface was enumerated when the kernel was constructed");
        Assert.Null(s_mouseOwnerBeforePass, "no class driver should have bound the HID boot mouse interface before the pass");

        UsbInterface? mouse = FindInterface(HidClass, BootInterfaceSubclass, MouseProtocol, out _);
        if (mouse is null)
        {
            Assert.Fail("no HID boot mouse interface enumerated");
            return;
        }

        Assert.True(mouse.DriverName == UsbBootMouseDriver.Name,
            $"the HID boot mouse interface should be owned by {UsbBootMouseDriver.Name}, is {mouse.DriverName ?? "unowned"}");
        Assert.True(mouse.Driver == KitUsbDriver.Instance, "the USB stack should record the kit's class driver as the mouse interface's driver");
        Assert.True(mouse.DriverContext is not null && mouse.DriverContext == UsbBootMouseDriver.Context,
            "the mouse interface should keep the context its driver bound it with");
    }

    // ==================== Registration ====================

    // A second registration under a taken name is refused, whether another
    // registration or a built-in driver holds the name, and the one that
    // registered first keeps it.
    private static void TestRegister_TakenNameRefused()
    {
        Assert.Equal(AcceptedRegistrationCount, s_acceptedRegistrations, "every distinct registration should be accepted");
        Assert.False(s_duplicateNameAccepted, "a second registration named edu should be refused");
        Assert.False(s_builtInNameAccepted, "a registration named after the xhci built-in should be refused");
        Assert.False(s_bootDisplayNameAccepted, "a registration named gop should be refused");
    }

    // Each malformed registration is refused when it is built, before it can
    // reach Register.
    private static void TestRegister_InvalidRegistrationThrows()
    {
        Assert.True(ThrowsArgumentException(static () => new PciDriverRegistration(string.Empty, CreateInvalid, PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId))),
            "an empty name should throw ArgumentException");
        Assert.True(ThrowsArgumentException(static () => new PciDriverRegistration(InvalidName, CreateInvalid)),
            "a registration without match entries should throw ArgumentException");
        Assert.True(ThrowsArgumentException(static () => new PciDriverRegistration(InvalidName, CreateInvalid, default(PciMatch))),
            "a default PciMatch entry should throw ArgumentException");
        Assert.True(ThrowsArgumentException(static () => new PciDriverRegistration(InvalidName, CreateInvalid,
                PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId), default(PciMatch))),
            "a default PciMatch among valid entries should throw ArgumentException");
    }

    // The pass closed registration: a driver registered now could no longer
    // be offered anything.
    private static void TestRegister_AfterPassThrows()
    {
        bool threw = false;
        try
        {
            DriverCore.Register(new PciDriverRegistration(LateName, CreateInvalid, PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId)));
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        Assert.True(threw, "Register after the driver pass should throw InvalidOperationException");
    }

    // The pass closes registration before the first factory runs, so
    // Register from a factory or a Probe would throw even without the
    // engine's re-entrancy flag. The message tells the two checks apart,
    // and it must be the flag's.
    private static void TestRegister_FromDriverCallbackThrows()
    {
        Assert.True(ReentrantDriver.FactoryRan, "the reentrant driver's factory never ran");
        Assert.True(ReentrantDriver.FactoryRegisterMessage == DriverCore.RegisterFromDriverCallbackMessage,
            $"Register from inside a factory should be refused by the re-entrancy check, threw {ReentrantDriver.FactoryRegisterMessage ?? "nothing"}");
        Assert.True(ReentrantDriver.ProbeRegisterMessage == DriverCore.RegisterFromDriverCallbackMessage,
            $"Register from inside Probe should be refused by the re-entrancy check, threw {ReentrantDriver.ProbeRegisterMessage ?? "nothing"}");
    }

    // USB and PCI registrations share one name space, which holds the
    // built-in drivers' names too, the USB class drivers' among them: the
    // device list names an owner by it. A name another registration of
    // either bus holds, or a built-in's of either bus, is refused.
    private static void TestRegister_UsbTakenNameRefused()
    {
        Assert.Equal(AcceptedUsbRegistrationCount, s_acceptedUsbRegistrations, "every distinct USB registration should be accepted");
        Assert.False(s_usbDuplicateNameAccepted, $"a second USB registration named {UsbBootMouseDriver.Name} should be refused");
        Assert.False(s_usbNamedAfterPciAccepted, $"a USB registration named {EduDriver.Name}, a PCI registration's name, should be refused");
        Assert.False(s_pciNamedAfterUsbAccepted, $"a PCI registration named {UsbBootMouseDriver.Name}, a USB registration's name, should be refused");
        Assert.False(s_usbHubNameAccepted, "a USB registration named after the hub built-in should be refused");
        Assert.False(s_usbKeyboardNameAccepted, "a USB registration named after the HID boot keyboard built-in should be refused");
        Assert.False(s_usbMassStorageNameAccepted, "a USB registration named after the mass storage built-in should be refused");
        Assert.False(s_usbNamedAfterPciBuiltInAccepted, "a USB registration named after the xhci built-in should be refused");
        Assert.False(s_pciNamedAfterUsbBuiltInAccepted, "a PCI registration named after the hub built-in should be refused");
    }

    // Each malformed USB registration is refused when it is built, before
    // it can reach Register.
    private static void TestRegister_UsbInvalidRegistrationThrows()
    {
        Assert.True(ThrowsArgumentException(static () => new UsbDriverRegistration(string.Empty, CreateInvalidUsb, UsbMatch.Interface(HidClass))),
            "an empty name should throw ArgumentException");
        Assert.True(ThrowsArgumentException(static () => new UsbDriverRegistration(InvalidName, CreateInvalidUsb)),
            "a registration without match entries should throw ArgumentException");
        Assert.True(ThrowsArgumentException(static () => new UsbDriverRegistration(InvalidName, CreateInvalidUsb, default(UsbMatch))),
            "a default UsbMatch entry should throw ArgumentException");
        Assert.True(ThrowsArgumentException(static () => new UsbDriverRegistration(InvalidName, CreateInvalidUsb,
                UsbMatch.Interface(HidClass), default(UsbMatch))),
            "a default UsbMatch among valid entries should throw ArgumentException");
    }

    // The pass closed USB registration too: the interfaces present at boot
    // were offered already, and every later one goes to the drivers the
    // pass knew.
    private static void TestRegister_UsbAfterPassThrows()
    {
        bool threw = false;
        try
        {
            DriverCore.Register(new UsbDriverRegistration(LateUsbName, CreateInvalidUsb, UsbMatch.Interface(HidClass)));
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        Assert.True(threw, "Register of a USB driver after the driver pass should throw InvalidOperationException");
    }

    // As for PCI: the flag's message, not the closed registration's, from a
    // USB driver's factory and from its Probe.
    private static void TestRegister_FromUsbDriverCallbackThrows()
    {
        Assert.True(UsbHidDeviceDriver.Probes > 0, "the device-matching USB driver was never probed");
        Assert.True(UsbHidDeviceDriver.FactoryRegisterMessage == DriverCore.RegisterFromDriverCallbackMessage,
            $"Register from inside a USB driver's factory should be refused by the re-entrancy check, threw {UsbHidDeviceDriver.FactoryRegisterMessage ?? "nothing"}");
        Assert.True(UsbHidDeviceDriver.ProbeRegisterMessage == DriverCore.RegisterFromDriverCallbackMessage,
            $"Register from inside a USB driver's Probe should be refused by the re-entrancy check, threw {UsbHidDeviceDriver.ProbeRegisterMessage ?? "nothing"}");
    }

    // ==================== Ranking ====================

    // The class registration came first, yet the device registrations were
    // offered edu before it, and one of them bound before it was reached.
    private static void TestRanking_DeviceMatchBeatsClassMatch()
    {
        PciDevice? edu = FindFunction(EduDriver.VendorId, EduDriver.DeviceId);
        Assert.True(edu is not null && edu.Owner == EduDriver.Name, "edu should be owned by the device-matching edu driver");
        Assert.False(ProbeLog.Recorded(EduClassName), $"the class match should never have been probed, probes ran {ProbeLog.Describe()}");
    }

    // The throwing and the declining candidate each let the function go on
    // to the next, in registration order among equal matches. On NVMe the
    // device match that fails, registered after the class match, is still
    // offered the controller first, and the class match binds it after.
    private static void TestRanking_FailedAndDeclinedFallThrough()
    {
        string expected = s_isEduCell ? ExpectedEduProbeOrder : ExpectedNvmeProbeOrder;
        string order = ProbeLog.Describe();
        Assert.True(order == expected, $"the profile's function should be probed as {expected}, was {order}");
    }

    // Registered first, the class-only match is still offered the NIC after
    // the one naming the programming interface too, and the interrupt
    // driver's and the RTL8139 driver's equal class matches, registered
    // later, after both, in registration order. The first two decline; the
    // interrupt driver binds the 82574L and declines the RTL8139, which the
    // RTL8139 driver then binds.
    private static void TestRanking_ClassWithInterfaceBeatsClass()
    {
        string expected = s_isE1000ECell ? ExpectedE1000EProbeOrder : ExpectedRtl8139ProbeOrder;
        string order = ProbeLog.Describe();
        Assert.True(order == expected, $"NIC drivers should be probed as {expected}, were {order}");
    }

    // The class-only match registered first, yet the mouse interface went
    // to the device match, which declined having opened nothing, then to
    // the two boot mouse matches in registration order, and the second one
    // bound it before the class match was reached. Nothing offered the class
    // match anything: the tablet's offering ended before it, see the no
    // fall-through cell.
    private static void TestUsbRanking_DeviceMatchFallsThroughToBootMouse()
    {
        UsbInterface? mouse = FindInterface(HidClass, BootInterfaceSubclass, MouseProtocol, out UsbDevice? device);
        if (mouse is null || device is null)
        {
            Assert.Fail("no HID boot mouse interface enumerated");
            return;
        }

        string order = ProbeLog.Describe(ExpectedUsbPath(device, mouse));
        Assert.True(order == ExpectedMouseProbeOrder, $"the mouse interface should be offered as {ExpectedMouseProbeOrder}, was {order}");
        Assert.False(ProbeLog.Recorded(UsbHidClassName), $"the class-only match should never have been offered an interface, probes ran {ProbeLog.Describe()}");
    }

    // ==================== Teardown ====================

    // Every failed or declined attempt writes the Command register back as it
    // found it, but with bus mastering off, and every attempt starts with it
    // off, whatever firmware left. On edu, two attempts ran before the edu
    // driver's, and on NVMe one, which turned on memory decoding and bus
    // mastering, so the driver that binds must find the register as the
    // kernel constructor did, bus mastering off, plus the INTx disable its
    // own attempt set; on the 82574L the two class drivers declined before
    // the interrupt driver's turn, and on the RTL8139 those two and the
    // interrupt driver before the RTL8139 driver's.
    private static void TestTeardown_RestoresCommandRegister()
    {
        Assert.True(s_functionFoundBeforePass, "the profile's PCI function was not enumerated when the kernel was constructed");
        string boundDriver = BoundDriverName();
        bool probed;
        ushort found;
        if (s_isEduCell)
        {
            probed = EduDriver.Probed;
            found = EduDriver.CommandAtProbe;
        }
        else if (s_isNvmeCell)
        {
            probed = NvmeDriver.Probed;
            found = NvmeDriver.CommandAtProbe;
        }
        else if (s_isE1000ECell)
        {
            probed = E1000EInterruptDriver.Probed;
            found = E1000EInterruptDriver.CommandAtProbe;
        }
        else
        {
            probed = Rtl8139Driver.Probed;
            found = Rtl8139Driver.CommandAtProbe;
        }

        Assert.True(probed, $"{boundDriver} was never probed");
        ushort before = (ushort)s_commandBeforePass;
        ushort expected = (ushort)((before & ~BusMasterBit) | InterruptDisableBit);
        Assert.True(found == expected, $"{boundDriver} should find Command 0x{expected:X4}, found 0x{found:X4} (0x{before:X4} before the pass)");
    }

    // The throwing driver kept its BAR region and DMA buffer past its failed
    // probe. Both must refuse to be used now: the buffer's pages are free
    // again and the device may be another driver's.
    private static void TestTeardown_InvalidatesRegionAndBuffer()
    {
        MmioRegion? region = ThrowingDriver.Region;
        DmaBuffer? buffer = ThrowingDriver.Buffer;
        Assert.NotNull(region, "the throwing driver should have mapped BAR 0");
        Assert.NotNull(buffer, "the throwing driver should have allocated a DMA buffer");
        if (region is null || buffer is null)
        {
            return;
        }

        Assert.True(ThrowsInvalidOperation(() => region.Read32(0)), "the failed attempt's region should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => buffer.Span.Clear()), "the failed attempt's DMA buffer should throw InvalidOperationException");
    }

    // ==================== Context ====================

    // Resources are handed out during Probe only, so a bound context refuses
    // them afterwards.
    private static void TestContext_ProbeOnlyMembersThrowAfterProbe()
    {
        PciDeviceContext? context = EduDriver.Context;
        if (context is null)
        {
            Assert.Fail("the edu driver was never probed");
            return;
        }

        Assert.True(ThrowsInvalidOperation(() => context.TryMapBar(0, out _)), "TryMapBar after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.TryMapIoBar(0, out _)), "TryMapIoBar after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.TryAllocateDma(1, ulong.MaxValue, out _)), "TryAllocateDma after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(context.EnableBusMastering), "EnableBusMastering after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.TryRequestInterrupts(static _ => { })), "TryRequestInterrupts after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.CreateEvent()), "CreateEvent after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.TryCreateWorkItem(static () => { }, out _)), "TryCreateWorkItem after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.PublishMouse()), "PublishMouse after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.PublishNetworkLink(MACAddress.None, static _ => false)), "PublishNetworkLink after Probe should throw InvalidOperationException");
        Assert.True(context.IsPresent, "a bound PCI function is always present");
        Assert.True(context.Path == ExpectedPath(FindFunction(EduDriver.VendorId, EduDriver.DeviceId)),
            $"the context's path {context.Path} should name the edu function");
    }

    // Config writes stay open after Probe, in thread context, but never below
    // 0x40, where the header the kit manages sits, nor past the space.
    private static void TestContext_WriteConfigSparesHeader()
    {
        PciDeviceContext? context = EduDriver.Context;
        if (context is null)
        {
            Assert.Fail("the edu driver was never probed");
            return;
        }

        Assert.True(ThrowsArgumentOutOfRange(() => context.WriteConfig16(0x04, 0)), "a write to the Command register should throw ArgumentOutOfRangeException");
        Assert.True(ThrowsArgumentOutOfRange(() => context.WriteConfig8(FirstDriverConfigOffset - 1, 0)), "a write below 0x40 should throw ArgumentOutOfRangeException");
        Assert.True(ThrowsArgumentOutOfRange(() => context.WriteConfig8(0x100, 0)), "a write past the configuration space should throw ArgumentOutOfRangeException");
        Assert.True(ThrowsArgumentOutOfRange(() => context.WriteConfig16(FirstDriverConfigOffset + 1, 0)), "a misaligned write should throw ArgumentOutOfRangeException");

        // Writes the first capability dword back unchanged: allowed, and
        // leaves edu's MSI capability as it was.
        uint value = context.Function.ReadConfig32(FirstDriverConfigOffset);
        context.WriteConfig32(FirstDriverConfigOffset, value);
        Assert.True(context.Function.ReadConfig32(FirstDriverConfigOffset) == value, "the capability dword should read back as written");
    }

    // The IDs, config reads and the capability walk, through the seam.
    private static void TestFunction_ReadsConfigSpace()
    {
        PciDeviceContext? context = EduDriver.Context;
        if (context is null)
        {
            Assert.Fail("the edu driver was never probed");
            return;
        }

        PciFunction function = context.Function;
        Assert.Equal(EduDriver.VendorId, function.VendorId, "PciFunction.VendorId");
        Assert.Equal(EduDriver.DeviceId, function.DeviceId, "PciFunction.DeviceId");
        Assert.Equal(UnclassifiedClassCode, function.BaseClass, "PciFunction.BaseClass");
        Assert.Equal(OtherSubclass, function.Subclass, "PciFunction.Subclass");
        Assert.Equal(EduDriver.VendorId, function.ReadConfig16(0x00), "config offset 0x00 should hold the vendor ID");
        Assert.True(function.ReadConfig32(0x00) == ((uint)EduDriver.DeviceId << 16 | EduDriver.VendorId), "config dword 0x00 should hold both IDs");
        Assert.True(ThrowsArgumentOutOfRange(() => function.ReadConfig8(0x100)), "a read past the configuration space should throw ArgumentOutOfRangeException");
        Assert.True(ThrowsArgumentOutOfRange(() => function.ReadConfig32(0x02)), "a misaligned read should throw ArgumentOutOfRangeException");

        bool found = function.TryFindCapability(MsiCapabilityId, out ushort offset);
        Assert.True(found, "edu's MSI capability should be found");
        Assert.True(offset >= FirstDriverConfigOffset && function.ReadConfig8(offset) == MsiCapabilityId,
            $"the MSI capability offset 0x{offset:X} should point at its ID");
        Assert.False(function.TryFindCapability(0xFF, out _), "a capability edu lacks should not be found");
    }

    private static void TestMmio_MapsWholeBar()
    {
        MmioRegion? registers = EduDriver.Registers;
        if (registers is null)
        {
            Assert.Fail("the edu driver did not map BAR 0");
            return;
        }

        Assert.True(registers.Length == EduDriver.BarLength, $"BAR 0 should map {EduDriver.BarLength} bytes, maps {registers.Length}");
    }

    // Recorded during Probe: past the end, straddling the end, and a 32-bit
    // read at an offset that is not a multiple of 4.
    private static void TestMmio_RefusesBadAccesses()
    {
        Assert.True(EduDriver.PastEndRefused, "a read at the region's length should throw ArgumentOutOfRangeException");
        Assert.True(EduDriver.StraddlingRefused, "a read straddling the region's end should throw ArgumentOutOfRangeException");
        Assert.True(EduDriver.MisalignedRefused, "a misaligned read should throw ArgumentOutOfRangeException");
    }

    // ==================== edu ====================

    private static void TestEdu_Identification()
    {
        Assert.True(EduDriver.Identification == EduDriver.ExpectedIdentification,
            $"edu's identification register should read 0x{EduDriver.ExpectedIdentification:X8}, read 0x{EduDriver.Identification:X8}");
    }

    private static void TestEdu_Liveness()
    {
        uint expected = ~EduDriver.LivenessPattern;
        Assert.True(EduDriver.Liveness == expected,
            $"edu's liveness register should read 0x{expected:X8}, read 0x{EduDriver.Liveness:X8}");
    }

    private static void TestEdu_Factorial()
    {
        Assert.True(EduDriver.FactorialCompleted, "edu's factorial unit did not finish");
        Assert.True(EduDriver.Factorial == EduDriver.ExpectedFactorial,
            $"edu should compute {EduDriver.FactorialInput}! = {EduDriver.ExpectedFactorial}, computed {EduDriver.Factorial}");
    }

    // A round trip where the allocator has a page below edu's 28-bit limit
    // (x64, whose RAM starts low); elsewhere a failure the allocator explains
    // (arm64, whose RAM starts at 1 GiB).
    private static void TestEdu_Dma()
    {
        Assert.True(EduDriver.DmaPassed, EduDriver.DmaReport ?? "the DMA check never ran");
    }

    // ==================== Interrupts ====================

    // Granted to both edu attempts, polled since edu has no MSI-X; a second
    // request in the same Probe is refused.
    private static void TestInterrupts_RequestOncePerAttempt()
    {
        Assert.True(EduDriver.InterruptsGranted, "the edu driver's TryRequestInterrupts should be granted: the timer ticks, so it can be polled");
        Assert.True(EduDriver.SecondRequestThrew, "a second TryRequestInterrupts in the same Probe should throw InvalidOperationException");
        Assert.True(ThrowingDriver.InterruptsGranted, "the throwing driver's TryRequestInterrupts should be granted");
    }

    // The edu driver raised an interrupt in Probe and held it pending for
    // several polling periods: no handler call took it before Probe
    // returned, and one after Bound did.
    private static void TestInterrupts_HandlerIdleUntilBound()
    {
        Assert.False(EduDriver.HandlerRanBeforeBound, "the edu handler ran before Probe returned Bound");
        Assert.True((EduDriver.InterruptStatusAtProbeEnd & EduDriver.ProbeRaise) != 0,
            $"the interrupt raised in Probe should still be pending when Probe returns, the status read 0x{EduDriver.InterruptStatusAtProbeEnd:X}");
        Assert.True(WaitUntil(static () => (EduDriver.Acknowledged & EduDriver.ProbeRaise) != 0),
            "the interrupt raised in Probe was never serviced once the binding was Bound");
    }

    // An interrupt raised now reaches the polled handler, which clears it in
    // the device and signals the event a thread waits on.
    private static void TestInterrupts_PolledHandlerServicesDevice()
    {
        EduDriver.Raise(ServiceRaise);
        bool serviced = WaitUntil(static () => (EduDriver.Acknowledged & ServiceRaise) != 0);
        Assert.True(serviced, "the polled handler never serviced the interrupt raised through edu's raise register");
        if (!serviced)
        {
            return;
        }

        Assert.True(WaitUntil(static () => EduDriver.ReadInterruptStatus() == 0), "edu's interrupt status should be clear once the handler acknowledged it");

        // Signalled by every interrupt serviced, so this returns at once.
        DeviceEvent? serviceEvent = EduDriver.Event;
        Assert.True(serviceEvent is not null && serviceEvent.Wait(), "a wait on the event the handler signalled should return true");
    }

    // e1000e on arm64: MSI-X through the ITS on GICv3, with entry 0
    // programmed masked during Probe and unmasked on Bound, bus mastering
    // on; polled on GICv2, where the handler then runs on every tick.
    private static void TestInterrupts_E1000EModeFollowsGic()
    {
        Assert.True(E1000EInterruptDriver.Probed, "the 82574L interrupt driver was never probed");
        Assert.True(E1000EInterruptDriver.InterruptsGranted, "the 82574L's TryRequestInterrupts should be granted");
        if (!s_expectMsiX)
        {
            Assert.False(E1000EInterruptDriver.MsiXEnabledAfterRequest, "without an ITS the kit should poll, and leave MSI-X off");
            Assert.True(WaitUntil(static () => E1000EInterruptDriver.HandlerCalls > 0), "the polled handler never ran once the binding was Bound");
            return;
        }

        Assert.True(E1000EInterruptDriver.MsiXEnabledAfterRequest, "on GICv3 the kit should enable MSI-X, routed through the ITS");
        Assert.True(E1000EInterruptDriver.Entry0MaskedInProbe, "MSI-X entry 0 should be programmed masked during Probe");
        MmioRegion? table = E1000EInterruptDriver.MsiXTable;
        PciDeviceContext? context = E1000EInterruptDriver.Context;
        if (table is null || context is null)
        {
            Assert.Fail("the 82574L's MSI-X table did not map");
            return;
        }

        Assert.True((table.Read32(E1000EInterruptDriver.Entry0Control) & MsiXState.EntryMaskBit) == 0, "MSI-X entry 0 should be unmasked once the binding is Bound");
        Assert.True(MsiXState.IsEnabled(context.Function), "MSI-X should stay enabled once the binding is Bound");
        Assert.True((context.Function.ReadConfig16(CommandOffset) & BusMasterBit) != 0, "bus mastering, which an MSI-X message needs, should be on once the binding is Bound");
    }

    // Runs last among the edu cells, once the edu handler has run on many
    // ticks. The failed attempt's handler, polled from the same timer, was
    // never armed, since only Bound arms one, so it never ran; and the
    // teardown took its poll timer off the platform timer, which would
    // otherwise go on calling the disarmed handler's trampoline on every
    // tick for the life of the kernel.
    private static void TestInterrupts_TornDownHandlerNeverRuns()
    {
        Assert.True(EduDriver.HandlerCalls > 0, "the edu handler never ran, so the timer never polled anything");
        Assert.Equal(0, ThrowingDriver.HandlerCalls, "the failed attempt's interrupt handler ran");

        SoftwareTimer? pollTimer = ThrowingDriver.PollTimer;
        Assert.NotNull(pollTimer, "the failed attempt's granted interrupts should have been polled: edu has no MSI-X");
        Assert.False(pollTimer is not null && pollTimer.IsActive, "the failed attempt's poll timer should be off the platform timer");
    }

    // ==================== Work items and events ====================

    // Scheduled in Probe, where a second Schedule found it pending: it ran
    // once, after Bound, on the driver-work thread.
    private static void TestWorkItem_ScheduledInProbeRunsAfterBound()
    {
        Assert.True(EduDriver.WorkItemsCreated, "TryCreateWorkItem should succeed: the scheduler runs, so the driver-work thread starts");
        Assert.True(EduDriver.ProbeWorkScheduled, "Schedule in Probe should accept the work item");
        Assert.False(EduDriver.ProbeWorkScheduledAgain, "a second Schedule while the work item is pending should return false");
        Assert.True(WaitUntil(static () => EduDriver.ProbeWorkRuns > 0), "the work item scheduled in Probe never ran");
        Assert.Equal(1, EduDriver.ProbeWorkRuns, "scheduled once while pending, the work item should run once");
        Assert.False(EduDriver.ProbeWorkRanBeforeBound, "the work item scheduled in Probe ran before Probe returned Bound");
        Assert.False(EduDriver.ProbeWorkOnIdleThread, "a work item should run on the driver-work thread, not the idle thread");
        Assert.True(EduDriver.ProbeWorkThreadId != s_bootThreadId, "a work item should run on the driver-work thread, not the boot thread");
    }

    // The handler schedules its work item for every interrupt it services,
    // in interrupt context; the item then runs on the same driver-work
    // thread as the one Probe scheduled.
    private static void TestWorkItem_ScheduledFromHandlerRunsOnDriverWork()
    {
        int runsBefore = EduDriver.HandlerWorkRuns;
        EduDriver.Raise(WorkRaise);
        Assert.True(WaitUntil(() => EduDriver.HandlerWorkRuns > runsBefore), "the work item the handler scheduled never ran");
        Assert.False(EduDriver.HandlerWorkOnIdleThread, "the handler's work item should run on the driver-work thread, not the idle thread");
        Assert.True(EduDriver.HandlerWorkThreadId != s_bootThreadId, "the handler's work item should run on the driver-work thread, not the boot thread");
        Assert.True(EduDriver.HandlerWorkThreadId == EduDriver.ProbeWorkThreadId, "both work items should run on the one driver-work thread");
    }

    // The failed attempt scheduled one work item in Probe and created
    // another: the teardown dropped both, so the scheduled one never ran,
    // though the thread ran the edu driver's, and neither can be scheduled
    // any more. The scheduled one would refuse for still being pending even
    // if it had not been dropped; the other refuses only because it was.
    private static void TestWorkItem_DroppedWithFailedAttempt()
    {
        DeviceWorkItem? dropped = ThrowingDriver.WorkItem;
        DeviceWorkItem? unscheduled = ThrowingDriver.UnscheduledWorkItem;
        if (dropped is null || unscheduled is null)
        {
            Assert.Fail("the throwing driver got no work items");
            return;
        }

        Assert.True(ThrowingDriver.WorkScheduled, "Schedule in the failed Probe should have accepted the work item");
        Assert.True(EduDriver.ProbeWorkRuns > 0, "the driver-work thread never ran a work item, so the check below proves nothing");
        Assert.Equal(0, ThrowingDriver.WorkRuns, "the failed attempt's work item ran");
        Assert.False(dropped.Schedule(), "Schedule on the failed attempt's scheduled work item should return false");
        Assert.False(unscheduled.Schedule(), "Schedule on the failed attempt's unscheduled work item should return false");
    }

    // Interrupts are armed only after Probe, so a wait there could never end.
    private static void TestEvent_WaitThrowsInProbe()
    {
        Assert.True(EduDriver.WaitInProbeThrew, "DeviceEvent.Wait inside Probe should throw InvalidOperationException");
    }

    // The failed attempt's event was cancelled with it.
    private static void TestEvent_TornDownWaitReturnsFalse()
    {
        DeviceEvent? cancelled = ThrowingDriver.Event;
        if (cancelled is null)
        {
            Assert.Fail("the throwing driver got no event");
            return;
        }

        Assert.False(cancelled.Wait(), "a wait on the failed attempt's event should return false at once");
    }

    // Holding the lock masks interrupts: the polled edu handler, called on
    // every tick, is not called while it is held, and is again once the
    // scope ends, which also frees the lock to be entered again (a lock left
    // held would spin that second entry forever, and time the cell out).
    private static void TestLock_IrqSafeLockMasksInterrupts()
    {
        IrqSafeLock gate = new();
        int callsBefore;
        int callsHeld;
        using (gate.EnterScope())
        {
            callsBefore = EduDriver.HandlerCalls;
            SpinMilliseconds(LockHoldMilliseconds);
            callsHeld = EduDriver.HandlerCalls;
        }

        Assert.Equal(callsBefore, callsHeld, "the polled handler ran while the lock was held: entering did not mask interrupts");
        Assert.True(WaitUntil(() => EduDriver.HandlerCalls != callsHeld), "the polled handler never ran again once the scope ended: interrupts stayed masked");

        int reentries = 0;
        using (gate.EnterScope())
        {
            reentries++;
        }

        Assert.Equal(1, reentries, "the lock could not be entered again");

        // A default scope holds nothing, and disposing it releases nothing.
        default(IrqSafeLock.Scope).Dispose();
    }

    // ==================== Publications ====================

    // edu's Probe published a mouse, which the mouse manager took when the
    // binding became Bound. The handler reports through it when a cell
    // raises MouseRaise, so the report travels from the polled handler, in
    // interrupt context, to the manager, which moves the pointer, adds the
    // wheel and records the buttons.
    private static void TestPublish_MouseMovesPointer()
    {
        Assert.True(EduDriver.MousePublished, "PublishMouse in the edu driver's Probe should return a reporter");

        MouseManager.SetPosition(PointerStartX, PointerStartY);
        MouseManager.ResetScrollDelta();
        EduDriver.Raise(EduDriver.MouseRaise);

        // The handler reports before it records the acknowledgement.
        bool serviced = WaitUntil(static () => (EduDriver.Acknowledged & EduDriver.MouseRaise) != 0);
        Assert.True(serviced, "the polled handler never serviced the mouse raise");
        if (!serviced)
        {
            return;
        }

        Assert.Equal(PointerStartX + EduDriver.MouseDeltaX, MouseManager.X, "the report should move the pointer right by its X delta");
        Assert.Equal(PointerStartY + EduDriver.MouseDeltaY, MouseManager.Y, "the report should move the pointer down by its Y delta");
        Assert.Equal(EduDriver.MouseWheel, MouseManager.ScrollDelta, "the report's wheel should reach the scroll delta, negative for up");
        Assert.True(MouseManager.LeftButton, "the report holds the left button");
        Assert.True(MouseManager.MiddleButton, "the report holds the middle button");
        Assert.False(MouseManager.RightButton, "the report does not hold the right button");
    }

    // The throwing driver published a mouse and a network link before it
    // threw. Teardown dropped both: the link never joined the network
    // manager, and a report through the mouse moves nothing.
    private static void TestPublish_DroppedWithFailedAttempt()
    {
        MouseReporter? mouse = ThrowingDriver.Mouse;
        NetworkLink? link = ThrowingDriver.Link;
        if (mouse is null || link is null)
        {
            Assert.Fail("PublishMouse and PublishNetworkLink in the throwing driver's Probe should both return");
            return;
        }

        Assert.Equal(s_networkDevicesBeforePass, NetworkManager.DeviceCount, "the failed attempt's link should never join the network manager");
        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            MACAddress? registered = NetworkManager.GetAdapter(i).MacAddress;
            Assert.False(registered is not null && registered.Equals(link.Address), $"network device {i} has the failed attempt's MAC address {link.Address}");
        }

        MouseManager.SetPosition(PointerStartX, PointerStartY);
        mouse.Report(EduDriver.MouseDeltaX, EduDriver.MouseDeltaY, 0, MouseButtons.Right);
        Assert.Equal(PointerStartX, MouseManager.X, "a report through the failed attempt's mouse moved the pointer");
        Assert.Equal(PointerStartY, MouseManager.Y, "a report through the failed attempt's mouse moved the pointer");
        Assert.False(MouseManager.RightButton, "a report through the failed attempt's mouse reached the buttons");
        Assert.Equal(0, ThrowingDriver.Transmits, "the stack sent through the failed attempt's link");
    }

    // ==================== RTL8139 ====================

    // The link the RTL8139 driver published in Probe joined the network
    // manager when the binding became Bound. The profile attaches no other
    // NIC, so it is the only device and the primary one, with the address
    // and link state the driver read from the chip.
    private static void TestRtl8139_LinkRegistered()
    {
        Assert.True(Rtl8139Driver.Probed, "the RTL8139 driver was never probed");
        MACAddress? address = Rtl8139Driver.Address;
        if (address is null)
        {
            Assert.Fail(Rtl8139Driver.ProbeFailure ?? "the RTL8139 driver published no link");
            return;
        }

        Assert.Equal(0, s_networkDevicesBeforePass, "the rtl8139 profile attaches no other NIC, yet a network device registered before the pass");
        Assert.Equal(1, NetworkManager.DeviceCount, "the RTL8139's link should be the one network device");

        NetworkAdapter adapter = NetworkManager.GetAdapter(0);
        MACAddress? registered = adapter.MacAddress;
        Assert.True(registered is not null && registered.Equals(address),
            $"the registered device should have the RTL8139's address {address}, has {registered?.ToString() ?? "none"}");

        string expectedName = $"{Rtl8139Driver.Name} {ExpectedPath(FindFunction(Rtl8139Driver.VendorId, Rtl8139Driver.DeviceId))}";
        Assert.True(adapter.Name == expectedName, $"the registered device should be named {expectedName}, is {adapter.Name ?? "unnamed"}");
        Assert.True(adapter.Ready, "the link should be ready once delivered");
        Assert.True(Rtl8139Driver.LinkUpAtProbe, "the RTL8139 should report its link up");
        Assert.True(adapter.LinkUp, "the link state the driver set should reach the network manager");
        Assert.True(NetworkManager.Primary == adapter, "with no built-in NIC registered, the RTL8139's link should be primary");
    }

    // The RTL8139 has no MSI-X: the kit polls the handler from the timer,
    // which ticks, so it runs once the binding is Bound.
    private static void TestRtl8139_PolledHandlerRuns()
    {
        Assert.True(Rtl8139Driver.InterruptsGranted, "the RTL8139 driver's TryRequestInterrupts should be granted: the timer ticks, so it can be polled");
        Assert.True(WaitUntil(static () => Rtl8139Driver.HandlerCalls > 0), "the polled handler never ran once the binding was Bound");
    }

    // The kernel's DHCP client broadcasts from every network device and
    // applies the lease QEMU's user-mode network hands out: the Discover and
    // the Request leave through the driver's transmit handler, the Offer and
    // the Ack come back through the polled handler, the receive work item
    // and the link's Deliver. The client bounds its own waits, 5 s each.
    private static void TestRtl8139_DhcpLease()
    {
        int transmitsBefore = Rtl8139Driver.Transmits;
        int deliveredBefore = Rtl8139Driver.FramesDelivered;

        DhcpClient client = new();
        int elapsed = client.SendDiscoverPacket();
        Assert.True(elapsed >= 0,
            $"DHCP timed out: {Rtl8139Driver.Transmits - transmitsBefore} frames sent, {Rtl8139Driver.FramesDelivered - deliveredBefore} delivered, {Rtl8139Driver.ReceiveInterrupts} receive interrupts, {Rtl8139Driver.BadFrames} bad ring entries");
        if (elapsed < 0)
        {
            return;
        }

        IPConfig? config = NetworkManager.Primary.IPConfig;
        Assert.True(config is not null && !config.Address.IsZero, "DHCP should leave the RTL8139's link configured with a non-zero address");
        Assert.True(Rtl8139Driver.Transmits > transmitsBefore, "the DHCP exchange should have sent through the transmit handler");
        Assert.True(Rtl8139Driver.FramesDelivered > deliveredBefore, "the DHCP exchange should have delivered frames through the link");
        Assert.True(Rtl8139Driver.ReceiveInterrupts > 0, "the polled handler should have found the received frames");
        Assert.True(Rtl8139Driver.ReceiveWorkThreadId != s_bootThreadId, "the receive work item should run on the driver-work thread, not the boot thread");
    }

    // ==================== NVMe ====================

    // The device match, offered the controller first, got interrupts:
    // MSI-X where the platform routes it, with entry 0 programmed masked and
    // one vector bound on x64, polled elsewhere. It then failed, and its
    // handler never ran.
    private static void TestNvme_FailedAttemptRequestedInterrupts()
    {
        Assert.True(FailingNvmeDriver.Probed, "the failing NVMe driver was never probed");
        Assert.True(FailingNvmeDriver.InterruptsGranted, "the failing NVMe driver's TryRequestInterrupts should be granted");
        Assert.Equal(s_expectMsiX, FailingNvmeDriver.MsiXEnabledAfterRequest,
            s_expectMsiX ? "MSI-X should be enabled: the platform routes it" : "MSI-X should stay off: the platform cannot route it, so the kit polls");
        if (s_expectMsiX)
        {
            Assert.True(FailingNvmeDriver.Entry0Read && FailingNvmeDriver.Entry0MaskedAfterRequest, "MSI-X entry 0 should be programmed masked");
        }

        if (IsX64)
        {
            Assert.Equal(FailingNvmeDriver.BoundVectorsBefore + 1, FailingNvmeDriver.BoundVectorsAfterRequest, "the MSI-X entry should take one interrupt vector");
        }

        Assert.Equal(0, FailingNvmeDriver.HandlerCalls, "the failed attempt's interrupt handler ran");
    }

    // What the failed attempt held is back when the next candidate is
    // offered the controller: MSI-X Enable is off, the vector is free on
    // x64, the timer that polled it where MSI-X is out of reach is off the
    // platform timer, and the pages of its DMA buffer are free again.
    private static void TestNvme_TeardownReleasedInterrupts()
    {
        Assert.True(NvmeDriver.Probed, "the NVMe driver matched by class was never probed");
        Assert.False(NvmeDriver.MsiXEnabledAtProbe, "MSI-X Enable should be off when the next candidate is offered the controller");
        if (IsX64)
        {
            Assert.Equal(FailingNvmeDriver.BoundVectorsBefore, NvmeDriver.BoundVectorsAtProbe, "the failed attempt's interrupt vector should be free again");
        }

        SoftwareTimer? pollTimer = FailingNvmeDriver.PollTimer;
        if (s_expectMsiX)
        {
            Assert.Null(pollTimer, "the failed attempt's interrupts went through MSI-X, so nothing should have polled them");
        }
        else
        {
            Assert.NotNull(pollTimer, "the failed attempt's granted interrupts should have been polled: the platform cannot route MSI-X");
            Assert.False(pollTimer is not null && pollTimer.IsActive, "the failed attempt's poll timer should be off the platform timer");
        }

        Assert.True(FailingNvmeDriver.DmaAllocated, "the failed attempt got no DMA memory, so the page count proves nothing");
        long pagesLost = (long)FailingNvmeDriver.FreePagesBefore - (long)NvmeDriver.FreePagesAtProbe;
        Assert.True(pagesLost < PageLeakSlack,
            $"free pages fell by {pagesLost} across the failed attempt, which took {FailingNvmeDriver.LeakCheckPages} pages of DMA memory");
    }

    // The class match binds the same controller and is granted interrupts
    // again, the same way: MSI-X teardown left nothing that stops a second
    // Enable, and the entry masked in Probe is unmasked on Bound.
    private static void TestNvme_RebindGetsInterruptsAgain()
    {
        Assert.True(NvmeDriver.InterruptsGranted, NvmeDriver.ProbeFailure ?? "the NVMe driver's TryRequestInterrupts should be granted");
        Assert.Equal(s_expectMsiX, NvmeDriver.MsiXEnabledAfterRequest,
            s_expectMsiX ? "MSI-X should be enabled again for the next candidate" : "MSI-X should stay off: the platform cannot route it, so the kit polls");
        if (IsX64)
        {
            Assert.Equal(NvmeDriver.BoundVectorsAtProbe + 1, NvmeDriver.BoundVectorsAfterRequest, "the MSI-X entry should take one interrupt vector again");
        }

        if (!s_expectMsiX)
        {
            return;
        }

        Assert.True(NvmeDriver.Entry0MaskedInProbe, "MSI-X entry 0 should be programmed masked during Probe");
        MmioRegion? table = NvmeDriver.MsiXTable;
        PciDevice? function = FindFunction(s_expectedVendorId, s_expectedDeviceId);
        if (table is null || function is null)
        {
            Assert.Fail("the NVMe controller's MSI-X table did not map");
            return;
        }

        Assert.True((table.Read32(NvmeDriver.Entry0Control) & MsiXState.EntryMaskBit) == 0, "MSI-X entry 0 should be unmasked once the binding is Bound");
        Assert.True(((ushort)function.Command & BusMasterBit) != 0, "bus mastering, which an MSI-X message needs, should be on once the binding is Bound");
    }

    // The Identify completed during Probe, while the kit held the interrupt
    // back; after Bound the handler took the completion, from the MSI-X
    // message sent on unmask or from the first poll, and signalled the event.
    private static void TestNvme_CompletionInterruptAfterBound()
    {
        Assert.True(NvmeDriver.ControllerReady, NvmeDriver.ProbeFailure ?? "the NVMe controller never became ready");
        Assert.True(NvmeDriver.CompletedDuringProbe, "the Identify command should complete during Probe");
        Assert.False(NvmeDriver.HandlerRanBeforeBound, "the NVMe handler ran before Probe returned Bound");

        bool handled = WaitUntil(static () => NvmeDriver.CompletionHandled);
        Assert.True(handled, s_expectMsiX
            ? "the MSI-X message held back during Probe never reached the handler after Bound"
            : "the polled handler never found the completion after Bound");
        if (!handled)
        {
            return;
        }

        Assert.Equal(0u, NvmeDriver.CompletionStatus, "the Identify command should complete successfully");
        Assert.True(NvmeDriver.CompletionCommandId == NvmeDriver.SubmittedCommandId, "the completion should name the Identify command");

        DmaBuffer? identify = NvmeDriver.IdentifyData;
        ushort identifyVendorId = identify is null ? (ushort)0 : BinaryPrimitives.ReadUInt16LittleEndian(identify.Span);
        Assert.True(identifyVendorId == NvmeDriver.ExpectedIdentifyVendorId,
            $"the Identify data should start with vendor ID 0x{NvmeDriver.ExpectedIdentifyVendorId:X4}, starts with 0x{identifyVendorId:X4}");

        DeviceEvent? completionEvent = NvmeDriver.Event;
        Assert.True(completionEvent is not null && completionEvent.Wait(), "a wait on the event the handler signalled should return true");
    }

    // ==================== USB binding ====================

    // The sample driver's Probe as it ran on QEMU's mouse: the boot protocol
    // selected, idle reports asked for, the interrupt IN endpoint opened, and
    // a device descriptor read through the context while Probe ran.
    private static void TestUsbBind_BootMouseProbeSucceeded()
    {
        Assert.True(UsbBootMouseDriver.Probed, "the boot mouse driver was never probed");
        Assert.True(UsbBootMouseDriver.SetProtocolStatus == UsbTransferStatus.Success,
            $"SET_PROTOCOL to the boot protocol should succeed, ended {DescribeStatus(UsbBootMouseDriver.SetProtocolStatus)}");
        Assert.True(UsbBootMouseDriver.SetIdleStatus == UsbTransferStatus.Success,
            $"SET_IDLE should succeed, ended {DescribeStatus(UsbBootMouseDriver.SetIdleStatus)}");
        Assert.True(UsbBootMouseDriver.InterruptPipeOpened, "OpenInterruptIn on the mouse's interrupt IN endpoint should return true");

        UsbTransferResult? atProbe = UsbBootMouseDriver.ProbeDescriptorResult;
        Assert.True(atProbe is { } result && result.Status == UsbTransferStatus.Success && result.Length == UsbDescriptors.DeviceDescriptorLength,
            $"GET_DESCRIPTOR(device) during Probe should read {UsbDescriptors.DeviceDescriptorLength} bytes, ended {DescribeStatus(atProbe?.Status)} with {atProbe?.Length ?? 0}");
    }

    // A standard request through the bound context, from a thread after
    // Probe, with room for more than the descriptor: the device answers with
    // its 18 bytes and a short packet, and the result's length says how many
    // it sent.
    private static void TestUsbContext_ControlInReadsDeviceDescriptor()
    {
        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        if (context is null)
        {
            Assert.Fail("the boot mouse driver was never probed");
            return;
        }

        Span<byte> descriptor = stackalloc byte[64];
        UsbTransferResult result = context.ControlIn(UsbRequestKind.Standard, UsbRecipient.Device, UsbDescriptors.GetDescriptorRequest,
            UsbDescriptors.DeviceDescriptorValue, 0, descriptor);
        Assert.True(result.Status == UsbTransferStatus.Success, $"GET_DESCRIPTOR(device) should succeed, ended {DescribeStatus(result.Status)}");
        Assert.Equal(UsbDescriptors.DeviceDescriptorLength, result.Length, "GET_DESCRIPTOR(device) with a 64-byte buffer should report the 18 bytes the device sent");
        Assert.Equal((byte)UsbDescriptors.DeviceDescriptorLength, descriptor[0], "the descriptor's bLength");
        Assert.Equal(UsbDescriptors.DeviceDescriptorType, descriptor[UsbDescriptors.DescriptorTypeOffset], "the descriptor's bDescriptorType");

        ushort vendorId = BinaryPrimitives.ReadUInt16LittleEndian(descriptor.Slice(UsbDescriptors.VendorIdOffset));
        ushort productId = BinaryPrimitives.ReadUInt16LittleEndian(descriptor.Slice(UsbDescriptors.ProductIdOffset));
        Assert.True(vendorId == UsbDescriptors.QemuHidVendorId && productId == UsbDescriptors.QemuHidProductId,
            $"the device descriptor should name {UsbDescriptors.QemuHidVendorId:X4}:{UsbDescriptors.QemuHidProductId:X4}, names {vendorId:X4}:{productId:X4}");
    }

    // Endpoints and publications are handed out during Probe only, so a
    // bound context refuses them afterwards. What it describes is the mouse
    // and its interface, and it sits where the kit documents.
    private static void TestUsbContext_ProbeOnlyMembersThrowAfterProbe()
    {
        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        UsbInterface? mouse = FindInterface(HidClass, BootInterfaceSubclass, MouseProtocol, out UsbDevice? device);
        if (context is null || mouse is null || device is null)
        {
            Assert.Fail("the boot mouse driver was never probed");
            return;
        }

        UsbInterfaceInfo usbInterface = context.Interface;
        bool found = usbInterface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint);
        Assert.True(found, "the mouse interface's description should list its interrupt IN endpoint");
        Assert.True(ThrowsInvalidOperation(() => context.OpenInterruptIn(endpoint, static _ => { })), "OpenInterruptIn after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.TryOpenBulk(endpoint, out _)), "TryOpenBulk after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.CreateEvent()), "CreateEvent after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.TryCreateWorkItem(static () => { }, out _)), "TryCreateWorkItem after Probe should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.PublishMouse()), "PublishMouse after Probe should throw InvalidOperationException");
        Assert.True(context.IsPresent, "a bound interface whose device is on the bus is present");

        string expectedPath = ExpectedUsbPath(device, mouse);
        Assert.True(context.Path == expectedPath, $"the context's path {context.Path} should be {expectedPath}");

        UsbDeviceInfo info = context.Device;
        Assert.True(info.VendorId == UsbDescriptors.QemuHidVendorId && info.ProductId == UsbDescriptors.QemuHidProductId,
            $"the context should describe {UsbDescriptors.QemuHidVendorId:X4}:{UsbDescriptors.QemuHidProductId:X4}, describes {info.VendorId:X4}:{info.ProductId:X4}");
        Assert.Equal(device.Interfaces.Count, info.Interfaces.Count, "the context should describe every interface of the device");
        Assert.True(usbInterface.Number == mouse.Number && usbInterface.Class == HidClass && usbInterface.Subclass == BootInterfaceSubclass
            && usbInterface.Protocol == MouseProtocol, "the context should describe the mouse interface");
        Assert.True((endpoint.Address & 0x80) != 0 && endpoint.MaxPacketSize > 0 && endpoint.Interval > 0,
            $"the interrupt IN endpoint 0x{endpoint.Address:X2} should have the IN bit, a packet size and an interval");
    }

    // The device match asked, in each of its two Probes, for a bulk pipe on
    // the interface's interrupt endpoint and for an interrupt pipe on an
    // endpoint the interface lacks. Both returned false and opened nothing,
    // so its declines let each interface go on to the next candidate.
    private static void TestUsbContext_OpenRefusesOtherEndpoints()
    {
        Assert.Equal(2, UsbHidDeviceDriver.Probes, "the device match should be offered both of QEMU's HID interfaces, the mouse's and the tablet's");
        Assert.False(UsbHidDeviceDriver.InterruptEndpointMissing, "each HID interface should have an interrupt IN endpoint");
        Assert.False(UsbHidDeviceDriver.BulkOpenOfInterruptEndpointAccepted, "TryOpenBulk on an interrupt endpoint should return false");
        Assert.False(UsbHidDeviceDriver.InterruptOpenOfMissingEndpointAccepted, "OpenInterruptIn on an endpoint the interface lacks should return false");
        Assert.True(UsbBootMouseDriver.Probed && UsbTabletFailingDriver.Context is not null,
            "the interfaces should have gone on past the device match's declines");
    }

    // The mouse sent a report every 4 ms from the moment Probe opened its
    // endpoint. None reached the handler while Probe ran, and they do once
    // it is Bound. Then back to reports on change only, through the bound
    // context: a control request from a thread, after Probe.
    private static void TestUsbInterrupts_ReportsOnlyAfterBound()
    {
        Assert.Equal(0, UsbBootMouseDriver.CallsWhileProbing, "the report handler ran while Probe still ran");
        bool reported = WaitUntil(static () => UsbBootMouseDriver.HandlerCalls > 0);
        Assert.True(reported, "no report reached the handler once Bound, with the mouse sending one every 4 ms");
        Assert.True(UsbBootMouseDriver.LastReportLength >= MinimumBootReportLength,
            $"a boot mouse report holds at least {MinimumBootReportLength} bytes, the last one {UsbBootMouseDriver.LastReportLength}");

        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        if (context is null)
        {
            Assert.Fail("the boot mouse driver was never probed");
            return;
        }

        UsbTransferStatus status = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, UsbBootMouseDriver.SetIdleRequest,
            UsbBootMouseDriver.ReportOnChange, context.Interface.Number);
        Assert.True(status == UsbTransferStatus.Success, $"SET_IDLE back to reports on change should succeed, ended {DescribeStatus(status)}");
    }

    // The mouse the boot mouse driver published reached the mouse manager
    // when the binding became Bound: a report through it, here from a
    // thread, moves the pointer and sets the buttons.
    private static void TestUsbBind_PublishedMouseMovesPointer()
    {
        MouseReporter? mouse = UsbBootMouseDriver.Mouse;
        if (mouse is null)
        {
            Assert.Fail("PublishMouse in the boot mouse driver's Probe should return a reporter");
            return;
        }

        MouseManager.SetPosition(PointerStartX, PointerStartY);
        mouse.Report(UsbMouseDeltaX, UsbMouseDeltaY, 0, MouseButtons.Right);
        Assert.Equal(PointerStartX + UsbMouseDeltaX, MouseManager.X, "the report should move the pointer right by its X delta");
        Assert.Equal(PointerStartY + UsbMouseDeltaY, MouseManager.Y, "the report should move the pointer down by its Y delta");
        Assert.True(MouseManager.RightButton, "the report holds the right button");
        mouse.Report(0, 0, 0, MouseButtons.None);
        Assert.False(MouseManager.RightButton, "the next report releases the right button");
    }

    // ==================== USB no fall-through ====================

    // The tablet driver opened the tablet's interrupt IN endpoint, which the
    // host controller cannot close, then failed. The device match, which
    // opened nothing, had declined before it; the link driver's class and
    // subclass match and the class-only match, lower ranked, were never
    // offered the tablet after it, and either would have bound it. The
    // tablet stays without a driver.
    private static void TestUsbNoFallThrough_TabletOfferingEnds()
    {
        Assert.True(s_tabletFoundBeforePass, "no tablet interface was enumerated when the kernel was constructed");
        Assert.Null(s_tabletOwnerBeforePass, "no class driver should have bound the tablet interface before the pass");

        UsbInterface? tablet = FindInterface(HidClass, TabletSubclass, TabletProtocol, out UsbDevice? device);
        if (tablet is null || device is null)
        {
            Assert.Fail("no tablet interface enumerated");
            return;
        }

        Assert.True(UsbTabletFailingDriver.InterruptPipeOpened, "the tablet driver's OpenInterruptIn should return true");
        string order = ProbeLog.Describe(ExpectedUsbPath(device, tablet));
        Assert.True(order == ExpectedTabletProbeOrder, $"the tablet interface should be offered as {ExpectedTabletProbeOrder}, was {order}");
        Assert.False(ProbeLog.Recorded(UsbHidClassName), $"the class-only match should never have been offered the tablet, probes ran {ProbeLog.Describe()}");
        Assert.False(ProbeLog.Recorded(UsbTabletLinkDriver.Name), $"the link driver should never have been offered the tablet at boot, probes ran {ProbeLog.Describe()}");
        Assert.Null(tablet.Driver, "the tablet interface should be left without a driver");
        Assert.Null(tablet.DriverContext, "the tablet interface should keep no binding");
        Assert.Null(tablet.DriverName, "the tablet interface should have no owner");
    }

    // Teardown of the failed attempt: its context refuses control requests,
    // its mouse never reached the mouse manager, and the reports the tablet
    // keeps sending, one every 4 ms, all stop at the disarmed kit.
    private static void TestUsbTeardown_FailedAttemptInvalidated()
    {
        UsbDeviceContext? context = UsbTabletFailingDriver.Context;
        MouseReporter? mouse = UsbTabletFailingDriver.Mouse;
        UsbInterface? tablet = FindInterface(HidClass, TabletSubclass, TabletProtocol, out UsbDevice? device);
        if (context is null || mouse is null || tablet is null || device is null)
        {
            Assert.Fail("the tablet driver never got as far as publishing a mouse");
            return;
        }

        Assert.True(ThrowsInvalidOperation(() => context.ControlIn(UsbRequestKind.Standard, UsbRecipient.Device, UsbDescriptors.GetDescriptorRequest,
                UsbDescriptors.DeviceDescriptorValue, 0, new byte[UsbDescriptors.DeviceDescriptorLength])),
            "ControlIn on the failed attempt's context should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, UsbBootMouseDriver.SetIdleRequest,
                UsbBootMouseDriver.ReportOnChange, 0)),
            "ControlOut on the failed attempt's context should throw InvalidOperationException");

        MouseManager.SetPosition(PointerStartX, PointerStartY);
        mouse.Report(UsbMouseDeltaX, UsbMouseDeltaY, 0, MouseButtons.Left);
        Assert.Equal(PointerStartX, MouseManager.X, "a report through the failed attempt's mouse moved the pointer");
        Assert.Equal(PointerStartY, MouseManager.Y, "a report through the failed attempt's mouse moved the pointer");
        Assert.False(MouseManager.LeftButton, "a report through the failed attempt's mouse reached the buttons");

        Assert.True(UsbTabletFailingDriver.SetIdleStatus == UsbTransferStatus.Success,
            $"SET_IDLE to the tablet should succeed, or it sends no report for the kit to hold back; ended {DescribeStatus(UsbTabletFailingDriver.SetIdleStatus)}");
        SpinMilliseconds(TabletReportWatchMilliseconds);
        Assert.Equal(0, UsbTabletFailingDriver.HandlerCalls, "the failed attempt's report handler ran");

        // Quiet for the rest of the run, through the USB stack itself: the
        // seam refuses, the attempt being over.
        device.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, UsbBootMouseDriver.SetIdleRequest, UsbBootMouseDriver.ReportOnChange, tablet.Number);
    }

    // ==================== USB hot-plug ====================

    // QEMU moves the mouse with the left button held, then releases it, and
    // each report travels the whole way: the xHCI interrupt, or on GICv2 the
    // hot-plug thread's poll every 250 ms, the kit's armed trampoline, the
    // boot mouse driver's handler, its published mouse and the mouse
    // manager. The driver's own sums of what it passed on show the movement
    // came through it, not through x64's PS/2 mouse.
    private static void TestUsbHotPlug_PointerMoveReachesBootMouseDriver()
    {
        Assert.True(UsbManager.IsHotPlugRunning, "the USB hot-plug thread should run: the scheduler switches threads on every cell of this suite");
        CheckPointerMoveThroughBootMouse();
    }

    // The mouse is pulled out while a work item of its binding runs and its
    // left button is held, as in a drag. The kit's unplug runs its steps in
    // order: by the time the driver's Remove runs, once, the context reports
    // the device gone and the published mouse is withdrawn, and the work
    // item that was running has returned: the kit waited for it.
    private static void TestUsbUnplug_RemoveRunsOnceAfterWithdrawal()
    {
        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        MouseReporter? mouse = UsbBootMouseDriver.Mouse;
        DeviceWorkItem? workItem = UsbBootMouseDriver.WorkItem;
        DeviceEvent? deviceEvent = UsbBootMouseDriver.Event;
        UsbInterface? usbInterface = FindInterface(HidClass, BootInterfaceSubclass, MouseProtocol, out _);
        if (context is null || mouse is null || workItem is null || deviceEvent is null || usbInterface is null)
        {
            Assert.Fail("the boot mouse driver's binding lacks its context, mouse, work item, event or interface");
            return;
        }

        s_unpluggedMouseContext = context;
        s_unpluggedMouse = mouse;
        s_unpluggedWorkItem = workItem;
        s_unpluggedEvent = deviceEvent;
        s_unpluggedMouseInterface = usbInterface;

        // Pressed through QEMU, so the mouse's own reports hold it: the
        // release never comes, since the mouse is gone before it could.
        TR.RequestUsbPointerMove(MouseUsbIndex, PointerMoveX, PointerMoveY, LeftButtonBit);
        bool pressed = WaitUntil(static () => MouseManager.LeftButton, HotPlugWaitMilliseconds);
        Assert.True(pressed, "QEMU's left button press never reached the mouse manager");

        UsbBootMouseDriver.HoldWorkAcrossUnplug();
        Assert.True(workItem.Schedule(), "the bound binding's work item should accept a Schedule");
        bool holding = WaitUntil(static () => UsbBootMouseDriver.WorkHolding);
        Assert.True(holding, "the work item never started on the driver-work thread");

        TR.RequestUsbDeviceUnplug(MouseUsbIndex);
        bool removed = WaitUntil(static () => UsbBootMouseDriver.RemoveCalls > 0, HotPlugWaitMilliseconds);
        Assert.True(removed, "the boot mouse driver's Remove never ran after the mouse was pulled out");
        if (!removed)
        {
            return;
        }

        Assert.Equal(1, UsbBootMouseDriver.RemoveCalls, "Remove should run once for the binding");
        Assert.True(UsbBootMouseDriver.RemovedContext == context, "Remove should be handed the binding's context");
        Assert.False(UsbBootMouseDriver.PresentAtRemove, "the context should report the mouse gone by the time Remove runs");
        Assert.True(UsbBootMouseDriver.MouseWithdrawnAtRemove, "the published mouse should be withdrawn before Remove runs");
        Assert.False(UsbBootMouseDriver.WorkRunningAtRemove, "Remove ran while a work item of the binding still ran");
        long finishedAt = UsbBootMouseDriver.WorkFinishedAt;
        Assert.True(finishedAt != 0 && UsbBootMouseDriver.RemoveStartedAt >= finishedAt,
            "Remove should wait for the work item that was running when the mouse left, and started before it returned");
    }

    // The mouse the driver published left the mouse manager, which cleared
    // its handler and released the left button the mouse held, whose
    // release never came, and the kit silenced it: a report the driver still
    // makes through its stale reporter moves nothing.
    private static void TestUsbUnplug_MouseLeavesMouseManager()
    {
        MouseReporter? mouse = s_unpluggedMouse;
        if (mouse is null)
        {
            Assert.Fail("the unplug cell captured no mouse");
            return;
        }

        PublishedMouse published = mouse.Device;
        Assert.False(MouseManager.IsRegistered(published), "the unplugged mouse's published mouse is still registered with the mouse manager");
        Assert.True(published.IsWithdrawn, "the kit should mark the unplugged mouse's published mouse withdrawn");
        Assert.Null(published.OnMouseEvent, "the mouse manager should clear the handler of a mouse it let go of");
        Assert.False(MouseManager.LeftButton, "the left button the mouse held when it was pulled out should be released");

        MouseManager.SetPosition(PointerStartX, PointerStartY);
        mouse.Report(UsbMouseDeltaX, UsbMouseDeltaY, 0, MouseButtons.Right);
        Assert.Equal(PointerStartX, MouseManager.X, "a report through the unplugged mouse's reporter moved the pointer");
        Assert.Equal(PointerStartY, MouseManager.Y, "a report through the unplugged mouse's reporter moved the pointer");
        Assert.False(MouseManager.RightButton, "a report through the unplugged mouse's reporter reached the buttons");
    }

    // The binding is over. The context reports the device gone and answers
    // Disconnected to control requests rather than throwing: a driver's
    // thread may still hold it. The Probe-only members throw, and the
    // interface the mouse had keeps no binding.
    private static void TestUsbUnplug_StaleContextAnswersDisconnected()
    {
        UsbDeviceContext? context = s_unpluggedMouseContext;
        UsbInterface? usbInterface = s_unpluggedMouseInterface;
        if (context is null || usbInterface is null)
        {
            Assert.Fail("the unplug cell captured no context");
            return;
        }

        Assert.False(context.IsPresent, "the context of a mouse pulled out should not be present");
        Assert.True(context.State == DeviceContextState.Removed, "the context of a mouse pulled out should end Removed");

        Span<byte> descriptor = stackalloc byte[UsbDescriptors.DeviceDescriptorLength];
        UsbTransferResult result = context.ControlIn(UsbRequestKind.Standard, UsbRecipient.Device, UsbDescriptors.GetDescriptorRequest,
            UsbDescriptors.DeviceDescriptorValue, 0, descriptor);
        Assert.True(result.Status == UsbTransferStatus.Disconnected && result.Length == 0,
            $"ControlIn on an unplugged context should return Disconnected with no data, returned {DescribeStatus(result.Status)} with {result.Length}");

        UsbTransferStatus status = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, UsbBootMouseDriver.SetIdleRequest,
            UsbBootMouseDriver.ReportOnChange, context.Interface.Number);
        Assert.True(status == UsbTransferStatus.Disconnected, $"ControlOut on an unplugged context should return Disconnected, returned {DescribeStatus(status)}");

        Assert.True(ThrowsInvalidOperation(() => context.PublishMouse()), "PublishMouse on an unplugged context should throw InvalidOperationException");
        Assert.True(ThrowsInvalidOperation(() => context.CreateEvent()), "CreateEvent on an unplugged context should throw InvalidOperationException");
        Assert.Null(usbInterface.DriverContext, "the interface of a mouse pulled out should keep no binding");
        Assert.Null(usbInterface.Driver, "the interface of a mouse pulled out should have no class driver");
    }

    // What the binding scheduled and waited on is cancelled: its work item
    // refuses a Schedule and never runs again, and a Wait on its event
    // returns false at once. Signalled first, a Wait on an event nothing
    // cancelled would consume the signal and return true, never block.
    private static void TestUsbUnplug_WorkItemsAndEventsCancelled()
    {
        DeviceWorkItem? workItem = s_unpluggedWorkItem;
        DeviceEvent? deviceEvent = s_unpluggedEvent;
        if (workItem is null || deviceEvent is null)
        {
            Assert.Fail("the unplug cell captured no work item or event");
            return;
        }

        int runs = UsbBootMouseDriver.WorkRuns;
        Assert.False(workItem.Schedule(), "the work item of a binding whose mouse was pulled out should refuse a Schedule");
        SpinMilliseconds(LockHoldMilliseconds);
        Assert.Equal(runs, UsbBootMouseDriver.WorkRuns, "the work item of a binding whose mouse was pulled out ran again");

        deviceEvent.Signal();
        Assert.False(deviceEvent.Wait(), "a Wait on the event of a binding whose mouse was pulled out should return false");
    }

    // The hot-plug thread rebuilt the device list once the mouse was gone:
    // its interface is no longer listed, nor any interface the boot mouse
    // driver owns.
    private static void TestUsbUnplug_InterfaceLeavesDeviceList()
    {
        string? path = s_unpluggedMouseContext?.Path;
        if (path is null)
        {
            Assert.Fail("the unplug cell captured no context");
            return;
        }

        bool gone = WaitUntil(() => FindRecord(DriverCore.Devices, path) is null, HotPlugWaitMilliseconds);
        Assert.True(gone, $"{path} should leave the device list once the mouse is pulled out");
        Assert.Null(FindUsbRecord(HidClass, BootInterfaceSubclass, MouseProtocol), "no boot mouse interface should be listed while the mouse is out");
    }

    // The mouse plugged back in reaches the kit on the hot-plug thread,
    // after the built-ins declined it, and goes through the same ranking as
    // at boot: the device match and the declining boot mouse match, then the
    // boot mouse driver, from a new instance the factory created. Its Probe
    // saw no report either, and the old binding's Remove did not run again.
    private static void TestUsbReplug_NewDriverBindsOnHotPlugThread()
    {
        int instancesBefore = UsbBootMouseDriver.Instances;
        int probesBefore = ProbeLog.Count;
        TR.RequestUsbDevicePlug(MouseUsbIndex);
        bool bound = WaitUntil(static () => IsOwnedBy(FindUsbRecord(HidClass, BootInterfaceSubclass, MouseProtocol), UsbBootMouseDriver.Name),
            HotPlugWaitMilliseconds);
        Assert.True(bound, $"the mouse plugged back in should be listed as owned by {UsbBootMouseDriver.Name}");
        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        if (!bound || context is null)
        {
            return;
        }

        Assert.Equal(instancesBefore + 1, UsbBootMouseDriver.Instances, "the factory should create one new driver for the mouse plugged back in");
        Assert.True(context != s_unpluggedMouseContext, "the mouse plugged back in should be bound through a new context");
        Assert.True(context.IsPresent && context.State == DeviceContextState.Bound, "the new binding's context should be present and Bound");
        Assert.True(UsbBootMouseDriver.ProbeThreadId != s_bootThreadId && !UsbBootMouseDriver.ProbeOnIdleThread,
            "the mouse plugged back in should be probed on the hot-plug thread, not the boot thread");

        string order = ProbeLog.DescribeSince(probesBefore, context.Path);
        Assert.True(order == ExpectedMouseProbeOrder, $"the mouse plugged back in should be offered as {ExpectedMouseProbeOrder}, was {order}");
        Assert.Equal(0, UsbBootMouseDriver.CallsWhileProbing, "the report handler ran while the new Probe still ran");
        Assert.Equal(1, UsbBootMouseDriver.RemoveCalls, "plugging the mouse back in ran Remove again");
    }

    // The new binding's mouse joined the mouse manager, which wired its
    // handler, while the old one stays out; the device list names the new
    // interface's owner, IDs and class at the new context's path.
    private static void TestUsbReplug_NewMouseRegisteredAndListed()
    {
        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        MouseReporter? mouse = UsbBootMouseDriver.Mouse;
        MouseReporter? old = s_unpluggedMouse;
        if (context is null || mouse is null || old is null || mouse == old)
        {
            Assert.Fail("the mouse plugged back in published no new mouse");
            return;
        }

        Assert.True(MouseManager.IsRegistered(mouse.Device), "the new binding's mouse should be registered with the mouse manager");
        Assert.NotNull(mouse.Device.OnMouseEvent, "the mouse manager should wire the new mouse's handler");
        Assert.False(MouseManager.IsRegistered(old.Device), "the unplugged binding's mouse came back into the mouse manager");

        DeviceRecord? listed = FindRecord(DriverCore.Devices, context.Path);
        if (listed is not { } record)
        {
            Assert.Fail($"the device list has no entry for {context.Path}");
            return;
        }

        Assert.True(record.DriverName == UsbBootMouseDriver.Name, $"{context.Path} should be listed as owned by {UsbBootMouseDriver.Name}, is {record.DriverName ?? "nothing"}");
        Assert.True(record.VendorId == UsbDescriptors.QemuHidVendorId && record.DeviceId == UsbDescriptors.QemuHidProductId,
            $"{context.Path} is listed with the wrong IDs");
        Assert.True(record.Class == HidClass && record.Subclass == BootInterfaceSubclass && record.Protocol == MouseProtocol,
            $"{context.Path} is listed with the wrong class");
    }

    // Back to reports on change through the new context, then QEMU's
    // pointer movement reaches the mouse manager through the new binding as
    // it did through the first.
    private static void TestUsbReplug_ReportsReachNewBinding()
    {
        UsbDeviceContext? context = UsbBootMouseDriver.Context;
        if (context is null)
        {
            Assert.Fail("the boot mouse driver was never probed");
            return;
        }

        UsbTransferStatus status = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, UsbBootMouseDriver.SetIdleRequest,
            UsbBootMouseDriver.ReportOnChange, context.Interface.Number);
        Assert.True(status == UsbTransferStatus.Success, $"SET_IDLE back to reports on change should succeed through the new context, ended {DescribeStatus(status)}");
        CheckPointerMoveThroughBootMouse();
    }

    // The tablet, left without a driver by the pass, is pulled out: no
    // driver has anything to let go of, and the hot-plug thread still drops
    // its interface from the device list.
    private static void TestUsbTabletUnplug_UnboundInterfaceLeavesDeviceList()
    {
        UsbInterface? tablet = FindInterface(HidClass, TabletSubclass, TabletProtocol, out UsbDevice? device);
        if (tablet is null || device is null)
        {
            Assert.Fail("no tablet interface enumerated");
            return;
        }

        Assert.Null(tablet.DriverName, "the tablet should still have no driver before it is pulled out");
        string path = ExpectedUsbPath(device, tablet);
        TR.RequestUsbDeviceUnplug(TabletUsbIndex);
        bool gone = WaitUntil(() => FindRecord(DriverCore.Devices, path) is null, HotPlugWaitMilliseconds);
        Assert.True(gone, $"{path} should leave the device list once the tablet is pulled out");
        Assert.Equal(0, UsbTabletLinkDriver.RemoveCalls, "no driver bound the tablet, so no Remove should run");
    }

    // Plugged back in, the tablet is offered on the hot-plug thread to the
    // device match and to the tablet driver, which now declines having
    // opened nothing, so the offering goes on, unlike at boot, to the link
    // driver, which binds. Its link joins the network manager after every
    // device already there: the primary one stays primary, or, with none,
    // the link becomes it.
    private static void TestUsbTabletReplug_LinkDriverBindsAfterDeclines()
    {
        s_networkDevicesBeforeLink = NetworkManager.DeviceCount;
        s_primaryBeforeLink = NetworkManager.Primary;
        s_primaryAddressBeforeLink = s_primaryBeforeLink.MacAddress;

        int probesBefore = ProbeLog.Count;
        int expectedDevices = s_networkDevicesBeforeLink + 1;
        TR.RequestUsbDevicePlug(TabletUsbIndex);
        bool bound = WaitUntil(() => IsOwnedBy(FindUsbRecord(HidClass, TabletSubclass, TabletProtocol), UsbTabletLinkDriver.Name)
            && NetworkManager.DeviceCount == expectedDevices, HotPlugWaitMilliseconds);
        Assert.True(bound, $"the tablet plugged back in should be owned by {UsbTabletLinkDriver.Name} and its link registered, probes ran {ProbeLog.Describe()}");
        UsbDeviceContext? context = UsbTabletLinkDriver.Context;
        NetworkLink? link = UsbTabletLinkDriver.Link;
        if (!bound || context is null || link is null)
        {
            return;
        }

        string order = ProbeLog.DescribeSince(probesBefore, context.Path);
        Assert.True(order == ExpectedTabletReplugProbeOrder, $"the tablet plugged back in should be offered as {ExpectedTabletReplugProbeOrder}, was {order}");
        Assert.Equal(2, UsbTabletFailingDriver.Probes, "the tablet driver should be offered the tablet twice, at boot and plugged back in");
        Assert.Equal(1, UsbTabletLinkDriver.Instances, "the link driver's factory should run once");
        Assert.True(UsbTabletLinkDriver.ProbeThreadId != s_bootThreadId && !UsbTabletLinkDriver.ProbeOnIdleThread,
            "the tablet plugged back in should be probed on the hot-plug thread, not the boot thread");

        NetworkAdapter adapter = NetworkManager.GetAdapter(s_networkDevicesBeforeLink);
        MACAddress? registered = adapter.MacAddress;
        Assert.True(registered is not null && registered.Equals(link.Address),
            $"the device registered last should be the link, {link.Address}, is {registered?.ToString() ?? "none"}");
        string expectedName = $"{UsbTabletLinkDriver.Name} {context.Path}";
        Assert.True(adapter.Name == expectedName, $"the link should be named {expectedName}, is {adapter.Name ?? "unnamed"}");
        Assert.True(adapter.Ready, "the link should be ready once delivered");
        if (s_networkDevicesBeforeLink > 0)
        {
            Assert.True(NetworkManager.Primary == s_primaryBeforeLink, "the device that was primary before the link joined should stay primary");
        }
        else
        {
            Assert.True(NetworkManager.Primary == adapter, "with no other network device, the link should be primary");
        }
    }

    // The link is configured and carries a frame, then the tablet is pulled
    // out. The link leaves the network manager before Remove runs, the
    // primary device is what it was before the link joined, or none when
    // the link was primary, and the stack forgets the link's addresses and
    // configuration. A send the stack still makes through the device it held
    // fails before the driver sees it, and the handle to it names nothing.
    private static void TestUsbTabletUnplug_LinkLeavesNetworkManager()
    {
        NetworkLink? link = UsbTabletLinkDriver.Link;
        int index = s_networkDevicesBeforeLink;
        NetworkAdapter adapter = NetworkManager.GetAdapter(index);
        if (link is null || !adapter.IsValid)
        {
            Assert.Fail("the link driver published no link to pull out");
            return;
        }

        PublishedNetworkDevice device = link.Device;
        // A private network QEMU's user-mode NIC does not use.
        Address4 address = new(192, 168, 77, 2);
        Assert.True(IPConfig.Enable(adapter, address, new Address4(255, 255, 255, 0), Address4.Zero), "configuring the link should succeed");
        Assert.True(adapter.IPConfig is { } config && config.Address.Equals(address), "configuring the link should record its address");
        Assert.NotNull(NetworkStack.LinkLocalOf(device), "configuring the link should map a link-local IPv6 address to it");

        byte[] frame = new byte[MinimumFrameLength];
        int transmits = UsbTabletLinkDriver.Transmits;
        Assert.True(device.Send(frame, frame.Length), "a send through the bound link should reach its transmit handler");
        Assert.Equal(transmits + 1, UsbTabletLinkDriver.Transmits, "a send through the bound link should reach its transmit handler once");

        s_staleLinkAdapter = adapter;
        s_staleLinkDevice = device;
        TR.RequestUsbDeviceUnplug(TabletUsbIndex);
        bool removed = WaitUntil(() => UsbTabletLinkDriver.RemoveCalls > 0 && NetworkManager.DeviceCount == index, HotPlugWaitMilliseconds);
        Assert.True(removed, "the link driver's Remove never ran, or the link stayed registered, after the tablet was pulled out");
        if (!removed)
        {
            return;
        }

        Assert.Equal(1, UsbTabletLinkDriver.RemoveCalls, "Remove should run once for the binding");
        Assert.False(UsbTabletLinkDriver.PresentAtRemove, "the context should report the tablet gone by the time Remove runs");
        Assert.True(UsbTabletLinkDriver.LinkWithdrawnAtRemove, "the link should be withdrawn before Remove runs");
        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            MACAddress? other = NetworkManager.GetAdapter(i).MacAddress;
            Assert.False(other is not null && other.Equals(link.Address), $"network device {i} still has the unplugged link's address {link.Address}");
        }

        // Through the primary shortcuts too, which read the primary device
        // itself rather than a handle to it.
        MACAddress? primaryAddress = NetworkManager.MacAddress;
        if (index > 0)
        {
            Assert.True(NetworkManager.Primary == s_primaryBeforeLink && primaryAddress is not null && primaryAddress.Equals(s_primaryAddressBeforeLink),
                "the device that was primary before the link joined should still be primary once the link left");
        }
        else
        {
            Assert.False(NetworkManager.Primary.IsValid, "with the link, which was primary, gone and no other device, there should be no primary device");
            Assert.Null(primaryAddress, $"with no network device left, there should be no primary MAC address, is {primaryAddress?.ToString() ?? "none"}");
            Assert.Null(NetworkManager.Name, "with no network device left, there should be no primary device name");
        }

        Assert.True(device.IsWithdrawn && !device.Ready, "the kit should mark the unplugged link withdrawn");
        int transmitsBefore = UsbTabletLinkDriver.Transmits;
        Assert.False(device.Send(frame, frame.Length), "a send through the unplugged link should fail");
        Assert.Equal(transmitsBefore, UsbTabletLinkDriver.Transmits, "a send through the unplugged link reached its transmit handler");
        Assert.Null(device.OnPacketReceived, "the stack should stop taking frames from the unplugged link");

        Assert.False(adapter.IsValid, "a handle to the unplugged link should name no device");
        Assert.Null(adapter.Name, "a handle to the unplugged link should have no name");
        Assert.Equal(-1, adapter.Index, "a handle to the unplugged link should have no index");
        Assert.Null(NetworkStack.LinkLocalOf(device), "the stack still maps the unplugged link's addresses to it");
        Assert.Null(IPConfig.Get(device), "the stack still holds the unplugged link's configuration");
    }

    // Plugged back in once more, the tablet gets a new link, which takes the
    // index the first one had. The handle to the first one still names no
    // device: an index alone would name the new link.
    private static void TestUsbTabletReplug_StaleAdapterNamesNoNewLink()
    {
        if (s_staleLinkDevice is null)
        {
            Assert.Fail("the unplug cell captured no link");
            return;
        }

        int index = s_networkDevicesBeforeLink;
        TR.RequestUsbDevicePlug(TabletUsbIndex);
        bool bound = WaitUntil(() => UsbTabletLinkDriver.Instances == 2 && IsOwnedBy(FindUsbRecord(HidClass, TabletSubclass, TabletProtocol), UsbTabletLinkDriver.Name)
            && NetworkManager.DeviceCount == index + 1, HotPlugWaitMilliseconds);
        Assert.True(bound, $"the tablet plugged back in again should be owned by {UsbTabletLinkDriver.Name} through a new link");
        NetworkLink? link = UsbTabletLinkDriver.Link;
        if (!bound || link is null)
        {
            return;
        }

        NetworkAdapter fresh = NetworkManager.GetAdapter(index);
        MACAddress? registered = fresh.MacAddress;
        Assert.True(link.Device != s_staleLinkDevice, "the tablet plugged back in again should publish a new link");
        Assert.True(registered is not null && registered.Equals(link.Address),
            $"the new link should take the index the first one had, {index}, where {registered?.ToString() ?? "nothing"} is");
        Assert.False(s_staleLinkAdapter.IsValid, "a handle to the unplugged link names the link that took its index");
        Assert.Null(s_staleLinkAdapter.Name, "a handle to the unplugged link names the link that took its index");
        Assert.True(s_staleLinkAdapter != fresh, "a handle to the unplugged link equals one to the link that took its index");
        Assert.Equal(index, fresh.Index, "the new link's handle should report the index it sits at");
    }

    // A send the stack makes from a thread can be preempted after the
    // link's check that it carries traffic and before the transmit handler,
    // and meanwhile the USB hot-plug thread can withdraw the link and run the
    // driver's Remove. A tablet pulled out meets a sender there only by
    // chance, so this cell makes the race on purpose: a thread sends back to
    // back through a link, and this one withdraws the link each time the
    // scheduler hands it the CPU back, wherever the timer stopped the
    // sender, then puts a new link in its place, round after round. Once
    // Withdraw returned, the transmit handler must not run for that link.
    private static void TestLinkWithdraw_SendInFlightNeverTransmits()
    {
        UsbDeviceContext? context = UsbTabletLinkDriver.Context;
        if (context is null)
        {
            Assert.Fail("the link driver was never probed, so no context can name the race's links");
            return;
        }

        MACAddress address = new([0x02, 0x00, 0x00, 0x00, 0x7B, 0x01]);
        s_raceStop = false;
        s_raceLink = null;
        SysThread sender = new(SendBackToBack);
        sender.Start();

        int rounds = 0;
        while (rounds < LinkRaceRounds)
        {
            PublishedNetworkDevice link = new(context, address, CountRaceTransmit);
            link.Initialize();
            int transmits = Volatile.Read(ref s_raceTransmits);
            s_raceLink = link;

            // The sender runs once the scheduler takes the CPU from this
            // thread, and this one runs again once the scheduler takes it
            // from the sender: at a timer tick, wherever the sender was.
            if (!WaitUntil(() => Volatile.Read(ref s_raceTransmits) > transmits))
            {
                break;
            }

            link.Withdraw();
            rounds++;
        }

        s_raceStop = true;
        bool stopped = sender.Join(RaceThreadJoinMilliseconds);
        Assert.Equal(LinkRaceRounds, rounds, "the sender thread stopped reaching the transmit handler of a live link");
        Assert.True(stopped, "the sender thread never returned");
        Assert.Equal(0, Volatile.Read(ref s_transmitsAfterWithdraw),
            "a send that was preempted before the transmit handler reached it after the link was withdrawn");
    }

    // The same race the other way: a driver's thread delivers frames back
    // to back through a link, and this one withdraws the link each time the
    // scheduler hands it the CPU back, wherever the timer stopped the
    // deliverer, including between Deliver's first check and the stack. The
    // stack lets go of the link only after it was withdrawn, so its receive
    // handler, which this cell's stands in for, is still set then. Once
    // Withdraw returned, no frame delivered through that link may reach it.
    private static void TestLinkWithdraw_DeliverInFlightNeverReachesStack()
    {
        UsbDeviceContext? context = UsbTabletLinkDriver.Context;
        if (context is null)
        {
            Assert.Fail("the link driver was never probed, so no context can name the race's links");
            return;
        }

        MACAddress address = new([0x02, 0x00, 0x00, 0x00, 0x7B, 0x02]);
        s_raceStop = false;
        s_raceLink = null;
        SysThread deliverer = new(DeliverBackToBack);
        deliverer.Start();

        int rounds = 0;
        while (rounds < LinkRaceRounds)
        {
            PublishedNetworkDevice link = new(context, address, RefuseFrame);
            link.OnPacketReceived = CountRaceFrame;
            link.Initialize();
            int frames = Volatile.Read(ref s_raceFrames);
            s_raceLink = link;
            if (!WaitUntil(() => Volatile.Read(ref s_raceFrames) > frames))
            {
                break;
            }

            link.Withdraw();
            rounds++;
        }

        s_raceStop = true;
        bool stopped = deliverer.Join(RaceThreadJoinMilliseconds);
        Assert.Equal(LinkRaceRounds, rounds, "the delivering thread stopped reaching the receive handler of a live link");
        Assert.True(stopped, "the delivering thread never returned");
        Assert.Equal(0, Volatile.Read(ref s_framesAfterWithdraw),
            "a frame whose delivery was preempted before the stack reached it after the link was withdrawn");
    }

    // The stack picks the address a packet leaves from by walking the
    // configured interfaces, from whatever thread sends, and the USB
    // hot-plug thread takes an unplugged link's configuration out of that
    // list. A thread looks a source address up here over and over, a walk
    // that passes the first of two configurations this cell adds and stops
    // at the second, while this one adds a third configuration and takes it
    // out again each time the scheduler hands it the CPU back. No lookup
    // may fail, or find another address.
    private static void TestLinkWithdraw_RouteLookupSurvivesRemoval()
    {
        UsbDeviceContext? context = UsbTabletLinkDriver.Context;
        if (context is null)
        {
            Assert.Fail("the link driver was never probed, so no context can name the race's devices");
            return;
        }

        // Private networks nothing else in the suite uses. The devices only
        // key the configurations: they are never delivered, so they carry
        // nothing.
        Address4 mask = new(255, 255, 255, 0);
        PublishedNetworkDevice passed = new(context, new MACAddress([0x02, 0x00, 0x00, 0x00, 0x7C, 0x01]), RefuseFrame);
        PublishedNetworkDevice matched = new(context, new MACAddress([0x02, 0x00, 0x00, 0x00, 0x7C, 0x02]), RefuseFrame);
        PublishedNetworkDevice toggled = new(context, new MACAddress([0x02, 0x00, 0x00, 0x00, 0x7C, 0x03]), RefuseFrame);
        IPConfig passedConfig = new(new Address4(192, 168, 79, 1), mask, Address4.Zero);
        IPConfig matchedConfig = new(new Address4(192, 168, 80, 1), mask, Address4.Zero);
        IPConfig toggledConfig = new(new Address4(192, 168, 81, 1), mask, Address4.Zero);
        IPConfig.Set(passed, passedConfig);
        IPConfig.Set(matched, matchedConfig);

        s_routeDestination = new Address4(192, 168, 80, 2);
        s_routeSource = matchedConfig.Address;
        s_routeLookupFailures = 0;
        s_routeLookupError = null;
        s_raceStop = false;
        SysThread walker = new(LookUpSourcesBackToBack);
        walker.Start();

        int rounds = 0;
        while (rounds < LinkRaceRounds)
        {
            int lookups = Volatile.Read(ref s_routeLookups);
            if (!WaitUntil(() => Volatile.Read(ref s_routeLookups) > lookups))
            {
                break;
            }

            if (rounds % 2 == 0)
            {
                IPConfig.Set(toggled, toggledConfig);
            }
            else
            {
                IPConfig.Remove(toggled);
            }

            rounds++;
        }

        s_raceStop = true;
        bool stopped = walker.Join(RaceThreadJoinMilliseconds);
        IPConfig.Remove(toggled);
        IPConfig.Remove(matched);
        IPConfig.Remove(passed);

        Assert.Equal(LinkRaceRounds, rounds, "the lookup thread stopped looking source addresses up");
        Assert.True(stopped, "the lookup thread never returned");
        Assert.Equal(0, Volatile.Read(ref s_routeLookupFailures),
            $"a source address lookup failed while a configuration was added or taken out: {s_routeLookupError ?? "it found another address"}");
        Assert.Null(IPConfig.Get(toggled), "the configuration the cell added and took out is still recorded");
    }

    /// <summary>
    /// The send race's thread: sends a frame through the race's current
    /// link, back to back, until the cell stops it. A link the cell withdrew
    /// stays the one it sends through until the cell puts a new one there.
    /// </summary>
    private static void SendBackToBack()
    {
        byte[] frame = new byte[MinimumFrameLength];
        while (!s_raceStop)
        {
            if (s_raceLink is { } link)
            {
                s_sendingLink = link;
                _ = link.Send(frame, frame.Length);
            }
        }
    }

    /// <summary>
    /// The race links' transmit handler, on the sender thread with
    /// interrupts masked: counts the frame, and counts it as a failure when
    /// the link it went through was withdrawn already. With interrupts
    /// masked no other thread runs, so the cell cannot withdraw the link
    /// between this check and the handler's return.
    /// </summary>
    private static bool CountRaceTransmit(ReadOnlySpan<byte> frame)
    {
        if (s_sendingLink is { IsWithdrawn: true })
        {
            s_transmitsAfterWithdraw++;
        }

        Volatile.Write(ref s_raceTransmits, s_raceTransmits + 1);
        return true;
    }

    /// <summary>
    /// The delivery race's thread: delivers a frame through the race's
    /// current link, back to back, until the cell stops it, as a driver's
    /// receive work item does.
    /// </summary>
    private static void DeliverBackToBack()
    {
        byte[] frame = new byte[MinimumFrameLength];
        while (!s_raceStop)
        {
            if (s_raceLink is { } link)
            {
                s_deliveringLink = link;
                link.Deliver(frame);
            }
        }
    }

    /// <summary>
    /// The delivery race links' receive handler, standing in for the
    /// stack's, on the delivering thread with interrupts masked: counts the
    /// frame, and counts it as a failure when the link it came through was
    /// withdrawn already.
    /// </summary>
    private static void CountRaceFrame(byte[] data, int length)
    {
        if (s_deliveringLink is { IsWithdrawn: true })
        {
            s_framesAfterWithdraw++;
        }

        Volatile.Write(ref s_raceFrames, s_raceFrames + 1);
    }

    /// <summary>The transmit handler of the race links nothing sends through.</summary>
    private static bool RefuseFrame(ReadOnlySpan<byte> frame) => false;

    /// <summary>
    /// The route race's thread: looks up the address a packet to the cell's
    /// destination leaves from, back to back, until the cell stops it, and
    /// counts every lookup that throws or finds another address.
    /// </summary>
    private static void LookUpSourcesBackToBack()
    {
        Address4? destination = s_routeDestination;
        Address? expected = s_routeSource;
        if (destination is null || expected is null)
        {
            s_routeLookupError = "the cell set no destination";
            s_routeLookupFailures++;
            return;
        }

        while (!s_raceStop)
        {
            try
            {
                Address? found = IPConfig.FindNetwork(destination);
                if (found is null || !found.Equals(expected))
                {
                    s_routeLookupFailures++;
                }
            }
            catch (Exception exception)
            {
                s_routeLookupError ??= exception.Message;
                s_routeLookupFailures++;
            }

            Volatile.Write(ref s_routeLookups, s_routeLookups + 1);
        }
    }

    // QEMU's keyboard presents QEMU's HID IDs, which the device match
    // registration matches, and a HID interface, which the class match
    // does; the built-in keyboard driver took it at boot, so the pass
    // offered it to neither. Pulled out, the built-in lets go and the
    // hot-plug thread drops it from the device list.
    private static void TestUsbKeyboardUnplug_BuiltInLetsGo()
    {
        Assert.True(s_keyboardOwnerBeforePass == UsbKeyboardName,
            $"the built-in keyboard driver should own the keyboard before the pass, {s_keyboardOwnerBeforePass ?? "nothing"} did");
        string? bootPath = s_keyboardPathBeforePass;
        UsbInterface? keyboard = FindInterface(HidClass, BootInterfaceSubclass, KeyboardProtocol, out UsbDevice? device);
        if (bootPath is null || keyboard is null || device is null)
        {
            Assert.Fail("no HID boot keyboard interface enumerated");
            return;
        }

        string offered = ProbeLog.Describe(bootPath);
        Assert.True(offered.Length == 0, $"no registered driver should be offered the keyboard the built-in took, it was offered to {offered}");
        Assert.True(device.VendorId == UsbDescriptors.QemuHidVendorId && device.ProductId == UsbDescriptors.QemuHidProductId,
            $"the keyboard should present QEMU's HID IDs, which the device match registration matches, presents {device.VendorId:X4}:{device.ProductId:X4}");
        Assert.True(keyboard.DriverName == UsbKeyboardName, $"the keyboard should still be the built-in's, is {keyboard.DriverName ?? "nothing"}'s");

        TR.RequestUsbDeviceUnplug(KeyboardUsbIndex);
        bool gone = WaitUntil(static () => FindUsbRecord(HidClass, BootInterfaceSubclass, KeyboardProtocol) is null, HotPlugWaitMilliseconds);
        Assert.True(gone, "the keyboard's interface should leave the device list once it is pulled out");
    }

    // Plugged back in, the keyboard goes to the built-in again, which comes
    // before the kit in the USB stack's class drivers: no registered driver
    // is offered it on the hot-plug thread either.
    private static void TestUsbKeyboardReplug_BuiltInKeepsItsInterface()
    {
        int probesBefore = ProbeLog.Count;
        TR.RequestUsbDevicePlug(KeyboardUsbIndex);
        bool bound = WaitUntil(static () => IsOwnedBy(FindUsbRecord(HidClass, BootInterfaceSubclass, KeyboardProtocol), UsbKeyboardName),
            HotPlugWaitMilliseconds);
        Assert.True(bound, $"the keyboard plugged back in should be listed as owned by {UsbKeyboardName}");
        Assert.Equal(probesBefore, ProbeLog.Count, $"no registered driver should be offered the keyboard plugged back in, probes ran {ProbeLog.Describe()}");
    }

    // ==================== Device list ====================

    // Every enumerated function once, first and in bus order, owned as it is
    // now: built-ins, the boot display, the edu driver and nothing alike.
    private static void TestDeviceList_MatchesEveryFunction()
    {
        IReadOnlyList<DeviceRecord> records = DriverCore.Devices;
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            Assert.Fail("PCI was not set up");
            return;
        }

        Assert.True(records.Count >= (int)PciManager.Count, $"the device list should hold every enumerated function, holds {records.Count} entries for {PciManager.Count} functions");
        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice function = devices[i];
            string path = ExpectedPath(function);
            DeviceRecord? record = FindRecord(records, path);
            if (record is not { } found)
            {
                Assert.Fail($"the device list has no entry for {path}");
                return;
            }

            Assert.True(found.DriverName == function.Owner, $"{path} should be listed as owned by {function.Owner ?? "nothing"}, is {found.DriverName ?? "nothing"}");
            Assert.True(found.VendorId == function.VendorId && found.DeviceId == function.DeviceId, $"{path} is listed with the wrong IDs");
            Assert.True(found.Class == function.ClassCode && found.Subclass == function.Subclass && found.Protocol == function.ProgIf,
                $"{path} is listed with the wrong class");
        }

        int functions = Math.Min((int)PciManager.Count, records.Count);
        for (int i = 1; i < functions; i++)
        {
            Assert.True(BusOrderKey(records[i - 1].Path, devices) < BusOrderKey(records[i].Path, devices),
                $"{records[i - 1].Path} is listed before {records[i].Path}");
        }
    }

    // After the functions, every interface of every configured USB device,
    // in the USB stack's order, at its path, with its owner, its device's
    // IDs and its class. None on the PCI cells, which attach no USB
    // controller. On the usb-hid cell, after the hot-plug cells plugged
    // every device back in, the mouse's is the boot mouse driver's, the
    // tablet's the link driver's and the keyboard's the built-in's.
    private static void TestDeviceList_ListsEveryUsbInterface()
    {
        IReadOnlyList<DeviceRecord> records = DriverCore.Devices;
        IReadOnlyList<UsbDevice> devices = UsbManager.Devices;
        int interfaceCount = 0;
        for (int i = 0; i < devices.Count; i++)
        {
            interfaceCount += devices[i].Interfaces.Count;
        }

        int index = (int)PciManager.Count;
        Assert.Equal(index + interfaceCount, records.Count, "the device list should hold every enumerated function, then every USB interface");
        if (s_isUsbCell)
        {
            Assert.True(interfaceCount >= 3, "the usb-hid profile's mouse, tablet and keyboard should all be listed");
        }

        for (int i = 0; i < devices.Count; i++)
        {
            UsbDevice device = devices[i];
            List<UsbInterface> interfaces = device.Interfaces;
            for (int j = 0; j < interfaces.Count && index < records.Count; j++, index++)
            {
                UsbInterface usbInterface = interfaces[j];
                DeviceRecord record = records[index];
                string path = ExpectedUsbPath(device, usbInterface);
                Assert.True(record.Path == path, $"entry {index} should be {path}, is {record.Path}");
                Assert.True(record.DriverName == ExpectedUsbOwner(usbInterface),
                    $"{path} should be listed as owned by {ExpectedUsbOwner(usbInterface) ?? "nothing"}, is {record.DriverName ?? "nothing"}");
                Assert.True(record.VendorId == device.VendorId && record.DeviceId == device.ProductId, $"{path} is listed with the wrong IDs");
                Assert.True(record.Class == usbInterface.Class && record.Subclass == usbInterface.Subclass && record.Protocol == usbInterface.Protocol,
                    $"{path} is listed with the wrong class");
            }
        }
    }

    // ==================== Helpers ====================

    /// <summary>Records what the cell's profile attaches, from its base profile name.</summary>
    private static void SelectCellHardware()
    {
        if (IsCellOf(EduProfile))
        {
            s_isEduCell = true;
            SelectPciFunction(EduDriver.VendorId, EduDriver.DeviceId, UnclassifiedClassCode, OtherSubclass);
        }
        else if (IsCellOf(Rtl8139Profile))
        {
            s_isNicCell = true;
            s_isRtl8139Cell = true;
            SelectPciFunction(Rtl8139Driver.VendorId, Rtl8139Driver.DeviceId, NetworkClassCode, EthernetSubclass);
        }
        else if (IsCellOf(E1000EProfile))
        {
            s_isNicCell = true;
            s_isE1000ECell = true;
            SelectPciFunction(IntelVendorId, I82574LDeviceId, NetworkClassCode, EthernetSubclass);
        }
        else if (IsCellOf(NvmeProfile))
        {
            s_isNvmeCell = true;
            SelectPciFunction(FailingNvmeDriver.VendorId, FailingNvmeDriver.DeviceId, MassStorageClassCode, NonVolatileMemorySubclass);
        }
        else
        {
            s_isUsbCell = IsCellOf(UsbHidProfile);
        }

        // From the build and the cell's name, not from what the kit found:
        // a binder that failed to come up must fail the MSI-X cells.
        s_expectMsiX = IsX64 || TR.ProfileContains(Gicv3Modifier);
    }

    /// <summary>The driver that should own the cell's PCI function after the pass. PCI cells only.</summary>
    private static string BoundDriverName()
    {
        if (s_isEduCell)
        {
            return EduDriver.Name;
        }

        if (s_isNvmeCell)
        {
            return NvmeDriver.Name;
        }

        return s_isE1000ECell ? E1000EInterruptDriver.Name : Rtl8139Driver.Name;
    }

    /// <summary>
    /// Spins until <paramref name="condition"/> holds or
    /// <see cref="InterruptWaitMilliseconds"/> of Stopwatch time pass, with
    /// interrupts on, so the timer keeps polling and the scheduler keeps
    /// running the driver-work thread.
    /// </summary>
    /// <returns>Whether the condition held.</returns>
    private static bool WaitUntil(Func<bool> condition) => WaitUntil(condition, InterruptWaitMilliseconds);

    /// <summary>
    /// Spins until <paramref name="condition"/> holds or
    /// <paramref name="milliseconds"/> of Stopwatch time pass, with
    /// interrupts on, so the scheduler keeps running the USB hot-plug and
    /// driver-work threads meanwhile.
    /// </summary>
    /// <returns>Whether the condition held.</returns>
    private static bool WaitUntil(Func<bool> condition, long milliseconds)
    {
        long limit = Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;
        long startedAt = Stopwatch.GetTimestamp();
        while (!condition())
        {
            if (Stopwatch.GetTimestamp() - startedAt >= limit)
            {
                return condition();
            }
        }

        return true;
    }

    /// <summary>Spins for <paramref name="milliseconds"/> of Stopwatch time, which runs with interrupts masked too.</summary>
    private static void SpinMilliseconds(long milliseconds)
    {
        long limit = Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;
        long startedAt = Stopwatch.GetTimestamp();
        while (Stopwatch.GetTimestamp() - startedAt < limit)
        {
        }
    }

    private static void SelectPciFunction(ushort vendorId, ushort deviceId, byte classCode, byte subclass)
    {
        s_isPciCell = true;
        s_expectedVendorId = vendorId;
        s_expectedDeviceId = deviceId;
        s_expectedClassCode = classCode;
        s_expectedSubclass = subclass;
    }

    /// <summary>Records the profile's PCI function as the built-in drivers left it.</summary>
    private static void CaptureFunctionBeforePass()
    {
        if (!s_isPciCell)
        {
            return;
        }

        PciDevice? function = FindFunction(s_expectedVendorId, s_expectedDeviceId);
        if (function is null)
        {
            return;
        }

        s_functionFoundBeforePass = true;
        s_ownerBeforePass = function.Owner;
        s_commandBeforePass = function.Command;
    }

    /// <summary>Records the usb-hid profile's mouse, tablet and keyboard interfaces as the built-in class drivers left them.</summary>
    private static void CaptureUsbInterfacesBeforePass()
    {
        if (!s_isUsbCell)
        {
            return;
        }

        UsbInterface? mouse = FindInterface(HidClass, BootInterfaceSubclass, MouseProtocol, out _);
        if (mouse is not null)
        {
            s_mouseFoundBeforePass = true;
            s_mouseOwnerBeforePass = mouse.DriverName;
        }

        UsbInterface? tablet = FindInterface(HidClass, TabletSubclass, TabletProtocol, out _);
        if (tablet is not null)
        {
            s_tabletFoundBeforePass = true;
            s_tabletOwnerBeforePass = tablet.DriverName;
        }

        UsbInterface? keyboard = FindInterface(HidClass, BootInterfaceSubclass, KeyboardProtocol, out UsbDevice? keyboardDevice);
        if (keyboard is not null && keyboardDevice is not null)
        {
            s_keyboardOwnerBeforePass = keyboard.DriverName;
            s_keyboardPathBeforePass = ExpectedUsbPath(keyboardDevice, keyboard);
        }
    }

    /// <summary>
    /// Registers the suite's drivers, the same set on every cell: each cell's
    /// hardware decides which of them the pass offers anything to. Then tries
    /// the names Register must refuse.
    /// </summary>
    private static void RegisterTestDrivers()
    {
        PciMatch eduDevice = PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId);

        // Would bind edu if it were ever offered it: a ranking that put it
        // ahead of the device matches would show as edu's owner.
        Accept(DriverCore.Register(new PciDriverRegistration(EduClassName,
            static () => new RecordingDriver(EduClassName, ProbeResult.Bound), PciMatch.Class(UnclassifiedClassCode, OtherSubclass))));
        Accept(DriverCore.Register(new PciDriverRegistration(ThrowingDriver.Name, static () => new ThrowingDriver(), eduDevice)));
        Accept(DriverCore.Register(new PciDriverRegistration(ReentrantDriver.Name, ReentrantDriver.Create, eduDevice)));
        Accept(DriverCore.Register(new PciDriverRegistration(EduDriver.Name, static () => new EduDriver(), eduDevice)));

        Accept(DriverCore.Register(new PciDriverRegistration(NicClassName,
            static () => new RecordingDriver(NicClassName, ProbeResult.Declined), PciMatch.Class(NetworkClassCode, EthernetSubclass))));
        Accept(DriverCore.Register(new PciDriverRegistration(NicClassWithInterfaceName,
            static () => new RecordingDriver(NicClassWithInterfaceName, ProbeResult.Declined),
            PciMatch.Class(NetworkClassCode, EthernetSubclass, EthernetProgIf))));

        // After the Ethernet class driver it ties with, so that one is still
        // offered each NIC first.
        Accept(DriverCore.Register(new PciDriverRegistration(E1000EInterruptDriver.Name,
            static () => new E1000EInterruptDriver(), PciMatch.Class(NetworkClassCode, EthernetSubclass))));

        // Last of the Ethernet class drivers, so every other one is offered
        // the RTL8139 before it binds it.
        Accept(DriverCore.Register(new PciDriverRegistration(Rtl8139Driver.Name,
            static () => new Rtl8139Driver(), PciMatch.Class(NetworkClassCode, EthernetSubclass))));

        // The class match first: only its lower rank puts it behind the
        // device match registered after it.
        Accept(DriverCore.Register(new PciDriverRegistration(NvmeDriver.Name,
            static () => new NvmeDriver(), PciMatch.Class(MassStorageClassCode, NonVolatileMemorySubclass, NvmeProgIf))));
        Accept(DriverCore.Register(new PciDriverRegistration(FailingNvmeDriver.Name,
            static () => new FailingNvmeDriver(), PciMatch.Device(FailingNvmeDriver.VendorId, FailingNvmeDriver.DeviceId))));

        s_duplicateNameAccepted = DriverCore.Register(new PciDriverRegistration(EduDriver.Name, CreateInvalid, eduDevice));
        s_builtInNameAccepted = DriverCore.Register(new PciDriverRegistration(XhciOwner, CreateInvalid, eduDevice));
        s_bootDisplayNameAccepted = DriverCore.Register(new PciDriverRegistration(GopOwner, CreateInvalid, eduDevice));
    }

    private static void Accept(bool accepted)
    {
        if (accepted)
        {
            s_acceptedRegistrations++;
        }
    }

    /// <summary>
    /// Registers the suite's USB drivers, after the PCI ones, whose names
    /// they must not take, then tries the names Register must refuse across
    /// both buses.
    /// </summary>
    private static void RegisterUsbTestDrivers()
    {
        UsbMatch hidClass = UsbMatch.Interface(HidClass);

        // Would bind any HID interface it were ever offered: a ranking that
        // put it ahead of a more specific match would show as the owner of
        // the mouse, and a fall-through past the tablet driver as the tablet's.
        AcceptUsb(DriverCore.Register(new UsbDriverRegistration(UsbHidClassName,
            static () => new UsbRecordingDriver(UsbHidClassName, ProbeResult.Bound), hidClass)));
        AcceptUsb(DriverCore.Register(new UsbDriverRegistration(UsbHidDeviceDriver.Name, UsbHidDeviceDriver.Create,
            UsbMatch.Device(UsbDescriptors.QemuHidVendorId, UsbDescriptors.QemuHidProductId))));

        // Ties with the boot mouse driver, and registers first, so it is
        // offered the mouse before it.
        AcceptUsb(DriverCore.Register(new UsbDriverRegistration(UsbMouseDeclinesName,
            static () => new UsbRecordingDriver(UsbMouseDeclinesName, ProbeResult.Declined),
            UsbMatch.Interface(HidClass, BootInterfaceSubclass, MouseProtocol))));
        AcceptUsb(DriverCore.Register(UsbBootMouseDriver.CreateRegistration()));
        AcceptUsb(DriverCore.Register(new UsbDriverRegistration(UsbTabletFailingDriver.Name,
            static () => new UsbTabletFailingDriver(), UsbMatch.Interface(HidClass, TabletSubclass, TabletProtocol))));

        // Would bind the tablet if it were offered it: at boot only a fall
        // through past the tablet driver's failure could give it the tablet,
        // and once the tablet driver declines a tablet plugged in later, it
        // takes that one.
        AcceptUsb(DriverCore.Register(UsbTabletLinkDriver.CreateRegistration()));

        s_usbDuplicateNameAccepted = DriverCore.Register(new UsbDriverRegistration(UsbBootMouseDriver.Name, CreateInvalidUsb, hidClass));
        s_usbNamedAfterPciAccepted = DriverCore.Register(new UsbDriverRegistration(EduDriver.Name, CreateInvalidUsb, hidClass));
        s_pciNamedAfterUsbAccepted = DriverCore.Register(new PciDriverRegistration(UsbBootMouseDriver.Name, CreateInvalid,
            PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId)));
        s_usbHubNameAccepted = DriverCore.Register(new UsbDriverRegistration(HubName, CreateInvalidUsb, hidClass));
        s_usbKeyboardNameAccepted = DriverCore.Register(new UsbDriverRegistration(UsbKeyboardName, CreateInvalidUsb, hidClass));
        s_usbMassStorageNameAccepted = DriverCore.Register(new UsbDriverRegistration(MassStorageName, CreateInvalidUsb, hidClass));
        s_usbNamedAfterPciBuiltInAccepted = DriverCore.Register(new UsbDriverRegistration(XhciOwner, CreateInvalidUsb, hidClass));
        s_pciNamedAfterUsbBuiltInAccepted = DriverCore.Register(new PciDriverRegistration(HubName, CreateInvalid,
            PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId)));
    }

    private static void AcceptUsb(bool accepted)
    {
        if (accepted)
        {
            s_acceptedUsbRegistrations++;
        }
    }

    /// <summary>Factory of the registrations that must never reach the pass; a driver that declines if one did.</summary>
    private static PciDriver CreateInvalid() => new RecordingDriver(InvalidName, ProbeResult.Declined);

    /// <summary>Factory of the USB registrations that must never reach the pass; a driver that declines if one did.</summary>
    private static UsbDriver CreateInvalidUsb() => new UsbRecordingDriver(InvalidName, ProbeResult.Declined);

    /// <summary>
    /// True when the cell's base profile is <paramref name="profile"/>: the
    /// cell name is that profile alone or that profile followed by its
    /// modifiers. A bare prefix test would also take a longer name that
    /// starts the same way.
    /// </summary>
    private static bool IsCellOf(string profile)
    {
        if (!TR.ProfileHasPrefix(profile))
        {
            return false;
        }

        string cell = TR.ProfileName;
        return cell.Length == profile.Length || cell[profile.Length] == ModifierSeparator;
    }

    /// <summary>
    /// The path the kit documents for <paramref name="function"/>, as in
    /// <c>pci/0000:00:04.0</c>: segment 0000, then bus and device as two hex
    /// digits and the function as one; null for no function.
    /// </summary>
    [return: NotNullIfNotNull(nameof(function))]
    private static string? ExpectedPath(PciDevice? function)
    {
        if (function is null)
        {
            return null;
        }

        return $"pci/0000:{function.Bus:x2}:{function.Slot:x2}.{function.Function:x}";
    }

    /// <summary>
    /// Sort key of the enumerated function listed at <paramref name="path"/>:
    /// bus, then device, then function. Past every real key when no
    /// function has that path, which fails the order check.
    /// </summary>
    private static ulong BusOrderKey(string path, PciDevice[] devices)
    {
        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice function = devices[i];
            if (ExpectedPath(function) == path)
            {
                return ((ulong)function.Bus << 16) | ((ulong)function.Slot << 8) | function.Function;
            }
        }

        return ulong.MaxValue;
    }

    private static DeviceRecord? FindRecord(IReadOnlyList<DeviceRecord> records, string path)
    {
        for (int i = 0; i < records.Count; i++)
        {
            if (records[i].Path == path)
            {
                return records[i];
            }
        }

        return null;
    }

    /// <summary>
    /// The first USB interface the device list holds with the given class,
    /// subclass and protocol, or null. The list, unlike the USB stack's own,
    /// is safe to read while the hot-plug thread changes the devices. A PCI
    /// function with the same class code, such as a VGA adapter's 03/00/00,
    /// is not a USB interface.
    /// </summary>
    private static DeviceRecord? FindUsbRecord(byte interfaceClass, byte subclass, byte protocol)
    {
        IReadOnlyList<DeviceRecord> records = DriverCore.Devices;
        for (int i = 0; i < records.Count; i++)
        {
            DeviceRecord record = records[i];
            if (record.Path.StartsWith(UsbPathPrefix, StringComparison.Ordinal)
                && record.Class == interfaceClass && record.Subclass == subclass && record.Protocol == protocol)
            {
                return record;
            }
        }

        return null;
    }

    private static bool IsOwnedBy(DeviceRecord? record, string owner) => record is { } found && found.DriverName == owner;

    /// <summary>
    /// Asks QEMU to move the usb-mouse with the left button held, then to let
    /// the button go, and checks each report reached the mouse manager
    /// through the boot mouse driver: the pointer moves by what was asked
    /// and the button follows, and the driver passed on that same movement
    /// and that button.
    /// </summary>
    private static void CheckPointerMoveThroughBootMouse()
    {
        MouseManager.SetPosition(PointerStartX, PointerStartY);
        int forwardedBefore = UsbBootMouseDriver.ForwardedReports;
        int movedXBefore = UsbBootMouseDriver.ForwardedX;
        int movedYBefore = UsbBootMouseDriver.ForwardedY;
        TR.RequestUsbPointerMove(MouseUsbIndex, PointerMoveX, PointerMoveY, LeftButtonBit);
        bool pressed = WaitUntil(static () => MouseManager.LeftButton && MouseManager.X == PointerStartX + PointerMoveX
            && MouseManager.Y == PointerStartY + PointerMoveY, HotPlugWaitMilliseconds);
        Assert.True(pressed, $"QEMU's pointer movement never reached the mouse manager: the pointer is at ({MouseManager.X}, {MouseManager.Y}), the boot mouse driver passed on {UsbBootMouseDriver.ForwardedReports - forwardedBefore} reports");
        if (!pressed)
        {
            return;
        }

        Assert.True(UsbBootMouseDriver.ForwardedReports > forwardedBefore, "the movement should come through the boot mouse driver's handler");
        Assert.Equal(PointerMoveX, UsbBootMouseDriver.ForwardedX - movedXBefore, "the boot mouse driver should pass on the X movement QEMU sent");
        Assert.Equal(PointerMoveY, UsbBootMouseDriver.ForwardedY - movedYBefore, "the boot mouse driver should pass on the Y movement QEMU sent");
        Assert.Equal(LeftButtonBit, UsbBootMouseDriver.LastForwardedButtons, "the boot mouse driver should pass on the left button QEMU holds");

        int forwardedAfterPress = UsbBootMouseDriver.ForwardedReports;
        TR.RequestUsbPointerMove(MouseUsbIndex, 0, 0, NoButtons);
        bool released = WaitUntil(static () => !MouseManager.LeftButton, HotPlugWaitMilliseconds);
        Assert.True(released, "QEMU's button release never reached the mouse manager");
        Assert.True(UsbBootMouseDriver.ForwardedReports > forwardedAfterPress, "the release should come through the boot mouse driver's handler");
        Assert.Equal(NoButtons, UsbBootMouseDriver.LastForwardedButtons, "the boot mouse driver should pass on the release");
        Assert.Equal(PointerStartX + PointerMoveX, MouseManager.X, "a release without movement moved the pointer");
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static bool ThrowsArgumentOutOfRange(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    private static bool ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    /// <summary>Number of enumerated functions with the given vendor and device id.</summary>
    private static int CountFunctions(ushort vendorId, ushort deviceId)
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return 0;
        }

        int count = 0;
        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (devices[i].VendorId == vendorId && devices[i].DeviceId == deviceId)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>First enumerated function with the given vendor and device id, or null.</summary>
    private static PciDevice? FindFunction(ushort vendorId, ushort deviceId)
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return null;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (devices[i].VendorId == vendorId && devices[i].DeviceId == deviceId)
            {
                return devices[i];
            }
        }

        return null;
    }

    private static bool IsXhci(PciDevice device) =>
        device.ClassCode == SerialBusClassCode && device.Subclass == UsbSubclass && device.ProgIf == XhciProgIf;

    /// <summary>Number of enumerated xHCI controllers.</summary>
    private static int CountXhciFunctions()
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return 0;
        }

        int count = 0;
        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (IsXhci(devices[i]))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>First enumerated xHCI controller, or null.</summary>
    private static PciDevice? FindXhciFunction()
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return null;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (IsXhci(devices[i]))
            {
                return devices[i];
            }
        }

        return null;
    }

    private static bool IsInterface(UsbInterface usbInterface, byte interfaceClass, byte subclass, byte protocol) =>
        usbInterface.Class == interfaceClass && usbInterface.Subclass == subclass && usbInterface.Protocol == protocol;

    /// <summary>Number of interfaces with the given class, subclass and protocol across every enumerated USB device.</summary>
    private static int CountInterfaces(byte interfaceClass, byte subclass, byte protocol)
    {
        IReadOnlyList<UsbDevice> devices = UsbManager.Devices;
        int count = 0;
        for (int i = 0; i < devices.Count; i++)
        {
            List<UsbInterface> interfaces = devices[i].Interfaces;
            for (int j = 0; j < interfaces.Count; j++)
            {
                if (IsInterface(interfaces[j], interfaceClass, subclass, protocol))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>First interface with the given class, subclass and protocol of any enumerated USB device, and that device; null for none.</summary>
    private static UsbInterface? FindInterface(byte interfaceClass, byte subclass, byte protocol, out UsbDevice? device)
    {
        IReadOnlyList<UsbDevice> devices = UsbManager.Devices;
        for (int i = 0; i < devices.Count; i++)
        {
            List<UsbInterface> interfaces = devices[i].Interfaces;
            for (int j = 0; j < interfaces.Count; j++)
            {
                if (IsInterface(interfaces[j], interfaceClass, subclass, protocol))
                {
                    device = devices[i];
                    return interfaces[j];
                }
            }
        }

        device = null;
        return null;
    }

    /// <summary>
    /// The path the kit documents for <paramref name="usbInterface"/> of
    /// <paramref name="device"/>, as in <c>usb/1-2.1:1.0</c>: the bus, which
    /// is the host controller's position from 1, a dash, the root port then
    /// each hub port below it joined with dots, a colon, the configuration
    /// value, a dot and the interface number, all in decimal.
    /// </summary>
    private static string ExpectedUsbPath(UsbDevice device, UsbInterface usbInterface)
    {
        IReadOnlyList<UsbHostController> controllers = UsbManager.Controllers;
        int bus = 0;
        for (int i = 0; i < controllers.Count; i++)
        {
            if (controllers[i] == device.HostController)
            {
                bus = i + 1;
                break;
            }
        }

        string ports = $"{device.PortNumber}";
        for (UsbDevice? hub = device.Parent; hub is not null; hub = hub.Parent)
        {
            ports = $"{hub.PortNumber}.{ports}";
        }

        return $"usb/{bus}-{ports}:{device.ConfigurationValue}.{usbInterface.Number}";
    }

    /// <summary>
    /// The owner the device list should name for <paramref name="usbInterface"/>:
    /// pinned for the usb-hid profile's mouse, tablet and keyboard, and for
    /// any other interface the class driver the USB stack recorded, a
    /// built-in. The tablet has no driver until the hot-plug cells plug it
    /// back in, and the link driver's from then on.
    /// </summary>
    private static string? ExpectedUsbOwner(UsbInterface usbInterface)
    {
        if (IsInterface(usbInterface, HidClass, BootInterfaceSubclass, MouseProtocol))
        {
            return UsbBootMouseDriver.Name;
        }

        if (IsInterface(usbInterface, HidClass, TabletSubclass, TabletProtocol))
        {
            return UsbTabletLinkDriver.IsBound ? UsbTabletLinkDriver.Name : null;
        }

        if (IsInterface(usbInterface, HidClass, BootInterfaceSubclass, KeyboardProtocol))
        {
            return UsbKeyboardName;
        }

        return usbInterface.Driver?.Name;
    }

    /// <summary>A transfer status for a message; the enum's own ToString needs metadata the kernel may not keep.</summary>
    private static string DescribeStatus(UsbTransferStatus? status) => status switch
    {
        null => "never run",
        UsbTransferStatus.Success => "Success",
        UsbTransferStatus.Stall => "Stall",
        UsbTransferStatus.Timeout => "Timeout",
        UsbTransferStatus.Error => "Error",
        UsbTransferStatus.Disconnected => "Disconnected",
        _ => "an unknown status"
    };
}
