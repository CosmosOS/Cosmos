using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.Drivers
{
    public class VBEDriver
    {
       
        private Core.IOGroup.VBE IO = Core.Global.BaseIOGroups.VBE;


        /// <summary>
        /// The current Width of the screen in pixels
        /// </summary>
        public uint ScreenWidth { get; set; }
        /// <summary>
        /// The current Height of the screen in pixels
        /// </summary>
        public uint ScreenHeight { get; set; }
        /// <summary>
        /// The current Bytes per pixel
        /// </summary>
        public uint ScreenBpp { get; set; }

        private void vbe_write(ushort index, ushort value)
        {
            IO.VbeIndex.Word =  index;
            IO.VbeData.Word = value;
        }

        public void vbe_set(ushort xres, ushort yres, ushort bpp)
        {
            ScreenWidth = xres;
            ScreenHeight = yres;
            ScreenBpp = bpp;

            //Disable Display
            vbe_write(0x4, 0x00);
            //Set Display Xres
            vbe_write(0x1, xres);
            //SetDisplay Yres
            vbe_write(0x2, yres);
            //SetDisplay bpp
            vbe_write(0x3, bpp);
            //Enable Display and LFB           
            vbe_write(0x4, (ushort)(0x01 | 0x40));
        }

        public void set_vram(uint index, byte value)
        {
            IO.VBEMemoryBlock[index] = value;
        }
        
        public byte get_vram(uint index)
        {
            return IO.VBEMemoryBlock[index];
        }
        
    }
}
