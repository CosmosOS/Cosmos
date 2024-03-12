using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers.Video.SVGAII;
using Cosmos.System.Graphics.Fonts;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Defines a VMWare SVGAII canvas implementation. Please note that this implementation
    /// of <see cref="Canvas"/> can only be used with virtualizers that do implement
    /// SVGAII, meaning that this class will not work on regular hardware.
    /// </summary>
    public class SVGAIICanvas : Canvas
    {
        internal Debugger debugger = new("SVGAIIScreen");
        static readonly Mode defaultMode = new(1024, 768, ColorDepth.ColorDepth32);

        private Mode mode;
        private readonly VMWareSVGAII driver;

        /// <summary>
        /// Initializes a new instance of the <see cref="SVGAIICanvas"/> class.
        /// </summary>
        public SVGAIICanvas()
            : this(defaultMode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SVGAIICanvas"/> class.
        /// </summary>
        /// <param name="aMode">The graphics mode.</param>
        public SVGAIICanvas(Mode aMode) : base(aMode)
        {
            debugger.SendInternal($"Called ctor with mode {aMode}");
            ThrowIfModeIsNotValid(aMode);

            driver = new VMWareSVGAII();
            Mode = aMode;
        }

        public override string Name() => "VMWareSVGAII";

        /// <summary>
        /// Get and set graphics mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">(set) Thrown if mode is not suppoted.</exception>
        public override Mode Mode
        {
            get => mode;
            set
            {
                mode = value;
                debugger.SendInternal($"Called Mode set property with mode {mode}");
                SetGraphicsMode(mode);
            }
        }

        public override Mode DefaultGraphicsMode => defaultMode;

        public override void Disable()
        {
            driver.Disable();
        }

        public override void DrawPoint(Color color, int x, int y)
        {
            if (color.A < 255)
            {
                if (color.A == 0)
                {
                    return;
                }

                color = AlphaBlend(color, GetPointColor(x, y), color.A);
            }

            driver.SetPixel((uint)x, (uint)y, (uint)color.ToArgb());
        }

        public override void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
        {
            var argb = color.ToArgb();
            var frameSize = (int)driver.FrameSize;

            // For now write directly into video memory, once _xSVGADriver.Fill will be faster it will have to be changed
            for (int i = yStart; i < yStart + height; i++)
            {
                driver.videoMemory.Fill(GetPointOffset(xStart, i) + (int)frameSize, width, argb);
            }
        }

        //public override IReadOnlyList<Mode> AvailableModes { get; } = new List<Mode>
        /// <summary>
        /// Available SVGA 2 supported video modes.
        /// <para>
        /// SD:
        /// <list type="bullet">
        /// <item>320x200x32.</item>
        /// <item>320x240x32.</item>
        /// <item>640x480x32.</item>
        /// <item>720x480x32.</item>
        /// <item>800x600x32.</item>
        /// <item>1024x768x32.</item>
        /// <item>1152x768x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HD:
        /// <list type="bullet">
        /// <item>1280x720x32.</item>
        /// <item>1280x768x32.</item>
        /// <item>1280x800x32.</item>
        /// <item>1280x1024x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDR:
        /// <list type="bullet">
        /// <item>1360x768x32.</item>
        /// <item>1366x768x32.</item>
        /// <item>1440x900x32.</item>
        /// <item>1400x1050x32.</item>
        /// <item>1600x1200x32.</item>
        /// <item>1680x1050x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDTV:
        /// <list type="bullet">
        /// <item>1920x1080x32.</item>
        /// <item>1920x1200x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// 2K:
        /// <list type="bullet">
        /// <item>2048x1536x32.</item>
        /// <item>2560x1080x32.</item>
        /// <item>2560x1600x32.</item>
        /// <item>2560x2048x32.</item>
        /// <item>3200x2048x32.</item>
        /// <item>3200x2400x32.</item>
        /// <item>3840x2400x32.</item>
        /// </list>
        /// </para>
        /// </summary>
        public override List<Mode> AvailableModes { get; } = new List<Mode>
        {
            /*  VmWare maybe supports 16 bit resolutions but CGS not yet (we should need to do RGB32->RGB16 conversion) */
#if false
            /* 16-bit Depth Resolutions*/

            /* SD Resolutions */
            new Mode(320, 200, ColorDepth.ColorDepth16),
            new Mode(320, 240, ColorDepth.ColorDepth16),
            new Mode(640, 480, ColorDepth.ColorDepth16),
            new Mode(720, 480, ColorDepth.ColorDepth16),
            new Mode(800, 600, ColorDepth.ColorDepth16),
            new Mode(1024, 768, ColorDepth.ColorDepth16),
            new Mode(1152, 768, ColorDepth.ColorDepth16),

            /* Old HD-Ready Resolutions */
            new Mode(1280, 720, ColorDepth.ColorDepth16),
            new Mode(1280, 768, ColorDepth.ColorDepth16),
            new Mode(1280, 800, ColorDepth.ColorDepth16),  // WXGA
            new Mode(1280, 1024, ColorDepth.ColorDepth16), // SXGA

            /* Better HD-Ready Resolutions */
            new Mode(1360, 768, ColorDepth.ColorDepth16),
            new Mode(1366, 768, ColorDepth.ColorDepth16),  // Original Laptop Resolution
            new Mode(1440, 900, ColorDepth.ColorDepth16),  // WXGA+
            new Mode(1400, 1050, ColorDepth.ColorDepth16), // SXGA+
            new Mode(1600, 1200, ColorDepth.ColorDepth16), // UXGA
            new Mode(1680, 1050, ColorDepth.ColorDepth16), // WXGA++

            /* HDTV Resolutions */
            new Mode(1920, 1080, ColorDepth.ColorDepth16),
            new Mode(1920, 1200, ColorDepth.ColorDepth16), // WUXGA

            /* 2K Resolutions */
            new Mode(2048, 1536, ColorDepth.ColorDepth16), // QXGA
            new Mode(2560, 1080, ColorDepth.ColorDepth16), // UW-UXGA
            new Mode(2560, 1600, ColorDepth.ColorDepth16), // WQXGA
            new Mode(2560, 2048, ColorDepth.ColorDepth16), // QXGA+
            new Mode(3200, 2048, ColorDepth.ColorDepth16), // WQXGA+
            new Mode(3200, 2400, ColorDepth.ColorDepth16), // QUXGA
            new Mode(3840, 2400, ColorDepth.ColorDepth16), // WQUXGA
#endif
            /* 32-bit Depth Resolutions*/
            /* SD Resolutions */
                new Mode(320, 200, ColorDepth.ColorDepth32),
                new Mode(320, 240, ColorDepth.ColorDepth32),
                new Mode(640, 480, ColorDepth.ColorDepth32),
                new Mode(720, 480, ColorDepth.ColorDepth32),
                new Mode(800, 600, ColorDepth.ColorDepth32),
                new Mode(1024, 768, ColorDepth.ColorDepth32),
                new Mode(1152, 768, ColorDepth.ColorDepth32),

                /* Old HD-Ready Resolutions */
                new Mode(1280, 720, ColorDepth.ColorDepth32),
                new Mode(1280, 768, ColorDepth.ColorDepth32),
                new Mode(1280, 800, ColorDepth.ColorDepth32),  // WXGA
                new Mode(1280, 1024, ColorDepth.ColorDepth32), // SXGA

                /* Better HD-Ready Resolutions */
                new Mode(1360, 768, ColorDepth.ColorDepth32),
                //new Mode(1366, 768, ColorDepth.ColorDepth32),  // Original Laptop Resolution - this one is for some reason broken in vmware
                new Mode(1440, 900, ColorDepth.ColorDepth32),  // WXGA+
                new Mode(1400, 1050, ColorDepth.ColorDepth32), // SXGA+
                new Mode(1600, 1200, ColorDepth.ColorDepth32), // UXGA
                new Mode(1680, 1050, ColorDepth.ColorDepth32), // WXGA++

                /* HDTV Resolutions */
                new Mode(1920, 1080, ColorDepth.ColorDepth32),
                new Mode(1920, 1200, ColorDepth.ColorDepth32), // WUXGA

                /* 2K Resolutions */
                new Mode(2048, 1536, ColorDepth.ColorDepth32), // QXGA
                new Mode(2560, 1080, ColorDepth.ColorDepth32), // UW-UXGA
                new Mode(2560, 1600, ColorDepth.ColorDepth32), // WQXGA
                new Mode(2560, 2048, ColorDepth.ColorDepth32), // QXGA+
                new Mode(3200, 2048, ColorDepth.ColorDepth32), // WQXGA+
                new Mode(3200, 2400, ColorDepth.ColorDepth32), // QUXGA
                new Mode(3840, 2400, ColorDepth.ColorDepth32), // WQUXGA
        };

        /// <summary>
        /// Sets the graphics mode to the specified value.
        /// </summary>
        private void SetGraphicsMode(Mode mode)
        {
            ThrowIfModeIsNotValid(mode);

            var width = (uint)mode.Width;
            var height = (uint)mode.Height;
            var colorDepth = (uint)mode.ColorDepth;

            driver.SetMode(width, height, colorDepth);
        }

        public override void Clear(int color)
        {
            driver.Clear((uint)color);
        }

        public override void Clear(Color color)
        {
            driver.Clear((uint)color.ToArgb());
        }

        public Color GetPixel(int x, int y)
        {
            var argb = driver.GetPixel((uint)x, (uint)y);
            return Color.FromArgb((int)argb);
        }

        /// <summary>
        /// Sets the state of the cursor.
        /// </summary>
        /// <param name="visible">Whether the cursor should be visible.</param>
        /// <param name="x">The X coordinate of the cursor.</param>
        /// <param name="y">The Y coordinate of the cursor.</param>
        public void SetCursor(bool visible, int x, int y)
        {
            driver.SetCursor(visible, (uint)x, (uint)y);
        }

        /// <summary>
        /// Creates the hardware cursor.
        /// </summary>
        public void CreateCursor()
        {
            driver.DefineCursor();
        }

        /// <summary>
        /// Performs a bit blit operation, copying pixels from one region
        /// to another.
        /// </summary>
        /// <param name="srcX">The source X coordinate.</param>
        /// <param name="srcY">The source Y coordinate.</param>
        /// <param name="dstX">The destination X coordinate.</param>
        /// <param name="dstY">The destination Y coordinate.</param>
        /// <param name="width">The width of the region.</param>
        /// <param name="height">The height of the region.</param>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectangle copy capability</exception>
        public void CopyPixels(int srcX, int srcY, int dstX, int dstY, int width = 1, int height = 1)
        {
            driver.Copy((uint)srcX, (uint)srcY, (uint)dstX, (uint)dstY, (uint)width, (uint)height);
        }

        /// <summary>
        /// Moves a single pixel.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="newX">The new X coordinate.</param>
        /// <param name="newY">The new Y coordinate.</param>
        public void MovePixel(int x, int y, int newX, int newY)
        {
            driver.Copy((uint)x, (uint)y, (uint)newX, (uint)newY, 1, 1);
            driver.SetPixel((uint)x, (uint)y, 0);
        }

        public override Color GetPointColor(int x, int y)
        {
            return Color.FromArgb((int)driver.GetPixel((uint)x, (uint)y));
        }

        public override void Display()
        {
            driver.DoubleBufferUpdate();
        }

        public override void DrawString(string str, Font font, Color color, int x, int y)
        {
            var len = str.Length;
            var width = font.Width;

            for (int i = 0; i < len; i++)
            {
                DrawChar(str[i], font, color, x, y);
                x += width;
            }
        }

        public override void DrawChar(char c, Font font, Color color, int x, int y)
        {
            var height = font.Height;
            var width = font.Width;
            var data = font.Data;
            int p = height * (byte)c;

            for (int cy = 0; cy < height; cy++)
            {
                for (byte cx = 0; cx < width; cx++)
                {
                    if (font.ConvertByteToBitAddress(data[p + cy], cx + 1))
                    {
                        DrawPoint(color, (ushort)(x + cx), (ushort)(y + cy));
                    }
                }
            }
        }

        public override void DrawImage(Image image, int x, int y)
        {
            var width = (int)image.Width;
            var height = (int)image.Height;
            var frameSize = (int)driver.FrameSize;
            var data = image.RawData;

            for (int i = 0; i < height; i++)
            {
                driver.videoMemory.Copy(GetPointOffset(x, y + i) + frameSize, data, i * width, width);
            }
        }
    }
}
