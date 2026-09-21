using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;
using Cosmos.Kernel.HAL.Devices.Graphic.Virtio;
using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Detects the display device and hands out the canvas that drives it,
/// caching one canvas until <see cref="Disable"/> gives the screen back.
/// </summary>
/// <remarks>
/// Internal: a kernel reaches all of this through <see cref="Canvas"/>'s
/// static full-screen members, which are the ring's single acquisition point.
/// </remarks>
internal static class FullScreenCanvas
{
    private static Canvas? s_videoDriver;

    /// <summary>
    /// The canvas currently driving the screen, or <see langword="null"/> when
    /// nothing has acquired it yet or the last one was disabled.
    /// </summary>
    internal static Canvas? Current => s_videoDriver;

    /// <summary>
    /// Returns the display device to text mode and drops the cached canvas, so
    /// a later acquisition builds a fresh one against a re-enabled device.
    /// Keeping the disabled canvas cached would hand it straight back from the
    /// next acquisition with the device still off.
    /// </summary>
    internal static void Disable()
    {
        if (s_videoDriver is null)
        {
            return;
        }

        s_videoDriver.Disable();
        s_videoDriver = null;
    }

    /// <summary>
    /// Creates the canvas matching the detected display device. Order matters:
    /// virtio-gpu is preferred over SVGA-II when both are present (the test
    /// suite explicitly wires virtio-gpu-pci to check the new path), and GOP
    /// is the fallback when neither PCI device is bound. On the VMware SVGA II
    /// adapter the canvas type depends on whether the device negotiated 3D
    /// support, so users can discover 3D capability with
    /// <c>canvas is Canvas3D</c>.
    /// </summary>
    private static Canvas CreateVideoDriver(Mode? mode)
    {
        // virtio-gpu takes priority: a kernel that wired -device virtio-gpu-pci
        // expects its driver to drive the display even when vmware-svga is
        // also on the PCI bus.
        if (CosmosFeatures.PCIEnabled && CosmosFeatures.GraphicsEnabled)
        {
            VirtioGpu? virtioGpu = VirtioDevice.GetDevice<VirtioGpu>();
            if (virtioGpu is not null && virtioGpu.Ready)
            {
                return new VirtioGpuCanvas(virtioGpu);
            }
        }

        if (CosmosFeatures.PCIEnabled)
        {
            PciDevice? svgaDevice = PciManager.GetDevice(VendorId.VmWare, DeviceId.SvgaiiAdapter);
            if (svgaDevice is not null)
            {
                SvgaIIDriver driver = new(svgaDevice);
                Mode svgaMode = mode ?? SvgaIIRender.DefaultMode;

                return driver.Is3DEnabled
                    ? new SvgaII3DCanvas(driver, svgaMode)
                    : new SvgaIICanvas(driver, svgaMode);
            }
        }

        return mode is null ? new GopCanvas() : new GopCanvas(mode.Value);
    }

    /// <summary>
    /// Gets the screen display canvas. The canvas's <see cref="Canvas.Mode"/> reflects the
    /// actual framebuffer resolution (set by the driver at construction); subsequent calls
    /// return the same canvas without resetting the mode, so callers always see the real
    /// screen width/height.
    /// </summary>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out.</exception>
    internal static Canvas Get()
    {
        ThrowIfGraphicsDisabled();

        s_videoDriver ??= CreateVideoDriver(null);
        return s_videoDriver;
    }

    /// <summary>
    /// Gets the screen display canvas, changing the display mode to
    /// <paramref name="mode"/>.
    /// </summary>
    /// <param name="mode">The display mode to switch to.</param>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The display device does not support the mode.</exception>
    internal static Canvas Get(Mode mode)
    {
        ThrowIfGraphicsDisabled();

        if (s_videoDriver is null)
        {
            s_videoDriver = CreateVideoDriver(mode);
        }
        else
        {
            s_videoDriver.Mode = mode;
        }

        return s_videoDriver;
    }

    private static void ThrowIfGraphicsDisabled()
    {
        if (!CosmosFeatures.GraphicsEnabled)
        {
            throw new InvalidOperationException("Graphics support is disabled. Set CosmosEnableGraphics=true in your csproj to enable it.");
        }
    }
}
