
// Please note, everything below this
// point was originally available here:
// http://www.modthesims.info/showthread.php?t=361153
//
//
// The source has been modified for use in this library.
// 
// This disclaimer was last
// modified on August 9, 2011.


using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SaveGameEditor
{
    public static class DDS
    {
        private static byte[] Expand5 = new byte[0x20];
        private static byte[] Expand6 = new byte[0x40];
        private static int nIterPower = 4;
        private static byte[,] OMatch5 = new byte[0x100, 2];
        private static byte[,] OMatch6 = new byte[0x100, 2];
        private static readonly int[] prods = new int[] { 0x90000, 0x900, 0x40102, 0x10402 };
        private static byte[] QuantGTab = new byte[0x110];
        private static byte[] QuantRBTab = new byte[0x110];
        private static bool sInitted;
        private static readonly int[] w1Tab = new int[] { 3, 0, 2, 1 };

        private static void CompressAlphaBlock(BinaryWriter dest, Pixel[] block, int quality)
        {
            int num2;
            int num = num2 = block[0].a;
            for (int i = 1; i < 0x10; i++)
            {
                num = Math.Min(num, block[i].a);
                num2 = Math.Max(num2, block[i].a);
            }
            dest.Write((byte) num2);
            dest.Write((byte) num);
            int num4 = num2 - num;
            int num5 = (num * 7) - (num4 >> 1);
            int num6 = num4 * 4;
            int num7 = num4 * 2;
            int num8 = 0;
            int num9 = 0;
            for (int j = 0; j < 0x10; j++)
            {
                int num11 = (block[j].a * 7) - num5;
                int num13 = (num6 - num11) >> 0x1f;
                int num12 = num13 & 4;
                num11 -= num6 & num13;
                num13 = (num7 - num11) >> 0x1f;
                num12 += num13 & 2;
                num11 -= num7 & num13;
                num13 = (num4 - num11) >> 0x1f;
                num12 += num13 & 1;
                num12 = -num12 & 7;
                num12 ^= (2 > num12) ? 1 : 0;
                num9 |= num12 << num8;
                num8 += 3;
                if (num8 >= 8)
                {
                    dest.Write((byte) num9);
                    num9 = num9 >> 8;
                    num8 -= 8;
                }
            }
        }

        private static void CompressColorBlock(BinaryWriter dest, Pixel[] block, int quality, Pixel[] tmppixels16, Pixel[] tmppixels4)
        {
            uint num2;
            ushort num4;
            ushort num5;
            uint num6;
            Pixel[] pixelArray = tmppixels16;
            Pixel[] color = tmppixels4;
            uint num = num2 = block[0].v;
            for (int i = 1; i < 0x10; i++)
            {
                num = Math.Min(num, block[i].v);
                num2 = Math.Max(num2, block[i].v);
            }
            if (num != num2)
            {
                if (quality > 0)
                {
                    DitherBlock(pixelArray, block);
                }
                OptimizeColorsBlock((quality > 0) ? pixelArray : block, out num5, out num4);
                if (num5 != num4)
                {
                    EvalColors(color, num5, num4);
                    num6 = MatchColorsBlock(block, color, quality != 0);
                }
                else
                {
                    num6 = 0;
                }
                if (RefineBlock((quality > 0) ? pixelArray : block, ref num5, ref num4, num6))
                {
                    if (num5 != num4)
                    {
                        EvalColors(color, num5, num4);
                        num6 = MatchColorsBlock(block, color, quality != 0);
                    }
                    else
                    {
                        num6 = 0;
                    }
                }
            }
            else
            {
                int r = block[0].r;
                int g = block[0].g;
                int b = block[0].b;
                num6 = 0xaaaaaaaa;
                num5 = (ushort) (((OMatch5[r, 0] << 11) | (OMatch6[g, 0] << 5)) | OMatch5[b, 0]);
                num4 = (ushort) (((OMatch5[r, 1] << 11) | (OMatch6[g, 1] << 5)) | OMatch5[b, 1]);
            }
            if (num5 < num4)
            {
                num5 = (ushort) (num5 ^ num4);
                num4 = (ushort) (num4 ^ num5);
                num5 = (ushort) (num5 ^ num4);
                num6 ^= 0x55555555;
            }
            dest.Write(num5);
            dest.Write(num4);
            dest.Write(num6);
        }

        public static Image Decode(byte[] data, out string type)
        {
            type = null;
            if (data == null)
            {
                return null;
            }
            MemoryStream input = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(input);
            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "DDS ")
            {
                return null;
            }
            if (reader.ReadInt32() != 0x7c)
            {
                return null;
            }
            input.Position += 4L;
            int height = reader.ReadInt32();
            int width = reader.ReadInt32();
            input.Position += 8L;
            int mipmaps = reader.ReadInt32();
            input.Position += 0x2cL;
            if (reader.ReadInt32() != 0x20)
            {
                return null;
            }
            if ((reader.ReadInt32() & 4) == 0)
            {
                return null;
            }
            FourCC fourcc = (FourCC) reader.ReadUInt32();
            input.Position += 40L;
            string str = "";
            if ((mipmaps == 1) && ((width > 1) || (height > 1)))
            {
                str = str + ",nomipgen";
            }
            type = string.Format("DDS:{0}{1}", Encoding.ASCII.GetString(BitConverter.GetBytes((uint) fourcc)), str);
            return DecodeTextureData(input, fourcc, width, height, mipmaps, 0, false);
        }

        private static void DecodeATI1Texture(byte[] pixels, int poffset, int width, int height, byte[] data)
        {
            int[] numArray = new int[8];
            for (int i = 0; i < height; i += 4)
            {
                for (int j = 0; j < width; j += 4)
                {
                    int startIndex = (((j + 3) >> 2) + (((i + 3) >> 2) * ((width + 3) >> 2))) << 3;
                    ulong num4 = BitConverter.ToUInt64(data, startIndex);
                    numArray[0] = (int) (num4 & ((ulong) 0xffL));
                    num4 = num4 >> 8;
                    numArray[1] = (int) (num4 & ((ulong) 0xffL));
                    num4 = num4 >> 8;
                    if (numArray[0] > numArray[1])
                    {
                        for (int m = 1; m < 7; m++)
                        {
                            numArray[m + 1] = (((7 - m) * numArray[0]) + (m * numArray[1])) / 7;
                        }
                    }
                    else
                    {
                        for (int n = 1; n < 5; n++)
                        {
                            numArray[n + 1] = (((5 - n) * numArray[0]) + (n * numArray[1])) / 5;
                        }
                        numArray[6] = 0;
                        numArray[7] = 0xff;
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        if ((i + k) < height)
                        {
                            int index = (poffset + (j << 2)) + ((height - ((k + i) + 1)) * (width << 2));
                            int num9 = 0;
                            while (num9 < 4)
                            {
                                if ((j + num9) < width)
                                {
                                    byte num11;
                                    uint num10 = (uint) numArray[(int) ((IntPtr) (num4 & ((ulong) 7L)))];
                                    num4 = num4 >> 3;
                                    pixels[index + 2] = num11 = (byte) num10;
                                    pixels[index] = pixels[index + 1] = num11;
                                }
                                num9++;
                                index += 4;
                            }
                        }
                    }
                }
            }
        }

        private static void DecodeATI2Texture(byte[] pixels, int poffset, int width, int height, byte[] data)
        {
            int[] numArray = new int[8];
            int[] numArray2 = new int[8];
            for (int i = 0; i < height; i += 4)
            {
                for (int j = 0; j < width; j += 4)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        int startIndex = (((j + 3) >> 2) + (((i + 3) >> 2) * ((width + 3) >> 2))) << 4;
                        ulong num5 = BitConverter.ToUInt64(data, startIndex);
                        ulong num6 = BitConverter.ToUInt64(data, startIndex + 8);
                        numArray[0] = (int) (num5 & ((ulong) 0xffL));
                        num5 = num5 >> 8;
                        numArray[1] = (int) (num5 & ((ulong) 0xffL));
                        num5 = num5 >> 8;
                        if (numArray[0] > numArray[1])
                        {
                            for (int n = 1; n < 7; n++)
                            {
                                numArray[n + 1] = (((7 - n) * numArray[0]) + (n * numArray[1])) / 7;
                            }
                        }
                        else
                        {
                            for (int num8 = 1; num8 < 5; num8++)
                            {
                                numArray[num8 + 1] = (((5 - num8) * numArray[0]) + (num8 * numArray[1])) / 5;
                            }
                            numArray[6] = 0;
                            numArray[7] = 0xff;
                        }
                        numArray2[0] = (int) (num6 & ((ulong) 0xffL));
                        num6 = num6 >> 8;
                        numArray2[1] = (int) (num6 & ((ulong) 0xffL));
                        num6 = num6 >> 8;
                        if (numArray2[0] > numArray2[1])
                        {
                            for (int num9 = 1; num9 < 7; num9++)
                            {
                                numArray2[num9 + 1] = (((7 - num9) * numArray2[0]) + (num9 * numArray2[1])) / 7;
                            }
                        }
                        else
                        {
                            for (int num10 = 1; num10 < 5; num10++)
                            {
                                numArray2[num10 + 1] = (((5 - num10) * numArray2[0]) + (num10 * numArray2[1])) / 5;
                            }
                            numArray2[6] = 0;
                            numArray2[7] = 0xff;
                        }
                        for (int m = 0; m < 4; m++)
                        {
                            if ((i + m) < height)
                            {
                                int index = (poffset + (j << 2)) + ((height - ((m + i) + 1)) * (width << 2));
                                int num13 = 0;
                                while (num13 < 4)
                                {
                                    if ((j + num13) < width)
                                    {
                                        int num14 = numArray[(int) ((IntPtr) (num5 & ((ulong) 7L)))];
                                        num5 = num5 >> 3;
                                        int num15 = numArray2[(int) ((IntPtr) (num6 & ((ulong) 7L)))];
                                        num6 = num6 >> 3;
                                        double d = Math.Round((double) (Math.Sqrt((double) ((0x4000 - ((num15 - 0x7f) * (num15 - 0x7f))) - ((num14 - 0x7f) * (num14 - 0x7f)))) + 127.0));
                                        if (d > 255.0)
                                        {
                                            d = 255.0;
                                        }
                                        else if ((d < 128.0) || double.IsNaN(d))
                                        {
                                            d = 128.0;
                                        }
                                        pixels[index + 2] = (byte) num15;
                                        pixels[index + 1] = (byte) num14;
                                        pixels[index] = (byte) d;
                                    }
                                    num13++;
                                    index += 4;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DecodeDXT1Texture(byte[] pixels, int poffset, int width, int height, byte[] data)
        {
            uint[] numArray = new uint[4];
            for (int i = 0; i < height; i += 4)
            {
                for (int j = 0; j < width; j += 4)
                {
                    int startIndex = (((j + 3) >> 2) + (((i + 3) >> 2) * ((width + 3) >> 2))) << 3;
                    ulong num4 = BitConverter.ToUInt64(data, startIndex);
                    uint num5 = (uint) (num4 & ((ulong) 0xffffL));
                    num4 = num4 >> 0x10;
                    uint num6 = (uint) (num4 & ((ulong) 0xffffL));
                    num4 = num4 >> 0x10;
                    uint num7 = (uint) ((num5 & 0x1f) << 3);
                    num7 |= num7 >> 5;
                    uint num8 = (uint) (((num5 >> 5) & 0x3f) << 2);
                    num8 |= num8 >> 6;
                    uint num9 = (num5 >> 11) << 3;
                    num9 |= num9 >> 5;
                    uint num10 = (uint) ((num6 & 0x1f) << 3);
                    num10 |= num10 >> 5;
                    uint num11 = (uint) (((num6 >> 5) & 0x3f) << 2);
                    num11 |= num11 >> 6;
                    uint num12 = (num6 >> 11) << 3;
                    num12 |= num12 >> 5;
                    numArray[0] = ((num9 << 0x10) | (num8 << 8)) | num7;
                    numArray[1] = ((num12 << 0x10) | (num11 << 8)) | num10;
                    if (num5 > num6)
                    {
                        numArray[2] = ((uint) (((((2 * num9) + num12) / 3) << 0x10) | ((((2 * num8) + num11) / 3) << 8))) | (((2 * num7) + num10) / 3);
                        numArray[3] = ((uint) ((((num9 + (2 * num12)) / 3) << 0x10) | (((num8 + (2 * num11)) / 3) << 8))) | ((num7 + (2 * num10)) / 3);
                    }
                    else
                    {
                        numArray[2] = ((((num9 + num12) >> 1) << 0x10) | (((num8 + num11) >> 1) << 8)) | ((num7 + num10) >> 1);
                        numArray[3] = 510;
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        int index = (poffset + (j << 2)) + ((height - ((k + i) + 1)) * (width << 2));
                        int num15 = 0;
                        while (num15 < 4)
                        {
                            if ((j + num15) < width)
                            {
                                BitConverter.GetBytes(numArray[(int) ((IntPtr) (num4 & ((ulong) 3L)))]).CopyTo(pixels, index);
                            }
                            num4 = num4 >> 2;
                            num15++;
                            index += 4;
                        }
                    }
                }
            }
        }

        private static void DecodeDXT5AlphaBlock(int[] pixels, int poffset, byte[] data, int doffset, int[] alphavals)
        {
            ulong num = BitConverter.ToUInt64(data, doffset);
            alphavals[0] = (int) (num & ((ulong) 0xffL));
            num = num >> 8;
            alphavals[1] = (int) (num & ((ulong) 0xffL));
            num = num >> 8;
            if (alphavals[0] > alphavals[1])
            {
                for (int j = 1; j < 7; j++)
                {
                    alphavals[j + 1] = (((7 - j) * alphavals[0]) + (j * alphavals[1])) / 7;
                }
            }
            else
            {
                for (int k = 1; k < 5; k++)
                {
                    alphavals[k + 1] = (((5 - k) * alphavals[0]) + (k * alphavals[1])) / 5;
                }
                alphavals[6] = 0;
                alphavals[7] = 0xff;
            }
            for (int i = 0; i < 4; i++)
            {
                for (int m = 0; m < 4; m++)
                {
                    pixels[((i * 4) + m) + poffset] = alphavals[(int) ((IntPtr) (num & ((ulong) 7L)))];
                    num = num >> 3;
                }
            }
        }

        private static void DecodeDXT5Texture(byte[] pixels, int poffset, int width, int height, byte[] data)
        {
            uint[] numArray = new uint[8];
            uint[] numArray2 = new uint[4];
            for (int i = 0; i < height; i += 4)
            {
                for (int j = 0; j < width; j += 4)
                {
                    int startIndex = (((j + 3) >> 2) + (((i + 3) >> 2) * ((width + 3) >> 2))) << 4;
                    ulong num4 = BitConverter.ToUInt64(data, startIndex);
                    numArray[0] = (uint) (num4 & ((ulong) 0xffL));
                    num4 = num4 >> 8;
                    numArray[1] = (uint) (num4 & ((ulong) 0xffL));
                    num4 = num4 >> 8;
                    if (numArray[0] > numArray[1])
                    {
                        for (int m = 1; m < 7; m++)
                        {
                            numArray[m + 1] = (uint) ((((uint) (((7 - m) * numArray[0]) + (m * numArray[1]))) / 7) << 0x18);
                        }
                    }
                    else
                    {
                        for (int n = 1; n < 5; n++)
                        {
                            numArray[n + 1] = (uint) ((((uint) (((5 - n) * numArray[0]) + (n * numArray[1]))) / 5) << 0x18);
                        }
                        numArray[6] = 0;
                        numArray[7] = 0xff000000;
                    }
                    numArray[0] = numArray[0] << 0x18;
                    numArray[1] = numArray[1] << 0x18;
                    ulong num7 = BitConverter.ToUInt64(data, startIndex + 8);
                    uint num8 = (uint) (num7 & ((ulong) 0xffffL));
                    num7 = num7 >> 0x10;
                    uint num9 = (uint) (num7 & ((ulong) 0xffffL));
                    num7 = num7 >> 0x10;
                    uint num10 = (uint) ((num8 & 0x1f) << 3);
                    num10 |= num10 >> 5;
                    uint num11 = (uint) (((num8 >> 5) & 0x3f) << 2);
                    num11 |= num11 >> 6;
                    uint num12 = (num8 >> 11) << 3;
                    num12 |= num12 >> 5;
                    uint num13 = (uint) ((num9 & 0x1f) << 3);
                    num13 |= num13 >> 5;
                    uint num14 = (uint) (((num9 >> 5) & 0x3f) << 2);
                    num14 |= num14 >> 6;
                    uint num15 = (num9 >> 11) << 3;
                    num15 |= num15 >> 5;
                    numArray2[0] = ((num12 << 0x10) | (num11 << 8)) | num10;
                    numArray2[1] = ((num15 << 0x10) | (num14 << 8)) | num13;
                    numArray2[2] = ((uint) (((((2 * num12) + num15) / 3) << 0x10) | ((((2 * num11) + num14) / 3) << 8))) | (((2 * num10) + num13) / 3);
                    numArray2[3] = ((uint) ((((num12 + (2 * num15)) / 3) << 0x10) | (((num11 + (2 * num14)) / 3) << 8))) | ((num10 + (2 * num13)) / 3);
                    for (int k = 0; k < 4; k++)
                    {
                        int index = (poffset + (j << 2)) + ((height - ((k + i) + 1)) * (width << 2));
                        int num18 = 0;
                        while (num18 < 4)
                        {
                            if ((j + num18) < width)
                            {
                                uint num19 = numArray[(int) ((IntPtr) (num4 & ((ulong) 7L)))] | numArray2[(int) ((IntPtr) (num7 & ((ulong) 3L)))];
                                BitConverter.GetBytes(num19).CopyTo(pixels, index);
                            }
                            num4 = num4 >> 3;
                            num7 = num7 >> 2;
                            num18++;
                            index += 4;
                        }
                    }
                }
            }
        }

        private static Image DecodeTextureData(Stream file, FourCC fourcc, int width, int height, int mipmaps, int mipmapno, bool mipmapreversed)
        {
            uint num = GetMipMapOffset(fourcc, width, height, mipmaps, mipmapno, mipmapreversed);
            uint num2 = GetMipMapSize(fourcc, width, height, mipmaps, mipmapno);
            if ((num2 == 0) || (((num2 + num) + file.Position) > file.Length))
            {
                return null;
            }
            width = ((width + (1 << (mipmapno & 0x1f))) - 1) >> mipmapno;
            height = ((height + (1 << (mipmapno & 0x1f))) - 1) >> mipmapno;
            file.Seek((long) num, SeekOrigin.Current);
            byte[] buffer = new byte[num2];
            file.Read(buffer, 0, (int) num2);
            byte[] array = new byte[0x36 + ((width * height) * 4)];
            array[0] = 0x42;
            array[1] = 0x4d;
            BitConverter.GetBytes(array.Length).CopyTo(array, 2);
            array[10] = 0x36;
            array[14] = 40;
            BitConverter.GetBytes(width).CopyTo(array, 0x12);
            BitConverter.GetBytes(height).CopyTo(array, 0x16);
            array[0x1a] = 1;
            array[0x1c] = 0x20;
            BitConverter.GetBytes((int) ((width * height) * 4)).CopyTo(array, 0x22);
            switch (fourcc)
            {
                case FourCC.ATI2:
                    DecodeATI2Texture(array, 0x36, width, height, buffer);
                    break;

                case FourCC.DXT5:
                    DecodeDXT5Texture(array, 0x36, width, height, buffer);
                    break;

                case FourCC.ATI1:
                    DecodeATI1Texture(array, 0x36, width, height, buffer);
                    break;

                case FourCC.DXT1:
                    DecodeDXT1Texture(array, 0x36, width, height, buffer);
                    break;
            }
            MemoryStream stream = new MemoryStream(array);
            using (stream)
            {
                return new Bitmap(Image.FromStream(stream));
            }
        }

        private static void DitherBlock(Pixel[] dest, Pixel[] block)
        {
            int[] numArray = new int[8];
            int index = 0;
            int num2 = 4;
            for (int i = 0; i < 3; i++)
            {
                int num4 = 0;
                int num5 = 8;
                byte[] buffer = (i == 1) ? QuantGTab : QuantRBTab;
                for (int j = 0; j < numArray.Length; j++)
                {
                    numArray[j] = 0;
                }
                for (int k = 0; k < 4; k++)
                {
                    dest[num4].components[i] = buffer[(num5 + block[num4].components[i]) + (((3 * numArray[num2 + 1]) + (5 * numArray[num2])) >> 4)];
                    numArray[index] = block[num4].components[i] - dest[num4].components[i];
                    dest[num4 + 1].components[i] = buffer[(num5 + block[num4 + 1].components[i]) + (((((7 * numArray[index]) + (3 * numArray[num2 + 2])) + (5 * numArray[num2 + 1])) + numArray[num2]) >> 4)];
                    numArray[index + 1] = block[num4 + 1].components[i] - dest[num4 + 1].components[i];
                    dest[num4 + 2].components[i] = buffer[(num5 + block[num4 + 2].components[i]) + (((((7 * numArray[index + 1]) + (3 * numArray[num2 + 3])) + (5 * numArray[num2 + 2])) + numArray[num2 + 1]) >> 4)];
                    numArray[index + 2] = block[num4 + 2].components[i] - dest[num4 + 2].components[i];
                    dest[num4 + 3].components[i] = buffer[(num5 + block[num4 + 3].components[i]) + ((((7 * numArray[index + 2]) + (5 * numArray[num2 + 3])) + numArray[num2 + 2]) >> 4)];
                    numArray[index + 3] = block[num4 + 3].components[i] - dest[num4 + 3].components[i];
                    index = num2;
                    num2 = 4 - num2;
                    num4 += 4;
                }
            }
        }

        public static byte[] Encode(Image data, string type)
        {
            int quality = 1;
            FourCC encodertype = FourCC.DXT1;
            bool flag = true;
            foreach (string str in type.Substring(4).Split(new char[] { ',' }))
            {
                string str2 = str.ToLower();
                if (str2 == null)
                {
                    goto Label_0094;
                }
                if (!(str2 == "dxt1"))
                {
                    if (str2 == "dxt5")
                    {
                        goto Label_0084;
                    }
                    if (str2 == "nomipgen")
                    {
                        goto Label_008C;
                    }
                    if (str2 == "nodither")
                    {
                        goto Label_0090;
                    }
                    goto Label_0094;
                }
                encodertype = FourCC.DXT1;
                goto Label_00A6;
            Label_0084:
                encodertype = FourCC.DXT5;
                goto Label_00A6;
            Label_008C:
                flag = false;
                goto Label_00A6;
            Label_0090:
                quality = 0;
                goto Label_00A6;
            Label_0094:
                throw new Exception(string.Format("Bad DDS encoder parameter: {0}", str));
            Label_00A6:;
            }
            int num2 = 0;
            int num3 = 0;
            while ((((int) 1) << num2) < data.Width)
            {
                num2++;
            }
            while ((((int) 1) << num3) < data.Height)
            {
                num3++;
            }
            while ((((int) 1) << num2) > 0x400)
            {
                num2--;
            }
            while ((((int) 1) << num3) > 0x400)
            {
                num3--;
            }
            int num4 = 1;
            if (flag)
            {
                num4 = Math.Max((int) (num2 + 1), (int) (num3 + 1));
            }
            MemoryStream output = new MemoryStream();
            using (output)
            {
                BinaryWriter bw = new BinaryWriter(output);
                bw.Write((uint) 0x20534444);
                bw.Write((uint) 0x7c);
                bw.Write((uint) 0x61007);
                bw.Write((uint) (((uint) 1) << num3));
                bw.Write((uint) (((uint) 1) << num2));
                bw.Write((uint) (((((((int) 1) << num3) + 3) / 4) * (((((int) 1) << num2) + 3) / 4)) * ((encodertype == FourCC.DXT1) ? 8 : 0x10)));
                bw.Write((uint) 1);
                bw.Write(num4);
                for (int i = 0; i < 11; i++)
                {
                    bw.Write((uint) 0);
                }
                bw.Write((uint) 0x20);
                bw.Write((uint) 4);
                bw.Write((uint) encodertype);
                for (int j = 0; j < 5; j++)
                {
                    bw.Write((uint) 0);
                }
                bw.Write((uint) 0x401000);
                for (int k = 0; k < 4; k++)
                {
                    bw.Write((uint) 0);
                }
                sInitDXT();
                Bitmap image = new Bitmap(((int) 1) << num2, ((int) 1) << num3, PixelFormat.Format32bppArgb);
                using (image)
                {
                    Graphics graphics = Graphics.FromImage(image);
                    using (graphics)
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        for (int m = 0; m < num4; m++)
                        {
                            int num9;
                            int num10;
                            if (m > num2)
                            {
                                num9 = 1;
                            }
                            else
                            {
                                num9 = ((int) 1) << (num2 - m);
                            }
                            if (m > num3)
                            {
                                num10 = 1;
                            }
                            else
                            {
                                num10 = ((int) 1) << (num3 - m);
                            }
                            graphics.DrawImage(data, 0, 0, num9, num10);
                            EncodeDDS(bw, image, num9, num10, encodertype, quality);
                        }
                    }
                }
                return output.GetBuffer();
            }
        }

        private static void EncodeDDS(BinaryWriter bw, Bitmap bm, int width, int height, FourCC encodertype, int quality)
        {
            byte[] buffer;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapdata = bm.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                buffer = new byte[bitmapdata.Stride * bitmapdata.Height];
                Marshal.Copy(bitmapdata.Scan0, buffer, 0, buffer.Length);
            }
            finally
            {
                bm.UnlockBits(bitmapdata);
            }
            Pixel[] src = new Pixel[0x10];
            Pixel[] pixelArray2 = new Pixel[0x10];
            Pixel[] pixelArray3 = new Pixel[4];
            for (int i = 0; i < 0x10; i++)
            {
                src[i] = new Pixel();
                pixelArray2[i] = new Pixel();
                if (i < 4)
                {
                    pixelArray3[i] = new Pixel();
                }
            }
            for (int j = 0; j < height; j += 4)
            {
                for (int k = 0; k < width; k += 4)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        for (int n = 0; n < 4; n++)
                        {
                            int num6 = (k + n) & (width - 1);
                            int num7 = (j + m) & (height - 1);
                            Array.Copy(buffer, (bitmapdata.Stride * num7) + (4 * num6), src[(m * 4) + n].components, 0, 4);
                        }
                    }
                    sCompressDXTBlock(bw, src, encodertype == FourCC.DXT5, quality, pixelArray2, pixelArray3);
                }
            }
        }

        private static void EvalColors(Pixel[] color, ushort c0, ushort c1)
        {
            color[0].From16Bit(c0);
            color[1].From16Bit(c1);
            color[2].LerpRGB(color[0], color[1], 0x55);
            color[3].LerpRGB(color[0], color[1], 170);
        }

        private static uint GetMipMapOffset(FourCC fourcc, int width, int height, int mipmaps, int mipmapno, bool mipmapreversed)
        {
            uint num = 0;
            if (mipmapreversed)
            {
                for (int j = mipmaps - 1; j > mipmapno; j--)
                {
                    num += GetMipMapSize(fourcc, width, height, mipmaps, j);
                }
                return num;
            }
            for (int i = mipmapno; i > 0; i--)
            {
                num += GetMipMapSize(fourcc, width, height, mipmaps, i - 1);
            }
            return num;
        }

        private static uint GetMipMapSize(FourCC fourcc, int width, int height, int mipmaps, int mipmapno)
        {
            width = ((width + (1 << (mipmapno & 0x1f))) - 1) >> mipmapno;
            height = ((height + (1 << (mipmapno & 0x1f))) - 1) >> mipmapno;
            switch (fourcc)
            {
                case FourCC.ATI1:
                    return (uint) ((((width + 3) / 4) * ((height + 3) / 4)) * 8);

                case FourCC.DXT1:
                    return (uint) ((((width + 3) / 4) * ((height + 3) / 4)) * 8);

                case FourCC.ATI2:
                    return (uint) ((((width + 3) / 4) * ((height + 3) / 4)) * 0x10);

                case FourCC.DXT5:
                    return (uint) ((((width + 3) / 4) * ((height + 3) / 4)) * 0x10);
            }
            return 0;
        }

        private static uint MatchColorsBlock(Pixel[] block, Pixel[] color, bool dither)
        {
            uint num = 0;
            int num2 = color[0].r - color[1].r;
            int num3 = color[0].g - color[1].g;
            int num4 = color[0].b - color[1].b;
            int[] numArray = new int[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                numArray[i] = ((block[i].r * num2) + (block[i].g * num3)) + (block[i].b * num4);
            }
            int[] numArray2 = new int[4];
            for (int j = 0; j < 4; j++)
            {
                numArray2[j] = ((color[j].r * num2) + (color[j].g * num3)) + (color[j].b * num4);
            }
            int num7 = (numArray2[1] + numArray2[3]) >> 1;
            int num8 = (numArray2[3] + numArray2[2]) >> 1;
            int num9 = (numArray2[2] + numArray2[0]) >> 1;
            if (!dither)
            {
                for (int n = 15; n >= 0; n--)
                {
                    num = num << 2;
                    int num11 = numArray[n];
                    if (num11 < num8)
                    {
                        num |= (uint)((num11 < num7) ? 1 : 3);
                    }
                    else
                    {
                        num |= (uint)((num11 < num9) ? 2 : 0);
                    }
                }
                return num;
            }
            int[] numArray3 = new int[8];
            int index = 0;
            int num13 = 4;
            int num14 = 0;
            num7 = num7 << 4;
            num8 = num8 << 4;
            num9 = num9 << 4;
            for (int k = 0; k < 8; k++)
            {
                numArray3[k] = 0;
            }
            for (int m = 0; m < 4; m++)
            {
                int num19;
                int num17 = (numArray[num14] << 4) + ((3 * numArray3[num13 + 1]) + (5 * numArray3[num13]));
                if (num17 < num8)
                {
                    num19 = (num17 < num7) ? 1 : 3;
                }
                else
                {
                    num19 = (num17 < num9) ? 2 : 0;
                }
                numArray3[index] = numArray[num14] - numArray2[num19];
                int num18 = num19;
                num17 = (numArray[num14 + 1] << 4) + ((((7 * numArray3[index]) + (3 * numArray3[num13 + 2])) + (5 * numArray3[num13 + 1])) + numArray3[num13]);
                if (num17 < num8)
                {
                    num19 = (num17 < num7) ? 1 : 3;
                }
                else
                {
                    num19 = (num17 < num9) ? 2 : 0;
                }
                numArray3[index + 1] = numArray[num14 + 1] - numArray2[num19];
                num18 |= num19 << 2;
                num17 = (numArray[num14 + 2] << 4) + ((((7 * numArray3[index + 1]) + (3 * numArray3[num13 + 3])) + (5 * numArray3[num13 + 2])) + numArray3[num13 + 1]);
                if (num17 < num8)
                {
                    num19 = (num17 < num7) ? 1 : 3;
                }
                else
                {
                    num19 = (num17 < num9) ? 2 : 0;
                }
                numArray3[index + 2] = numArray[num14 + 2] - numArray2[num19];
                num18 |= num19 << 4;
                num17 = (numArray[num14 + 3] << 4) + (((7 * numArray3[index + 2]) + (5 * numArray3[num13 + 3])) + numArray3[num13 + 2]);
                if (num17 < num8)
                {
                    num19 = (num17 < num7) ? 1 : 3;
                }
                else
                {
                    num19 = (num17 < num9) ? 2 : 0;
                }
                numArray3[index + 3] = numArray[num14 + 3] - numArray2[num19];
                num18 |= num19 << 6;
                index = num13;
                num13 = 4 - num13;
                num14 += 4;
                num |= (uint) (num18 << (m * 8));
            }
            return num;
        }

        private static int Mul8Bit(int a, int b)
        {
            int num = (a * b) + 0x80;
            return ((num + (num >> 8)) >> 8);
        }

        private static void OptimizeColorsBlock(Pixel[] block, out ushort max16, out ushort min16)
        {
            int num22;
            int num23;
            int num24;
            int[] numArray = new int[3];
            int[] numArray2 = new int[3];
            int[] numArray3 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                int num4;
                int num5;
                int index = 0;
                int num3 = num4 = num5 = block[index].components[i];
                for (int num6 = 0; num6 < 0x10; num6++)
                {
                    int num7 = block[index + num6].components[i];
                    num3 += num7;
                    num4 = Math.Min(num4, num7);
                    num5 = Math.Max(num5, num7);
                }
                numArray[i] = (num3 + 8) >> 4;
                numArray2[i] = num4;
                numArray3[i] = num5;
            }
            int[] numArray4 = new int[6];
            for (int j = 0; j < 6; j++)
            {
                numArray4[j] = 0;
            }
            for (int k = 0; k < 0x10; k++)
            {
                int num10 = block[k].r - numArray[2];
                int num11 = block[k].g - numArray[1];
                int num12 = block[k].b - numArray[0];
                numArray4[0] += num10 * num10;
                numArray4[1] += num10 * num11;
                numArray4[2] += num10 * num12;
                numArray4[3] += num11 * num11;
                numArray4[4] += num11 * num12;
                numArray4[5] += num12 * num12;
            }
            float[] numArray5 = new float[6];
            for (int m = 0; m < 6; m++)
            {
                numArray5[m] = ((float) numArray4[m]) / 255f;
            }
            float num13 = numArray3[2] - numArray2[2];
            float num14 = numArray3[1] - numArray2[1];
            float num15 = numArray3[0] - numArray2[0];
            for (int n = 0; n < nIterPower; n++)
            {
                float num18 = ((num13 * numArray5[0]) + (num14 * numArray5[1])) + (num15 * numArray5[2]);
                float num19 = ((num13 * numArray5[1]) + (num14 * numArray5[3])) + (num15 * numArray5[4]);
                float num20 = ((num13 * numArray5[2]) + (num14 * numArray5[4])) + (num15 * numArray5[5]);
                num13 = num18;
                num14 = num19;
                num15 = num20;
            }
            float num21 = Math.Max(Math.Max(Math.Abs(num13), Math.Abs(num14)), Math.Abs(num15));
            if (num21 < 4f)
            {
                num22 = 0x94;
                num23 = 300;
                num24 = 0x3a;
            }
            else
            {
                num21 = 512f / num21;
                num22 = (int) (num13 * num21);
                num23 = (int) (num14 * num21);
                num24 = (int) (num15 * num21);
            }
            int num25 = 0x7fffffff;
            int num26 = -2147483647;
            Pixel pixel = new Pixel();
            Pixel pixel2 = new Pixel();
            for (int num27 = 0; num27 < 0x10; num27++)
            {
                int num28 = ((block[num27].r * num22) + (block[num27].g * num23)) + (block[num27].b * num24);
                if (num28 < num25)
                {
                    num25 = num28;
                    pixel = block[num27];
                }
                if (num28 > num26)
                {
                    num26 = num28;
                    pixel2 = block[num27];
                }
            }
            max16 = pixel2.As16Bit();
            min16 = pixel.As16Bit();
        }

        private static void PrepareOptTable(byte[,] Table, byte[] expand, int size)
        {
            for (int i = 0; i < 0x100; i++)
            {
                int num2 = 0x100;
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        int num5 = expand[j];
                        int num6 = expand[k];
                        int num7 = (num6 + Mul8Bit(num5 - num6, 0x55)) - i;
                        if (num7 < 0)
                        {
                            num7 = -num7;
                        }
                        if (num7 < num2)
                        {
                            Table[i, 0] = (byte) k;
                            Table[i, 1] = (byte) j;
                            num2 = num7;
                        }
                    }
                }
            }
        }

        private static bool RefineBlock(Pixel[] block, ref ushort max16, ref ushort min16, uint mask)
        {
            int num3;
            int num4;
            int num6;
            int num7;
            int num = 0;
            uint num8 = mask;
            int num2 = num3 = num4 = 0;
            int num5 = num6 = num7 = 0;
            int index = 0;
            while (index < 0x10)
            {
                int num10 = ((int) num8) & 3;
                int num11 = w1Tab[num10];
                int r = block[index].r;
                int g = block[index].g;
                int b = block[index].b;
                num += prods[num10];
                num2 += num11 * r;
                num3 += num11 * g;
                num4 += num11 * b;
                num5 += r;
                num6 += g;
                num7 += b;
                index++;
                num8 = num8 >> 2;
            }
            num5 = (3 * num5) - num2;
            num6 = (3 * num6) - num3;
            num7 = (3 * num7) - num4;
            int num15 = num >> 0x10;
            int num16 = (num >> 8) & 0xff;
            int num17 = num & 0xff;
            if (((num16 == 0) || (num15 == 0)) || ((num15 * num16) == (num17 * num17)))
            {
                return false;
            }
            float num18 = 0.3647059f / ((float) ((num15 * num16) - (num17 * num17)));
            float num19 = (num18 * 63f) / 31f;
            ushort num20 = min16;
            ushort num21 = max16;
            max16 = (ushort) (((uint) Math.Min(Math.Max((float) ((((num2 * num16) - (num5 * num17)) * num18) + 0.5f), (float) 0f), 31f)) << 11);
            max16 = (ushort) (max16 | ((ushort) (((uint) Math.Min(Math.Max((float) ((((num3 * num16) - (num6 * num17)) * num19) + 0.5f), (float) 0f), 63f)) << 5)));
            max16 = (ushort) (max16 | ((ushort) ((uint) Math.Min(Math.Max((float) ((((num4 * num16) - (num7 * num17)) * num18) + 0.5f), (float) 0f), 31f))));
            min16 = (ushort) (((uint) Math.Min(Math.Max((float) ((((num5 * num15) - (num2 * num17)) * num18) + 0.5f), (float) 0f), 31f)) << 11);
            min16 = (ushort) (min16 | ((ushort) (((uint) Math.Min(Math.Max((float) ((((num6 * num15) - (num3 * num17)) * num19) + 0.5f), (float) 0f), 63f)) << 5)));
            min16 = (ushort) (min16 | ((ushort) ((uint) Math.Min(Math.Max((float) ((((num7 * num15) - (num4 * num17)) * num18) + 0.5f), (float) 0f), 31f))));
            if (num20 == min16)
            {
                return (num21 != max16);
            }
            return true;
        }

        private static void sCompressDXTBlock(BinaryWriter dest, Pixel[] src, bool alpha, int quality, Pixel[] tmppixels16, Pixel[] tmppixels4)
        {
            if (alpha)
            {
                CompressAlphaBlock(dest, src, quality);
            }
            CompressColorBlock(dest, src, quality, tmppixels16, tmppixels4);
        }

        private static void sInitDXT()
        {
            if (!sInitted)
            {
                sInitted = true;
                for (int i = 0; i < 0x20; i++)
                {
                    Expand5[i] = (byte) ((i << 3) | (i >> 2));
                }
                for (int j = 0; j < 0x40; j++)
                {
                    Expand6[j] = (byte) ((j << 2) | (j >> 4));
                }
                for (int k = 0; k < 0x110; k++)
                {
                    int a = Math.Min(Math.Max(k - 8, 0), 0xff);
                    QuantRBTab[k] = Expand5[Mul8Bit(a, 0x1f)];
                    QuantGTab[k] = Expand6[Mul8Bit(a, 0x3f)];
                }
                PrepareOptTable(OMatch5, Expand5, 0x20);
                PrepareOptTable(OMatch6, Expand6, 0x40);
            }
        }

        public enum FourCC
        {
            ATI1 = 0x31495441,
            ATI2 = 0x32495441,
            DDS = 0x20534444,
            DXT1 = 0x31545844,
            DXT2 = 0x32545844,
            DXT3 = 0x33545844,
            DXT4 = 0x34545844,
            DXT5 = 0x35545844
        }

        private class Pixel
        {
            public byte[] components = new byte[4];

            public ushort As16Bit()
            {
                return (ushort) (((DDS.Mul8Bit(this.r, 0x1f) << 11) + (DDS.Mul8Bit(this.g, 0x3f) << 5)) + DDS.Mul8Bit(this.b, 0x1f));
            }

            public void From16Bit(ushort v)
            {
                int index = (v & 0xf800) >> 11;
                int num2 = (v & 0x7e0) >> 5;
                int num3 = v & 0x1f;
                this.a = 0;
                this.r = DDS.Expand5[index];
                this.g = DDS.Expand6[num2];
                this.b = DDS.Expand5[num3];
            }

            public void LerpRGB(DDS.Pixel p1, DDS.Pixel p2, int f)
            {
                this.r = (byte) (p1.r + DDS.Mul8Bit(p2.r - p1.r, f));
                this.g = (byte) (p1.g + DDS.Mul8Bit(p2.g - p1.g, f));
                this.b = (byte) (p1.b + DDS.Mul8Bit(p2.b - p1.b, f));
            }

            public byte a
            {
                get
                {
                    return this.components[3];
                }
                set
                {
                    this.components[3] = value;
                }
            }

            public byte b
            {
                get
                {
                    return this.components[0];
                }
                set
                {
                    this.components[0] = value;
                }
            }

            public byte g
            {
                get
                {
                    return this.components[1];
                }
                set
                {
                    this.components[1] = value;
                }
            }

            public byte r
            {
                get
                {
                    return this.components[2];
                }
                set
                {
                    this.components[2] = value;
                }
            }

            public uint v
            {
                get
                {
                    return BitConverter.ToUInt32(this.components, 0);
                }
                set
                {
                    this.components = BitConverter.GetBytes(value);
                }
            }
        }
    }
}

