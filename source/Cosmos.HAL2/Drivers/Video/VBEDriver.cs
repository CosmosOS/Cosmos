#define COSMOSDEBUG

using System;
using Cosmos.Core.IOGroup;

namespace Cosmos.HAL.Drivers
{
    public class VBEDriver
    {

        private static readonly VBE IO = Core.Global.BaseIOGroups.VBE;

        private enum RegisterIndex
        {
            DisplayID = 0x00,
            DisplayXResolution,
            DisplayYResolution,
            DisplayBPP,
            DisplayEnable,
            DisplayBankMode,
            DisplayVirtualWidth,
            DisplayVirtualHeight,
            DisplayXOffset,
            DisplayYOffset
        };

        [Flags]
        private enum EnableValues
        {
            Disabled = 0x00,
            Enabled,
            UseLinearFrameBuffer = 0x40,
            NoClearMemory = 0x80,
        };

        public VBEDriver(ushort xres, ushort yres, ushort bpp)
        {
            if (!Available())
            {
                Global.mDebugger.SendInternal("No Bochs Graphics Adapter found...");
                throw new NotSupportedException("No Bochs Graphics Adapter found...");
            }
            Global.mDebugger.SendInternal($"Creating VBEDriver with Mode {xres}*{yres}@{bpp}");
            VBESet(xres, yres, bpp);
        }

        private static void VBEWrite(RegisterIndex index, ushort value)
        {
            IO.VbeIndex.Word = (ushort) index;
            IO.VbeData.Word = value;
        }

        private static ushort VBERead(RegisterIndex index)
        {
            IO.VbeIndex.Word = (ushort)index;
            return IO.VbeData.Word;
        }
        public static bool Available()
        {
            //This code wont work as long as Bochs uses BGA ISA, since it wont discover it in PCI
#if false
            return HAL.PCI.GetDevice(VendorID.Bochs, DeviceID.BGA) != null;
#endif
            return VBERead(RegisterIndex.DisplayID) == 0xB0C5;
        }
        
        public void DisableDisplay()
        {
            Global.mDebugger.SendInternal($"Disabling VBE display");
            VBEWrite(RegisterIndex.DisplayEnable, (ushort)EnableValues.Disabled);
        }

        private void SetXResolution(ushort xres)
        {
            Global.mDebugger.SendInternal($"VBE Setting X resolution to {xres}");
            VBEWrite(RegisterIndex.DisplayXResolution, xres);
        }

        private void SetYResolution(ushort yres)
        {
            Global.mDebugger.SendInternal($"VBE Setting Y resolution to {yres}");
            VBEWrite(RegisterIndex.DisplayYResolution, yres);
        }

        private void SetDisplayBPP(ushort bpp)
        {
            Global.mDebugger.SendInternal($"VBE Setting BPP to {bpp}");
            VBEWrite(RegisterIndex.DisplayBPP, bpp);
        }

        private void EnableDisplay(EnableValues EnableFlags)
        {
            //Global.mDebugger.SendInternal($"VBE Enabling display with EnableFlags (ushort){EnableFlags}");
            VBEWrite(RegisterIndex.DisplayEnable, (ushort)EnableFlags);
        }

        public void VBESet(ushort xres, ushort yres, ushort bpp)
        {
            DisableDisplay();
            SetXResolution(xres);
            SetYResolution(yres);
            SetDisplayBPP(bpp);
            /*
             * Re-enable the Display with LinearFrameBuffer and without clearing video memory of previous value 
             * (this permits to change Mode without losing the previous datas)
             */ 
            EnableDisplay(EnableValues.Enabled | EnableValues.UseLinearFrameBuffer | EnableValues.NoClearMemory);
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

        public void ClearVRAM(int aStart, int aCount, int value)
        {
            IO.LinearFrameBuffer.Fill(aStart, aCount, value);
        }

        public void CopyVRAM(int aStart, int[] aData, int aIndex, int aCount)
        {
            IO.LinearFrameBuffer.Copy(aStart, aData, aIndex, aCount);
        }
    }
}
