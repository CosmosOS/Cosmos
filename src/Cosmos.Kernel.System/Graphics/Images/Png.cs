using System.IO;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Represents a PNG image. All PNG color types are supported (grayscale,
/// truecolor and palette, with or without alpha), at every legal bit depth,
/// with or without Adam7 interlacing. Decoding is done entirely in managed
/// code by the vendored BigGustave decoder and SharpZipLib inflater (see the
/// Credits page of the documentation).
/// </summary>
public sealed class Png : Image
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Png"/> class, using the specified path to a PNG file.
    /// </summary>
    /// <param name="path">Path to the PNG file.</param>
    public Png(string path) : base(0, 0, ColorDepth.ColorDepth32)
    {
        using FileStream stream = new FileStream(path, FileMode.Open);
        Create(stream);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Png"/> class from the bytes of a PNG file.
    /// </summary>
    /// <param name="imageData">The bytes of the PNG file.</param>
    public Png(byte[] imageData) : base(0, 0, ColorDepth.ColorDepth32)
    {
        using MemoryStream stream = new MemoryStream(imageData);
        Create(stream);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Png"/> class from a stream containing PNG data.
    /// </summary>
    /// <param name="stream">The stream containing PNG data.</param>
    public Png(Stream stream) : base(0, 0, ColorDepth.ColorDepth32)
    {
        Create(stream);
    }

    private void Create(Stream stream)
    {
        BigGustave.Png png = BigGustave.Png.Open(stream);

        int width = png.Width;
        int height = png.Height;

        Width = width;
        Height = height;
        ColorDepth = ColorDepth.ColorDepth32;
        RawData = new int[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                BigGustave.Pixel pixel = png.GetPixel(x, y);
                RawData[(y * width) + x] =
                    (pixel.A << 24) | (pixel.R << 16) | (pixel.G << 8) | pixel.B;
            }
        }
    }
}
