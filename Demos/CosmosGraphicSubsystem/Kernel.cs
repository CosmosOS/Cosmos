using System;
using Cosmos.System.Graphics;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;

/*
 * Beware Demo Kernels are not recompiled when its dependencies changes!
 * To force recompilation right click on on the Cosmos icon of the demo solution and do "Build".
 */
namespace Cosmos_Graphic_Subsytem
{
    public class Kernel : Sys.Kernel
    {
        public static Cosmos.HAL.Mouse m = new Cosmos.HAL.Mouse();
        public Debugger debugger = new Debugger("System", "CGS");
        Canvas canvas;
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphic Mode");
            Console.WriteLine("Using default graphics mode");
            //Mode start = new Mode(800, 600, ColorDepth.ColorDepth32);

            Console.WriteLine("Here we go!");
            Console.ReadKey(true);
            // Create new instance of FullScreenCanvas, using default graphics mode
            canvas = FullScreenCanvas.GetFullScreenCanvas();    // canvas = GetFullScreenCanvas(start);
            
            /* Clear the Screen with the color 'Blue' */
            canvas.Clear(Color.Blue);
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                /* A red Point */
                var pen = new Pen(Color.Red);
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

                pen.Color = Color.LightSalmon;
                canvas.DrawFilledEllipse(pen, 400, 300, 100, 150);

                /*
                 * It will be really beautiful to do here:
                 * canvas.DrawString(pen, "Please press any key to continue the Demo...");
                 */
                Console.ReadKey();
                /*
                // Throws a NotImplementedException in VMWare,
                // due to being unable to escape from the clutches of the SVGA II driver...
                //
                // Probably best to leave this commented out until it can work in VMWare as well
                
                canvas.Disable();
                Console.Clear();
                Console.WriteLine("If it worked, you've successfully returned back to standard VGA Text Mode!");
                Console.WriteLine("Let's try returning back to CGS mode... Press any key!");
                Console.ReadKey(true);
                */

                /* Let's try to change mode...*/
                canvas.Mode = new Mode(1024, 768, ColorDepth.ColorDepth32);
                canvas.Clear(Color.Blue);
                /* A LimeGreen rectangle */
                pen.Color = Color.LimeGreen;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                /* A filled rectange */
                pen.Color = Color.Chocolate;
                canvas.DrawFilledRectangle(pen, 200, 150, 400, 300);

                pen.Color = Color.Aquamarine;
                canvas.DrawFilledRectangle(pen, 0, 0, 1024, 150);

                pen.Color = Color.Blue;
                canvas.DrawFilledCircle(pen, 69, 69, 10);
                /*
                 * It will be really beautiful to do here:
                 * canvas.DrawString(pen, "Please press any key to end the Demo...");
                 */
                Console.ReadKey();

                Sys.Power.Shutdown();
            }
            catch (Exception e)
            {
                debugger.Send($"Got fatal exception {e.Message}");
                Console.WriteLine($"Got fatal exception {e.Message}");
            }
        }
    }
}
