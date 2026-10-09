using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using DevKernel.Graphics;

namespace DevKernel.Diagnostics.Charting;

public static class TextRenderer
{
    // Draws text, cutting it at the last character that still fits in maxWidth.
    public static void DrawTruncated(
        Canvas canvas,
        PCScreenFont font,
        string text,
        Color color,
        int x,
        int y,
        int maxWidth)
    {
        int maxChars = maxWidth / font.Width;
        if (maxChars <= 0)
        {
            return;
        }

        if (text.Length > maxChars)
        {
            text = text.Substring(0, maxChars);
        }

        canvas.DrawString(text, font, color, x, y);
    }

    // Returns the first variant that fits in maxWidth, or the last one as a fallback.
    // Variants are expected to go from the most verbose to the most compact.
    public static string LongestThatFits(PCScreenFont font, int maxWidth, params string[] variants)
    {
        int maxChars = maxWidth / font.Width;

        for (int i = 0; i < variants.Length - 1; i++)
        {
            if (variants[i].Length <= maxChars)
            {
                return variants[i];
            }
        }

        return variants[variants.Length - 1];
    }
}
