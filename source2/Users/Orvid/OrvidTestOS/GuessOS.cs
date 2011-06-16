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

        private const float precision = 0.01f; // 1 thousandth precision.

        private static float Abs(float value)
        {
            if (value < 0)
                return (0 - value);
            else
                return value;
        }

        public static double Sqrt(double number)
        {
            if (number < 0)
                Cosmos.System.Global.Console.WriteLine("number must be non-negative!");

            Cosmos.System.Global.Console.WriteLine("Value of number is: " + number.ToString());
            double guess = number / 2;
            double diff = (guess * guess) - number;
            Cosmos.System.Global.Console.WriteLine("Num of Iterations: 0 , estimate: " + guess.ToString());
            int counter = 1;
            while (Math.Abs(diff) > precision && counter <= 20)
            {
                guess = (guess - diff) / (2 * guess);
                diff = (guess * guess) - number;
                Cosmos.System.Global.Console.WriteLine("Num of Iterations: " + counter.ToString() + " , estimate: " + guess.ToString());
                counter++;
            }
            if (counter > 100)
                Cosmos.System.Global.Console.WriteLine("100 iterations done with no good enough answer");
            return guess;
        }

        protected override void Run()
        {

            Vec2 v;
            s.Taskbar.Clear(30);

            #region Draw myself a palette reference
            uint i = 0;
            for (uint y = 0; y < 16; y++)
            {
                for (uint x = 0; x < 16; x++)
                {
                    s.Taskbar.SetPixel((x * 4 + 1), (y * 4 + 1), i);
                    s.Taskbar.SetPixel((x * 4 + 1), (y * 4 + 2), i);
                    s.Taskbar.SetPixel((x * 4 + 1), (y * 4 + 3), i);
                    s.Taskbar.SetPixel((x * 4 + 1), (y * 4 + 4), i);
                    s.Taskbar.SetPixel((x * 4 + 2), (y * 4 + 1), i);
                    s.Taskbar.SetPixel((x * 4 + 2), (y * 4 + 2), i);
                    s.Taskbar.SetPixel((x * 4 + 2), (y * 4 + 3), i);
                    s.Taskbar.SetPixel((x * 4 + 2), (y * 4 + 4), i);
                    s.Taskbar.SetPixel((x * 4 + 3), (y * 4 + 1), i);
                    s.Taskbar.SetPixel((x * 4 + 3), (y * 4 + 2), i);
                    s.Taskbar.SetPixel((x * 4 + 3), (y * 4 + 3), i);
                    s.Taskbar.SetPixel((x * 4 + 3), (y * 4 + 4), i);
                    s.Taskbar.SetPixel((x * 4 + 4), (y * 4 + 1), i);
                    s.Taskbar.SetPixel((x * 4 + 4), (y * 4 + 2), i);
                    s.Taskbar.SetPixel((x * 4 + 4), (y * 4 + 3), i);
                    s.Taskbar.SetPixel((x * 4 + 4), (y * 4 + 4), i);
                    i++;
                }
            }
            s.Taskbar.SetPixel(1, 1, 255);
            s.Taskbar.SetPixel(1, 2, 255);
            s.Taskbar.SetPixel(1, 3, 255);
            s.Taskbar.SetPixel(1, 4, 255);
            s.Taskbar.SetPixel(2, 1, 255);
            s.Taskbar.SetPixel(2, 2, 255);
            s.Taskbar.SetPixel(2, 3, 255);
            s.Taskbar.SetPixel(2, 4, 255);
            s.Taskbar.SetPixel(3, 1, 255);
            s.Taskbar.SetPixel(3, 2, 255);
            s.Taskbar.SetPixel(3, 3, 255);
            s.Taskbar.SetPixel(3, 4, 255);
            s.Taskbar.SetPixel(4, 1, 255);
            s.Taskbar.SetPixel(4, 2, 255);
            s.Taskbar.SetPixel(4, 3, 255);
            s.Taskbar.SetPixel(4, 4, 255);
            #endregion

            v = new Vec2(150, 100);
            s.Taskbar.DrawCircleOutline(v, 10, (uint)8);

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
                Tick++;
                //m.HandleMouse();

                if (OddRefresh)
                {
                    s.Taskbar.SetPixel(312, 4, 255);
                    s.Taskbar.SetPixel(313, 4, 255);
                    s.Taskbar.SetPixel(312, 5, 255);
                    s.Taskbar.SetPixel(313, 5, 255);
                    OddRefresh = false;
                }
                else
                {
                    s.Taskbar.SetPixel(312, 4, 31);
                    s.Taskbar.SetPixel(313, 4, 31);
                    s.Taskbar.SetPixel(312, 5, 31);
                    s.Taskbar.SetPixel(313, 5, 31);
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
                    s.Taskbar.DrawCircleOutline(v, 20, (uint)48);
                    ProcessKeyboard(c);
                    HadCharPrev = true;
                }
                else if (HadCharPrev)
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 20, (uint)30);
                    HadCharPrev = false;
                }
                chrsProcd = 0;

                if (m.Buttons != Mouse.MouseState.None)
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 30, (uint)8);
                }
                else
                {
                    v = new Vec2(150, 100);
                    s.Taskbar.DrawCircleOutline(v, 30, (uint)30);
                }

                //for (uint y = 0; y < s.s.PixelHeight; y++)
                //    for (uint x = 0; x < s.s.PixelWidth; x++)
                //        s.SetPixel(y, x, (byte)(x + y));

                //s.DrawPolygon(new Vec2[] { new Vec2(100, 50), new Vec2(150, 50), new Vec2(175, 75), new Vec2(175, 125), new Vec2(150, 150), new Vec2(100, 150), new Vec2(75, 125), new Vec2(75, 75) }, (uint)4);
                //s.DrawPolygonOutline(new Vec2[] { new Vec2(400, 50), new Vec2(450, 50), new Vec2(475, 75), new Vec2(475, 125), new Vec2(450, 150), new Vec2(400, 150), new Vec2(375, 125), new Vec2(375, 75) }, (uint)4);
                //s.DrawTriangle(new Vec2(75,100), new Vec2(75,75), new Vec2(25,100), (uint)216);
                //v = new Vec2(100, 100);
                //s.Taskbar.DrawCircle(v, 20, (uint)16);
                //s.DrawCircleOutline(new Vec2(150, 100), 20, (uint)8);
                //s.DrawCircleOutline(new Vec2(Mouse.X, Mouse.Y), 15, (uint)16);
                //s.Taskbar.DrawElipticalArc(v, 30, 10, 10, 300, (uint)90);
                s.Taskbar.DrawRectangle(new Vec2(120, 80), new Vec2(80, 140), (uint)238);



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