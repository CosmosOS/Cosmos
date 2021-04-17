using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics
{
    public class ImageCanvas : Canvas
    {
        /// <summary>
        /// This property does not apply to the image canvas - does nothing
        /// </summary>
        public override List<Mode> AvailableModes { get; } = new List<Mode>();

        /// <summary>
        /// Default mode of image(32x32x32)
        /// </summary>
        public override Mode DefaultGraphicMode { get { return defaultMode; } }
        private Mode defaultMode = new Mode(32, 32, ColorDepth.ColorDepth32);

        /// <summary>
        /// Raw color data of image
        /// </summary>
        public Color[] RawData { get; private set; }

        /// <summary>
        /// Active mode
        /// </summary>
        public int Width { get { return Mode.Columns; } }
        public int Height { get { return Mode.Rows; } }
        public override Mode Mode { get { return mode; } set { mode = value; } }
        private Mode mode;

        // stores position of drawn image
        private Point drawPoint = new Point(0, 0);

        /// <summary>
        /// Create a new image canvas with default properties(32x32x32)
        /// </summary>
        public ImageCanvas()
        {
            // set mode and create image
            mode = defaultMode;
            CreateImage(mode.Columns, mode.Rows, mode.ColorDepth);
        }

        /// <summary>
        /// Create a new image canvas with specified width, height, and depth
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="depth"></param>
        public ImageCanvas(int w, int h, ColorDepth depth)
        {
            // set mode and create image
            mode = new Mode(w, h, depth);
            CreateImage(w, h, depth);

        }

        public override void Clear(Color color)
        {
            for (int i = 0; i < Width * Height; i++) { RawData[i] = color; }
        }

        // create image
        private void CreateImage(int w, int h, ColorDepth depth)
        {
            RawData = new Color[w * h];
            Clear(Color.Black);
        }

        // resize image
        private Color[] tempColors;
        public void Resize(int newWidth, int newHeight)
        {
            // resize image in seperate array
            int w1 = (int)Width;
            int h1 = (int)Height;
            tempColors = new Color[newWidth * newHeight];
            int x_ratio = (int)((w1 << 16) / newWidth) + 1;
            int y_ratio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;
            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    tempColors[(i * newWidth) + j] = RawData[(y2 * w1) + x2];
                }
            }

            // copy new data back to image
            CreateImage(newWidth, newHeight, mode.ColorDepth);
            for (int i = 0; i < newWidth * newHeight; i++) { RawData[i] = tempColors[i]; }
            tempColors = null;
        }

        /// <summary>
        /// This function does not apply to the image canvas and does nothing
        /// </summary>
        public override void Disable() { }

        /// <summary>
        /// Display the image canvas onto active fullscreen canvas
        /// </summary>
        public override void Display()
        {
            drawPoint.X = 0; drawPoint.Y = 0;
            FullScreenCanvas.GetFullScreenCanvas().DrawArray(RawData, drawPoint, mode.Columns, mode.Rows);
        }

        /// <summary>
        /// Display the image canvas onto active fullscreen canvas with specified position
        /// </summary>
        public void Display(int x, int y)
        {
            drawPoint.X = x; drawPoint.Y = y;
            FullScreenCanvas.GetFullScreenCanvas().DrawArray(RawData, drawPoint, mode.Columns, mode.Rows);
        }

        /// <summary>
        /// Display the image canvas onto an existing canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="parent"></param>
        public void Display(int x, int y, Canvas parent)
        {
            drawPoint.X = x; drawPoint.Y = y;
            parent.DrawArray(RawData, drawPoint, mode.Columns, Mode.Rows);
        }

        /// <summary>
        /// Draw an array of colors onto the image canvas
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            for (int i = 0; i < width * height; i++)
            {
                // get real position
                int xx = x + (i % width);
                int yy = y + (i / width);
                // copy data
                RawData[xx + (yy * Width)] = colors[i];
            }
        }

        /// <summary>
        /// Draw a single point onto the image canvas
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawPoint(Pen pen, int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return; }
            RawData[x + (y * Width)] = pen.Color;
        }

        /// <summary>
        /// Draw a single point onto the image canvas with floating points
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawPoint(Pen pen, float x, float y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return; }
            RawData[(int)x + ((int)y * Width)] = pen.Color;
        }

        /// <summary>
        /// Return color at specified point of image canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Color GetPointColor(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return Color.Black; }
            return RawData[x + (y * Width)];
        }
    }
}
