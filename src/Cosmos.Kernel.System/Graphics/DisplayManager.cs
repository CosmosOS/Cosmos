// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// The displays the driver kit published, in the order their drivers bound.
/// <see cref="FullScreenCanvas"/> takes the first one when it builds a
/// canvas, and falls back to the framebuffer firmware left behind when the
/// list is empty.
/// </summary>
/// <remarks>
/// <para>
/// Internal, unlike the keyboard and mouse managers: a kernel reaches the
/// screen through <see cref="Canvas"/>'s static full-screen members, which
/// are the ring's single acquisition point, and a display is of no use to it
/// outside one.
/// </para>
/// <para>
/// Every member runs on whichever thread asked for the screen. Registration
/// happens in the driver pass, before any canvas exists, so the list is built
/// once and read many times; a driver bound later by a work item can still
/// add to it, which is why the array is replaced rather than mutated.
/// </para>
/// </remarks>
internal static class DisplayManager
{
    private static IGraphicDevice[]? s_displays;

    /// <summary>True when the kernel was built with graphics support.</summary>
    internal static bool IsEnabled => CosmosFeatures.GraphicsEnabled;

    /// <summary>How many displays the kit has published. Zero before the driver pass runs.</summary>
    internal static int DeviceCount => s_displays?.Length ?? 0;

    /// <summary>
    /// The display a canvas should drive, which is the first one published,
    /// or null when the kit bound no display driver and the boot framebuffer
    /// is all there is.
    /// </summary>
    internal static IGraphicDevice? Primary
    {
        get
        {
            IGraphicDevice[]? displays = s_displays;
            return displays is { Length: > 0 } ? displays[0] : null;
        }
    }

    /// <summary>
    /// Readies the manager, before the driver pass can publish anything into
    /// it. Registration is refused until this has run, as the mouse manager
    /// refuses one.
    /// </summary>
    internal static void Initialize() => s_displays ??= [];

    /// <summary>
    /// Takes a display the kit published. Called by the driver pass, or by a
    /// bound driver's work item.
    /// </summary>
    internal static void RegisterDisplay(IGraphicDevice display)
    {
        if (s_displays is null || display is null)
        {
            return;
        }

        s_displays = [.. s_displays, display];

        Core.IO.Serial.Write("[DisplayManager] Registered display, total: ");
        Core.IO.Serial.WriteNumber((uint)s_displays.Length);
        Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Forgets a display that is gone. The canvas driving it, if any, is
    /// given up first, so the next acquisition builds a fresh one against
    /// whatever is left rather than drawing into a device that has gone.
    /// A display that is not registered is left alone.
    /// </summary>
    internal static void UnregisterDisplay(IGraphicDevice display)
    {
        IGraphicDevice[]? displays = s_displays;
        if (displays is null || display is null)
        {
            return;
        }

        int index = IndexOf(displays, display);
        if (index < 0)
        {
            return;
        }

        IGraphicDevice[] remaining = new IGraphicDevice[displays.Length - 1];
        for (int i = 0, j = 0; i < displays.Length; i++)
        {
            if (i != index)
            {
                remaining[j++] = displays[i];
            }
        }

        s_displays = remaining;
        FullScreenCanvas.Disable();
    }

    private static int IndexOf(IGraphicDevice[] displays, IGraphicDevice display)
    {
        for (int i = 0; i < displays.Length; i++)
        {
            if (ReferenceEquals(displays[i], display))
            {
                return i;
            }
        }

        return -1;
    }
}
