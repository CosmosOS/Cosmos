// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.ARM64.Cpu;
using Cosmos.Kernel.Core.ARM64.IO;
using Cosmos.Kernel.Core.ARM64.Power;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Platform;
using Cosmos.Kernel.HAL.Firmware;
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

    /// <summary>Physical base of the virt machine's virtio-mmio window: 32 slots of 0x200 bytes, the machine's hardcoded table, not an ACPI node.</summary>
    private const ulong VirtioMmioBase = 0x0a000000;
    /// <summary>Bytes per virtio-mmio slot: one register window.</summary>
    private const ulong VirtioMmioSlotSize = 0x200;
    /// <summary>Slots in the virtio-mmio window.</summary>
    private const uint VirtioMmioSlotCount = 32;
    /// <summary>INTID of slot 0's line: SPI 16, the slots' SPIs are consecutive (INTID 48 + slot).</summary>
    private const uint VirtioMmioIrqBase = 48;
    /// <summary>Offset of the virtio-mmio MagicValue register in a slot (virtio 4.2.2).</summary>
    private const ulong VirtioMmioMagicRegister = 0x000;
    /// <summary>Offset of the virtio-mmio DeviceID register in a slot: 0 in an empty slot.</summary>
    private const ulong VirtioMmioDeviceIdRegister = 0x008;
    /// <summary>The MagicValue a virtio-mmio slot reads: "virt" in little endian.</summary>
    private const uint VirtioMmioMagic = 0x74726976;

    /// <summary>Shift of the bus number in an ECAM address: 1 MiB of configuration space per bus.</summary>
    private const int EcamBusShift = 20;
    /// <summary>Bytes of ECAM per bus, the smallest window that describes one bus.</summary>
    private const ulong EcamBusSize = 1UL << EcamBusShift;

    private GenericTimer? _timer;

    public string PlatformName => "ARM64";
    public PlatformArchitecture Architecture => PlatformArchitecture.ARM64;

    public IPortIO CreatePortIO() => new ARM64MemoryIO();
    public ICpuOps CreateCpuOps() => new ARM64CpuOps();
    public IPowerOps CreatePowerOps() => new ARM64PowerOps();
    public IInterruptController CreateInterruptController() => new ARM64InterruptController();

    public bool EnsureMmioMapped(ulong physBase)
    {
        // Limine's HHDM on aarch64 only covers RAM with Normal-cacheable
        // attributes; device MMIO has to be mapped explicitly as Device
        // memory so register reads/writes aren't reordered or cached.
        // Safe to call repeatedly: DeviceMapper.EnsureMapped no-ops if the
        // mapping already exists. Address 0 is an unassigned BAR, not a
        // device: nothing is mapped for it.
        if (physBase == 0)
        {
            return false;
        }

        return DeviceMapper.EnsureMapped(physBase);
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
    }

    /// <summary>
    /// The virt machine description, over the design's three sources in
    /// order of preference. One PCI host node over the ECAM window, from
    /// ACPI's MCFG when an entry exists, else from the device tree's
    /// <c>pci-host-ecam-generic</c> node when the bootloader handed a tree
    /// over, else none (the machine then has no PCI), when PCI
    /// is compiled in. Then one platform node per occupied virtio-mmio
    /// slot, always: from the device tree's <c>virtio,mmio</c> nodes when
    /// there is a tree, else from the virt machine's hardcoded window; a
    /// slot is occupied when its magic and device id registers say so,
    /// whichever source named it. Thread context, interrupts disabled,
    /// from the HAL library initializer; the lines are only described
    /// here, the transport driver connects them at bind time.
    /// </summary>
    public void PublishPlatformNodes()
    {
        PublishPciHostNode();
        PublishVirtioMmioNodes();
    }

    /// <summary>
    /// Publishes the ECAM PCI host node from ACPI's MCFG, else from the
    /// device tree; nothing when PCI is compiled out or neither source
    /// names a host. Thread context, interrupts disabled.
    /// </summary>
    private static void PublishPciHostNode()
    {
        if (!CosmosFeatures.PCIEnabled)
        {
            return;
        }

        if (AcpiMcfg.TryGetInfo(out AcpiMcfg.McfgInfo mcfg))
        {
            Serial.WriteString("[ARM64HAL] Publishing the ECAM PCI host node...\n");
            PciHostAccess host = PciHostAccess.ForEcam(mcfg.BaseAddress, mcfg.Segment, mcfg.StartBus, mcfg.EndBus);

            // The window the node reports is the host's range, not the table's:
            // the access ends its range at the last bus it could map.
            ulong windowLength = (ulong)(host.EndBus - host.StartBus + 1) << EcamBusShift;
            PlatformIdentity identity = new($"pci@{mcfg.BaseAddress:x}", ["pci-host-ecam-generic"]);
            DeviceResource[] resources = [DeviceResource.MemoryWindow(mcfg.BaseAddress, windowLength)];
            PlatformBus.Publish(identity, resources, [], host);
            return;
        }

        DeviceTree? tree = BootFirmware.DeviceTree;
        if (tree is not null && TryFindDeviceTreePciHost(tree, out ulong ecamBase, out ulong ecamLength, out byte firstBus, out byte lastBus))
        {
            Serial.WriteString("[ARM64HAL] Publishing the ECAM PCI host node from the device tree...\n");

            // Segment 0: the tree names none and linux,pci-domain is 0. The
            // bus range is the smaller of bus-range and the buses the reg
            // window holds, so the host's range differs from it only when
            // ForEcam clamped it at a megabyte it could not map; the window
            // compared is the one those buses span, not the reg length,
            // which a narrower bus-range leaves partly unused.
            PciHostAccess host = PciHostAccess.ForEcam(ecamBase, 0, firstBus, lastBus);
            ulong requestedLength = (ulong)(lastBus - firstBus + 1) << EcamBusShift;
            ulong windowLength = (ulong)(host.EndBus - host.StartBus + 1) << EcamBusShift;
            if (windowLength != requestedLength)
            {
                Serial.WriteString($"[ARM64HAL] device tree ECAM window is 0x{requestedLength:X} bytes, the host maps 0x{windowLength:X}\n");
            }

            PlatformIdentity identity = new($"pci@{ecamBase:x}", ["pci-host-ecam-generic"]);
            DeviceResource[] resources = [DeviceResource.MemoryWindow(ecamBase, windowLength)];
            PlatformBus.Publish(identity, resources, [], host);
            return;
        }

        Serial.WriteString("[ARM64HAL] No MCFG entry and no device tree PCI host: no PCI host node published\n");
    }

    /// <summary>
    /// Finds the first enabled <c>pci-host-ecam-generic</c> child of the
    /// tree's root with a reg window of at least one bus: its ECAM base
    /// and length, and its bus range from <c>bus-range</c> (0 to 255
    /// without the property), the last bus bounded by the buses the
    /// window holds. Thread context; allocation-free (the skip lines are
    /// written piece by piece).
    /// </summary>
    /// <param name="tree">The device tree.</param>
    /// <param name="ecamBase">The ECAM window's physical base.</param>
    /// <param name="ecamLength">The ECAM window's length in bytes.</param>
    /// <param name="firstBus">The first bus the host decodes.</param>
    /// <param name="lastBus">The last bus the host decodes.</param>
    private static bool TryFindDeviceTreePciHost(DeviceTree tree, out ulong ecamBase, out ulong ecamLength, out byte firstBus, out byte lastBus)
    {
        ecamBase = 0;
        ecamLength = 0;
        firstBus = 0;
        lastBus = 0;
        bool more = tree.Root.TryGetFirstChild(out DeviceTreeNode node);
        while (more)
        {
            if (node.IsCompatible("pci-host-ecam-generic") && !node.IsDisabled)
            {
                if (!node.TryReadReg(0, out ulong address, out ulong length))
                {
                    Serial.WriteString("[ARM64HAL] device tree PCI host without reg: skipped\n");
                }
                else if (length < EcamBusSize)
                {
                    Serial.WriteString("[ARM64HAL] device tree PCI host with a ");
                    Serial.WriteNumber(length);
                    Serial.WriteString(" byte window: skipped\n");
                }
                else
                {
                    if (!node.TryReadBusRange(out firstBus, out lastBus))
                    {
                        firstBus = 0;
                        lastBus = byte.MaxValue;
                    }

                    // The reg length is the binding's own bound on the buses:
                    // ForEcam clamps only when a megabyte cannot be mapped, and
                    // past the window lies RAM the HHDM already maps.
                    ulong windowBuses = length >> EcamBusShift;
                    ulong windowLastBus = firstBus + windowBuses - 1;
                    if (windowLastBus < lastBus)
                    {
                        lastBus = (byte)windowLastBus;
                    }

                    ecamBase = address;
                    ecamLength = length;
                    return true;
                }
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }

        return false;
    }

    /// <summary>
    /// Publishes one "virtio,mmio" platform node per occupied virtio-mmio
    /// slot, whatever the feature switches and whether or not ACPI
    /// described anything: from the device tree's nodes when the
    /// bootloader handed a tree over, else from the virt machine's
    /// hardcoded window. A slot whose magic does not match or whose device
    /// id reads 0 is empty and gets no node, whichever source named it;
    /// the transport driver validates the slot again at bind time. Thread
    /// context, interrupts disabled.
    /// </summary>
    private static void PublishVirtioMmioNodes()
    {
        DeviceTree? tree = BootFirmware.DeviceTree;
        if (tree is null)
        {
            PublishVirtioMmioNodesFromTable();
            return;
        }

        Serial.WriteString("[ARM64HAL] Publishing the virtio-mmio nodes from the device tree...\n");
        bool more = tree.Root.TryGetFirstChild(out DeviceTreeNode node);
        while (more)
        {
            if (node.IsCompatible("virtio,mmio") && !node.IsDisabled)
            {
                PublishVirtioMmioNodeFromTree(node);
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }
    }

    /// <summary>
    /// Publishes one <c>virtio,mmio</c> node of the device tree when its
    /// slot is occupied: the window is Device-mapped before its registers
    /// are read (a mapping already installed is a no-op, so the 32 slots
    /// cost one), the magic and device id probed as the table path does,
    /// and the GIC line taken from the first <c>interrupts</c> entry when
    /// it is a routable SPI; the flags cell is read and ignored, every
    /// routed line is programmed level-triggered. Thread context.
    /// </summary>
    /// <param name="node">The tree node.</param>
    private static void PublishVirtioMmioNodeFromTree(DeviceTreeNode node)
    {
        if (!node.TryReadReg(0, out ulong address, out ulong size))
        {
            return;
        }

        if (!DeviceMapper.EnsureMapped(address))
        {
            Serial.WriteString($"[ARM64HAL] virtio_mmio@{address:x} not mapped: skipped\n");
            return;
        }

        if (Native.MMIO.Read32(PhysToVirt(address + VirtioMmioMagicRegister)) != VirtioMmioMagic)
        {
            return;
        }

        if (Native.MMIO.Read32(PhysToVirt(address + VirtioMmioDeviceIdRegister)) == 0)
        {
            return;
        }

        InterruptSource[] interrupts;
        if (node.TryReadInterrupt(0, out uint type, out uint number, out uint flags))
        {
            // The number is bounded before GicSpiBase is added, so a huge
            // number cannot wrap into an SGI or PPI INTID.
            if (type == DeviceTreeFormat.GicSpiType && number <= GicLineRouting.LastTableLine - DeviceTreeFormat.GicSpiBase)
            {
                interrupts = [new PlatformLineInterruptSource(DeviceTreeFormat.GicSpiBase + number, GicLineRouting.Instance)];
            }
            else
            {
                interrupts = [];
                Serial.WriteString($"[ARM64HAL] virtio_mmio@{address:x}: interrupt type {type} number {number} flags {flags} is not a routable SPI, published without a line\n");
            }
        }
        else
        {
            interrupts = [];
            Serial.WriteString($"[ARM64HAL] virtio_mmio@{address:x}: no interrupts property, published without a line\n");
        }

        PlatformIdentity identity = new($"virtio_mmio@{address:x}", ["virtio,mmio"]);
        DeviceResource[] resources = [DeviceResource.MemoryWindow(address, size)];
        PlatformBus.Publish(identity, resources, interrupts, null);
    }

    /// <summary>
    /// Publishes one "virtio,mmio" platform node per occupied slot of the
    /// virt machine's virtio-mmio window, the hardcoded table used when no
    /// device tree was handed over: the slot's register window and its GIC
    /// line, routed through <see cref="GicLineRouting"/>. A slot whose
    /// magic does not match or whose device id reads 0 is empty and gets
    /// no node. Thread context, interrupts disabled.
    /// </summary>
    private static void PublishVirtioMmioNodesFromTable()
    {
        // The window is Device-mapped before its registers are read (the
        // HHDM alias of an unmapped device address faults); this is the one
        // place the window is mapped, the transport driver maps its slot
        // again through the kit's register window when it binds.
        if (!DeviceMapper.EnsureMapped(VirtioMmioBase))
        {
            Serial.WriteString("[ARM64HAL] virtio-mmio window not mapped: no virtio-mmio nodes published\n");
            return;
        }

        Serial.WriteString("[ARM64HAL] Publishing the virtio-mmio nodes...\n");
        for (uint slot = 0; slot < VirtioMmioSlotCount; slot++)
        {
            ulong slotBase = VirtioMmioBase + slot * VirtioMmioSlotSize;
            if (Native.MMIO.Read32(PhysToVirt(slotBase + VirtioMmioMagicRegister)) != VirtioMmioMagic)
            {
                continue;
            }

            if (Native.MMIO.Read32(PhysToVirt(slotBase + VirtioMmioDeviceIdRegister)) == 0)
            {
                continue;
            }

            uint line = VirtioMmioIrqBase + slot;
            PlatformIdentity identity = new($"virtio_mmio@{slotBase:x}", ["virtio,mmio"]);
            DeviceResource[] resources = [DeviceResource.MemoryWindow(slotBase, VirtioMmioSlotSize)];
            InterruptSource[] interrupts = [new PlatformLineInterruptSource(line, GicLineRouting.Instance)];
            PlatformBus.Publish(identity, resources, interrupts, null);
        }
    }

    /// <summary>
    /// The HHDM alias of a physical device address: the Device-memory
    /// mapping <see cref="DeviceMapper"/> installs lives under Limine's
    /// higher-half offset, and the raw physical address would hit the
    /// cacheable identity mapping. The address itself without an HHDM
    /// response. Any context.
    /// </summary>
    /// <param name="phys">The physical address.</param>
    private static unsafe ulong PhysToVirt(ulong phys)
    {
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        if (hhdmOffset != 0 && phys < hhdmOffset)
        {
            return phys + hhdmOffset;
        }

        return phys;
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
