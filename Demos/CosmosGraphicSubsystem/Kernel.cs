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
        private Canvas canvas;
        private Bitmap bitmap;

        Bitmap _mytestBmp;
        PixelFarm.CpuBlit.ActualBitmap _actualBmp;


        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's go in Graphic Mode");
            Console.WriteLine("Using default graphics mode");
            //Mode start = new Mode(800, 600, ColorDepth.ColorDepth32);

            bitmap = new Bitmap(10, 10,
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



            //--------------------------
            _actualBmp = new PixelFarm.CpuBlit.ActualBitmap(100, 100);
            int[] rawBmp = PixelFarm.CpuBlit.ActualBitmap.GetBuffer(_actualBmp);
            var painter = PixelFarm.CpuBlit.AggPainter.Create(_actualBmp);

            painter.FillColor = PixelFarm.Drawing.Color.Red;
            painter.StrokeColor = PixelFarm.Drawing.Color.Transparent;
            painter.StrokeWidth = 1;//svg standard, init stroke-width =1
            painter.Clear(PixelFarm.Drawing.Color.Black);
            painter.RenderQuality = PixelFarm.Drawing.RenderQualtity.HighQuality;
            //--------------------------             
            {
                PixelFarm.Drawing.VertexStore vx = new PixelFarm.Drawing.VertexStore();
                vx.AddVertex(0, 0, PixelFarm.CpuBlit.VertexCmd.MoveTo);
                vx.AddVertex(0, 30, PixelFarm.CpuBlit.VertexCmd.LineTo);
                vx.AddVertex(50, 80, PixelFarm.CpuBlit.VertexCmd.LineTo);
                vx.AddVertex(25, 10, PixelFarm.CpuBlit.VertexCmd.LineTo);
                vx.AddVertex(0, 0, PixelFarm.CpuBlit.VertexCmd.Close);
                painter.Fill(vx);
            }

            {
                PixelFarm.Drawing.VertexStore vx = new PixelFarm.Drawing.VertexStore();
                vx.AddVertex(0 + 15, 0, PixelFarm.CpuBlit.VertexCmd.MoveTo);
                vx.AddVertex(0 + 15, 30, PixelFarm.CpuBlit.VertexCmd.LineTo);
                vx.AddVertex(50 + 15, 80, PixelFarm.CpuBlit.VertexCmd.LineTo);
                vx.AddVertex(25 + 15, 10, PixelFarm.CpuBlit.VertexCmd.LineTo);
                vx.AddVertex(0, 0, PixelFarm.CpuBlit.VertexCmd.Close);
                painter.FillColor = PixelFarm.Drawing.Color.FromArgb(180, PixelFarm.Drawing.Color.Yellow);
                painter.Fill(vx);
            }
            //--------------------------
            //painter.StrokeColor = PixelFarm.Drawing.Color.Transparent;
            //var svgLion = PixelFarm.CpuBlit.Rasterization.SampleData.GetLionSvg();
            //svgLion.Render(painter);

            //--------------------------
            //int bmpLen = rawBmp.Length;
            //int colorInt = (int)Color.FromArgb(120, Color.Red).ToARGB();
            //for (int i = 0; i < bmpLen; ++i)
            //{
            //    rawBmp[i] = colorInt;
            //} 
            _mytestBmp = new Bitmap(100, 100, rawBmp, ColorDepth.ColorDepth32);

            Console.WriteLine("Press any key to continue!");
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
                Cosmos.System.Graphics.Pen pen = new Cosmos.System.Graphics.Pen(Color.Red);
                canvas.DrawPoint(pen, 69, 69);



                /* A GreenYellow horizontal line */
                pen.Color = Color.Gray;
                canvas.DrawLine(pen, 250, 100, 400, 100);

                /* An IndianRed vertical line */
                pen.Color = Color.Red;
                canvas.DrawLine(pen, 350, 150, 350, 250);

                /* A MintCream diagonal line */
                pen.Color = Color.Green;
                canvas.DrawLine(pen, 250, 150, 400, 250);

                /* A PaleVioletRed rectangle */
                pen.Color = Color.Blue;
                canvas.DrawRectangle(pen, 350, 350, 80, 60);

                pen.Color = Color.Red;
                canvas.DrawCircle(pen, 69, 69, 10);

                pen.Color = Color.OrangeRed;
                canvas.DrawEllipse(pen, 400, 300, 100, 150);

                pen.Color = Color.Magenta;
                canvas.DrawPolygon(pen, new Point(200, 250), new Point(250, 300), new Point(220, 350), new Point(210, 275));

                canvas.DrawImage(bitmap, new Point(20, 20));

                /*
                 * It will be really beautiful to do here:
                 * canvas.DrawString(pen, "Please press any key to continue the Demo...");
                 */
                Console.ReadKey();

                /* Let's try to change mode...*/
                canvas.Mode = new Mode(800, 600, ColorDepth.ColorDepth32);

                //If the background is not redrawn, it gets corrupted
                canvas.Clear(Color.White);

                /* A LimeGreen rectangle */
                pen.Color = Color.Green;
                canvas.DrawRectangle(pen, 450, 450, 80, 60);

                /* A filled rectange */
                pen.Color = Color.Red;
                canvas.DrawFilledRectangle(pen, 200, 150, 400, 300);

                /*
                 * It will be really beautiful to do here:
                 * canvas.DrawString(pen, "Please press any key to end the Demo...");
                 */
                Console.ReadKey();



                canvas.DrawImage(_mytestBmp, 0, 0, 100, 100, 0, 0);

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
