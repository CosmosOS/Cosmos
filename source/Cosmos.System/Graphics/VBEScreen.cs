using Cosmos.HAL.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Graphics;

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
            // We don't need to add a Control for defaultGraphicMode we assume it to be always a valid mode
            VBEDriver = new VBEDriver((ushort)defaultGraphicMode.Rows, (ushort)defaultGraphicMode.Columns, (ushort)defaultGraphicMode.ColorDepth);
        }

        public VBEScreen(Mode mode) : base(mode)
        {
            if (!IsModeValid(mode))
                throw new ArgumentOutOfRangeException($"Mode {mode} is not supported by VBE Driver");

            VBEDriver = new VBEDriver((ushort)mode.Rows, (ushort)mode.Columns, (ushort)mode.ColorDepth);
        }

        /// <summary>
        /// The current Width of the screen in pixels
        /// </summary>
        public int ScreenWidth { get; set; }
        /// <summary>
        /// The current Height of the screen in pixels
        /// </summary>
        public int ScreenHeight { get; set; }
        /// <summary>
        /// The current Bytes per pixel
        /// </summary>
        public int ScreenBpp { get; set; }

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
        /// All the avalible screen resolutions
        /// </summary>
        public enum ScreenSize
        {
            Size320x200,
            Size640x400,
            Size640x480,
            Size800x600,
            Size1024x768,
            Size1280x1024
        }

        public override List<Mode> getAviableModes()
        {
            /* I hardcode for now 1024x768@32 (the default mode) so the check does not fails */
            return new List<Mode> { new Mode(1024, 768, ColorDepth.ColorDepth32) };
#if false
            /*
             * The real VBE has a command to obtain all aviable modes to see if the "fake" Bochs VBE has a way to get this
             * or if I have to hardcode them
             */
            throw new NotImplementedException();
#endif
        }

        protected override Mode getDefaultGraphicMode() => new Mode(1024, 768, ColorDepth.ColorDepth32);

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="Mode">The desired Mode resolution</param>
        private void SetMode(Mode mode)
        {
            if (!IsModeValid(mode))
                throw new ArgumentOutOfRangeException($"Mode {mode} is not supported by VBE Driver");

            ushort xres = (ushort)Mode.Rows;
            ushort yres = (ushort)Mode.Columns;
            ushort bpp = (ushort)Mode.ColorDepth;

            //set the screen
           VBEDriver.VBESet(xres, yres, bpp);
        }

        #endregion

        #region Drawing

        public override void DrawPoint(Pen pen, int x, int y)
        {
            int where = x * ((int)mode.ColorDepth / 8) + y * (mode.Columns * ((int)mode.ColorDepth / 8));

            /*
             * The old VBEScreen used directly the RGB colors ignoring the selected ColorDepth I don't think this is correct
             * for now we can Draw only if the ColorDepth is 32 bit, we will throw otherwise
             */
             switch (ColorDepth.ColorDepth32)
            {
                case ColorDepth.ColorDepth32:
                    VBEDriver.SetVRAM((uint)where, pen.Color.B);       // BLUE
                    VBEDriver.SetVRAM((uint)where + 1, pen.Color.G);   // GREEN
                    VBEDriver.SetVRAM((uint)where + 2, pen.Color.R);   // RED

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Reading
        // TODO add to Canvas GetPointColor()

        #endregion

    }
}
