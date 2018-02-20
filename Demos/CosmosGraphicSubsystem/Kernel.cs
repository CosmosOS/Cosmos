using System;
using Cosmos.System.Graphics;
using Sys = Cosmos.System;

/*
 * Beware Demo Kernels are not recompiled when its dependencies changes!
 * To force recompilation right click on on the Cosmos icon of the demo solution and do "Build".
 */
namespace Cosmos_Graphic_Subsytem
{
    public class Kernel : Sys.Kernel
    {
        Canvas canvas;
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphic Mode");
            
            /* Get on instance of the Canvas that is all the Screen */
            canvas = FullScreenCanvas.GetFullScreenCanvas();

            /* Clear the Screen with the color 'Blue' */
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

                pen.Color = Color.LightSalmon;
                canvas.DrawEllipse(pen, 400, 300, 100, 150);

                pen.Color = Color.MediumPurple;
                canvas.DrawPolygon(pen, new Point(200, 250), new Point(250, 300), new Point(220, 350), new Point(210, 275));

                /*
                 * It will be really beautiful to do here:
                 * canvas.DrawString(pen, "Please press any key to continue the Demo...");
                 */
                Console.ReadKey();

                /* Let's try to change mode...*/
                canvas.Mode = new Mode(800, 600, ColorDepth.ColorDepth32);

                /* A LimeGreen rectangle */
                pen.Color = Color.LimeGreen;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                /* A filled rectange */
                pen.Color = Color.Chocolate;
                canvas.DrawFilledRectangle(pen, 200, 150, 400, 300);

                /*
                 * It will be really beautiful to do here:
                 * canvas.DrawString(pen, "Please press any key to end the Demo...");
                 */
                Console.ReadKey();

                Sys.Power.Shutdown();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Got fatal exception {e.Message}");
            }
        }
    }
}
