
// Please note, everything below this
// point was originally from a plugin 
// for Paint.Net, the plugin is available here:
// http://forums.getpaint.net/index.php?/topic/17202-pnm-file-type-plugin/
//
//
// The source has been modified for use in this library.
// 
// This disclaimer was last
// modified on August 9, 2011.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using ShaniSoft.IO.DataWriter;
using ShaniSoft.IO.DataReader;
using ShaniSoft.Drawing.PnmReader;
using ShaniSoft.Drawing.PnmWriter;

namespace ShaniSoft
{

    #region Drawing
    namespace Drawing
    {

        #region Pnm
        public class Pnm
        {
            public static PnmType DetectType(Image im)
            {
                bool flag = true;
                for (int i = 0; i < im.Height; i++)
                {
                    for (int j = 0; j < im.Width; j++)
                    {
                        Color pixel = ((Bitmap)im).GetPixel(j, i);
                        if ((pixel.R != pixel.G) || (pixel.R != pixel.B))
                        {
                            return PnmType.Ppm;
                        }
                        if (flag && ((((pixel.R > 0) || (pixel.R < 0xff)) || ((pixel.G > 0) || (pixel.G < 0xff))) || ((pixel.B > 0) || (pixel.B < 0xff))))
                        {
                            flag = false;
                        }
                    }
                }
                if (!flag)
                {
                    return PnmType.Pgm;
                }
                return PnmType.Pbm;
            }

            public static Image ReadPnm(Stream inputStream)
            {
                char ch;
                string str;
                string magicWord = ((char)inputStream.ReadByte()).ToString() + ((char)inputStream.ReadByte()).ToString();
                inputStream.ReadByte();
                PnmEncoding pNMEncoding = PnmFactory.GetPNMEncoding(magicWord);
                IPnmDataReader iPNMDataReader = PnmFactory.GetIPNMDataReader(inputStream, pNMEncoding);
                if (iPNMDataReader == null)
                {
                    throw new Exception("Currently only Binary and ASCII encoding is supported");
                }
                IPnmReader iPNMReader = PnmFactory.GetIPNMReader(PnmFactory.GetPNMType(magicWord));
                if (iPNMReader == null)
                {
                    throw new Exception("Currently only PBM, PGM and PNM Image Types are supported");
                }
                do
                {
                    str = iPNMDataReader.ReadLine();
                    if (str.Length == 0)
                    {
                        ch = '#';
                    }
                    else
                    {
                        ch = str.Substring(0, 1).ToCharArray(0, 1)[0];
                    }
                }
                while (ch == '#');
                string[] strArray = str.Split(new char[] { ' ' });
                int width = int.Parse(strArray[0]);
                int height = int.Parse(strArray[1]);
                if (magicWord != "P1")
                {
                    do
                    {
                        str = iPNMDataReader.ReadLine();
                        if (str.Length == 0)
                        {
                            ch = '#';
                        }
                        else
                        {
                            ch = str.Substring(0, 1).ToCharArray(0, 1)[0];
                        }
                    }
                    while (ch == '#');
                    if (int.Parse(str) != 0xff)
                    {
                        Console.WriteLine("Warning, max value for pixels in this image is not 255");
                    }
                }
                return iPNMReader.ReadImageData(iPNMDataReader, width, height);
            }

            public static void WritePnm(Stream fs, Image im, PnmEncoding encoding, PnmType ptype)
            {
                IPnmDataWriter iPNMDataWriter = PnmFactory.GetIPNMDataWriter(fs, encoding);
                if (iPNMDataWriter == null)
                {
                    throw new Exception("Currently only Binary and ASCII encoding is supported");
                }
                try
                {
                    iPNMDataWriter.WriteLine(PnmFactory.GetMagicWord(ptype, encoding));
                    iPNMDataWriter.WriteLine(im.Width.ToString() + " " + im.Height.ToString());
                    if (ptype != PnmType.Pbm)
                    {
                        iPNMDataWriter.WriteLine("255");
                    }
                    PnmFactory.GetIPNMWriter(ptype).WriteImageData(iPNMDataWriter, im);
                }
                catch
                {
                    throw;
                }
            }
        }
        #endregion

        #region PnmFactory
        internal class PnmFactory
        {
            internal static IPnmDataReader GetIPNMDataReader(Stream fs, PnmEncoding encoding)
            {
                switch (encoding)
                {
                    case PnmEncoding.BinaryEncoding:
                        return new BinaryDataReader(fs);

                    case PnmEncoding.ASCIIEncoding:
                        return new ASCIIDataReader(fs);
                }
                return null;
            }

            internal static IPnmDataWriter GetIPNMDataWriter(Stream fs, PnmEncoding encoding)
            {
                switch (encoding)
                {
                    case PnmEncoding.BinaryEncoding:
                        return new BinaryDataWriter(fs);

                    case PnmEncoding.ASCIIEncoding:
                        return new ASCIIDataWriter(fs);
                }
                return null;
            }

            internal static IPnmReader GetIPNMReader(PnmType ptype)
            {
                switch (ptype)
                {
                    case PnmType.Pbm:
                        return new PbmReader();

                    case PnmType.Pgm:
                        return new PgmReader();

                    case PnmType.Ppm:
                        return new PpmReader();
                }
                return null;
            }

            internal static IPnmWriter GetIPNMWriter(PnmType ptype)
            {
                switch (ptype)
                {
                    case PnmType.Pbm:
                        return new PbmWriter();

                    case PnmType.Pgm:
                        return new PgmWriter();

                    case PnmType.Ppm:
                        return new PpmWriter();
                }
                return null;
            }

            internal static string GetMagicWord(PnmType ptype, PnmEncoding encoding)
            {
                if ((ptype == PnmType.Pgm) && (encoding == PnmEncoding.ASCIIEncoding))
                {
                    return "P2";
                }
                if ((ptype != PnmType.Pgm) || (encoding != PnmEncoding.BinaryEncoding))
                {
                    if ((ptype == PnmType.Ppm) && (encoding == PnmEncoding.ASCIIEncoding))
                    {
                        return "P3";
                    }
                    if ((ptype == PnmType.Ppm) && (encoding == PnmEncoding.BinaryEncoding))
                    {
                        return "P6";
                    }
                    if ((ptype == PnmType.Pbm) && (encoding == PnmEncoding.ASCIIEncoding))
                    {
                        return "P1";
                    }
                    if ((ptype == PnmType.Pbm) && (encoding == PnmEncoding.BinaryEncoding))
                    {
                        throw new Exception("PBM files are only written in ASCII encoding.");
                    }
                }
                return "P5";
            }

            internal static string GetMaxPixel(PnmType ptype)
            {
                if (ptype == PnmType.Pbm)
                {
                    return "-1";
                }
                return "255";
            }

            internal static PnmEncoding GetPNMEncoding(string MagicWord)
            {
                switch (MagicWord)
                {
                    case "P1":
                    case "P2":
                    case "P3":
                        return PnmEncoding.ASCIIEncoding;

                    case "P5":
                    case "P6":
                        return PnmEncoding.BinaryEncoding;
                }
                return PnmEncoding.BinaryEncoding;
            }

            internal static PnmType GetPNMType(string MagicWord)
            {
                switch (MagicWord)
                {
                    case "P1":
                        return PnmType.Pbm;

                    case "P2":
                    case "P5":
                        return PnmType.Pgm;

                    case "P6":
                    case "P3":
                        return PnmType.Ppm;
                }
                return PnmType.Pgm;
            }
        }
        #endregion

        #region PnmType
        public enum PnmType
        {
            Pbm,
            Pgm,
            Ppm
        }
        #endregion

        #region PnmEncoding
        public enum PnmEncoding
        {
            BinaryEncoding,
            ASCIIEncoding
        }
        #endregion


        #region PnmReader
        namespace PnmReader
        {

            #region IPnmReader
            internal abstract class IPnmReader
            {
                public abstract Image ReadImageData(IPnmDataReader dr, int width, int height);
            }
            #endregion

            #region PbmReader
            internal class PbmReader : IPnmReader
            {
                public override Image ReadImageData(IPnmDataReader dr, int width, int height)
                {
                    Image image;
                    try
                    {
                        int count = (dr is ASCIIDataReader) ? width : ((int)Math.Ceiling((double)(((double)width) / 8.0)));
                        Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        System.Drawing.Imaging.BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
                        palette.Entries[0] = Color.Black;
                        palette.Entries[1] = Color.White;
                        bitmap.Palette = palette;
                        byte[] data = new byte[height * bitmapdata.Stride];
                        for (int i = 0; i < height; i++)
                        {
                            dr.Read(data, i * bitmapdata.Stride, count);
                        }
                        System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapdata.Scan0, data.Length);
                        bitmap.UnlockBits(bitmapdata);
                        image = bitmap;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        dr.Close();
                    }
                    return image;
                }
            }
            #endregion

            #region PgmReader
            internal class PgmReader : IPnmReader
            {
                public override Image ReadImageData(IPnmDataReader dr, int width, int height)
                {
                    Image image;
                    try
                    {
                        Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        System.Drawing.Imaging.BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
                        for (int i = 0; i < palette.Entries.Length; i++)
                        {
                            palette.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        bitmap.Palette = palette;
                        byte[] data = new byte[height * bitmapdata.Stride];
                        for (int j = 0; j < height; j++)
                        {
                            dr.Read(data, j * bitmapdata.Stride, width);
                        }
                        System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapdata.Scan0, data.Length);
                        bitmap.UnlockBits(bitmapdata);
                        image = bitmap;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        dr.Close();
                    }
                    return image;
                }
            }
            #endregion

            #region PpmReader
            internal class PpmReader : IPnmReader
            {
                public override Image ReadImageData(IPnmDataReader dr, int width, int height)
                {
                    Image image;
                    try
                    {
                        Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        System.Drawing.Imaging.BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        byte[] data = new byte[height * bitmapdata.Stride];
                        for (int i = 0; i < height; i++)
                        {
                            dr.Read(data, i * bitmapdata.Stride, width * 3);
                        }
                        System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapdata.Scan0, data.Length);
                        bitmap.UnlockBits(bitmapdata);
                        image = bitmap;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        dr.Close();
                    }
                    return image;
                }
            }
            #endregion

        }
        #endregion

        #region PnmWriter
        namespace PnmWriter
        {

            #region IPnmWriter
            internal abstract class IPnmWriter
            {
                public abstract void WriteImageData(IPnmDataWriter dw, Image im);
            }
            #endregion

            #region PbmWriter
            internal class PbmWriter : IPnmWriter
            {
                public override void WriteImageData(IPnmDataWriter dw, Image im)
                {
                    int num = 0;
                    for (int i = 0; i < im.Height; i++)
                    {
                        for (int j = 0; j < im.Width; j++)
                        {
                            Color pixel = ((Bitmap)im).GetPixel(j, i);
                            int num4 = (pixel.R + pixel.G) + pixel.B;
                            if (num4 > 0)
                            {
                                dw.WriteByte(1);
                            }
                            else
                            {
                                dw.WriteByte(0);
                            }
                            num++;
                            if ((dw is ASCIIDataWriter) && (num >= 0x22))
                            {
                                num = 0;
                                dw.WriteLine(string.Empty);
                            }
                        }
                    }
                    dw.Close();
                }
            }
            #endregion

            #region PgmWriter
            internal class PgmWriter : IPnmWriter
            {
                public override void WriteImageData(IPnmDataWriter dw, Image im)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < im.Height; i++)
                    {
                        for (int j = 0; j < im.Width; j++)
                        {
                            Color pixel = ((Bitmap)im).GetPixel(j, i);
                            int num3 = (int)(((pixel.R * 0.3) + (pixel.G * 0.59)) + (pixel.B * 0.11));
                            if (dw is ASCIIDataWriter)
                            {
                                string str = num3.ToString();
                                if ((builder.Length + str.Length) >= 70)
                                {
                                    dw.WriteLine(string.Empty);
                                    builder.Remove(0, builder.Length);
                                }
                                else
                                {
                                    builder.Append(str);
                                }
                            }
                            dw.WriteByte((byte)num3);
                        }
                    }
                    dw.Close();
                }
            }
            #endregion

            #region PpmWriter
            internal class PpmWriter : IPnmWriter
            {
                public override void WriteImageData(IPnmDataWriter dw, Image im)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < im.Height; i++)
                    {
                        for (int j = 0; j < im.Width; j++)
                        {
                            Color pixel = ((Bitmap)im).GetPixel(j, i);
                            if (dw is ASCIIDataWriter)
                            {
                                string str = string.Concat(new object[] { pixel.R, " ", pixel.G, " ", pixel.B, " " });
                                if ((builder.Length + str.Length) >= 70)
                                {
                                    dw.WriteLine(string.Empty);
                                    builder.Remove(0, builder.Length);
                                }
                                else
                                {
                                    builder.Append(str);
                                }
                            }
                            dw.WriteByte(pixel.R);
                            dw.WriteByte(pixel.G);
                            dw.WriteByte(pixel.B);
                        }
                    }
                    dw.Close();
                }
            }
            #endregion

        }
        #endregion

    }
    #endregion

    #region IO
    namespace IO
    {

        #region DataReader
        namespace DataReader
        {

            #region ASCIIDataReader
            internal class ASCIIDataReader : IPnmDataReader
            {
                private StreamReader sr;
                private System.Collections.Queue toks = new System.Collections.Queue();
                private char[] WhiteSpaces = new char[] { ' ', '\t', '\r', '\n' };

                public ASCIIDataReader(Stream fs)
                {
                    this.sr = new StreamReader(fs, Encoding.ASCII);
                }

                public override void Close()
                {
                }

                public override int Read(byte[] data, int index, int count)
                {
                    int num = 0;
                    try
                    {
                        while (num < count)
                        {
                            data[index + num] = this.ReadByte();
                            num++;
                        }
                    }
                    catch
                    {
                    }
                    return num;
                }

                public override byte ReadByte()
                {
                    if ((this.toks != null) && (this.toks.Count > 0))
                    {
                        return byte.Parse(this.toks.Dequeue().ToString());
                    }
                    string str = this.sr.ReadLine();
                    if (str == null)
                    {
                        throw new Exception("Unexpected end of file");
                    }
                    foreach (string str2 in str.Split(this.WhiteSpaces))
                    {
                        if (str2.Length > 0)
                        {
                            this.toks.Enqueue(str2);
                        }
                    }
                    return this.ReadByte();
                }

                public override string ReadLine()
                {
                    return this.sr.ReadLine();
                }
            }
            #endregion

            #region BinaryDataReader
            internal class BinaryDataReader : IPnmDataReader
            {
                private BinaryReader br;
                private char EndLine = '\n';

                public BinaryDataReader(Stream fs)
                {
                    this.br = new BinaryReader(fs);
                }

                public override void Close()
                {
                }

                public override int Read(byte[] data, int start, int length)
                {
                    return this.br.Read(data, start, length);
                }

                public override byte ReadByte()
                {
                    return this.br.ReadByte();
                }

                public override string ReadLine()
                {
                    StringBuilder builder = new StringBuilder();
                    for (byte i = this.br.ReadByte(); i != this.EndLine; i = this.br.ReadByte())
                    {
                        builder.Append(((char)i).ToString());
                    }
                    return builder.ToString();
                }
            }
            #endregion

            #region IPnmDataReader
            internal abstract class IPnmDataReader
            {
                public abstract void Close();
                public abstract int Read(byte[] data, int index, int count);
                public abstract byte ReadByte();
                public abstract string ReadLine();
            }
            #endregion

        }
        #endregion

        #region DataWriter
        namespace DataWriter
        {

            #region ASCIIDataWriter
            internal class ASCIIDataWriter : IPnmDataWriter
            {
                private StreamWriter sw;

                public ASCIIDataWriter(Stream fs)
                {
                    this.sw = new StreamWriter(fs);
                }

                public override void Close()
                {
                    this.sw.Flush();
                }

                public override void WriteByte(byte data)
                {
                    this.sw.Write(data.ToString() + " ");
                }

                public override void WriteLine(string line)
                {
                    this.sw.WriteLine(line);
                }
            }
            #endregion

            #region BinaryDataWriter
            internal class BinaryDataWriter : IPnmDataWriter
            {
                private BinaryWriter bw;

                public BinaryDataWriter(Stream fs)
                {
                    this.bw = new BinaryWriter(fs);
                }

                public override void Close()
                {
                    this.bw.Flush();
                }

                public override void WriteByte(byte data)
                {
                    this.bw.Write(data);
                }

                public override void WriteLine(string line)
                {
                    this.bw.Write(Encoding.ASCII.GetBytes(line + "\n"));
                }
            }
            #endregion

            #region IPnmDataWriter
            internal abstract class IPnmDataWriter
            {
                public abstract void Close();
                public abstract void WriteByte(byte data);
                public abstract void WriteLine(string line);
            }
            #endregion

        }
        #endregion

    }
    #endregion

}
