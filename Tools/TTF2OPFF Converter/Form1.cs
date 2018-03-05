//#define GZipCompression
//#define DeflateCompression

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;

namespace TTF2OPFF_Converter
{
    internal enum CompressionType
    {
        None,
        Deflate,
        LZMA,
        GZip,
        BZip2,
    }

    public partial class Form1 : Form
    {

        private class BinaryWriter : System.IO.BinaryWriter
        {
            private bool[] curByte = new bool[8];
            private byte curBitIndx = 0;
            private System.Collections.BitArray ba;

            public BinaryWriter(Stream s) : base(s) { }

            public override void Flush()
            {
                base.Write(ConvertToByte(curByte));
                base.Flush();
            }

            public override void Write(bool value)
            {
                curByte[curBitIndx] = value;
                curBitIndx++;

                if (curBitIndx == 8)
                {
                    base.Write(ConvertToByte(curByte));
                    this.curBitIndx = 0;
                    this.curByte = new bool[8];
                }
            }

            public override void Write(byte value)
            {
                ba = new BitArray(new byte[] { value });
                for (byte i = 0; i < 8; i++)
                {
                    this.Write(ba[i]);
                }
                ba = null;
            }

            public override void Write(byte[] buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    this.Write((byte)buffer[i]);
                }
            }

            public override void Write(uint value)
            {
                ba = new BitArray(BitConverter.GetBytes(value));
                for (byte i = 0; i < 32; i++)
                {
                    this.Write(ba[i]);
                }
                ba = null;
            }

            public override void Write(ulong value)
            {
                ba = new BitArray(BitConverter.GetBytes(value));
                for (byte i = 0; i < 64; i++)
                {
                    this.Write(ba[i]);
                }
                ba = null;
            }

            public override void Write(ushort value)
            {
                ba = new BitArray(BitConverter.GetBytes(value));
                for (byte i = 0; i < 16; i++)
                {
                    this.Write(ba[i]);
                }
                ba = null;
            }

            private static byte ConvertToByte(bool[] bools)
            {
                byte b = 0;

                byte bitIndex = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (bools[i])
                    {
                        b |= (byte)(((byte)1) << bitIndex);
                    }
                    bitIndex++;
                }

                return b;
            }
        }

        private string OutputFileName;
        private CompressionType CompressionMode = CompressionType.None;

        public Form1()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputFileName = saveFileDialog1.FileName;
                textBox1.Text = OutputFileName;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (FontFamily f in FontFamily.Families)
            {
                FontComboBox.Items.Add(f.Name);
            }
            FontComboBox.SelectedIndex = 0;
            CompressionComboBox.SelectedIndex = 0;
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (OutputFileName == null || OutputFileName == "")
            {
                MessageBox.Show("Invalid File Name!");
            }
            else
            {
                //FileStream str;
                FileStream final;
                dynamic strm;

                #region Already Exists
                if (File.Exists(OutputFileName))
                {
                    if (MessageBox.Show("A file at '" + OutputFileName + "' already exists! Would you like to overwrite it?", "File Already Exists", MessageBoxButtons.YesNoCancel) == System.Windows.Forms.DialogResult.Yes)
                    {

                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    File.Create(OutputFileName).Close();
                }
                #endregion

                #region Compression Config
                //if (CompressionMode == CompressionType.Deflate)
                //{
                //    str = new FileStream(OutputFileName, FileMode.Truncate);
                //    strm = new DeflateStream(str, System.IO.Compression.CompressionMode.Compress);
                //    final = null;
                //}
                //else if (CompressionMode == CompressionType.GZip)
                //{
                //    str = new FileStream(OutputFileName, FileMode.Truncate);
                //    strm = new GZipStream(str, System.IO.Compression.CompressionMode.Compress);
                //    final = null;
                //}
                if (CompressionMode == CompressionType.LZMA)
                {
                    if (File.Exists(OutputFileName + ".uncmpr"))
                    {
                        File.Delete(OutputFileName + ".uncmpr");
                    }
                    strm = File.Create(OutputFileName + ".uncmpr");
                    final = new FileStream(OutputFileName, FileMode.Truncate);
                }
                else
                {
                    strm = new FileStream(OutputFileName, FileMode.Truncate);
                    final = null;
                }
                #endregion

                BinaryWriter br = new BinaryWriter(strm);

                br.Write((byte)0);
                br.Write((byte)0);
                br.Write((byte)0);
                br.Write((byte)0);
                br.Write((byte)0); // Write the 8 empty bytes of the header
                br.Write((byte)0);
                br.Write((byte)0);
                br.Write((byte)0);

                byte[] buffer;
                br.Write((UInt16)47); // write the format version.


                string FontName = (string)FontComboBox.SelectedItem;

                buffer = ASCIIEncoding.ASCII.GetBytes(FontName);
                int i = 256 - buffer.Length;
                if (i < 0)
                {
                    throw new Exception("Font Name is to long!");
                }
                else
                {
                    br.Write(buffer);
                    for (int i2 = 0; i2 < i; i2++)
                    {
                        br.Write((byte)0); // Fill the rest of the 256 bytes.
                    }
                }

                System.Windows.Media.Typeface t = new System.Windows.Media.Typeface(FontName);
                System.Windows.Media.GlyphTypeface glyph;
                if (t.TryGetGlyphTypeface(out glyph))
                {
                    t = null;
                    IDictionary<int, ushort> charKeyMap = (IDictionary<int, ushort>)glyph.CharacterToGlyphMap;
                    glyph = null;
                    SortedSet<int> chars = new SortedSet<int>();
                    foreach (KeyValuePair<int, ushort> c in charKeyMap)
                    {
                        chars.Add(c.Key);
                    }
                    charKeyMap = null;
                    Font f = new Font(FontName, 128, GraphicsUnit.Pixel);

                    UInt64 charsToWrite = (ulong)chars.Count;// *16;
                    br.Write(charsToWrite); // Write the number of chars to read.

                    int prevChar = 0;

                    for (byte style = 0; style < 1; style++)
                    {
                        f = new Font(FontName, 64, (FontStyle)style, GraphicsUnit.Pixel);
                        foreach (int ch in chars)
                        {
                            Bitmap Backend = new Bitmap(1, 1);
                            Graphics g = Graphics.FromImage(Backend);
                            SizeF sz = g.MeasureString(new String(new char[] { (char)ch }), f);
                            byte height = (byte)Math.Ceiling(sz.Height + 2);
                            byte width = (byte)Math.Ceiling(sz.Width + 4);
                            Backend = new Bitmap(width, height);
                            g = Graphics.FromImage(Backend);
                            g.Clear(Color.White);

                            g.DrawString(new String(new char[] { (char)ch }), f, new SolidBrush(Color.Black), 2, 2);
                            g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
                            if (prevChar + 1 == ch)
                            {
                                br.Write((byte)255); // write that it's incremented from the previous char.
                            }
                            else
                            {
                                br.Write((byte)0); // write that it's not incremented from the previous char.
                                br.Write((UInt32)ch); // Write the char number.
                            }
                            pictureBox1.Image = Backend;
                            pictureBox1.Refresh();

                            br.Write((byte)style); // write it's style
                            br.Write((byte)height); // write the height
                            br.Write((byte)width); // write the width
                            buffer = ConvertToByteArray(Backend);
                            br.Write(buffer);
                            prevChar = ch;
                        }
                        strm.Flush();
                        System.GC.Collect();
                    }

                    if (CompressionMode == CompressionType.LZMA)
                    {
                        strm.Position = 0;
                        buffer = new byte[strm.Length];
                        strm.Read(buffer, 0, (Int32)strm.Length);
                        buffer = Orvid.Compression.LZMACoder.Compress(buffer);
                        final.WriteByte(255);
                        final.Write(buffer, 0, buffer.Length);
                        final.Flush();
                        final.Close();
                        final.Dispose();
                    }

                    strm.Flush();
                    strm.Close();
                    strm.Dispose();
                    if (CompressionMode == CompressionType.LZMA)
                    {
                        File.Delete(OutputFileName + ".uncmpr");
                    }
                    //pictureBox1.Image = null;

                    MessageBox.Show("Conversion Completed Successfully!");
                }
                else
                {
                    throw new Exception("Unable to load the glyph typeface!");
                }
            }
        }

        private byte[] ConvertToByteArray(Bitmap b)
        {
            MemoryStream m = new MemoryStream();
            BinaryWriter br = new BinaryWriter(m);
            int bitnum = 0;
            //Color White = Color.FromArgb(0, 0, 0);
            //Color Black = Color.FromArgb(255, 255, 255);
            Color curPix;
            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    curPix = b.GetPixel(x, y);
                    //if (curPix != White)
                    //{
                    //    if (curPix == Black)
                    //    {
                    //        br.Write(false);
                    //        bitnum++;
                    //        br.Write(false);
                    //        bitnum++;
                    //    }
                    //    else // Write that it's a greyscale pixel, and it's value.
                    //    {
                    //        br.Write(false);
                    //        bitnum++;
                    //        br.Write(true);
                    //        bitnum++;
                            br.Write(GetGreyscaleByte(curPix));
                            bitnum += 8;
                    //    }
                    //}
                    //else
                    //{
                    //    br.Write(true);
                    //    bitnum++;
                    //}
                }
            }

            byte[] tmp = BitConverter.GetBytes((UInt32)bitnum);
            byte[] arr2 = new byte[m.Length + 4];
            Array.Copy(tmp, arr2, 4); // write the number of bits
            tmp = m.GetBuffer();
            Array.Copy(tmp, 0, arr2, 4, m.Length);
            //throw new Exception();
            return arr2;
        }

        private byte GetGreyscaleByte(Color c)
        {
            return (byte)((0.2125 * c.R) + (0.7154 * c.G) + (0.0721 * c.B));
        }

        private void FontComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        //private static Orvid.Graphics.FontSupport.OPFF opfFont = null;

        private void button1_Click(object sender, EventArgs e)
        {
            //if (opfFont == null)
            //{
            //    FileStream str = new FileStream("Arial-Compressed.opff", FileMode.Open);
            //    byte[] data = new byte[str.Length];
            //    str.Read(data, 0, (int)str.Length);
            //    opfFont = new Orvid.Graphics.FontSupport.OPFF(data);
            //    data = null;
            //    str.Close();
            //    str.Dispose();
            //}
            //Orvid.Graphics.Image i = opfFont.GetCharacter(Int32.Parse(textBox2.Text), Orvid.Graphics.FontSupport.FontStyle.Normal);
            ////i = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width * 4, i.Height * 4));
            ////o.AntiAlias();
            ////o.HalveSize();
            ////o.HalveSize();
            //pictureBox1.Image = (Bitmap)i;
            //pictureBox1.Size = new Size(i.Width, i.Height);
            //pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            //pictureBox1.BringToFront();
            //System.GC.Collect();

            //Font f2 = new Font("Arial", 64, (FontStyle)0, GraphicsUnit.Pixel);
            //Bitmap Backend = new Bitmap(i.Width, i.Height);
            //Graphics g = Graphics.FromImage(Backend);
            //g.Clear(Color.White);
            //g.DrawString(new String(new char[] { (char)Int32.Parse(textBox2.Text) }), f2, new SolidBrush(Color.Black), 2, 2);
            //g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
            //pictureBox2.Image = Backend;
            //pictureBox2.Size = new Size(i.Width, i.Height);
            //f2 = null;
            //g = null;
            //i = null;
            //System.GC.Collect();
        }

        private void CompressionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CompressionMode = (CompressionType)Enum.Parse(typeof(CompressionType), (String)CompressionComboBox.Items[CompressionComboBox.SelectedIndex]);
        }

        private void CompressionComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }


        bool thso = false;
        bool t2 = false;
        bool t3 = false;
        private void TestButton2_Click(object sender, EventArgs e)
        {
            //Bitmap b = (Bitmap)Bitmap.FromFile(Path.GetFullPath("IMGP4154.JPG"));
            //FileStream s = new FileStream(Path.GetFullPath("test2.oif"), FileMode.OpenOrCreate);
            //Orvid.Graphics.Image i = new Orvid.Graphics.Image(b.Width, b.Height);
            //for (uint x = 0; x < i.Width; x++)
            //{
            //    for (uint y = 0; y < i.Height; y++)
            //    {
            //        i.SetPixel(x, y, b.GetPixel((int)x, (int)y));
            //    }
            //}
            //Orvid.Graphics.ImageFormats.OIFImage oif = new Orvid.Graphics.ImageFormats.OIFImage();
            //oif.Save(i, s);
            //s.Flush();
            //s.Close();

            if (!thso)
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.png"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PngImage p = new Orvid.Graphics.ImageFormats.PngImage();
                Orvid.Graphics.Image i = p.Load(s);
                s.Close();
                s.Dispose();
                Bitmap b = new Bitmap(i.Width, i.Height);
                for (uint x = 0; x < i.Width; x++)
                {
                    for (uint y = 0; y < i.Height; y++)
                    {
                        b.SetPixel((int)x, (int)y, i.GetPixel(x, y));
                    }
                }
                pictureBox1.Image = b;
                thso = true;
            }
            else
            {
                if (!t2)
                {
                    Bitmap b = (Bitmap)pictureBox1.Image;
                    Orvid.Graphics.Image i = new Orvid.Graphics.Image(b.Width, b.Height);
                    for (uint x = 0; x < i.Width; x++)
                    {
                        for (uint y = 0; y < i.Height; y++)
                        {
                            i.SetPixel(x, y, b.GetPixel((int)x, (int)y));
                        }
                    }
                    i = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(b.Width / 4, b.Height / 4), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.Bicubic);
                    Bitmap b2 = new Bitmap(i.Width, i.Height);
                    for (uint x = 0; x < i.Width; x++)
                    {
                        for (uint y = 0; y < i.Height; y++)
                        {
                            b2.SetPixel((int)x, (int)y, i.GetPixel(x, y));
                        }
                    }
                    pictureBox2.Image = b2;
                    pictureBox2.Size = new Size(b2.Width, b2.Height);
                    t2 = true;
                }
                else
                {
                    if (!t3)
                    {
                        Bitmap b = (Bitmap)pictureBox1.Image;
                        Orvid.Graphics.Image i = new Orvid.Graphics.Image(b.Width, b.Height);
                        for (uint x = 0; x < i.Width; x++)
                        {
                            for (uint y = 0; y < i.Height; y++)
                            {
                                i.SetPixel(x, y, b.GetPixel((int)x, (int)y));
                            }
                        }
                        i = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(b.Width / 4, b.Height / 4), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.Bilinear);
                        Bitmap b2 = new Bitmap(i.Width, i.Height);
                        for (uint x = 0; x < i.Width; x++)
                        {
                            for (uint y = 0; y < i.Height; y++)
                            {
                                b2.SetPixel((int)x, (int)y, i.GetPixel(x, y));
                            }
                        }
                        pictureBox2.Image = b2;
                        pictureBox2.Size = new Size(b2.Width, b2.Height);
                        t3 = true;
                    }
                    else
                    {

                        Bitmap b = (Bitmap)pictureBox1.Image;
                        Orvid.Graphics.Image i = new Orvid.Graphics.Image(b.Width, b.Height);
                        for (uint x = 0; x < i.Width; x++)
                        {
                            for (uint y = 0; y < i.Height; y++)
                            {
                                i.SetPixel(x, y, b.GetPixel((int)x, (int)y));
                            }
                        }
                        i = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(b.Width / 2, b.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.NearestNeighbor);
                        Bitmap b2 = new Bitmap(i.Width, i.Height);
                        for (uint x = 0; x < i.Width; x++)
                        {
                            for (uint y = 0; y < i.Height; y++)
                            {
                                b2.SetPixel((int)x, (int)y, i.GetPixel(x, y));
                            }
                        }
                        pictureBox2.Image = b2;
                        pictureBox2.Size = new Size(b2.Width, b2.Height);
                        t2 = false;
                        t3 = false;
                    }
                }
            }

            //Font f = new Font("Chiller", 64, (FontStyle)0, GraphicsUnit.Pixel);
            //Bitmap b = new Bitmap(1, 1);
            //Graphics g = Graphics.FromImage(b);
            //SizeF sz = g.MeasureString(new String(new char[] { (char)6 }), f);
            //byte height = (byte)Math.Ceiling(sz.Height + 2);
            //byte width = (byte)Math.Ceiling(sz.Width + 4);
            //b = new Bitmap(width, height);
            //g = Graphics.FromImage(b);
            //g.Clear(Color.White);
            //g.DrawString(new String(new char[] { (char)Int32.Parse(textBox2.Text) }), f, new SolidBrush(Color.Black), 2, 2);
            //g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
            //pictureBox1.Image = b;
            //pictureBox1.Size = new Size(b.Width, b.Height);

            //Orvid.Graphics.Image i = new Orvid.Graphics.Image(b.Width, b.Height);
            //for (uint x = 0; x < i.Width; x++)
            //{
            //    for (uint y = 0; y < i.Height; y++)
            //    {
            //        i.SetPixel(x, y, b.GetPixel((int)x, (int)y));
            //    }
            //}
            //FileStream s = new FileStream("Test.oif", FileMode.OpenOrCreate);
            //Orvid.Graphics.ImageFormats.OIFImage t = new Orvid.Graphics.ImageFormats.OIFImage();
            //t.Save(i, s);
            //s.Flush();
            //s.Close();
            //s.Dispose();

            //s = new FileStream("Test.oif", FileMode.Open);
            //Orvid.Graphics.Image i2 = t.Load(s);
            //Bitmap b2 = new Bitmap(i2.Width, i2.Height);
            //for (uint x = 0; x < i2.Width; x++)
            //{
            //    for (uint y = 0; y < i2.Height; y++)
            //    {
            //        b2.SetPixel((int)x, (int)y, i2.GetPixel(x, y));
            //    }
            //}
            //pictureBox2.Image = b2;
            //pictureBox2.Size = new Size(b2.Width, b2.Height);
            //s.Close();
            //s.Dispose();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Return)
            {
                e.Handled = true;
                button1_Click(button1, new EventArgs());
            }
        }

    }
}
