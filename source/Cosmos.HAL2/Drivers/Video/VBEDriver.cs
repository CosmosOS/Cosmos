//#define COSMOSDEBUG

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

        private enum VBERegisterIndex {
            VBEDisplayID = 0x00,
            VBEDisplayXResolution,
            VBEDisplayYResolution,
            VBEDisplayBPP,
            VBEDisplayEnable,
            VBEDisplayBankMode,
            VBEDisplayVirtualWidth,
            VBEDisplayVirtualHeight,
            VBEDisplayXOffset,
            VBEDisplayYOffset
        };

        [Flags]
        private enum VBEEnableValues
        {
            VBEDisabled = 0x00,
            VBEEnabled,
            VBEUseLinearFrameBuffer = 0x40,
            VBENoClearMemory = 0x80,
        };

        /* We never want that the default empty constructor is used to create a VBEDriver */
        private VBEDriver()
        {

        }

        public VBEDriver(ushort xres, ushort yres, ushort bpp)
        {
            /*
             * XXX Why this simple test is killing the CPU? It is not working in Bochs too... probably it was neither
             * tested... bah! Removing it for now.
             */
#if false
            if (HAL.PCI.GetDevice(1234, 1111) == null)
            {
                throw new NotSupportedException("No BGA adapter found..");
            }
#endif

            Global.mDebugger.SendInternal($"Creating VBEDriver with Mode {xres}*{yres}@{bpp}");
            VBESet(xres, yres, bpp);
        }

        private void VBEWrite(VBERegisterIndex index, ushort value)
        {
            IO.VbeIndex.Word = (ushort) index;
            IO.VbeData.Word = value;
        }

        private void VBEDisableDisplay()
        {
            Global.mDebugger.SendInternal($"Disabling VBE display");
            VBEWrite(VBERegisterIndex.VBEDisplayEnable, (ushort)VBEEnableValues.VBEDisabled);
        }

        private void VBESetXResolution(ushort xres)
        {
            Global.mDebugger.SendInternal($"VBE Setting X resolution to {xres}");
            VBEWrite(VBERegisterIndex.VBEDisplayXResolution, xres);
        }

        private void VBESetYResolution(ushort yres)
        {
            Global.mDebugger.SendInternal($"VBE Setting Y resolution to {yres}");
            VBEWrite(VBERegisterIndex.VBEDisplayYResolution, yres);
        }

        private void VBESetDisplayBPP(ushort bpp)
        {
            Global.mDebugger.SendInternal($"VBE Setting BPP to {bpp}");
            VBEWrite(VBERegisterIndex.VBEDisplayBPP, bpp);
        }

        private void VBEEnableDisplay(VBEEnableValues EnableFlags)
        {
            //Global.mDebugger.SendInternal($"VBE Enabling display with EnableFlags (ushort){EnableFlags}");
            VBEWrite(VBERegisterIndex.VBEDisplayEnable, (ushort)EnableFlags);
        }

        public void VBESet(ushort xres, ushort yres, ushort bpp)
        {
            VBEDisableDisplay();
            VBESetXResolution(xres);
            VBESetYResolution(yres);
            VBESetDisplayBPP(bpp);
            /*
             * Re-enable the Display with LinearFrameBuffer and without clearing video memory of previous value 
             * (this permits to change Mode without losing the previous datas)
             */ 
            VBEEnableDisplay(VBEEnableValues.VBEEnabled | VBEEnableValues.VBEUseLinearFrameBuffer | VBEEnableValues.VBENoClearMemory);
        }

        public void SetVRAM(uint index, byte value)
        {
            Global.mDebugger.SendInternal($"Writing to driver memory in position {index} value {value} (as byte)");
            IO.LinearFrameBuffer.Bytes[index] = value;
        }

        public void SetVRAM(uint index, ushort value)
        {
            Global.mDebugger.SendInternal($"Writing to driver memory in position {index} value {value} (as ushort)");
            IO.LinearFrameBuffer.Words[index] = value;
        }

        public void SetVRAM(uint index, uint value)
        {
            //Global.mDebugger.SendInternal($"Writing to driver memory in position {index} value {value} (as uint)");
            IO.LinearFrameBuffer.DWords[index] = value;
        }

        public byte GetVRAM(uint index)
        {
            return IO.LinearFrameBuffer.Bytes[index];
        }

        public void ClearVRAM(uint value)
        {
            IO.LinearFrameBuffer.Fill(value);
        }
    }
}
