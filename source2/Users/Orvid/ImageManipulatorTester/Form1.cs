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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LabeledImage li = new LabeledImage();
            Orvid.Graphics.Image i;

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

            #region Nearest Neighbor Scaling
            {
                Bitmap b2 = (Bitmap)Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width / 2, i.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.NearestNeighbor);
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Nearest Neighbor Scaling 1/2";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            #region Bi-Linear Scaling
            {
                Bitmap b2 = (Bitmap)Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width / 2, i.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.Bilinear);
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Bi-Linear Scaling 1/2";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            #region Bi-Cubic Scaling
            {
                Bitmap b2 = (Bitmap)Orvid.Graphics.ImageManipulator.Resize(i, new Orvid.Graphics.Vec2(i.Width / 2, i.Height / 2), Orvid.Graphics.ImageManipulator.ScalingAlgorithm.Bicubic);
                LabeledImage l = new LabeledImage();
                l.BorderStyle = BorderStyle.FixedSingle;
                l.Parent = flowLayoutPanel1;
                l.Text = "Bi-Cubic Scaling 1/2";
                l.Height = b2.Height;
                l.Width = b2.Width;
                l.Image = b2;
            }
            #endregion

            #region Load Jpeg
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.jpg"), FileMode.Open);
                Orvid.Graphics.ImageFormats.JpegImage p = new Orvid.Graphics.ImageFormats.JpegImage();
                i = p.Load(s);
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

            #region Load Png
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.png"), FileMode.Open);
                Orvid.Graphics.ImageFormats.PngImage p = new Orvid.Graphics.ImageFormats.PngImage();
                i = p.Load(s);
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

            #region Load 24-Bit Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-24Bit.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();
                i = p.Load(s);
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

            #region Load 256-Color Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-256Color.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();
                i = p.Load(s);
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

            #region Load 16-Color Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-16Color.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();
                i = p.Load(s);
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

            #region Load Monochrome Bmp
            {
                FileStream s = new FileStream(Path.GetFullPath("Building-Monochrome.bmp"), FileMode.Open);
                Orvid.Graphics.ImageFormats.BmpImage p = new Orvid.Graphics.ImageFormats.BmpImage();
                i = p.Load(s);
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

            #region Load Tiff
            {
                FileStream s = new FileStream(Path.GetFullPath("Building.tiff"), FileMode.Open);
                Orvid.Graphics.ImageFormats.TiffImage p = new Orvid.Graphics.ImageFormats.TiffImage();
                i = p.Load(s);
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


        }
    }
}
