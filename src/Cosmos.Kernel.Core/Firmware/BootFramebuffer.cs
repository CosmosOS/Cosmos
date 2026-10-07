// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Firmware;

/// <summary>
/// The framebuffer the bootloader handed the kernel, as
/// <see cref="BootFirmware.TryGetFramebuffer"/> read it. Limine already
/// mapped it at <see cref="Address"/>.
/// </summary>
internal readonly struct BootFramebuffer
{
    /// <summary>The virtual address of the first byte, as Limine mapped it.</summary>
    internal ulong Address { get; }

    /// <summary>The physical address of the first byte.</summary>
    internal ulong PhysicalAddress { get; }

    /// <summary>Width in pixels.</summary>
    internal int Width { get; }

    /// <summary>Height in pixels.</summary>
    internal int Height { get; }

    /// <summary>Bytes from the start of one row to the start of the next.</summary>
    internal int Pitch { get; }

    /// <summary>Bits per pixel.</summary>
    internal int BitsPerPixel { get; }

    /// <summary>The refresh rate in Hz, from the EDID, or 60 when the EDID carries none.</summary>
    internal int RefreshRate { get; }

    internal BootFramebuffer(ulong address, ulong physicalAddress, int width, int height, int pitch, int bitsPerPixel, int refreshRate)
    {
        Address = address;
        PhysicalAddress = physicalAddress;
        Width = width;
        Height = height;
        Pitch = pitch;
        BitsPerPixel = bitsPerPixel;
        RefreshRate = refreshRate;
    }
}
