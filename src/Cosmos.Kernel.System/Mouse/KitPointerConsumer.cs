// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Mouse;

/// <summary>
/// The mouse manager's consumer of the driver kit: every pointer a kit
/// driver publishes becomes a <see cref="KitPointerDevice"/> in the
/// manager's list, and leaves it when withdrawn. A report is handed to the
/// adapter, which raises the manager's mouse event handler the way a
/// platform mouse does from its interrupt. Installed by
/// <see cref="MouseManager.Initialize"/>, before the driver stage runs.
/// <para>
/// Contexts: <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run on
/// the kit worker in thread context; <see cref="OnRelative"/> and
/// <see cref="OnAbsolute"/> run in the sink caller's context, an interrupt
/// included, and allocate nothing: the adapter is found by a scan of a
/// copy-on-write array.
/// </para>
/// </summary>
internal sealed class KitPointerConsumer : PointerConsumer
{
    private KitPointerDevice[] _adapters = [];

    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device)
    {
        IPointer pointer = (IPointer)device.Device;
        KitPointerDevice adapter = new(device, pointer);
        MouseManager.RegisterMouse(adapter);

        KitPointerDevice[] current = _adapters;
        KitPointerDevice[] adapters = new KitPointerDevice[current.Length + 1];
        Array.Copy(current, adapters, current.Length);
        adapters[current.Length] = adapter;
        _adapters = adapters;
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        KitPointerDevice? adapter = Find(device);
        if (adapter is null)
        {
            return;
        }

        KitPointerDevice[] current = _adapters;
        KitPointerDevice[] adapters = new KitPointerDevice[current.Length - 1];
        int kept = 0;
        for (int i = 0; i < current.Length; i++)
        {
            if (!ReferenceEquals(current[i], adapter))
            {
                adapters[kept++] = current[i];
            }
        }

        _adapters = adapters;
        MouseManager.UnregisterMouse(adapter);
    }

    /// <inheritdoc/>
    public override void OnRelative(PublishedDevice device, int deltaX, int deltaY, PointerButtons buttons, int wheel) =>
        Find(device)?.OnRelative(deltaX, deltaY, buttons, wheel);

    /// <inheritdoc/>
    public override void OnAbsolute(PublishedDevice device, int x, int y, PointerButtons buttons) =>
        Find(device)?.OnAbsolute(x, y, buttons);

    /// <summary>The adapter of a published device, by reference: a scan of a copy-on-write array, allocation-free. Any context.</summary>
    private KitPointerDevice? Find(PublishedDevice device)
    {
        KitPointerDevice[] adapters = _adapters;
        for (int i = 0; i < adapters.Length; i++)
        {
            if (ReferenceEquals(adapters[i].Published, device))
            {
                return adapters[i];
            }
        }

        return null;
    }
}
