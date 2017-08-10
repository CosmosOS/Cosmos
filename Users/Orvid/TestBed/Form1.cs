using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using Forms = System.Windows.Forms;
using Orvid.Graphics;
using OForms;
using OForms.Controls;
using OForms.Windows;

namespace TestBed
{
    public partial class Form1 : Forms.Form
    {
        private const int DesktopWidth = 640;
        private const int DesktopHeight = 480;
        List<ObjectEvents> Objects = new List<ObjectEvents>();
        Image i = new Image(DesktopWidth, DesktopHeight);
        WindowManager windowManager = new WindowManager(new Vec2(DesktopWidth, DesktopHeight));
        //Orvid.Graphics.FontSupport.Font fnt;
        Window w1;

        public Form1()
        {
            InitializeComponent();
            //System.IO.StreamReader sr = new System.IO.StreamReader("Vera-10.bdf");
            //Orvid.Graphics.FontSupport.FontManager.Instance.LoadFont(1, sr.BaseStream);
            //System.IO.StreamReader sr = new System.IO.StreamReader("MS-Sans-Serif_24.FNT");
            //fnt = Orvid.Graphics.FontSupport.FontManager.Instance.LoadFont(2, sr.BaseStream);
        }

        void bt_Click(Vec2 loc, MouseButtons buttons)
        {
            //if (w1.CurrentState == OForms.Windows.WindowState.Normal)
            //{
            //    windowManager.MaximizeWindow(w1);
            //}
            //else
            //{
            //    windowManager.RestoreWindow(w1);
            //}
            Window w = new Window(new Vec2(130, 30), new Vec2(120, 80), "Test Window 3");
            w.ClearColor = Colors.Blue;
            windowManager.AddWindow(w);
        }

        #region Mouse Functions
        /// <summary>
        /// This image contains everything that is behind the mouse.
        /// </summary>
        private Image behindMouseImage = new Image(5, 5); // This means max mouse size is 4x4
        private Image MouseImage;
        public uint MouseX = 0;
        public uint MouseY = 0;
        private void DrawCursor()
        {

            #region SaveBehindMouse

            behindMouseImage.SetPixel(0, 0, i.GetPixel(MouseX, MouseY));
            behindMouseImage.SetPixel(1, 0, i.GetPixel(MouseX + 1, MouseY));
            behindMouseImage.SetPixel(2, 0, i.GetPixel(MouseX + 2, MouseY));
            behindMouseImage.SetPixel(0, 1, i.GetPixel(MouseX, MouseY + 1));
            behindMouseImage.SetPixel(0, 2, i.GetPixel(MouseX, MouseY + 2));
            behindMouseImage.SetPixel(1, 1, i.GetPixel(MouseX + 1, MouseY + 1));
            behindMouseImage.SetPixel(2, 2, i.GetPixel(MouseX + 2, MouseY + 2));
            behindMouseImage.SetPixel(3, 3, i.GetPixel(MouseX + 3, MouseY + 3));
            behindMouseImage.SetPixel(4, 4, i.GetPixel(MouseX + 4, MouseY + 4));

            #region Old Block Mouse
            //behindMouseImage.SetPixel(0, 0, i.GetPixel(MouseX, MouseY));
            //behindMouseImage.SetPixel(1, 0, i.GetPixel(MouseX + 1, MouseY));
            //behindMouseImage.SetPixel(2, 0, i.GetPixel(MouseX + 2, MouseY));
            //behindMouseImage.SetPixel(3, 0, i.GetPixel(MouseX + 3, MouseY));
            //behindMouseImage.SetPixel(4, 0, i.GetPixel(MouseX + 4, MouseY));

            //behindMouseImage.SetPixel(0, 1, i.GetPixel(MouseX, MouseY + 1));
            //behindMouseImage.SetPixel(1, 1, i.GetPixel(MouseX + 1, MouseY + 1));
            //behindMouseImage.SetPixel(2, 1, i.GetPixel(MouseX + 2, MouseY + 1));
            //behindMouseImage.SetPixel(3, 1, i.GetPixel(MouseX + 3, MouseY + 1));
            //behindMouseImage.SetPixel(4, 1, i.GetPixel(MouseX + 4, MouseY + 1));

            //behindMouseImage.SetPixel(0, 2, i.GetPixel(MouseX, MouseY + 2));
            //behindMouseImage.SetPixel(1, 2, i.GetPixel(MouseX + 1, MouseY + 2));
            //behindMouseImage.SetPixel(2, 2, i.GetPixel(MouseX + 2, MouseY + 2));
            //behindMouseImage.SetPixel(3, 2, i.GetPixel(MouseX + 3, MouseY + 2));
            //behindMouseImage.SetPixel(4, 2, i.GetPixel(MouseX + 4, MouseY + 2));

            //behindMouseImage.SetPixel(0, 3, i.GetPixel(MouseX, MouseY + 3));
            //behindMouseImage.SetPixel(1, 3, i.GetPixel(MouseX + 1, MouseY + 3));
            //behindMouseImage.SetPixel(2, 3, i.GetPixel(MouseX + 2, MouseY + 3));
            //behindMouseImage.SetPixel(3, 3, i.GetPixel(MouseX + 3, MouseY + 3));
            //behindMouseImage.SetPixel(4, 3, i.GetPixel(MouseX + 4, MouseY + 3));

            //behindMouseImage.SetPixel(0, 4, i.GetPixel(MouseX, MouseY + 4));
            //behindMouseImage.SetPixel(1, 4, i.GetPixel(MouseX + 1, MouseY + 4));
            //behindMouseImage.SetPixel(2, 4, i.GetPixel(MouseX + 2, MouseY + 4));
            //behindMouseImage.SetPixel(3, 4, i.GetPixel(MouseX + 3, MouseY + 4));
            //behindMouseImage.SetPixel(4, 4, i.GetPixel(MouseX + 4, MouseY + 4));
            #endregion

            #endregion

            #region Draw Mouse
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            i.DrawImage(v, MouseImage);
            #endregion

        }

        private void SetMouseDown()
        {
            MouseImage.SetPixel(0, 0, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(1, 0, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(2, 0, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(0, 1, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(0, 2, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(1, 1, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(2, 2, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(3, 3, new Pixel(128, 128, 128, 255));
            MouseImage.SetPixel(4, 4, new Pixel(128, 128, 128, 255));
        }

        private void SetMouseUp()
        {
            MouseImage.SetPixel(0, 0, Colors.Black);
            MouseImage.SetPixel(1, 0, Colors.Black);
            MouseImage.SetPixel(2, 0, Colors.Black);
            MouseImage.SetPixel(0, 1, Colors.Black);
            MouseImage.SetPixel(0, 2, Colors.Black);
            MouseImage.SetPixel(1, 1, Colors.Black);
            MouseImage.SetPixel(2, 2, Colors.Black);
            MouseImage.SetPixel(3, 3, Colors.Black);
            MouseImage.SetPixel(4, 4, Colors.Black);
        }

        private void InitializeMouse()
        {
            MouseImage = new Image(5, 5);
            // Now we need to setup the mouse
            MouseImage.SetPixel(0, 0, Colors.Black);
            MouseImage.SetPixel(1, 0, Colors.Black);
            MouseImage.SetPixel(2, 0, Colors.Black);
            MouseImage.SetPixel(0, 1, Colors.Black);
            MouseImage.SetPixel(0, 2, Colors.Black);
            MouseImage.SetPixel(1, 1, Colors.Black);
            MouseImage.SetPixel(2, 2, Colors.Black);
            MouseImage.SetPixel(3, 3, Colors.Black);
            MouseImage.SetPixel(4, 4, Colors.Black);

            // Old mouse
            //MouseImage.SetPixel(0, 0, Colors.Black);
            //MouseImage.SetPixel(1, 0, Colors.Black);
            //MouseImage.SetPixel(2, 0, Colors.Black);
            //MouseImage.SetPixel(3, 0, Colors.Black);
            //MouseImage.SetPixel(0, 1, Colors.Black);
            //MouseImage.SetPixel(1, 1, Colors.White);
            //MouseImage.SetPixel(2, 1, Colors.White);
            //MouseImage.SetPixel(3, 1, Colors.Black);
            //MouseImage.SetPixel(0, 2, Colors.Black);
            //MouseImage.SetPixel(1, 2, Colors.White);
            //MouseImage.SetPixel(2, 2, Colors.White);
            //MouseImage.SetPixel(3, 2, Colors.Black);
            //MouseImage.SetPixel(0, 3, Colors.Black);
            //MouseImage.SetPixel(1, 3, Colors.Black);
            //MouseImage.SetPixel(2, 3, Colors.Black);
            //MouseImage.SetPixel(3, 3, Colors.Black);
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            w1 = new Window(new Vec2(30, 30), new Vec2(120, 80), "Test Window 1");
            w1.ClearColor = Colors.Green;
            Button bt = new Button(new Vec2(10, 10), new Vec2(30, 10));
            bt.Click += new MouseEvent(bt_Click);
            bt.Parent = w1;
            w1.Controls.Add(bt);
            Window w2 = new Window(new Vec2(80, 30), new Vec2(120, 80), "Test Window 2");
            w2.ClearColor = Colors.Red;
            Window w3 = new Window(new Vec2(130, 30), new Vec2(120, 80), "Test Window 3");
            w3.ClearColor = Colors.Blue;
            windowManager.AddWindow(w1);
            windowManager.AddWindow(w2);
            windowManager.AddWindow(w3);
            windowManager.BringWindowToFront(w1);

            InitializeMouse();
            Forms.Cursor.Hide();

            i.Clear(Colors.White);

            //i.DrawString(new Vec2(30, 30), "T", fnt, 20, Orvid.Graphics.FontSupport.FontStyle.Normal, Colors.Black);

            // Draw exit button.
            ExitButton b = new ExitButton(new Vec2(DesktopWidth - 21, 1), new Vec2(20, 20), i, this);
            Objects.Add(b.evnts);


            DrawCursor();
            pictureBox1.Image = (System.Drawing.Bitmap)i;
            pictureBox1.Refresh();
            // Now we need to restore what was behind the mouse.
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            i.DrawImage(v, behindMouseImage);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            foreach (ObjectEvents o in Objects)
            {
                if (o.Bounds.IsInBounds(new Vec2((int)MouseX, (int)MouseY)))
                {
                    o.MouseClick(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, (int)MouseX, (int)MouseY, 0));
                }
            }

            windowManager.HandleMouseClick(new Vec2((int)MouseX, (int)MouseY), OForms.MouseButtons.Left, i);

            DrawCursor();
            pictureBox1.Image = (System.Drawing.Bitmap)i;
            pictureBox1.Refresh();
            // Now we need to restore what was behind the mouse.
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            i.DrawImage(v, behindMouseImage);
        }

        private void pictureBox1_MouseMove(object sender, Forms.MouseEventArgs e)
        {
            MouseX = (uint)e.X;
            MouseY = (uint)e.Y;
            if (MouseX >= i.Width - 5)
            {
                MouseX = (uint)(i.Width - 6);
            }
            if (MouseY >= i.Height - 5)
            {
                MouseY = (uint)(i.Height - 6);
            }


            windowManager.HandleMouseMove(new Vec2((int)MouseX, (int)MouseY), Utils.GetButtons(e.Button), i);


            foreach (ObjectEvents c in Objects)
            {
                if (!c.IsIn)
                {
                    if (c.Bounds.IsInBounds(new Vec2(e.X, e.Y)))
                    {
                        c.IsIn = true;
                        if (c.IsMouseDown)
                        {
                            c.MouseDown(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, (int)MouseX, (int)MouseY, 0));
                        }
                        else
                        {
                            c.MouseEnter(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, (int)MouseX, (int)MouseY, 0));
                        }
                    }
                }
                else
                {
                    if (!c.Bounds.IsInBounds(new Vec2(e.X, e.Y)))
                    {
                        c.IsIn = false;
                        c.MouseLeave(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, (int)MouseX, (int)MouseY, 0));
                    }
                }
            }

            DrawCursor();
            pictureBox1.Image = (System.Drawing.Bitmap)i;
            pictureBox1.Refresh();
            // Now we need to restore what was behind the mouse.
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            i.DrawImage(v, behindMouseImage);
        }

        private void pictureBox1_MouseDown(object sender, Forms.MouseEventArgs e)
        {
            windowManager.HandleMouseDown(new Vec2(e.X, e.Y), Utils.GetButtons(e.Button), i);
            
            foreach (ObjectEvents c in Objects)
            {
                if (c.Bounds.IsInBounds(new Vec2(e.X, e.Y)))
                {
                    c.IsMouseDown = true;
                    c.MouseDown(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, (int)MouseX, (int)MouseY, 0));
                }
            }

            SetMouseDown();
            DrawCursor();
            pictureBox1.Image = (System.Drawing.Bitmap)i;
            pictureBox1.Refresh();
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            i.DrawImage(v, behindMouseImage);
        }

        private void pictureBox1_MouseUp(object sender, Forms.MouseEventArgs e)
        {
            windowManager.HandleMouseUp(new Vec2(e.X, e.Y), Utils.GetButtons(e.Button), i);

            foreach (ObjectEvents c in Objects)
            {
                if (c.IsMouseDown)
                {
                    c.IsMouseDown = false;
                    c.MouseUp(sender, new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, (int)MouseX, (int)MouseY, 0));
                }
            }


            SetMouseUp();
            DrawCursor();
            pictureBox1.Image = (System.Drawing.Bitmap)i;
            pictureBox1.Refresh();
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            i.DrawImage(v, behindMouseImage);
        }

    }

}
