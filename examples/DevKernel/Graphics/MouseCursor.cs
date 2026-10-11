using System.Drawing;
using Cosmos.Kernel.System.Graphics;

namespace DevKernel.Graphics;

/// <summary>
/// A simple arrow mouse cursor, blitted pixel by pixel onto a canvas.
/// </summary>
internal static class MouseCursor
{
    /// <summary>Width (pixels) of the cursor bitmap, and the stride of <see cref="s_pattern"/>.</summary>
    private const int CursorWidth = 10;

    /// <summary>Height (pixels) of the cursor bitmap.</summary>
    private const int CursorHeight = 16;

    /// <summary>Pattern code for a border (black) pixel.</summary>
    private const int PatternBorder = 1;

    /// <summary>Pattern code for a fill (white) pixel.</summary>
    private const int PatternFill = 2;

    /// <summary>Row-major arrow bitmap; allocated once and reused every frame.</summary>
    private static readonly int[] s_pattern = new int[]
    {
        1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        1, 1, 0, 0, 0, 0, 0, 0, 0, 0,
        1, 2, 1, 0, 0, 0, 0, 0, 0, 0,
        1, 2, 2, 1, 0, 0, 0, 0, 0, 0,
        1, 2, 2, 2, 1, 0, 0, 0, 0, 0,
        1, 2, 2, 2, 2, 1, 0, 0, 0, 0,
        1, 2, 2, 2, 2, 2, 1, 0, 0, 0,
        1, 2, 2, 2, 2, 2, 2, 1, 0, 0,
        1, 2, 2, 2, 2, 2, 2, 2, 1, 0,
        1, 2, 2, 2, 2, 2, 2, 2, 2, 1,
        1, 2, 2, 2, 2, 2, 1, 1, 1, 1,
        1, 2, 2, 1, 2, 2, 1, 0, 0, 0,
        1, 2, 1, 0, 1, 2, 2, 1, 0, 0,
        1, 1, 0, 0, 1, 2, 2, 1, 0, 0,
        1, 0, 0, 0, 0, 1, 2, 2, 1, 0,
        0, 0, 0, 0, 0, 1, 1, 1, 1, 0,
    };

    /// <summary>Draws the cursor with its hotspot at (<paramref name="x"/>, <paramref name="y"/>), clipped to the canvas.</summary>
    public static void Draw(Canvas canvas, int x, int y)
    {
        for (int cy = 0; cy < CursorHeight; cy++)
        {
            for (int cx = 0; cx < CursorWidth; cx++)
            {
                int px = x + cx;
                int py = y + cy;

                if (px < 0 || px >= canvas.Mode.Width || py < 0 || py >= canvas.Mode.Height)
                {
                    continue;
                }

                int pixel = s_pattern[cy * CursorWidth + cx];
                if (pixel == PatternBorder)
                {
                    canvas.DrawPoint(Color.Black, px, py);
                }
                else if (pixel == PatternFill)
                {
                    canvas.DrawPoint(Color.White, px, py);
                }

                // Any other code is transparent: leave the canvas as it was.
            }
        }
    }

    /// <summary>
    /// The arrow as premultiplied ARGB pixels for a hardware cursor, each
    /// pattern pixel drawn as a <paramref name="scale"/> by <paramref name="scale"/>
    /// square. The hotspot is the top left corner, as for <see cref="Draw"/>.
    /// </summary>
    /// <param name="scale">Pixels per pattern pixel, 1 or more.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <returns>The pixels, row-major, <paramref name="width"/> times <paramref name="height"/> of them.</returns>
    public static uint[] ToArgb(int scale, out int width, out int height)
    {
        width = CursorWidth * scale;
        height = CursorHeight * scale;
        uint[] pixels = new uint[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixel = s_pattern[y / scale * CursorWidth + x / scale];
                if (pixel == PatternBorder)
                {
                    pixels[y * width + x] = (uint)Color.Black.ToArgb();
                }
                else if (pixel == PatternFill)
                {
                    pixels[y * width + x] = (uint)Color.White.ToArgb();
                }
            }
        }

        return pixels;
    }
}
