using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Provides functionality to fetch canvases that write directly to the
/// underlying display device.
/// </summary>
public static class FullScreenCanvas
{
    /// <summary>
    /// Whether the CGS (Cosmos Graphics Subsystem) is currently in use.
    /// </summary>
    public static bool IsInUse { get; private set; }

    /// <summary>
    /// Disables the specified graphics driver used, and returns to VGA text mode 80x25.
    /// </summary>
    public static void Disable()
    {
        if (IsInUse)
        {
            s_videoDriver!.Disable();
            IsInUse = false;
        }
    }

    private static Canvas? s_videoDriver = null;

    /// <summary>
    /// Gets a <see cref="Canvas"/> instance, using an implementation based on
    /// the currently used video driver.
    /// </summary>
    private static Canvas GetVideoDriver()
    {
        if (CosmosFeatures.PCIEnabled)
        {
            PciDevice? svgaDevice = PciManager.GetDevice(VendorId.VmWare, DeviceId.SvgaiiAdapter);
            if (svgaDevice is not null)
            {
                return new SVGAII3DCanvas(svgaDevice);
            }
        }

        return new GopCanvas();
    }

    /// <summary>
    /// Gets a <see cref="Canvas"/> instance, using an implementation based on
    /// the currently used video driver, constructing the canvas with the given
    /// <paramref name="mode"/>.
    /// </summary>
    private static Canvas GetVideoDriver(Mode mode)
    {
        if (CosmosFeatures.PCIEnabled)
        {
            PciDevice? svgaDevice = PciManager.GetDevice(VendorId.VmWare, DeviceId.SvgaiiAdapter);
            if (svgaDevice is not null)
            {
                return new SVGAII3DCanvas(svgaDevice);
            }
        }

        return new GopCanvas(mode);
    }

    /// <summary>
    /// Gets the screen display canvas. The canvas's <see cref="Canvas.Mode"/> reflects the
    /// actual framebuffer resolution (set by the driver at construction); subsequent calls
    /// return the same canvas without resetting the mode, so callers always see the real
    /// screen width/height.
    /// </summary>
    public static Canvas GetFullScreenCanvas()
    {
        if (!CosmosFeatures.GraphicsEnabled)
        {
            throw new InvalidOperationException("Graphics support is disabled. Set CosmosEnableGraphics=true in your csproj to enable it.");
        }

        s_videoDriver ??= GetVideoDriver();

        IsInUse = true;
        return s_videoDriver;
    }

    /// <summary>
    /// Gets a screen display canvas, and changes the display mode to the given <paramref name="mode"/>.
    /// </summary>
    public static Canvas GetFullScreenCanvas(Mode mode)
    {
        if (!CosmosFeatures.GraphicsEnabled)
        {
            throw new InvalidOperationException("Graphics support is disabled. Set CosmosEnableGraphics=true in your csproj to enable it.");
        }

        if (s_videoDriver == null)
        {
            s_videoDriver = GetVideoDriver(mode);
        }
        else
        {
            s_videoDriver.Mode = mode;
        }

        IsInUse = true;
        return s_videoDriver;
    }

    /// <summary>
    /// Attempts to get a screen display canvas, and changes the display mode to the default.
    /// </summary>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetFullScreenCanvas(Mode mode, out Canvas? canvas)
    {
        try
        {
            canvas = GetFullScreenCanvas(mode);
            IsInUse = true;
            return true;
        }
        catch
        {
        }

        canvas = null;
        return false;
    }

    /// <summary>
    /// Gets the currently used screen display canvas.
    /// </summary>
    public static Canvas? GetCurrentFullScreenCanvas()
    {
        return s_videoDriver;
    }
}
