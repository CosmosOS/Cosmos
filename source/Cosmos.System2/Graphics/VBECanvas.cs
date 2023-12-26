using System.Collections.Generic;
using Cosmos.HAL.Drivers.Video;
using Cosmos.Core.Multiboot;
using System.Drawing;
using System;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Defines a VBE (VESA Bios Extensions) canvas implementation. Please note
    /// that this implementation of <see cref="Canvas"/> only works on BIOS
    /// implementations, meaning that it is not available on UEFI systems.
    /// </summary>
    public class VBECanvas : Canvas
    {
        static readonly Mode defaultMode = new(1024, 768, ColorDepth.ColorDepth32);
        readonly VBEDriver driver;
        Mode mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="VBECanvas"/> class.
        /// </summary>
        public VBECanvas() : this(defaultMode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VBECanvas"/> class.
        /// </summary>
        /// <param name="mode">The display mode to use.</param>
        public unsafe VBECanvas(Mode mode) : base(mode)
        {
            if (Multiboot2.IsVBEAvailable)
            {
                mode = new(Multiboot2.Framebuffer->Width, Multiboot2.Framebuffer->Height, (ColorDepth)Multiboot2.Framebuffer->Bpp);
            }

            ThrowIfModeIsNotValid(mode);

            driver = new VBEDriver((ushort)mode.Width, (ushort)mode.Height, (ushort)mode.ColorDepth);
            Mode = mode;
        }

        public override void Disable()
        {
            driver.DisableDisplay();
        }

        public override string Name() => "VBECanvas";

        public override Mode Mode
        {
            get => mode;
            set
            {
                mode = value;
                SetMode(mode);
            }
        }

        #region Display
        /// <summary>
        /// Available VBE supported video modes.
        /// <para>
        /// Low res:
        /// <list type="bullet">
        /// <item>320x240x32.</item>
        /// <item>640x480x32.</item>
        /// <item>800x600x32.</item>
        /// <item>1024x768x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HD:
        /// <list type="bullet">
        /// <item>1280x720x32.</item>
        /// <item>1280x1024x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDR:
        /// <list type="bullet">
        /// <item>1366x768x32.</item>
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
        /// </summary>
        public override List<Mode> AvailableModes { get; } = new()
        {
            new Mode(320, 240, ColorDepth.ColorDepth32),
            new Mode(640, 480, ColorDepth.ColorDepth32),
            new Mode(800, 600, ColorDepth.ColorDepth32),
            new Mode(1024, 768, ColorDepth.ColorDepth32),
            /* The so called HD-Ready resolution */
            new Mode(1280, 720, ColorDepth.ColorDepth32),
            new Mode(1280, 768, ColorDepth.ColorDepth32),
            new Mode(1280, 1024, ColorDepth.ColorDepth32),
            /* A lot of HD-Ready screen uses this instead of 1280x720 */
            new Mode(1366, 768, ColorDepth.ColorDepth32),
            new Mode(1680, 1050, ColorDepth.ColorDepth32),
            /* HDTV resolution */
            new Mode(1920, 1080, ColorDepth.ColorDepth32),
            /* HDTV resolution (16:10 AR) */
            new Mode(1920, 1200, ColorDepth.ColorDepth32),
        };

        public override Mode DefaultGraphicsMode => defaultMode;

        /// <summary>
        /// Sets the used display mode, disabling text mode if it is active.
        /// </summary>
        private void SetMode(Mode mode)
        {
            ThrowIfModeIsNotValid(mode);

            ushort xres = (ushort)Mode.Width;
            ushort yres = (ushort)Mode.Height;
            ushort bpp = (ushort)Mode.ColorDepth;

            driver.VBESet(xres, yres, bpp);
        }
        #endregion

        #region Drawing

        public override void Clear(int aColor)
        {
            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            switch (mode.ColorDepth)
            {
                case ColorDepth.ColorDepth4:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth8:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth16:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth24:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth32:
                    driver.ClearVRAM((uint)aColor);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Clear(Color aColor)
        {
            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            switch (mode.ColorDepth)
            {
                case ColorDepth.ColorDepth4:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth8:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth16:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth24:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth32:
                    driver.ClearVRAM((uint)aColor.ToArgb());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /*
         * As DrawPoint() is the basic block of DrawLine() and DrawRect() and in theory of all the future other methods that will
         * be implemented is better to not check the validity of the arguments here or it will repeat the check for any point
         * to be drawn slowing down all.
         */
        public override void DrawPoint(Color aColor, int aX, int aY)
        {
            uint offset;

            /*
             * For now we can Draw only if the ColorDepth is 32 bit, we will throw otherwise.
             *
             * How to support other ColorDepth? The offset calculation should be the same (and so could be done out of the switch)
             * ColorDepth.ColorDepth16 and ColorDepth.ColorDepth8 need a conversion from color (an ARGB32 color) to the RGB16 and RGB8
             * how to do this conversion faster maybe using pre-computed tables? What happens if the color cannot be converted? We will throw?
             */
            switch (Mode.ColorDepth)
            {
                case ColorDepth.ColorDepth32:

                    offset = (uint)GetPointOffset(aX, aY);

                    if (aColor.A < 255)
                    {
                        if (aColor.A == 0)
                        {
                            return;
                        }

                        aColor = AlphaBlend(aColor, GetPointColor(aX, aY), aColor.A);
                    }

                    driver.SetVRAM(offset, aColor.B);
                    driver.SetVRAM(offset + 1, aColor.G);
                    driver.SetVRAM(offset + 2, aColor.R);
                    driver.SetVRAM(offset + 3, aColor.A);

                    break;
                case ColorDepth.ColorDepth24:

                    offset = (uint)GetPointOffset(aX, aY);

                    driver.SetVRAM(offset, aColor.B);
                    driver.SetVRAM(offset + 1, aColor.G);
                    driver.SetVRAM(offset + 2, aColor.R);

                    break;
                default:
                    throw new NotImplementedException("Drawing pixels with color depth " + (int)Mode.ColorDepth + "is not yet supported.");
            }
        }

        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
        {
            ThrowIfCoordNotValid(aX, aY);
            ThrowIfCoordNotValid(aX + aWidth, aY + aHeight);

            for (int i = 0; i < aX; i++)
            {
                for (int ii = 0; ii < aY; ii++)
                {
                    DrawPoint(aColors[i + (ii * aWidth)], i, ii);
                }
            }
        }

        public override void DrawFilledRectangle(Color aColor, int aX, int aY, int aWidth, int aHeight)
        {
            // ClearVRAM clears one uint at a time. So we clear pixelwise not byte wise. That's why we divide by 32 and not 8.
            aWidth = (int)(Math.Min(aWidth, Mode.Width - aX) * (int)Mode.ColorDepth / 32);
            var color = aColor.ToArgb();

            for (int i = aY; i < aY + aHeight; i++)
            {
                driver.ClearVRAM(GetPointOffset(aX, i), aWidth, color);
            }
        }

        public override void DrawImage(Image aImage, int aX, int aY)
        {
            var xBitmap = aImage.RawData;
            var xWidth = (int)aImage.Width;
            var xHeight = (int)aImage.Height;

            int xOffset = GetPointOffset(aX, aY);

            for (int i = 0; i < xHeight; i++)
            {
                driver.CopyVRAM((i * (int)Mode.Width) + xOffset, xBitmap, i * xWidth, xWidth);
            }
        }

        #endregion

        public override void Display()
        {
            driver.Swap();
        }

        #region Reading

        public override Color GetPointColor(int aX, int aY)
        {
            uint offset = (uint)GetPointOffset(aX, aY);
            return Color.FromArgb((int)driver.GetVRAM(offset));
        }

        #endregion

    }
}