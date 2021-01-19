using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;

namespace Cosmos.HAL.Drivers.PCI.Video
{
    /// <summary>
    /// VMWareSVGAII class.
    /// </summary>
    public class DoubleBufferedVMWareSVGAII
    {
        #region 没用的

        /// <summary>
        /// Register values.
        /// </summary>
        public enum Register : ushort
        {
            /// <summary>
            /// ID.
            /// </summary>
            ID = 0,
            /// <summary>
            /// Enabled.
            /// </summary>
            Enable = 1,
            /// <summary>
            /// Width.
            /// </summary>
            Width = 2,
            /// <summary>
            /// Height.
            /// </summary>
            Height = 3,
            /// <summary>
            /// Max width.
            /// </summary>
            MaxWidth = 4,
            /// <summary>
            /// Max height.
            /// </summary>
            MaxHeight = 5,
            /// <summary>
            /// Depth.
            /// </summary>
            Depth = 6,
            /// <summary>
            /// Bits per pixel.
            /// </summary>
            BitsPerPixel = 7,
            /// <summary>
            /// Pseudo color.
            /// </summary>
            PseudoColor = 8,
            /// <summary>
            /// Red mask.
            /// </summary>
            RedMask = 9,
            /// <summary>
            /// Green mask.
            /// </summary>
            GreenMask = 10,
            /// <summary>
            /// Blue mask.
            /// </summary>
            BlueMask = 11,
            /// <summary>
            /// Bytes per line.
            /// </summary>
            BytesPerLine = 12,
            /// <summary>
            /// Frame buffer start.
            /// </summary>
            FrameBufferStart = 13,
            /// <summary>
            /// Frame buffer offset.
            /// </summary>
            FrameBufferOffset = 14,
            /// <summary>
            /// VRAM size.
            /// </summary>
            VRamSize = 15,
            /// <summary>
            /// Frame buffer size.
            /// </summary>
            FrameBufferSize = 16,
            /// <summary>
            /// Capabilities.
            /// </summary>
            Capabilities = 17,
            /// <summary>
            /// Memory start.
            /// </summary>
            MemStart = 18,
            /// <summary>
            /// Memory size.
            /// </summary>
            MemSize = 19,
            /// <summary>
            /// Config done.
            /// </summary>
            ConfigDone = 20,
            /// <summary>
            /// Sync.
            /// </summary>
            Sync = 21,
            /// <summary>
            /// Busy.
            /// </summary>
            Busy = 22,
            /// <summary>
            /// Guest ID.
            /// </summary>
            GuestID = 23,
            /// <summary>
            /// Cursor ID.
            /// </summary>
            CursorID = 24,
            /// <summary>
            /// Cursor X.
            /// </summary>
            CursorX = 25,
            /// <summary>
            /// Cursor Y.
            /// </summary>
            CursorY = 26,
            /// <summary>
            /// Cursor on.
            /// </summary>
            CursorOn = 27,
            /// <summary>
            /// Host bits per pixel.
            /// </summary>
            HostBitsPerPixel = 28,
            /// <summary>
            /// Scratch size.
            /// </summary>
            ScratchSize = 29,
            /// <summary>
            /// Memory registers.
            /// </summary>
            MemRegs = 30,
            /// <summary>
            /// Number of displays.
            /// </summary>
            NumDisplays = 31,
            /// <summary>
            /// Pitch lock.
            /// </summary>
            PitchLock = 32,
            /// <summary>
            /// Indicates maximum size of FIFO Registers.
            /// </summary>
            FifoNumRegisters = 293
        }

        internal void SetPixel(int v1, int v2, uint v3)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ID values.
        /// </summary>
        private enum ID : uint
        {
            /// <summary>
            /// Magic starting point.
            /// </summary>
            Magic = 0x900000,
            /// <summary>
            /// V0.
            /// </summary>
            V0 = Magic << 8,
            /// <summary>
            /// V1.
            /// </summary>
            V1 = (Magic << 8) | 1,
            /// <summary>
            /// V2.
            /// </summary>
            V2 = (Magic << 8) | 2,
            /// <summary>
            /// Invalid
            /// </summary>
            Invalid = 0xFFFFFFFF
        }

        /// <summary>
        /// FIFO values.
        /// </summary>
        public enum FIFO : uint
        {   // values are multiplied by 4 to access the array by byte index
            /// <summary>
            /// Min.
            /// </summary>
            Min = 0,
            /// <summary>
            /// Max.
            /// </summary>
            Max = 4,
            /// <summary>
            /// Next command.
            /// </summary>
            NextCmd = 8,
            /// <summary>
            /// Stop.
            /// </summary>
            Stop = 12
        }

        /// <summary>
        /// FIFO command values.
        /// </summary>
        private enum FIFOCommand
        {
            /// <summary>
            /// Update.
            /// </summary>
            Update = 1,
            /// <summary>
            /// Rectange fill.
            /// </summary>
            RECT_FILL = 2,
            /// <summary>
            /// Rectange copy.
            /// </summary>
            RECT_COPY = 3,
            /// <summary>
            /// Define bitmap.
            /// </summary>
            DEFINE_BITMAP = 4,
            /// <summary>
            /// Define bitmap scanline.
            /// </summary>
            DEFINE_BITMAP_SCANLINE = 5,
            /// <summary>
            /// Define pixmap.
            /// </summary>
            DEFINE_PIXMAP = 6,
            /// <summary>
            /// Define pixmap scanline.
            /// </summary>
            DEFINE_PIXMAP_SCANLINE = 7,
            /// <summary>
            /// Rectange bitmap fill.
            /// </summary>
            RECT_BITMAP_FILL = 8,
            /// <summary>
            /// Rectange pixmap fill.
            /// </summary>
            RECT_PIXMAP_FILL = 9,
            /// <summary>
            /// Rectange bitmap copy.
            /// </summary>
            RECT_BITMAP_COPY = 10,
            /// <summary>
            /// Rectange pixmap fill.
            /// </summary>
            RECT_PIXMAP_COPY = 11,
            /// <summary>
            /// Free object.
            /// </summary>
            FREE_OBJECT = 12,
            /// <summary>
            /// Rectangle raster operation fill.
            /// </summary>
            RECT_ROP_FILL = 13,
            /// <summary>
            /// Rectangle raster operation copy.
            /// </summary>
            RECT_ROP_COPY = 14,
            /// <summary>
            /// Rectangle raster operation bitmap fill.
            /// </summary>
            RECT_ROP_BITMAP_FILL = 15,
            /// <summary>
            /// Rectangle raster operation pixmap fill.
            /// </summary>
            RECT_ROP_PIXMAP_FILL = 16,
            /// <summary>
            /// Rectangle raster operation bitmap copy.
            /// </summary>
            RECT_ROP_BITMAP_COPY = 17,
            /// <summary>
            /// Rectangle raster operation pixmap copy.
            /// </summary>
            RECT_ROP_PIXMAP_COPY = 18,
            /// <summary>
            /// Define cursor.
            /// </summary>
            DEFINE_CURSOR = 19,
            /// <summary>
            /// Display cursor.
            /// </summary>
            DISPLAY_CURSOR = 20,
            /// <summary>
            /// Move cursor.
            /// </summary>
            MOVE_CURSOR = 21,
            /// <summary>
            /// Define alpha cursor.
            /// </summary>
            DEFINE_ALPHA_CURSOR = 22
        }

        /// <summary>
        /// IO port offset.
        /// </summary>
        private enum IOPortOffset : byte
        {
            /// <summary>
            /// Index.
            /// </summary>
            Index = 0,
            /// <summary>
            /// Value.
            /// </summary>
            Value = 1,
            /// <summary>
            /// BIOS.
            /// </summary>
            Bios = 2,
            /// <summary>
            /// IRQ.
            /// </summary>
            IRQ = 3
        }

        /// <summary>
        /// Capability values.
        /// </summary>
        [Flags]
        private enum Capability
        {
            /// <summary>
            /// None.
            /// </summary>
            None = 0,
            /// <summary>
            /// Rectangle fill.
            /// </summary>
            RectFill = 1,
            /// <summary>
            /// Rectangle copy.
            /// </summary>
            RectCopy = 2,
            /// <summary>
            /// Rectangle pattern fill.
            /// </summary>
            RectPatFill = 4,
            /// <summary>
            /// Lecacy off screen.
            /// </summary>
            LecacyOffscreen = 8,
            /// <summary>
            /// Raster operation.
            /// </summary>
            RasterOp = 16,
            /// <summary>
            /// Cruser.
            /// </summary>
            Cursor = 32,
            /// <summary>
            /// Cursor bypass.
            /// </summary>
            CursorByPass = 64,
            /// <summary>
            /// Cursor bypass2.
            /// </summary>
            CursorByPass2 = 128,
            /// <summary>
            /// Eigth bit emulation.
            /// </summary>
            EigthBitEmulation = 256,
            /// <summary>
            /// Alpha cursor.
            /// </summary>
            AlphaCursor = 512,
            /// <summary>
            /// Glyph.
            /// </summary>
            Glyph = 1024,
            /// <summary>
            /// Glyph clipping.
            /// </summary>
            GlyphClipping = 0x00000800,
            /// <summary>
            /// Offscreen.
            /// </summary>
            Offscreen1 = 0x00001000,
            /// <summary>
            /// Alpha blend.
            /// </summary>
            AlphaBlend = 0x00002000,
            /// <summary>
            /// Three D.
            /// </summary>
            ThreeD = 0x00004000,
            /// <summary>
            /// Extended FIFO.
            /// </summary>
            ExtendedFifo = 0x00008000,
            /// <summary>
            /// Multi monitors.
            /// </summary>
            MultiMon = 0x00010000,
            /// <summary>
            /// Pitch lock.
            /// </summary>
            PitchLock = 0x00020000,
            /// <summary>
            /// IRQ mask.
            /// </summary>
            IrqMask = 0x00040000,
            /// <summary>
            /// Display topology.
            /// </summary>
            DisplayTopology = 0x00080000,
            /// <summary>
            /// GMR.
            /// </summary>
            Gmr = 0x00100000,
            /// <summary>
            /// Traces.
            /// </summary>
            Traces = 0x00200000,
            /// <summary>
            /// GMR2.
            /// </summary>
            Gmr2 = 0x00400000,
            /// <summary>
            /// Screen objects.
            /// </summary>
            ScreenObject2 = 0x00800000
        }

        /// <summary>
        /// Index port.
        /// </summary>
        private IOPort IndexPort;
        /// <summary>
        /// Value port.
        /// </summary>
        private IOPort ValuePort;
        /// <summary>
        /// BIOS port.
        /// </summary>
        private IOPort BiosPort;
        /// <summary>
        /// IRQ port.
        /// </summary>
        private IOPort IRQPort;

        /// <summary>
        /// Video memory block.
        /// </summary>
        public MemoryBlock Video_Memory;
        /// <summary>
        /// FIFO memory block.
        /// </summary>
        private MemoryBlock FIFO_Memory;

        /// <summary>
        /// PCI device.
        /// </summary>
        private PCIDevice device;
        /// <summary>
        /// Height.
        /// </summary>
        public uint height { get; private set; }
        /// <summary>
        /// Width.
        /// </summary>
        public uint width { get; private set; }
        /// <summary>
        /// Depth.
        /// </summary>
        public uint depth;
        /// <summary>
        /// Capabilities.
        /// </summary>
        private uint capabilities;

        /// <summary>
        /// Initialize FIFO.
        /// </summary>
        protected void InitializeFIFO()
        {
            FIFO_Memory = new MemoryBlock(ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
            FIFO_Memory[(uint)FIFO.Min] = (uint)Register.FifoNumRegisters * sizeof(uint);
            FIFO_Memory[(uint)FIFO.Max] = FIFO_Memory.Size;
            FIFO_Memory[(uint)FIFO.NextCmd] = FIFO_Memory[(uint)FIFO.Min];
            FIFO_Memory[(uint)FIFO.Stop] = FIFO_Memory[(uint)FIFO.Min];
            WriteRegister(Register.ConfigDone, 1);
        }

        /// <summary>
        /// Set video mode.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="depth">Depth.</param>
        public void SetMode(uint width, uint height, uint depth = 32)
        {
            // Depth is color depth in bytes.
            this.depth = (depth / 8);
            this.width = width;
            this.height = height;
            WriteRegister(Register.Width, width);
            WriteRegister(Register.Height, height);
            WriteRegister(Register.BitsPerPixel, depth);
            WriteRegister(Register.Enable, 1);
            InitializeFIFO();

            Init();
        }

        /// <summary>
        /// Write register.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <param name="value">A value.</param>
        public void WriteRegister(Register register, uint value)
        {
            IndexPort.DWord = (uint)register;
            ValuePort.DWord = value;
        }

        /// <summary>
        /// Read register.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <returns>uint value.</returns>
        public uint ReadRegister(Register register)
        {
            IndexPort.DWord = (uint)register;
            return ValuePort.DWord;
        }

        /// <summary>
        /// Get FIFO.
        /// </summary>
        /// <param name="cmd">FIFO command.</param>
        /// <returns>uint value.</returns>
        protected uint GetFIFO(FIFO cmd)
        {
            return FIFO_Memory[(uint)cmd];
        }

        /// <summary>
        /// Set FIFO.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <param name="value">Value.</param>
        /// <returns></returns>
        protected uint SetFIFO(FIFO cmd, uint value)
        {
            return FIFO_Memory[(uint)cmd] = value;
        }

        /// <summary>
        /// Wait for FIFO.
        /// </summary>
        protected void WaitForFifo()
        {
            WriteRegister(Register.Sync, 1);
            while (ReadRegister(Register.Busy) != 0) { }
        }

        /// <summary>
        /// Write to FIFO.
        /// </summary>
        /// <param name="value">Value to write.</param>
        protected void WriteToFifo(uint value)
        {
            if (((GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4) && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min)) ||
                (GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop)))
                WaitForFifo();

            SetFIFO((FIFO)GetFIFO(FIFO.NextCmd), value);
            SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

            if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
                SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
        }

        /// <summary>
        /// Update FIFO.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public void Update(uint x, uint y, uint width, uint height)
        {
            WriteToFifo((uint)FIFOCommand.Update);
            WriteToFifo(x);
            WriteToFifo(y);
            WriteToFifo(width);
            WriteToFifo(height);
            WaitForFifo();
        }

        /// <summary>
        /// Set pixel.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void SetPixel(uint x, uint y, uint color)
        {
            Video_Memory[((y * width + x) * depth)] = color;
        }

        /// <summary>
        /// Get pixel.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>uint value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public uint GetPixel(uint x, uint y)
        {
            return Video_Memory[((y * width + x) * depth)];
        }

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public void Clear(uint color)
        {
            Fill(0, 0, width, height, color);
        }

        /// <summary>
        /// Copy rectangle.
        /// </summary>
        /// <param name="x">Source X coordinate.</param>
        /// <param name="y">Source Y coordinate.</param>
        /// <param name="newX">Destination X coordinate.</param>
        /// <param name="newY">Destination Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public void Copy(uint x, uint y, uint newX, uint newY, uint width, uint height)
        {
            if ((capabilities & (uint)Capability.RectCopy) != 0)
            {
                WriteToFifo((uint)FIFOCommand.RECT_COPY);
                WriteToFifo(x);
                WriteToFifo(y);
                WriteToFifo(newX);
                WriteToFifo(newY);
                WriteToFifo(width);
                WriteToFifo(height);
                WaitForFifo();
            }
            else
                throw new NotImplementedException("VMWareSVGAII Copy()");
        }

        /// <summary>
        /// Fill rectangle.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public void Fill(uint x, uint y, uint width, uint height, uint color)
        {
            if ((capabilities & (uint)Capability.RectFill) != 0)
            {
                WriteToFifo((uint)FIFOCommand.RECT_FILL);
                WriteToFifo(color);
                WriteToFifo(x);
                WriteToFifo(y);
                WriteToFifo(width);
                WriteToFifo(height);
                WaitForFifo();
            }
            else
            {
                if ((capabilities & (uint)Capability.RectCopy) != 0)
                {
                    // fill first line and copy it to all other
                    uint xTarget = (x + width);
                    uint yTarget = (y + height);

                    for (uint xTmp = x; xTmp < xTarget; xTmp++)
                    {
                        SetPixel(xTmp, y, color);
                    }
                    // refresh first line for copy process


                    Update(x, y, width, 1);
                    for (uint yTmp = y + 1; yTmp < yTarget; yTmp++)
                    {
                        Copy(x, y, x, yTmp, width, 1);
                    }
                }
                else
                {
                    uint xTarget = (x + width);
                    uint yTarget = (y + height);
                    for (uint xTmp = x; xTmp < xTarget; xTmp++)
                    {
                        for (uint yTmp = y; yTmp < yTarget; yTmp++)
                        {
                            SetPixel(xTmp, yTmp, color);
                        }
                    }
                    Update(x, y, width, height);
                }
            }
        }

        /// <summary>
        /// Define cursor.
        /// </summary>
        public void DefineCursor()
        {
            WaitForFifo();
            WriteToFifo((uint)FIFOCommand.DEFINE_CURSOR);
            WriteToFifo(1);
            WriteToFifo(0);
            WriteToFifo(0);
            WriteToFifo(2);
            WriteToFifo(2);
            WriteToFifo(1);
            WriteToFifo(1);
            for (int i = 0; i < 4; i++)
                WriteToFifo(0);
            for (int i = 0; i < 4; i++)
                WriteToFifo(0xFFFFFF);
            WaitForFifo();
        }

        public void Disable()
        {
            WriteRegister(Register.Enable, 0);
        }

        /// <summary>
        /// Set cursor.
        /// </summary>
        /// <param name="visible">Visible.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void SetCursor(bool visible, uint x, uint y)
        {
            WriteRegister(Register.CursorID, 1);
            if (visible)
            {
                WaitForFifo();
                WriteToFifo((uint)FIFOCommand.MOVE_CURSOR);
                WriteToFifo(x);
                WriteToFifo(y);
            }
            WriteRegister(Register.CursorOn, (uint)(visible ? 1 : 0));
        }

        /// <summary>
        /// Create new inctanse of the <see cref="VMWareSVGAII"/> class.
        /// </summary>
        public DoubleBufferedVMWareSVGAII()
        {
            device = (Cosmos.HAL.PCI.GetDevice(Cosmos.HAL.VendorID.VMWare, Cosmos.HAL.DeviceID.SVGAIIAdapter));
            device.EnableMemory(true);
            uint basePort = device.BaseAddressBar[0].BaseAddress;
            IndexPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Index));
            ValuePort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Value));
            BiosPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Bios));
            IRQPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.IRQ));

            WriteRegister(Register.ID, (uint)ID.V2);
            if (ReadRegister(Register.ID) != (uint)ID.V2)
                return;

            Video_Memory = new MemoryBlock(ReadRegister(Register.FrameBufferStart), ReadRegister(Register.VRamSize));

            capabilities = ReadRegister(Register.Capabilities);
            InitializeFIFO();
        }
        #endregion

        /*
        ManagedMemoryBlock managedMemoryBlock;

        private void Init()
        {
            managedMemoryBlock = new ManagedMemoryBlock(ReadRegister(Register.FrameBufferSize));
        }

        public void _SetPixel(uint x, uint y, uint color)
        {
            uint offset = (y * width + x) * depth;

            Kernel.text = $"Last Offset: {offset}";

            if (offset < managedMemoryBlock.Size && x < this.width)
            {
                managedMemoryBlock.Write32(offset, color);
            }
        }

        public void _Clear(uint color)
        {
            managedMemoryBlock.Fill(color);
        }

        public void _Update()
        {
            Video_Memory.Copy(managedMemoryBlock);
            WriteToFifo((int)FIFOCommand.Update);
            WriteToFifo(0);
            WriteToFifo(0);
            WriteToFifo(width);
            WriteToFifo(height);
            WaitForFifo();
        }
        */
        
        //By nifanfa
        
        public uint FrameSize;
        public uint FrameOffset;

        private void Init()
        {
            FrameSize = ReadRegister(Register.FrameBufferSize);
            FrameOffset = ReadRegister(Register.FrameBufferOffset);
        }

        public void DoubleBuffer_SetPixel(uint x, uint y, uint color)
        {
            if (x < width)
            {
                Video_Memory[((y * width + x) * depth) + FrameSize] = color;
            }
        }

        public void DoubleBuffer_SetVRAM(int[] colors, int Offset)
        {
            //Video_Memory.Copy((int)FrameSize, colors, 0, colors.Length);
            Video_Memory.Copy(Offset, colors, 0, colors.Length);
        }

        public void DoubleBuffer_Clear(uint color)
        {
            Video_Memory.Fill(FrameSize, FrameSize, color);
        }

        public void DoubleBuffer_Update()
        {
            try
            {
                Video_Memory.MoveDown(FrameOffset, FrameSize, FrameSize);
            }
            catch (Exception)
            {
            }
            Update(0, 0, width, height);
        }

        public void DoubleBuffer_DrawFillRectangle(uint x, uint y, uint width, uint height, uint color)
        {
            for (uint h = 0; h < height; h++)
            {
                for (uint w = 0; w < width; w++)
                {
                    DoubleBuffer_SetPixel(w + x, y + h, color);
                }
            }
        }

        private void DrawVerticalLine(uint color, int dy, int x1, int y1)
        {
            int i;

            for (i = 0; i < dy; i++)
            {
                DoubleBuffer_SetPixel((uint)x1, (uint)(y1 + i), color);
            }
        }

        private void DrawHorizontalLine(uint color, int dx, int x1, int y1)
        {
            uint i;

            for (i = 0; i < dx; i++)
            {
                DoubleBuffer_SetPixel((uint)(x1 + i), (uint)y1, color);
            }
        }

        protected void TrimLine(ref int x1, ref int y1, ref int x2, ref int y2)
        {
            // in case of vertical lines, no need to perform complex operations
            if (x1 == x2)
            {
                x1 = (int)Math.Min(width - 1, Math.Max(0, x1));
                x2 = x1;
                y1 = (int)Math.Min(height - 1, Math.Max(0, y1));
                y2 = (int)Math.Min(height - 1, Math.Max(0, y2));

                return;
            }

            // never attempt to remove this part,
            // if we didn't calculate our new values as floats, we would end up with inaccurate output
            float x1_out = x1, y1_out = y1;
            float x2_out = x2, y2_out = y2;

            // calculate the line slope, and the entercepted part of the y axis
            float m = (y2_out - y1_out) / (x2_out - x1_out);
            float c = y1_out - m * x1_out;

            // handle x1
            if (x1_out < 0)
            {
                x1_out = 0;
                y1_out = c;
            }
            else if (x1_out >= width)
            {
                x1_out = width - 1;
                y1_out = (width - 1) * m + c;
            }

            // handle x2
            if (x2_out < 0)
            {
                x2_out = 0;
                y2_out = c;
            }
            else if (x2_out >= width)
            {
                x2_out = width - 1;
                y2_out = (width - 1) * m + c;
            }

            // handle y1
            if (y1_out < 0)
            {
                x1_out = -c / m;
                y1_out = 0;
            }
            else if (y1_out >= height)
            {
                x1_out = (height - 1 - c) / m;
                y1_out = height - 1;
            }

            // handle y2
            if (y2_out < 0)
            {
                x2_out = -c / m;
                y2_out = 0;
            }
            else if (y2_out >= height)
            {
                x2_out = (height - 1 - c) / m;
                y2_out = height - 1;
            }

            // final check, to avoid lines that are totally outside bounds
            if (x1_out < 0 || x1_out >= width || y1_out < 0 || y1_out >= height)
            {
                x1_out = 0; x2_out = 0;
                y1_out = 0; y2_out = 0;
            }

            if (x2_out < 0 || x2_out >= width || y2_out < 0 || y2_out >= height)
            {
                x1_out = 0; x2_out = 0;
                y1_out = 0; y2_out = 0;
            }

            // replace inputs with new values
            x1 = (int)x1_out; y1 = (int)y1_out;
            x2 = (int)x2_out; y2 = (int)y2_out;
        }

        public virtual void DoubleBuffer_DrawImage(Image image, uint x, uint y)
        {
            for (uint _x = 0; _x < image.Width; _x++)
            {
                for (uint _y = 0; _y < image.Height; _y++)
                {
                    DoubleBuffer_SetPixel(x + _x, y + _y, (uint)image.rawData[_x + _y * image.Width]);
                }
            }
        }

        public virtual void DoubleBuffer_DrawLine(uint color, int x1, int y1, int x2, int y2)
        {
            // trim the given line to fit inside the canvas boundries
            TrimLine(ref x1, ref y1, ref x2, ref y2);

            int dx, dy;

            dx = x2 - x1;      /* the horizontal distance of the line */
            dy = y2 - y1;      /* the vertical distance of the line */

            if (dy == 0) /* The line is horizontal */
            {
                DrawHorizontalLine(color, dx, x1, y1);
                return;
            }

            if (dx == 0) /* the line is vertical */
            {
                DrawVerticalLine(color, dy, x1, y1);
                return;
            }

            /* the line is neither horizontal neither vertical, is diagonal then! */
            DrawDiagonalLine(color, dx, dy, x1, y1);
        }

        public virtual void DoubleBuffer_DrawRectangle(uint color, int x, int y, int width, int height)
        {
            /* The check of the validity of x and y are done in DrawLine() */

            /* The vertex A is where x,y are */
            int xa = x;
            int ya = y;

            /* The vertex B has the same y coordinate of A but x is moved of width pixels */
            int xb = x + width;
            int yb = y;

            /* The vertex C has the same x coordiate of A but this time is y that is moved of height pixels */
            int xc = x;
            int yc = y + height;

            /* The Vertex D has x moved of width pixels and y moved of height pixels */
            int xd = x + width;
            int yd = y + height;

            /* Draw a line betwen A and B */
            DoubleBuffer_DrawLine(color, xa, ya, xb, yb);

            /* Draw a line between A and C */
            DoubleBuffer_DrawLine(color, xa, ya, xc, yc);

            /* Draw a line between B and D */
            DoubleBuffer_DrawLine(color, xb, yb, xd, yd);

            /* Draw a line between C and D */
            DoubleBuffer_DrawLine(color, xc, yc, xd, yd);
        }

        private void DrawDiagonalLine(uint color, int dx, int dy, int x1, int y1)
        {
            int i, sdx, sdy, dxabs, dyabs, x, y, px, py;

            dxabs = Math.Abs(dx);
            dyabs = Math.Abs(dy);
            sdx = Math.Sign(dx);
            sdy = Math.Sign(dy);
            x = dyabs >> 1;
            y = dxabs >> 1;
            px = x1;
            py = y1;

            if (dxabs >= dyabs) /* the line is more horizontal than vertical */
            {
                for (i = 0; i < dxabs; i++)
                {
                    y += dyabs;
                    if (y >= dxabs)
                    {
                        y -= dxabs;
                        py += sdy;
                    }
                    px += sdx;
                    DoubleBuffer_SetPixel((uint)px, (uint)py, color);
                }
            }
            else /* the line is more vertical than horizontal */
            {
                for (i = 0; i < dyabs; i++)
                {
                    x += dxabs;
                    if (x >= dyabs)
                    {
                        x -= dyabs;
                        px += sdx;
                    }
                    py += sdy;
                    DoubleBuffer_SetPixel((uint)px, (uint)py, color);
                }
            }
        }
    }
}