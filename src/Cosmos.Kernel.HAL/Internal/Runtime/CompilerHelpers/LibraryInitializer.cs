using Cosmos.Kernel;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.Core.Scheduler.Stride;
using Cosmos.Kernel.HAL;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Pci;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// This class is responsible for initializing the library and its dependencies. It is called by the runtime before any managed code is executed.
/// </summary>
internal class LibraryInitializer
{
    /// <summary>
    /// Initialize HAL, interrupts, PCI, and platform-specific hardware. This method is called by the runtime before any managed code is executed.
    /// </summary>
    public static void InitializeLibrary()
    {
        // Get the platform initializer (registered by HAL.X64 or HAL.ARM64 module initializer)
        IPlatformInitializer? initializer = PlatformHAL.Initializer;
        if (initializer is null)
        {
            Panic.Halt("No platform initializer registered. Reference Cosmos.Kernel.HAL.X64 or Cosmos.Kernel.HAL.ARM64.");
        }

        // Display architecture
        Serial.WriteString("[KERNEL]   - Architecture: ");
        Serial.WriteString(initializer.PlatformName);
        Serial.WriteString("\n");

        // Initialize platform-specific HAL
        Serial.WriteString("[KERNEL]   - Initializing HAL...\n");
        PlatformHAL.Initialize(initializer);

        // Initialize interrupts (skipped if CosmosEnableInterrupts=false)
        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Initializing interrupts...\n");
            InterruptManager.Initialize(initializer.CreateInterruptController());

            if (CosmosFeatures.PCIEnabled)
            {
                // Initialize PCI (requires interrupts for MSI/MSI-X)
                Serial.WriteString("[KERNEL]   - Initializing PCI...\n");
                ulong ecamBase = AcpiMcfg.GetEcamBase();
                initializer.PreparePciMapping(ecamBase);
                PciDevice.SetEcamBase(ecamBase);
                PciManager.Setup();
            }

            // Initialize platform-specific hardware (ACPI, APIC, GIC, timers, etc.)
            Serial.WriteString("[KERNEL]   - Initializing platform hardware...\n");
            initializer.InitializeHardware();

            // Bind drivers to virtio PCI devices on any architecture.
            // Must run after InitializeHardware: MSI-X routing needs the
            // platform MSI binder (LAPIC on x64, GICv3 ITS on ARM64).
            if (CosmosFeatures.PCIEnabled &&
                (CosmosFeatures.NetworkEnabled || CosmosFeatures.KeyboardEnabled || CosmosFeatures.MouseEnabled))
            {
                Serial.WriteString("[KERNEL]   - Scanning for virtio PCI devices...\n");
                VirtioDevice.InitializePciBus();
            }

            // Initialize storage controllers (AHCI for SATA, NVMe for PCIe).
            // Both drivers are architecture-independent and live in HAL.
            if (CosmosFeatures.StorageEnabled)
            {
                Serial.WriteString("[KERNEL]   - Initializing AHCI...\n");
                Ahci.Initialize();

                Serial.WriteString("[KERNEL]   - Initializing NVMe...\n");
                Nvme.Initialize();
            }
        }
    }
}
