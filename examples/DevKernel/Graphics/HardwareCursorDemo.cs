// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.System.Graphics;
using DevKernel.Shell;
using SysThread = System.Threading.Thread;

namespace DevKernel.Graphics;

/// <summary>
/// The hardware cursor test behind <c>display cursor</c>: defines the arrow
/// of <see cref="MouseCursor"/> on the primary display's
/// <see cref="IHardwareCursor"/> facet and moves it on its own, so it runs
/// on a machine without a pointer. It circles the middle of the screen, then
/// follows an ellipse wider than the screen, so the cursor crosses every
/// edge and the display has to clip it, then hides. Escape stops it early.
/// </summary>
internal static class HardwareCursorDemo
{
    /// <summary>Pixels per pattern pixel of the arrow.</summary>
    private const int Scale = 2;

    /// <summary>Seconds spent circling the middle of the screen.</summary>
    private const double CircleSeconds = 4;

    /// <summary>Seconds spent on the ellipse that crosses the edges.</summary>
    private const double EdgeSeconds = 6;

    /// <summary>Seconds per turn, on both paths.</summary>
    private const double TurnSeconds = 2;

    /// <summary>Radius of the circle, as a fraction of the shorter screen side.</summary>
    private const double CircleRadius = 0.3;

    /// <summary>Radii of the ellipse, as a fraction of each screen side: past the half, so it leaves the screen.</summary>
    private const double EdgeRadius = 0.6;

    /// <summary>Milliseconds between two moves.</summary>
    private const int FrameDelayMs = 8;

    /// <summary>Runs the test on the primary display; returns when it ends or Escape is pressed, at once when no display can show the cursor.</summary>
    public static void Run()
    {
        DisplayDevice? display = DisplayManager.Primary;
        if (display is null)
        {
            Terminal.Warning("No display published.");
            return;
        }

        if (!display.TryGetFacet(out IHardwareCursor? cursor))
        {
            Terminal.Warning($"{display.Name} ({display.DriverName}) has no hardware cursor.");
            return;
        }

        uint[] arrow = MouseCursor.ToArgb(Scale, out int width, out int height);
        if (!cursor.TryDefine(0, 0, width, height, arrow))
        {
            Terminal.Error($"{display.Name} refused a {width}x{height} cursor image.");
            return;
        }

        Terminal.Info("The cursor circles the middle, then crosses every edge; Esc stops it.");

        double centerX = display.Width / 2.0;
        double centerY = display.Height / 2.0;
        double circle = Math.Min(display.Width, display.Height) * CircleRadius;
        long start = Stopwatch.GetTimestamp();
        while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
        {
            double seconds = (Stopwatch.GetTimestamp() - start) / (double)Stopwatch.Frequency;
            if (seconds > CircleSeconds + EdgeSeconds)
            {
                break;
            }

            double angle = seconds / TurnSeconds * 2 * Math.PI;
            double radiusX = seconds < CircleSeconds ? circle : display.Width * EdgeRadius;
            double radiusY = seconds < CircleSeconds ? circle : display.Height * EdgeRadius;
            cursor.Set((int)(centerX + radiusX * Math.Cos(angle)), (int)(centerY + radiusY * Math.Sin(angle)), true);
            SysThread.Sleep(FrameDelayMs);
        }

        cursor.Set(0, 0, false);
        Terminal.Success("Cursor hidden.");
    }
}
