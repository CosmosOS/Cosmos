// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.Devices.Input;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// A mouse a registered driver published, as the mouse manager sees it: a
/// <see cref="MouseDevice"/> like the built-in ones, so the manager wires
/// its <see cref="MouseDevice.OnMouseEvent"/> the same way. The driver
/// reports through the <see cref="MouseReporter"/> in front of it; nothing
/// is polled.
/// </summary>
internal sealed class PublishedMouse : MouseDevice
{
    /// <summary>
    /// Set by the mouse manager once it has wired <see cref="MouseDevice.OnMouseEvent"/>.
    /// A mouse published by an attempt that was declined or failed never
    /// reaches the manager, so it is never enabled and its reports go nowhere.
    /// </summary>
    private volatile bool _enabled;

    /// <summary>Always false: the driver pushes each report, there is nothing to read.</summary>
    public override bool DataAvailable => false;

    /// <summary>Puts the reported state back at rest: no movement, no wheel and no button held.</summary>
    public override void Initialize()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            X = 0;
            Y = 0;
            ScrollDelta = 0;
            LeftButton = false;
            RightButton = false;
            MiddleButton = false;
        }
    }

    /// <summary>Lets reports through to <see cref="MouseDevice.OnMouseEvent"/>. The mouse manager calls it once it is listening.</summary>
    public override void Enable() => _enabled = true;

    /// <summary>Drops every report from now on, until <see cref="Enable"/>.</summary>
    public override void Disable() => _enabled = false;

    /// <summary>
    /// Records one report and hands it to the mouse manager. IRQ-safe:
    /// interrupts are masked while it runs, so a report from a thread and
    /// one from an interrupt handler, this mouse's or a built-in's, never
    /// interleave in the manager's pointer arithmetic; and it allocates
    /// nothing, and calls through a delegate, not an interface.
    /// </summary>
    internal void Report(int deltaX, int deltaY, int wheel, MouseButtons buttons)
    {
        bool left = (buttons & MouseButtons.Left) != 0;
        bool right = (buttons & MouseButtons.Right) != 0;
        bool middle = (buttons & MouseButtons.Middle) != 0;

        using (InternalCpu.DisableInterruptsScope())
        {
            if (!_enabled)
            {
                return;
            }

            X += deltaX;
            Y += deltaY;
            ScrollDelta = wheel;
            LeftButton = left;
            RightButton = right;
            MiddleButton = middle;
            OnMouseEvent?.Invoke(deltaX, deltaY, wheel, left, right, middle);
        }
    }
}
