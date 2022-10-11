//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;

using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VBECanvas class. Used to control screen, by VBE (VESA BIOS Extensions) standard. See also: <seealso cref="Canvas"/>.
    /// </summary>
    public class VBECanvas : Canvas
    {
        
        /// <summary>
        /// Driver for Setting vbe modes and ploting/getting pixels
        /// </summary>
        private readonly VBEDriver _VBEDriver;

        /// <summary>
        /// Create new instance of the <see cref="VBEScreen"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if default mode (1024x768x32) is not suppoted.</exception>
        public VBECanvas() : this(DefaultMode)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="VBEScreen"/> class.
        /// </summary>
        /// <param name="mode">VBE mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        public VBECanvas(Mode aMode) : base(aMode)
        {
            Global.mDebugger.SendInternal($"Creating new VBEScreen() with mode {aMode.Width}x{aMode.Height}x{(uint)aMode.ColorDepth}");

            if (Core.VBE.IsAvailable())
            {
                Core.VBE.ModeInfo ModeInfo = Core.VBE.getModeInfo();
                aMode = new Mode(ModeInfo.width, ModeInfo.height, (ColorDepth)ModeInfo.bpp);
                Global.mDebugger.SendInternal($"Detected VBE VESA with {aMode.Width}x{aMode.Height}x{(uint)aMode.ColorDepth}");
            }

            ThrowIfModeIsNotValid(aMode);

            _VBEDriver = new VBEDriver((ushort)aMode.Width, (ushort)aMode.Height, (ushort)aMode.ColorDepth);
            Mode = aMode;
        }

        /// <summary>
        /// Disables VBE Graphics mode, parent method returns to VGA text mode (80x25)
        /// </summary>
        public override void Disable()
        {
            _VBEDriver.DisableDisplay();
        }

        #region Display

        ///// <summary>
        ///// All the available screen modes VBE supports, I would like to query the hardware and obtain from it the list but I have
        ///// not yet find how to do it! For now I hardcode the most used VESA modes, VBE seems to support until HDTV resolution
        ///// without problems that is well... excellent :-)
        ///// </summary>
        //public override IReadOnlyList<Mode> AvailableModes { get; } = new List<Mode>

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
        public List<Mode> AvailableModes { get; } = new List<Mode>
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

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="Mode">The desired Mode resolution</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        private void SetMode(Mode aMode)
        {
            aMode.ThrowIfNotValid();

            ushort xres = (ushort)Mode.Width;
            ushort yres = (ushort)Mode.Height;
            ushort bpp = (ushort)Mode.ColorDepth;

            //set the screen
            _VBEDriver.VBESet(xres, yres, bpp);
        }
        #endregion

        #region Drawing

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        public override void Clear(Color aColor)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {aColor}");
            //if (color == null)
            //   throw new ArgumentNullException(nameof(color));

            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            switch (Mode.ColorDepth)
            {
                case ColorDepth.ColorDepth4:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth8:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth16:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth24:
                    _VBEDriver.ClearVRAM((uint)aColor.ToArgb());
                    break;
                case ColorDepth.ColorDepth32:
                    _VBEDriver.ClearVRAM((uint)aColor.ToArgb());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <param name="aWidth">Width.</param>
        /// <param name="aHeight">Height.</param>
        /// <param name="aColor">Color to draw with.</param>
        public override void DrawFilledRectangle(int aX, int aY, uint aWidth, uint aHeight, Color aColor)
        {
            //ClearVRAM clears one uint at a time. So we clear pixelwise not byte wise. That's why we divide by 32 and not 8.
            aWidth = (uint)(Math.Min(aWidth, Mode.Width - aX) * (int)Mode.ColorDepth / 32);

            for (int i = aY; i < aY + aHeight; i++)
            {
                _VBEDriver.ClearVRAM((int)GetIndex(aX, i), (int)aWidth, aColor.ToArgb());
            }
        }

        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="aImage">Image.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        public override void DrawImage(int aX, int aY, Canvas aImage)
        {
            var xBitmap = aImage.Buffer;
            var xWidth = (int)aImage.Mode.Width;
            var xHeight = (int)aImage.Mode.Height;

            for (int i = 0; i < xHeight; i++)
            {
                _VBEDriver.CopyVRAM((int)((i * Mode.Width) + (int)GetIndex(aX, aY)), (int[])(object)xBitmap, (i * xWidth), xWidth);
            }
        }

        #endregion

        /// <summary>
        /// Display screen
        /// </summary>
        public override void Display()
        {
            _VBEDriver.Swap();
        }
    }
}
