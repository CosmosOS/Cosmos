// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.Engine;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A mouse a driver published through <see cref="DeviceContext.PublishMouse"/>:
/// what the driver reports through it moves the kernel's pointer, exactly as
/// a built-in mouse does. Reports reach the mouse manager once the driver's
/// Probe returned Bound; a report made before that, or through the reporter
/// of an attempt that was declined or failed, is dropped.
/// </summary>
internal sealed class MouseReporter
{
    private readonly PublishedMouse _mouse;

    /// <summary>Puts a reporter in front of <paramref name="mouse"/>, the adapter the kit delivers to the mouse manager.</summary>
    internal MouseReporter(PublishedMouse mouse)
    {
        _mouse = mouse;
    }

    /// <summary>
    /// Reports one movement of the mouse and the buttons held down after it.
    /// IRQ-safe: it allocates nothing and masks interrupts only while it
    /// hands the report on, so the driver's interrupt handler may call it,
    /// and so may a thread.
    /// </summary>
    /// <param name="deltaX">Horizontal movement since the last report; positive moves right.</param>
    /// <param name="deltaY">Vertical movement since the last report; positive moves down, as on screen.</param>
    /// <param name="wheel">Wheel movement since the last report: negative scrolls up, positive scrolls down, zero for none.</param>
    /// <param name="buttons">The buttons held down now.</param>
    public void Report(int deltaX, int deltaY, int wheel, MouseButtons buttons) =>
        _mouse.Report(deltaX, deltaY, wheel, buttons);
}
