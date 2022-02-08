# Introduction

The Cosmos Graphic Subsystem (CGS from now on) is based on the abstraction of Canvas that is an empty space in which the user of CGS can draw its content. CGS is not a widget toolkit as Winforms or Gnome / GTK but is thought to be more lower level and it will be the basic in which widget toolkits will be implemented. CGS hides the graphics driver (so far VGA, VBE and SVGA II) used and it is thought to be the universal way to draw on the screen in Cosmos.

While Canvas could be overwritten (for example to create sub windows) the user of CGS does not have to deal with it directly but they must use the static class FullScreenCanvas.

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
    void DrawImage(Image image, int x, int y) draws an image at the x and y specified
    void DrawString(String string, Font font, Brush brush, int x, int y) draws a string with the specified font and brush at the specified x and y coordinates
    void Display() only for double buffering, swaps the 2 buffers then display everything to the screen


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

        private readonly Bitmap bitmap = new Bitmap(10, 10,
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
            // If all works correctly you should not really see this :-)
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphical Mode");

            // You don't have to specify the Mode, but here we do to show that you can.
            canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(640, 480, ColorDepth.ColorDepth32));
            canvas.Clear(Color.Blue);
        }

        protected override void Run()
        {
            try
            {
                Pen pen = new Pen(Color.Red);

                // A red Point
                canvas.DrawPoint(pen, 69, 69);

                // A GreenYellow horizontal line
                pen.Color = Color.GreenYellow;
                canvas.DrawLine(pen, 250, 100, 400, 100);

                // An IndianRed vertical line
                pen.Color = Color.IndianRed;
                canvas.DrawLine(pen, 350, 150, 350, 250);

                // A MintCream diagonal line
                pen.Color = Color.MintCream;
                canvas.DrawLine(pen, 250, 150, 400, 250);

                // A PaleVioletRed rectangle
                pen.Color = Color.PaleVioletRed;
                canvas.DrawRectangle(pen, 350, 350, 80, 60);

                // A LimeGreen rectangle
                pen.Color = Color.LimeGreen;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                // A bitmap
                canvas.DrawImage(bitmap, new Point(100, 150));

                Console.ReadKey();
                Sys.Power.Shutdown();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                Sys.Power.Shutdown();
            }
        }
    }
}
```
# Limitations of the current implementation

1. Only 32 bit color depth is actually supported, the API provides methods to set a resolution with 24, 16, 8 and 4 bit but the low level Bochs driver has not yet implemented them.

2. In addition, some other nice things could be implemented:
    - Plugging System.Drawing functions for easier manipulation of colors
    - A double buffering strategy, to make drawing faster (one is already implemented in the VBE driver, but not VGA or SVGA II)

3. CGS interacts badly with the Kernel.Stop method: the screen will freeze without displaying any error message whatsoever. You must use the Sys.Power.Shutdown() function to properly shut down your computer.
