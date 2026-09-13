using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Represents a video mode: the width and height of the display in pixels, and
/// the number of bits each pixel takes.
/// </summary>
public readonly struct Mode
{
    /// <summary>
    /// The width of the display mode, in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the display mode, in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// The color depth of the display mode, i.e. the amount of bits per a single pixel.
    /// </summary>
    public ColorDepth ColorDepth { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mode"/> struct.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="colorDepth">The color depth, i.e. the amount of bits per a single pixel.</param>
    public Mode(int width, int height, ColorDepth colorDepth)
    {
        Width = width;
        Height = height;
        ColorDepth = colorDepth;
    }

    /// <summary>
    /// Checks whether this mode has the same width, height and color depth as
    /// <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The mode to compare with.</param>
    public bool Equals(Mode other)
    {
        return Width == other.Width && Height == other.Height && ColorDepth == other.ColorDepth;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Mode mode && Equals(mode);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // overflow is acceptable in this case
        unchecked
        {
            int hash = Width.GetHashCode();
            hash = hash * 17 + Height.GetHashCode();
            hash = hash * 31 + ColorDepth.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Orders modes by resolution: negative when both dimensions are smaller
    /// than <paramref name="other"/>'s, positive when both are larger, zero
    /// otherwise. Color depth does not participate in the ordering.
    /// </summary>
    /// <param name="other">The mode to compare with.</param>
    public int CompareTo(Mode other)
    {
        if (Width < other.Width && Height < other.Height)
        {
            return -1;
        }

        if (Width > other.Width && Height > other.Height)
        {
            return 1;
        }

        // They are effectively Equals
        return 0;
    }

    /// <summary>Checks whether the two modes are equal.</summary>
    /// <param name="a">The first mode.</param>
    /// <param name="b">The second mode.</param>
    public static bool operator ==(Mode a, Mode b) => a.Equals(b);

    /// <summary>Checks whether the two modes differ.</summary>
    /// <param name="a">The first mode.</param>
    /// <param name="b">The second mode.</param>
    public static bool operator !=(Mode a, Mode b) => !(a == b);

    /// <summary>Checks whether <paramref name="a"/> has a higher resolution than <paramref name="b"/>, per <see cref="CompareTo"/>.</summary>
    /// <param name="a">The first mode.</param>
    /// <param name="b">The second mode.</param>
    public static bool operator >(Mode a, Mode b) => a.CompareTo(b) > 0;

    /// <summary>Checks whether <paramref name="a"/> has a lower resolution than <paramref name="b"/>, per <see cref="CompareTo"/>.</summary>
    /// <param name="a">The first mode.</param>
    /// <param name="b">The second mode.</param>
    public static bool operator <(Mode a, Mode b) => a.CompareTo(b) < 0;

    /// <summary>Checks whether <paramref name="a"/> compares greater than or equal to <paramref name="b"/>, per <see cref="CompareTo"/>.</summary>
    /// <param name="a">The first mode.</param>
    /// <param name="b">The second mode.</param>
    public static bool operator >=(Mode a, Mode b) => a.CompareTo(b) >= 0;

    /// <summary>Checks whether <paramref name="a"/> compares less than or equal to <paramref name="b"/>, per <see cref="CompareTo"/>.</summary>
    /// <param name="a">The first mode.</param>
    /// <param name="b">The second mode.</param>
    public static bool operator <=(Mode a, Mode b) => a.CompareTo(b) <= 0;

    /// <summary>
    /// Formats the mode as <c>width x height @ depth</c>, e.g. <c>1024x768@32</c>.
    /// </summary>
    public override string ToString()
    {
        return Width + "x" + Height + "@" + (int)ColorDepth;
    }
}
