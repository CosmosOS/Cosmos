using System;
using System.Collections.Generic;
using System.IO;

namespace Cosmos.Kernel.System.Graphics.Fonts;

/// <summary>
/// Represents a TrueType font. Unlike the fixed-cell bitmap fonts, glyphs have
/// individual widths and pair kerning, and the font can be rasterized at any
/// pixel size. <see cref="Font.Data"/> holds the raw bytes of the .ttf file;
/// <see cref="Font.Width"/> and <see cref="Font.Height"/> are zero because a
/// TrueType font has no fixed character cell. Rasterized glyphs are cached per
/// (character, size) as grayscale coverage, so the same font instance can draw
/// in any color without re-rasterizing.
/// </summary>
public sealed class TrueTypeFont : Font
{
    private readonly LunarLabs.Fonts.Font _font;
    private readonly Dictionary<int, TrueTypeGlyph?> _glyphCache = [];

    /// <summary>
    /// The text size in pixels used when this font is drawn through the
    /// size-less <see cref="Canvas.DrawString(string, Font, global::System.Drawing.Color, int, int)"/>
    /// overload that takes a plain <see cref="Font"/>.
    /// </summary>
    public int SizePx { get; set; }

    /// <summary>
    /// Loads a TrueType font from a file.
    /// </summary>
    /// <param name="path">The path of the .ttf file.</param>
    /// <param name="sizePx">The default text size in pixels.</param>
    public TrueTypeFont(string path, int sizePx = 16) : this(ReadAllBytes(path), sizePx)
    {
    }

    /// <summary>
    /// Loads a TrueType font from the raw bytes of a .ttf file.
    /// </summary>
    /// <param name="fontData">The raw bytes of the .ttf file.</param>
    /// <param name="sizePx">The default text size in pixels.</param>
    public TrueTypeFont(byte[] fontData, int sizePx = 16) : base(0, 0, fontData)
    {
        _font = new LunarLabs.Fonts.Font(fontData);
        SizePx = sizePx;
    }

    /// <summary>
    /// Loads a TrueType font from a stream holding a .ttf file.
    /// </summary>
    /// <param name="stream">The stream to read the font from.</param>
    /// <param name="sizePx">The default text size in pixels.</param>
    public TrueTypeFont(Stream stream, int sizePx = 16) : this(ReadAllBytes(stream), sizePx)
    {
    }

    /// <summary>
    /// Returns whether the font contains a glyph for the given character.
    /// </summary>
    /// <param name="c">The character to look up.</param>
    public bool HasGlyph(char c) => _font.HasGlyph(c);

    /// <inheritdoc cref="GetLineMetrics(int, out int, out int, out int)"/>
    public void GetLineMetrics(out int ascent, out int descent, out int lineGap)
        => GetLineMetrics(SizePx, out ascent, out descent, out lineGap);

    /// <summary>
    /// Gets the distance in pixels from the top of a text line to the baseline
    /// at this font's <see cref="SizePx"/>.
    /// </summary>
    public int GetAscent() => GetAscent(SizePx);

    /// <summary>
    /// Gets the kerning adjustment in pixels for a pair of characters at this
    /// font's <see cref="SizePx"/>.
    /// </summary>
    /// <param name="left">The left character of the pair.</param>
    /// <param name="right">The right character of the pair.</param>
    public int GetKerning(char left, char right) => GetKerning(left, right, SizePx);

    /// <inheritdoc />
    public override int GetLineHeight() => GetLineHeight(SizePx);

    /// <inheritdoc />
    public override int GetAdvance(char c) => GetAdvance(c, SizePx);

    /// <inheritdoc />
    public override int MeasureString(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return MeasureString(text, SizePx);
    }

    /// <inheritdoc />
    public override int GetMaxAdvance()
    {
        int maxAdvance = 0;

        for (char c = '!'; c <= '~'; c++)
        {
            int advance = GetAdvance(c, SizePx);
            if (advance > maxAdvance)
            {
                maxAdvance = advance;
            }
        }

        return maxAdvance;
    }

    /// <summary>
    /// Gets the vertical metrics of the font at the given size. The ascent is
    /// the distance from the top of a text line to the baseline, the descent
    /// is negative and reaches from the baseline to the bottom of the line,
    /// and the line gap is the extra spacing between two lines.
    /// </summary>
    /// <param name="sizePx">The text size in pixels.</param>
    /// <param name="ascent">The scaled ascent in pixels.</param>
    /// <param name="descent">The scaled descent in pixels (negative).</param>
    /// <param name="lineGap">The scaled gap between lines in pixels.</param>
    public void GetLineMetrics(int sizePx, out int ascent, out int descent, out int lineGap)
    {
        float scale = GetScale(sizePx);
        _font.GetFontVMetrics(out int rawAscent, out int rawDescent, out int rawLineGap);
        ascent = (int)Math.Round(rawAscent * scale);
        descent = (int)Math.Round(rawDescent * scale);
        lineGap = (int)Math.Round(rawLineGap * scale);
    }

    /// <summary>
    /// Gets the distance in pixels from the top of a text line to the
    /// baseline at the given size.
    /// </summary>
    /// <param name="sizePx">The text size in pixels.</param>
    public int GetAscent(int sizePx)
    {
        GetLineMetrics(sizePx, out int ascent, out _, out _);
        return ascent;
    }

    /// <summary>
    /// Gets the distance in pixels between the tops of two consecutive text
    /// lines at the given size.
    /// </summary>
    /// <param name="sizePx">The text size in pixels.</param>
    public int GetLineHeight(int sizePx)
    {
        GetLineMetrics(sizePx, out int ascent, out int descent, out int lineGap);
        return ascent - descent + lineGap;
    }

    /// <summary>
    /// Gets the horizontal advance in pixels of a single character at the
    /// given size, without kerning. Zero when the font has no glyph for the
    /// character.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <param name="sizePx">The text size in pixels.</param>
    public int GetAdvance(char c, int sizePx)
    {
        if (!_font.HasGlyph(c))
        {
            return 0;
        }

        _font.GetCodepointHMetrics(c, out int advance, out _);
        return (int)Math.Floor(advance * GetScale(sizePx));
    }

    /// <summary>
    /// Gets the kerning adjustment in pixels for a pair of characters at the
    /// given size. Zero for fonts without a legacy kerning table.
    /// </summary>
    /// <param name="left">The left character of the pair.</param>
    /// <param name="right">The right character of the pair.</param>
    /// <param name="sizePx">The text size in pixels.</param>
    public int GetKerning(char left, char right, int sizePx) => _font.GetKerning(left, right, GetScale(sizePx));

    /// <summary>
    /// Measures the width in pixels that <see cref="Canvas.DrawString(string, TrueTypeFont, int, global::System.Drawing.Color, int, int)"/>
    /// would use to draw the given text at the given size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="sizePx">The text size in pixels.</param>
    public int MeasureString(string text, int sizePx)
    {
        int width = 0;
        char previous = '\0';

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            TrueTypeGlyph? glyph = GetGlyph(c, sizePx);
            if (glyph is null)
            {
                continue;
            }

            if (previous != '\0')
            {
                width += GetKerning(previous, c, sizePx);
            }

            width += glyph.Advance;
            previous = c;
        }

        return width;
    }

    /// <summary>
    /// Gets the rasterized glyph for a character at the given size, from the
    /// cache when possible. Returns null when the font has no glyph for the
    /// character (after the case fallback of the rasterizer).
    /// </summary>
    /// <param name="c">The character to rasterize.</param>
    /// <param name="sizePx">The text size in pixels.</param>
    internal TrueTypeGlyph? GetGlyph(char c, int sizePx)
    {
        if (sizePx is <= 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(sizePx), $"Text size ({sizePx}) must be between 1 and {ushort.MaxValue}");
        }

        int key = (sizePx << 16) | c;
        if (_glyphCache.TryGetValue(key, out TrueTypeGlyph? cached))
        {
            return cached;
        }

        TrueTypeGlyph? glyph = Rasterize(c, sizePx);
        _glyphCache[key] = glyph;
        return glyph;
    }

    private TrueTypeGlyph? Rasterize(char c, int sizePx)
    {
        if (c == ' ')
        {
            // The rasterizer substitutes '_' for spaces; take the real space
            // advance from the metrics instead and skip the bitmap.
            _font.GetCodepointHMetrics(c, out int spaceAdvance, out _);
            return new TrueTypeGlyph(null, 0, 0, 0, 0, (int)Math.Floor(spaceAdvance * GetScale(sizePx)));
        }

        LunarLabs.Fonts.FontGlyph? rendered = _font.RenderGlyph(c, GetScale(sizePx));
        if (rendered is null)
        {
            return null;
        }

        LunarLabs.Fonts.GlyphBitmap image = rendered.Image ?? throw new Exception($"{nameof(Image)} can not be null");
        return new TrueTypeGlyph(image.Pixels, image.Width, image.Height, rendered.xOfs, rendered.yOfs, rendered.xAdvance);
    }

    private float GetScale(int sizePx) => _font.ScaleInPixels(sizePx);

    private static byte[] ReadAllBytes(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        return ReadAllBytes(stream);
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}

/// <summary>
/// A rasterized TrueType glyph: grayscale coverage plus the metrics needed to
/// place it relative to the pen position on the baseline.
/// </summary>
internal sealed class TrueTypeGlyph
{
    /// <summary>
    /// One coverage byte per pixel (0 = transparent, 255 = fully opaque),
    /// row-major. Null for whitespace glyphs that only advance the pen.
    /// </summary>
    public byte[]? Coverage { get; }

    /// <summary>
    /// The width of the coverage bitmap in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the coverage bitmap in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// The horizontal offset from the pen position to the left edge of the bitmap.
    /// </summary>
    public int OffsetX { get; }

    /// <summary>
    /// The vertical offset from the baseline to the top edge of the bitmap
    /// (negative above the baseline).
    /// </summary>
    public int OffsetY { get; }

    /// <summary>
    /// How far the pen moves to the right after this glyph, in pixels.
    /// </summary>
    public int Advance { get; }

    public TrueTypeGlyph(byte[]? coverage, int width, int height, int offsetX, int offsetY, int advance)
    {
        Coverage = coverage;
        Width = width;
        Height = height;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Advance = advance;
    }
}
