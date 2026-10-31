// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Firmware;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.Core.RISCV64.Bridge;
using Cosmos.Kernel.Core.RISCV64.Cpu;
using Cosmos.Kernel.Core.RISCV64.IO;
using Cosmos.Kernel.Core.RISCV64.Power;
using Cosmos.Kernel.HAL.Boot;
using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Buses.Platform;
using Cosmos.Kernel.HAL.DriverKit.Interrupts;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.Timers;

namespace Cosmos.Kernel.HAL.RISCV64;

/// <summary>
/// RISC-V 64 platform initializer - creates the RISC-V HAL components for the
/// QEMU virt machine: the SBI calls, the PLIC, the supervisor timer and the
/// machine description, read from the device tree Limine hands over, with the
/// virt machine's fixed layout as the fallback.
/// </summary>
internal class RISCV64PlatformInitializer : IPlatformInitializer
{
    /// <summary>Number of microseconds in one second.</summary>
    private const ulong MicrosecondsPerSecond = 1_000_000UL;

    /// <summary>Physical base of the virt machine's virtio-mmio window: 8 slots of 0x1000 bytes.</summary>
    private const ulong VirtioMmioBase = 0x10001000;
    /// <summary>Bytes per virtio-mmio slot.</summary>
    private const ulong VirtioMmioSlotSize = 0x1000;
    /// <summary>Slots in the virtio-mmio window.</summary>
    private const uint VirtioMmioSlotCount = 8;
    /// <summary>PLIC source of slot 0's line: the slots' sources are consecutive (1 + slot).</summary>
    private const uint VirtioMmioIrqBase = 1;
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

    private SbiTimer? _timer;

    public string PlatformName => "RISCV64";
    public PlatformArchitecture Architecture => PlatformArchitecture.RISCV64;

    public IPortIO CreatePortIO() => new RISCV64PortIO();
    public ICpuOps CreateCpuOps() => new RISCV64CpuOps();
    public IPowerOps CreatePowerOps() => new RISCV64PowerOps();

    public IInterruptController CreateInterruptController()
    {
        // The device tree is recorded by now: the PLIC and the timebase
        // come from it before the controller initializes.
        Sbi.Initialize();
        Serial.WriteString("[RISCV64HAL] SBI specification 0x");
        Serial.WriteHex(Sbi.SpecVersion);
        Serial.WriteString("\n");

        ReadMachineDescription();
        return new RISCV64InterruptController();
    }

    public bool EnsureMmioMapped(ulong physBase)
    {
        // Address 0 is an unassigned BAR, not a device: nothing is mapped for it.
        if (physBase == 0)
        {
            return false;
        }

        return DeviceMapper.EnsureMapped(physBase);
    }

    public bool EnsureWriteCombiningMapped(ulong physBase)
    {
        // A page table entry has no memory type without Svpbmt: a framebuffer
        // gets the attributes the platform gives its range, as any window.
        return EnsureMmioMapped(physBase);
    }

    public void DmaBarrier()
    {
        // fence iorw, iorw: orders normal-memory descriptor writes against the
        // device-register doorbell store that follows, and device-written
        // flags against the payload reads that follow them.
        CsrNative.IoFence();
    }

    /// <inheritdoc />
    public void DelayMicroseconds(uint microseconds)
    {
        // The time CSR is readable from S-mode without any driver init.
        ulong freq = Timebase.Frequency;
        ulong target = CsrNative.ReadTime() + (freq * microseconds + (MicrosecondsPerSecond - 1UL)) / MicrosecondsPerSecond;
        while (CsrNative.ReadTime() < target)
        {
        }
    }

    /// <inheritdoc />
    public void InitializeHardware()
    {
        Serial.WriteString("[RISCV64HAL] Initializing the supervisor timer...\n");
        _timer = new SbiTimer();
        _timer.Initialize();

        Serial.WriteString("[RISCV64HAL] Registering timer interrupt handler...\n");
        _timer.RegisterIRQHandler();

        Serial.WriteString("[RISCV64HAL] Initializing RTC...\n");
        new GoldfishRtc().Initialize();
    }

    /// <summary>
    /// Reads what the boot needs from the device tree before the interrupt
    /// controller starts: the timebase frequency from <c>/cpus</c> and the
    /// PLIC's window and source count. Without a tree, or without the nodes,
    /// the virt machine's values stay. Thread context, interrupts disabled.
    /// </summary>
    private static void ReadMachineDescription()
    {
        DeviceTree? tree = BootFirmware.DeviceTree;
        if (tree is null)
        {
            Serial.WriteString("[RISCV64HAL] No device tree: QEMU virt layout assumed\n");
            Plic.Configure(Plic.QemuVirtBase, Plic.QemuVirtSupervisorContext, Plic.QemuVirtSourceCount);
            return;
        }

        bool more = tree.Root.TryGetFirstChild(out DeviceTreeNode node);
        while (more)
        {
            if (node.NameEquals("cpus") && node.TryReadU32("timebase-frequency", out uint timebase))
            {
                Timebase.SetFrequency(timebase);
                Serial.WriteString("[RISCV64HAL] Timebase from the device tree: ");
                Serial.WriteNumber(timebase);
                Serial.WriteString(" Hz\n");
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }

        DeviceTreeNode parent = DeviceParent(tree);
        more = parent.TryGetFirstChild(out node);
        while (more)
        {
            if ((node.IsCompatible("riscv,plic0") || node.IsCompatible("sifive,plic-1.0.0")) && !node.IsDisabled
                && node.TryReadReg(0, out ulong plicBase, out _))
            {
                uint sourceCount = node.TryReadU32("riscv,ndev", out uint ndev) ? ndev + 1 : Plic.QemuVirtSourceCount;

                // Hart 0's S-mode context, as on the virt machine, where
                // contexts come in M/S pairs per hart: the kernel runs on one
                // hart, the boot hart, which is hart 0 under QEMU.
                Plic.Configure(plicBase, Plic.QemuVirtSupervisorContext, sourceCount);
                return;
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }

        Serial.WriteString("[RISCV64HAL] No PLIC in the device tree: QEMU virt layout assumed\n");
    }

    /// <summary>
    /// The node the virt machine's devices hang from: <c>/soc</c> on RISC-V
    /// (the ARM64 virt machine puts them at the root), or the root when there
    /// is no such node. Thread context.
    /// </summary>
    /// <param name="tree">The device tree.</param>
    private static DeviceTreeNode DeviceParent(DeviceTree tree)
    {
        bool more = tree.Root.TryGetFirstChild(out DeviceTreeNode node);
        while (more)
        {
            if (node.NameEquals("soc"))
            {
                return node;
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }

        return tree.Root;
    }

    /// <summary>
    /// The virt machine description: one PCI host node over the ECAM window,
    /// from ACPI's MCFG when an entry exists, else from the device tree's
    /// <c>pci-host-ecam-generic</c> node, when PCI is compiled in; then one
    /// platform node per occupied virtio-mmio slot, from the device tree when
    /// there is one, else from the virt machine's fixed window. Thread
    /// context, interrupts disabled, from the HAL library initializer.
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
            Serial.WriteString("[RISCV64HAL] Publishing the ECAM PCI host node...\n");
            PciHostAccess host = PciHostAccess.ForEcam(mcfg.BaseAddress, mcfg.Segment, mcfg.StartBus, mcfg.EndBus);
            ulong windowLength = (ulong)(host.EndBus - host.StartBus + 1) << EcamBusShift;
            PlatformIdentity identity = new($"pci@{mcfg.BaseAddress:x}", ["pci-host-ecam-generic"]);
            DeviceResource[] resources = [DeviceResource.MemoryWindow(mcfg.BaseAddress, windowLength)];
            PlatformBus.Publish(identity, resources, [], host);
            return;
        }

        DeviceTree? tree = BootFirmware.DeviceTree;
        if (tree is not null && TryFindDeviceTreePciHost(DeviceParent(tree), out ulong ecamBase, out byte firstBus, out byte lastBus))
        {
            Serial.WriteString("[RISCV64HAL] Publishing the ECAM PCI host node from the device tree...\n");
            PciHostAccess host = PciHostAccess.ForEcam(ecamBase, 0, firstBus, lastBus);
            ulong windowLength = (ulong)(host.EndBus - host.StartBus + 1) << EcamBusShift;
            PlatformIdentity identity = new($"pci@{ecamBase:x}", ["pci-host-ecam-generic"]);
            DeviceResource[] resources = [DeviceResource.MemoryWindow(ecamBase, windowLength)];
            PlatformBus.Publish(identity, resources, [], host);
            return;
        }

        Serial.WriteString("[RISCV64HAL] No MCFG entry and no device tree PCI host: no PCI host node published\n");
    }

    /// <summary>
    /// Finds the first enabled <c>pci-host-ecam-generic</c> child of
    /// <paramref name="parent"/> with a reg window of at least one bus: its
    /// ECAM base and its bus range from <c>bus-range</c> (0 to 255 without
    /// the property), the last bus bounded by the buses the window holds.
    /// Thread context.
    /// </summary>
    private static bool TryFindDeviceTreePciHost(DeviceTreeNode parent, out ulong ecamBase, out byte firstBus, out byte lastBus)
    {
        ecamBase = 0;
        firstBus = 0;
        lastBus = 0;
        bool more = parent.TryGetFirstChild(out DeviceTreeNode node);
        while (more)
        {
            if (node.IsCompatible("pci-host-ecam-generic") && !node.IsDisabled
                && node.TryReadReg(0, out ulong address, out ulong length) && length >= EcamBusSize)
            {
                if (!node.TryReadBusRange(out firstBus, out lastBus))
                {
                    firstBus = 0;
                    lastBus = byte.MaxValue;
                }

                ulong windowLastBus = firstBus + (length >> EcamBusShift) - 1;
                if (windowLastBus < lastBus)
                {
                    lastBus = (byte)windowLastBus;
                }

                ecamBase = address;
                return true;
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }

        return false;
    }

    /// <summary>
    /// Publishes one "virtio,mmio" platform node per occupied virtio-mmio
    /// slot: from the device tree's nodes when the bootloader handed a tree
    /// over, else from the virt machine's fixed window. A slot whose magic
    /// does not match or whose device id reads 0 is empty and gets no node.
    /// Thread context, interrupts disabled.
    /// </summary>
    private static void PublishVirtioMmioNodes()
    {
        DeviceTree? tree = BootFirmware.DeviceTree;
        if (tree is null)
        {
            PublishVirtioMmioNodesFromTable();
            return;
        }

        Serial.WriteString("[RISCV64HAL] Publishing the virtio-mmio nodes from the device tree...\n");
        bool more = DeviceParent(tree).TryGetFirstChild(out DeviceTreeNode node);
        while (more)
        {
            if (node.IsCompatible("virtio,mmio") && !node.IsDisabled && node.TryReadReg(0, out ulong address, out ulong size))
            {
                uint source = node.TryReadPlicInterrupt(0, out uint plicSource) ? plicSource : 0;
                PublishVirtioMmioSlot(address, size, source);
            }

            more = node.TryGetNextSibling(out DeviceTreeNode next);
            node = next;
        }
    }

    /// <summary>
    /// Publishes one node per occupied slot of the virt machine's fixed
    /// virtio-mmio window, used when no device tree was handed over.
    /// Thread context, interrupts disabled.
    /// </summary>
    private static void PublishVirtioMmioNodesFromTable()
    {
        Serial.WriteString("[RISCV64HAL] Publishing the virtio-mmio nodes...\n");
        for (uint slot = 0; slot < VirtioMmioSlotCount; slot++)
        {
            PublishVirtioMmioSlot(VirtioMmioBase + slot * VirtioMmioSlotSize, VirtioMmioSlotSize, VirtioMmioIrqBase + slot);
        }
    }

    /// <summary>
    /// Publishes the virtio-mmio slot at <paramref name="address"/> when it is
    /// occupied, with its PLIC source as its line (none when 0). The slot is
    /// mapped before its registers are read; the windows sit in the low
    /// 4 GiB Limine maps, so this costs nothing on the virt machine.
    /// </summary>
    private static void PublishVirtioMmioSlot(ulong address, ulong size, uint source)
    {
        if (!DeviceMapper.EnsureMapped(address))
        {
            Serial.WriteString($"[RISCV64HAL] virtio_mmio@{address:x} not mapped: skipped\n");
            return;
        }

        ulong virt = address + AddressSpace.HhdmOffset;
        if (Native.MMIO.Read32(virt + VirtioMmioMagicRegister) != VirtioMmioMagic)
        {
            return;
        }

        if (Native.MMIO.Read32(virt + VirtioMmioDeviceIdRegister) == 0)
        {
            return;
        }

        InterruptSource[] interrupts = source != 0
            ? [new PlatformLineInterruptSource(source, PlicLineRouting.Instance)]
            : [];
        PlatformIdentity identity = new($"virtio_mmio@{address:x}", ["virtio,mmio"]);
        DeviceResource[] resources = [DeviceResource.MemoryWindow(address, size)];
        PlatformBus.Publish(identity, resources, interrupts, null);
    }

    public TickSource? CreateTickSource()
    {
        if (!CosmosFeatures.TimerEnabled)
        {
            return null;
        }

        if (_timer is null)
        {
            _timer = new SbiTimer();
            _timer.Initialize();
        }

        return _timer;
    }

    public uint GetCpuCount()
    {
        // For now, the boot hart only
        return 1;
    }

    public void StartSchedulerTimer(uint quantumMs)
    {
        Serial.WriteString("[RISCV64HAL] Starting the supervisor timer for scheduling...\n");
        if (_timer is not null)
        {
            _timer.SetPeriod(quantumMs * 1_000_000UL);
            _timer.Start();
        }
    }
}
