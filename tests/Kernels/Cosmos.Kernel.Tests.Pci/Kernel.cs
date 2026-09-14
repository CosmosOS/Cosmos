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

    /// <summary>Number of tests announced to the runner in TR.Start.</summary>
    private const int ExpectedTestCount = 6;

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
        bool anyDevice = s_firstDevice != null;

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
}
