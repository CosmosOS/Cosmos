using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Hardware;
using Orvid.Graphics;

namespace GuessKernel
{
    public class GuessOS : Sys.Kernel
    {
        public static int Tick = 0;
        public static Monitor s = new Monitor();
        public static Keyboard k = new Keyboard();
        public static Mouse m = new Mouse();

        public GuessOS()
        {

        }

        protected override void BeforeRun()
        {
            m.Initialize();
        }

        public static uint MouseX
        {
            get
            {
                return s.MouseX;
            }
            set
            {
                s.MouseX = value;
            }
        }

        public static uint MouseY
        {
            get
            {
                return s.MouseY;
            }
            set
            {
                s.MouseY = value;
            }
        }

        /// <summary>
        /// This is used to draw a small flashing square in 
        /// the top right corner of the screen.
        /// </summary>
        private bool OddRefresh = false;

        private bool HadCharPrev = false;

        protected override void Run()
        {
            Console.WriteLine("This is just a test.");

            Vec2 v;
            s.Taskbar.Clear(new Pixel(0,255,0,255));

            //#region Draw myself a palette reference
            //uint i = 0;
            //for (uint height = 0; height < 16; height++)
            //{
            //    for (uint width = 0; width < 16; width++)
            //    {
            //        s.Taskbar.SetPixel((width * 4 + 1), (height * 4 + 1), i);
            //        s.Taskbar.SetPixel((width * 4 + 1), (height * 4 + 2), i);
            //        s.Taskbar.SetPixel((width * 4 + 1), (height * 4 + 3), i);
            //        s.Taskbar.SetPixel((width * 4 + 1), (height * 4 + 4), i);
            //        s.Taskbar.SetPixel((width * 4 + 2), (height * 4 + 1), i);
            //        s.Taskbar.SetPixel((width * 4 + 2), (height * 4 + 2), i);
            //        s.Taskbar.SetPixel((width * 4 + 2), (height * 4 + 3), i);
            //        s.Taskbar.SetPixel((width * 4 + 2), (height * 4 + 4), i);
            //        s.Taskbar.SetPixel((width * 4 + 3), (height * 4 + 1), i);
            //        s.Taskbar.SetPixel((width * 4 + 3), (height * 4 + 2), i);
            //        s.Taskbar.SetPixel((width * 4 + 3), (height * 4 + 3), i);
            //        s.Taskbar.SetPixel((width * 4 + 3), (height * 4 + 4), i);
            //        s.Taskbar.SetPixel((width * 4 + 4), (height * 4 + 1), i);
            //        s.Taskbar.SetPixel((width * 4 + 4), (height * 4 + 2), i);
            //        s.Taskbar.SetPixel((width * 4 + 4), (height * 4 + 3), i);
            //        s.Taskbar.SetPixel((width * 4 + 4), (height * 4 + 4), i);
            //        i++;
            //    }
            //}
            //s.Taskbar.SetPixel(1, 1, 255);
            //s.Taskbar.SetPixel(1, 2, 255);
            //s.Taskbar.SetPixel(1, 3, 255);
            //s.Taskbar.SetPixel(1, 4, 255);
            //s.Taskbar.SetPixel(2, 1, 255);
            //s.Taskbar.SetPixel(2, 2, 255);
            //s.Taskbar.SetPixel(2, 3, 255);
            //s.Taskbar.SetPixel(2, 4, 255);
            //s.Taskbar.SetPixel(3, 1, 255);
            //s.Taskbar.SetPixel(3, 2, 255);
            //s.Taskbar.SetPixel(3, 3, 255);
            //s.Taskbar.SetPixel(3, 4, 255);
            //s.Taskbar.SetPixel(4, 1, 255);
            //s.Taskbar.SetPixel(4, 2, 255);
            //s.Taskbar.SetPixel(4, 3, 255);
            //s.Taskbar.SetPixel(4, 4, 255);
            //#endregion

            v = new Vec2(150, 100);
            s.Taskbar.DrawCircleOutline(v, 10, new Pixel(0, 0, 128, 255));

            //int i1 = 4;
            //int i2 = 9;
            //int i3 = 5;
            //double i1r = Sqrt(i1);
            //double i2r = Sqrt(i2);
            //double i3r = Sqrt(i3);
            //Cosmos.System.Global.Console.WriteLine("The Square root of 4 is: '" + i1r.ToString() + "'");
            //Cosmos.System.Global.Console.WriteLine("The Square root of 9 is: '" + i2r.ToString() + "'");
            //Cosmos.System.Global.Console.WriteLine("The Square root of 5 is: '" + i3r.ToString() + "'");

            //Vec2 lp = new Vec2(175, 50);
            //Vec2 rp = new Vec2(225, 50);
            //Vec2 tp = new Vec2(200, 150);
            //s.Taskbar.DrawTriangle(lp, rp, tp, 4);
            //lp = new Vec2(185, 50);
            //rp = new Vec2(235, 50);
            //tp = new Vec2(210, 150);
            //s.Taskbar.DrawTriangle(lp, rp, tp, 4);

            //s.Taskbar.DrawPolygon(new Vec2[] { new Vec2(100, 50), new Vec2(150, 50), new Vec2(175, 75), new Vec2(175, 125), new Vec2(150, 150), new Vec2(100, 150), new Vec2(75, 125), new Vec2(75, 75) }, (uint)4);
            
            while (true)
            {
                Console.WriteLine("This is just a test.");
                Tick++;
                MouseX = (uint)m.X;
                MouseY = (uint)m.Y;
                //m.HandleMouse();

                if (OddRefresh)
                {
                    s.Taskbar.SetPixel(312, 4, Colors.Black);
                    s.Taskbar.SetPixel(313, 4, Colors.Black);
                    s.Taskbar.SetPixel(312, 5, Colors.Black);
                    s.Taskbar.SetPixel(313, 5, Colors.Black);
                    OddRefresh = false;
                }
                else
                {
                    s.Taskbar.SetPixel(312, 4, new Pixel(0,255,0,255));
                    s.Taskbar.SetPixel(313, 4, new Pixel(0,255,0,255));
                    s.Taskbar.SetPixel(312, 5, new Pixel(0,255,0,255));
                    s.Taskbar.SetPixel(313, 5, new Pixel(0,255,0,255));
                    OddRefresh = true;
                }


                //OButton b = new OButton();
                //b.Color = 31;
                //b.Location = new Vec2(150, 100);
                //b.Size = new Vec2(75, 20);
                //b.Draw();

                char c;
                if (k.GetChar(out c))
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 20, new Pixel(255, 0, 0, 255));
                    ProcessKeyboard(c);
                    HadCharPrev = true;
                }
                else if (HadCharPrev)
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 20, new Pixel(0, 255, 0, 255));
                    HadCharPrev = false;
                }
                chrsProcd = 0;

                if (m.Buttons != Mouse.MouseState.None)
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 30, new Pixel(255, 0, 0, 255));
                }
                else
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 30, new Pixel(0, 255, 0, 255));
                }

                //for (uint height = 0; height < s.s.PixelHeight; height++)
                //    for (uint width = 0; width < s.s.PixelWidth; width++)
                //        s.SetPixel(height, width, (byte)(width + height));

                //s.DrawPolygon(new Vec2[] { new Vec2(100, 50), new Vec2(150, 50), new Vec2(175, 75), new Vec2(175, 125), new Vec2(150, 150), new Vec2(100, 150), new Vec2(75, 125), new Vec2(75, 75) }, (uint)4);
                //s.DrawPolygonOutline(new Vec2[] { new Vec2(400, 50), new Vec2(450, 50), new Vec2(475, 75), new Vec2(475, 125), new Vec2(450, 150), new Vec2(400, 150), new Vec2(375, 125), new Vec2(375, 75) }, (uint)4);
                //s.DrawTriangle(new Vec2(75,100), new Vec2(75,75), new Vec2(25,100), (uint)216);
                //v = new Vec2(100, 100);
                //s.Taskbar.DrawCircle(v, 20, (uint)16);
                //s.DrawCircleOutline(new Vec2(150, 100), 20, (uint)8);
                //s.DrawCircleOutline(new Vec2(Mouse.X, Mouse.Y), 15, (uint)16);
                //s.Taskbar.DrawElipticalArc(v, 30, 10, 10, 300, (uint)90);
                //s.Taskbar.DrawReverseRectangle(new Vec2(120, 80), new Vec2(80, 140), (uint)238);



                //s.DrawLine(new Vec2(30, 30), new Vec2(80, 80), 9);
                //s.DrawElipse(new Vec2(150, 50), 10, 10, 8);


                // And finally, update the screen
                s.Update();
            }
        }

        #region Process keyboard
        private int chrsProcd = 0;
        private void ProcessKeyboard(char c)
        {

            if (c == 72 || c == 'w') // Up arrow
            {
                if (MouseY > 0)
                {
                    MouseY--;
                    // The following can be used to create 
                    // the appearance that the mouse is in 
                    // multiple places at once.

                    //DrawCursor();
                }
            }
            else if (c == 80 || c == 's') // Down arrow
            {
                if (MouseY < s.Height - 4)
                {
                    MouseY++;
                    //DrawCursor();
                }
            }
            else if (c == 75 || c == 'a') // Left arrow
            {
                if (MouseX > 0)
                {
                    MouseX--;
                    //DrawCursor();
                }
            }
            else if (c == 77 || c == 'd') // Right arrow
            {
                if (MouseX < s.Width - 4)
                {
                    MouseX++;
                    //DrawCursor();
                }
            }

            // The following allows for processing multiple
            // characters per tick.
            if (chrsProcd < 7)
            {
                char c2;
                if (k.GetChar(out c2))
                {
                    chrsProcd++;
                    ProcessKeyboard(c2);
                }
            }
        }
        #endregion
    }
}