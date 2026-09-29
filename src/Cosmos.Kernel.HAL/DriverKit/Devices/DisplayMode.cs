// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>A display's geometry, pixel format and refresh rate. A mode is not the display's identity: it changes when the canvas switches it.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct DisplayMode
{
    /// <summary>Creates a mode.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="pitch">Bytes per row of the framebuffer.</param>
    /// <param name="bitsPerPixel">Bits per pixel.</param>
    public DisplayMode(int width, int height, int pitch, int bitsPerPixel)
        : this(width, height, pitch, bitsPerPixel, 0)
    {
    }

    /// <summary>Creates a mode with a known refresh rate.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="pitch">Bytes per row of the framebuffer.</param>
    /// <param name="bitsPerPixel">Bits per pixel.</param>
    /// <param name="refreshRate">Refresh rate in Hz, 0 when unknown.</param>
    public DisplayMode(int width, int height, int pitch, int bitsPerPixel, int refreshRate)
    {
        Width = width;
        Height = height;
        Pitch = pitch;
        BitsPerPixel = bitsPerPixel;
        RefreshRate = refreshRate;
    }

    /// <summary>Width in pixels.</summary>
    public int Width { get; }

    /// <summary>Height in pixels.</summary>
    public int Height { get; }

    /// <summary>Bytes per row of the framebuffer.</summary>
    public int Pitch { get; }

    /// <summary>Bits per pixel.</summary>
    public int BitsPerPixel { get; }

    /// <summary>Refresh rate in Hz; 0 when unknown.</summary>
    public int RefreshRate { get; }

    /// <summary>True for the unprogrammed case: an adapter whose scanout the driver has not programmed yet.</summary>
    public bool IsEmpty => Width == 0 || Height == 0;
}
