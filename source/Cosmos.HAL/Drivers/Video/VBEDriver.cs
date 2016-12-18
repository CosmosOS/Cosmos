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

        /* We never want that the default empty constructor is used to create a VBEDriver */
        private VBEDriver()
        {

        }

        public VBEDriver(ushort xres, ushort yres, ushort bpp)
        {
            VBESet(xres, yres, bpp);
        }

        private void VBEWrite(ushort index, ushort value)
        {
            IO.VbeIndex.Word =  index;
            IO.VbeData.Word = value;
        }

        public void VBESet(ushort xres, ushort yres, ushort bpp)
        {
            //Disable Display
            VBEWrite(0x4, 0x00);
            //Set Display Xres
            VBEWrite(0x1, xres);
            //SetDisplay Yres
            VBEWrite(0x2, yres);
            //SetDisplay bpp
            VBEWrite(0x3, bpp);
            //Enable Display and LFB
            VBEWrite(0x4, (ushort)(0x01 | 0x40));
        }

        public void SetVRAM(uint index, byte value)
        {
            IO.VGAMemoryBlock[index] = value;
        }

        public byte GetVRAM(uint index)
        {
            return IO.VGAMemoryBlock[index];
        }
    }
}
