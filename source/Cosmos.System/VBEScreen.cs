using Cosmos.HAL.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System
{
   public class VBEScreen
    {
        private VBEDriver _vbe = new VBEDriver();

        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int ScreenBpp { get; set; }

        public  enum ScreenSize
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
            Bpp15 = 1500,
            Bpp16 = 1600,
            Bpp24 = 2400,
            Bpp32 = 3200
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
           _vbe.vbe_set((ushort)ScreenWidth, (ushort)ScreenHeight, (ushort)ScreenBpp);
        }

        public void Clear(uint color)
        {
            for (uint x = 0; x < ScreenWidth; x++)
            {
                for (uint y = 0; y < ScreenHeight; y++)
                {
                    SetPixel(x, y, color);
                }
            }


        }

        public void Clear(byte red, byte green, byte blue)
        {

        }

        public void SetPixel(uint x, uint y, uint color)
        {
           
            uint where = x * ((uint)ScreenBpp  / 8) + y * (uint)(ScreenWidth * ((uint)ScreenBpp / 8));
            _vbe.set_vram(where, (byte)(color & 255));              // BLUE
            _vbe.set_vram(where + 1, (byte)((color >> 8) & 255));   // GREEN
            _vbe.set_vram(where + 2, (byte)((color >> 16) & 255));  // RED
        }

        public void SetPixel(uint x, uint y, byte red, byte green, byte blue)
        {

        }


    }
}
