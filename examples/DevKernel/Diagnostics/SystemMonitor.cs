using System.Diagnostics;
using System.Drawing;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using DevKernel.Graphics;
using MouseManager = Cosmos.Kernel.System.Mouse.MouseManager;

namespace DevKernel.Diagnostics;

/// <summary>
/// Full-screen overlay behind <c>startx</c>: live memory and GC counters, a
/// frame-rate readout paced to the canvas refresh rate, and a mouse cursor.
/// Runs until the machine is reset — there is no exit key.
/// </summary>
internal static class SystemMonitor
{
    /// <summary>Frame interval at which the loop triggers a heap collection.</summary>
    private const int GcCollectFrameInterval = 100;

    /// <summary>Runs the overlay loop; never returns.</summary>
    public static void Run()
    {
        Canvas canvas = Canvas.GetFullScreen();
        PCScreenFont font = PCScreenFont.DefaultFont;

        int fps = 0;
        int frames = 0;
        int framesSinceFps = 0;
        long lastFpsTicks = 0;
        long swFrequency = Stopwatch.Frequency;
        int refreshRate = canvas.RefreshRate;
        long frameInterval = swFrequency / refreshRate;
        long lastFrameStart = Stopwatch.GetTimestamp();

        Log.Write("Testing Canvas with mode " + canvas.Mode + " @ " + refreshRate + " Hz\n");

        if (KernelFeatures.Mouse)
        {
            MouseManager.SetScreenSize(canvas.Mode.Width, canvas.Mode.Height);
        }

        int x = OverlayLayout.TextMarginPx;
        int y = OverlayLayout.TextMarginPx;
        int lineHeight = OverlayLayout.LineHeight(font);

        while (true)
        {
            canvas.Clear(Color.Black);

            frames++;
            framesSinceFps++;

            long nowTicks = Stopwatch.GetTimestamp();
            if (lastFpsTicks == 0)
            {
                lastFpsTicks = nowTicks;
            }
            else if (nowTicks - lastFpsTicks >= swFrequency)
            {
                fps = (int)(framesSinceFps * swFrequency / (nowTicks - lastFpsTicks));
                framesSinceFps = 0;
                lastFpsTicks = nowTicks;
            }

            int rowY = DrawMemorySection(canvas, font, x, y, lineHeight);
            rowY = DrawGcSection(canvas, font, x, rowY, lineHeight);

            canvas.DrawString("FPS: " + fps + " / " + refreshRate + " Hz", font, Color.Yellow, x, rowY);

            MouseCursor.Draw(canvas, MouseManager.X, MouseManager.Y);

            if (frames % GcCollectFrameInterval == 0)
            {
                MemoryInfo.Collect();
            }

            canvas.Display();

            WaitForNextFrame(ref lastFrameStart, frameInterval);
        }
    }

    /// <summary>Draws the memory counters; returns the next free row.</summary>
    private static int DrawMemorySection(Canvas canvas, PCScreenFont font, int x, int rowY, int lineHeight)
    {
        ulong totalPages = MemoryInfo.TotalPages;
        ulong freePages = MemoryInfo.FreePages;
        ulong usedPages = totalPages - freePages;
        ulong pageSize = MemoryInfo.PageSizeBytes;

        canvas.DrawString("Meminfo", font, Color.Cyan, x, rowY);
        rowY += lineHeight;
        canvas.DrawString("Total: " + Units.ToMiB(totalPages * pageSize) + " MB", font, Color.White, x, rowY);
        rowY += lineHeight;
        canvas.DrawString("Used : " + Units.ToMiB(usedPages * pageSize) + " MB", font, Color.White, x, rowY);
        rowY += lineHeight;
        canvas.DrawString("Free : " + Units.ToMiB(freePages * pageSize) + " MB", font, Color.White, x, rowY);
        rowY += lineHeight;
        canvas.DrawString("Pages: " + usedPages + "/" + totalPages, font, Color.White, x, rowY);
        return rowY + lineHeight * OverlayLayout.SectionBreakRowCount;
    }

    /// <summary>Draws the collector counters; returns the next free row.</summary>
    private static int DrawGcSection(Canvas canvas, PCScreenFont font, int x, int rowY, int lineHeight)
    {
        canvas.DrawString("GCinfo", font, Color.Cyan, x, rowY);
        rowY += lineHeight;
        canvas.DrawString("Collections: " + MemoryInfo.TotalCollections, font, Color.White, x, rowY);
        rowY += lineHeight;
        canvas.DrawString("Objects Freed: " + MemoryInfo.TotalObjectsFreed, font, Color.White, x, rowY);
        return rowY + lineHeight * OverlayLayout.SectionBreakRowCount;
    }

    /// <summary>Spins until the next frame deadline, resetting it when a frame overran.</summary>
    private static void WaitForNextFrame(ref long lastFrameStart, long frameInterval)
    {
        lastFrameStart += frameInterval;
        long now = Stopwatch.GetTimestamp();
        if (now > lastFrameStart)
        {
            // Fell behind; reset to avoid a burst of catch-up frames.
            lastFrameStart = now;
            return;
        }

        while (Stopwatch.GetTimestamp() < lastFrameStart)
        {
        }
    }
}
