# Introduction

The Cosmos Graphic Subsystem (CGS from now on) is based on the abstraction of Canvas that is an empty space in which the user of CGS can draw its content. CGS is not a widget toolkit as Winforms or Gnome / GTK but is tough to be more lower level and it will be the basic in which widget toolkits will be implemented. CGS hides the graphics driver (so far VGA and VBE) used and it is tough to be the universal way to draw on the screen in Cosmos.

While Canvas could be overwritten (for example to create sub windows) the user of CGS does not have to deal with it directly but it must use the static class FullScreenCanvas.

Let's give a look to its API methods.
# FullScreenCanvas

    public static Canvas GetFullScreenCanvas(Mode mode) gets the instance of Canvas representing the complete screen (actually the instance of the running VGA driver) using the specified mode
    public static Canvas GetFullScreenCanvas() gets the instance of Canvas representing the complete screen (actually the instance of the running VGA driver) using the VGA driver's preferred mode

To really draw into the screen we need to use the Canvas class. Let's give a look to the API:
# Canvas
## List of Properties of the Canvas class

    Mode: get / set the mode of the video card to mode. It throws if the selected mode is not supported by the video card
    DefaultGraphicMode: default graphic mode this will change based on the underlying hardware
    AvailableModes: list of the available modes supported this will change based on the underlying hardware

## List of Methods of the Canvas class

    Clear(Color color: black) clear the entire Canvas using the specified color as background
    void DrawPoint(Pen pen, int x, int y) draws a point at the coordinates specified by x and y with the specified pen
    void DrawLine(Pen pen, int x_start, int y_start, int x_end, int y_end) draws a line at the coordinates specified by x_start, y_start and x_end, y_end with the specified pen
    void DrawRectangle(Pen pen, int x_start, int y_start,int width, int height) draws a rectangle specified by a coordinate pair, a width, and a height with the specified pen

Really simple right?
# A working example
```CSharp
using System;
using Sys = Cosmos.System;
using Cosmos.System.Graphics;

namespace GraphicTest
{
    public class Kernel : Sys.Kernel
    {
        Canvas canvas;

        protected override void BeforeRun()
        {
            /* If all works correctly you should not really see this :-) */
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphic Mode");

            canvas = FullScreenCanvas.GetFullScreenCanvas();

            canvas.Clear(Color.Blue);
        }

        protected override void Run()
        {
            try
            {
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

                Console.ReadKey();

                /* Let's try to change mode...*/
                canvas.Mode = new Mode(800, 600, ColorDepth.ColorDepth32);

                /* A LimeGreen rectangle */
                pen.Color = Color.LimeGreen;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                Console.ReadKey();

                Stop();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                Stop();
            }
        }
    }
}
```
# Limitation of the current implementation

1. Only the Bochs Graphic Adapter is actually supported; this means that CGS will work only on Bochs, QEMU and VirtualBox. CGS does not make any assumption on the underlying hardware, simple a VGA driver should be adapted to use it. No change to user code should be required to use another driver
2. Only 32 bit color depth is actually supported, the API provides methods to set a resolution with 24, 16, 8 and 4 bit but the low level Bochs driver has not yet implemented them
3. CGS does not permits yet to do basic operations that would permit to fulfill its promise to be the basic block from which a "Widget Toolkit" could be derived for example these methods should be added:
    DrawFilledRectangle((Pen pen, int x_start, int y_start,int width, int height)
    void DrawImage(Image image, int x, int y) Canvas has already defined this method but it is not yet implemented
    void DrawString(String string, Font font, Brush brush, int x, int y) Canvas has already defined this method but it is not yet implemented
    .Net System.Drawing has overload of these methods taking a float to evaluate if they are really needed for CGS, they are defined but for now no implementation is provided
    A double buffering strategy could be implemented to make it faster
    This is more a limitation of the Bochs driver when the Graphic Mode is set there is no more a way to return to text mode
    CGS / Graphic Mode interacts badly with the Kernel.Stop method: Bochs does not exit cleanly. You must use Sys.Power.Shutdown to shut down correctly your computer.
