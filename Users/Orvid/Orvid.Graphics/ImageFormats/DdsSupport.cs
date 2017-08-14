using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Graphics;

namespace Orvid.Graphics.ImageFormats
{
    public class DdsImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            //byte[] b = DDS.Encode((System.Drawing.Bitmap)i, "dxt5");
            //dest.Write(b, 0, b.Length);
            //b = null;
        }

        public override Image Load(Stream s)
        {
            DDSImage im = new DDSImage(s);
            return im.BitmapImage;
        }
    }
}

#region Internals
// Please note, everything below this
// point was originally from 
// http://sourceforge.net/projects/igaeditor/
//
//
// The source has been modified for use in this library,
// the modifications include extending functionality.
//
// The extended functionality is licensed seperately from
// the rest of this file. Permission for it's use have been 
// granted only for use in Cosmos, and products created
// using the Cosmos toolkit. It is not allowed to be used
// in any other way, shape, or form.
// 
//
// This disclaimer and license was last
// modified on August 10, 2011.

namespace Orvid.Graphics.ImageFormats
{
    public class DDSImage
    {
        #region PixelFormat
        private enum PixelFormat
        {
            /// <summary>
            /// 32-bit image, with 8-bit red, green, blue and alpha.
            /// </summary>
            ARGB,
            /// <summary>
            /// 24-bit image with 8-bit red, green, blue.
            /// </summary>
            RGB,
            /// <summary>
            /// 16-bit DXT-1 compression, 1-bit alpha.
            /// </summary>
            DXT1,
            /// <summary>
            /// DXT-2 Compression
            /// </summary>
            DXT2,
            /// <summary>
            /// DXT-3 Compression
            /// </summary>
            DXT3,
            /// <summary>
            /// DXT-4 Compression
            /// </summary>
            DXT4,
            /// <summary>
            /// DXT-5 Compression
            /// </summary>
            DXT5,
            /// <summary>
            /// 3DC Compression
            /// </summary>
            THREEDC,
            /// <summary>
            /// ATI1n Compression
            /// </summary>
            ATI1N,
            LUMINANCE,
            LUMINANCE_ALPHA,
            RXGB,
            A16B16G16R16,
            R16F,
            G16R16F,
            A16B16G16R16F,
            R32F,
            G32R32F,
            A32B32G32R32F,
            /// <summary>
            /// Unknown pixel format.
            /// </summary>
            UNKNOWN
        }
        #endregion

        private static byte[] DDS_HEADER = Convert.FromBase64String("RERTIA==");

        // fourccs
        private const uint FOURCC_DXT1 = 827611204;
        private const uint FOURCC_DXT2 = 844388420;
        private const uint FOURCC_DXT3 = 861165636;
        private const uint FOURCC_DXT4 = 877942852;
        private const uint FOURCC_DXT5 = 894720068;
        private const uint FOURCC_ATI1 = 826889281;
        private const uint FOURCC_ATI2 = 843666497;
        private const uint FOURCC_RXGB = 1111971922;
        private const uint FOURCC_DOLLARNULL = 36;
        private const uint FOURCC_oNULL = 111;
        private const uint FOURCC_pNULL = 112;
        private const uint FOURCC_qNULL = 113;
        private const uint FOURCC_rNULL = 114;
        private const uint FOURCC_sNULL = 115;
        private const uint FOURCC_tNULL = 116;


        // other defines
        private const uint DDS_LINEARSIZE = 524288;
        private const uint DDS_PITCH = 8;
        private const uint DDS_FOURCC = 4;
        private const uint DDS_LUMINANCE = 131072;
        private const uint DDS_ALPHAPIXELS = 1;

        // headers 
        // DDSURFACEDESC2 structure
        private byte[] signature;
        private uint size1;
        private uint flags1;
        private uint height;
        private uint width;
        private uint linearsize;
        private uint depth;
        private uint mipmapcount;
        private uint alphabitdepth;
        // DDPIXELFORMAT structure
        private uint size2;
        private uint flags2;
        private uint fourcc;
        private uint rgbbitcount;
        private uint rbitmask;
        private uint bbitmask;
        private uint gbitmask;
        private uint alphabitmask;

        // DDCAPS2 structure
        private uint ddscaps1;
        private uint ddscaps2;
        private uint ddscaps3;
        private uint ddscaps4;
        // end DDCAPS2 structure
        private uint texturestage;
        // end DDSURFACEDESC2 structure

        private PixelFormat CompFormat;
        private uint blocksize;

        private uint bpp;
        private uint bps;
        private uint sizeofplane;
        private uint compsize;
        private byte[] compdata;
        private byte[] rawidata;
        private bool Has16BitComponents = false;
        private BinaryReader br;
        private Image img;

        public Image BitmapImage { get { return this.img; } }

        /// <summary>
        /// Constructs a new DDSImage object using the given byte array, which
        /// contains the raw DDS file.
        /// </summary>
        /// <param name="ddsimage">A byte[] containing the DDS file.</param>
        public DDSImage(Stream ms)
        {
            br = new BinaryReader(ms);
            this.signature = br.ReadBytes(4);

            if (!IsByteArrayEqual(this.signature, DDS_HEADER))
            {
                System.Console.WriteLine("Got header of '" + ASCIIEncoding.ASCII.GetString(this.signature, 0, this.signature.Length) + "'.");

                throw new Exception("Not a dds File!");
            }

            //System.Console.WriteLine("Got dds header okay");

            // now read in the rest
            this.size1 = br.ReadUInt32();
            this.flags1 = br.ReadUInt32();
            this.height = br.ReadUInt32();
            this.width = br.ReadUInt32();
            this.linearsize = br.ReadUInt32();
            this.depth = br.ReadUInt32();
            this.mipmapcount = br.ReadUInt32();
            this.alphabitdepth = br.ReadUInt32();

            // skip next 10 uints
            for (int x = 0; x < 10; x++)
            {
                br.ReadUInt32();
            }

            this.size2 = br.ReadUInt32();
            this.flags2 = br.ReadUInt32();
            this.fourcc = br.ReadUInt32();
            this.rgbbitcount = br.ReadUInt32();
            this.rbitmask = br.ReadUInt32();
            this.gbitmask = br.ReadUInt32();
            this.bbitmask = br.ReadUInt32();
            this.alphabitmask = br.ReadUInt32();
            this.ddscaps1 = br.ReadUInt32();
            this.ddscaps2 = br.ReadUInt32();
            this.ddscaps3 = br.ReadUInt32();
            this.ddscaps4 = br.ReadUInt32();
            this.texturestage = br.ReadUInt32();


            // patches for stuff
            if (this.depth == 0)
            {
                this.depth = 1;
            }

            if ((this.flags2 & DDS_FOURCC) > 0)
            {
                blocksize = ((this.width + 3) / 4) * ((this.height + 3) / 4) * this.depth;

                switch (this.fourcc)
                {
                    case FOURCC_DXT1:
                        CompFormat = PixelFormat.DXT1;
                        blocksize *= 8;
                        break;

                    case FOURCC_DXT2:
                        CompFormat = PixelFormat.DXT2;
                        blocksize *= 16;
                        break;

                    case FOURCC_DXT3:
                        CompFormat = PixelFormat.DXT3;
                        blocksize *= 16;
                        break;

                    case FOURCC_DXT4:
                        CompFormat = PixelFormat.DXT4;
                        blocksize *= 16;
                        break;

                    case FOURCC_DXT5:
                        CompFormat = PixelFormat.DXT5;
                        blocksize *= 16;
                        break;

                    case FOURCC_ATI1:
                        CompFormat = PixelFormat.ATI1N;
                        blocksize *= 8;
                        break;

                    case FOURCC_ATI2:
                        CompFormat = PixelFormat.THREEDC;
                        blocksize *= 16;
                        break;

                    case FOURCC_RXGB:
                        CompFormat = PixelFormat.RXGB;
                        blocksize *= 16;
                        break;

                    case FOURCC_DOLLARNULL:
                        CompFormat = PixelFormat.A16B16G16R16;
                        blocksize = this.width * this.height * this.depth * 8;
                        break;

                    case FOURCC_oNULL:
                        CompFormat = PixelFormat.R16F;
                        blocksize = this.width * this.height * this.depth * 2;
                        break;

                    case FOURCC_pNULL:
                        CompFormat = PixelFormat.G16R16F;
                        blocksize = this.width * this.height * this.depth * 4;
                        break;

                    case FOURCC_qNULL:
                        CompFormat = PixelFormat.A16B16G16R16F;
                        blocksize = this.width * this.height * this.depth * 8;
                        break;

                    case FOURCC_rNULL:
                        CompFormat = PixelFormat.R32F;
                        blocksize = this.width * this.height * this.depth * 4;
                        break;

                    case FOURCC_sNULL:
                        CompFormat = PixelFormat.G32R32F;
                        blocksize = this.width * this.height * this.depth * 8;
                        break;

                    case FOURCC_tNULL:
                        CompFormat = PixelFormat.A32B32G32R32F;
                        blocksize = this.width * this.height * this.depth * 16;
                        break;

                    default:
                        CompFormat = PixelFormat.UNKNOWN;
                        blocksize *= 16;
                        break;
                } // switch
            }
            else
            {
                // uncompressed image
                if ((this.flags2 & DDS_LUMINANCE) > 0)
                {
                    if ((this.flags2 & DDS_ALPHAPIXELS) > 0)
                    {
                        CompFormat = PixelFormat.LUMINANCE_ALPHA;
                    }
                    else
                    {
                        CompFormat = PixelFormat.LUMINANCE;
                    }
                }
                else
                {
                    if ((this.flags2 & DDS_ALPHAPIXELS) > 0)
                    {
                        CompFormat = PixelFormat.ARGB;
                    }
                    else
                    {
                        CompFormat = PixelFormat.RGB;
                    }
                }

                blocksize = (this.width * this.height * this.depth * (this.rgbbitcount >> 3));
            }

            if (CompFormat == PixelFormat.UNKNOWN)
            {
                throw new Exception("Invalid Header Format!");
                return;
            }

            if ((this.flags1 & (DDS_LINEARSIZE | DDS_PITCH)) == 0
                || this.linearsize == 0)
            {
                this.flags1 |= DDS_LINEARSIZE;
                this.linearsize = blocksize;
            }


            // get image data
            this.ReadData();

            // allocate bitmap
            this.bpp = this.PixelFormatToBpp(this.CompFormat);
            this.bps = this.width * this.bpp * this.PixelFormatToBpc(this.CompFormat);
            this.sizeofplane = this.bps * this.height;
            this.rawidata = new byte[this.width * this.height * 4];

            Check16BitComponents();

            // decompress
            switch (this.CompFormat)
            {
                case PixelFormat.ARGB:
                case PixelFormat.RGB:
                case PixelFormat.LUMINANCE:
                case PixelFormat.LUMINANCE_ALPHA:
                    this.DecompressARGB();
                    break;

                case PixelFormat.DXT1:
                    this.DecompressDXT1();
                    break;

                case PixelFormat.DXT2:
                    this.DecompressDXT3();
                    //this.CorrectPreMult();
                    break;

                case PixelFormat.DXT3:
                    this.DecompressDXT3();
                    break;

                case PixelFormat.DXT4:
                    this.DecompressDXT5();
                    //this.CorrectPreMult();
                    break;

                case PixelFormat.DXT5:
                    this.DecompressDXT5();
                    break;

                case PixelFormat.THREEDC:
                    this.Decompress3Dc();
                    break;

                case PixelFormat.ATI1N:
                    //throw new Exception();
                    this.DecompressAti1n();
                    break;

                case PixelFormat.A16B16G16R16:
                    this.DecompressA16B16G16R16();
                    break;

                case PixelFormat.R16F:
                case PixelFormat.G16R16F:
                case PixelFormat.A16B16G16R16F:
                case PixelFormat.R32F:
                case PixelFormat.G32R32F:
                case PixelFormat.A32B32G32R32F:
                    this.DecompressFloat();
                    break;

                default:
                    //throw new Exception("Unknown file format!");
                    break;
            }

            this.img = new Image((int)this.width, (int)this.height);

            //try
            //{
            // now fill bitmap with raw image datas.
            uint pos = 0;
            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    // draw
                    this.img.SetPixel((uint)x, (uint)y, new Pixel(this.rawidata[pos], this.rawidata[pos + 1], this.rawidata[pos + 2], this.rawidata[pos + 3]));
                    pos += 4;
                }
            }
            //}
            //catch { }

            // cleanup
            this.rawidata = null;
            this.compdata = null;

        }

        #region Support Methods
        private void CorrectPreMult()
        {
            for (uint i = 0; i < this.rawidata.Length; i += 4)
            {
                if (this.rawidata[i + 3] != 0) // Cannot divide by 0.
                {
                    this.rawidata[i] = (byte)((uint)(this.rawidata[i] << 8) / this.rawidata[i + 3]);
                    this.rawidata[i + 1] = (byte)((uint)(this.rawidata[i + 1] << 8) / this.rawidata[i + 3]);
                    this.rawidata[i + 2] = (byte)((uint)(this.rawidata[i + 2] << 8) / this.rawidata[i + 3]);
                }
            }
        }

        private void Check16BitComponents()
        {
            if (this.rgbbitcount != 32)
            {
                Has16BitComponents = false;
            }

            // a2b10g10r10 format
            if (this.rbitmask == 0x3FF00000 && this.gbitmask == 0x000FFC00 && this.bbitmask == 0x000003FF && this.alphabitmask == 0xC0000000)
            {
                Has16BitComponents = true;
            }
            // a2r10g10b10 format
            else if (this.rbitmask == 0x000003FF && this.gbitmask == 0x000FFC00 && this.bbitmask == 0x3FF00000 && this.alphabitmask == 0xC0000000)
            {
                Has16BitComponents = true;
            }
            else
            {
                Has16BitComponents = false;
            }
            return;
        }

        private static bool IsByteArrayEqual(byte[] arg0, byte[] arg1)
        {
            if (arg0.Length != arg1.Length)
            {
                return false;
            }

            for (int x = 0; x < arg0.Length; x++)
            {
                if (arg0[x] != arg1[x])
                {
                    return false;
                }
            }

            return true;
        }

        // iCompFormatToBpp
        private uint PixelFormatToBpp(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.LUMINANCE:
                case PixelFormat.LUMINANCE_ALPHA:
                case PixelFormat.ARGB:
                case PixelFormat.RGB:
                    return this.rgbbitcount / 8;

                case PixelFormat.THREEDC:
                case PixelFormat.RXGB:
                    return 3;

                case PixelFormat.ATI1N:
                    return 1;

                case PixelFormat.R16F:
                    return 2;

                case PixelFormat.A16B16G16R16:
                case PixelFormat.A16B16G16R16F:
                case PixelFormat.G32R32F:
                    return 8;

                case PixelFormat.A32B32G32R32F:
                    return 16;

                default:
                    return 4;
            }
        }

        // iCompFormatToBpc
        private uint PixelFormatToBpc(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.R16F:
                case PixelFormat.G16R16F:
                case PixelFormat.A16B16G16R16F:
                    return 4;

                case PixelFormat.R32F:
                case PixelFormat.G32R32F:
                case PixelFormat.A32B32G32R32F:
                    return 4;

                case PixelFormat.A16B16G16R16:
                    return 2;

                default:
                    return 1;
            }
        }

        // iCompFormatToChannelCount
        private uint PixelFormatToChannelCount(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.RGB:
                case PixelFormat.THREEDC:
                case PixelFormat.RXGB:
                    return 3;

                case PixelFormat.LUMINANCE:
                case PixelFormat.R16F:
                case PixelFormat.R32F:
                case PixelFormat.ATI1N:
                    return 1;

                case PixelFormat.LUMINANCE_ALPHA:
                case PixelFormat.G16R16F:
                case PixelFormat.G32R32F:
                    return 2;

                default:
                    return 4;
            }
        }

        private void ReadData()
        {
            this.compdata = null;

            if ((this.flags1 & DDS_LINEARSIZE) > 1)
            {
                this.compdata = this.br.ReadBytes((int)this.linearsize);
                this.compsize = (uint)this.compdata.Length;
            }
            else
            {
                this.bps = this.width * (this.rgbbitcount / 8);
                this.compsize = this.bps * this.height * this.depth;
                this.compdata = new byte[this.compsize];

                MemoryStream mem = new MemoryStream((int)this.compsize);


                byte[] temp;
                for (int z = 0; z < this.depth; z++)
                {
                    for (int y = 0; y < this.height; y++)
                    {
                        temp = this.br.ReadBytes((int)this.bps);
                        mem.Write(temp, 0, temp.Length);
                    }
                }
                mem.Seek(0, SeekOrigin.Begin);

                mem.Read(this.compdata, 0, this.compdata.Length);
                mem.Close();
            }
        }

        private void GetBitsFromMask(uint Mask, out byte ShiftLeft, out byte ShiftRight)
        {
            uint Temp, i;
            if (Mask == 0)
            {
                ShiftLeft = ShiftRight = 0;
                return;
            }
            Temp = Mask;
            for (i = 0; i < 32; i++, Temp >>= 1)
            {
                if ((Temp & 1) > 0)
                    break;
            }
            ShiftRight = (byte)i;
            for (i = 0; i < 8; i++, Temp >>= 1)
            {
                if (!((Temp & 1) > 0))
                    break;
            }
            ShiftLeft = (byte)(8 - i);
            return;
        }

        private uint CountBitsFromMask(uint Mask)
        {
            uint i, TestBit = 0x01, Count = 0;
            bool FoundBit = false;

            for (i = 0; i < 32; i++, TestBit <<= 1)
            {
                if ((Mask & TestBit) > 0)
                {
                    if (!FoundBit)
                        FoundBit = true;
                    Count++;
                }
                else if (FoundBit)
                    return Count;
            }

            return Count;
        }
        #endregion


        #region 3Dc
        private void Decompress3Dc()
        {
            int x, y, z, i, j, k, t1, t2;
            int t, tx, ty;
            //double d;
            //double r, g, b;
            int TempLoc = 0, Temp2Loc = 0;
            byte[] XColours = new byte[8], YColours = new byte[8];
            uint bitmask, bitmask2, Offset, CurrOffset;


            Offset = 0;
            for (z = 0; z < depth; z++)
            {
                for (y = 0; y < height; y += 4)
                {
                    for (x = 0; x < width; x += 4)
                    {
                        Temp2Loc = TempLoc + 8;

                        //Read Y palette
                        t1 = YColours[0] = compdata[TempLoc];
                        t2 = YColours[1] = compdata[TempLoc + 1];
                        TempLoc += 2;
                        if (t1 > t2)
                        {
                            for (i = 2; i < 8; ++i)
                            {
                                YColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
                            }
                        }
                        else
                        {
                            for (i = 2; i < 6; ++i)
                            {
                                YColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
                            }
                            YColours[6] = 0;
                            YColours[7] = 255;
                        }

                        // Read X palette
                        t1 = XColours[0] = compdata[Temp2Loc];
                        t2 = XColours[1] = compdata[Temp2Loc + 1];
                        Temp2Loc += 2;
                        if (t1 > t2)
                        {
                            for (i = 2; i < 8; ++i)
                            {
                                XColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
                            }
                        }
                        else
                        {
                            for (i = 2; i < 6; ++i)
                            {
                                XColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
                            }
                            XColours[6] = 0;
                            XColours[7] = 255;
                        }

                        //decompress pixel data
                        CurrOffset = Offset;
                        for (k = 0; k < 4; k += 2)
                        {
                            // First three bytes
                            bitmask = (compdata[TempLoc]) | ((uint)(compdata[TempLoc + 1]) << 8) | ((uint)(compdata[TempLoc + 2]) << 16);
                            bitmask2 = (compdata[Temp2Loc]) | ((uint)(compdata[Temp2Loc + 1]) << 8) | ((uint)(compdata[Temp2Loc + 2]) << 16);
                            for (j = 0; j < 2; j++)
                            {
                                // only put pixels out < height
                                if ((y + k + j) < height)
                                {
                                    for (i = 0; i < 4; i++)
                                    {
                                        // only put pixels out < width
                                        if (((x + i) < width))
                                        {

                                            t1 = (int)(CurrOffset + ((x + i) * 4));
                                            rawidata[t1 + 1] = YColours[bitmask & 7];
                                            rawidata[t1 + 0] = XColours[bitmask2 & 7];
                                            tx = XColours[bitmask2 & 7];
                                            ty = YColours[bitmask & 7];

                                            //calculate b (z) component ((r/255)^2 + (g/255)^2 + (b/255)^2 = 1
                                            //d = (double)255 * Math.Sqrt(((double)tx / 255) + ((double)ty / 255));
                                            //if (d > 255)
                                            //{
                                            //    rawidata[t1 + 2] = (byte)((d - 255) + 127);
                                            //    //rawidata[t1    ] = (byte)(rawidata[t1    ] + 127);
                                            //    //rawidata[t1 + 1] = (byte)((255 - rawidata[t1 + 1]) + 127);
                                            //}
                                            //else
                                            //{
                                            //    rawidata[t1 + 2] = (byte)(d);
                                            //}
                                            //rawidata[t1 + 2] = (d > 255 ? (byte)((d - 255) + 128) : (byte)d);

                                            //if (rawidata[t1 + 2] == 0 && d != 0)
                                            //{
                                            //    throw new Exception();
                                            //}

                                            //r = XColours[bitmask2 & 7];
                                            //g = YColours[bitmask & 7];
                                            //b = (d > 255 ? (byte)255 : (byte)d);
                                            //if ((((r / 255) * (r / 255)) + ((g / 255) * (g / 255)) + ((b / 255) * (b / 255))) != 1)
                                            //{
                                                //throw new Exception();
                                                //rawidata[t1 + 2] = 127;
                                            //}

                                            t = (int)(127 * 128 - ((tx - 127) * (tx - 128)) - ((ty - 127) * (ty - 128)));
                                            if (t > 0)
                                            {
                                                //rawidata[t1 + 2] = 0x7F;
                                                rawidata[t1 + 2] = (byte)(Math.Sqrt(t) + 128);// > 255 ? (byte)255 : (Math.Sqrt(t) + 128));
                                            }
                                            else
                                            {
                                                rawidata[t1 + 2] = 0x7F;
                                            }
                                            rawidata[t1 + 3] = 255;
                                        }
                                        bitmask >>= 3;
                                        bitmask2 >>= 3;
                                    }
                                    CurrOffset += width * 4;
                                }
                            }
                            TempLoc += 3;
                            Temp2Loc += 3;
                        }
                        TempLoc += 8;
                    }
                    Offset += this.width * 16;
                }
            }
        }
        #endregion

        #region Ati1n
        private void DecompressAti1n()
        {
            int x, y, z, i, j, k, t1, t2;
            uint TempLoc = 0;
            byte[] Colours = new byte[8];
            uint bitmask, Offset, CurrOffset;

            Offset = 0;
            for (z = 0; z < depth; z++)
            {
                for (y = 0; y < height; y += 4)
                {
                    for (x = 0; x < width; x += 4)
                    {
                        //Read palette
                        t1 = Colours[0] = compdata[TempLoc];
                        t2 = Colours[1] = compdata[TempLoc + 1];
                        TempLoc += 2;
                        if (t1 > t2)
                        {
                            for (i = 2; i < 8; ++i)
                            {
                                Colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
                            }
                        }
                        else
                        {
                            for (i = 2; i < 6; ++i)
                            {
                                Colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
                            }
                            Colours[6] = 0;
                            Colours[7] = 255;
                        }

                        //decompress pixel data
                        CurrOffset = Offset;
                        for (k = 0; k < 4; k += 2)
                        {
                            // First three bytes
                            bitmask = ((compdata[TempLoc]) | ((uint)(compdata[TempLoc + 1]) << 8) | ((uint)(compdata[TempLoc + 2]) << 16));
                            for (j = 0; j < 2; j++)
                            {
                                // only put pixels out < height
                                if ((y + k + j) < height)
                                {
                                    for (i = 0; i < 4; i++)
                                    {
                                        // only put pixels out < width
                                        if (((x + i) < width))
                                        {
                                            t1 = (byte)(CurrOffset + ((x + i) * 4));
                                            rawidata[t1    ] = Colours[bitmask & 0x07];
                                            rawidata[t1 + 1] = Colours[bitmask & 0x07];
                                            rawidata[t1 + 2] = Colours[bitmask & 0x07];
                                            rawidata[t1 + 3] = 255;
                                        }
                                        bitmask >>= 3;
                                    }
                                    CurrOffset += width * 4;
                                }
                            }
                            TempLoc += 3;
                        }
                    }
                    Offset += this.width * 16;
                }
            }
        }
        #endregion

        //        #region RXGB
        //        ILboolean DecompressRXGB()
        //{
        //    int			x, y, z, i, j, k, Select;
        //    ILubyte		*Temp;
        //    Color565	*color_0, *color_1;
        //    Color8888	colours[4], *col;
        //    ILuint		bitmask, Offset;
        //    ILubyte		alphas[8], *alphamask;
        //    ILuint		bits;

        //    if (!CompData)
        //        return IL_FALSE;

        //    Temp = CompData;
        //    for (z = 0; z < Depth; z++) {
        //        for (y = 0; y < Height; y += 4) {
        //            for (x = 0; x < Width; x += 4) {
        //                if (y >= Height || x >= Width)
        //                    break;
        //                alphas[0] = Temp[0];
        //                alphas[1] = Temp[1];
        //                alphamask = Temp + 2;
        //                Temp += 8;
        //                color_0 = ((Color565*)Temp);
        //                color_1 = ((Color565*)(Temp+2));
        //                bitmask = ((ILuint*)Temp)[1];
        //                Temp += 8;

        //                colours[0].r = color_0->nRed << 3;
        //                colours[0].g = color_0->nGreen << 2;
        //                colours[0].b = color_0->nBlue << 3;
        //                colours[0].a = 0xFF;

        //                colours[1].r = color_1->nRed << 3;
        //                colours[1].g = color_1->nGreen << 2;
        //                colours[1].b = color_1->nBlue << 3;
        //                colours[1].a = 0xFF;

        //                // Four-color block: derive the other two colors.    
        //                // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
        //                // These 2-bit codes correspond to the 2-bit fields 
        //                // stored in the 64-bit block.
        //                colours[2].b = (2 * colours[0].b + colours[1].b + 1) / 3;
        //                colours[2].g = (2 * colours[0].g + colours[1].g + 1) / 3;
        //                colours[2].r = (2 * colours[0].r + colours[1].r + 1) / 3;
        //                colours[2].a = 0xFF;

        //                colours[3].b = (colours[0].b + 2 * colours[1].b + 1) / 3;
        //                colours[3].g = (colours[0].g + 2 * colours[1].g + 1) / 3;
        //                colours[3].r = (colours[0].r + 2 * colours[1].r + 1) / 3;
        //                colours[3].a = 0xFF;

        //                k = 0;
        //                for (j = 0; j < 4; j++) {
        //                    for (i = 0; i < 4; i++, k++) {

        //                        Select = (bitmask & (0x03 << k*2)) >> k*2;
        //                        col = &colours[Select];

        //                        // only put pixels out < width or height
        //                        if (((x + i) < Width) && ((y + j) < Height)) {
        //                            Offset = z * Image->SizeOfPlane + (y + j) * Image->Bps + (x + i) * Image->Bpp;
        //                            Image->Data[Offset + 0] = col->r;
        //                            Image->Data[Offset + 1] = col->g;
        //                            Image->Data[Offset + 2] = col->b;
        //                        }
        //                    }
        //                }

        //                // 8-alpha or 6-alpha block?    
        //                if (alphas[0] > alphas[1]) {    
        //                    // 8-alpha block:  derive the other six alphas.    
        //                    // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
        //                    alphas[2] = (6 * alphas[0] + 1 * alphas[1] + 3) / 7;	// bit code 010
        //                    alphas[3] = (5 * alphas[0] + 2 * alphas[1] + 3) / 7;	// bit code 011
        //                    alphas[4] = (4 * alphas[0] + 3 * alphas[1] + 3) / 7;	// bit code 100
        //                    alphas[5] = (3 * alphas[0] + 4 * alphas[1] + 3) / 7;	// bit code 101
        //                    alphas[6] = (2 * alphas[0] + 5 * alphas[1] + 3) / 7;	// bit code 110
        //                    alphas[7] = (1 * alphas[0] + 6 * alphas[1] + 3) / 7;	// bit code 111
        //                }
        //                else {
        //                    // 6-alpha block.
        //                    // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
        //                    alphas[2] = (4 * alphas[0] + 1 * alphas[1] + 2) / 5;	// Bit code 010
        //                    alphas[3] = (3 * alphas[0] + 2 * alphas[1] + 2) / 5;	// Bit code 011
        //                    alphas[4] = (2 * alphas[0] + 3 * alphas[1] + 2) / 5;	// Bit code 100
        //                    alphas[5] = (1 * alphas[0] + 4 * alphas[1] + 2) / 5;	// Bit code 101
        //                    alphas[6] = 0x00;										// Bit code 110
        //                    alphas[7] = 0xFF;										// Bit code 111
        //                }

        //                // Note: Have to separate the next two loops,
        //                //	it operates on a 6-byte system.
        //                // First three bytes
        //                bits = *((ILint*)alphamask);
        //                for (j = 0; j < 2; j++) {
        //                    for (i = 0; i < 4; i++) {
        //                        // only put pixels out < width or height
        //                        if (((x + i) < Width) && ((y + j) < Height)) {
        //                            Offset = z * Image->SizeOfPlane + (y + j) * Image->Bps + (x + i) * Image->Bpp + 0;
        //                            Image->Data[Offset] = alphas[bits & 0x07];
        //                        }
        //                        bits >>= 3;
        //                    }
        //                }

        //                // Last three bytes
        //                bits = *((ILint*)&alphamask[3]);
        //                for (j = 2; j < 4; j++) {
        //                    for (i = 0; i < 4; i++) {
        //                        // only put pixels out < width or height
        //                        if (((x + i) < Width) && ((y + j) < Height)) {
        //                            Offset = z * Image->SizeOfPlane + (y + j) * Image->Bps + (x + i) * Image->Bpp + 0;
        //                            Image->Data[Offset] = alphas[bits & 0x07];
        //                        }
        //                        bits >>= 3;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return IL_TRUE;
        //}
        //        #endregion

        #region Float
        private void DecompressFloat()
        {
            uint i, j, Size;

            switch (this.CompFormat)
            {
                case PixelFormat.R32F:  // Red float, green = blue = max
                    Size = this.width * this.height * 4;
                    for (i = 0, j = 0; i < Size; i += 4, j += 4)
                    {
                        rawidata[i] = (byte)((BitConverter.ToSingle(compdata, (int)j)) * 255);
                        rawidata[i + 1] = 255;
                        rawidata[i + 2] = 255;
                        rawidata[i + 3] = 255;
                    }
                    return;

                case PixelFormat.A32B32G32R32F:  // Direct copy of float RGBA data
                    Size = this.width * this.height * 4;
                    for (i = 0, j = 0; i < Size; i += 4, j += 16)
                    {
                        rawidata[i] = (byte)((BitConverter.ToSingle(compdata, (int)j)) * 255);
                        rawidata[i + 1] = (byte)((BitConverter.ToSingle(compdata, (int)j + 4)) * 255);
                        rawidata[i + 2] = (byte)((BitConverter.ToSingle(compdata, (int)j + 8)) * 255); ;
                        rawidata[i + 3] = (byte)((BitConverter.ToSingle(compdata, (int)j + 12)) * 255); ;
                    }

                    return;
                case PixelFormat.G32R32F:  // Red double, green float, blue = max
                    Size = this.width * this.height * 4;
                    for (i = 0, j = 0; i < Size; i += 4, j += 8)
                    {
                        rawidata[i] = (byte)((BitConverter.ToSingle(compdata, (int)j)) * 255);
                        rawidata[i + 1] = (byte)((BitConverter.ToSingle(compdata, (int)j + 4)) * 255);
                        rawidata[i + 2] = 255;
                        rawidata[i + 3] = 255;
                    }
                    return;

                case PixelFormat.R16F:  // Red float, green = blue = max
                    iConvR16ToFloat32();
                    return;

                case PixelFormat.A16B16G16R16F:  // Just convert from half to float.
                    iConvFloat16ToFloat32();
                    return;

                case PixelFormat.G16R16F:  // Convert from half to float, set blue = max.
                    iConvG16R16ToFloat32();
                    return;

                default:
                    throw new Exception("Unknown Float Type!");
            }
        }

        private float[] compFloatData;

        private void iConvFloat16ToFloat32()
        {
            uint flen = (uint)Math.Ceiling((double)(compdata.Length / 2));
            compFloatData = new float[flen];
            for (uint i = 0, cloc = 0; i < flen; i++, cloc += 2)
            {
                compFloatData[i] = (float)Half.ToHalf(compdata, (int)cloc);
            }

            DecompressFloatData();
        }

        private void DecompressFloatData()
        {
            uint Size = this.width * this.height * 4;
            for (uint i = 0, j = 0; i < Size; i += 4, j += 4)
            {
                rawidata[i] = (byte)((compFloatData[j]) * 255);
                rawidata[i + 1] = (byte)((compFloatData[j + 1]) * 255);
                rawidata[i + 2] = (byte)((compFloatData[j + 2]) * 255);
                rawidata[i + 3] = (byte)((compFloatData[j + 3]) * 255);
            }
            compFloatData = null;
        }

        private void iConvG16R16ToFloat32()
        {
            uint flen = (uint)Math.Ceiling((double)(compdata.Length / 2));
            compFloatData = new float[flen];
            for (uint i = 0, cloc = 0; i < flen; i += 4, cloc += 4)
            {
                compFloatData[i] = (float)Half.ToHalf(compdata, (int)cloc);
                compFloatData[i + 1] = (float)Half.ToHalf(compdata, (int)cloc + 2);
                compFloatData[i + 2] = 1.0f;
                compFloatData[i + 3] = 1.0f;
            }

            DecompressFloatData();
        }

        private void iConvR16ToFloat32()
        {
            uint flen = (uint)Math.Ceiling((double)(compdata.Length / 2));
            compFloatData = new float[flen];
            for (uint i = 0, cloc = 0; i < flen; i += 4, cloc += 2)
            {
                compFloatData[i] = (float)Half.ToHalf(compdata, (int)cloc);
                compFloatData[i + 1] = 1.0f;
                compFloatData[i + 2] = 1.0f;
                compFloatData[i + 3] = 1.0f;
            }

            DecompressFloatData();
        }
        #endregion

        #region A16B16G16R16
        private void DecompressA16B16G16R16()
        {
            uint curloc = 0;
            for (uint i = 0; i < compdata.Length; i += 8)
            {
                rawidata[curloc] = compdata[i + 0];
                rawidata[curloc + 1] = compdata[i + 2];
                rawidata[curloc + 2] = compdata[i + 4];
                rawidata[curloc + 3] = compdata[i + 6];

                curloc += 4;
            }
        }
        #endregion

        #region ARGB
        private void DecompressARGB()
        {
            if (CompFormat == PixelFormat.LUMINANCE)
            {
                byte ReadI = 0;
                uint i, curloc = 0;
                this.rawidata = new byte[this.compdata.Length * 4];
                for (i = 0; i < this.compdata.Length; i += 1)
                {
                    ReadI = compdata[i];
                    this.rawidata[curloc] = ReadI;
                    this.rawidata[curloc + 1] = ReadI;
                    this.rawidata[curloc + 2] = ReadI;
                    this.rawidata[curloc + 3] = 0xff;
                    curloc += 4;
                }
                return;
            }
            else if (CompFormat == PixelFormat.LUMINANCE_ALPHA)
            {
                if (this.bpp == 2)
                {
                    byte ReadI = 0;
                    uint i, curloc = 0;
                    for (i = 0; i < this.compdata.Length; i += 2)
                    {
                        ReadI = compdata[i];
                        this.rawidata[curloc] = (byte)ReadI;
                        this.rawidata[curloc + 1] = (byte)ReadI;
                        this.rawidata[curloc + 2] = (byte)ReadI;
                        this.rawidata[curloc + 3] = compdata[i + 1];
                        curloc += 4;
                    }
                }
                else if (this.bpp == 1)
                {
                    byte ReadI = 0;
                    uint i, curloc = 0;
                    for (i = 0; i < this.compdata.Length; i++)
                    {
                        ReadI = (byte)((compdata[i] & 15) << 4);
                        this.rawidata[curloc] = ReadI;
                        this.rawidata[curloc + 1] = ReadI;
                        this.rawidata[curloc + 2] = ReadI;
                        this.rawidata[curloc + 3] = (byte)(compdata[i] & 240);
                        curloc += 4;
                    }
                }
                else
                {
                    throw new Exception("Unknown Luminance-Alpha Type!");
                }
                return;
            }
            else
            {
                uint ReadI = 0, TempBpp, i;
                byte RedL, RedR;
                byte GreenL, GreenR;
                byte BlueL, BlueR;
                byte AlphaL, AlphaR;
                uint curloc = 0;

                if (Has16BitComponents)
                {
                    DecompressARGB16();
                    return;
                }


                GetBitsFromMask(rbitmask, out RedL, out RedR);
                GetBitsFromMask(gbitmask, out GreenL, out GreenR);
                GetBitsFromMask(bbitmask, out BlueL, out BlueR);
                GetBitsFromMask(alphabitmask, out AlphaL, out AlphaR);
                TempBpp = rgbbitcount / 8;

                for (i = 0; i < this.compdata.Length; i += this.bpp)
                {
                    if (this.bpp == 4)
                    {
                        ReadI = compdata[i] | (uint)(compdata[i + 1] << 8) | (uint)(compdata[i + 2] << 16) | (uint)(compdata[i + 3] << 24);
                    }
                    else if (this.bpp == 3)
                    {
                        ReadI = compdata[i] | (uint)(compdata[i + 1] << 8) | (uint)(compdata[i + 2] << 16);
                    }
                    else if (this.bpp == 2)
                    {
                        ReadI = compdata[i] | (uint)(compdata[i + 1] << 8);
                    }
                    else if (this.bpp == 1)
                    {
                        ReadI = compdata[i];
                    }
                    else
                    {
                        throw new Exception("Un-recognized bit depth!");
                    }


                    this.rawidata[curloc] = (byte)(((ReadI & rbitmask) >> RedR) << RedL);
                    this.rawidata[curloc + 1] = (byte)(((ReadI & gbitmask) >> GreenR) << GreenL);
                    this.rawidata[curloc + 2] = (byte)(((ReadI & bbitmask) >> BlueR) << BlueL);

                    if (this.alphabitmask > 0)
                    {
                        this.rawidata[curloc + 3] = (byte)(((ReadI & alphabitmask) >> AlphaR) << AlphaL);
                        if (AlphaL >= 7)
                        {
                            this.rawidata[curloc + 3] = (byte)(this.rawidata[curloc + 3] > 0 ? 0xFF : 0x00);
                        }
                        else if (AlphaL >= 4)
                        {
                            this.rawidata[curloc + 3] = (byte)(this.rawidata[curloc + 3] | (this.rawidata[curloc + 3] >> 4));
                        }
                    }
                    else
                    {
                        this.rawidata[curloc + 3] = 0xff;
                    }

                    curloc += 4;
                }
            }
        }
        #endregion

        #region ARGB16
        private void DecompressARGB16()
        {
            uint ReadI = 0, i;
            byte RedL, RedR;
            byte GreenL, GreenR;
            byte BlueL, BlueR;
            byte AlphaL, AlphaR;
            uint curloc = 0;


            GetBitsFromMask(rbitmask, out RedL, out RedR);
            GetBitsFromMask(gbitmask, out GreenL, out GreenR);
            GetBitsFromMask(bbitmask, out BlueL, out BlueR);
            GetBitsFromMask(alphabitmask, out AlphaL, out AlphaR);
            RedL = (byte)(RedL + (16 - CountBitsFromMask(rbitmask)));
            GreenL = (byte)(GreenL + (16 - CountBitsFromMask(gbitmask)));
            BlueL = (byte)(BlueL + (16 - CountBitsFromMask(bbitmask)));
            AlphaL = (byte)(AlphaL + (16 - CountBitsFromMask(alphabitmask)));


            for (i = 0; i < this.compdata.Length; i += this.bpp)
            {
                if (this.bpp == 4)
                {
                    ReadI = compdata[i] | (uint)(compdata[i + 1] << 8) | (uint)(compdata[i + 2] << 16) | (uint)(compdata[i + 3] << 24);
                }
                else if (this.bpp == 3)
                {
                    ReadI = compdata[i] | (uint)(compdata[i + 1] << 8) | (uint)(compdata[i + 2] << 16);
                }
                else if (this.bpp == 2)
                {
                    ReadI = compdata[i] | (uint)(compdata[i + 1] << 8);
                }
                else if (this.bpp == 1)
                {
                    ReadI = compdata[i];
                }
                else
                {
                    throw new Exception("Un-recognized bit depth!");
                }


                this.rawidata[curloc + 2] = (byte)((ReadI & rbitmask) >> (RedR + 2));// << RedL);
                this.rawidata[curloc + 1] = (byte)((ReadI & gbitmask) >> (GreenR + 2));// << GreenL);
                this.rawidata[curloc] = (byte)((ReadI & bbitmask) >> (BlueR + 2));// << BlueL);

                if (this.alphabitmask > 0)
                {
                    this.rawidata[curloc + 3] = (byte)(((ReadI & alphabitmask) >> AlphaR) << 6);
                    if (AlphaL >= 7)
                    {
                        this.rawidata[curloc + 3] = (byte)(this.rawidata[curloc + 3] > 0 ? 0xFF : 0x00);
                    }
                    else if (AlphaL >= 4)
                    {
                        this.rawidata[curloc + 3] = (byte)(this.rawidata[curloc + 3] | (this.rawidata[curloc + 3] >> 4));
                    }
                }
                else
                {
                    this.rawidata[curloc + 3] = 0xff;
                }

                //throw new Exception();
                curloc += 4;

            }
        }
        #endregion

        #region Dxt1
        private void DecompressDXT1()
        {
            // DXT1 decompressor
            Pixel[] colours = new Pixel[4];
            ushort colour0, colour1;
            uint bitmask, offset;
            int i, j, k, x, y, z, Select;


            MemoryStream mem = new MemoryStream(this.compdata.Length);
            mem.Write(this.compdata, 0, this.compdata.Length);
            mem.Seek(0, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(mem);

            colours[0].A = 255;
            colours[1].A = 255;
            colours[2].A = 255;

            for (z = 0; z < this.depth; z++)
            {
                for (y = 0; y < this.height; y += 4)
                {
                    for (x = 0; x < this.width; x += 4)
                    {
                        colour0 = r.ReadUInt16();
                        colour1 = r.ReadUInt16();

                        this.ReadColour(colour0, ref colours[0]);
                        this.ReadColour(colour1, ref colours[1]);

                        bitmask = r.ReadUInt32();

                        if (colour0 > colour1)
                        {
                            // Four-color block: derive the other two colors.
                            // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                            // These 2-bit codes correspond to the 2-bit fields
                            // stored in the 64-bit block.
                            colours[2].B = (byte)((2 * colours[0].B + colours[1].B + 1) / 3);
                            colours[2].G = (byte)((2 * colours[0].G + colours[1].G + 1) / 3);
                            colours[2].R = (byte)((2 * colours[0].R + colours[1].R + 1) / 3);

                            colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                            colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                            colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);
                            colours[3].A = 0xFF;
                        }
                        else
                        {
                            // Three-color block: derive the other color.
                            // 00 = color_0,  01 = color_1,  10 = color_2,
                            // 11 = transparent.
                            // These 2-bit codes correspond to the 2-bit fields 
                            // stored in the 64-bit block. 
                            colours[2].B = (byte)((colours[0].B + colours[1].B) / 2);
                            colours[2].G = (byte)((colours[0].G + colours[1].G) / 2);
                            colours[2].R = (byte)((colours[0].R + colours[1].R) / 2);

                            colours[3].B = 0;
                            colours[3].G = 0;
                            colours[3].R = 0;
                            colours[3].A = 0;
                        }


                        for (j = 0, k = 0; j < 4; j++)
                        {
                            for (i = 0; i < 4; i++, k++)
                            {
                                Select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    offset = (uint)(z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp);
                                    this.rawidata[offset] = (byte)colours[Select].R;
                                    this.rawidata[offset + 1] = (byte)colours[Select].G;
                                    this.rawidata[offset + 2] = (byte)colours[Select].B;
                                    this.rawidata[offset + 3] = (byte)colours[Select].A;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Dxt3
        private void DecompressDXT3()
        {
            Pixel[] colours = new Pixel[4];
            uint bitmask, offset;
            int i, j, k, x, y, z, Select;
            ushort word, colour0, colour1;
            byte[] alpha; //temp;

            MemoryStream mem = new MemoryStream(this.compdata.Length);
            mem.Write(this.compdata, 0, this.compdata.Length);
            mem.Seek(0, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(mem);

            for (z = 0; z < this.depth; z++)
            {
                for (y = 0; y < this.height; y += 4)
                {
                    for (x = 0; x < this.width; x += 4)
                    {
                        alpha = r.ReadBytes(8);

                        colour0 = r.ReadUInt16();
                        colour1 = r.ReadUInt16();
                        this.ReadColour(colour0, ref colours[0]);
                        this.ReadColour(colour1, ref colours[1]);

                        bitmask = r.ReadUInt32();

                        colours[2].B = (byte)((2 * colours[0].B + colours[1].B + 1) / 3);
                        colours[2].G = (byte)((2 * colours[0].G + colours[1].G + 1) / 3);
                        colours[2].R = (byte)((2 * colours[0].R + colours[1].R + 1) / 3);

                        colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                        colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                        colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);

                        for (j = 0, k = 0; j < 4; j++)
                        {
                            for (i = 0; i < 4; k++, i++)
                            {
                                Select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);

                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    offset = (uint)(z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp);
                                    this.rawidata[offset] = (byte)colours[Select].R;
                                    this.rawidata[offset + 1] = (byte)colours[Select].G;
                                    this.rawidata[offset + 2] = (byte)colours[Select].B;
                                }
                            }
                        }

                        for (j = 0; j < 4; j++)
                        {
                            word = (ushort)(alpha[2 * j] + 256 * alpha[2 * j + 1]);
                            for (i = 0; i < 4; i++)
                            {
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    offset = (uint)(z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp + 3);
                                    this.rawidata[offset] = (byte)(word & 0x0F);
                                    this.rawidata[offset] = (byte)(this.rawidata[offset] | (this.rawidata[offset] << 4));
                                }
                                word >>= 4;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Dxt5
        private void DecompressDXT5()
        {
            uint x, y, z, i, j, Select, bitmask, Offset, bits;
            Pixel[] Colors = new Pixel[4];
            byte[] Alphas = new byte[8];
            byte[] alphaMask = new byte[6];
            int k;

            MemoryStream mem = new MemoryStream(this.compdata.Length);
            mem.Write(this.compdata, 0, this.compdata.Length);
            mem.Seek(0, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(mem);
            for (z = 0; z < this.depth; z++)
            {
                for (y = 0; y < this.height; y += 4)
                {
                    for (x = 0; x < this.width; x += 4)
                    {
                        if (y >= this.height || x >= this.width)
                            break;
                        Alphas[0] = r.ReadByte();
                        Alphas[1] = r.ReadByte();
                        alphaMask = r.ReadBytes(6);

                        ReadColour(r.ReadUInt16(), ref Colors[0]);
                        ReadColour(r.ReadUInt16(), ref Colors[1]);
                        bitmask = r.ReadUInt32();

                        // Four-color block: derive the other two colors.    
                        // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                        // These 2-bit codes correspond to the 2-bit fields 
                        // stored in the 64-bit block.
                        Colors[2].B = (byte)((2 * Colors[0].B + Colors[1].B + 1) / 3);
                        Colors[2].G = (byte)((2 * Colors[0].G + Colors[1].G + 1) / 3);
                        Colors[2].R = (byte)((2 * Colors[0].R + Colors[1].R + 1) / 3);

                        Colors[3].B = (byte)((Colors[0].B + 2 * Colors[1].B + 1) / 3);
                        Colors[3].G = (byte)((Colors[0].G + 2 * Colors[1].G + 1) / 3);
                        Colors[3].R = (byte)((Colors[0].R + 2 * Colors[1].R + 1) / 3);

                        k = 0;
                        for (j = 0; j < 4; j++)
                        {
                            for (i = 0; i < 4; i++, k++)
                            {

                                Select = (uint)((bitmask & (0x03 << k * 2)) >> k * 2);

                                // only put pixels out < width or height
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    Offset = z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp;
                                    this.rawidata[Offset + 0] = Colors[Select].R;
                                    this.rawidata[Offset + 1] = Colors[Select].G;
                                    this.rawidata[Offset + 2] = Colors[Select].B;
                                }
                            }
                        }

                        // 8-alpha or 6-alpha block?    
                        if (Alphas[0] > Alphas[1])
                        {
                            // 8-alpha block:  derive the other six alphas.    
                            // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                            Alphas[2] = (byte)((6 * Alphas[0] + 1 * Alphas[1] + 3) / 7);	// bit code 010
                            Alphas[3] = (byte)((5 * Alphas[0] + 2 * Alphas[1] + 3) / 7);	// bit code 011
                            Alphas[4] = (byte)((4 * Alphas[0] + 3 * Alphas[1] + 3) / 7);	// bit code 100
                            Alphas[5] = (byte)((3 * Alphas[0] + 4 * Alphas[1] + 3) / 7);	// bit code 101
                            Alphas[6] = (byte)((2 * Alphas[0] + 5 * Alphas[1] + 3) / 7);	// bit code 110
                            Alphas[7] = (byte)((1 * Alphas[0] + 6 * Alphas[1] + 3) / 7);	// bit code 111
                        }
                        else
                        {
                            // 6-alpha block.
                            // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                            Alphas[2] = (byte)((4 * Alphas[0] + 1 * Alphas[1] + 2) / 5);	// Bit code 010
                            Alphas[3] = (byte)((3 * Alphas[0] + 2 * Alphas[1] + 2) / 5);	// Bit code 011
                            Alphas[4] = (byte)((2 * Alphas[0] + 3 * Alphas[1] + 2) / 5);	// Bit code 100
                            Alphas[5] = (byte)((1 * Alphas[0] + 4 * Alphas[1] + 2) / 5);	// Bit code 101
                            Alphas[6] = 0x00;										// Bit code 110
                            Alphas[7] = 0xFF;										// Bit code 111
                        }

                        // Note: Have to separate the next two loops,
                        //	it operates on a 6-byte system.

                        // First three bytes
                        //bits = *((ILint*)alphamask);
                        bits = (uint)(alphaMask[0]) | (uint)(alphaMask[1] << 8) | (uint)(alphaMask[2] << 16);
                        for (j = 0; j < 2; j++)
                        {
                            for (i = 0; i < 4; i++)
                            {
                                // only put pixels out < width or height
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    Offset = z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp + 3;
                                    this.rawidata[Offset] = Alphas[bits & 0x07];
                                }
                                bits >>= 3;
                            }
                        }

                        // Last three bytes
                        //bits = *((ILint*)&alphamask[3]);
                        bits = (uint)(alphaMask[3]) | (uint)(alphaMask[4] << 8) | (uint)(alphaMask[5] << 16);
                        for (j = 2; j < 4; j++)
                        {
                            for (i = 0; i < 4; i++)
                            {
                                // only put pixels out < width or height
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    Offset = z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp + 3;
                                    this.rawidata[Offset] = Alphas[bits & 0x07];
                                }
                                bits >>= 3;
                            }
                        }
                    }
                }
            }

        }
        #endregion


        private void ReadColour(ushort Data, ref Pixel op)
        {
            byte r, g, b;

            b = (byte)(Data & 0x1f);
            g = (byte)((Data & 0x7E0) >> 5);
            r = (byte)((Data & 0xF800) >> 11);

            op.R = (byte)(r * 255 / 31);
            op.G = (byte)(g * 255 / 63);
            op.B = (byte)(b * 255 / 31);
        }
    }
}

#endregion
