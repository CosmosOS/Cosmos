//#define COSMOSDEBUG

using System;
using Cosmos.Core;
using Cosmos.Core.IOGroup;

namespace Cosmos.HAL.Drivers.Video
{
    /// <summary>
    /// VBEDriver class. Used to directly write registers values to the port.
    /// </summary>
    public class VBEDriver : VideoDriver
    {
        private static readonly VBEIOGroup _IO = Core.Global.BaseIOGroups.VBE;

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
        public VBEDriver(ushort xres, ushort yres, ushort bpp) : base(xres, yres, (byte)bpp)
        {
            PCIDevice videocard;

            if (VBE.IsAvailable()) //VBE VESA Enabled Mulitboot Parsing
            {
                Global.mDebugger.SendInternal($"Creating VBE VESA driver with Mode {xres}*{yres}@{bpp}");
                _IO.LinearFrameBuffer = new MemoryBlock(VBE.getLfbOffset(), (uint)xres * yres * (uint)(bpp / 8));
            }
            else if (ISAModeAvailable()) //Bochs Graphics Adaptor ISA Mode
            {
                Global.mDebugger.SendInternal($"Creating VBE BGA driver with Mode {xres}*{yres}@{bpp}.");

                _IO.LinearFrameBuffer = new MemoryBlock(0xE0000000, 1920 * 1200 * 4);
                VBESet(xres, yres, bpp);
            }
            else if (((videocard = HAL.PCI.GetDevice(VendorID.VirtualBox, DeviceID.VBVGA)) != null) || //VirtualBox Video Adapter PCI Mode
            ((videocard = HAL.PCI.GetDevice(VendorID.Bochs, DeviceID.BGA)) != null)) // Bochs Graphics Adaptor PCI Mode
            {
                Global.mDebugger.SendInternal($"Creating VBE BGA driver with Mode {xres}*{yres}@{bpp}. Framebuffer address=" + videocard.BAR0);

                _IO.LinearFrameBuffer = new MemoryBlock(videocard.BAR0, 1920 * 1200 * 4);
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
            _IO.VbeIndex.Word = (ushort)index;
            _IO.VbeData.Word = value;
        }

        private static ushort VBERead(RegisterIndex index)
        {
            _IO.VbeIndex.Word = (ushort)index;
            return _IO.VbeData.Word;
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
        /// Swap back buffer to video memory
        /// </summary>
        public unsafe void Swap()
        {
            MemoryOperations.Copy((uint*)_IO.LinearFrameBuffer.Base, Buffer, (int)(Width * Height * BitDepth));
        }
    }
}
