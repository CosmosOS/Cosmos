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
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Mouse;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4.DHCP;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The driver kit. Each cell attaches one piece of hardware that no
/// built-in driver claims: the PCI profiles (edu, rtl8139, e1000e-arm64,
/// and nvme, which this kernel leaves free by building without Storage)
/// put one function on the bus, and the usb-mouse profile puts a HID boot
/// mouse behind an xHCI controller. The kernel registers its test drivers
/// from its constructor, the driver pass in Global.StartKernel offers them
/// the free functions, and the cells check what came of it: the
/// registration rules, the ranking, the teardown of failed attempts, the
/// interrupts, work items and events the kit hands out, the mouse and
/// network link drivers publish, on edu a driver that drives the device's
/// registers, DMA engine and polled interrupt, on the RTL8139 a NIC driver
/// whose link gets a DHCP lease through the kernel's network stack, and on
/// NVMe a failed attempt with MSI-X followed by a driver that gets it again
/// and takes a real command's completion interrupt.
///
/// One kernel binary serves every cell of an architecture, so the hardware a
/// cell presents is read from the profile name the engine puts on the kernel
/// command line. The gates key off that name, not off what was found: a
/// device that failed to enumerate must fail its cell, not skip it.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 47;

    // Base profile names, spelled as in tests/profiles.json. A cell name is
    // one of them followed by "+modifier" for each modifier composed onto it.
    private const string EduProfile = "edu";
    private const string Rtl8139Profile = "rtl8139";
    private const string E1000EProfile = "e1000e-arm64";
    private const string UsbMouseProfile = "usb-mouse";
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
    // §4.2-§4.3), as QEMU's usb-mouse does.
    private const byte HidClass = 0x03;
    private const byte BootInterfaceSubclass = 0x01;
    private const byte MouseProtocol = 0x02;

    // The suite's registrations besides the edu driver and its two
    // edu-matching siblings. The class ones register before the ones that
    // must beat them, so only the kind of match can put them behind.
    private const string EduClassName = "edu-class";
    private const string NicClassName = "nic-class";
    private const string NicClassWithInterfaceName = "nic-class-progif";
    private const string LateName = "late";
    private const string InvalidName = "invalid";

    /// <summary>Registrations the constructor expects Register to accept.</summary>
    private const int AcceptedRegistrationCount = 10;

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
        s_networkDevicesBeforePass = NetworkManager.DeviceCount;
        RegisterTestDrivers();
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
        TR.RunIf(s_isUsbCell, "Usb_XhciOwnedByXhci",       TestUsb_XhciOwnedByXhci,       SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_MouseEnumeratedOnce",   TestUsb_MouseEnumeratedOnce,   SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_MouseInterfaceUnbound", TestUsb_MouseInterfaceUnbound, SkipNotUsbCell);

        // ==================== Registration ====================
        TR.Run("Register_TakenNameRefused",           TestRegister_TakenNameRefused);
        TR.Run("Register_InvalidRegistrationThrows",  TestRegister_InvalidRegistrationThrows);
        TR.Run("Register_AfterPassThrows",            TestRegister_AfterPassThrows);
        TR.RunIf(s_isEduCell, "Register_FromDriverCallbackThrows", TestRegister_FromDriverCallbackThrows, SkipNotEduCell);

        // ==================== Ranking ====================
        TR.RunIf(s_isEduCell, "Ranking_DeviceMatchBeatsClassMatch",       TestRanking_DeviceMatchBeatsClassMatch,       SkipNotEduCell);
        TR.RunIf(s_isEduCell || s_isNvmeCell, "Ranking_FailedAndDeclinedFallThrough", TestRanking_FailedAndDeclinedFallThrough, SkipNotEduOrNvmeCell);
        TR.RunIf(s_isNicCell, "Ranking_ClassWithInterfaceBeatsClass",     TestRanking_ClassWithInterfaceBeatsClass,     SkipNotNicCell);

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

        // ==================== Device list ====================
        TR.Run("DeviceList_MatchesEveryFunction", TestDeviceList_MatchesEveryFunction);

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
    // interface, since the profile attaches one mouse and nothing else on USB.
    private static void TestUsb_MouseEnumeratedOnce()
    {
        Assert.Equal(1, CountMouseInterfaces(), "exactly one USB device should present a HID boot mouse interface");
    }

    // No class driver takes a HID mouse: the keyboard driver matches the
    // keyboard protocol only, the hub and mass storage drivers other classes.
    // The interface stays free for a USB driver a kernel registers.
    private static void TestUsb_MouseInterfaceUnbound()
    {
        UsbInterface? mouse = FindMouseInterface();
        if (mouse is null)
        {
            Assert.Fail("no HID boot mouse interface enumerated");
            return;
        }

        Assert.Null(mouse.Driver, "no class driver should be bound to the HID boot mouse interface");
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

    // ==================== Device list ====================

    // Every enumerated function once, in bus order, owned as it is now:
    // built-ins, the boot display, the edu driver and nothing alike.
    private static void TestDeviceList_MatchesEveryFunction()
    {
        IReadOnlyList<DeviceRecord> records = DriverCore.Devices;
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            Assert.Fail("PCI was not set up");
            return;
        }

        Assert.Equal((int)PciManager.Count, records.Count, "the device list should hold every enumerated function");
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

        for (int i = 1; i < records.Count; i++)
        {
            Assert.True(BusOrderKey(records[i - 1].Path, devices) < BusOrderKey(records[i].Path, devices),
                $"{records[i - 1].Path} is listed before {records[i].Path}");
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
            s_isUsbCell = IsCellOf(UsbMouseProfile);
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
    private static bool WaitUntil(Func<bool> condition)
    {
        long limit = Stopwatch.Frequency / MillisecondsPerSecond * InterruptWaitMilliseconds;
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

    /// <summary>Factory of the registrations that must never reach the pass; a driver that declines if one did.</summary>
    private static PciDriver CreateInvalid() => new RecordingDriver(InvalidName, ProbeResult.Declined);

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

    private static bool IsBootMouse(UsbInterface usbInterface) =>
        usbInterface.Class == HidClass && usbInterface.Subclass == BootInterfaceSubclass && usbInterface.Protocol == MouseProtocol;

    /// <summary>Number of HID boot mouse interfaces across every enumerated USB device.</summary>
    private static int CountMouseInterfaces()
    {
        IReadOnlyList<UsbDevice> devices = UsbManager.Devices;
        int count = 0;
        for (int i = 0; i < devices.Count; i++)
        {
            List<UsbInterface> interfaces = devices[i].Interfaces;
            for (int j = 0; j < interfaces.Count; j++)
            {
                if (IsBootMouse(interfaces[j]))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>First HID boot mouse interface of any enumerated USB device, or null.</summary>
    private static UsbInterface? FindMouseInterface()
    {
        IReadOnlyList<UsbDevice> devices = UsbManager.Devices;
        for (int i = 0; i < devices.Count; i++)
        {
            List<UsbInterface> interfaces = devices[i].Interfaces;
            for (int j = 0; j < interfaces.Count; j++)
            {
                if (IsBootMouse(interfaces[j]))
                {
                    return interfaces[j];
                }
            }
        }

        return null;
    }
}
