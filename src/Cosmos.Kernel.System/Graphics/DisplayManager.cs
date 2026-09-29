// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// The displays the kit published, and which one is primary: the firmware
/// framebuffer the bootloader handed over, published by the engine before
/// it starts, and every display a kit driver publishes, which the manager's
/// <see cref="KitDisplayConsumer"/> lists from the kit worker and drops when
/// it is withdrawn. <see cref="Canvas.GetFullScreen()"/> draws on the
/// primary display. Reads answer honestly before <see cref="Initialize"/>
/// ran and when graphics are compiled out: 0, null, false.
/// </summary>
public static class DisplayManager
{
    private static KitDisplayConsumer? s_consumer;

    /// <summary>
    /// Whether graphics support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.GraphicsEnabled;

    /// <summary>How many displays are published now. Any context.</summary>
    public static int Count => s_consumer?.Count ?? 0;

    /// <summary>The primary display, or null when none is published. Any context.</summary>
    public static DisplayDevice? Primary => s_consumer?.Primary;

    /// <summary>
    /// Throws when graphics support is compiled out. Guards the one action
    /// here, <see cref="Initialize"/>; the reads answer honestly instead.
    /// </summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Graphics support is disabled. Set CosmosEnableGraphics=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Installs the consumer. Called once during boot from the library
    /// initializer, before the driver stage runs, so the consumer sees the
    /// firmware display the engine publishes at its start and every display
    /// a kit driver publishes after it.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_consumer is not null)
        {
            return;
        }

        s_consumer = new KitDisplayConsumer();
        DeviceRegistry.SetConsumer(DeviceKind.Display, s_consumer);
    }

    /// <summary>The display at <paramref name="index"/> in the manager's list (primary first). Any context.</summary>
    /// <param name="index">Position, from 0 to <see cref="Count"/> exclusive.</param>
    /// <param name="display">The display at that position.</param>
    /// <returns>False when the index is out of range, which before <see cref="Initialize"/> ran is every index.</returns>
    public static bool TryGet(int index, [NotNullWhen(true)] out DisplayDevice? display)
    {
        if (s_consumer is null)
        {
            display = null;
            return false;
        }

        return s_consumer.TryGet(index, out display);
    }
}
