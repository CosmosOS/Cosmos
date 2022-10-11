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

        #region Structure

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

        #endregion

        /// <summary>
        /// Create new instance of the <see cref="VBEDriver"/> class.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        /// <param name="yres">Y resolution.</param>
        /// <param name="bpp">BPP (color depth).</param>
        public VBEDriver(ushort aXres, ushort aYres, ushort aBpp) : base(aXres, aYres, (byte)aBpp)
        {
            PCIDevice videocard;

            if (VBE.IsAvailable()) //VBE VESA Enabled Mulitboot Parsing
            {
                Global.mDebugger.SendInternal($"Creating VBE VESA driver with Mode {aXres}*{aYres}@{aBpp}");
                _IO.LinearFrameBuffer = new MemoryBlock(VBE.getLfbOffset(), (uint)aXres * aYres * (uint)(aBpp / 8));
            }
            else if (ISAModeAvailable()) //Bochs Graphics Adaptor ISA Mode
            {
                Global.mDebugger.SendInternal($"Creating VBE BGA driver with Mode {aXres}*{aYres}@{aBpp}.");

                _IO.LinearFrameBuffer = new MemoryBlock(0xE0000000, 1920 * 1200 * 4);
                VBESet(aXres, aYres, aBpp);
            }
            else if (((videocard = HAL.PCI.GetDevice(VendorID.VirtualBox, DeviceID.VBVGA)) != null) || //VirtualBox Video Adapter PCI Mode
            ((videocard = HAL.PCI.GetDevice(VendorID.Bochs, DeviceID.BGA)) != null)) // Bochs Graphics Adaptor PCI Mode
            {
                Global.mDebugger.SendInternal($"Creating VBE BGA driver with Mode {aXres}*{aYres}@{aBpp}. Framebuffer address=" + videocard.BAR0);

                _IO.LinearFrameBuffer = new MemoryBlock(videocard.BAR0, 1920 * 1200 * 4);
                VBESet(aXres, aYres, aBpp);
            }
            else
            {
                throw new Exception("No supported VBE device found.");
            }
        }

        /// <summary>
        /// Write value to VBE index.
        /// </summary>
        /// <param name="aIndex">Register index.</param>
        /// <param name="aValue">Value to write.</param>
        private static void VBEWrite(RegisterIndex aIndex, ushort aValue)
        {
            _IO.VbeIndex.Word = (ushort)aIndex;
            _IO.VbeData.Word = aValue;
        }

        /// <summary>
        /// Read value from VBE index.
        /// </summary>
        /// <param name="aIndex">Register index.</param>
        /// <returns>Value at index.</returns>
        private static ushort VBERead(RegisterIndex aIndex)
        {
            return _IO.VbeData.Word = (ushort)aIndex;
        }

        public static bool ISAModeAvailable()
        { 
            return VBERead(RegisterIndex.DisplayID) == 0xB0C5;
        }

        /// <summary>
        /// Disable display.
        /// </summary>
        public override void Disable()
        {
            Global.mDebugger.SendInternal($"Disabling VBE display");
            VBEWrite(RegisterIndex.DisplayEnable, (ushort)EnableValues.Disabled);
        }

        /// <summary>
        /// Enable display.
        /// </summary>
        private void EnableDisplay(EnableValues aEnableFlags)
        {
            //Global.mDebugger.SendInternal($"VBE Enabling display with EnableFlags (ushort){EnableFlags}");
            VBEWrite(RegisterIndex.DisplayEnable, (ushort)aEnableFlags);
        }

        /// <summary>
        /// Set VBE values.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        /// <param name="yres">Y resolution.</param>
        /// <param name="bpp">BPP (color depth).</param>
        public void VBESet(ushort aXres, ushort aYres, ushort aBpp, bool aDoClear = false)
        {
            Disable();
            VBEWrite(RegisterIndex.DisplayXResolution, aXres);
            VBEWrite(RegisterIndex.DisplayYResolution, aYres);
            VBEWrite(RegisterIndex.DisplayBPP, aBpp);
            if (aDoClear)
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
        public override unsafe void Update()
        {
            MemoryOperations.Copy((uint*)_IO.LinearFrameBuffer.Base, Buffer, (int)(Width * Height * Depth));
        }
    }
}
