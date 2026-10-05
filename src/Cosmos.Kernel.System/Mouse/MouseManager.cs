// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Mouse;

/// <summary>
/// Manages mouse input from every pointer a driver kit driver publishes (the
/// PS/2 mouse on x64, virtio mice), which the manager's
/// <see cref="KitPointerConsumer"/> registers from the kit worker when it is
/// published and unregisters when it is withdrawn.
/// </summary>
public static class MouseManager
{
    /// <summary>
    /// Whether mouse support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.MouseEnabled;

    /// <summary>
    /// The published pointers, kept for the registration log and the count.
    /// Replaced on every change, never changed in place; changed by the kit
    /// worker only.
    /// </summary>
    private static PublishedDevice[]? s_mice;

    /// <summary>
    /// Current X position (screen coordinates).
    /// </summary>
    public static int X { get; private set; }

    /// <summary>
    /// Current Y position (screen coordinates).
    /// </summary>
    public static int Y { get; private set; }

    /// <summary>
    /// Accumulated scroll wheel delta. Wheel events add to it and it is never
    /// cleared by the driver; pollers consume it with <see cref="ResetScrollDelta"/>
    /// (gen2 parity). Accumulating instead of overwriting matters: a PS/2 wheel
    /// click arrives as a z!=0 packet immediately followed by a z=0 packet, so
    /// assignment would zero the delta before any poller can observe it.
    /// </summary>
    public static int ScrollDelta { get; private set; }

    /// <summary>
    /// Clears <see cref="ScrollDelta"/> after a poller has consumed it, so a
    /// stale delta is not re-processed every frame until the next wheel event.
    /// </summary>
    /// <exception cref="InvalidOperationException">Mouse support is disabled.</exception>
    public static void ResetScrollDelta()
    {
        ThrowIfDisabled();

        ScrollDelta = 0;
    }

    /// <summary>
    /// Left button state.
    /// </summary>
    public static bool LeftButton { get; private set; }

    /// <summary>
    /// Right button state.
    /// </summary>
    public static bool RightButton { get; private set; }

    /// <summary>
    /// Middle button state.
    /// </summary>
    public static bool MiddleButton { get; private set; }

    /// <summary>
    /// Screen width for boundary checking.
    /// </summary>
    public static int ScreenWidth { get; private set; } = 1024;

    /// <summary>
    /// Screen height for boundary checking.
    /// </summary>
    public static int ScreenHeight { get; private set; } = 768;

    private static float s_sensitivity = 1.0f;

    /// <summary>
    /// Mouse sensitivity multiplier (default 1.0).
    /// </summary>
    /// <exception cref="InvalidOperationException">Mouse support is disabled.</exception>
    public static float Sensitivity
    {
        get => s_sensitivity;
        set
        {
            ThrowIfDisabled();

            s_sensitivity = value;
        }
    }

    /// <summary>
    /// Throws when mouse support is compiled out. Guards actions, not reads:
    /// a read answers honestly (0, null, false, empty) so a kernel can branch
    /// on it, and an action names the switch to set instead of failing
    /// silently.
    /// </summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Mouse support is disabled. Set CosmosEnableMouse=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Initializes the mouse manager. Called once during boot, before the
    /// driver stage runs, so the pointer consumer it installs sees every
    /// pointer a kit driver publishes.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_mice is not null)
        {
            return;
        }

        X = ScreenWidth / 2;
        Y = ScreenHeight / 2;
        s_mice = [];
        DeviceRegistry.SetConsumer(DeviceKind.Pointer, new KitPointerConsumer());
    }

    /// <summary>
    /// Registers a published pointer with the manager. Thread context, the
    /// kit worker when a published pointer is consumed; the list is replaced,
    /// never changed in place.
    /// </summary>
    /// <param name="mouse">The pointer to register; nothing when the manager is not initialized.</param>
    internal static void RegisterMouse(PublishedDevice mouse)
    {
        if (s_mice is null)
        {
            return;
        }

        s_mice = [.. s_mice, mouse];

        Core.IO.Serial.Write("[MouseManager] Registered mouse, total: ");
        Core.IO.Serial.WriteNumber((uint)s_mice.Length);
        Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Forgets a pointer that is gone (withdrawn by its driver's teardown).
    /// Its reports stop arriving; the cursor and the button flags keep their
    /// last values. Thread context, the kit worker in a teardown; the list is
    /// replaced, never changed in place.
    /// </summary>
    /// <param name="mouse">The pointer to remove; nothing when it is not registered.</param>
    internal static void UnregisterMouse(PublishedDevice mouse)
    {
        if (s_mice is null)
        {
            return;
        }

        List<PublishedDevice> kept = new(s_mice.Length);
        foreach (PublishedDevice other in s_mice)
        {
            if (!ReferenceEquals(other, mouse))
            {
                kept.Add(other);
            }
        }

        s_mice = kept.ToArray();

        Core.IO.Serial.Write("[MouseManager] Unregistered mouse, total: ");
        Core.IO.Serial.WriteNumber((uint)s_mice.Length);
        Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// A relative movement report: applies the sensitivity, moves the cursor,
    /// clamps it to the screen, accumulates the wheel and sets the buttons.
    /// Sink caller's context, an interrupt included; allocation-free.
    /// </summary>
    /// <param name="deltaX">Horizontal movement since the last report.</param>
    /// <param name="deltaY">Vertical movement since the last report.</param>
    /// <param name="buttons">Buttons held down.</param>
    /// <param name="wheel">Wheel movement since the last report: negative scrolls up, positive scrolls down.</param>
    internal static void HandleRelative(int deltaX, int deltaY, PointerButtons buttons, int wheel)
    {
        // Apply sensitivity
        int adjustedDeltaX = (int)(deltaX * Sensitivity);
        int adjustedDeltaY = (int)(deltaY * Sensitivity);

        // Update position with boundary checking
        X += adjustedDeltaX;
        Y += adjustedDeltaY;
        ScrollDelta += wheel;

        // Clamp to screen bounds
        if (X < 0)
        {
            X = 0;
        }

        if (X >= ScreenWidth)
        {
            X = ScreenWidth - 1;
        }

        if (Y < 0)
        {
            Y = 0;
        }

        if (Y >= ScreenHeight)
        {
            Y = ScreenHeight - 1;
        }

        // Update button states
        LeftButton = (buttons & PointerButtons.Left) != 0;
        RightButton = (buttons & PointerButtons.Right) != 0;
        MiddleButton = (buttons & PointerButtons.Middle) != 0;
    }

    /// <summary>
    /// An absolute position report: sets the three buttons and nothing else,
    /// the migration-period mapping of an absolute device onto a manager that
    /// only knows movement: the buttons are learned and the cursor stays
    /// where it was (the scaling of an absolute device to the screen is an
    /// open item). Sink caller's context, an interrupt included;
    /// allocation-free.
    /// </summary>
    /// <param name="x">Horizontal position in the device's own range; not applied.</param>
    /// <param name="y">Vertical position in the device's own range; not applied.</param>
    /// <param name="buttons">Buttons held down.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The method has the absolute report's shape; the position is not applied until the manager scales an absolute device to the screen.")]
    internal static void HandleAbsolute(int x, int y, PointerButtons buttons)
    {
        LeftButton = (buttons & PointerButtons.Left) != 0;
        RightButton = (buttons & PointerButtons.Right) != 0;
        MiddleButton = (buttons & PointerButtons.Middle) != 0;
    }

    /// <summary>
    /// Sets the mouse position directly (useful for initialization or reset).
    /// </summary>
    /// <param name="x">New horizontal position, clamped to the screen width.</param>
    /// <param name="y">New vertical position, clamped to the screen height.</param>
    /// <exception cref="InvalidOperationException">Mouse support is disabled.</exception>
    public static void SetPosition(int x, int y)
    {
        ThrowIfDisabled();

        X = x;
        Y = y;

        // Clamp to screen bounds
        if (X < 0)
        {
            X = 0;
        }

        if (X >= ScreenWidth)
        {
            X = ScreenWidth - 1;
        }

        if (Y < 0)
        {
            Y = 0;
        }

        if (Y >= ScreenHeight)
        {
            Y = ScreenHeight - 1;
        }
    }

    /// <summary>
    /// Updates screen dimensions (call when resolution changes).
    /// </summary>
    /// <param name="width">New screen width in pixels.</param>
    /// <param name="height">New screen height in pixels.</param>
    /// <exception cref="InvalidOperationException">Mouse support is disabled.</exception>
    public static void SetScreenSize(int width, int height)
    {
        ThrowIfDisabled();

        ScreenWidth = width;
        ScreenHeight = height;

        // Ensure cursor is still within bounds
        if (X >= ScreenWidth)
        {
            X = ScreenWidth - 1;
        }

        if (Y >= ScreenHeight)
        {
            Y = ScreenHeight - 1;
        }
    }
}
