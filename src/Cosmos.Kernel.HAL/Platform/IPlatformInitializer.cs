// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Power;

namespace Cosmos.Kernel.HAL.Platform;

/// <summary>
/// Interface for platform-specific HAL initialization.
/// Implemented by HAL.X64 and HAL.ARM64.
/// </summary>
internal interface IPlatformInitializer
{
    /// <summary>
    /// Human-readable platform name (e.g. "x86-64", "ARM64").
    /// </summary>
    string PlatformName { get; }

    /// <summary>
    /// Architecture this initializer targets.
    /// </summary>
    PlatformArchitecture Architecture { get; }

    /// <summary>
    /// Creates the platform-specific port I/O implementation.
    /// </summary>
    IPortIO CreatePortIO();

    /// <summary>
    /// Creates the platform-specific CPU operations (halt, interrupt
    /// enable/disable, etc.).
    /// </summary>
    ICpuOps CreateCpuOps();

    /// <summary>
    /// Creates the platform-specific power operations (shutdown, reboot).
    /// </summary>
    IPowerOps CreatePowerOps();

    /// <summary>
    /// Creates the platform-specific interrupt controller.
    /// </summary>
    IInterruptController CreateInterruptController();

    /// <summary>
    /// Maps the 2 MiB block containing a physical MMIO address so its
    /// HHDM-virtual alias is accessible with Device-memory attributes.
    /// Called by the driver kit's register mapper and by its MSI-X table
    /// mapper before touching their BARs. ARM64
    /// installs a Device mapping in TTBR1 via
    /// <c>DeviceMapper.EnsureMapped</c>; x64 maps only blocks above 4 GiB,
    /// since Limine's page tables already cover the low 4 GiB.
    /// </summary>
    /// <param name="physBase">Physical base address of the MMIO region.</param>
    /// <returns>
    /// True when the block is mapped on return, including when it already
    /// was; false when it could not be mapped and its HHDM alias must not be
    /// dereferenced.
    /// </returns>
    bool EnsureMmioMapped(ulong physBase);

    /// <summary>
    /// Full data-synchronization barrier ordering prior normal-memory
    /// accesses against subsequent device MMIO accesses. DMA drivers call
    /// this between filling a descriptor/queue entry in RAM and ringing
    /// the device doorbell (and after observing a device-written flag
    /// before consuming the data it guards). ARM64 issues <c>dsb sy</c>;
    /// x64's total store order already provides this, so it's a no-op.
    /// </summary>
    void DmaBarrier();

    /// <summary>
    /// Busy-waits for at least <paramref name="microseconds"/> µs without
    /// depending on interrupts or the scheduler, so it is usable from phase-3
    /// device init. x64 paces on legacy port-0x80 reads (~1 µs each);
    /// ARM64 polls the generic timer (CNTVCT/CNTFRQ).
    /// </summary>
    void DelayMicroseconds(uint microseconds);

    /// <summary>
    /// Initializes platform-specific hardware (ACPI, APIC, GIC, timers, etc.).
    /// Called after HAL and interrupt manager are initialized.
    /// </summary>
    void InitializeHardware();

    /// <summary>
    /// Publishes the root platform nodes of this machine into the driver
    /// kit: on x64 the 8042 keyboard controller and the PCI host, on ARM64
    /// the ECAM host (from ACPI's MCFG, else from the device tree the
    /// bootloader handed over) and one node per occupied virtio-mmio slot
    /// (from the device tree, else from the virt machine's table), whatever
    /// the feature switches say. Called once from the HAL library
    /// initializer after <see cref="InitializeHardware"/>, with interrupts
    /// disabled; the nodes are offered when the driver stage runs, and a
    /// node's lines are only described here, connected by the driver that
    /// binds it.
    /// </summary>
    void PublishPlatformNodes();

    /// <summary>
    /// Creates and initializes the platform timer device.
    /// </summary>
    ITimerDevice CreateTimer();

    /// <summary>
    /// Gets the number of CPUs detected on this platform.
    /// </summary>
    uint GetCpuCount();

    /// <summary>
    /// Starts the platform timer for preemptive scheduling.
    /// Called after all initialization is complete.
    /// </summary>
    /// <param name="quantumMs">Scheduler time quantum in milliseconds.</param>
    void StartSchedulerTimer(uint quantumMs);
}
