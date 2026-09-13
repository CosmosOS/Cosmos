// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.ARM64.Cpu;
using Cosmos.Kernel.Core.ARM64.IO;
using Cosmos.Kernel.Core.ARM64.Power;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.HAL.ARM64.Devices.Clock;
using Cosmos.Kernel.HAL.ARM64.Devices.Timer;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.ARM64;

/// <summary>
/// ARM64 platform initializer - creates ARM64-specific HAL components.
/// </summary>
internal class ARM64PlatformInitializer : IPlatformInitializer
{
    /// <summary>Number of microseconds in one second, used to convert the generic-timer frequency (Hz) into ticks per microsecond.</summary>
    private const ulong MicrosecondsPerSecond = 1_000_000UL;

    /// <summary>Crude spin iterations (dsb sy + isb barriers) per microsecond used when firmware left CNTFRQ unprogrammed.</summary>
    private const uint FallbackSpinLoopsPerMicrosecond = 100;

    // QEMU virt virtio MMIO window: 32 slots of 0x200 bytes at 0x0a000000,
    // with consecutive SPIs starting at 16 (INTID 48).
    private const ulong VirtioMmioBase = 0x0a000000;
    private const ulong VirtioMmioSlotSize = 0x200;
    private const uint VirtioMmioSlotCount = 32;
    private const uint VirtioMmioIrqBase = 48;

    private GenericTimer? _timer;

    public string PlatformName => "ARM64";
    public PlatformArchitecture Architecture => PlatformArchitecture.ARM64;

    public IPortIO CreatePortIO() => new ARM64MemoryIO();
    public ICpuOps CreateCpuOps() => new ARM64CpuOps();
    public IPowerOps CreatePowerOps() => new ARM64PowerOps();
    public IInterruptController CreateInterruptController() => new ARM64InterruptController();

    public void PreparePciMapping(ulong ecamBase)
    {
        if (ecamBase != 0)
        {
            DeviceMapper.EnsureMapped(ecamBase);
        }
    }

    public void EnsureMmioMapped(ulong physBase)
    {
        // Limine's HHDM on aarch64 only covers RAM with Normal-cacheable
        // attributes; device MMIO has to be mapped explicitly as Device
        // memory so register reads/writes aren't reordered or cached.
        // Safe to call repeatedly — DeviceMapper.EnsureMapped no-ops if the
        // mapping already exists.
        if (physBase != 0)
        {
            DeviceMapper.EnsureMapped(physBase);
        }
    }

    public void DmaBarrier()
    {
        // dsb sy + isb: orders Normal-memory descriptor/queue writes against
        // the Device-memory doorbell store that follows (and device-written
        // flags against the payload reads that follow them). ARM64 does not
        // order Normal vs Device accesses on its own.
        Cosmos.Kernel.Core.ARM64.Bridge.DeviceMapperNative.DsbIsb();
    }

    /// <inheritdoc />
    public void DelayMicroseconds(uint microseconds)
    {
        // Generic-timer busy wait: CNTVCT/CNTFRQ are readable from EL1
        // without any driver init, so this works during phase-3 device
        // bring-up. Falls back to a crude spin if firmware left CNTFRQ
        // unprogrammed (should not happen on QEMU virt or real EL2 boots).
        ulong freq = Cosmos.Kernel.Core.ARM64.Bridge.GenericTimerNative.GetFrequency();
        if (freq == 0)
        {
            for (uint i = 0; i < microseconds * FallbackSpinLoopsPerMicrosecond; i++)
            {
                Cosmos.Kernel.Core.ARM64.Bridge.DeviceMapperNative.DsbIsb();
            }
            return;
        }

        ulong target = Cosmos.Kernel.Core.ARM64.Bridge.GenericTimerNative.GetCounter() + (freq * microseconds + (MicrosecondsPerSecond - 1UL)) / MicrosecondsPerSecond;
        while (Cosmos.Kernel.Core.ARM64.Bridge.GenericTimerNative.GetCounter() < target)
        {
        }
    }

    public void InitializeHardware()
    {
        // Initialize Generic Timer
        Serial.WriteString("[ARM64HAL] Initializing Generic Timer...\n");
        _timer = new GenericTimer();
        _timer.Initialize();

        // Register timer interrupt handler
        Serial.WriteString("[ARM64HAL] Registering timer interrupt handler...\n");
        _timer.RegisterIRQHandler();

        // Initialize RTC (reads boot wall-clock time from PL031 if available)
        Serial.WriteString("[ARM64HAL] Initializing RTC...\n");
        new RTC().Initialize();

        if (CosmosFeatures.KeyboardEnabled || CosmosFeatures.MouseEnabled || CosmosFeatures.NetworkEnabled)
        {
            DeviceMapper.EnsureMapped(VirtioMmioBase);
            // Scan for virtio devices
            Serial.WriteString("[ARM64HAL] Scanning for virtio devices...\n");
            VirtioDevice.InitializeMmioBus(VirtioMmioBase, VirtioMmioSlotSize, VirtioMmioSlotCount,
                VirtioMmioIrqBase, EnableVirtioIrq);
        }

    }

    /// <summary>
    /// Wires a virtio MMIO interrupt line: handler into the dense table, then
    /// GIC configuration. The handler must be installed BEFORE enabling the
    /// interrupt — virtio MMIO lines are level-triggered, and the GIC fires
    /// immediately on enable if the line is already asserted.
    /// </summary>
    private static void EnableVirtioIrq(uint intid, InterruptManager.IrqDelegate handler)
    {
        InterruptManager.SetHandler((byte)intid, handler);

        GIC.ConfigureInterrupt(intid, false);
        GIC.SetPriority(intid, 0x80);
        GIC.EnableInterrupt(intid);
    }

    public ITimerDevice CreateTimer()
    {
        if (!CosmosFeatures.TimerEnabled)
        {
            return null!;
        }

        if (_timer == null)
        {
            _timer = new GenericTimer();
            _timer.Initialize();
        }
        return _timer;
    }

    public IKeyboardDevice[] GetKeyboardDevices()
    {
        if (!CosmosFeatures.KeyboardEnabled)
        {
            return [];
        }

        return VirtioDevice.GetKeyboards();
    }

    public IMouseDevice[] GetMouseDevices()
    {
        if (!CosmosFeatures.MouseEnabled)
        {
            return [];
        }

        return VirtioDevice.GetMice();
    }

    public INetworkDevice? GetNetworkDevice()
    {
        return VirtioDevice.GetDevice<VirtioNet>();
    }

    public uint GetCpuCount()
    {
        // For now, single CPU on ARM64
        return 1;
    }

    public void StartSchedulerTimer(uint quantumMs)
    {
        // Start the timer for preemptive scheduling. Honour quantumMs: the
        // period the timer was initialized with is the same 10 ms by
        // coincidence, and silently ignoring the parameter left the caller
        // believing it had set the tick interval on both architectures.
        Serial.WriteString("[ARM64HAL] Starting Generic Timer for scheduling...\n");
        if (_timer is not null)
        {
            _timer.SetPeriod(quantumMs * 1_000_000UL);
            _timer.Start();
        }
    }
}
