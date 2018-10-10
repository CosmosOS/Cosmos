using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Drawing;
using Point = Cosmos.System.Graphics.Point;

/*
 * Please note this is an atypical TestRunner:
 * - no Assertion can be done
 * - it cannot be executed automatically
 *
 * it exists to make easier tests while changing low level stuff (it would be better and faster to use the Demo kernel but
 * sometimes it is a problem to make it see modifications done at low level)
 *
 * Remember to comment this test again on TestKernelSets.cs when you are ready to merge your modifications!
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

            canvas.Clear(Color.Blue);
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                /* A red Point */
                Pen pen = new Pen(Color.Red);
                canvas.DrawPoint(pen, 69, 69);

                /* A GreenYellow horizontal line */
                pen.Color = Color.GreenYellow;
                canvas.DrawLine(pen, 250, 100, 400, 100);

                /* An IndianRed vertical line */
                pen.Color = Color.IndianRed;
                canvas.DrawLine(pen, 350, 150, 350, 250);

                /* A MintCream diagonal line */
                pen.Color = Color.MintCream;
                canvas.DrawLine(pen, 250, 150, 400, 250);

                /* A PaleVioletRed rectangle */
                pen.Color = Color.PaleVioletRed;
                canvas.DrawRectangle(pen, 350, 350, 80, 60);

                pen.Color = Color.Chartreuse;
                canvas.DrawCircle(pen, 69, 69, 10);

                pen.Color = Color.DimGray;
                canvas.DrawEllipse(pen, 100, 69, 10, 50);

                pen.Color = Color.MediumPurple;
                canvas.DrawPolygon(pen, new Point(200, 250), new Point(250, 300), new Point(220, 350), new Point(210, 275));

                Console.ReadKey();

                /* Let's try to change mode...*/
                canvas.Mode = new Mode(800, 600, ColorDepth.ColorDepth32);

                //If the background is not redrawn, it gets corrupted (this happens more in VmWare)
                canvas.Clear(Color.Blue);

                /* A LimeGreen rectangle */
                pen.Color = Color.LimeGreen;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                /* A filled rectange */
                pen.Color = Color.Chocolate;
                canvas.DrawFilledRectangle(pen, 200, 150, 400, 300);

                canvas.DrawImage(bitmap, new Point(0, 0));

                Console.ReadKey();

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
