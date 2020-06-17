//#define COSMOSDEBUG

using System;

namespace Cosmos.HAL.Drivers
{
    /// <summary>
    /// VBEDriver class. Used to directly write registers values to the port.
    /// </summary>
    public class VBEDriver
    {
        private readonly Core.IOGroup.VBE IO = Core.Global.BaseIOGroups.VBE;

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

        /// <summary>
        /// Write value to VBE index.
        /// </summary>
        /// <param name="index">Register index.</param>
        /// <param name="value">Value.</param>
        private void Write(RegisterIndex index, ushort value)
        {
            IO.VbeIndex.Word = (ushort) index;
            IO.VbeData.Word = value;
        }

        /// <summary>
        /// Disable display.
        /// </summary>
        private void DisableDisplay()
        {
            Global.mDebugger.SendInternal($"Disabling VBE display");
            Write(RegisterIndex.DisplayEnable, (ushort)EnableValues.Disabled);
        }

        /// <summary>
        /// Set X resolution.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        private void SetXResolution(ushort xres)
        {
            Global.mDebugger.SendInternal($"VBE Setting X resolution to {xres}");
            Write(RegisterIndex.DisplayXResolution, xres);
        }

        /// <summary>
        /// Set Y resolution.
        /// </summary>
        /// <param name="yres">Y resolution.</param>
        private void SetYResolution(ushort yres)
        {
            Global.mDebugger.SendInternal($"VBE Setting Y resolution to {yres}");
            Write(RegisterIndex.DisplayYResolution, yres);
        }

        /// <summary>
        /// Set BPP.
        /// </summary>
        /// <param name="bpp">BPP (color depth).</param>
        private void SetDisplayBPP(ushort bpp)
        {
            Global.mDebugger.SendInternal($"VBE Setting BPP to {bpp}");
            Write(RegisterIndex.DisplayBPP, bpp);
        }

        /// <summary>
        /// Enable display.
        /// </summary>
        private void EnableDisplay(EnableValues EnableFlags)
        {
            //Global.mDebugger.SendInternal($"VBE Enabling display with EnableFlags (ushort){EnableFlags}");
            Write(RegisterIndex.DisplayEnable, (ushort)EnableFlags);
        }

        /// <summary>
        /// Set VBE values.
        /// </summary>
        /// <param name="xres">X resolution.</param>
        /// <param name="yres">Y resolution.</param>
        /// <param name="bpp">BPP (color depth).</param>
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

        /// <summary>
        /// Set VRAM.
        /// </summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">Value to set.</param>
        public void SetVRAM(uint index, byte value)
        {
            Global.mDebugger.SendInternal($"Writing to driver memory in position {index} value {value} (as byte)");
            IO.LinearFrameBuffer.Bytes[index] = value;
        }

        /// <summary>
        /// Set VRAM.
        /// </summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">Value to set.</param>
        public void SetVRAM(uint index, ushort value)
        {
            Global.mDebugger.SendInternal($"Writing to driver memory in position {index} value {value} (as ushort)");
            IO.LinearFrameBuffer.Words[index] = value;
        }

        /// <summary>
        /// Set VRAM.
        /// </summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">Value to set.</param>
        public void SetVRAM(uint index, uint value)
        {
            //Global.mDebugger.SendInternal($"Writing to driver memory in position {index} value {value} (as uint)");
            IO.LinearFrameBuffer.DWords[index] = value;
        }

        /// <summary>
        /// Get VRAM.
        /// </summary>
        /// <param name="index">Index to get.</param>
        /// <returns>byte value.</returns>
        public byte GetVRAM(uint index)
        {
            return IO.LinearFrameBuffer.Bytes[index];
        }

        /// <summary>
        /// Clear VRAM.
        /// </summary>
        /// <param name="value">Value of fill with.</param>
        public void ClearVRAM(uint value)
        {
            IO.LinearFrameBuffer.Fill(value);
        }

        /// <summary>
        /// Clear VRAM.
        /// </summary>
        /// <param name="aStart">A start.</param>
        /// <param name="aCount">A count.</param>
        /// <param name="value">A volum.</param>
        public void ClearVRAM(int aStart, int aCount, int value)
        {
            IO.LinearFrameBuffer.Fill(aStart, aCount, value);
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
            IO.LinearFrameBuffer.Copy(aStart, aData, aIndex, aCount);
        }
    }
}
