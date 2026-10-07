// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Firmware;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Boot;
using Cosmos.Kernel.HAL.Devices.Display;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// Boot-time initializer of Cosmos.Kernel.HAL. ILC finds this type by name and calls <see cref="InitializeLibrary"/> before the kernel's entry point runs.
/// </summary>
internal static class LibraryInitializer
{
    /// <summary>
    /// Initialize the HAL, the interrupt controller, the platform hardware and the driver kit's platform nodes.
    /// </summary>
    public static void InitializeLibrary()
    {
        // Registered by the HAL.X64 or HAL.ARM64 module initializer.
        IPlatformInitializer? initializer = PlatformHAL.Initializer;
        if (initializer is null)
        {
            Panic.Halt("No platform initializer registered. Reference Cosmos.Kernel.HAL.X64 or Cosmos.Kernel.HAL.ARM64.");
        }

        Serial.WriteString("[KERNEL]   - Architecture: ");
        Serial.WriteString(initializer.PlatformName);
        Serial.WriteString("\n");

        Serial.WriteString("[KERNEL]   - Initializing HAL...\n");
        PlatformHAL.Initialize(initializer);

        // Record the device tree the bootloader handed over, if any, before the
        // interrupt controller and the machine description run: both may read it.
        // The heap is up (Core's initializer ran first), the blob is RAM in the
        // higher-half map and stays mapped for the kernel's lifetime.
        Serial.WriteString("[KERNEL]   - Recording the firmware device tree...\n");
        BootFirmware.DiscoverDeviceTree();

        // Skipped if CosmosEnableInterrupts=false.
        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Initializing interrupts...\n");
            InterruptManager.Initialize(initializer.CreateInterruptController());

            // ACPI, APIC, GIC, timers, etc.
            Serial.WriteString("[KERNEL]   - Initializing platform hardware...\n");
            initializer.InitializeHardware();

            // Seed the driver kit's platform bus with this machine's root
            // nodes: on x64 the 8042 keyboard controller and the PCI host, on
            // ARM64 the ECAM host from ACPI's MCFG or from the device tree and
            // one node per occupied virtio-mmio slot, from the device tree or
            // the virt machine's table. Interrupts are still disabled and the
            // nodes wait in the engine's queue until Kernel.Start runs the
            // driver stage. A machine description that throws costs the kit
            // its nodes, not the boot.
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
        }

        // Record the framebuffer the bootloader handed over, for the driver
        // stage to publish as the firmware display. Outside the interrupts
        // block: the framebuffer is there whenever graphics are on, as the
        // early console reads it. The graphics switch alone, so a kernel
        // that turned it off carries no firmware display.
        if (CosmosFeatures.GraphicsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Recording the firmware framebuffer...\n");
            FirmwareDisplay.DiscoverBoot();
        }
    }
}
