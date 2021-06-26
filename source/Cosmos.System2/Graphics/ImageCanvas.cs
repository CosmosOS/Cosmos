using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class ImageCanvas : Canvas
    {
        // available modes
        public override List<Mode> AvailableModes { get; } = new List<Mode>();

        // default mode
        public override Mode DefaultGraphicMode { get; } = new Mode(32, 32, ColorDepth.ColorDepth32);

        // mode property
        private Mode mode;
        public override Mode Mode { get { return mode; } set { mode = value; } }

        /// <summary>
        /// Get and set position of canvas on screen
        /// </summary>
        public Point Position;

        /// <summary>
        /// Manipulatable image parent
        /// </summary>
        public Bitmap Image { get; private set; }

        /// <summary>
        /// Create new image canvas at specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mode"></param>
        public ImageCanvas(int x, int y, Mode mode)
        {
            this.Position = new Point(x, y);
            this.Mode = mode;
            this.Image = new Bitmap(mode.Width, mode.Height, mode.Depth);
        }

        /// <summary>
        /// Create new image canvas with existing image at specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="bitmap"></param>
        public ImageCanvas(int x, int y, Bitmap bitmap)
        {
            Position = new Point(x, y);
            Mode = new Mode(bitmap.Width, bitmap.Height, bitmap.Depth);
            Image = bitmap;
        }

        /// <summary>
        /// Clear image canvas
        /// </summary>
        /// <param name="color"></param>
        public override void Clear(Color color) { for (int i = 0; i < Mode.Width * Mode.Height; i++) { Image.RawData[i] = (uint)color.ToArgb(); } }

        /// <summary>
        /// Not used with image canvas - does nothing
        /// </summary>
        public override void Disable() { return; }

        /// <summary>
        /// Display canvas onto screen with default position
        /// </summary>
        public override void Display() { FullScreenCanvas.GetFullScreenCanvas().DrawImage(Image, Position.X, Position.Y); }

        /// <summary>
        /// Display canvas onto screen with specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Display(int x, int y) { Position.X = x; Position.Y = y; Display(); }

        /// <summary>
        /// Display canvas onto screen with specified position
        /// </summary>
        /// <param name="pos"></param>
        public void Display(Point pos) { Position.X = pos.X; Position.Y = pos.Y; Display(); }

        /// <summary>
        /// Draw point with color at specified position
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawPoint(Color color, int x, int y)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) {  return; }
            Image.RawData[x + (y * Mode.Width)] = (uint)color.ToArgb();
        }

        /// <summary>
        /// Get color at specified position on canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Color GetPointColor(int x, int y)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { return Color.Black; }
            return Color.FromArgb((int)Image.RawData[x + (y * Mode.Width)]);
        }
    }
}
