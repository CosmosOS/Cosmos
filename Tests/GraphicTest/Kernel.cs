using System;
using System.Drawing;
//using Screen = Cosmos.System.Graphics.Canvas;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
//using static Cosmos.System.VGAScreen;
//using VGA = Cosmos.System.VGAScreen;

using Pen = Cosmos.System.Graphics.Pen;

namespace GraphicTest
{
    public class Kernel : Sys.Kernel
    {
        Canvas canvas;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphic Mode");

            canvas = FullScreenCanvas.GetFullScreenCanvas();
            //VGA.SetGraphicsMode(ScreenSize.Size320x200, ColorDepth.BitDepth8);
            //VScreen.SetMode(ScreenSize.Size640x480, Bpp.Bpp32);
            //VScreen.SetMode(ScreenSize.Size320x200, Bpp.Bpp32);
        }

        protected override void Run()
        {
            //for (int i = 0; i < 256; i++)
            //Color c = Color.Red;
            //VScreen.SetPixel(69, 69, 0xFF0000);
            //VScreen.SetPixel(69, 69, c.G, c.R, c.B);
            Pen pen = new Pen(Color.Blue);
            canvas.DrawPoint(pen, 69, 69);

            Console.ReadKey();

            TestController.Completed();
        }
    }
}
