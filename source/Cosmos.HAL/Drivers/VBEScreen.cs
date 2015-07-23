using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.Drivers
{
    public class VBEScreen
    {
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int ScreenBpp { get; set; }

        #region Internals

        private Core.IOGroup.VBE IO = Core.Global.BaseIOGroups.VBE;

        private void vbe_set(ushort xres, ushort yres, ushort bpp)
        {
            //Disable Display
            IO.VBE_DISPI_INDEX_ENABLE.Word = 0x00;
            //Set Display Xres
            IO.VBE_DISPI_INDEX_XRES.Word = xres;
            //SetDisplay Yres
            IO.VBE_DISPI_INDEX_YRES.Word = yres;
            //SetDisplay bpp
            IO.VBE_DISPI_INDEX_BPP.Word = bpp;
            //Enable Display
            IO.VBE_DISPI_INDEX_ENABLE.Word = (ushort)(0x01 | 0x00);
        }

        #endregion



        public enum ScreenSize
        {
            Size320x200,
            Size640x400,
            Size640x480,
            Size800x600,
            Size1024x768,
            Size1280x1024

        }

        public enum Bpp
        {
            Bpp15 = 15,
            Bpp16 = 16,
            Bpp24 = 24,
            Bpp32 = 32
        }

        public void SetMode(ScreenSize aSize, Bpp aBpp)
        {
            //Get screen size
            switch (aSize)
            {
                case ScreenSize.Size320x200:
                    ScreenWidth = 320;
                    ScreenHeight = 200;
                    break;
                case ScreenSize.Size640x400:
                    ScreenWidth = 640;
                    ScreenHeight = 400;
                    break;
                case ScreenSize.Size640x480:
                    ScreenWidth = 640;
                    ScreenHeight = 480;
                    break;
                case ScreenSize.Size800x600:
                    ScreenWidth = 800;
                    ScreenHeight = 600;
                    break;
                case ScreenSize.Size1024x768:
                    ScreenWidth = 1024;
                    ScreenHeight = 768;
                    break;
                case ScreenSize.Size1280x1024:
                    ScreenWidth = 1280;
                    ScreenHeight = 1024;
                    break;
            }
            //Get bpp
            switch (aBpp)
            {
                case Bpp.Bpp15:
                    ScreenBpp = 15;
                    break;
                case Bpp.Bpp16:
                    ScreenBpp = 16;
                    break;
                case Bpp.Bpp24:
                    ScreenBpp = 24;
                    break;
                case Bpp.Bpp32:
                    ScreenBpp = 32;
                    break;
            }
            vbe_set((ushort)ScreenWidth, (ushort)ScreenHeight, (ushort)ScreenBpp);

        }
    }
}
