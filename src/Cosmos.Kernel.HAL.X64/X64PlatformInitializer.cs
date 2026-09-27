// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.Core.X64;
using Cosmos.Kernel.Core.X64.Cpu;
using Cosmos.Kernel.Core.X64.IO;
using Cosmos.Kernel.Core.X64.Power;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.X64.Devices.Clock;
using Cosmos.Kernel.HAL.X64.Devices.Input;
using Cosmos.Kernel.HAL.X64.Devices.Timer;

namespace Cosmos.Kernel.HAL.X64;

/// <summary>
/// X64 platform initializer - creates x64-specific HAL components.
/// </summary>
internal class X64PlatformInitializer : IPlatformInitializer
{
    private PIT? _pit;
    private RTC? _rtc;
    private PS2Controller? _ps2Controller;

    public string PlatformName => "x86-64";
    public PlatformArchitecture Architecture => PlatformArchitecture.X64;

    public IPortIO CreatePortIO() => new X64PortIO();
    public ICpuOps CreateCpuOps() => new X64CpuOps();
    public IPowerOps CreatePowerOps() => new X64PowerOps();
    public IInterruptController CreateInterruptController() => new X64InterruptController();

    public void PreparePciMapping(ulong ecamBase)
    {
        // x64 uses legacy port I/O (0xCF8/0xCFC) for PCI config access,
        // which bypasses the MMU — no memory mapping needed.
    }

    public bool EnsureMmioMapped(ulong physBase)
    {
        // Limine's blanket map (base revision 0) only covers the low 4 GiB
        // plus memory-map regions; a 64-bit BAR relocated above 4 GiB is in
        // neither, and touching its HHDM alias would page-fault. Install an
        // on-demand UC mapping for it (no-op for already-mapped regions,
        // i.e. everything below 4 GiB).
        return DeviceMapper.EnsureMapped(physBase);
    }

    public void DmaBarrier()
    {
        // x86-64's total store order already makes normal-memory stores
        // visible before a subsequent MMIO (UC) store, and keeps loads in
        // program order — no fence instruction is required here.
    }

    /// <inheritdoc />
    public void DelayMicroseconds(uint microseconds)
    {
        // Legacy POST-port read: ~1 µs per access on PC chipsets, no
        // interrupts or calibration needed, safe from phase-3 init.
        for (uint i = 0; i < microseconds; i++)
        {
            s_delayPort.ReadByte(PlatformHAL.LegacyPostPort);
        }
    }

    private static readonly X64PortIO s_delayPort = new();

    public void InitializeHardware()
    {
        // Display ACPI MADT information
        Serial.WriteString("[X64HAL] Displaying ACPI MADT info...\n");
        AcpiMadt.DisplayMadtInfo();

        // Initialize APIC
        Serial.WriteString("[X64HAL] Initializing APIC...\n");
        ApicManager.Initialize();

        // Calibrate TSC frequency
        Serial.WriteString("[X64HAL] Calibrating TSC frequency...\n");
        X64CpuOps.CalibrateTsc();
        Serial.WriteString("[X64HAL] TSC frequency: ");
        Serial.WriteNumber((ulong)X64CpuOps.TscFrequency);
        Serial.WriteString(" Hz\n");

        // Initialize RTC
        Serial.WriteString("[X64HAL] Initializing RTC...\n");
        _rtc = new RTC();
        _rtc.Initialize();

        // Initialize PIT
        Serial.WriteString("[X64HAL] Initializing PIT...\n");
        _pit = new PIT();
        _pit.Initialize();
        _pit.RegisterIRQHandler();

        // Initialize PS/2 Controller (if keyboard or mouse feature enabled)
        if (CosmosFeatures.KeyboardEnabled || CosmosFeatures.MouseEnabled)
        {
            Serial.WriteString("[X64HAL] Initializing PS/2 controller...\n");
            _ps2Controller = new PS2Controller();
            _ps2Controller.Initialize();
        }
    }

    public ITimerDevice CreateTimer()
    {
        if (!CosmosFeatures.TimerEnabled)
        {
            return null!;
        }

        if (_pit == null)
        {
            _pit = new PIT();
            _pit.Initialize();
        }
        return _pit;
    }

    public IKeyboardDevice[] GetKeyboardDevices()
    {
        if (!CosmosFeatures.KeyboardEnabled)
        {
            return [];
        }

        // PS/2 only: the virtio keyboards this architecture sees are all on
        // the PCI bus, where the built-in virtio-input kit driver binds them
        // in the driver pass and publishes them, and q35 has no virtio-mmio
        // window for HAL's own virtio keyboard driver to find one on. USB
        // keyboards come from the pass too, through the built-in boot
        // keyboard driver.
        return _ps2Controller is not null ? PS2Controller.GetKeyboardDevices() : [];
    }

    public IMouseDevice[] GetMouseDevices()
    {
        if (!CosmosFeatures.MouseEnabled)
        {
            return [];
        }

        // PS/2 only, for the reason given in GetKeyboardDevices.
        return _ps2Controller is not null ? PS2Controller.GetMouseDevices() : [];
    }

    public INetworkDevice? GetNetworkDevice()
    {
        // None: this architecture's NICs all arrive on the PCI bus, where the
        // built-in E1000E and virtio-net kit drivers bind them in the driver
        // pass and publish their links, and q35 has no virtio-mmio window for
        // HAL's own virtio-net driver to find one on.
        return null;
    }

    public unsafe uint GetCpuCount()
    {
        var madtInfo = AcpiMadt.GetMadtInfoPtr();
        return madtInfo != null ? madtInfo->CpuCount : 1;
    }

    public void StartSchedulerTimer(uint quantumMs)
    {
        // Register LAPIC timer handler
        Serial.WriteString("[X64HAL] Registering LAPIC timer handler...\n");
        LocalApic.RegisterTimerHandler();

        // Start LAPIC timer for preemptive scheduling
        Serial.WriteString("[X64HAL] Starting LAPIC timer for scheduling...\n");
        LocalApic.StartPeriodicTimer(quantumMs);
    }
}
