// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Mouse;

/// <summary>
/// Manages mouse input from physical mouse devices.
/// </summary>
public static class MouseManager
{
    /// <summary>
    /// Whether mouse support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.MouseEnabled;

    private static List<IMouseDevice>? s_mice;

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
    /// platform mice are registered.
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
    }

    /// <summary>
    /// Registers a mouse device with the manager.
    /// </summary>
    internal static void RegisterMouse(IMouseDevice mouse)
    {
        if (s_mice is null || mouse is null)
        {
            return;
        }

        // Set up event handler for mouse devices that use MouseDevice base class
        if (mouse is MouseDevice mouseDevice)
        {
            mouseDevice.OnMouseEvent = HandleMouseEvent;
        }

        s_mice.Add(mouse);

        // Enable mouse after callback is set
        mouse.Enable();

        Cosmos.Kernel.Core.IO.Serial.Write("[MouseManager] Registered mouse, total: ");
        Cosmos.Kernel.Core.IO.Serial.WriteNumber((uint)s_mice.Count);
        Cosmos.Kernel.Core.IO.Serial.Write("\n");
    }

    /// <summary>
    /// Handles mouse events from devices.
    /// </summary>
    private static void HandleMouseEvent(int deltaX, int deltaY, int deltaZ, bool leftButton, bool rightButton, bool middleButton)
    {
        // Apply sensitivity
        int adjustedDeltaX = (int)(deltaX * Sensitivity);
        int adjustedDeltaY = (int)(deltaY * Sensitivity);

        // Update position with boundary checking
        X += adjustedDeltaX;
        Y += adjustedDeltaY;
        ScrollDelta += deltaZ;

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
        LeftButton = leftButton;
        RightButton = rightButton;
        MiddleButton = middleButton;
    }

    /// <summary>
    /// Polls all registered mice for events.
    /// </summary>
    internal static void Poll()
    {
        if (s_mice is null)
        {
            return;
        }

        foreach (var mouse in s_mice)
        {
            mouse.Poll();
        }
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
