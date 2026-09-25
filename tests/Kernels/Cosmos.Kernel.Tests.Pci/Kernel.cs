using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.HAL.Pci;
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

    /// <summary>Reason surfaced when the machine has no Intel NIC for E1000E to bind (arm64 virt's default NIC is virtio).</summary>
    private const string SkipNoIntelNic = "no Intel Ethernet function on this machine";

    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 10;

    // Owner names, spelled out rather than read from PciOwner so a renamed
    // constant is caught instead of compared against itself.
    private const string GopOwner = "gop";
    private const string E1000EOwner = "e1000e";

    /// <summary>Owner names the TryClaim cells write on their private copies of a function.</summary>
    private const string FirstDriver = "test-first";
    private const string SecondDriver = "test-second";

    /// <summary>PCI base class of display controllers.</summary>
    private const byte DisplayClassCode = 0x03;

    /// <summary>Display subclass of a VGA-compatible adapter, the boot display on q35 (std VGA).</summary>
    private const byte VgaCompatibleSubclass = 0x00;

    /// <summary>PCI base class of network controllers.</summary>
    private const byte NetworkClassCode = 0x02;

    /// <summary>PCI vendor id of Intel.</summary>
    private const ushort IntelVendorId = 0x8086;

    /// <summary>All-ones vendor/device id returned by an unmapped or empty config-space read (PCI spec: 0xFFFF = no device).</summary>
    private const ushort AllOnesId = 0xFFFF;

    /// <summary>All-zeros vendor id, the pattern seen when a config-space read hits stale/zeroed memory instead of the device.</summary>
    private const ushort AllZerosVendorId = 0x0000;

    /// <summary>Highest spec-defined PCI base class code (0x00..0x13 per PCI-SIG class code list; 0xFF is "unassigned").</summary>
    private const byte MaxDefinedClassCode = 0x13;

    /// <summary>Vendor ID register offset in PCI configuration space (16-bit, offset 0x00).</summary>
    private const byte VendorIdRegisterOffset = 0x00;

    protected override void BeforeRun()
    {
        Log.WriteString("[Pci] BeforeRun() reached!\n");

        TR.Start("PCI Subsystem Tests", expectedTests: ExpectedTestCount);

        s_firstDevice = PciManager.Count > 0 ? PciManager.Devices![0] : null;
        bool anyDevice = s_firstDevice is not null;

        // ==================== Manager ====================
        TR.Run("Manager_Initialized", TestManager_Initialized);
        // Unconditional on purpose: this suite's only cell is the default
        // q35/virt machine, where zero enumerated devices is an enumeration
        // regression — the one thing this suite exists to catch — not an
        // environment condition. Gating it on anyDevice (= Count > 0) made
        // it a tautology that converted such a regression into 5 skips and
        // a green CI. anyDevice keeps gating only the per-device
        // ConfigSpace spot-checks below.
        TR.Run("Manager_HasDevices", TestManager_HasDevices);

        // ==================== ConfigSpace ====================
        TR.RunIf(anyDevice, "ConfigSpace_VendorId_NotAllOnes",     TestConfigSpace_VendorIdNotAllOnes,     SkipNoDevice);
        TR.RunIf(anyDevice, "ConfigSpace_DeviceId_NotAllOnes",     TestConfigSpace_DeviceIdNotAllOnes,     SkipNoDevice);
        TR.RunIf(anyDevice, "ConfigSpace_ClassCode_InRange",       TestConfigSpace_ClassCodeInRange,       SkipNoDevice);
        TR.RunIf(anyDevice, "ConfigSpace_VendorRead_StableAcrossCalls", TestConfigSpace_VendorReadStable,  SkipNoDevice);

        // ==================== Owner ====================
        TR.Run("Owner_BootDisplay_ReservedAsGop", TestOwner_BootDisplayReservedAsGop);
        TR.RunIf(anyDevice, "Owner_TryClaim_RefusesAnotherDriver", TestOwner_TryClaimRefusesAnotherDriver, SkipNoDevice);
        TR.RunIf(anyDevice, "Owner_TryClaim_TakesOverBootDisplay", TestOwner_TryClaimTakesOverBootDisplay, SkipNoDevice);
        TR.RunIf(FindFunction(IntelVendorId, NetworkClassCode) is not null, "Owner_E1000E_OnEnumeratedFunction",
            TestOwner_E1000EOnEnumeratedFunction, SkipNoIntelNic);

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
    // zero devices end up being found. A null array means Setup never ran —
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

    // ==================== Owner ====================

    // Enumeration reserves the function that scans out the boot framebuffer,
    // so no driver bound later reprograms the BAR the console draws into. On
    // q35 the firmware framebuffer lives in the std VGA adapter's BAR, so
    // that function must be the one; virt has no PCI display (its
    // framebuffer is ramfb, in RAM), so nothing may be reserved there. The
    // choice is read from PciManager.BootDisplay, which no driver rewrites,
    // and the count catches a second function pinned alongside the right one.
    private static void TestOwner_BootDisplayReservedAsGop()
    {
        PciDevice? vga = FindFunctionByClass(DisplayClassCode, VgaCompatibleSubclass);
        PciDevice? bootDisplay = PciManager.BootDisplay;
        int reserved = CountOwnedBy(GopOwner);
        if (vga is not null)
        {
            Assert.True(bootDisplay == vga, "the VGA adapter holding the boot framebuffer should be the function reserved");
            Assert.True(vga.Owner == GopOwner, "the VGA adapter holding the boot framebuffer should be reserved as gop");
            Assert.Equal(1, reserved, "exactly one function should be reserved as the boot display");
        }
        else
        {
            Assert.Null(bootDisplay, "no function should be reserved when no PCI display holds the framebuffer");
            Assert.Equal(0, reserved, "no function should be owned by gop when none was reserved");
        }
    }

    // TryClaim is the only writer of Owner. Both cells run on a private copy
    // of the first function (its constructor only reads config space), so
    // the enumerated function's real owner is left alone.
    private static void TestOwner_TryClaimRefusesAnotherDriver()
    {
        PciDevice? copy = CopyFirstDevice();
        if (copy is null)
        {
            Assert.Fail("no PCI device enumerated");
            return;
        }

        Assert.True(copy.Owner is null, "a function nobody claimed should have no owner");
        Assert.False(copy.Claimed, "a function nobody owns should not report claimed");
        Assert.True(copy.TryClaim(FirstDriver), "a free function should be taken");
        Assert.True(copy.TryClaim(FirstDriver), "the owner should be able to claim its function again");
        Assert.False(copy.TryClaim(SecondDriver), "a function another driver owns should be refused");
        Assert.True(copy.Owner == FirstDriver, "a refused claim should leave the owner unchanged");
        Assert.True(copy.Claimed, "a driver-owned function should report claimed");
    }

    // The boot display reservation keeps later drivers off the console's
    // BAR, but it is not a driver: Claimed stays false, and a built-in
    // display driver may still take the function over.
    private static void TestOwner_TryClaimTakesOverBootDisplay()
    {
        PciDevice? copy = CopyFirstDevice();
        if (copy is null)
        {
            Assert.Fail("no PCI device enumerated");
            return;
        }

        Assert.True(copy.TryClaim(GopOwner), "a free function should accept the boot display reservation");
        Assert.False(copy.Claimed, "the boot display reservation should not report claimed");
        Assert.True(copy.TryClaim(FirstDriver), "a display driver should be able to take the reserved function");
        Assert.True(copy.Owner == FirstDriver, "the display driver should own the function after taking it");
        Assert.True(copy.Claimed, "the function should report claimed once a driver owns it");
        Assert.False(copy.TryClaim(GopOwner), "a reservation should never displace a driver");
    }

    // E1000E is a PciDevice itself, a second object for its NIC's function.
    // An owner recorded on that object would leave the function PciManager
    // holds looking free to every other driver, so it must land on the latter.
    private static void TestOwner_E1000EOnEnumeratedFunction()
    {
        PciDevice? nic = FindFunction(IntelVendorId, NetworkClassCode);
        if (nic is null)
        {
            Assert.Fail("no Intel Ethernet function enumerated");
            return;
        }

        Assert.True(nic.Owner == E1000EOwner, "the Intel NIC's enumerated function should be owned by e1000e");
    }

    // ==================== Helpers ====================

    /// <summary>A second object for the first enumerated function, or null when none was enumerated.</summary>
    private static PciDevice? CopyFirstDevice()
    {
        PciDevice? first = s_firstDevice;
        return first is null ? null : new PciDevice(first.Bus, first.Slot, first.Function);
    }

    /// <summary>First enumerated function with the given vendor id and base class, or null.</summary>
    private static PciDevice? FindFunction(ushort vendorId, byte classCode)
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return null;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (devices[i].VendorId == vendorId && devices[i].ClassCode == classCode)
            {
                return devices[i];
            }
        }

        return null;
    }

    /// <summary>First enumerated function with the given base class and subclass, or null.</summary>
    private static PciDevice? FindFunctionByClass(byte classCode, byte subclass)
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return null;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (devices[i].ClassCode == classCode && devices[i].Subclass == subclass)
            {
                return devices[i];
            }
        }

        return null;
    }

    /// <summary>Number of enumerated functions whose owner is <paramref name="owner"/>.</summary>
    private static int CountOwnedBy(string owner)
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return 0;
        }

        int count = 0;
        for (uint i = 0; i < PciManager.Count; i++)
        {
            if (devices[i].Owner == owner)
            {
                count++;
            }
        }

        return count;
    }
}
