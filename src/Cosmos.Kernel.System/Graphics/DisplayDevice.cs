// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// One published display as the ring sees it: the name the driver gave it,
/// which driver published it (or the firmware), where it sits in the device
/// tree, and its mode read live from the driver. Created by
/// <see cref="KitDisplayConsumer"/> when the kit publishes a display and
/// kept in <see cref="DisplayManager"/>'s list until it is withdrawn; a
/// canvas holds one to draw on. Once withdrawn the mode reads as zero and no
/// facet is found, and the contract object behind it is never read again:
/// the identity members are copied at creation, so they stay readable.
/// </summary>
public sealed class DisplayDevice
{
    /// <summary>The refresh rate reported when the driver does not know its own.</summary>
    private const int DefaultRefreshRate = 60;

    private readonly PublishedDevice _published;
    private readonly IDisplay _display;

    /// <summary>Wraps a published display. Thread context, from the consumer's <see cref="KitDisplayConsumer.OnPublished"/>.</summary>
    /// <param name="published">The published device, whose contract object is an <see cref="IDisplay"/>.</param>
    internal DisplayDevice(PublishedDevice published)
    {
        _published = published;
        _display = (IDisplay)published.Device;
        Name = published.Name;
        DeviceBinding? binding = published.Binding;
        DriverName = binding is null ? "firmware" : binding.Driver.Name;
        NodePath = binding?.Node.Path;
        IsFirmware = published.Provenance == DeviceProvenance.Firmware;
    }

    /// <summary>The driver's name for the display: <c>framebuffer</c>, <c>virtio-gpu</c>, <c>vmware-svga</c>.</summary>
    public string Name { get; }

    /// <summary>The class name of the driver that published the display, or <c>firmware</c> for the boot framebuffer.</summary>
    public string DriverName { get; }

    /// <summary>True for the framebuffer the bootloader handed over, which no driver stands behind.</summary>
    public bool IsFirmware { get; }

    /// <summary>The path of the device tree node the publishing driver bound; null for the firmware display.</summary>
    public string? NodePath { get; }

    /// <summary>Width in pixels of the current mode; 0 once withdrawn.</summary>
    public int Width => _published.IsWithdrawn ? 0 : _display.Mode.Width;

    /// <summary>Height in pixels of the current mode; 0 once withdrawn.</summary>
    public int Height => _published.IsWithdrawn ? 0 : _display.Mode.Height;

    /// <summary>Bits per pixel of the current mode; 0 once withdrawn.</summary>
    public int BitsPerPixel => _published.IsWithdrawn ? 0 : _display.Mode.BitsPerPixel;

    /// <summary>Bytes per row of the framebuffer in the current mode; 0 once withdrawn.</summary>
    public int Pitch => _published.IsWithdrawn ? 0 : _display.Mode.Pitch;

    /// <summary>Refresh rate in Hz of the current mode; 60 when the driver reports none, 0 once withdrawn.</summary>
    public int RefreshRate
    {
        get
        {
            if (_published.IsWithdrawn)
            {
                return 0;
            }

            int refreshRate = _display.Mode.RefreshRate;
            return refreshRate == 0 ? DefaultRefreshRate : refreshRate;
        }
    }

    /// <summary>True once the kit withdrew the display: the driver unbound, or a driver retired the firmware framebuffer.</summary>
    public bool IsWithdrawn => _published.IsWithdrawn;

    /// <summary>
    /// The contract object the driver published, for the canvas that draws
    /// on it. Read only while <see cref="IsWithdrawn"/> is false, the kit's
    /// rule on contract objects. Any context.
    /// </summary>
    internal IDisplay Display => _display;

    /// <summary>The kit's record of the publication: the withdrawal flag and the reference the consumer matches reports by. Any context.</summary>
    internal PublishedDevice Published => _published;

    /// <summary>
    /// Finds a facet of the display: an extra interface the published
    /// object implements, such as <see cref="IDisplayModes"/>,
    /// <see cref="IHardwareCursor"/> or <see cref="ICanvas3DFactory"/>, or
    /// a driver's own state type. Thread context.
    /// </summary>
    /// <typeparam name="T">The facet's type.</typeparam>
    /// <param name="facet">The facet, when the display implements it.</param>
    /// <returns>False when the display does not implement <typeparamref name="T"/>, and always once the display is withdrawn.</returns>
    public bool TryGetFacet<T>([NotNullWhen(true)] out T? facet) where T : class
    {
        if (_published.IsWithdrawn)
        {
            facet = null;
            return false;
        }

        if (_display is T found)
        {
            facet = found;
            return true;
        }

        facet = null;
        return false;
    }
}
