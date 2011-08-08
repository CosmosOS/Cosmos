using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ImageManipulatorTester
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private StreamWriter st;

        public Form1()
        {
            InitializeComponent();
        }

        private void WriteToLog(String s)
        {
            st.WriteLine(s);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LabeledImage li = new LabeledImage();
            Orvid.Graphics.Image i;
            st = new StreamWriter(Path.GetFullPath("Log.txt"));
            System.Diagnostics.Stopwatch t = System.Diagnostics.Stopwatch.StartNew();
            t.Stop();
            t.Reset();

            #region Load Original
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.png"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PngImage p = new Orvid.Graphics.ImageFormats.PngImage();
                i = p.Load(s);
                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                li.BorderStyle = BorderStyle.FixedSingle;
                li.Parent = flowLayoutPanel1;
                li.Text = "Original Image";
                li.Height = b.Height;
                li.Width = b.Width;
                li.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Nearest Neighbor Scaling
            {
                t.Start();
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width / 2, i.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.NearestNeighbor);
                t.Stop();
                WriteToLog("Nearest Neighbor Scaling took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                Bitmap b2 = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Nearest Neighbor Scaling 1/2";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            System.GC.Collect();

            #region Bi-Linear Scaling
            {
                t.Start();
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width / 2, i.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.Bilinear);
                t.Stop();
                WriteToLog("Bi-Linear Scaling took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                Bitmap b2 = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Bi-Linear Scaling 1/2";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            System.GC.Collect();

            #region Bi-Cubic Scaling
            {
                t.Start();
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width / 2, i.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.Bicubic);
                t.Stop();
                WriteToLog("Bi-Cubic Scaling took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                Bitmap b2 = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Bi-Cubic Scaling 1/2";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            System.GC.Collect();

            #region Convert To Greyscale
            {
                t.Start();
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.ConvertToGreyscale(i);
                t.Stop();
                WriteToLog("Converting To Greyscale took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                Bitmap b2 = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Convert To Greyscale";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            System.GC.Collect();

            #region Invert Colors
            {
                t.Start();
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.InvertColors(i);
                t.Stop();
                WriteToLog("Inverting Colors took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                Bitmap b2 = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Invert Colors";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            System.GC.Collect();

            #region Load Jpeg
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.jpg"), FileMode.Open);
                Orvid.Graphics.ImageFormats.JpegImage p = new Orvid.Graphics.ImageFormats.JpegImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a Jpeg Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Jpeg Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Tga
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.tga"), FileMode.Open);
                Orvid.Graphics.ImageFormats.TgaImage p = new Orvid.Graphics.ImageFormats.TgaImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a Tga  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Tga Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Tiff
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.tiff"), FileMode.Open);
                Orvid.Graphics.ImageFormats.TiffImage p = new Orvid.Graphics.ImageFormats.TiffImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a Tiff Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Tiff Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Png
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.png"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PngImage p = new Orvid.Graphics.ImageFormats.PngImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a Png Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Png Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load 24-Bit Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-24Bit.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a 24-Bit Bmp took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded 24-Bit Bmp Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load 256-Color Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-256Color.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a 256-Color Bmp  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded 256-Color Bmp Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load 16-Color Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-16Color.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a 16-Color Bmp  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded 16-Color Bmp Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Monochrome Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-Monochrome.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                i = p.Load(s);
                t.Stop();
                WriteToLog("Loading a Monochrome Bmp  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)i;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Monochrome Bmp Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Gif
            {
                FileStream s = new FileStream(Path.GetFullPath("Test.gif"), FileMode.Open);

                t.Start();
                anim = Orvid.Graphics.ImageFormats.GifSupport.Load(s);
                t.Stop();
                WriteToLog("Loading a Gif  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();

                Bitmap b = (Bitmap)anim[0];
                GifPictureBox = new PictureBox();
                GifPictureBox.BorderStyle = BorderStyle.FixedSingle;
                GifPictureBox.Parent = flowLayoutPanel2;
                GifPictureBox.Height = b.Height;
                GifPictureBox.Width = b.Width;
                GifPictureBox.Image = b;
                animpar = new Orvid.Graphics.Shapes.ShapedImage(anim.Width, anim.Height);
                anim.Parent = animpar;
                animpar.Shapes.Add(anim);

                //time.Interval = anim.TimePerFrame * 4;
                //time.Tick += new EventHandler(time_Tick);
                //time.Start();
            }
            #endregion

            st.Flush();
            st.Close();
            st.Dispose();

        }

        private PictureBox GifPictureBox;
        private Orvid.Graphics.AnimatedImage anim;
        private Orvid.Graphics.Shapes.ShapedImage animpar;
        private Timer time = new Timer();
        private void time_Tick(object sender, EventArgs e)
        {
            animpar.Modified = true;
            GifPictureBox.Image = (Bitmap)animpar.Render();
            GifPictureBox.Refresh();
        }
    }
}
