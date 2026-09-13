using Cosmos.Kernel.System.Graphics;
using SysThread = System.Threading.Thread;

namespace DevKernel.Graphics;

/// <summary>
/// Background thread that draws a color-cycling, radially shaded square in a
/// corner of the framebuffer. Exists to prove the scheduler keeps a managed
/// thread running while the shell stays interactive.
/// </summary>
internal static class ColorSquareWorker
{
    /// <summary>Side (pixels) of the square.</summary>
    private const int SquareSize = 80;

    /// <summary>Gap (pixels) between the square and the screen edges.</summary>
    private const int Margin = 20;

    /// <summary>Number of phases in the color cycle (six segments, one per hue transition).</summary>
    private const int ColorCyclePhaseCount = 60;

    /// <summary>Number of phases per color-cycle segment (one channel ramp).</summary>
    private const int PhaseSegmentLength = 10;

    /// <summary>Color channel increment per phase (~255 / 10) within a segment.</summary>
    private const int PhaseColorStep = 25;

    /// <summary>Maximum value of an 8-bit color channel.</summary>
    private const int ColorChannelMax = 255;

    /// <summary>Bit position of the red channel in a 0x00RRGGBB pixel value.</summary>
    private const int RedShiftBits = 16;

    /// <summary>Bit position of the green channel in a 0x00RRGGBB pixel value.</summary>
    private const int GreenShiftBits = 8;

    /// <summary>Divisor turning a full extent into its half, i.e. the offset of the square's center.</summary>
    private const int HalfDivisor = 2;

    /// <summary>Delay (ms) between frames drawn by the worker.</summary>
    private const int FrameDelayMs = 100;

    /// <summary>Spawns the worker on its own managed thread.</summary>
    public static void Start()
    {
        SysThread thread = new(Run);
        thread.Start();
    }

    private static void Run()
    {
        if (KernelConsole.Default.Canvas.Mode.Width == 0 || KernelConsole.Default.Canvas.Mode.Height == 0)
        {
            return;
        }

        int x = KernelConsole.Default.Canvas.Mode.Width >= SquareSize + Margin * HalfDivisor
            ? KernelConsole.Default.Canvas.Mode.Width - SquareSize - Margin
            : Margin;
        int y = KernelConsole.Default.Canvas.Mode.Height >= SquareSize + Margin * HalfDivisor
            ? KernelConsole.Default.Canvas.Mode.Height - SquareSize - Margin
            : Margin;

        int frame = 0;

        while (true)
        {
            GetPhaseColor(frame % ColorCyclePhaseCount, out byte r, out byte g, out byte b);
            DrawShadedSquare(x, y, r, g, b);

            frame++;
            KernelConsole.Default.Canvas.Display();
            SysThread.Sleep(FrameDelayMs);
        }
    }

    /// <summary>Walks the RGB cube's six edges, one channel ramp per <see cref="PhaseSegmentLength"/> phases.</summary>
    private static void GetPhaseColor(int phase, out byte r, out byte g, out byte b)
    {
        if (phase < PhaseSegmentLength)
        {
            r = ColorChannelMax;
            g = (byte)(phase * PhaseColorStep);
            b = 0;
        }
        else if (phase < PhaseSegmentLength * 2)
        {
            r = (byte)(ColorChannelMax - (phase - PhaseSegmentLength) * PhaseColorStep);
            g = ColorChannelMax;
            b = 0;
        }
        else if (phase < PhaseSegmentLength * 3)
        {
            r = 0;
            g = ColorChannelMax;
            b = (byte)((phase - PhaseSegmentLength * 2) * PhaseColorStep);
        }
        else if (phase < PhaseSegmentLength * 4)
        {
            r = 0;
            g = (byte)(ColorChannelMax - (phase - PhaseSegmentLength * 3) * PhaseColorStep);
            b = ColorChannelMax;
        }
        else if (phase < PhaseSegmentLength * 5)
        {
            r = (byte)((phase - PhaseSegmentLength * 4) * PhaseColorStep);
            g = 0;
            b = ColorChannelMax;
        }
        else
        {
            r = ColorChannelMax;
            g = 0;
            b = (byte)(ColorChannelMax - (phase - PhaseSegmentLength * 5) * PhaseColorStep);
        }
    }

    /// <summary>Fills the square, dimming each pixel with its squared distance from the center.</summary>
    private static void DrawShadedSquare(int x, int y, byte r, byte g, byte b)
    {
        for (int dy = 0; dy < SquareSize; dy++)
        {
            for (int dx = 0; dx < SquareSize; dx++)
            {
                int cx = dx - SquareSize / HalfDivisor;
                int cy = dy - SquareSize / HalfDivisor;
                int dist = (cx * cx + cy * cy) * ColorChannelMax / (SquareSize * SquareSize / HalfDivisor);
                if (dist > ColorChannelMax)
                {
                    dist = ColorChannelMax;
                }

                int factor = ColorChannelMax - dist / HalfDivisor;
                byte pr = (byte)((r * factor) / ColorChannelMax);
                byte pg = (byte)((g * factor) / ColorChannelMax);
                byte pb = (byte)((b * factor) / ColorChannelMax);
                uint pixelColor = (uint)((pr << RedShiftBits) | (pg << GreenShiftBits) | pb);

                KernelConsole.Default.Canvas.DrawPoint(pixelColor, x + dx, y + dy);
            }
        }
    }
}
