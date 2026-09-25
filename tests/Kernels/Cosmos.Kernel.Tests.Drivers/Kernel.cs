// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// Driver kit groundwork. Each cell attaches one piece of hardware that no
/// built-in driver claims, and the suite pins that the kernel sees it and
/// leaves it free: the PCI profiles (edu, rtl8139, e1000e-arm64) put one
/// function on the bus, and the usb-mouse profile puts a HID boot mouse
/// behind an xHCI controller. The binding cells arrive with the driver
/// engine and build on these, since a device the kernel never enumerated,
/// or one a built-in already owns, is not one a user driver can bind.
///
/// One kernel binary serves every cell of an architecture, so the hardware a
/// cell presents is read from the profile name the engine puts on the kernel
/// command line. The gates key off that name, not off what was found: a
/// device that failed to enumerate must fail its cell, not skip it.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 7;

    // Base profile names, spelled as in tests/profiles.json. A cell name is
    // one of them followed by "+modifier" for each modifier composed onto it.
    private const string EduProfile = "edu";
    private const string Rtl8139Profile = "rtl8139";
    private const string E1000EProfile = "e1000e-arm64";
    private const string UsbMouseProfile = "usb-mouse";

    /// <summary>Separator between a cell's base profile and each modifier composed onto it.</summary>
    private const char ModifierSeparator = '+';

    /// <summary>Reason surfaced for the PCI cells on a cell whose profile attaches USB hardware.</summary>
    private const string SkipNotPciCell = "this cell attaches no unclaimed PCI function";

    /// <summary>Reason surfaced for the USB cells on a cell whose profile attaches a PCI function.</summary>
    private const string SkipNotUsbCell = "this cell attaches no USB device";

    // QEMU's edu device. QEMU gives it class code 0x00ff (PCI_CLASS_OTHERS):
    // base class 0x00, which predates class codes, and subclass 0xff.
    private const ushort QemuVendorId = 0x1234;
    private const ushort EduDeviceId = 0x11E8;
    private const byte UnclassifiedClassCode = 0x00;
    private const byte OtherSubclass = 0xFF;

    private const ushort RealtekVendorId = 0x10EC;
    private const ushort Rtl8139DeviceId = 0x8139;

    // Intel 82574L, the function QEMU's e1000e model presents.
    private const ushort IntelVendorId = 0x8086;
    private const ushort I82574LDeviceId = 0x10D3;

    private const byte NetworkClassCode = 0x02;
    private const byte EthernetSubclass = 0x00;

    // An xHCI controller: serial bus controller, USB, programming interface 0x30.
    private const byte SerialBusClassCode = 0x0C;
    private const byte UsbSubclass = 0x03;
    private const byte XhciProgIf = 0x30;

    // Owner name, spelled out rather than read from PciOwner so a renamed
    // constant is caught instead of compared against itself.
    private const string XhciOwner = "xhci";

    // A HID interface that declares the boot mouse protocol (HID 1.11
    // §4.2-§4.3), as QEMU's usb-mouse does.
    private const byte HidClass = 0x03;
    private const byte BootInterfaceSubclass = 0x01;
    private const byte MouseProtocol = 0x02;

    // The hardware this cell's profile attaches, chosen once in BeforeRun
    // from the profile name.
    private static bool s_isPciCell;
    private static bool s_isUsbCell;
    private static ushort s_expectedVendorId;
    private static ushort s_expectedDeviceId;
    private static byte s_expectedClassCode;
    private static byte s_expectedSubclass;

    protected override void BeforeRun()
    {
        Log.WriteString("[Drivers] BeforeRun() reached!\n");

        TR.Start("Driver Kit Tests", expectedTests: ExpectedTestCount);

        SelectCellHardware();

        // ==================== Profile ====================
        TR.Run("Profile_Recognized", TestProfile_Recognized);

        // ==================== PCI ====================
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionEnumeratedOnce", TestPci_ProfileFunctionEnumeratedOnce, SkipNotPciCell);
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionClassMatches",   TestPci_ProfileFunctionClassMatches,   SkipNotPciCell);
        TR.RunIf(s_isPciCell, "Pci_ProfileFunctionUnowned",        TestPci_ProfileFunctionUnowned,        SkipNotPciCell);

        // ==================== USB ====================
        TR.RunIf(s_isUsbCell, "Usb_XhciOwnedByXhci",       TestUsb_XhciOwnedByXhci,       SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_MouseEnumeratedOnce",   TestUsb_MouseEnumeratedOnce,   SkipNotUsbCell);
        TR.RunIf(s_isUsbCell, "Usb_MouseInterfaceUnbound", TestUsb_MouseInterfaceUnbound, SkipNotUsbCell);

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
    // user driver's registration matches, changing what its cells test.
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

    // Free for the user driver the binding cells will register: no built-in
    // took the function, and it is not the boot display either.
    private static void TestPci_ProfileFunctionUnowned()
    {
        PciDevice? function = FindFunction(s_expectedVendorId, s_expectedDeviceId);
        if (function is null)
        {
            Assert.Fail("the profile's PCI function was not enumerated");
            return;
        }

        Assert.Null(function.Owner, "no built-in driver should own the profile's PCI function");
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
    // The interface stays free for the user USB driver the binding cells will
    // register.
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

    // ==================== Helpers ====================

    /// <summary>Records what the cell's profile attaches, from its base profile name.</summary>
    private static void SelectCellHardware()
    {
        if (IsCellOf(EduProfile))
        {
            SelectPciFunction(QemuVendorId, EduDeviceId, UnclassifiedClassCode, OtherSubclass);
        }
        else if (IsCellOf(Rtl8139Profile))
        {
            SelectPciFunction(RealtekVendorId, Rtl8139DeviceId, NetworkClassCode, EthernetSubclass);
        }
        else if (IsCellOf(E1000EProfile))
        {
            SelectPciFunction(IntelVendorId, I82574LDeviceId, NetworkClassCode, EthernetSubclass);
        }
        else
        {
            s_isUsbCell = IsCellOf(UsbMouseProfile);
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
