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
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Firmware;
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

            // Seed the driver kit's platform bus with this machine's root
            // nodes: the PCI host, and on ARM64 the occupied slots of the
            // virt machine's virtio-mmio window. Interrupts are still
            // disabled and the nodes wait in the engine's queue until
            // Kernel.Start runs the driver stage. A machine description
            // that throws costs the kit its nodes, not the boot.
            Serial.WriteString("[KERNEL]   - Publishing platform nodes...\n");
            try
            {
                initializer.PublishPlatformNodes();
            }
            catch (Exception exception)
            {
                Serial.WriteString("[KERNEL]   - Platform nodes not published: ");
                Serial.WriteString(exception.Message);
                Serial.WriteString("\n");
            }

            // Bring up USB host controllers and enumerate the devices behind
            // them. Must run after InitializeHardware: MSI-X routing needs
            // the platform MSI binder (LAPIC on x64, GICv3 ITS on ARM64). USB's own switch
            // alone, not PCI && (Keyboard || Storage): Sdk.targets already
            // turns it off with PCI and derives its default from Keyboard and
            // Storage, and a compound guard does not fold in Debug IL, so ILC
            // would keep the whole USB stack in a kernel that turned it off.
            if (CosmosFeatures.UsbEnabled)
            {
                Serial.WriteString("[KERNEL]   - Initializing USB...\n");
                UsbManager.Initialize();
            }
        }

        // Record the framebuffer the bootloader handed over, for the driver
        // stage to publish as the firmware display. Outside the interrupts
        // block: the framebuffer is there whenever graphics are on, as the
        // early console reads it. The graphics switch alone, so a kernel
        // that turned it off carries no firmware display.
        if (CosmosFeatures.GraphicsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Recording the firmware framebuffer...\n");
            BootFirmware.DiscoverBootDisplay();
        }
    }
}
