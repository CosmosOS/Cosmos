//#define GZipCompression
//#define DeflateCompression

using System;
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

                strm.WriteByte(0);
                strm.WriteByte(0);
                strm.WriteByte(0);
                strm.WriteByte(0);
                strm.WriteByte(0); // Write the 8 empty bytes of the header
                strm.WriteByte(0);
                strm.WriteByte(0);
                strm.WriteByte(0);


                string FontName = (string)FontComboBox.SelectedItem;

                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(FontName);
                int i = 256 - buffer.Length;
                if (i < 0)
                {
                    throw new Exception("Font Name is to long!");
                }
                else
                {
                    strm.Write(buffer, 0, buffer.Length);
                    for (int i2 = 0; i2 < i; i2++)
                    {
                        strm.WriteByte(0); // Fill the rest of the 256 bytes.
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

                    UInt64 charsToWrite = (ulong)chars.Count * 16;
                    buffer = BitConverter.GetBytes(charsToWrite);
                    strm.Write(buffer, 0, buffer.Length); // Write the number of chars to read.

                    int prevChar = 0;

                    for (byte style = 0; style < 16; style++)
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
                                strm.WriteByte(255); // write that it's incremented from the previous char.
                            }
                            else
                            {
                                strm.WriteByte(0); // write that it's not incremented from the previous char.
                                buffer = BitConverter.GetBytes((UInt32)ch);
                                strm.Write(buffer, 0, buffer.Length); // Write the char number.
                            }
                            pictureBox1.Image = Backend;
                            pictureBox1.Refresh();

                            strm.WriteByte(style); // write it's style
                            strm.WriteByte(height); // write the height
                            strm.WriteByte(width); // write the width
                            buffer = ConvertToByteArray(Backend);
                            strm.Write(buffer, 0, buffer.Length);
                            prevChar = ch;
                        }
                        strm.Flush();
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
            bool[] bits = new bool[b.Height * b.Width];
            int bitnum = 0;
            Color Black = Color.FromArgb(255, 255, 255);
            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    if (b.GetPixel(x, y) == Black)
                    {
                        bits[bitnum] = false;
                    }
                    else
                    {
                        bits[bitnum] = true;
                    }
                    bitnum++;
                }
            }
            int bytes = (Int32)Math.Ceiling((double)((b.Width * b.Height) / 8));
            byte[] arr2 = new byte[bytes];
            int bitIndex = 0, byteIndex = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    arr2[byteIndex] |= (byte)(((byte)1) << bitIndex);
                }
                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return arr2;
        }

        private void FontComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileStream str = new FileStream("AgencyFB.opff", FileMode.Open);
            byte[] data = new byte[str.Length];
            str.Read(data, 0, (int)str.Length);
            Orvid.Graphics.FontSupport.OPFF f = new Orvid.Graphics.FontSupport.OPFF(data);
            data = null;
            str.Close();
            str.Dispose();
            Orvid.Graphics.Image i = f.GetCharacter(Int32.Parse(textBox2.Text), Orvid.Graphics.FontSupport.FontFlag.Normal);
            //i.AntiAlias();
            //i.HalveSize();
            i.HalveSize();
            Bitmap b = new Bitmap(i.Width, i.Height);
            for (uint x = 0; x < i.Width; x++)
            {
                for (uint y = 0; y < i.Height; y++)
                {
                    //if (i.GetPixel(x, y) == null)
                    //    throw new Exception();
                    b.SetPixel((int)x, (int)y, i.GetPixel(x, y));
                }
            }
            pictureBox1.Image = b;
            pictureBox1.Size = new Size(i.Width, i.Height);
            f = null;
            System.GC.Collect();

            Font f2 = new Font("Agency FB", 32, (FontStyle)0, GraphicsUnit.Pixel);
            Bitmap Backend = new Bitmap(i.Width, i.Height);
            Graphics g = Graphics.FromImage(Backend);
            g.Clear(Color.White);
            g.DrawString(new String(new char[] { (char)Int32.Parse(textBox2.Text) }), f2, new SolidBrush(Color.Black), 2, 2);
            g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
            pictureBox2.Image = Backend;
            pictureBox2.Size = new Size(i.Width, i.Height);
            f2 = null;
            g = null;
            i = null;
            System.GC.Collect();
        }

        private void CompressionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CompressionMode = (CompressionType)Enum.Parse(typeof(CompressionType), (String)CompressionComboBox.Items[CompressionComboBox.SelectedIndex]);
        }

        private void CompressionComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
