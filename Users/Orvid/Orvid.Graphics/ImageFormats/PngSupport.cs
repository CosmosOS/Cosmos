using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compression.Streams;
using Orvid.Compression.Checksums;

namespace Orvid.Graphics.ImageFormats
{
    public class PngImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            PngEncoder p = new PngEncoder();
            p.Encode(i, dest);
        }

        public override Image Load(Stream s)
        {
            PngDecoder p = new PngDecoder();
            return p.Decode(s);
        }

        // Please note, everything below this
        // point was originally from the ImageTools 
        // Library, available here:
        // http://imagetools.codeplex.com/
        //
        //
        // The source has been modified for use in this library.
        // 
        // This disclaimer was last
        // modified on August 9, 2011.

        #region Encoder
        /// <summary>
        /// Image encoder for writing image data to a stream in png format.
        /// </summary>
        private class PngEncoder
        {
            private const int MaxBlockSize = 0xFFFF;
            private Stream _stream;
            private Image _image;

            /// <summary>
            /// Gets or sets a value indicating whether this encoder
            /// will write the image uncompressed the stream.
            /// </summary>
            /// <value>
            /// <c>true</c> if the image should be written uncompressed to
            /// the stream; otherwise, <c>false</c>.
            /// </value>
            public bool IsWritingUncompressed { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is writing
            /// gamma information to the stream. The default value is false.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is writing gamma 
            /// information to the stream.; otherwise, <c>false</c>.
            /// </value>
            public bool IsWritingGamma { get; set; }

            /// <summary>
            /// Gets or sets the gamma value, that will be written
            /// the the stream, when the <see cref="IsWritingGamma"/> property
            /// is set to true. The default value is 2.2f.
            /// </summary>
            /// <value>The gamma value of the image.</value>
            public double Gamma { get; set; }

            /// <summary>
            /// Gets the default file extension for this encoder.
            /// </summary>
            /// <value>The default file extension for this encoder.</value>
            public string Extension
            {
                get { return "PNG"; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PngEncoder"/> class.
            /// </summary>
            public PngEncoder()
            {
                Gamma = 2.2f;
            }

            /// <summary>
            /// Indicates if the image encoder supports the specified
            /// file extension.
            /// </summary>
            /// <param name="extension">The file extension.</param>
            /// <returns><c>true</c>, if the encoder supports the specified
            /// extensions; otherwise <c>false</c>.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="extension"/>
            /// is null (Nothing in Visual Basic).</exception>
            /// <exception cref="ArgumentException"><paramref name="extension"/> is a string
            /// of length zero or contains only blanks.</exception>
            public bool IsSupportedFileExtension(string extension)
            {
                string extensionAsUpper = extension.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
                return extensionAsUpper == "PNG";
            }

            public void Encode(Image image, Stream stream)
            {
                _image = image;
                _stream = stream;
                // Write the png header.
                stream.Write(
                    new byte[] 
                { 
                    0x89, 0x50, 0x4E, 0x47, 
                    0x0D, 0x0A, 0x1A, 0x0A 
                }, 0, 8);

                PngHeader header = new PngHeader();
                header.Width = image.Width;
                header.Height = image.Height;
                header.ColorType = 6;
                header.BitDepth = 8;
                header.FilterMethod = 0;
                header.CompressionMethod = 0;
                header.InterlaceMethod = 0;

                WriteHeaderChunk(header);

                WritePhysicsChunk();
                WriteGammaChunk();

                if (IsWritingUncompressed)
                {
                    WriteDataChunksFast();
                }
                else
                {
                    WriteDataChunks();
                }
                WriteEndChunk();

                stream.Flush();
            }

            private void WritePhysicsChunk()
            {
                //if (_image.DensityX > 0 && _image.DensityY > 0)
                {
#warning TODO: Un-Comment this when DPI is supported in Orvid.Graphics.Image
                    //int dpmX = (int)Math.Round(_image.DensityX * 39.3700787d);
                    //int dpmY = (int)Math.Round(_image.DensityY * 39.3700787d);

                    // DPI isn't currently supported,
                    // so used 75 as the default.
                    int dpmX = 2953;
                    int dpmY = 2953;

                    byte[] chunkData = new byte[9];

                    WriteInteger(chunkData, 0, dpmX);
                    WriteInteger(chunkData, 4, dpmY);

                    chunkData[8] = 1;

                    WriteChunk(PngChunkTypes.Physical, chunkData);
                }
            }

            private void WriteGammaChunk()
            {
                if (IsWritingGamma)
                {
                    int gammeValue = (int)(Gamma * 100000f);

                    byte[] fourByteData = new byte[4];

                    byte[] size = BitConverter.GetBytes(gammeValue);
                    fourByteData[0] = size[3]; fourByteData[1] = size[2]; fourByteData[2] = size[1]; fourByteData[3] = size[0];

                    WriteChunk(PngChunkTypes.Gamma, fourByteData);
                }
            }

            private void WriteDataChunksFast()
            {
                byte[] pixels = ConvertPixelArrayToByteArray(_image.Data);

                // Convert the pixel array to a new array for adding
                // the filter byte.
                // --------------------------------------------------
                byte[] data = new byte[_image.Width * _image.Height * 4 + _image.Height];

                int rowLength = _image.Width * 4 + 1;

                for (int y = 0; y < _image.Height; y++)
                {
                    data[y * rowLength] = 0;

                    Array.Copy(pixels, y * _image.Width * 4, data, y * rowLength + 1, _image.Width * 4);
                }
                // --------------------------------------------------

                Adler32 adler32 = new Adler32();
                adler32.Update(data);

                using (MemoryStream tempStream = new MemoryStream())
                {
                    int remainder = data.Length;

                    int blockCount;
                    if ((data.Length % MaxBlockSize) == 0)
                    {
                        blockCount = data.Length / MaxBlockSize;
                    }
                    else
                    {
                        blockCount = (data.Length / MaxBlockSize) + 1;
                    }

                    // Write headers
                    tempStream.WriteByte(0x78);
                    tempStream.WriteByte(0xDA);

                    for (int i = 0; i < blockCount; i++)
                    {
                        // Write the length
                        ushort length = (ushort)((remainder < MaxBlockSize) ? remainder : MaxBlockSize);

                        if (length == remainder)
                        {
                            tempStream.WriteByte(0x01);
                        }
                        else
                        {
                            tempStream.WriteByte(0x00);
                        }

                        tempStream.Write(BitConverter.GetBytes(length), 0, 2);

                        // Write one's compliment of length
                        tempStream.Write(BitConverter.GetBytes((ushort)~length), 0, 2);

                        // Write blocks
                        tempStream.Write(data, (int)(i * MaxBlockSize), length);

                        // Next block
                        remainder -= MaxBlockSize;
                    }

                    WriteInteger(tempStream, (int)adler32.Value);

                    tempStream.Seek(0, SeekOrigin.Begin);

                    byte[] zipData = new byte[tempStream.Length];
                    tempStream.Read(zipData, 0, (int)tempStream.Length);

                    WriteChunk(PngChunkTypes.Data, zipData);
                }
            }

            private byte[] ConvertPixelArrayToByteArray(Pixel[] a)
            {
                byte[] b = new byte[a.Length * 4];

                int indx = 0;
                Pixel p;
                for (uint i = 0; i < a.Length; i++)
                {
                    p = a[i];
                    b[indx] = p.R;
                    indx++;
                    b[indx] = p.G;
                    indx++;
                    b[indx] = p.B;
                    indx++;
                    b[indx] = p.A;
                    indx++;
                }

                return b;
            }

            private void WriteDataChunks()
            {
                byte[] pixels = ConvertPixelArrayToByteArray(_image.Data);

                byte[] data = new byte[_image.Width * _image.Height * 4 + _image.Height];

                int rowLength = _image.Width * 4 + 1;

                for (int y = 0; y < _image.Height; y++)
                {
                    byte compression = 0;
                    if (y > 0)
                    {
                        compression = 2;
                    }
                    data[y * rowLength] = compression;

                    for (int x = 0; x < _image.Width; x++)
                    {
                        // Calculate the offset for the new array.
                        int dataOffset = y * rowLength + x * 4 + 1;

                        // Calculate the offset for the original pixel array.
                        int pixelOffset = (y * _image.Width + x) * 4;

                        data[dataOffset + 0] = pixels[pixelOffset + 0];
                        data[dataOffset + 1] = pixels[pixelOffset + 1];
                        data[dataOffset + 2] = pixels[pixelOffset + 2];
                        data[dataOffset + 3] = pixels[pixelOffset + 3];

                        if (y > 0)
                        {
                            int lastOffset = ((y - 1) * _image.Width + x) * 4;

                            data[dataOffset + 0] -= pixels[lastOffset + 0];
                            data[dataOffset + 1] -= pixels[lastOffset + 1];
                            data[dataOffset + 2] -= pixels[lastOffset + 2];
                            data[dataOffset + 3] -= pixels[lastOffset + 3];
                        }
                    }
                }

                byte[] buffer = null;
                int bufferLength = 0;

                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream();
                    using (DeflaterOutputStream zStream = new DeflaterOutputStream(memoryStream))
                    {
                        memoryStream = null;

                        zStream.Write(data, 0, data.Length);
                        zStream.Flush();
                        zStream.Finish();

                        bufferLength = (int)memoryStream.Length;
                        buffer = memoryStream.GetBuffer();
                    }
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                    }
                }

                int numChunks = bufferLength / MaxBlockSize;

                if (bufferLength % MaxBlockSize != 0)
                {
                    numChunks++;
                }

                for (int i = 0; i < numChunks; i++)
                {
                    int length = bufferLength - i * MaxBlockSize;

                    if (length > MaxBlockSize)
                    {
                        length = MaxBlockSize;
                    }

                    WriteChunk(PngChunkTypes.Data, buffer, i * MaxBlockSize, length);
                }
            }

            private void WriteEndChunk()
            {
                WriteChunk(PngChunkTypes.End, null);
            }

            private void WriteHeaderChunk(PngHeader header)
            {
                byte[] chunkData = new byte[13];

                WriteInteger(chunkData, 0, header.Width);
                WriteInteger(chunkData, 4, header.Height);

                chunkData[8] = header.BitDepth;
                chunkData[9] = header.ColorType;
                chunkData[10] = header.CompressionMethod;
                chunkData[11] = header.FilterMethod;
                chunkData[12] = header.InterlaceMethod;

                WriteChunk(PngChunkTypes.Header, chunkData);
            }

            private void WriteChunk(string type, byte[] data)
            {
                WriteChunk(type, data, 0, data != null ? data.Length : 0);
            }

            private void WriteChunk(string type, byte[] data, int offset, int length)
            {
                WriteInteger(_stream, length);

                byte[] typeArray = new byte[4];
                typeArray[0] = (byte)type[0];
                typeArray[1] = (byte)type[1];
                typeArray[2] = (byte)type[2];
                typeArray[3] = (byte)type[3];

                _stream.Write(typeArray, 0, 4);

                if (data != null)
                {
                    _stream.Write(data, offset, length);
                }

                Crc32 crc32 = new Crc32();
                crc32.Update(typeArray);

                if (data != null)
                {
                    crc32.Update(data, offset, length);
                }

                WriteInteger(_stream, (uint)crc32.Value);
            }

            private static void WriteInteger(byte[] data, int offset, int value)
            {
                byte[] buffer = BitConverter.GetBytes(value);

                Array.Reverse(buffer);
                Array.Copy(buffer, 0, data, offset, 4);
            }

            private static void WriteInteger(Stream stream, int value)
            {
                byte[] buffer = BitConverter.GetBytes(value);

                Array.Reverse(buffer);

                stream.Write(buffer, 0, 4);
            }

            private static void WriteInteger(Stream stream, uint value)
            {
                byte[] buffer = BitConverter.GetBytes(value);

                Array.Reverse(buffer);

                stream.Write(buffer, 0, 4);
            }
        }
        #endregion

        #region Decoder
        private class PngDecoder
        {
            private static readonly Dictionary<int, PngColorTypeInformation> _colorTypes = new Dictionary<int, PngColorTypeInformation>();
            private Stream _stream;
            private PngHeader _header;

            static PngDecoder()
            {
                _colorTypes.Add(0,
                    new PngColorTypeInformation(1, new int[] { 1, 2, 4, 8 },
                        (p, a) => new GrayscaleReader(false)));

                _colorTypes.Add(2,
                    new PngColorTypeInformation(3, new int[] { 8 },
                        (p, a) => new TrueColorReader(false)));

                _colorTypes.Add(3,
                    new PngColorTypeInformation(1, new int[] { 1, 2, 4, 8 },
                        (p, a) => new PaletteIndexReader(p, a)));

                _colorTypes.Add(4,
                    new PngColorTypeInformation(2, new int[] { 8 },
                        (p, a) => new GrayscaleReader(true)));

                _colorTypes.Add(6,
                    new PngColorTypeInformation(4, new int[] { 8 },
                        (p, a) => new TrueColorReader(true)));
            }

            #region IImageDecoder Members

            /// <summary>
            /// Gets the size of the header for this image type.
            /// </summary>
            /// <value>The size of the header.</value>
            public int HeaderSize
            {
                get { return 8; }
            }

            /// <summary>
            /// Indicates if the image decoder supports the specified
            /// file extension.
            /// </summary>
            /// <param name="extension">The file extension.</param>
            /// <returns>
            /// 	<c>true</c>, if the decoder supports the specified
            /// extensions; otherwise <c>false</c>.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="extension"/>
            /// is null (Nothing in Visual Basic).</exception>
            /// <exception cref="ArgumentException"><paramref name="extension"/> is a string
            /// of length zero or contains only blanks.</exception>
            public bool IsSupportedFileExtension(string extension)
            {
                string extensionAsUpper = extension.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
                return extensionAsUpper == "PNG";
            }

            /// <summary>
            /// Indicates if the image decoder supports the specified
            /// file header.
            /// </summary>
            /// <param name="header">The file header.</param>
            /// <returns>
            /// <c>true</c>, if the decoder supports the specified
            /// file header; otherwise <c>false</c>.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="header"/>
            /// is null (Nothing in Visual Basic).</exception>
            public bool IsSupportedFileFormat(byte[] header)
            {
                bool isPng = false;

                if (header.Length >= 8)
                {
                    isPng =
                        header[0] == 0x89 &&
                        header[1] == 0x50 && // P
                        header[2] == 0x4E && // N
                        header[3] == 0x47 && // G
                        header[4] == 0x0D && // CR
                        header[5] == 0x0A && // LF
                        header[6] == 0x1A && // EOF
                        header[7] == 0x0A;   // LF
                }

                return isPng;
            }

            /// <summary>
            /// Decodes the image from the specified stream and sets
            /// the data to image.
            /// </summary>
            /// <param name="image">The image, where the data should be set to.
            /// Cannot be null (Nothing in Visual Basic).</param>
            /// <param name="stream">The stream, where the image should be
            /// decoded from. Cannot be null (Nothing in Visual Basic).</param>
            /// <exception cref="ArgumentNullException">
            /// 	<para><paramref name="image"/> is null (Nothing in Visual Basic).</para>
            /// 	<para>- or -</para>
            /// 	<para><paramref name="stream"/> is null (Nothing in Visual Basic).</para>
            /// </exception>
            public Image Decode(Stream stream)
            {
                _stream = stream;
                _stream.Seek(8, SeekOrigin.Current);

                bool isEndChunckReached = false;

                PngChunk currentChunk = null;

                byte[] palette = null;
                byte[] paletteAlpha = null;

                using (MemoryStream dataStream = new MemoryStream())
                {
                    while ((currentChunk = ReadChunk()) != null)
                    {
                        if (isEndChunckReached)
                        {
                            throw new Exception("Image does not end with end chunk.");
                        }

                        if (currentChunk.Type == PngChunkTypes.Header)
                        {
                            ReadHeaderChunk(currentChunk.Data);

                            ValidateHeader();
                        }
                        else if (currentChunk.Type == PngChunkTypes.Physical)
                        {
                            ReadPhysicalChunk(currentChunk.Data);
                        }
                        else if (currentChunk.Type == PngChunkTypes.Data)
                        {
                            dataStream.Write(currentChunk.Data, 0, currentChunk.Data.Length);
                        }
                        else if (currentChunk.Type == PngChunkTypes.Palette)
                        {
                            palette = currentChunk.Data;
                        }
                        else if (currentChunk.Type == PngChunkTypes.PaletteAlpha)
                        {
                            paletteAlpha = currentChunk.Data;
                        }
                        else if (currentChunk.Type == PngChunkTypes.Text)
                        {
                            ReadTextChunk(currentChunk.Data);
                        }
                        else if (currentChunk.Type == PngChunkTypes.End)
                        {
                            isEndChunckReached = true;
                        }
                    }

                    byte[] pixels = new byte[_header.Width * _header.Height * 4];

                    PngColorTypeInformation colorTypeInformation = _colorTypes[_header.ColorType];

                    if (colorTypeInformation != null)
                    {
                        ColorReader colorReader = colorTypeInformation.CreateColorReader(palette, paletteAlpha);

                        ReadScanlines(dataStream, pixels, colorReader, colorTypeInformation);
                    }
                    Image i = new Image(_header.Width, _header.Height);
                    int indx = 0;
                    byte r, g, b, a;
                    for (uint y = 0; y < i.Height; y++)
                    {
                        for (uint x = 0; x < i.Width; x++)
                        {
                            r = pixels[indx];
                            indx++;
                            g = pixels[indx];
                            indx++;
                            b = pixels[indx];
                            indx++;
                            a = pixels[indx];
                            indx++;
                            i.SetPixel(x, y, new Pixel(r, g, b, a));
                        }
                    }
                    pixels = null;
                    System.GC.Collect();
                    return i;
                }
            }

            private void ReadPhysicalChunk(byte[] data)
            {
                Array.Reverse(data, 0, 4);
                Array.Reverse(data, 4, 4);

                //_image.DensityX = BitConverter.ToInt32(data, 0) / 39.3700787d;
                //_image.DensityY = BitConverter.ToInt32(data, 4) / 39.3700787d;
            }

            private int CalculateScanlineLength(PngColorTypeInformation colorTypeInformation)
            {
                int scanlineLength = (_header.Width * _header.BitDepth * colorTypeInformation.ChannelsPerColor);

                int amount = scanlineLength % 8;
                if (amount != 0)
                {
                    scanlineLength += 8 - amount;
                }

                return scanlineLength / 8;
            }

            private int CalculateScanlineStep(PngColorTypeInformation colorTypeInformation)
            {
                int scanlineStep = 1;

                if (_header.BitDepth >= 8)
                {
                    scanlineStep = (colorTypeInformation.ChannelsPerColor * _header.BitDepth) / 8;
                }

                return scanlineStep;
            }

            private void ReadScanlines(MemoryStream dataStream, byte[] pixels, ColorReader colorReader, PngColorTypeInformation colorTypeInformation)
            {
                dataStream.Position = 0;

                int scanlineLength = CalculateScanlineLength(colorTypeInformation);

                int scanlineStep = CalculateScanlineStep(colorTypeInformation);

                byte[] lastScanline = new byte[scanlineLength];
                byte[] currScanline = new byte[scanlineLength];

                byte a = 0;
                byte b = 0;
                byte c = 0;

                int row = 0, filter = 0, column = -1;

                using (DeflaterInputStream compressedStream = new DeflaterInputStream(dataStream))
                {
                    int readByte = 0;
                    while ((readByte = compressedStream.ReadByte()) >= 0)
                    {
                        if (column == -1)
                        {
                            filter = readByte;

                            column++;
                        }
                        else
                        {
                            currScanline[column] = (byte)readByte;

                            if (column >= scanlineStep)
                            {
                                a = currScanline[column - scanlineStep];
                                c = lastScanline[column - scanlineStep];
                            }
                            else
                            {
                                a = 0;
                                c = 0;
                            }

                            b = lastScanline[column];

                            if (filter == 1)
                            {
                                currScanline[column] = (byte)(currScanline[column] + a);
                            }
                            else if (filter == 2)
                            {
                                currScanline[column] = (byte)(currScanline[column] + b);
                            }
                            else if (filter == 3)
                            {
                                currScanline[column] = (byte)(currScanline[column] + (byte)Math.Floor((double)((a + b) / 2)));
                            }
                            else if (filter == 4)
                            {
                                currScanline[column] = (byte)(currScanline[column] + PaethPredicator(a, b, c));
                            }

                            column++;

                            if (column == scanlineLength)
                            {
                                colorReader.ReadScanline(currScanline, pixels, _header);

                                column = -1;
                                row++;

                                Swap(ref currScanline, ref lastScanline);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Swaps two references.
            /// </summary>
            /// <typeparam name="TRef">The type of the references to swap.</typeparam>
            /// <param name="lhs">The first reference.</param>
            /// <param name="rhs">The second reference.</param>
            private static void Swap<TRef>(ref TRef lhs, ref TRef rhs) where TRef : class
            {
                TRef tmp = lhs;
                lhs = rhs;
                rhs = tmp;
            }

            private static byte PaethPredicator(byte a, byte b, byte c)
            {
                byte predicator = 0;

                int p = a + b - c;
                int pa = Math.Abs(p - a);
                int pb = Math.Abs(p - b);
                int pc = Math.Abs(p - c);

                if (pa <= pb && pa <= pc)
                {
                    predicator = a;
                }
                else if (pb <= pc)
                {
                    predicator = b;
                }
                else
                {
                    predicator = c;
                }

                return predicator;
            }

            private void ReadTextChunk(byte[] data)
            {
                //int zeroIndex = 0;

                //for (int i = 0; i < data.Length; i++)
                //{
                //    if (data[i] == (byte)0)
                //    {
                //        zeroIndex = i;
                //        break;
                //    }
                //}

                //string name = Encoding.Unicode.GetString(data, 0, zeroIndex);
                //string value = Encoding.Unicode.GetString(data, zeroIndex + 1, data.Length - zeroIndex - 1);

                //_image.Properties.Add(new ImageProperty(name, value));
            }

            private void ReadHeaderChunk(byte[] data)
            {
                _header = new PngHeader();

                Array.Reverse(data, 0, 4);
                Array.Reverse(data, 4, 4);

                _header.Width = BitConverter.ToInt32(data, 0);
                _header.Height = BitConverter.ToInt32(data, 4);

                _header.BitDepth = data[8];
                _header.ColorType = data[9];
                _header.FilterMethod = data[11];
                _header.InterlaceMethod = data[12];
                _header.CompressionMethod = data[10];
            }

            private void ValidateHeader()
            {
                if (!_colorTypes.ContainsKey(_header.ColorType))
                {
                    throw new Exception("Color type is not supported or not valid.");
                }

                bool found = false;
                foreach (int i in _colorTypes[_header.ColorType].SupportedBitDepths)
                {
                    if (i == _header.BitDepth)
                    {
                        found = true;
                        continue;
                    }
                }
                if (!found)
                {
                    throw new Exception("Bit depth is not supported or not valid.");
                }

                if (_header.FilterMethod != 0)
                {
                    throw new Exception("The png specification only defines 0 as filter method.");
                }

                if (_header.InterlaceMethod != 0)
                {
                    throw new Exception("Interlacing is not supported.");
                }
            }

            private PngChunk ReadChunk()
            {
                PngChunk chunk = new PngChunk();

                if (ReadChunkLength(chunk) == 0)
                {
                    return null;
                }

                byte[] typeBuffer = ReadChunkType(chunk);

                ReadChunkData(chunk);
                ReadChunkCrc(chunk, typeBuffer);

                return chunk;
            }

            private void ReadChunkCrc(PngChunk chunk, byte[] typeBuffer)
            {
                //    byte[] crcBuffer = new byte[4];

                _stream.Position += 4;
                //    if (numBytes >= 1 && numBytes <= 3)
                //    {
                //        throw new Exception("Image stream is not valid!");
                //    }

                //    Array.Reverse(crcBuffer);

                //    chunk.Crc = BitConverter.ToUInt32(crcBuffer, 0);

                //    Crc32 crc = new Crc32();
                //    crc.Update(typeBuffer);
                //    crc.Update(chunk.Data);

                //    if (crc.Value != chunk.Crc)
                //    {
                //        throw new Exception("CRC Error. PNG Image chunk is corrupt!");
                //    }
            }

            private void ReadChunkData(PngChunk chunk)
            {
                chunk.Data = new byte[chunk.Length];

                _stream.Read(chunk.Data, 0, chunk.Length);
            }

            private byte[] ReadChunkType(PngChunk chunk)
            {
                byte[] typeBuffer = new byte[4];

                int numBytes = _stream.Read(typeBuffer, 0, 4);
                if (numBytes >= 1 && numBytes <= 3)
                {
                    throw new Exception("Image stream is not valid!");
                }

                char[] chars = new char[4];
                chars[0] = (char)typeBuffer[0];
                chars[1] = (char)typeBuffer[1];
                chars[2] = (char)typeBuffer[2];
                chars[3] = (char)typeBuffer[3];

                chunk.Type = new string(chars);

                return typeBuffer;
            }

            private int ReadChunkLength(PngChunk chunk)
            {
                byte[] lengthBuffer = new byte[4];

                int numBytes = _stream.Read(lengthBuffer, 0, 4);
                if (numBytes >= 1 && numBytes <= 3)
                {
                    throw new Exception("Image stream is not valid!");
                }

                Array.Reverse(lengthBuffer);

                chunk.Length = BitConverter.ToInt32(lengthBuffer, 0);

                return numBytes;
            }

            #endregion
        }
        #endregion

        #region ColorReaders

        #region ColorReader
        /// <summary>
        /// Interface for color readers, which are responsible for reading 
        /// different color formats from a png file.
        /// </summary>
        private abstract class ColorReader
        {
            protected byte[] ToArrayByBitsLength(byte[] bytes, int bits)
            {
                byte[] result = null;
                if (bits < 8)
                {
                    result = new byte[bytes.Length * 8 / bits];
                    int factor = (int)Math.Pow(2, bits) - 1;
                    int mask = (0xFF >> (8 - bits));
                    int resultOffset = 0;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        for (int shift = 0; shift < 8; shift += bits)
                        {
                            int colorIndex = (((bytes[i]) >> (8 - bits - shift)) & mask) * (255 / factor);
                            result[resultOffset] = (byte)colorIndex;
                            resultOffset++;
                        }
                    }
                }
                else { result = bytes; }
                return result;
            }

            public abstract void ReadScanline(byte[] scanline, byte[] pixels, PngHeader header);
        }
        #endregion

        #region GrayscaleReader
        /// <summary>
        /// Color reader for reading grayscale colors from a PNG file.
        /// </summary>
        private sealed class GrayscaleReader : ColorReader
        {
            private int _row;
            private bool _useAlpha;
            public GrayscaleReader(bool useAlpha)
            {
                _useAlpha = useAlpha;
            }
            public override void ReadScanline(byte[] scanline, byte[] pixels, PngHeader header)
            {
                int offset = 0;
                byte[] newScanline = ToArrayByBitsLength(scanline, header.BitDepth);
                if (_useAlpha)
                {
                    for (int x = 0; x < header.Width / 2; x++)
                    {
                        offset = (_row * header.Width + x) * 4;
                        pixels[offset + 0] = newScanline[x * 2];
                        pixels[offset + 1] = newScanline[x * 2];
                        pixels[offset + 2] = newScanline[x * 2];
                        pixels[offset + 3] = newScanline[x * 2 + 1];
                    }
                }
                else
                {
                    for (int x = 0; x < header.Width; x++)
                    {
                        offset = (_row * header.Width + x) * 4;
                        pixels[offset + 0] = newScanline[x];
                        pixels[offset + 1] = newScanline[x];
                        pixels[offset + 2] = newScanline[x];
                        pixels[offset + 3] = (byte)255;
                    }
                }
                _row++;
            }
        }
        #endregion

        #region PaletteIndexReader
        /// <summary>
        /// A color reader for reading palette indices from the PNG file.
        /// </summary>
        private sealed class PaletteIndexReader : ColorReader
        {
            private int _row;
            private byte[] _palette;
            private byte[] _paletteAlpha;
            public PaletteIndexReader(byte[] palette, byte[] paletteAlpha)
            {
                _palette = palette;
                _paletteAlpha = paletteAlpha;
            }
            public override void ReadScanline(byte[] scanline, byte[] pixels, PngHeader header)
            {
                byte[] newScanline = ToArrayByBitsLength(scanline, header.BitDepth);

                int offset = 0, index = 0;

                if (_paletteAlpha != null && _paletteAlpha.Length > 0)
                {
                    for (int i = 0; i < header.Width; i++)
                    {
                        index = newScanline[i];
                        offset = (_row * header.Width + i) * 4;
                        pixels[offset + 0] = _palette[index * 3];
                        pixels[offset + 1] = _palette[index * 3 + 1];
                        pixels[offset + 2] = _palette[index * 3 + 2];
                        pixels[offset + 3] = _paletteAlpha.Length > index ? _paletteAlpha[index] : (byte)255;
                    }
                }
                else
                {
                    for (int i = 0; i < header.Width; i++)
                    {
                        index = newScanline[i];
                        offset = (_row * header.Width + i) * 4;
                        pixels[offset + 0] = _palette[index * 3];
                        pixels[offset + 1] = _palette[index * 3 + 1];
                        pixels[offset + 2] = _palette[index * 3 + 2];
                        pixels[offset + 3] = (byte)255;
                    }
                }
                _row++;
            }
        }
        #endregion

        #region TrueColorReader
        /// <summary>
        /// Color reader for reading truecolors from a PNG file. Only colors
        /// with 24 or 32 bit (3 or 4 bytes) per pixel are supported at the moment.
        /// </summary>
        sealed class TrueColorReader : ColorReader
        {
            private int _row;
            private bool _useAlpha;
            public TrueColorReader(bool useAlpha)
            {
                _useAlpha = useAlpha;
            }
            public override void ReadScanline(byte[] scanline, byte[] pixels, PngHeader header)
            {
                int offset = 0;
                byte[] newScanline = ToArrayByBitsLength(scanline, header.BitDepth);
                if (_useAlpha)
                {
                    Array.Copy(newScanline, 0, pixels, _row * header.Width * 4, newScanline.Length);
                }
                else
                {
                    for (int x = 0; x < newScanline.Length / 3; x++)
                    {
                        offset = (_row * header.Width + x) * 4;
                        pixels[offset + 0] = newScanline[x * 3];
                        pixels[offset + 1] = newScanline[x * 3 + 1];
                        pixels[offset + 2] = newScanline[x * 3 + 2];
                        pixels[offset + 3] = (byte)255;
                    }
                }
                _row++;
            }
        }
        #endregion

        #endregion

        #region PngChunkTypes
        /// <summary>
        /// Contains a list of possible chunk type identifier.
        /// </summary>
        private static class PngChunkTypes
        {
            /// <summary>
            /// The first chunk in a png file. Can only exists once. Contains 
            /// common information like the width and the height of the image or
            /// the used compression method.
            /// </summary>
            public const string Header = "IHDR";
            /// <summary>
            /// The PLTE chunk contains from 1 to 256 palette entries, each a three byte
            /// series in the RGB format.
            /// </summary>
            public const string Palette = "PLTE";
            /// <summary>
            /// The IDAT chunk contains the actual image data. The image can contains more
            /// than one chunk of this type. All chunks together are the whole image.
            /// </summary>
            public const string Data = "IDAT";
            /// <summary>
            /// This chunk must appear last. It marks the end of the PNG datastream. 
            /// The chunk's data field is empty. 
            /// </summary>
            public const string End = "IEND";
            /// <summary>
            /// This chunk specifies that the image uses simple transparency: 
            /// either alpha values associated with palette entries (for indexed-color images) 
            /// or a single transparent color (for grayscale and truecolor images). 
            /// </summary>
            public const string PaletteAlpha = "tRNS";
            /// <summary>
            /// Textual information that the encoder wishes to record with the image can be stored in 
            /// tEXt chunks. Each tEXt chunk contains a keyword and a text string.
            /// </summary>
            public const string Text = "tEXt";
            /// <summary>
            /// This chunk specifies the relationship between the image samples and the desired 
            /// display output intensity.
            /// </summary>
            public const string Gamma = "gAMA";
            /// <summary>
            /// The pHYs chunk specifies the intended pixel size or aspect ratio for display of the image. 
            /// </summary>
            public const string Physical = "pHYs";
        }
        #endregion

        #region PngChunk
        /// <summary>
        /// Stores header information about a chunk.
        /// </summary>
        private sealed class PngChunk
        {
            /// <summary>
            /// An unsigned integer giving the number of bytes in the chunk's 
            /// data field. The length counts only the data field, not itself, 
            /// the chunk type code, or the CRC. Zero is a valid length
            /// </summary>
            public int Length;
            /// <summary>
            /// A chunk type as string with 4 chars.
            /// </summary>
            public string Type;
            /// <summary>
            /// The data bytes appropriate to the chunk type, if any. 
            /// This field can be of zero length. 
            /// </summary>
            public byte[] Data;
            ///// <summary>
            ///// A CRC (Cyclic Redundancy Check) calculated on the preceding bytes in the chunk, 
            ///// including the chunk type code and chunk data fields, but not including the length field. 
            ///// The CRC is always present, even for chunks containing no data
            ///// </summary>
            //public uint Crc;
        }
        #endregion

        #region PngColorTypeInformation
        /// <summary>
        /// Contains information that are required when loading a png with a specific color type.
        /// </summary>
        private sealed class PngColorTypeInformation
        {
            /// <summary>
            /// Gets an array with the bit depths that are supported for the color type
            /// where this object is created for.
            /// </summary>
            /// <value>The supported bit depths that can be used in combination with this color type.</value>
            public int[] SupportedBitDepths { get; private set; }

            /// <summary>
            /// Gets a function that is used the create the color reader for the color type where 
            /// this object is created for.
            /// </summary>
            /// <value>The factory function to create the color type.</value>
            public Func<byte[], byte[], ColorReader> ScanlineReaderFactory { get; private set; }

            /// <summary>
            /// Gets a factor that is used when iterating through the scanlines.
            /// </summary>
            /// <value>The scanline factor.</value>
            public int ChannelsPerColor { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PngColorTypeInformation"/> class with 
            /// the scanline factory, the function to create the color reader and the supported bit depths.
            /// </summary>
            /// <param name="scanlineFactor">The scanline factor.</param>
            /// <param name="supportedBitDepths">The supported bit depths.</param>
            /// <param name="scanlineReaderFactory">The factory to create the color reader.</param>
            public PngColorTypeInformation(int scanlineFactor, int[] supportedBitDepths, Func<byte[], byte[], ColorReader> scanlineReaderFactory)
            {
                ChannelsPerColor = scanlineFactor;
                ScanlineReaderFactory = scanlineReaderFactory;

                SupportedBitDepths = supportedBitDepths;
            }

            /// <summary>
            /// Creates the color reader for the color type where this object is create for.
            /// </summary>
            /// <param name="palette">The palette of the image. Can be null when no palette is used.</param>
            /// <param name="paletteAlpha">The alpha palette of the image. Can be null when 
            /// no palette is used for the image or when the image has no alpha.</param>
            /// <returns>The color reader for the image.</returns>
            public ColorReader CreateColorReader(byte[] palette, byte[] paletteAlpha)
            {
                return ScanlineReaderFactory(palette, paletteAlpha);
            }
        }
        #endregion

        #region PngHeader
        /// <summary>
        /// Represents the png header chunk.
        /// </summary>
        private sealed class PngHeader
        {
            /// <summary>
            /// The dimension in x-direction of the image in pixels.
            /// </summary>
            public int Width;
            /// <summary>
            /// The dimension in y-direction of the image in pixels.
            /// </summary>
            public int Height;
            /// <summary>
            /// Bit depth is a single-byte integer giving the number of bits per sample 
            /// or per palette index (not per pixel). Valid values are 1, 2, 4, 8, and 16, 
            /// although not all values are allowed for all color types. 
            /// </summary>
            public byte BitDepth;
            /// <summary>
            /// Color type is a integer that describes the interpretation of the 
            /// image data. Color type codes represent sums of the following values: 
            /// 1 (palette used), 2 (color used), and 4 (alpha channel used).
            /// </summary>
            public byte ColorType;
            /// <summary>
            /// Indicates the method  used to compress the image data. At present, 
            /// only compression method 0 (deflate/inflate compression with a sliding 
            /// window of at most 32768 bytes) is defined.
            /// </summary>
            public byte CompressionMethod;
            /// <summary>
            /// Indicates the preprocessing method applied to the image 
            /// data before compression. At present, only filter method 0 
            /// (adaptive filtering with five basic filter types) is defined.
            /// </summary>
            public byte FilterMethod;
            /// <summary>
            /// Indicates the transmission order of the image data. 
            /// Two values are currently defined: 0 (no interlace) or 1 (Adam7 interlace).
            /// </summary>
            public byte InterlaceMethod;
        }
        #endregion

    }
}
