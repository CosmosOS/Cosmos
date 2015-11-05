using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Orvid.Graphics.Drivers
{
    public class VMWareSVGAII : GraphicsDriver
    {

        #region Register
        public enum Register : byte
        {
            ID = 0,
            Enable = 1,
            Width = 2,
            Height = 3,
            MaxWidth = 4,
            MaxHeight = 5,
            Depth = 6,
            BitsPerPixel = 7,
            PseudoColor = 8,
            RedMask = 9,
            GreenMask = 10,
            BlueMask = 11,
            BytesPerLine = 12,
            FrameBufferStart = 13,
            FrameBufferOffset = 14,
            VRamSize = 15,
            FrameBufferSize = 16,

            Capabilities = 17,
            MemStart = 18,
            MemSize = 19,
            ConfigDone = 20,
            Sync = 21,
            Busy = 22,
            GuestID = 23,
            CursorID = 24,
            CursorX = 25,
            CursorY = 26,
            CursorOn = 27,
            HostBitsPerPixel = 28,
            ScratchSize = 29,
            MemRegs = 30,
            NumDisplays = 31,
            PitchLock = 32
        }
        #endregion

        #region ID
        private enum ID : uint
        {
            Magic = 0x900000,
            V0 = Magic << 8,
            V1 = (Magic << 8) | 1,
            V2 = (Magic << 8) | 2,
            Invalid = 0xFFFFFFFF
        }
        #endregion

        #region FIFO
        public enum FIFO : uint
        {
            Min = 0,
            Max = 1,
            NextCmd = 2,
            Stop = 3
        }
        #endregion

        #region FIFOCommand
        private enum FIFOCommand
        {
            Update = 1,
            RECT_FILL = 2,
            RECT_COPY = 3,
            DEFINE_BITMAP = 4,
            DEFINE_BITMAP_SCANLINE = 5,
            DEFINE_PIXMAP = 6,
            DEFINE_PIXMAP_SCANLINE = 7,
            RECT_BITMAP_FILL = 8,
            RECT_PIXMAP_FILL = 9,
            RECT_BITMAP_COPY = 10,
            RECT_PIXMAP_COPY = 11,
            FREE_OBJECT = 12,
            RECT_ROP_FILL = 13,
			RECT_ROP_COPY = 14,
			RECT_ROP_BITMAP_FILL = 15,
			RECT_ROP_PIXMAP_FILL = 16,
			RECT_ROP_BITMAP_COPY = 17,
			RECT_ROP_PIXMAP_COPY = 18,
			DEFINE_CURSOR = 19,
			DISPLAY_CURSOR = 20,
			MOVE_CURSOR = 21,
			DEFINE_ALPHA_CURSOR = 22
        }
        #endregion

        #region IOPortOffset
        private enum IOPortOffset : byte
        {
            Index = 0,
            Value = 1,
            Bios = 2,
            IRQ = 3
        }
        #endregion

        private Cosmos.Core.IOPort IndexPort;
        private Cosmos.Core.IOPort ValuePort;
        private Cosmos.Core.IOPort BiosPort;
        private Cosmos.Core.IOPort IRQPort;

        private Cosmos.Core.MemoryBlock Video_Memory;
        private Cosmos.Core.MemoryBlock FIFO_Memory;

        private PCIDeviceNormal device;

        public VMWareSVGAII()
        {
            device = (PCIDeviceNormal)Cosmos.Core.PCI.GetDevice(0x15AD, 0x0405);
            device.EnableMemory(true);
            uint basePort = device.BaseAddresses[0].BaseAddress();
            IndexPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Index));
            ValuePort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Value));
            BiosPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Bios));
            IRQPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.IRQ));

            WriteRegister(Register.ID, (uint)ID.V2);
            if (ReadRegister(Register.ID) != (uint)ID.V2)
                return;

            Video_Memory = new MemoryBlock(ReadRegister(Register.FrameBufferStart), ReadRegister(Register.VRamSize));            
            InitializeFIFO();
        }

        protected void InitializeFIFO()
        {
            FIFO_Memory = new MemoryBlock(ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
            FIFO_Memory[(uint)FIFO.Min] = 16;
            FIFO_Memory[(uint)FIFO.Max] = FIFO_Memory.Size;
            FIFO_Memory[(uint)FIFO.NextCmd] = 16;
            FIFO_Memory[(uint)FIFO.Stop] = 16;
            WriteRegister(Register.ConfigDone, 1);
        }

        protected void SetMode(ushort width, ushort height, ushort depth)
        {
            WriteRegister(Register.Width, width);
            WriteRegister(Register.Height, height);
            WriteRegister(Register.BitsPerPixel, depth);
            WriteRegister(Register.Enable, 1);
            InitializeFIFO();
        }

        protected void WriteRegister(Register register, uint value)
        {
            IndexPort.DWord = (uint)register;
            ValuePort.DWord = value;
        }

        protected uint ReadRegister(Register register)
        {
            IndexPort.DWord = (uint)register;
            return ValuePort.DWord;
        }

        protected uint GetFIFO(FIFO cmd)
        {
            return FIFO_Memory[(uint)cmd];
        }

        protected uint SetFIFO(FIFO cmd, uint value)
        {
            return FIFO_Memory[(uint)cmd] = value;
        }

        protected void WaitForFifo()
        {
            WriteRegister(Register.Sync, 1);
            while (ReadRegister(Register.Busy) != 0){}
        }

        protected void WriteToFifo(uint value)
        {
            if (((GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4) && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min)) ||
                (GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop)))
                WaitForFifo();

            SetFIFO((FIFO)(GetFIFO(FIFO.NextCmd) / 4), value);
            SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

            if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
                SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
        }

        public void Update(ushort x, ushort y, ushort width, ushort height)
        {
            WriteToFifo((uint)FIFOCommand.Update);
            WriteToFifo(x);
            WriteToFifo(y);
            WriteToFifo(width);
            WriteToFifo(height);
            WaitForFifo();
        }

        public void SetPixel(ushort x, ushort y, uint color)
        {
            Video_Memory[(uint)((y * ReadRegister(Register.Width)) + x)] = color;
        }

        public uint GetPixel(ushort x, ushort y)
        {
            return Video_Memory[(uint)((y * ReadRegister(Register.Width)) + x)];
        }


        public void Clear(uint color)
        {
            Fill(0, 0, 800, 600, color);
        }

        public void Fill(ushort x, ushort y, ushort width, ushort height, uint color)
        {
            WriteToFifo((uint)FIFOCommand.RECT_FILL);
            WriteToFifo(color);
            WriteToFifo(x);
            WriteToFifo(y);
            WriteToFifo(width);
            WriteToFifo(height);
            WaitForFifo();
        }

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

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override string Version
        {
            get { throw new NotImplementedException(); }
        }

        public override string Company
        {
            get { throw new NotImplementedException(); }
        }

        public override string Author
        {
            get { throw new NotImplementedException(); }
        }

        public override GraphicsMode Mode
        {
            get { throw new NotImplementedException(); }
        }

        public override void Update(Image i)
        {
            for (ushort x = 0; x < i.Width; x++)
            {
                for (ushort y = 0; y < i.Height; y++)
                {
                    SetPixel(x, y, i.GetPixel(x, y).ToUInt());
                }
            }
        }

        public override List<GraphicsMode> GetSupportedModes()
        {
            throw new NotImplementedException();
        }

        public override void SetMode(GraphicsMode mode)
        {
            SetMode(800, 600, 32);
        }

        public override bool Supported()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            device = (PCIDeviceNormal)Cosmos.Core.PCI.GetDevice(0x15AD, 0x0405);
            device.EnableMemory(true);
            uint basePort = device.BaseAddresses[0].BaseAddress();
            IndexPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Index));
            ValuePort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Value));
            BiosPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.Bios));
            IRQPort = new IOPort((ushort)(basePort + (uint)IOPortOffset.IRQ));

            WriteRegister(Register.ID, (uint)ID.V2);
            if (ReadRegister(Register.ID) != (uint)ID.V2)
                return;

            Video_Memory = new MemoryBlock(ReadRegister(Register.FrameBufferStart), ReadRegister(Register.VRamSize));
            InitializeFIFO();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
