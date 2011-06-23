using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Graphics;
using OForms.Windows;

namespace Orvid.Graphics
{
    /// <summary>
    /// This class is describes a single monitor,
    /// and also handles graphics driver wrangling.
    /// </summary>
    public class Monitor
    {
        /// <summary>
        /// This is the image that contains the background,
        /// It is the lowest on the stack that we pull pixels from.
        /// </summary>
        public Image Background;
        /// <summary>
        /// This layer holds all of the desktop icons.
        /// </summary>
        public Image DesktopIcons;
        /// <summary>
        /// This layer contains all of the windows.
        /// </summary>
        public LinkedList<OWindow> Windows = new LinkedList<OWindow>();
        /// <summary>
        /// This image contains the taskbar.
        /// </summary>
        public Image Taskbar;
        /// <summary>
        /// This image contains the mouse.
        /// </summary>
        public Image Mouse;

        /// <summary>
        /// The current graphics driver.
        /// </summary>
        private GraphicsDriver curdriver;
        /// <summary>
        /// The current graphics driver.
        /// </summary>
        public GraphicsDriver CurrentDriver
        {
            get
            {
                return curdriver;
            }
        }

        /// <summary>
        /// This is the width of the monitor.
        /// </summary>
        public int Width;
        /// <summary>
        /// This is the height of the monitor.
        /// </summary>
        public int Height;

        public uint MouseX = 0;
        public uint MouseY = 0;


        public Monitor()
        {
            int width = 320;
            int height = 200;
            Background = new Image(width, height);
            DesktopIcons = new Image(width, height);
            Taskbar = new Image(width, height);
            Mouse = new Image(width, height);
            this.curdriver = new Drivers.VGADriver();
            //this.curdriver.Initialize();
            this.Width = width;
            this.Height = height;

            InitializeMouse();

            Taskbar.Clear(1); // this should clear the screen white.
            CurrentDriver.Update(Taskbar);
        }

        public void Update()
        {
            DrawCursor();
            //uint i = 0;
            //uint yPixBase = 0;
            //for (uint y = 0; y < Taskbar.Height; y++)
            //{
            //    yPixBase = y * 320;
            //    for (uint x = 0; x < Taskbar.Width; x++)
            //    {
            //        Cosmos.System.Global.Console.WriteLine("The value at (" + x.ToString() + ", " + y.ToString() + ") is '" + Taskbar.GetPixel(x, y).ToString() + "'");
            //        if (i >= 24)
            //        {
            //            Cosmos.System.Global.Console.Y = 0;
            //            Cosmos.System.Global.Console.X = 0;
            //            i = 0;
            //        }
            //        i++;
            //    }
            //}

            CurrentDriver.Update(GetEffectiveImage());
            // Now we need to restore what was behind the mouse.
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            Taskbar.DrawImage(v, behindMouseImage);
        }

        #region Mouse Functions
        /// <summary>
        /// This image contains everything that is behind the mouse.
        /// </summary>
        private Image behindMouseImage = new Image(4, 4); // This means max mouse size is 4x4
        private Image MouseImage = new Image(4, 4);
        private void DrawCursor()
        {
            #region SaveBehindMouse
            behindMouseImage.SetPixel(0, 0, Taskbar.GetPixel(MouseX, MouseY));
            behindMouseImage.SetPixel(1, 0, Taskbar.GetPixel(MouseX + 1, MouseY));
            behindMouseImage.SetPixel(2, 0, Taskbar.GetPixel(MouseX + 2, MouseY));
            behindMouseImage.SetPixel(3, 0, Taskbar.GetPixel(MouseX + 3, MouseY));
            behindMouseImage.SetPixel(0, 1, Taskbar.GetPixel(MouseX, MouseY + 1));
            behindMouseImage.SetPixel(1, 1, Taskbar.GetPixel(MouseX + 1, MouseY + 1));
            behindMouseImage.SetPixel(2, 1, Taskbar.GetPixel(MouseX + 2, MouseY + 1));
            behindMouseImage.SetPixel(3, 1, Taskbar.GetPixel(MouseX + 3, MouseY + 1));
            behindMouseImage.SetPixel(0, 2, Taskbar.GetPixel(MouseX, MouseY + 2));
            behindMouseImage.SetPixel(1, 2, Taskbar.GetPixel(MouseX + 1, MouseY + 2));
            behindMouseImage.SetPixel(2, 2, Taskbar.GetPixel(MouseX + 2, MouseY + 2));
            behindMouseImage.SetPixel(3, 2, Taskbar.GetPixel(MouseX + 3, MouseY + 2));
            behindMouseImage.SetPixel(0, 3, Taskbar.GetPixel(MouseX, MouseY + 3));
            behindMouseImage.SetPixel(1, 3, Taskbar.GetPixel(MouseX + 1, MouseY + 3));
            behindMouseImage.SetPixel(2, 3, Taskbar.GetPixel(MouseX + 2, MouseY + 3));
            behindMouseImage.SetPixel(3, 3, Taskbar.GetPixel(MouseX + 3, MouseY + 3));
            #endregion

            #region Draw Mouse
            Vec2 v = new Vec2((int)MouseX, (int)MouseY);
            Taskbar.DrawImage(v, MouseImage);
            #endregion
        }

        private void InitializeMouse()
        {
            // Now we need to setup the mouse
            // The color 255 is black, and 1 is white.
            MouseImage.SetPixel(0, 0, 255);
            MouseImage.SetPixel(1, 0, 255);
            MouseImage.SetPixel(2, 0, 255);
            MouseImage.SetPixel(3, 0, 255);
            MouseImage.SetPixel(0, 1, 255);
            MouseImage.SetPixel(1, 1, 1);
            MouseImage.SetPixel(2, 1, 1);
            MouseImage.SetPixel(3, 1, 255);
            MouseImage.SetPixel(0, 2, 255);
            MouseImage.SetPixel(1, 2, 1);
            MouseImage.SetPixel(2, 2, 1);
            MouseImage.SetPixel(3, 2, 255);
            MouseImage.SetPixel(0, 3, 255);
            MouseImage.SetPixel(1, 3, 255);
            MouseImage.SetPixel(2, 3, 255);
            MouseImage.SetPixel(3, 3, 255);
        }
        #endregion

        public Image GetEffectiveImage()
        {
//            Image i = new Image(Width, Height);
//            Vec2 v = new Vec2(0,0);
//            i.DrawImage(v, Background);
//            i.DrawImage(v, DesktopIcons);

//#warning TODO: Draw the windows here.

//            i.DrawImage(v, Taskbar);
//            i.DrawImage(v, Mouse);

            return Taskbar;

        }
    }



    //public class Screen
    //{
    //    // The arrays are in the format Y,X
    //    public byte[][] Data;
    //    public bool[][] Filled;

    //    public Screen(int height, int width)
    //    {
    //        //Data = new byte[height, width];
    //        //Filled = new bool[height, width];
    //    }

    //    public bool IsFilled(int X, int Y)
    //    {
    //        return Filled[Y][X];
    //    }
    //}

    //public class LayeredScreen
    //{
    //    LinkedList<Screen> Screens = new LinkedList<Screen>();
    //    int Height = 200;
    //    int Width = 480;

    //    public LayeredScreen(int height, int width)
    //    {
    //        this.Height = height;
    //        this.Width = width;
    //        Screen s = new Screen(height, width);
    //        Screens.AddFirst(s);
    //    }

    //    public void SendToBack(Screen scrn)
    //    {
    //        Screens.Remove(scrn);
    //        Screens.AddLast(scrn);
    //    }

    //    public void BringToFront(Screen scrn)
    //    {
    //        Screens.Remove(scrn);
    //        Screens.AddFirst(scrn);
    //    }

    //    public byte[][] GetEffectiveScreen()
    //    {
    //        Screen scrn = new Screen(Height, Width);
    //        LinkedList<Screen> ToReturn = new LinkedList<Screen>();
    //        bool HaveAll = false;
    //        while (!HaveAll)
    //        {
    //            Screen s = Screens.First.Value;
    //            Screens.RemoveFirst();

    //            for (int y = 0; y < Height; y++)
    //            {
    //                for (int x = 0; x < Width; x++)
    //                {
    //                    if (!scrn.IsFilled(x, y))
    //                    {
    //                        if (s.IsFilled(x, y))
    //                        {
    //                            byte val = s.Data[y][x];
    //                            scrn.Data[y][x] = val;
    //                            scrn.Filled[y][x] = true;
    //                        }
    //                    }
    //                }
    //            }

    //            ToReturn.AddFirst(s);
    //            // Check if we've run out of screens
    //            if (Screens.Count < 1)
    //            {
    //                break;
    //            }
    //            // Otherwise we need to check if we have everything we need.
    //            //
    //            // TODO: Make this more efficient by only checking the lines
    //            // we still need.
    //            else
    //            {
    //                bool haveall = true;
    //                for (int y = 0; y < Height; y++)
    //                {
    //                    for (int x = 0; x < Width; x++)
    //                    {
    //                        if (s.Filled[y][x] == false)
    //                        {
    //                            haveall = false;
    //                            break;
    //                        }
    //                    }
    //                    if (!haveall)
    //                    {
    //                        break;
    //                    }
    //                }
    //                HaveAll = haveall;
    //            }
    //        }
    //        // Now we need to add the screens back in.
    //        int nmb = ToReturn.Count;
    //        for (int i = 0; i < nmb; i++)
    //        {
    //            Screen scrn1 = ToReturn.First.Value;
    //            ToReturn.RemoveFirst();
    //            Screens.AddFirst(scrn1);
    //        }
    //        return scrn.Data;
    //    }
    //}
}
