// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>A display's geometry and pixel format.</summary>
internal readonly struct DisplayMode
{
    /// <summary>Creates a mode.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="pitch">Bytes per row of the framebuffer.</param>
    /// <param name="bitsPerPixel">Bits per pixel.</param>
    public DisplayMode(int width, int height, int pitch, int bitsPerPixel)
    {
        Width = width;
        Height = height;
        Pitch = pitch;
        BitsPerPixel = bitsPerPixel;
    }

    /// <summary>Width in pixels.</summary>
    public int Width { get; }

    /// <summary>Height in pixels.</summary>
    public int Height { get; }

    /// <summary>Bytes per row of the framebuffer.</summary>
    public int Pitch { get; }

    /// <summary>Bits per pixel.</summary>
    public int BitsPerPixel { get; }
}
