namespace BigGustave
{
    using System;

    /// <summary>
    /// Provides convenience methods for indexing into a raw byte array to extract pixel values.
    /// </summary>
    internal class RawPngData
    {
        private readonly byte[] data;
        private readonly int bytesPerPixel;
        private readonly int width;
        private readonly Palette? palette;
        private readonly ColorType colorType;
        private readonly int rowOffset;
        private readonly int bitDepth;

        /// <summary>
        /// Create a new <see cref="RawPngData"/>.
        /// </summary>
        /// <param name="data">The decoded pixel data as bytes.</param>
        /// <param name="bytesPerPixel">The number of bytes in each pixel.</param>
        /// <param name="palette">The palette for the image.</param>
        /// <param name="imageHeader">The image header.</param>
        public RawPngData(byte[] data, int bytesPerPixel, Palette? palette, ImageHeader imageHeader)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException($"Width must be greater than or equal to 0, got {width}.");
            }

            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.bytesPerPixel = bytesPerPixel;
            this.palette = palette;

            width = imageHeader.Width;
            colorType = imageHeader.ColorType;
            rowOffset = imageHeader.InterlaceMethod == InterlaceMethod.Adam7 ? 0 : 1;
            bitDepth = imageHeader.BitDepth;
        }

        public Pixel GetPixel(int x, int y)
        {
            if (palette != null)
            {
                var pixelsPerByte = (8 / bitDepth);

                var bytesInRow = (1 + (width / pixelsPerByte));

                var byteIndexInRow = x / pixelsPerByte;
                var paletteIndex = (1 + (y * bytesInRow)) + byteIndexInRow;

                var b = data[paletteIndex];

                if (bitDepth == 8)
                {
                    return palette.GetPixel(b);
                }

                var withinByteIndex = x % pixelsPerByte;
                var rightShift = 8 - ((withinByteIndex + 1) * bitDepth);
                var indexActual = (b >> rightShift) & ((1 << bitDepth) - 1);

                return palette.GetPixel(indexActual);
            }

            var rowStartPixel = (rowOffset + (rowOffset * y)) + (bytesPerPixel * width * y);

            var pixelStartIndex = rowStartPixel + (bytesPerPixel * x);

            var first = data[pixelStartIndex];

            switch (bytesPerPixel)
            {
                case 1:
                    return new Pixel(first, first, first, 255, true);
                case 2:
                    switch (colorType)
                    {
                        case ColorType.None:
                        {
                            byte second = data[pixelStartIndex + 1];
                            var value = ToSingleByte(first, second);
                            return new Pixel(value, value, value, 255, true);
                        }
                        default:
                            return new Pixel(first, first, first, data[pixelStartIndex + 1], true);
                    }

                case 3:
                    return new Pixel(first, data[pixelStartIndex + 1], data[pixelStartIndex + 2], 255, false);

                case 4:
                    switch (colorType)
                    {
                        case ColorType.None | ColorType.AlphaChannelUsed:
                        {
                            var second = data[pixelStartIndex + 1];
                            var firstAlpha = data[pixelStartIndex + 2];
                            var secondAlpha = data[pixelStartIndex + 3];
                            var gray = ToSingleByte(first, second);
                            var alpha = ToSingleByte(firstAlpha, secondAlpha);
                            return new Pixel(gray, gray, gray, alpha, true);
                        }
                        case ColorType.ColorUsed | ColorType.AlphaChannelUsed:
                        {
                            var red = first;
                            var green = data[pixelStartIndex + 1];
                            var blue = data[pixelStartIndex + 2];
                            var alpha = data[pixelStartIndex + 3];
                            return new Pixel(red, green, blue, alpha, false);
                        }
                        default:
                            return new Pixel(first, data[pixelStartIndex + 1], data[pixelStartIndex + 2], data[pixelStartIndex + 3], false);
                    }

                case 6:
                    return new Pixel(first, data[pixelStartIndex + 2], data[pixelStartIndex + 4], 255, false);

                case 8:
                    return new Pixel(first, data[pixelStartIndex + 2], data[pixelStartIndex + 4], data[pixelStartIndex + 6], false);

                default:
                    throw new InvalidOperationException($"Unrecognized number of bytes per pixel: {bytesPerPixel}.");
            }
        }


        private static byte ToSingleByte(byte first, byte second)
        {
            var us = (first << 8) + second;
            var result = (byte)Math.Round((255 * us) / (double)ushort.MaxValue);
            return result;
        }
    }
}
