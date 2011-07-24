using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Orvid.Graphics;

namespace TestBed
{
    public partial class Form1 : Form
    {
        TestbedImage i = new TestbedImage(512, 512);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = i.back;

            //i.FloodFill(new Vec2(256, 256), new Pixel(true), Colors.White);

            i.DrawPolygon(new Vec2[] { new Vec2(100, 50), new Vec2(150, 50), new Vec2(175, 75), new Vec2(175, 125), new Vec2(150, 150), new Vec2(100, 150), new Vec2(75, 125), new Vec2(75, 75) }, Colors.Black);
            i.DrawCircleOutline(new Vec2(300, 128), 32, Colors.Black);
            i.DrawElipse(new Vec2(400, 400), 30, 60, new Pixel(0, 128, 0, 255));
        }
        uint x;
        uint y;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            i.SetPixel(x, y, new Pixel(255, 0, 0, 255));
            if (x < 511)
                x++;
            else
                x = 2;
            if (y < 511)
                y++;
            else
                y = 0;

            pictureBox1.Refresh();
        }
    }

    public class TestbedImage : Orvid.Graphics.Image
    {
        public Bitmap back;

        public TestbedImage(int width, int height) : base(width, height)
        {
            back = new Bitmap(width, height);
            
        }

        public override void SetPixel(uint x, uint y, Orvid.Graphics.Pixel p)
        {
            back.SetPixel((int)x, (int)y, p);
        }

        public override Pixel GetPixel(uint x, uint y)
        {
            return back.GetPixel((int)x, (int)y);
        }
    }
}
