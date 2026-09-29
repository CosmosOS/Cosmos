// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// The keyboard manager's consumer of the driver kit: every keyboard a kit
/// driver publishes becomes a <see cref="KitKeyboardDevice"/> in the
/// manager's list, and leaves it when withdrawn. A key report is handed to
/// the adapter's <see cref="IKeyboardDevice.OnKeyPressed"/>, which is the
/// manager's scan code handler, the same call the PS/2 keyboard makes from
/// its interrupt. Installed by <see cref="KeyboardManager.Initialize"/>,
/// before the driver stage runs.
/// <para>
/// Contexts: <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run on
/// the kit worker in thread context; <see cref="OnKey"/> runs in the sink
/// caller's context, an interrupt included, and allocates nothing: the
/// adapter is found by a scan of a copy-on-write array. The indicators are
/// lit by one kit-owned work item, <see cref="ApplyLeds"/>, on the kit
/// worker in thread context: <see cref="KeyboardManager"/> asks for them
/// from the key report that toggled a lock key, a PS/2 or USB interrupt
/// included, while <see cref="IKeyboard.SetLeds"/> is thread context, so
/// the adapters only schedule the item. The item and
/// <see cref="OnWithdrawn"/> share the worker, so a withdrawn keyboard's
/// contract object is never called after its withdrawal.
/// </para>
/// </summary>
internal sealed class KitKeyboardConsumer : KeyboardConsumer
{
    private readonly WorkItem _ledsWork;
    private KitKeyboardDevice[] _adapters = [];

    /// <summary>Creates the consumer and its indicator work item. Thread context, from <see cref="KeyboardManager.Initialize"/>.</summary>
    internal KitKeyboardConsumer()
    {
        _ledsWork = new WorkItem(ApplyLeds, binding: null);
    }

    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device)
    {
        IKeyboard keyboard = (IKeyboard)device.Device;
        KitKeyboardDevice adapter = new(device, keyboard, _ledsWork);
        KeyboardManager.RegisterKeyboard(adapter);

        KitKeyboardDevice[] current = _adapters;
        KitKeyboardDevice[] adapters = new KitKeyboardDevice[current.Length + 1];
        Array.Copy(current, adapters, current.Length);
        adapters[current.Length] = adapter;
        _adapters = adapters;
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        KitKeyboardDevice? adapter = Find(device);
        if (adapter is null)
        {
            return;
        }

        KitKeyboardDevice[] current = _adapters;
        KitKeyboardDevice[] adapters = new KitKeyboardDevice[current.Length - 1];
        int kept = 0;
        for (int i = 0; i < current.Length; i++)
        {
            if (!ReferenceEquals(current[i], adapter))
            {
                adapters[kept++] = current[i];
            }
        }

        _adapters = adapters;
        KeyboardManager.UnregisterKeyboard(adapter);
    }

    /// <inheritdoc/>
    public override void OnKey(PublishedDevice device, byte scanCode, bool released)
    {
        KitKeyboardDevice? adapter = Find(device);
        if (adapter is null)
        {
            return;
        }

        KeyPressedHandler? handler = adapter.OnKeyPressed;
        if (handler is null)
        {
            return;
        }

        handler(scanCode, released);
    }

    /// <summary>
    /// The indicator work item: reads the lock states
    /// <see cref="KeyboardManager"/> holds now, so coalesced schedules apply
    /// the latest, and lights them on every adapter that is not withdrawn.
    /// Thread context on the kit worker.
    /// </summary>
    private void ApplyLeds()
    {
        KeyboardLeds leds = KeyboardLeds.None;
        if (KeyboardManager.NumLock)
        {
            leds |= KeyboardLeds.NumLock;
        }

        if (KeyboardManager.CapsLock)
        {
            leds |= KeyboardLeds.CapsLock;
        }

        if (KeyboardManager.ScrollLock)
        {
            leds |= KeyboardLeds.ScrollLock;
        }

        KitKeyboardDevice[] adapters = _adapters;
        for (int i = 0; i < adapters.Length; i++)
        {
            adapters[i].ApplyLeds(leds);
        }
    }

    /// <summary>The adapter of a published device, by reference: a scan of a copy-on-write array, allocation-free. Any context.</summary>
    private KitKeyboardDevice? Find(PublishedDevice device)
    {
        KitKeyboardDevice[] adapters = _adapters;
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
