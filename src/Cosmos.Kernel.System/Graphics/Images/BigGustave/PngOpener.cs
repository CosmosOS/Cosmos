namespace BigGustave
{
    using System;
    using System.IO;
    using System.Text;
    using ICSharpCode.SharpZipLib.Zip.Compression;

    internal static class PngOpener
    {
        public static Png Open(Stream stream, IChunkVisitor? chunkVisitor = null) => Open(stream, new PngOpenerSettings
        {
            ChunkVisitor = chunkVisitor
        });

        public static Png Open(Stream stream, PngOpenerSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            HeaderValidationResult validHeader = HasValidHeader(stream);

            if (!validHeader.IsValid)
            {
                throw new ArgumentException($"The provided stream did not start with the PNG header. Got {validHeader}.");
            }

            byte[] crc = new byte[4];
            ImageHeader imageHeader = ReadImageHeader(stream, crc);

            bool hasEncounteredImageEnd = false;

            Palette? palette = null;

            using (MemoryStream output = new MemoryStream())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    while (TryReadChunkHeader(stream, out ChunkHeader header))
                    {
                        if (hasEncounteredImageEnd)
                        {
                            if (settings?.DisallowTrailingData == true)
                            {
                                throw new InvalidOperationException($"Found another chunk {header} after already reading the IEND chunk.");
                            }

                            break;
                        }

                        byte[] bytes = new byte[header.Length];
                        int read = stream.Read(bytes, 0, bytes.Length);
                        if (read != bytes.Length)
                        {
                            throw new InvalidOperationException($"Did not read {header.Length} bytes for the {header} header, only found: {read}.");
                        }

                        if (header.IsCritical)
                        {
                            switch (header.Name)
                            {
                                case "PLTE":
                                    if (header.Length % 3 != 0)
                                    {
                                        throw new InvalidOperationException($"Palette data must be multiple of 3, got {header.Length}.");
                                    }

                                    // Ignore palette data unless the header.ColorType indicates that the image is paletted.
                                    if ((imageHeader.ColorType & ColorType.PaletteUsed) == ColorType.PaletteUsed)
                                    {
                                        palette = new Palette(bytes);
                                    }

                                    break;

                                case "IDAT":
                                    memoryStream.Write(bytes, 0, bytes.Length);
                                    break;
                                case "IEND":
                                    hasEncounteredImageEnd = true;
                                    break;
                                default:
                                    throw new NotSupportedException($"Encountered critical header {header} which was not recognised.");
                            }
                        }
                        else
                        {
                            switch (header.Name)
                            {
                                case "tRNS":
                                    // Add transparency to palette, if the PLTE chunk has been read.
                                    palette?.SetAlphaValues(bytes);
                                    break;
                            }
                        }

                        read = stream.Read(crc, 0, crc.Length);
                        if (read != 4)
                        {
                            throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
                        }

                        int result = (int)Crc32.Calculate(Encoding.ASCII.GetBytes(header.Name), bytes);
                        int crcActual = (crc[0] << 24) + (crc[1] << 16) + (crc[2] << 8) + crc[3];

                        if (result != crcActual)
                        {
                            throw new InvalidOperationException($"CRC calculated {result} did not match file {crcActual} for chunk: {header.Name}.");
                        }

                        settings?.ChunkVisitor?.Visit(stream, imageHeader, header, bytes, crc);
                    }

                    // The IDAT chunks form a single zlib stream (RFC 1950 header +
                    // deflate data + Adler-32 checksum); the vendored SharpZipLib
                    // inflater consumes and verifies all three parts.
                    Inflater inflater = new Inflater(noHeader: false);
                    inflater.SetInput(memoryStream.ToArray());

                    byte[] buffer = new byte[65536];
                    while (!inflater.IsFinished)
                    {
                        int count = inflater.Inflate(buffer);
                        if (count > 0)
                        {
                            output.Write(buffer, 0, count);
                        }
                        else if (inflater.IsNeedingInput)
                        {
                            throw new InvalidOperationException("The PNG image data (IDAT) ended before the compressed stream was complete.");
                        }
                    }
                }

                byte[] bytesOut = output.ToArray();

                (byte bytesPerPixel, byte samplesPerPixel) = Decoder.GetBytesAndSamplesPerPixel(imageHeader);

                bytesOut = Decoder.Decode(bytesOut, imageHeader, bytesPerPixel, samplesPerPixel);

                return new Png(imageHeader, new RawPngData(bytesOut, bytesPerPixel, palette, imageHeader), palette?.HasAlphaValues ?? false);
            }
        }

        private static HeaderValidationResult HasValidHeader(Stream stream)
        {
            return new HeaderValidationResult(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte(),
                stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
        }

        private static bool TryReadChunkHeader(Stream stream, out ChunkHeader chunkHeader)
        {
            chunkHeader = default;

            long position = stream.Position;
            if (!StreamHelper.TryReadHeaderBytes(stream, out byte[] headerBytes))
            {
                return false;
            }

            int length = StreamHelper.ReadBigEndianInt32(headerBytes, 0);

            string name = Encoding.ASCII.GetString(headerBytes, 4, 4);

            chunkHeader = new ChunkHeader(position, length, name);

            return true;
        }

        private static ImageHeader ReadImageHeader(Stream stream, byte[] crc)
        {
            if (!TryReadChunkHeader(stream, out ChunkHeader header))
            {
                throw new ArgumentException("The provided stream did not contain a single chunk.");
            }

            if (header.Name != "IHDR")
            {
                throw new ArgumentException($"The first chunk was not the IHDR chunk: {header}.");
            }

            if (header.Length != 13)
            {
                throw new ArgumentException($"The first chunk did not have a length of 13 bytes: {header}.");
            }

            byte[] ihdrBytes = new byte[13];
            int read = stream.Read(ihdrBytes, 0, ihdrBytes.Length);

            if (read != 13)
            {
                throw new InvalidOperationException($"Did not read 13 bytes for the IHDR, only found: {read}.");
            }

            read = stream.Read(crc, 0, crc.Length);
            if (read != 4)
            {
                throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
            }

            int width = StreamHelper.ReadBigEndianInt32(ihdrBytes, 0);
            int height = StreamHelper.ReadBigEndianInt32(ihdrBytes, 4);

            byte bitDepth = ihdrBytes[8];
            byte colorType = ihdrBytes[9];
            byte compressionMethod = ihdrBytes[10];
            byte filterMethod = ihdrBytes[11];
            byte interlaceMethod = ihdrBytes[12];

            // Valid PNG color types are 0 (grayscale), 2 (truecolor), 3 (palette),
            // 4 (grayscale + alpha) and 6 (truecolor + alpha).
            if (colorType is not (0 or 2 or 3 or 4 or 6))
            {
                throw new NotSupportedException($"Unsupported color type: {colorType}.");
            }

            if (!IsPermittedBitDepth((ColorType)colorType, bitDepth))
            {
                throw new NotSupportedException($"Bit depth {bitDepth} is not permitted for color type {colorType}.");
            }

            if (compressionMethod != 0)
            {
                throw new NotSupportedException($"Unsupported compression method: {compressionMethod}.");
            }

            if (filterMethod != 0)
            {
                throw new NotSupportedException($"Unsupported filter method: {filterMethod}.");
            }

            if (interlaceMethod is not (0 or 1))
            {
                throw new NotSupportedException($"Unsupported interlace method: {interlaceMethod}.");
            }

            return new ImageHeader
            {
                Width = width,
                Height = height,
                BitDepth = bitDepth,
                ColorType = (ColorType)colorType,
                CompressionMethod = (CompressionMethod)compressionMethod,
                FilterMethod = (FilterMethod)filterMethod,
                InterlaceMethod = (InterlaceMethod)interlaceMethod
            };
        }

        private static bool IsPermittedBitDepth(ColorType colorType, byte bitDepth)
        {
            switch (colorType)
            {
                case ColorType.None:
                    return bitDepth is 1 or 2 or 4 or 8 or 16;
                case ColorType.PaletteUsed | ColorType.ColorUsed:
                    return bitDepth is 1 or 2 or 4 or 8;
                case ColorType.ColorUsed:
                case ColorType.AlphaChannelUsed:
                case ColorType.ColorUsed | ColorType.AlphaChannelUsed:
                    return bitDepth is 8 or 16;
                default:
                    return false;
            }
        }
    }
}
