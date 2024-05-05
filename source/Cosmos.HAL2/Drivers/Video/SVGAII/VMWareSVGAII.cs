using System;
using Cosmos.Core;
using Cosmos.HAL.BlockDevice.Registers;

namespace Cosmos.HAL.Drivers.Video.SVGAII
{
    /// <summary>
    /// VMWareSVGAII class.
    /// </summary>
    public class VMWareSVGAII
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VMWareSVGAII"/> class.
        /// </summary>
        public VMWareSVGAII()
        {
            device = HAL.PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);
            device.EnableMemory(true);
            uint basePort = device.BaseAddressBar[0].BaseAddress;
            indexPort = (ushort)(basePort + (uint)IOPortOffset.Index);
            valuePort = (ushort)(basePort + (uint)IOPortOffset.Value);

            WriteRegister(Register.ID, (uint)ID.V2);
            if (ReadRegister(Register.ID) != (uint)ID.V2)
            {
                return;
            }

            videoMemory = new MemoryBlock(ReadRegister(Register.FrameBufferStart), ReadRegister(Register.VRamSize));
            capabilities = ReadRegister(Register.Capabilities);
            InitializeFIFO();
        }

        #region Methods

        /// <summary>
        /// Initialize FIFO.
        /// </summary>
        public void InitializeFIFO()
        {
            fifoMemory = new MemoryBlock(ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
            fifoMemory[(uint)FIFO.Min] = (uint)Register.FifoNumRegisters * sizeof(uint);
            fifoMemory[(uint)FIFO.Max] = fifoMemory.Size;
            fifoMemory[(uint)FIFO.NextCmd] = fifoMemory[(uint)FIFO.Min];
            fifoMemory[(uint)FIFO.Stop] = fifoMemory[(uint)FIFO.Min];
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
        public void WriteRegister(Register register, uint value)
        {
            IOPort.Write32(indexPort, (uint)register);
            IOPort.Write32(valuePort, value);
        }

        /// <summary>
        /// Read register.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <returns>uint value.</returns>
        public uint ReadRegister(Register register)
        {
            IOPort.Write32(indexPort, (uint)register);
            return IOPort.Read32(valuePort);
        }

        /// <summary>
        /// Get FIFO.
        /// </summary>
        /// <param name="cmd">FIFO command.</param>
        /// <returns>uint value.</returns>
        public uint GetFIFO(FIFO cmd) => fifoMemory[(uint)cmd];

        /// <summary>
        /// Set FIFO.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <param name="value">Value.</param>
        /// <returns></returns>
        public uint SetFIFO(FIFO cmd, uint value) => fifoMemory[(uint)cmd] = value;

        /// <summary>
        /// Wait for FIFO.
        /// </summary>
        public void WaitForFifo()
        {
            WriteRegister(Register.Sync, 1);
            while (ReadRegister(Register.Busy) != 0) { }
        }

        /// <summary>
        /// Write to FIFO.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void WriteToFifo(uint value)
        {
            if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4 && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min) ||
                GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop))
            {
                WaitForFifo();
            }

            SetFIFO((FIFO)GetFIFO(FIFO.NextCmd), value);
            SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

            if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
            {
                SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
            }
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
        /// </summary>S
        public void DoubleBufferUpdate()
        {
            videoMemory.MoveDown(FrameOffset, FrameSize, FrameSize);
            Update(0, 0, width, height);
        }

        public void PartUpdate(uint x, uint y, uint UpdateWidth, uint UpdateHeight)
        {
            uint Offset = y * width + x;
            for (uint i = 0; i < UpdateHeight; i++)
            {
                videoMemory.MoveDown(FrameOffset + Offset, FrameSize + Offset, UpdateWidth);
                Offset += UpdateHeight;

            }

            Update(x, y, UpdateWidth*2, UpdateHeight);
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
            videoMemory[(y * width + x) * depth + FrameSize] = color;
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
            return videoMemory[(y * width + x) * depth + FrameSize];
        }

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectange copy capability</exception>
        public void Clear(uint color)
        {
            videoMemory.Fill(FrameSize, FrameSize, color);
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
            {
                throw new NotImplementedException("VMWareSVGAII Copy()");
            }
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

        /// <summary>
        /// Enable the SVGA Driver, only needed after Disable() has been called.
        /// </summary>
        public void Enable()
        {
            WriteRegister(Register.Enable, 1);
        }

        /// <summary>
        /// Disable the SVGA Driver, returns to text mode.
        /// </summary>
        public void Disable()
        {
            WriteRegister(Register.Enable, 0);
        }

        /// <summary>
        /// Sets the cursor position and draws it.
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

        #endregion

        #region Fields

        /// <summary>
        /// Index port.
        /// </summary>
        private readonly ushort indexPort;
        /// <summary>
        /// Value port.
        /// </summary>
        private readonly ushort valuePort;

        /// <summary>
        /// Video memory block.
        /// </summary>
        public readonly MemoryBlock videoMemory;

        /// <summary>
        /// FIFO memory block.
        /// </summary>
        private MemoryBlock fifoMemory;

        /// <summary>
        /// PCI device.
        /// </summary>
        private readonly PCIDevice device;

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
        private readonly uint capabilities;

        public uint FrameSize;
        public uint FrameOffset;

        #endregion

        
    }
}