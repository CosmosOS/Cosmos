// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// What the bootloader handed the kernel that the driver kit publishes as
/// devices of firmware provenance: today the boot framebuffer. Recorded by
/// the HAL initializer, before the driver stage; the engine publishes it
/// when it starts.
/// </summary>
internal static class BootFirmware
{
    /// <summary>The framebuffer Limine handed over, or null when the bootloader answered with none. Written once by <see cref="DiscoverBootDisplay"/>.</summary>
    internal static FirmwareDisplay? BootDisplay { get; private set; }

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
}
