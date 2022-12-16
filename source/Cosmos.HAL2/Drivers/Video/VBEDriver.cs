//#define COSMOSDEBUG

using System;
using Cosmos.Core;
using Cosmos.Core.IOGroup;

namespace Cosmos.HAL.Drivers
{
    /// <summary>
    /// VBEDriver class. Used to directly write registers values to the port.
    /// </summary>
    public class VBEDriver
    {
        /// <summary>
        /// Index IOPort.
        /// </summary>
        public const int VbeIndex = 0x01CE;
        /// <summary>
        /// Data IOPort.
        /// </summary>
        public const int VbeData = 0x01CF;

        /*
         * This not a lot optimal as we are taking a lot of memory and then maybe the driver is configured to go at 320*240!
         */
        /// <summary>
        /// Frame buffer memory block.
        /// </summary>
        public MemoryBlock LinearFrameBuffer;
        //public MemoryBlock LinearFrameBuffer = new MemoryBlock(0xE0000000, 1024 * 768 * 4);

        protected readonly ManagedMemoryBlock lastbuffer;

        /// <summary>
        /// Register index.
        /// <para>
        /// Avilable indexs:
        /// <list type="bullet">
        /// <item>DisplayID.</item>
        /// <item>DisplayXResolution.</item>
        /// <item>DisplayYResolution.</item>
        /// <item>DisplayBPP.</item>
        /// <item>DisplayEnable.</item>
        /// <item>DisplayBankMode.</item>
        /// <item>DisplayVirtualWidth.</item>
        /// <item>DisplayVirtualHeight.</item>
        /// <item>DisplayXOffset.</item>
        /// <item>DisplayYOffset.</item>
        /// </list>
        /// </para>
        /// </summary>
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

        /// <summary>
        /// Enable values.
        /// <para>
        /// Avilable values:
        /// <list type="bullet">
        /// <item>Disabled.</item>
        /// <item>Enabled.</item>
        /// <item>UseLinearFrameBuffer.</item>
        /// <item>NoClearMemory.</item>
        /// </list>
        /// </para>
        /// </summary>
        [Flags]
        private enum EnableValues
        {
            Disabled = 0x00,
            Enabled,
            UseLinearFrameBuffer = 0x40,
            NoClearMemory = 0x80,
        };

        /// <summary>
        /// Create new instance of the <see cref="VBEDriver"/> class.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        /// <param name="yres">Y resolution.</param>
        /// <param name="bpp">BPP (color depth).</param>
        public VBEDriver(ushort xres, ushort yres, ushort bpp)
        {
            PCIDevice videocard;

            if (VBE.IsAvailable()) //VBE VESA Enabled Mulitboot Parsing
            {
                Global.mDebugger.SendInternal($"Creating VBE VESA driver with Mode {xres}*{yres}@{bpp}");
                LinearFrameBuffer = new MemoryBlock(VBE.getLfbOffset(), (uint)xres * yres * (uint)(bpp / 8));
                lastbuffer = new ManagedMemoryBlock((uint)xres * yres * (uint)(bpp / 8));
            }
            else if (ISAModeAvailable()) //Bochs Graphics Adaptor ISA Mode
            {
                Global.mDebugger.SendInternal($"Creating VBE BGA driver with Mode {xres}*{yres}@{bpp}.");

                LinearFrameBuffer = new MemoryBlock(0xE0000000, 1920 * 1200 * 4);
                lastbuffer = new ManagedMemoryBlock(1920 * 1200 * 4);
                VBESet(xres, yres, bpp);
            }
            else if ((videocard = HAL.PCI.GetDevice(VendorID.VirtualBox, DeviceID.VBVGA)) != null || //VirtualBox Video Adapter PCI Mode
            (videocard = HAL.PCI.GetDevice(VendorID.Bochs, DeviceID.BGA)) != null) // Bochs Graphics Adaptor PCI Mode
            {
                Global.mDebugger.SendInternal($"Creating VBE BGA driver with Mode {xres}*{yres}@{bpp}. Framebuffer address=" + videocard.BAR0);

                LinearFrameBuffer = new MemoryBlock(videocard.BAR0, 1920 * 1200 * 4);
                lastbuffer = new ManagedMemoryBlock(1920 * 1200 * 4);
                VBESet(xres, yres, bpp);
            }
            else
            {
                throw new Exception("No supported VBE device found.");
            }
        }

        /// <summary>
        /// Write value to VBE index.
        /// </summary>
        /// <param name="index">Register index.</param>
        /// <param name="value">Value.</param>
        private static void VBEWrite(RegisterIndex index, ushort value)
        {
            IOPort.Write16(VbeIndex, (ushort)index);
            IOPort.Write16(VbeData, value);
        }

        private static ushort VBERead(RegisterIndex index)
        {
            IOPort.Write16(VbeIndex, (ushort)index);
            return IOPort.Read16(VbeData);
        }
        public static bool ISAModeAvailable()
        {
            //This code wont work as long as Bochs uses BGA ISA, since it wont discover it in PCI
#if false
            return HAL.PCI.GetDevice(VendorID.Bochs, DeviceID.BGA) != null;
#endif
            return VBERead(RegisterIndex.DisplayID) == 0xB0C5;
        }

        /// <summary>
        /// Disable display.
        /// </summary>
        public void DisableDisplay()
        {
            Global.mDebugger.SendInternal($"Disabling VBE display");
            VBEWrite(RegisterIndex.DisplayEnable, (ushort)EnableValues.Disabled);
        }

        /// <summary>
        /// Set X resolution.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        private void SetXResolution(ushort xres)
        {
            Global.mDebugger.SendInternal($"VBE Setting X resolution to {xres}");
            VBEWrite(RegisterIndex.DisplayXResolution, xres);
        }

        /// <summary>
        /// Set Y resolution.
        /// </summary>
        /// <param name="yres">Y resolution.</param>
        private void SetYResolution(ushort yres)
        {
            Global.mDebugger.SendInternal($"VBE Setting Y resolution to {yres}");
            VBEWrite(RegisterIndex.DisplayYResolution, yres);
        }

        /// <summary>
        /// Set BPP.
        /// </summary>
        /// <param name="bpp">BPP (color depth).</param>
        private void SetDisplayBPP(ushort bpp)
        {
            Global.mDebugger.SendInternal($"VBE Setting BPP to {bpp}");
            VBEWrite(RegisterIndex.DisplayBPP, bpp);
        }

        /// <summary>
        /// Enable display.
        /// </summary>
        private void EnableDisplay(EnableValues EnableFlags)
        {
            //Global.mDebugger.SendInternal($"VBE Enabling display with EnableFlags (ushort){EnableFlags}");
            VBEWrite(RegisterIndex.DisplayEnable, (ushort)EnableFlags);
        }

        /// <summary>
        /// Set VBE values.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        /// <param name="yres">Y resolution.</param>
        /// <param name="bpp">BPP (color depth).</param>
        public void VBESet(ushort xres, ushort yres, ushort bpp, bool clear = false)
        {
            DisableDisplay();
            SetXResolution(xres);
            SetYResolution(yres);
            SetDisplayBPP(bpp);
            if (clear)
            {
                EnableDisplay(EnableValues.Enabled | EnableValues.UseLinearFrameBuffer);

            }
            else
            {
                /*
                * Re-enable the Display with LinearFrameBuffer and without clearing video memory of previous value
                * (this permits to change Mode without losing the previous datas)
                */
                EnableDisplay(EnableValues.Enabled | EnableValues.UseLinearFrameBuffer | EnableValues.NoClearMemory);
            }
        }

        /// <summary>
        /// Set VRAM.
        /// </summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">Value to set.</param>
        public void SetVRAM(uint index, byte value)
        {
            lastbuffer[index] = value;
        }

        /// <summary>
        /// Set VRAM.
        /// </summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">Value to set.</param>
        public void SetVRAM(uint index, ushort value)
        {
            lastbuffer[index] = (byte)((value >> 8) & 0xFF);
            lastbuffer[index + 1] = (byte)((value >> 0) & 0xFF);
        }

        /// <summary>
        /// Set VRAM.
        /// </summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">Value to set.</param>
        public void SetVRAM(uint index, uint value)
        {
            lastbuffer[index] = (byte)((value >> 24) & 0xFF);
            lastbuffer[index + 1] = (byte)((value >> 16) & 0xFF);
            lastbuffer[index + 2] = (byte)((value >> 8) & 0xFF);
            lastbuffer[index + 3] = (byte)((value >> 0) & 0xFF);
        }

        /// <summary>
        /// Get VRAM.
        /// </summary>
        /// <param name="index">Index to get.</param>
        /// <returns>byte value.</returns>
        public uint GetVRAM(uint index)
        {
            int pixel = (lastbuffer[index + 3] << 24) | (lastbuffer[index + 2] << 16) | (lastbuffer[index + 1] << 8) | lastbuffer[index];

            return (uint)pixel;
        }

        /// <summary>
        /// Clear VRAM.
        /// </summary>
        /// <param name="value">Value of fill with.</param>
        public void ClearVRAM(uint value)
        {
            lastbuffer.Fill(value);
        }

        /// <summary>
        /// Clear VRAM.
        /// </summary>
        /// <param name="aStart">A start.</param>
        /// <param name="aCount">A count.</param>
        /// <param name="value">A volum.</param>
        public void ClearVRAM(int aStart, int aCount, int value)
        {
            lastbuffer.Fill(aStart, aCount, value);
        }

        /// <summary>
        /// Copy VRAM.
        /// </summary>
        /// <param name="aStart">A start.</param>
        /// <param name="aData">A data.</param>
        /// <param name="aIndex">A index.</param>
        /// <param name="aCount">A count.</param>
        public void CopyVRAM(int aStart, int[] aData, int aIndex, int aCount)
        {
            lastbuffer.Copy(aStart, aData, aIndex, aCount);
        }

        /// <summary>
        /// Copy VRAM.
        /// </summary>
        /// <param name="aStart">A start.</param>
        /// <param name="aData">A data.</param>
        /// <param name="aIndex">A index.</param>
        /// <param name="aCount">A count.</param>
        public void CopyVRAM(int aStart, byte[] aData, int aIndex, int aCount)
        {
            lastbuffer.Copy(aStart, aData, aIndex, aCount);
        }

        /// <summary>
        /// Swap back buffer to video memory
        /// </summary>
        public void Swap()
        {
            LinearFrameBuffer.Copy(lastbuffer);
        }
    }
}
