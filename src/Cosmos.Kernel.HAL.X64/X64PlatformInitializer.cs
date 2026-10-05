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
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Platform;
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
    /// <summary>The CONFIG_ADDRESS port of the legacy PCI configuration mechanism.</summary>
    private const ushort LegacyConfigAddressPort = 0xCF8;

    /// <summary>Ports the legacy mechanism occupies: CONFIG_ADDRESS and CONFIG_DATA, four bytes each.</summary>
    private const ushort LegacyConfigPortCount = 8;

    /// <summary>The PCI segment group the legacy mechanism reaches.</summary>
    private const ushort LegacyPciSegment = 0;

    /// <summary>The first bus the legacy mechanism decodes.</summary>
    private const byte LegacyFirstBus = 0;

    /// <summary>The last bus the legacy mechanism decodes: the bus field of CONFIG_ADDRESS is eight bits.</summary>
    private const byte LegacyLastBus = 255;

    /// <summary>The 8042 node's address: the controller's data port, in the device tree's name@hex form.</summary>
    private const string I8042Address = "i8042@60";

    /// <summary>The 8042 node's compatible string, the PNP id of the PC keyboard controller.</summary>
    private const string I8042Compatible = "pnp0303";

    /// <summary>The 8042's data port.</summary>
    private const ushort I8042DataPort = 0x60;

    /// <summary>The 8042's status port on read, command port on write.</summary>
    private const ushort I8042ControlPort = 0x64;

    /// <summary>Ports each of the 8042's two resources occupies.</summary>
    private const ushort I8042PortCount = 1;

    /// <summary>The ISA line the 8042 raises for a byte from its first port.</summary>
    private const uint I8042KeyboardLine = 1;

    /// <summary>The ISA line the 8042 raises for a byte from its second port.</summary>
    private const uint I8042AuxiliaryLine = 12;

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
        // which bypasses the MMU, so no memory mapping is needed.
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
        // program order, so no fence instruction is required here.
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

    /// <summary>
    /// The q35 machine description: the 8042 keyboard controller node, then
    /// one PCI host node over the legacy port mechanism, buses 0 to 255 of
    /// segment 0, which the PCI host driver enumerates. q35's MCFG is not
    /// used on x64 because sub-4 GiB MMIO carries Limine's cacheable
    /// attributes there, and the ports need no mapping at all. The 8042
    /// node is published first, so its driver binds first and holds lines 1
    /// and 12 before any PCI function is offered (the PCI line source then
    /// refuses line 12 as held). Always: every PC has the controller; its
    /// lines are only described here and connected by the driver that binds
    /// it. Thread context, interrupts disabled, from the HAL library
    /// initializer; the PCI host node only when PCI is compiled in.
    /// </summary>
    public void PublishPlatformNodes()
    {
        PublishI8042Node();

        if (!CosmosFeatures.PCIEnabled)
        {
            return;
        }

        Serial.WriteString("[X64HAL] Publishing the legacy PCI host node...\n");
        PlatformIdentity identity = new("pci@cf8", ["pci-host-legacy"]);
        DeviceResource[] resources = [DeviceResource.PortRange(LegacyConfigAddressPort, LegacyConfigPortCount)];
        PciHostAccess host = PciHostAccess.ForPorts(LegacyPciSegment, LegacyFirstBus, LegacyLastBus);
        PlatformBus.Publish(identity, resources, [], host);
    }

    /// <summary>
    /// Publishes the 8042 node, <c>platform:i8042@60</c>, compatible
    /// <c>pnp0303</c>: resource 0 the data port, resource 1 the status and
    /// command port; interrupt 0 line 1, interrupt 1 line 12, both over
    /// <see cref="IoApicLineRouting"/>. A port range grants no exclusivity:
    /// the power operations write the reset command to port 0x64 behind the
    /// driver. Thread context, interrupts disabled.
    /// </summary>
    private static void PublishI8042Node()
    {
        Serial.WriteString("[X64HAL] Publishing the 8042 node...\n");
        PlatformIdentity identity = new(I8042Address, [I8042Compatible]);
        DeviceResource[] resources = [DeviceResource.PortRange(I8042DataPort, I8042PortCount), DeviceResource.PortRange(I8042ControlPort, I8042PortCount)];
        InterruptSource[] interrupts = [new PlatformLineInterruptSource(I8042KeyboardLine, IoApicLineRouting.Instance), new PlatformLineInterruptSource(I8042AuxiliaryLine, IoApicLineRouting.Instance)];
        PlatformBus.Publish(identity, resources, interrupts, null);
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

        // PS/2 only: USB and virtio keyboards are kit drivers now,
        // published to the keyboard consumer.
        return _ps2Controller is not null ? PS2Controller.GetKeyboardDevices() : [];
    }

    public IMouseDevice[] GetMouseDevices()
    {
        if (!CosmosFeatures.MouseEnabled)
        {
            return [];
        }

        // PS/2 mice only: virtio input is a kit driver now, published to
        // the pointer consumer.
        return _ps2Controller is not null ? PS2Controller.GetMouseDevices() : [];
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
