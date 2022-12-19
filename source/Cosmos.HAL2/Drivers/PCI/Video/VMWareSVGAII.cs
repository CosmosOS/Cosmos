using System;
using Cosmos.Core;

namespace Cosmos.HAL.Drivers.PCI.Video
{
    /// <summary>
    /// VMWareSVGAII class.
    /// </summary>
    public class VMWareSVGAII
    {
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
            /// Cursor count.
            /// </summary>
            CursorCount = 0x0C,
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
        private readonly ushort IndexPort;
        /// <summary>
        /// Value port.
        /// </summary>
        private readonly ushort ValuePort;
        /// <summary>
        /// BIOS port.
        /// </summary>
        private ushort BiosPort;
        /// <summary>
        /// IRQ port.
        /// </summary>
        private ushort IRQPort;

        /// <summary>
        /// Video memory block.
        /// </summary>
        public readonly MemoryBlock VideoMemory;
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
        private uint height;
        /// <summary>
        /// Width.
        /// </summary>
        private uint width;
        /// <summary>
        /// Depth.
        /// </summary>
        private uint depth;
        /// <summary>
        /// Capabilities.
        /// </summary>
        private uint capabilities;

        public uint FrameSize;
        public uint FrameOffset;

        /// <summary>
        /// Create new instance of the <see cref="VMWareSVGAII"/> class.
        /// </summary>
        public VMWareSVGAII()
        {
            device = HAL.PCI.GetDevice(HAL.VendorID.VMWare, HAL.DeviceID.SVGAIIAdapter);
            device.EnableMemory(true);
            uint basePort = device.BaseAddressBar[0].BaseAddress;
            IndexPort = (ushort)(basePort + (uint)IOPortOffset.Index);
            ValuePort = (ushort)(basePort + (uint)IOPortOffset.Value);
            BiosPort = (ushort)(basePort + (uint)IOPortOffset.Bios);
            IRQPort = (ushort)(basePort + (uint)IOPortOffset.IRQ);

            WriteRegister(Register.ID, (uint)ID.V2);
            if (ReadRegister(Register.ID) != (uint)ID.V2)
                return;

            VideoMemory = new MemoryBlock(ReadRegister(Register.FrameBufferStart), ReadRegister(Register.VRamSize));
            capabilities = ReadRegister(Register.Capabilities);
            InitializeFIFO();
        }

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
            //Disable the Driver before writing new values and initiating it again to avoid a memory exception
            //Disable();

            // Depth is color depth in bytes.
            this.depth = depth / 8;
            this.width = width;
            this.height = height;
            WriteRegister(Register.Width, width);
            WriteRegister(Register.Height, height);
            WriteRegister(Register.BitsPerPixel, depth);
            Enable();
            InitializeFIFO();

            FrameSize = ReadRegister(Register.FrameBufferSize);
            FrameOffset = ReadRegister(Register.FrameBufferOffset);
        }

        /// <summary>
        /// Write register.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <param name="value">A value.</param>
        protected void WriteRegister(Register register, uint value)
        {
            IOPort.Write32(IndexPort, (uint)register);
            IOPort.Write32(ValuePort, value);
        }

        /// <summary>
        /// Read register.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <returns>uint value.</returns>
        protected uint ReadRegister(Register register)
        {
            IOPort.Write32(IndexPort, (uint)register);
            return IOPort.Read32(ValuePort);
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
            if ((GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4 && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min)) ||
                GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop))
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
        /// Update video memory.
        /// </summary>
        public void DoubleBufferUpdate()
        {
            VideoMemory.MoveDown(FrameOffset, FrameSize, FrameSize);
            Update(0, 0, width, height);
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
            VideoMemory[(y * width + x) * depth + FrameSize] = color;
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
            return VideoMemory[(y * width + x) * depth + FrameSize];
        }

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public void Clear(uint color)
        {
            VideoMemory.Fill(FrameSize, FrameSize, color);
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
                    uint xTarget = x + width;
                    uint yTarget = y + height;

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
                    uint xTarget = x + width;
                    uint yTarget = y + height;
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
            WriteToFifo(0); // ID
            WriteToFifo(0); // Hotspot X
            WriteToFifo(0); // Hotspot Y
            WriteToFifo(2);
            WriteToFifo(2);
            WriteToFifo(1);
            WriteToFifo(1);

            for (int i = 0; i < 4; i++)
            {
                WriteToFifo(0);
            }

            for (int i = 0; i < 4; i++)
            {
                WriteToFifo(0xFFFFFF);
            }

            WaitForFifo();
        }

        /// <summary>
        /// Define alpha cursor.
        /// </summary>
        public void DefineAlphaCursor(uint width, uint height, int[] data)
        {
            WaitForFifo();
            WriteToFifo((uint)FIFOCommand.DEFINE_ALPHA_CURSOR);
            WriteToFifo(0); // ID
            WriteToFifo(0); // Hotspot X
            WriteToFifo(0); // Hotspot Y
            WriteToFifo(width); // Width
            WriteToFifo(height); // Height

            for (int i = 0; i < data.Length; i++)
            {
                WriteToFifo((uint)data[i]);
            }

            WaitForFifo();
        }

        //Allow to enable the Driver again after it has been disabled (switch between text and graphics mode currently this is SVGA only)
        /// <summary>
        /// Enable the SVGA Driver , only needed after Disable() has been called
        /// </summary>
        public void Enable()
        {
            WriteRegister(Register.Enable, 1);
        }

        /// <summary>
        /// Disable the SVGA Driver , return to text mode
        /// </summary>
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
            WriteRegister(Register.CursorOn, (uint)(visible ? 1 : 0));
            WriteRegister(Register.CursorX, x);
            WriteRegister(Register.CursorY, y);
            WriteRegister(Register.CursorCount, ReadRegister(Register.CursorCount) + 1);
        }
    }
}
