// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.Firmware;

/// <summary>
/// What the bootloader handed the kernel besides memory: the boot
/// framebuffer, the device tree and the boot time, read from Limine's
/// responses so that no layer above Core dereferences one. The driver kit
/// publishes the framebuffer as the firmware display, the ARM64 machine
/// description reads the tree when it publishes its nodes, and the platform
/// clocks fall back on the boot time when the EFI clock does not answer.
/// </summary>
internal static class BootFirmware
{
    /// <summary>The refresh rate reported when the EDID carries none.</summary>
    private const int DefaultRefreshRate = 60;

    /// <summary>The device tree the bootloader handed over, or null when it answered with none or with a blob the parser refused. Written once by <see cref="DiscoverDeviceTree"/>; read by the machine description. Any context.</summary>
    internal static DeviceTree? DeviceTree { get; private set; }

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

    /// <summary>
    /// Reads Limine's framebuffer response: the first framebuffer, with the
    /// refresh rate of the first detailed timing of the EDID Limine passed
    /// on. Any context; allocation-free.
    /// </summary>
    /// <param name="framebuffer">The framebuffer, or default when there is none.</param>
    /// <returns>False when the bootloader answered with no framebuffer.</returns>
    internal static unsafe bool TryGetFramebuffer(out BootFramebuffer framebuffer)
    {
        framebuffer = default;
        LimineFramebufferResponse* response = Limine.Framebuffer.Response;
        if (response == null || response->FramebufferCount == 0)
        {
            return false;
        }

        LimineFramebuffer* entry = response->Framebuffers[0];
        if (entry == null)
        {
            return false;
        }

        ulong address = (ulong)entry->Address;
        ulong hhdmOffset = AddressSpace.HhdmOffset;
        framebuffer = new BootFramebuffer(
            address,
            address >= hhdmOffset ? address - hhdmOffset : address,
            (int)entry->Width,
            (int)entry->Height,
            (int)entry->Pitch,
            entry->BitsPerPixel,
            ParseEdidRefreshRate(entry));
        return true;
    }

    /// <summary>
    /// Reads the boot time Limine recorded. Any context; allocation-free.
    /// </summary>
    /// <param name="unixSeconds">
    /// The boot time in Unix seconds (UTC), which a bootloader that could not
    /// read the clock leaves at zero or below; 0 when there is no response.
    /// </param>
    /// <returns>False when the bootloader answered with no boot time.</returns>
    internal static unsafe bool TryGetBootTime(out long unixSeconds)
    {
        LimineBootTimeResponse* response = Limine.BootTime.Response;
        if (response == null)
        {
            unixSeconds = 0;
            return false;
        }

        unixSeconds = response->BootTime;
        return true;
    }

    /// <summary>
    /// The refresh rate of the first detailed timing descriptor of the EDID
    /// Limine passed on, or <see cref="DefaultRefreshRate"/> when the EDID is
    /// absent, malformed, or the descriptor is not a timing.
    /// </summary>
    private static unsafe int ParseEdidRefreshRate(LimineFramebuffer* framebuffer)
    {
        if (framebuffer->EdidSize < 128 || framebuffer->Edid == null)
        {
            return DefaultRefreshRate;
        }

        byte* edid = (byte*)framebuffer->Edid;

        // The EDID header: 00 FF FF FF FF FF FF 00.
        if (edid[0] != 0x00 || edid[1] != 0xFF || edid[7] != 0x00)
        {
            return DefaultRefreshRate;
        }

        // The first detailed timing descriptor starts at byte 54.
        byte* dtd = edid + 54;

        // Pixel clock in 10 kHz units (bytes 0 and 1, little-endian). Zero
        // means the descriptor is not a timing.
        uint pixelClock = (uint)(dtd[0] | (dtd[1] << 8));
        if (pixelClock == 0)
        {
            return DefaultRefreshRate;
        }

        uint hActive = (uint)(dtd[2] | ((dtd[4] >> 4) << 8));
        uint hBlank = (uint)(dtd[3] | ((dtd[4] & 0xF) << 8));
        uint vActive = (uint)(dtd[5] | ((dtd[7] >> 4) << 8));
        uint vBlank = (uint)(dtd[6] | ((dtd[7] & 0xF) << 8));

        uint hTotal = hActive + hBlank;
        uint vTotal = vActive + vBlank;
        if (hTotal == 0 || vTotal == 0)
        {
            return DefaultRefreshRate;
        }

        int hz = (int)((pixelClock * 10_000) / (hTotal * vTotal));
        if (hz < 24 || hz > 360)
        {
            return DefaultRefreshRate;
        }

        return hz;
    }
}
