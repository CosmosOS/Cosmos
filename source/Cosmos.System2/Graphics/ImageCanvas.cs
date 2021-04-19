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
        private Bitmap image;
        public Bitmap Image { get { return image; } set
            {
                Image = value;
                Mode = new Mode(value.Width, value.Height, value.Depth);
            } }

        /// <summary>
        /// Active mode
        /// </summary>
        public override Mode Mode { get { return mode; } set
            {
                mode = value;
            } }
        private Mode mode;

        /// <summary>
        /// Create a new image canvas with default properties(32x32x32)
        /// </summary>
        public ImageCanvas()
        {
            // set mode and create image
            Mode = DefaultGraphicMode;
            CreateImage(mode.Width, mode.Height, mode.Depth);
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
            Mode = new Mode(w, h, depth);
            CreateImage(w, h, depth);
        }

        /// <summary>
        /// Create a new image canvas with existing bitmap
        /// </summary>
        /// <param name="image"></param>
        public ImageCanvas(Bitmap image) { this.Image = image; }

        public override void Clear(Color color)
        {
            for (int i = 0; i < Mode.Width * Mode.Height; i++) { image.SetPixel(i % Mode.Width, i / Mode.Width, color); }
        }

        // create image
        private void CreateImage(int w, int h, ColorDepth depth)
        {
            Image = new Bitmap(w, h, depth);
        }

        // resize image
        private uint[] tempColors;
        public void Resize(int newWidth, int newHeight)
        {
            // resize image in seperate array
            int w1 = (int)Mode.Width;
            int h1 = (int)Mode.Height;
            tempColors = new uint[newWidth * newHeight];
            int x_ratio = (int)((w1 << 16) / newWidth) + 1;
            int y_ratio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;
            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    tempColors[(i * newWidth) + j] = Image.RawData[(y2 * w1) + x2];
                }
            }

            // copy new data back to image
            CreateImage(newWidth, newHeight, Mode.Depth);
            for (int i = 0; i < newWidth * newHeight; i++) { Image.RawData[i] = tempColors[i]; }
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
            FullScreenCanvas.GetFullScreenCanvas().DrawArray(0, 0, Mode.Width, Mode.Height, Image.RawData);
        }

        /// <summary>
        /// Display the image canvas onto active fullscreen canvas with specified position
        /// </summary>
        public void Display(int x, int y)
        {
            FullScreenCanvas.GetFullScreenCanvas().DrawArray(x, y, Mode.Width, Mode.Height, Image.RawData);
        }

        /// <summary>
        /// Display the image canvas onto an existing canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="parent"></param>
        public void Display(int x, int y, Canvas parent)
        {
            parent.DrawArray(x, y, Mode.Width, Mode.Height, Image.RawData);
        }

        /// <summary>
        /// Draw point at specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public override void DrawPoint(int x, int y, Color color) { Image.SetPixel(x, y, color); }

        /// <summary>
        /// Return color at specified point of image canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Color GetPointColor(int x, int y) { return Image.GetPixel(x, y); }
    }
}
