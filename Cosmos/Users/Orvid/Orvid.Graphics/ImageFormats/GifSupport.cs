using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public static class GifSupport
    {
        public static AnimatedImage Load(Stream s)
        {
            GifDecoder g = new GifDecoder();
            return g.DecodeImage(s);
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



        #region Internals

        #region DisposalMethod
        /// <summary>
        /// Specifies, what to do with the last image 
        /// in an animation sequence.
        /// </summary>
        private enum DisposalMethod : int
        {
            /// <summary>
            /// No disposal specified. The decoder is not 
            /// required to take any action. 
            /// </summary>
            Unspecified = 0,
            /// <summary>
            /// Do not dispose. The graphic is to be left in place. 
            /// </summary>
            NotDispose = 1,
            /// <summary>
            /// Restore to background color. 
            /// The area used by the graphic must be restored to
            /// the background color. 
            /// </summary>
            RestoreToBackground = 2,
            /// <summary>
            /// Restore to previous. The decoder is required to 
            /// restore the area overwritten by the 
            /// graphic with what was there prior to rendering the graphic. 
            /// </summary>
            RestoreToPrevious = 3
        }
        #endregion

        #region GifDecoder
        private class GifDecoder
        {
            private const byte ExtensionIntroducer = 0x21;
            private const byte Terminator = 0;
            private const byte ImageLabel = 0x2C;
            private const byte EndIntroducer = 0x3B;
            private const byte ApplicationExtensionLabel = 0xFF;
            private const byte CommentLabel = 0xFE;
            private const byte ImageDescriptorLabel = 0x2C;
            private const byte PlainTextLabel = 0x01;
            private const byte GraphicControlLabel = 0xF9;
            private AnimatedImage _image;
            private Stream _stream;
            private GifLogicalScreenDescriptor _logicalScreenDescriptor;
            private byte[] _globalColorTable;
            private byte[] _currentFrame;
            private GifGraphicsControlExtension _graphicsControl;


            public AnimatedImage DecodeImage(Stream stream)
            {
                _image = new AnimatedImage();

                _stream = stream;
                _stream.Seek(6, SeekOrigin.Current);

                ReadLogicalScreenDescriptor();

                if (_logicalScreenDescriptor.GlobalColorTableFlag == true)
                {
                    _globalColorTable = new byte[_logicalScreenDescriptor.GlobalColorTableSize * 3];

                    // Read the global color table from the stream
                    stream.Read(_globalColorTable, 0, _globalColorTable.Length);
                }

                int nextFlag = stream.ReadByte();
                while (nextFlag != 0)
                {
                    if (nextFlag == ImageLabel)
                    {
                        ReadFrame();
                    }
                    else if (nextFlag == ExtensionIntroducer)
                    {
                        int gcl = stream.ReadByte();
                        switch (gcl)
                        {
                            case GraphicControlLabel:
                                ReadGraphicalControlExtension();
                                break;
                            case CommentLabel:
                                ReadComments();
                                break;
                            case ApplicationExtensionLabel:
                                Skip(12);
                                break;
                            case PlainTextLabel:
                                Skip(13);
                                break;
                        }
                    }
                    else if (nextFlag == EndIntroducer)
                    {
                        break;
                    }
                    nextFlag = stream.ReadByte();
                }
                return _image;
            }

            private void ReadGraphicalControlExtension()
            {
                byte[] buffer = new byte[6];

                _stream.Read(buffer, 0, buffer.Length);

                byte packed = buffer[1];

                _graphicsControl = new GifGraphicsControlExtension();
                _graphicsControl.DelayTime = BitConverter.ToInt16(buffer, 2);
                _graphicsControl.TransparencyIndex = buffer[4];
                _graphicsControl.TransparencyFlag = (packed & 0x01) == 1;
                _graphicsControl.DisposalMethod = (DisposalMethod)((packed & 0x1C) >> 2);
            }

            private GifImageDescriptor ReadImageDescriptor()
            {
                byte[] buffer = new byte[9];

                _stream.Read(buffer, 0, buffer.Length);

                byte packed = buffer[8];

                GifImageDescriptor imageDescriptor = new GifImageDescriptor();
                imageDescriptor.Left = BitConverter.ToInt16(buffer, 0);
                imageDescriptor.Top = BitConverter.ToInt16(buffer, 2);
                imageDescriptor.Width = BitConverter.ToInt16(buffer, 4);
                imageDescriptor.Height = BitConverter.ToInt16(buffer, 6);
                imageDescriptor.LocalColorTableFlag = ((packed & 0x80) >> 7) == 1;
                imageDescriptor.LocalColorTableSize = 2 << (packed & 0x07);
                imageDescriptor.InterlaceFlag = ((packed & 0x40) >> 6) == 1;

                return imageDescriptor;
            }

            private void ReadLogicalScreenDescriptor()
            {
                byte[] buffer = new byte[7];

                _stream.Read(buffer, 0, buffer.Length);

                byte packed = buffer[4];

                _logicalScreenDescriptor = new GifLogicalScreenDescriptor();
                _logicalScreenDescriptor.Width = BitConverter.ToInt16(buffer, 0);
                _logicalScreenDescriptor.Height = BitConverter.ToInt16(buffer, 2);
                _logicalScreenDescriptor.Background = buffer[5];
                _logicalScreenDescriptor.GlobalColorTableFlag = ((packed & 0x80) >> 7) == 1;
                _logicalScreenDescriptor.GlobalColorTableSize = 2 << (packed & 0x07);
            }

            private void Skip(int length)
            {
                _stream.Seek(length, SeekOrigin.Current);

                int flag = 0;

                while ((flag = _stream.ReadByte()) != 0)
                {
                    _stream.Seek(flag, SeekOrigin.Current);
                }
            }

            private void ReadComments()
            {
                int flag = 0;

                while ((flag = _stream.ReadByte()) != 0)
                {
                    byte[] buffer = new byte[flag];
                    _stream.Read(buffer, 0, flag);
                }
            }

            private void ReadFrame()
            {
                GifImageDescriptor imageDescriptor = ReadImageDescriptor();

                byte[] localColorTable = ReadFrameLocalColorTable(imageDescriptor);

                byte[] indices = ReadFrameIndices(imageDescriptor);

                // Determine the color table for this frame. If there is a local one, use it
                // otherwise use the global color table.
                byte[] colorTable = localColorTable != null ? localColorTable : _globalColorTable;

                ReadFrameColors(indices, colorTable, imageDescriptor);

                int blockSize = _stream.ReadByte();
                if (blockSize > 0)
                {
                    _stream.Seek(blockSize, SeekOrigin.Current);
                }
            }

            private byte[] ReadFrameIndices(GifImageDescriptor imageDescriptor)
            {
                int dataSize = _stream.ReadByte();

                LZWDecoder lzwDecoder = new LZWDecoder(_stream);

                byte[] indices = lzwDecoder.DecodePixels(imageDescriptor.Width, imageDescriptor.Height, dataSize);
                return indices;
            }

            private byte[] ReadFrameLocalColorTable(GifImageDescriptor imageDescriptor)
            {
                byte[] localColorTable = null;

                if (imageDescriptor.LocalColorTableFlag == true)
                {
                    localColorTable = new byte[imageDescriptor.LocalColorTableSize * 3];

                    _stream.Read(localColorTable, 0, localColorTable.Length);
                }

                return localColorTable;
            }

            private void ReadFrameColors(byte[] indices, byte[] colorTable, GifImageDescriptor descriptor)
            {
                int imageWidth = _logicalScreenDescriptor.Width;
                int imageHeight = _logicalScreenDescriptor.Height;

                if (_currentFrame == null)
                {
                    _currentFrame = new byte[imageWidth * imageHeight * 4];
                }

                byte[] lastFrame = null;

                if (_graphicsControl != null &&
                    _graphicsControl.DisposalMethod == DisposalMethod.RestoreToPrevious)
                {
                    lastFrame = new byte[imageWidth * imageHeight * 4];

                    Array.Copy(_currentFrame, lastFrame, lastFrame.Length);
                }

                int offset = 0, i = 0, index = -1;

                int iPass = 0; // the interlace pass
                int iInc = 8; // the interlacing line increment
                int iY = 0; // the current interlaced line
                int writeY = 0; // the target y offset to write to

                for (int y = descriptor.Top; y < descriptor.Top + descriptor.Height; y++)
                {
                    // Check if this image is interlaced.
                    if (descriptor.InterlaceFlag)
                    {
                        // If so then we read lines at predetermined offsets.
                        // When an entire image height worth of offset lines has been read we consider this a pass.
                        // With each pass the number of offset lines changes and the starting line changes.
                        if (iY >= descriptor.Height)
                        {
                            iPass++;
                            switch (iPass)
                            {
                                case 1:
                                    iY = 4;
                                    break;
                                case 2:
                                    iY = 2;
                                    iInc = 4;
                                    break;
                                case 3:
                                    iY = 1;
                                    iInc = 2;
                                    break;
                            }
                        }

                        writeY = iY + descriptor.Top;

                        iY += iInc;
                    }
                    else
                    {
                        writeY = y;
                    }

                    for (int x = descriptor.Left; x < descriptor.Left + descriptor.Width; x++)
                    {
                        offset = writeY * imageWidth + x;

                        index = indices[i];

                        if (_graphicsControl == null ||
                            _graphicsControl.TransparencyFlag == false ||
                            _graphicsControl.TransparencyIndex != index)
                        {
                            _currentFrame[offset * 4 + 0] = colorTable[index * 3 + 0];
                            _currentFrame[offset * 4 + 1] = colorTable[index * 3 + 1];
                            _currentFrame[offset * 4 + 2] = colorTable[index * 3 + 2];
                            _currentFrame[offset * 4 + 3] = (byte)255;
                        }

                        i++;
                    }
                }

                byte[] pixels = new byte[imageWidth * imageHeight * 4];

                Array.Copy(_currentFrame, pixels, pixels.Length);
                _currentFrame = new byte[imageWidth * imageHeight * 4];
                Image frame = new Image(imageWidth, imageHeight);
                
                int indx = 0;
                byte r, g, b, a;
                for (uint y = 0; y < frame.Height; y++)
                {
                    for (uint x = 0; x < frame.Width; x++)
                    {
                        r = pixels[indx];
                        indx++;
                        g = pixels[indx];
                        indx++;
                        b = pixels[indx];
                        indx++;
                        a = pixels[indx];
                        indx++;
                        frame.SetPixel(x, y, new Pixel(r, g, b, a));
                    }
                }
                pixels = null;
                System.GC.Collect();
                _image.AddFrame(frame);


                if (_graphicsControl != null)
                {
                    if (_graphicsControl.DelayTime > 0)
                    {
                        _image.TimePerFrame = _graphicsControl.DelayTime;
                    }

                    if (_graphicsControl.DisposalMethod == DisposalMethod.RestoreToBackground)
                    {
                        Image im = new Image(imageWidth, imageHeight);
                        im.Clear(new Pixel(true));
                        _image.AddFrame(im);
                        _image.Loop = false;
                        
                    }
                    else if (_graphicsControl.DisposalMethod == DisposalMethod.RestoreToPrevious)
                    {
                        _image.Loop = true;
                    }
                }
            }
        }
        #endregion

        #region GifImageDescriptor
        private sealed class GifImageDescriptor
        {
            /// <summary>
            /// Column number, in pixels, of the left edge of the image, 
            /// with respect to the left edge of the Logical Screen. 
            /// Leftmost column of the Logical Screen is 0.
            /// </summary>
            public short Left;
            /// <summary>
            /// Row number, in pixels, of the top edge of the image with 
            /// respect to the top edge of the Logical Screen. 
            /// Top row of the Logical Screen is 0.
            /// </summary>
            public short Top;
            /// <summary>
            /// Width of the image in pixels.
            /// </summary>
            public short Width;
            /// <summary>
            /// Height of the image in pixels.
            /// </summary>
            public short Height;
            /// <summary>
            /// Indicates the presence of a Local Color Table immediately 
            /// following this Image Descriptor.
            /// </summary>
            public bool LocalColorTableFlag;
            /// <summary>
            /// If the Local Color Table Flag is set to 1, the value in this field 
            /// is used to calculate the number of bytes contained in the Local Color Table.
            /// </summary>
            public int LocalColorTableSize;
            /// <summary>
            /// Indicates if the image is interlaced. An image is interlaced 
            /// in a four-pass interlace pattern.
            /// </summary>
            public bool InterlaceFlag;
        }
        #endregion

        #region GifLogicalScreenDescriptor
        private sealed class GifLogicalScreenDescriptor
        {
            /// <summary>
            /// Width, in pixels, of the Logical Screen where the images will 
            /// be rendered in the displaying device.
            /// </summary>
            public short Width;
            /// <summary>
            /// Height, in pixels, of the Logical Screen where the images will be 
            /// rendered in the displaying device.
            /// </summary>
            public short Height;
            /// <summary>
            /// Index into the Global Color Table for the Background Color. 
            /// The Background Color is the color used for those 
            /// pixels on the screen that are not covered by an image.
            /// </summary>
            public byte Background;
            /// <summary>
            /// Flag indicating the presence of a Global Color Table; 
            /// if the flag is set, the Global Color Table will immediately 
            /// follow the Logical Screen Descriptor.
            /// </summary>
            public bool GlobalColorTableFlag;
            /// <summary>
            /// If the Global Color Table Flag is set to 1, 
            /// the value in this field is used to calculate the number of 
            /// bytes contained in the Global Color Table.
            /// </summary>
            public int GlobalColorTableSize;
        }
        #endregion

        #region GifGraphicsControlExtension
        private sealed class GifGraphicsControlExtension
        {
            /// <summary>
            /// Indicates the way in which the graphic is to be treated after being displayed. 
            /// </summary>
            public DisposalMethod DisposalMethod;
            /// <summary>
            /// Indicates whether a transparency index is given in the Transparent Index field. 
            /// (This field is the least significant bit of the byte.) 
            /// </summary>
            public bool TransparencyFlag;
            /// <summary>
            /// The Transparency Index is such that when encountered, the corresponding pixel 
            /// of the display device is not modified and processing goes on to the next pixel.
            /// </summary>
            public int TransparencyIndex;
            /// <summary>
            /// If not 0, this field specifies the number of hundredths (1/100) of a second to 
            /// wait before continuing with the processing of the Data Stream. 
            /// The clock starts ticking immediately after the graphic is rendered. 
            /// This field may be used in conjunction with the User Input Flag field. 
            /// </summary>
            public int DelayTime;
        }
        #endregion

        #region LZWDecoder
        private sealed class LZWDecoder
        {
            private const int StackSize = 4096;
            private const int NullCode = -1;

            private Stream _stream;

            /// <summary>
            /// Initializes a new instance of the <see cref="LZWDecoder"/> class
            /// and sets the stream, where the compressed data should be read from.
            /// </summary>
            /// <param name="stream">The stream. where to read from.</param>
            /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null
            /// (Nothing in Visual Basic).</exception>
            public LZWDecoder(Stream stream)
            {
                _stream = stream;
            }

            /// <summary>
            /// Decodes and uncompresses all pixel indices from the stream.
            /// </summary>
            /// <param name="width">The width of the pixel index array.</param>
            /// <param name="height">The height of the pixel index array.</param>
            /// <param name="dataSize">Size of the data.</param>
            /// <returns>The decoded and uncompressed array.</returns>
            public byte[] DecodePixels(int width, int height, int dataSize)
            {
                byte[] pixels = new byte[width * height];
                int clearCode = 1 << dataSize;
                if (dataSize == Int32.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("dataSize", "Must be less than Int32.MaxValue");
                }
                int codeSize = dataSize + 1;
                int endCode = clearCode + 1;
                int availableCode = clearCode + 2;
                #region Jillzhangs Code (Not From Me) see: http://giflib.codeplex.com/
                int code = NullCode;
                int old_code = NullCode;
                int code_mask = (1 << codeSize) - 1;
                int bits = 0;
                int[] prefix = new int[StackSize];
                int[] suffix = new int[StackSize];
                int[] pixelStatck = new int[StackSize + 1];
                int top = 0;
                int count = 0;
                int bi = 0;
                int xyz = 0;
                int data = 0;
                int first = 0;
                int inCode = NullCode;
                for (code = 0; code < clearCode; code++)
                {
                    prefix[code] = 0;
                    suffix[code] = (byte)code;
                }

                byte[] buffer = null;
                while (xyz < pixels.Length)
                {
                    if (top == 0)
                    {
                        if (bits < codeSize)
                        {
                            if (count == 0)
                            {
                                buffer = ReadBlock();
                                count = buffer.Length;
                                if (count == 0)
                                {
                                    break;
                                }
                                bi = 0;
                            }
                            data += buffer[bi] << bits;
                            bits += 8;
                            bi++;
                            count--;
                            continue;
                        }
                        code = data & code_mask;
                        data >>= codeSize;
                        bits -= codeSize;
                        if (code > availableCode || code == endCode)
                        {
                            break;
                        }
                        if (code == clearCode)
                        {
                            codeSize = dataSize + 1;
                            code_mask = (1 << codeSize) - 1;
                            availableCode = clearCode + 2;
                            old_code = NullCode;
                            continue;
                        }
                        if (old_code == NullCode)
                        {
                            pixelStatck[top++] = suffix[code];
                            old_code = code;
                            first = code;
                            continue;
                        }
                        inCode = code;
                        if (code == availableCode)
                        {
                            pixelStatck[top++] = (byte)first;
                            code = old_code;
                        }
                        while (code > clearCode)
                        {
                            pixelStatck[top++] = suffix[code];
                            code = prefix[code];
                        }
                        first = suffix[code];
                        if (availableCode > StackSize)
                        {
                            break;
                        }
                        pixelStatck[top++] = suffix[code];
                        prefix[availableCode] = old_code;
                        suffix[availableCode] = first;
                        availableCode++;
                        if (availableCode == code_mask + 1 && availableCode < StackSize)
                        {
                            codeSize++;
                            code_mask = (1 << codeSize) - 1;
                        }
                        old_code = inCode;
                    }
                    top--;
                    pixels[xyz++] = (byte)pixelStatck[top];
                }

                #endregion

                return pixels;
            }

            private byte[] ReadBlock()
            {
                int blockSize = _stream.ReadByte();
                return ReadBytes(blockSize);
            }

            private byte[] ReadBytes(int length)
            {
                byte[] buffer = new byte[length];
                _stream.Read(buffer, 0, length);
                return buffer;
            }
        }
        #endregion

        #endregion
    }

}
