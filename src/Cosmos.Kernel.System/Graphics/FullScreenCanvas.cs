using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Hands out the canvas on the primary display the driver kit published,
/// caching one canvas until <see cref="Disable"/> drops it.
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
    /// Runs the cached canvas's <see cref="Canvas.Disable"/>, which releases
    /// the device resources a 3D canvas holds, and drops the cache, so a
    /// later acquisition builds a fresh canvas on the primary display. The
    /// display stays in its mode: there is no device-level text mode to
    /// return to. A second acquisition with another mode then switches the
    /// display under a console still holding the old canvas, whose output is
    /// clipped until it re-acquires.
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
    /// Gets the screen display canvas. The canvas's <see cref="Canvas.Mode"/> reflects the
    /// actual display mode (read from the display at construction); subsequent calls
    /// return the same canvas without resetting the mode, so callers always see the real
    /// screen width/height. A cached canvas whose display was withdrawn is
    /// dropped first, so the call rebuilds on the current primary display.
    /// </summary>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out, or no display is published.</exception>
    internal static Canvas Get()
    {
        ThrowIfGraphicsDisabled();
        DropIfWithdrawn();

        s_videoDriver ??= CreateVideoDriver(null);
        return s_videoDriver;
    }

    /// <summary>
    /// Gets the screen display canvas, changing the display mode to
    /// <paramref name="mode"/>. A cached canvas whose display was withdrawn
    /// is dropped first, so the mode is applied to a canvas on the current
    /// primary display rather than to a dead one.
    /// </summary>
    /// <param name="mode">The display mode to switch to.</param>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out, or no display is published.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The display can switch modes and does not support the mode.</exception>
    internal static Canvas Get(Mode mode)
    {
        ThrowIfGraphicsDisabled();
        DropIfWithdrawn();

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

    /// <summary>
    /// Creates the canvas on the primary display. A display that implements
    /// <see cref="ICanvas3DFactory"/> hands out its own <see cref="Canvas3D"/>,
    /// which then takes the requested mode through its setter, so a kernel
    /// discovers 3D capability with <c>canvas is Canvas3D</c>; every other
    /// display gets the framework canvas. Runs only after
    /// <see cref="ThrowIfGraphicsDisabled"/>: the switch folding after that
    /// throw is what keeps the display manager and the canvas out of a
    /// graphics-off kernel.
    /// </summary>
    /// <param name="mode">The mode to switch the display to, or null for its current or default mode.</param>
    /// <exception cref="InvalidOperationException">No display is published.</exception>
    private static Canvas CreateVideoDriver(Mode? mode)
    {
        DisplayDevice primary = DisplayManager.Primary
            ?? throw new InvalidOperationException("No display is published. The kernel has no framebuffer from the bootloader and no display driver bound a device.");

        if (primary.TryGetFacet(out ICanvas3DFactory? factory))
        {
            Canvas3D canvas3D = factory.CreateCanvas3D(primary);
            if (mode is Mode requested)
            {
                try
                {
                    canvas3D.Mode = requested;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // The canvas built its device resources in its constructor;
                    // a refused mode must not orphan them.
                    canvas3D.Disable();
                    throw;
                }
            }

            return canvas3D;
        }

        return new Canvas(primary, mode);
    }

    /// <summary>
    /// Runs <see cref="Disable"/> when the cached canvas sits on a display the
    /// kit has withdrawn (its driver unbound, or the firmware display retired
    /// after the canvas was acquired), so the next acquisition builds on the
    /// display that is primary now.
    /// </summary>
    private static void DropIfWithdrawn()
    {
        if (s_videoDriver is { IsDisplayWithdrawn: true })
        {
            Disable();
        }
    }

    /// <summary>
    /// Throws when graphics support is compiled out, before anything of the
    /// display path is touched.
    /// </summary>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out.</exception>
    private static void ThrowIfGraphicsDisabled()
    {
        if (!CosmosFeatures.GraphicsEnabled)
        {
            throw new InvalidOperationException("Graphics support is disabled. Set CosmosEnableGraphics=true in your csproj to enable it.");
        }
    }
}
