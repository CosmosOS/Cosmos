// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// What the bootloader handed the kernel that the driver kit publishes as
/// devices of firmware provenance, or that the machine description reads:
/// today the boot framebuffer and the device tree. Recorded by the HAL
/// initializer, before the driver stage; the engine publishes the display
/// when it starts, the ARM64 description reads the tree when it publishes
/// its nodes.
/// </summary>
internal static class BootFirmware
{
    /// <summary>The framebuffer Limine handed over, or null when the bootloader answered with none. Written once by <see cref="DiscoverBootDisplay"/>.</summary>
    internal static FirmwareDisplay? BootDisplay { get; private set; }

    /// <summary>The device tree the bootloader handed over, or null when it answered with none or with a blob the parser refused. Written once by <see cref="DiscoverDeviceTree"/>; read by the machine description. Any context.</summary>
    internal static DeviceTree? DeviceTree { get; private set; }

    /// <summary>
    /// Reads Limine's framebuffer response and records the first
    /// framebuffer as <see cref="BootDisplay"/>, or nothing when there is
    /// none. Boot thread, from the HAL initializer, once.
    /// </summary>
    internal static unsafe void DiscoverBootDisplay()
    {
        LimineFramebufferResponse* response = Limine.Framebuffer.Response;
        if (response == null || response->FramebufferCount == 0)
        {
            return;
        }

        LimineFramebuffer* framebuffer = response->Framebuffers[0];
        if (framebuffer == null)
        {
            return;
        }

        BootDisplay = new FirmwareDisplay(framebuffer, DeviceMemory.HhdmOffset());
    }

    /// <summary>
    /// Reads Limine's device tree response and records the blob as
    /// <see cref="DeviceTree"/>, or nothing when there is none: Limine answers
    /// only when the firmware exposed a tree, which EDK2 on the virt machine
    /// does with ACPI off and never on q35. The address is a higher-half
    /// virtual address into bootloader-reclaimable memory, readable as given
    /// and never reclaimed by the kernel. Boot thread, from the HAL
    /// initializer, once; allocates the tree object.
    /// </summary>
    internal static unsafe void DiscoverDeviceTree()
    {
        LimineDTBResponse* response = Limine.DTB.Response;
        if (response == null || response->Address == null)
        {
            Serial.WriteString("[Firmware] No device tree\n");
            return;
        }

        if (!DeviceTree.TryOpen(response->Address, out DeviceTree? tree, out string? reason))
        {
            Serial.WriteString($"[Firmware] Device tree at 0x{(ulong)response->Address:X} refused: {reason}\n");
            return;
        }

        Serial.WriteString($"[Firmware] Device tree at 0x{tree.Address:X}, {tree.TotalSize} bytes\n");
        DeviceTree = tree;
    }
}
