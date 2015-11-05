//#define AnimateGif
#define DebugAllFormats

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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/png/Building.png"), FileMode.Open);
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

#if DebugAllFormats

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

            #region AddNoise Additive
            {
                t.Start();
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.AddNoise(i, new Orvid.Graphics.BoundingBox(0, i.Width, 0, i.Height), 200, Orvid.Graphics.ImageManipulator.NoiseGenerationMethod.Additive);
                t.Stop();
                WriteToLog("Additive Add-Noise took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                Bitmap b2 = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Additive Add-Noise";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            System.GC.Collect();

            #region Load Jpeg
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/jpeg/Building.jpg"), FileMode.Open);
                Orvid.Graphics.ImageFormats.JpegImage jp = new Orvid.Graphics.ImageFormats.JpegImage();

                t.Start();
                Orvid.Graphics.Image I2 = jp.Load(s);
                t.Stop();
                WriteToLog("Loading a Jpeg Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/tga/Building.tga"), FileMode.Open);
                Orvid.Graphics.ImageFormats.TgaImage tg = new Orvid.Graphics.ImageFormats.TgaImage();

                t.Start();
                Orvid.Graphics.Image I2 = tg.Load(s);
                t.Stop();
                WriteToLog("Loading a Tga  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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

            #region Load Pbm
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/pnm/Building.pbm"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PnmFamilyImage pm = new Orvid.Graphics.ImageFormats.PnmFamilyImage();

                t.Start();
                Orvid.Graphics.Image I2 = pm.Load(s);
                t.Stop();
                WriteToLog("Loading a Pbm Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Pbm Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Pgm
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/pnm/Building.pgm"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PnmFamilyImage pm = new Orvid.Graphics.ImageFormats.PnmFamilyImage();

                t.Start();
                Orvid.Graphics.Image I2 = pm.Load(s);
                t.Stop();
                WriteToLog("Loading a Pgm Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Pgm Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Ppm
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/pnm/Building.ppm"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PnmFamilyImage pm = new Orvid.Graphics.ImageFormats.PnmFamilyImage();

                t.Start();
#warning TODO: Make it so this isn't needed.
                Orvid.Graphics.Image I2 = Orvid.Graphics.ImageManipulator.ReverseRGB(pm.Load(s));
                t.Stop();
                WriteToLog("Loading a Ppm Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Ppm Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Load Tiff
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/tiff/Building.tiff"), FileMode.Open);
                Orvid.Graphics.ImageFormats.TiffImage tf = new Orvid.Graphics.ImageFormats.TiffImage();

                t.Start();
                Orvid.Graphics.Image I2 = tf.Load(s);
                t.Stop();
                WriteToLog("Loading a Tiff Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/png/Building.png"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PngImage p = new Orvid.Graphics.ImageFormats.PngImage();

                t.Start();
                Orvid.Graphics.Image I2 = p.Load(s);
                t.Stop();
                WriteToLog("Loading a Png Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/bmp/Building-24Bit.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage bm = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                Orvid.Graphics.Image I2 = bm.Load(s);
                t.Stop();
                WriteToLog("Loading a 24-Bit Bmp took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/bmp/Building-256Color.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage bm = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                Orvid.Graphics.Image I2 = bm.Load(s);
                t.Stop();
                WriteToLog("Loading a 256-Color Bmp  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/bmp/Building-16Color.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage bm = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                Orvid.Graphics.Image I2 = bm.Load(s);
                t.Stop();
                WriteToLog("Loading a 16-Color Bmp  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/bmp/Building-Monochrome.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage bm = new Orvid.Graphics.ImageFormats.BmpImage();

                t.Start();
                Orvid.Graphics.Image I2 = bm.Load(s);
                t.Stop();
                WriteToLog("Loading a Monochrome Bmp  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
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
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/gif/Test.gif"), FileMode.Open);

                t.Start();
#if AnimateGif
                anim = Orvid.Graphics.ImageFormats.GifSupport.Load(s);
#else
                Orvid.Graphics.AnimatedImage anim = Orvid.Graphics.ImageFormats.GifSupport.Load(s);
#endif
                t.Stop();
                WriteToLog("Loading a Gif  took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();

                Bitmap b = (Bitmap)anim[0];
#if !AnimateGif
                anim.Dispose();
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Gif Image";
                l.Height = b.Height * 2;
                l.Width = b.Width * 2;
                l.Image = b;
#else
                GifPictureBox = new PictureBox();
                GifPictureBox.BorderStyle = BorderStyle.FixedSingle;
                GifPictureBox.Parent = flowLayoutPanel2;
                GifPictureBox.Height = b.Height;
                GifPictureBox.Width = b.Width;
                GifPictureBox.Image = b;
                animpar = new Orvid.Graphics.Shapes.ShapedImage(anim.Width, anim.Height);
                anim.Parent = animpar;
                animpar.Shapes.Add(anim);

                time.Interval = anim.TimePerFrame * 4;
                time.Tick += new EventHandler(time_Tick);
                time.Start();
#endif
            }
            #endregion

            System.GC.Collect();

            #region Load Xpm
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/xpm/Building.xpm"), FileMode.Open);
                Orvid.Graphics.ImageFormats.XpmImage x = new Orvid.Graphics.ImageFormats.XpmImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Xpm Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Xpm Image";
                l.Height = b.Height * 2;
                l.Width = b.Width * 2;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

#endif

            #region Dds Loading

            #region Dxt1
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Dxt1.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Dxt1 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Dxt1 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Dxt2
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Dxt2.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Dxt2 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Dxt2 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Dxt3
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Dxt3.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Dxt3 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Dxt3 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Dxt4
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Dxt4.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Dxt4 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Dxt4 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Dxt5
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Dxt5.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Dxt5 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Dxt5 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A1R5G5B5
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A1R5G5B5.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A1R5G5B5 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A1R5G5B5 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A4R4G4B4
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A4R4G4B4.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A4R4G4B4 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A4R4G4B4 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A8B8G8R8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A8B8G8R8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A8B8G8R8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A8B8G8R8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A8R8G8B8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A8R8G8B8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A8R8G8B8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A8R8G8B8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region R5G6B5
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-R5G6B5.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-R5G6B5 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-R5G6B5 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region R8G8B8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-R8G8B8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-R8G8B8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-R8G8B8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region X8B8G8R8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-X8B8G8R8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-X8B8G8R8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-X8B8G8R8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region X8R8G8B8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-X8R8G8B8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-X8R8G8B8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-X8R8G8B8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region L8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-L8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-L8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-L8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region L8A8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-L8A8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-L8A8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-L8A8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A2R10G10B10
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A2R10G10B10.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A2R10G10B10 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A2R10G10B10 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A2B10G10R10
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A2B10G10R10.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A2B10G10R10 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A2B10G10R10 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A2W10V10U10
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A2W10V10U10.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A2W10V10U10 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A2W10V10U10 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A4L4
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A4L4.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A4L4 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A4L4 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A8R3G3B2
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A8R3G3B2.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A8R3G3B2 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A8R3G3B2 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A16B16G16R16
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A16B16G16R16.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A16B16G16R16 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A16B16G16R16 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A16B16G16R16F
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A16B16G16R16F.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A16B16G16R16F Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A16B16G16R16F Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region A32B32G32R32F
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-A32B32G32R32F.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-A32B32G32R32F Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-A32B32G32R32F Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region G8R8_G8B8
            {
                //FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-G8R8_G8B8.dds"), FileMode.Open);
                //Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

#warning TODO: Add support for this format.
                //t.Start();
                //Orvid.Graphics.Image I2 = x.Load(s);
                //t.Stop();
                //WriteToLog("Loading a Dds-G8R8_G8B8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                //t.Reset();

                //s.Close();
                //s.Dispose();
                //Bitmap b = (Bitmap)I2;
                //LabeledImage l = new LabeledImage();
                //l.BorderStyle = BorderStyle.FixedSingle;
                //l.Parent = flowLayoutPanel2;
                //l.Text = "Loaded Dds-G8R8_G8B8 Image";
                //l.Height = b.Height;
                //l.Width = b.Width;
                //l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Q8W8V8U8
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Q8W8V8U8.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Q8W8V8U8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Q8W8V8U8 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Q16W16V16U16
            {
                //FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Q16W16V16U16.dds"), FileMode.Open);
                //Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

#warning TODO: Add support for this format.
                //t.Start();
                //Orvid.Graphics.Image I2 = x.Load(s);
                //t.Stop();
                //WriteToLog("Loading a Dds-Q16W16V16U16 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                //t.Reset();

                //s.Close();
                //s.Dispose();
                //Bitmap b = (Bitmap)I2;
                //LabeledImage l = new LabeledImage();
                //l.BorderStyle = BorderStyle.FixedSingle;
                //l.Parent = flowLayoutPanel2;
                //l.Text = "Loaded Dds-Q16W16V16U16 Image";
                //l.Height = b.Height;
                //l.Width = b.Width;
                //l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region R3G3B2
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-R3G3B2.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-R3G3B2 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-R3G3B2 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region R8G8_B8G8
            {
                //FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-R8G8_B8G8.dds"), FileMode.Open);
                //Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

#warning TODO: Add support for this format.
                //t.Start();
                //Orvid.Graphics.Image I2 = x.Load(s);
                //t.Stop();
                //WriteToLog("Loading a Dds-R8G8_B8G8 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                //t.Reset();

                //s.Close();
                //s.Dispose();
                //Bitmap b = (Bitmap)I2;
                //LabeledImage l = new LabeledImage();
                //l.BorderStyle = BorderStyle.FixedSingle;
                //l.Parent = flowLayoutPanel2;
                //l.Text = "Loaded Dds-R8G8_B8G8 Image";
                //l.Height = b.Height;
                //l.Width = b.Width;
                //l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region UYVY
            {
                //FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-UYVY.dds"), FileMode.Open);
                //Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

#warning TODO: Add support for this format.
                //t.Start();
                //Orvid.Graphics.Image I2 = x.Load(s);
                //t.Stop();
                //WriteToLog("Loading a Dds-UYVY Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                //t.Reset();

                //s.Close();
                //s.Dispose();
                //Bitmap b = (Bitmap)I2;
                //LabeledImage l = new LabeledImage();
                //l.BorderStyle = BorderStyle.FixedSingle;
                //l.Parent = flowLayoutPanel2;
                //l.Text = "Loaded Dds-UYVY Image";
                //l.Height = b.Height;
                //l.Width = b.Width;
                //l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region X1R5G5B5
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-X1R5G5B5.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-X1R5G5B5 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-X1R5G5B5 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region X4R4G4B4
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-X4R4G4B4.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-X4R4G4B4 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-X4R4G4B4 Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region 3Dc
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-3Dc.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-3Dc Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-3Dc Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region Ati1n
            {
                FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-Ati1n.dds"), FileMode.Open);
                Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

                t.Start();
                Orvid.Graphics.Image I2 = x.Load(s);
                t.Stop();
                WriteToLog("Loading a Dds-Ati1n Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                t.Reset();

                s.Close();
                s.Dispose();
                Bitmap b = (Bitmap)I2;
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel2;
                l.Text = "Loaded Dds-Ati1n Image";
                l.Height = b.Height;
                l.Width = b.Width;
                l.Image = b;
            }
            #endregion

            System.GC.Collect();

            #region YUY2
            {
                //FileStream s = new FileStream(Path.GetFullPath("ImageFormats/dds/Building-YUY2.dds"), FileMode.Open);
                //Orvid.Graphics.ImageFormats.DdsImage x = new Orvid.Graphics.ImageFormats.DdsImage();

#warning TODO: Add support for this format.
                //t.Start();
                //Orvid.Graphics.Image I2 = x.Load(s);
                //t.Stop();
                //WriteToLog("Loading a Dds-YUY2 Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
                //t.Reset();

                //s.Close();
                //s.Dispose();
                //Bitmap b = (Bitmap)I2;
                //LabeledImage l = new LabeledImage();
                //l.BorderStyle = BorderStyle.FixedSingle;
                //l.Parent = flowLayoutPanel2;
                //l.Text = "Loaded Dds-YUY2 Image";
                //l.Height = b.Height;
                //l.Width = b.Width;
                //l.Image = b;
            }
            #endregion

            #endregion

            System.GC.Collect();


            //#region Load Pcx
            //{
            //    FileStream s = new FileStream(Path.GetFullPath("ImageFormats/pcx/Building.pcx"), FileMode.Open);
            //    Orvid.Graphics.ImageFormats.PcxImage px = new Orvid.Graphics.ImageFormats.PcxImage();

            //    t.Start();
            //    Orvid.Graphics.Image I2 = px.Load(s);
            //    t.Stop();
            //    WriteToLog("Loading a Pcx Image took '" + t.ElapsedMilliseconds.ToString() + " ms'");
            //    t.Reset();

            //    s.Close();
            //    s.Dispose();
            //    Bitmap b = (Bitmap)I2;
            //    LabeledImage l = new LabeledImage();
            //    l.BorderStyle = BorderStyle.FixedSingle;
            //    l.Parent = flowLayoutPanel2;
            //    l.Text = "Loaded Pcx Image";
            //    l.Height = b.Height;
            //    l.Width = b.Width;
            //    l.Image = b;
            //}
            //#endregion

            //System.GC.Collect();


            st.Flush();
            st.Close();
            st.Dispose();

        }

#if AnimateGif
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
#endif
    }
}
