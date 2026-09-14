using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Represents a raster image.
/// </summary>
public abstract class Image
{
    /// <summary>
    /// The raw data of the image. This array holds all of the pixel
    /// values of the raster image.
    /// </summary>
    /// <remarks>
    /// Established by the concrete image type, which is why the property
    /// starts null despite its non-nullable type. Like the other three
    /// setters here it is internal: the decoders that fill an image and the
    /// canvases that read pixels back both live in this assembly, and
    /// nothing outside it derives an image.
    /// </remarks>
    public int[] RawData { get; internal set; } = null!;

    /// <summary>
    /// The width of the image.
    /// </summary>
    public int Width { get; internal set; }

    /// <summary>
    /// The height of the image.
    /// </summary>
    public int Height { get; internal set; }

    /// <summary>
    /// The color depth of each pixel of the image, i.e. the amount of bits
    /// per each pixel.
    /// </summary>
    public ColorDepth ColorDepth { get; internal set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="colorDepth">The color depth of each pixel.</param>
    protected Image(int width, int height, ColorDepth colorDepth)
    {
        Width = width;
        Height = height;
        ColorDepth = colorDepth;
    }
}
