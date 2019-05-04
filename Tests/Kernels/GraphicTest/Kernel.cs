using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Drawing;
using Point = Cosmos.System.Graphics.Point;

/*
 * It is impossible to make assertions here but it is useful in any case to have it runs automatically
 * to catch stack overflows / corruptions when the CGS code is modified.
 */
namespace GraphicTest
{
    public class Kernel : Sys.Kernel
    {
        Canvas canvas;
        private Bitmap bitmap = new Bitmap(10, 10,
                new byte[] { 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0,
                    255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 23, 59, 88, 255,
                    23, 59, 88, 255, 0, 255, 243, 255, 0, 255, 243, 255, 23, 59, 88, 255, 23, 59, 88, 255, 0, 255, 243, 255, 0,
                    255, 243, 255, 0, 255, 243, 255, 23, 59, 88, 255, 153, 57, 12, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255,
                    243, 255, 0, 255, 243, 255, 153, 57, 12, 255, 23, 59, 88, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243,
                    255, 0, 255, 243, 255, 0, 255, 243, 255, 72, 72, 72, 255, 72, 72, 72, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0,
                    255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 72, 72,
                    72, 255, 72, 72, 72, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    10, 66, 148, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255,
                    243, 255, 10, 66, 148, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 10, 66, 148, 255, 10, 66, 148, 255,
                    10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255,
                    243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148, 255, 10, 66, 148,
                    255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255,
                    0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, 0, 255, 243, 255, }, ColorDepth.ColorDepth32);

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphic Mode");

            canvas = FullScreenCanvas.GetFullScreenCanvas();
        }

        private void DoTest (Canvas aCanvas)
        {
            mDebugger.Send($"Testing Canvas with mode {aCanvas.Mode}");
            aCanvas.Clear(Color.Blue);

            /* A red Point */
            Pen pen = new Pen(Color.Red);
            aCanvas.DrawPoint(pen, 69, 69);

            /* A GreenYellow horizontal line */
            pen.Color = Color.GreenYellow;
            aCanvas.DrawLine(pen, 250, 100, 400, 100);

            /* An IndianRed vertical line */
            pen.Color = Color.IndianRed;
            aCanvas.DrawLine(pen, 350, 150, 350, 250);

            /* A MintCream diagonal line */
            pen.Color = Color.MintCream;
            aCanvas.DrawLine(pen, 250, 150, 400, 250);

            /* Rectangles of various colors */
            pen.Color = Color.PaleVioletRed;
            aCanvas.DrawRectangle(pen, 350, 350, 80, 60);

            pen.Color = Color.Chartreuse;
            aCanvas.DrawCircle(pen, 69, 69, 10);

            pen.Color = Color.DimGray;
            aCanvas.DrawEllipse(pen, 100, 69, 10, 50);

            pen.Color = Color.MediumPurple;
            aCanvas.DrawPolygon(pen, new Point(200, 250), new Point(250, 300), new Point(220, 350), new Point(210, 275));

            /* A LimeGreen rectangle */
            pen.Color = Color.LimeGreen;
            aCanvas.DrawRectangle(pen, 450, 450, 80, 60);

            /* A filled rectange */
            pen.Color = Color.Chocolate;
            aCanvas.DrawFilledRectangle(pen, 200, 150, 400, 300);

            /* A Bitmpap image */
            aCanvas.DrawImage(bitmap, new Point(0, 0));

            mDebugger.Send($"Test of Canvas with mode {aCanvas.Mode} executed successfully");
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                /* First test with the DefaulMode */
                DoTest(canvas);

                DoTest(FullScreenCanvas.GetFullScreenCanvas(new Mode(800, 600, ColorDepth.ColorDepth32)));
                DoTest(FullScreenCanvas.GetFullScreenCanvas(new Mode(1024, 768, ColorDepth.ColorDepth32)));
                DoTest(FullScreenCanvas.GetFullScreenCanvas(new Mode(1366, 768, ColorDepth.ColorDepth32)));
                DoTest(FullScreenCanvas.GetFullScreenCanvas(new Mode(1280, 720, ColorDepth.ColorDepth32)));

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }
    }
}
