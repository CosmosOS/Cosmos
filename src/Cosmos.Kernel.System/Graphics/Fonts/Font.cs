using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cosmos.Kernel.System.Graphics.Fonts;

/// <summary>
/// Represents a bitmap font.
/// </summary>
public abstract class Font
{
    /// <summary>
    /// Gets the raw pixel data of the bitmap font.
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// The height of a single character in pixels.
    /// </summary>
    public byte Height { get; }

    /// <summary>
    /// The width of a single character in pixels.
    /// </summary>
    public byte Width { get; }

    /// <summary>
    /// Converts a byte to its byte address.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConvertByteToBitAddress(byte byteToConvert, int bitToReturn)
    {
        int mask = 1 << (8 - bitToReturn);
        return (byteToConvert & mask) != 0;
    }

    /// <summary>
    /// Gets the distance in pixels between the tops of two consecutive text
    /// lines. A bitmap font's glyphs fill their cell exactly, so this is
    /// <see cref="Height"/>; a TrueType font adds its own leading.
    /// </summary>
    public virtual int GetLineHeight() => Height;

    /// <summary>
    /// Gets the horizontal distance in pixels to advance after drawing
    /// <paramref name="c"/>. A bitmap font is fixed pitch, so this is
    /// <see cref="Width"/> for every character.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    public virtual int GetAdvance(char c) => Width;

    /// <summary>
    /// Gets the widest advance any printable character needs, which is the
    /// width of a character cell in a grid laid out with this font.
    /// </summary>
    public virtual int GetMaxAdvance() => Width;

    /// <summary>
    /// Measures the width in pixels that
    /// <see cref="Canvas.DrawString(string, Font, global::System.Drawing.Color, int, int)"/>
    /// would use to draw <paramref name="text"/> with this font.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    public virtual int MeasureString(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.Length * Width;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class.
    /// </summary>
    /// <param name="width">The width of a single character in pixels</param>
    /// <param name="height">The height of a single character in pixels</param>
    /// <param name="data">The raw pixel data.</param>
    public Font(byte width, byte height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data;
    }
}
