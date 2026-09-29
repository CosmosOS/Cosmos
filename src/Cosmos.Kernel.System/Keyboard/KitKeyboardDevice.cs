// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// The ring's view of a keyboard a driver kit driver published: an
/// <see cref="IKeyboardDevice"/> over the <see cref="IKeyboard"/>, so
/// <see cref="KeyboardManager"/> keeps its one contract while the kit's
/// keyboards and the platform's PS/2 and USB keyboards share its list.
/// Created by <see cref="KitKeyboardConsumer"/> on the kit worker when the
/// keyboard is published; keys reach <see cref="OnKeyPressed"/> through the
/// consumer, in the driver's context. The reads are any context.
/// </summary>
internal sealed class KitKeyboardDevice : IKeyboardDevice
{
    private readonly PublishedDevice _published;
    private readonly IKeyboard _keyboard;
    private readonly WorkItem _ledsWork;

    /// <summary>Wraps a published keyboard. Kit worker, thread context.</summary>
    /// <param name="published">The published device the keyboard came with.</param>
    /// <param name="keyboard">The driver's keyboard contract.</param>
    /// <param name="ledsWork">The consumer's indicator work item, which <see cref="UpdateLeds"/> schedules.</param>
    internal KitKeyboardDevice(PublishedDevice published, IKeyboard keyboard, WorkItem ledsWork)
    {
        _published = published;
        _keyboard = keyboard;
        _ledsWork = ledsWork;
    }

    /// <summary>The published device this adapter stands for, the key the consumer finds it by.</summary>
    public PublishedDevice Published => _published;

    /// <summary>The name the driver gave the keyboard.</summary>
    public string Name => _keyboard.Name;

    /// <summary>Always false: a kit keyboard pushes its keys through the consumer and buffers nothing here.</summary>
    public bool KeyAvailable => false;

    /// <inheritdoc/>
    public KeyPressedHandler? OnKeyPressed { get; set; }

    /// <summary>Nothing to do: the kit driver brought the device up in its probe, before publishing it.</summary>
    public void Initialize()
    {
    }

    /// <summary>Nothing to do: a published keyboard is always enabled, and the driver's teardown withdraws it.</summary>
    public void Enable()
    {
    }

    /// <summary>Nothing to do: see <see cref="Enable"/>.</summary>
    public void Disable()
    {
    }

    /// <summary>Nothing to do: the keys arrive through the consumer, so there is nothing to poll for.</summary>
    public void Poll()
    {
    }

    /// <summary>
    /// Schedules the consumer's indicator work item, which lights the
    /// indicators as <see cref="KeyboardManager"/> holds them on the kit
    /// worker. Any context, an interrupt included; allocation-free: the
    /// manager calls it from the key report that toggled a lock key, the
    /// same context the PS/2 path calls it from, while
    /// <see cref="IKeyboard.SetLeds"/> is thread context. A refused schedule
    /// (no kit worker in this kernel) is ignored: such a kernel publishes no
    /// kit keyboard.
    /// </summary>
    public void UpdateLeds() => _ledsWork.Schedule();

    /// <summary>
    /// Lights the given indicators through the driver's contract; nothing
    /// once the device is withdrawn, since the contract object must not be
    /// used after that. Thread context on the kit worker, from the
    /// consumer's work item.
    /// </summary>
    /// <param name="leds">The indicators to light.</param>
    internal void ApplyLeds(KeyboardLeds leds)
    {
        if (_published.IsWithdrawn)
        {
            return;
        }

        _keyboard.SetLeds(leds);
    }
}
