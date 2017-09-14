//#define COSMOSDEBUG
using Cosmos.HAL.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Graphics;
using Cosmos.Common.Extensions;

namespace Cosmos.System
{
   public class VBEScreen : Canvas
    {
        /// <summary>
        /// Driver for Setting vbe modes and ploting/getting pixels
        /// </summary>
        private VBEDriver VBEDriver;

        public VBEScreen() : base()
        {
            Global.mDebugger.SendInternal($"Creating new VBEScreen() with default mode {defaultGraphicMode}");

            // We don't need to add a Control for defaultGraphicMode we assume it to be always a valid mode
            VBEDriver = new VBEDriver((ushort)defaultGraphicMode.Columns, (ushort)defaultGraphicMode.Rows, (ushort)defaultGraphicMode.ColorDepth);
        }

        public VBEScreen(Mode mode) : base(mode)
        {
            Global.mDebugger.SendInternal($"Creating new VBEScreen() with mode {mode.Columns}x{mode.Rows}x{(uint)mode.ColorDepth}");

            ThrowIfModeIsNotValid(mode);

            VBEDriver = new VBEDriver((ushort)mode.Columns, (ushort)mode.Rows, (ushort)mode.ColorDepth);
        }

        public override Mode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
                SetMode(mode);
            }
        }

        #region Display
        /// <summary>
        /// All the available screen modes VBE supports, I would like to query the hardware and obtain from it the list but I have
        /// not yet find how to do it! For now I hardcode the most used VESA modes, VBE seems to support until HDTV resolution
        /// without problems that is well... excellent :-)
        /// </summary>
        public override List<Mode> getAvailableModes()
        {
            return new List<Mode>
                {
                  new Mode(320, 240, ColorDepth.ColorDepth32),
                  new Mode(640, 480, ColorDepth.ColorDepth32),
                  new Mode(800, 600, ColorDepth.ColorDepth32),
                  new Mode(1024, 768, ColorDepth.ColorDepth32),
                  /* The so called HD-Ready resolution */
                  new Mode(1280, 720, ColorDepth.ColorDepth32),
                  new Mode(1280, 1024, ColorDepth.ColorDepth32),
                  /* A lot of HD-Ready screen uses this instead of 1280x720 */
                  new Mode(1366, 768, ColorDepth.ColorDepth32),
                  new Mode(1680, 1050, ColorDepth.ColorDepth32),
                  /* HDTV resolution */
                  new Mode(1920, 1080, ColorDepth.ColorDepth32),
                  /* HDTV resolution (16:10 AR) */
                  new Mode(1920, 1200, ColorDepth.ColorDepth32),
            };
        }

        protected override Mode getDefaultGraphicMode() => new Mode(1024, 768, ColorDepth.ColorDepth32);

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="Mode">The desired Mode resolution</param>
        private void SetMode(Mode mode)
        {
            ThrowIfModeIsNotValid(mode);


            ushort xres = (ushort)Mode.Columns;
            ushort yres = (ushort)Mode.Rows;
            ushort bpp = (ushort)Mode.ColorDepth;

            //set the screen
           VBEDriver.VBESet(xres, yres, bpp);
        }
        #endregion

        #region Drawing

        public override void Clear(Color color)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {color}");
            //if (color == null)
            //   throw new ArgumentNullException(nameof(color));

            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            VBEDriver.ClearVRAM((uint)color.ToArgb());
        }

        /*
         * As DrawPoint() is the basic block of DrawLine() and DrawRect() and in theory of all the future other methods that will
         * be implemented is better to not check the validity of the arguments here or it will repeat the check for any point
         * to be drawn slowing down all.
         */ 
        public override void DrawPoint(Pen pen, int x, int y)
        {
            Color color = pen.Color;
            uint pitch;
            uint stride;
            uint offset;
            uint ColorDepthInBytes = (uint)mode.ColorDepth / 8;

            /*
             * For now we can Draw only if the ColorDepth is 32 bit, we will throw otherwise.
             *
             * How to support other ColorDepth? The offset calculation should be the same (and so could be done out of the switch)
             * ColorDepth.ColorDepth16 and ColorDepth.ColorDepth8 need a conversion from color (an ARGB32 color) to the RGB16 and RGB8
             * how to do this conversion faster maybe using pre-computed tables? What happens if the color cannot be converted? We will throw?
             */
            switch (mode.ColorDepth)
            {
                case ColorDepth.ColorDepth32:
                    Global.mDebugger.SendInternal("Computing offset...");
                    pitch = (uint)mode.Columns * ColorDepthInBytes;
                    stride = ColorDepthInBytes;
                    //offset = ((uint)x * pitch) + ((uint)y * stride);
                    offset = ((uint)x * stride) + ((uint)y * pitch);

                    Global.mDebugger.SendInternal($"Drawing Point of color {color} at offset {offset}");

                    VBEDriver.SetVRAM(offset, (uint)color.ToArgb());

                    Global.mDebugger.SendInternal("Point drawn");
                    break;
                case ColorDepth.ColorDepth24:
                    Global.mDebugger.SendInternal("Computing offset...");
                    pitch = (uint)mode.Columns * ColorDepthInBytes;
                    stride = ColorDepthInBytes;
                    //offset = ((uint)x * pitch) + ((uint)y * stride);
                    offset = ((uint)x * stride) + ((uint)y * pitch);

                    Global.mDebugger.SendInternal($"Drawing Point of color {color} at offset {offset}");
                    VBEDriver.SetVRAM(offset, (((uint)color.R * 1000 + color.G) * 1000 + color.B));

                    Global.mDebugger.SendInternal("Point drawn");
                    break;
                default:
                    String errorMsg = "DrawPoint() with ColorDepth " + (int)Mode.ColorDepth + " not yet supported";
                    throw new NotImplementedException(errorMsg);

            }
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Reading
        public override Color GetPointColor(int x, int y)
        {
            uint pitch;
            uint stride;
            uint offset;
            uint ColorDepthInBytes = (uint)mode.ColorDepth / 8;

            Global.mDebugger.SendInternal("Computing offset...");
            pitch = (uint)mode.Columns * ColorDepthInBytes;
            stride = ColorDepthInBytes;
            //offset = ((uint)x * pitch) + ((uint)y * stride);
            offset = ((uint)x * stride) + ((uint)y * pitch);

            Global.mDebugger.SendInternal($"Getting color from point at offset {offset}");
            return Color.FromArgb(VBEDriver.GetVRAM(offset));
        }

        #endregion

    }
}
