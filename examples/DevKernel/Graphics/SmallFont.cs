// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace DevKernel.Graphics;

// A bitmap font at half its size, for the small print of a screen whose headings use the
// font as it is. Each pixel of a small glyph stands for a block of two by two pixels of
// the glyph, and is drawn at an opacity that follows how many of the four are set, so a
// stroke that falls between two pixels comes out softer and does not vanish.
internal sealed class SmallFont
{
    private const int GlyphCount = 256;
    private const int Shrink = 2;

    // Opacity for one, two and three pixels set out of the four of a block, more than
    // their share: a thin stroke covers half a block at best. With all four set the color
    // is drawn as it comes.
    private const int FaintOpacity = 112;
    private const int HalfOpacity = 184;
    private const int StrongOpacity = 228;

    // For each glyph, row after row, the number of pixels set in each block.
    private readonly byte[] _coverage;

    public int Width { get; }

    public int Height { get; }

    public SmallFont(PCScreenFont font)
    {
        Width = font.Width / Shrink;
        Height = font.Height / Shrink;
        _coverage = new byte[GlyphCount * Width * Height];

        Canvas glyph = new(font.Width, font.Height);
        int blank = Color.Black.ToArgb();

        for (int code = 0; code < GlyphCount; code++)
        {
            glyph.Clear(Color.Black);
            glyph.DrawChar((char)code, font, Color.White, 0, 0);

            int first = code * Width * Height;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int set = 0;
                    for (int block = 0; block < Shrink * Shrink; block++)
                    {
                        if (glyph.GetRawPointColor(x * Shrink + block % Shrink, y * Shrink + block / Shrink) != blank)
                        {
                            set++;
                        }
                    }

                    _coverage[first + y * Width + x] = (byte)set;
                }
            }
        }
    }

    public void DrawString(Canvas canvas, string text, Color color, int x, int y)
    {
        Color faint = Color.FromArgb(FaintOpacity, color);
        Color half = Color.FromArgb(HalfOpacity, color);
        Color strong = Color.FromArgb(StrongOpacity, color);

        for (int i = 0; i < text.Length; i++)
        {
            int first = (byte)text[i] * Width * Height;
            int left = x + i * Width;

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    switch (_coverage[first + row * Width + column])
                    {
                        case 0:
                            break;
                        case 1:
                            canvas.DrawPoint(faint, left + column, y + row);
                            break;
                        case 2:
                            canvas.DrawPoint(half, left + column, y + row);
                            break;
                        case 3:
                            canvas.DrawPoint(strong, left + column, y + row);
                            break;
                        default:
                            canvas.DrawPoint(color, left + column, y + row);
                            break;
                    }
                }
            }
        }
    }
}
