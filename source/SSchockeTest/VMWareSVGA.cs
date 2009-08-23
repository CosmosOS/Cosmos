using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace Cosmos.Playground.SSchocke
{
    public class VMWareSVGA
    {
        private enum SVGA_REG
        {
            ID = 0,
            ENABLE = 1,
            WIDTH = 2,
            HEIGHT = 3,
            MAX_WIDTH = 4,
            MAX_HEIGHT = 5,
            DEPTH = 6,
            BITS_PER_PIXEL = 7,       /* Current bpp in the guest */
            PSEUDOCOLOR = 8,
            RED_MASK = 9,
            GREEN_MASK = 10,
            BLUE_MASK = 11,
            BYTES_PER_LINE = 12,
            FB_START = 13,            /* (Deprecated) */
            FB_OFFSET = 14,
            VRAM_SIZE = 15,
            FB_SIZE = 16,

            /* ID 0 implementation only had the above registers, then the palette */

            CAPABILITIES = 17,
            MEM_START = 18,           /* (Deprecated) */
            MEM_SIZE = 19,
            CONFIG_DONE = 20,         /* Set when memory area configured */
            SYNC = 21,                /* See "FIFO Synchronization Registers" */
            BUSY = 22,                /* See "FIFO Synchronization Registers" */
            GUEST_ID = 23,            /* Set guest OS identifier */
            CURSOR_ID = 24,           /* (Deprecated) */
            CURSOR_X = 25,            /* (Deprecated) */
            CURSOR_Y = 26,            /* (Deprecated) */
            CURSOR_ON = 27,           /* (Deprecated) */
            HOST_BITS_PER_PIXEL = 28, /* (Deprecated) */
            SCRATCH_SIZE = 29,        /* Number of scratch registers */
            MEM_REGS = 30,            /* Number of FIFO registers */
            NUM_DISPLAYS = 31,        /* (Deprecated) */
            PITCHLOCK = 32,           /* Fixed pitch for all modes */
            IRQMASK = 33,             /* Interrupt mask */

            /* Legacy multi-monitor support */
            NUM_GUEST_DISPLAYS = 34,/* Number of guest displays in X/Y direction */
            DISPLAY_ID = 35,        /* Display ID for the following display attributes */
            DISPLAY_IS_PRIMARY = 36,/* Whether this is a primary display */
            DISPLAY_POSITION_X = 37,/* The display position x */
            DISPLAY_POSITION_Y = 38,/* The display position y */
            DISPLAY_WIDTH = 39,     /* The display's width */
            DISPLAY_HEIGHT = 40,    /* The display's height */

            /* See "Guest memory regions" below. */
            GMR_ID = 41,
            GMR_DESCRIPTOR = 42,
            GMR_MAX_IDS = 43,
            GMR_MAX_DESCRIPTOR_LENGTH = 44,

            TRACES = 45,            /* Enable trace-based updates even when FIFO is on */
            TOP = 46,               /* Must be 1 more than the last register */

            SVGA_PALETTE_BASE = 1024,        /* Base of SVGA color map */
            /* Next 768 (== 256*3) registers exist for colormap */

            SVGA_SCRATCH_BASE = SVGA_PALETTE_BASE + 768
            /* Base of scratch registers */
            /* Next reg[SCRATCH_SIZE] registers exist for scratch usage:
               First 4 are reserved for VESA BIOS Extension; any remaining are for
               the use of the current SVGA driver. */
        };
        private enum SVGA_CAP
        {
            NONE = 0x00000000,
            RECT_COPY = 0x00000002,
            CURSOR = 0x00000020,
            CURSOR_BYPASS = 0x00000040,   // Legacy (Use Cursor Bypass 3 instead)
            CURSOR_BYPASS_2 = 0x00000080,   // Legacy (Use Cursor Bypass 3 instead)
            _8BIT_EMULATION = 0x00000100,
            ALPHA_CURSOR = 0x00000200,
            _3D = 0x00004000,
            EXTENDED_FIFO = 0x00008000,
            MULTIMON = 0x00010000,   // Legacy multi-monitor support
            PITCHLOCK = 0x00020000,
            IRQMASK = 0x00040000,
            DISPLAY_TOPOLOGY = 0x00080000,   // Legacy multi-monitor support
            GMR = 0x00100000,
            TRACES = 0x00200000
        };
        private enum SVGA_FIFO_CAP
        {
            NONE = 0,
            FENCE = 0x01,
            ACCELFRONT = 0x02,
            PITCHLOCK = 0x04,
            VIDEO = 0x08,
            CURSOR_BYPASS_3 = 0x10,
            ESCAPE = 0x20,
            RESERVE = 0x40,
        };
        private enum SVGA_CMD
        {
            INVALID_CMD = 0,
            UPDATE = 1,
            RECT_COPY = 3,
            DEFINE_CURSOR = 19,
            DEFINE_ALPHA_CURSOR = 22,
            UPDATE_VERBOSE = 25,
            FRONT_ROP_FILL = 29,
            FENCE = 30,
            ESCAPE = 33,
            MAX
        };

        private class FifoStruct
        {
           /*struct {
              uint32  reservedSize;
              Bool    usingBounceBuffer;
              uint8   bounceBuffer[1024 * 1024];
              uint32  nextFence;
           } fifo;*/
            internal UInt32 reservedSize;
            internal bool usingBounceEffect;
            internal ManagedMemorySpace bounceBuffer;
            internal UInt32 nextFence;

            public FifoStruct()
            {
                this.bounceBuffer = new ManagedMemorySpace(1024 * 1024, 4);
            }
        }

        public class Rect
        {
            public int x, y, w, h;

            public Rect(int xPos, int yPos, int width, int height)
            {
                this.x = xPos;
                this.y = yPos;
                this.w = width;
                this.h = height;
            }
        }
        public class BackBuffer
        {
            public MemoryAddressSpace buffer;
            internal List<Rect> dirtyRecs;

            public BackBuffer()
            {
                dirtyRecs = new List<Rect>();
            }

            public void MarkDirty(Rect r)
            {
                dirtyRecs.Add(new Rect(r.x, r.y, r.w, r.h));
            }

            public Rect[] DirtyRecs
            {
                get { return this.dirtyRecs.ToArray(); }
            }

            internal unsafe void DrawRect(Rect r, uint color)
            {
                uint bytesPerPixel = 4;
                uint bytesPerLine = 1024 * bytesPerPixel;

                for (int y = 0; y < r.h; y++)
                {
                    uint offset = (uint)(((r.y + y) * bytesPerLine) + (r.x * bytesPerPixel));
                    for (uint x = 0; x < (r.w * bytesPerPixel); x += 4)
                    {
                        (*(uint*)(buffer.Offset + offset + x)) = color;
                    }
                }
            }
        };

        public unsafe class VideoMemoryAddressSpace : MemoryAddressSpace
        {
            public VideoMemoryAddressSpace(UInt32 offset, UInt32 size)
                : base(offset, size)
            { }

        }

        private PCIDevice pciDev;
        private IOAddressSpace ioBase;
        private MemoryAddressSpace fifoMem;
        public MemoryAddressSpace fbMem;
        private UInt32 fifoSize;
        private UInt32 fbSize;

        private UInt32 deviceVersionID;
        private UInt32 capabilities;

        private UInt32 width;
        private UInt32 height;
        private UInt32 bpp;
        private UInt32 pitch;

        private FifoStruct fifo;

        private const UInt32 SVGA_MAGIC = 0x900000;
        private const UInt32 SVGA_ID_2 = (SVGA_MAGIC << 8) | 2;
        private const UInt32 SVGA_ID_1 = (SVGA_MAGIC << 8) | 1;
        private const UInt32 SVGA_ID_0 = (SVGA_MAGIC << 8) | 0;

        private const UInt32 SVGA_INDEX_PORT = 0x0;
        private const UInt32 SVGA_VALUE_PORT = 0x1;
        private const UInt32 SVGA_IRQSTATUS_PORT = 0x8;

        private const UInt32 SVGA3D_HWVERSION_CURRENT = 2 << 16;

        public static void Test()
        {
            VMWareSVGA vmsvga = null;

            Console.WriteLine("Scanning for VMWare SVGA cards...");
            foreach (PCIDevice device in Cosmos.Hardware.PCIBus.Devices)
            {
                if ((device.VendorID == 0x15AD) && (device.DeviceID == 0x0405))
                {
                    Console.WriteLine("Found VMWare SVGA on PCI " + device.Bus + ":" + device.Slot + ":" + device.Function);
                    Console.WriteLine("VGA IRQ: " + device.InterruptLine);
                    vmsvga = new VMWareSVGA(device);
                }
            }

            if (vmsvga == null)
            {
                Console.WriteLine("No VMWare SVGA Adapter found!!");
                return;
            }

            vmsvga.SetMode(1024, 768, 32);
            BackBuffer back = vmsvga.CreateBackBuffer();
            Rect screen = new Rect(0, 0, 1024, 768);
            Rect block = new Rect(100, 100, 100, 100);
            DebugUtil.SendMessage("VMWare", "Screen Rect: r.x=" + screen.x + ", r.y=" + screen.y
                                            + ", r.w=" + screen.w + ", r.h=" + screen.h);

            back.buffer.SetMem(0);
            back.MarkDirty(screen);
            vmsvga.SyncBuffers(back);

            uint color = 0xFF333333;
            int ydir = 2;
            int xdir = 2;

            while (true)
            {
                //DebugUtil.SendMessage("VMWare", "BackBuffer: offset=" + back.buffer.Offset + ", size=" + back.buffer.Size);
                //DebugUtil.SendMessage("VMWare", "FrameBuffer: offset=" + vmsvga.fbMem.Offset + ", size=" + vmsvga.fbMem.Size);
                back.DrawRect(block, color);
                back.MarkDirty(block);
                vmsvga.SyncBuffers(back);
                back.DrawRect(block, 0xFF000000);
                back.MarkDirty(block);

                block.x += xdir;
                block.y += ydir;
                if (block.x + block.w >= vmsvga.width)
                {
                    xdir *= -1;
                    color += 16;
                }
                if (block.y + block.h >= vmsvga.height)
                {
                    ydir *= -1;
                    color += 16;
                }
                if (block.x <= 0)
                {
                    xdir *= -1;
                    color += 16;
                }
                if (block.y <= 0)
                {
                    ydir *= -1;
                    color += 16;
                }
            }
        }

        public VMWareSVGA(PCIDevice dev)
        {
            this.pciDev = dev;
            this.fifo = new FifoStruct();

            ioBase = pciDev.GetAddressSpace(0) as Kernel.IOAddressSpace;
            fbMem = pciDev.GetAddressSpace(1) as MemoryAddressSpace;
            fifoMem = pciDev.GetAddressSpace(2) as Kernel.MemoryAddressSpace;

            // Determine device revision ID
            this.deviceVersionID = SVGA_ID_2;
            do
            {
                writeSVGARegister(SVGA_REG.ID, this.deviceVersionID);
                if (readSVGARegister(SVGA_REG.ID) == this.deviceVersionID)
                {
                    break;
                }
                else
                {
                    this.deviceVersionID--;
                }
            } while (this.deviceVersionID >= SVGA_ID_0);

            if (this.deviceVersionID < SVGA_ID_0)
            {
                Console.WriteLine("Unsupported Revision of VMWare SVGA adapter!!");
                return;
            }

            this.fbSize = readSVGARegister(SVGA_REG.FB_SIZE);
            this.fifoSize = readSVGARegister(SVGA_REG.MEM_SIZE);

            if (this.fbSize < 0x100000)
            {
                Console.WriteLine("Framebuffer Size too small... something is wrong!!");
                return;
            }
            if (this.fifoSize < 0x20000)
            {
                Console.WriteLine("FIFO Size too small... something is wrong!!");
                return;
            }

            if (deviceVersionID >= SVGA_ID_1)
            {
                this.capabilities = readSVGARegister(SVGA_REG.CAPABILITIES);
            }

            if ((this.capabilities & (uint)SVGA_CAP.IRQMASK) != 0)
            {
                writeSVGARegister(SVGA_REG.IRQMASK, 0);
                ioBase.Write32(SVGA_IRQSTATUS_PORT, 0xFF);

                Console.WriteLine("Adapter has Interrupt support, but not supported at present by us!!");
            }

            uint fifo_caps = this.fifoMem.Read32(16);
            Console.WriteLine("Adapter Properties: versionID=" + (deviceVersionID & 0xFF) + ", fbSize=" + fbSize
                + ", fifoSize=" + fifoSize + ", capabilities=" + capabilities + "fifo_cap=" + fifo_caps);
            DebugUtil.SendMessage("VMWare", "Adapter Properties: versionID=" + (deviceVersionID & 0xFF) + ", fbSize=" + fbSize
                + ", fifoSize=" + fifoSize + ", capabilities=" + capabilities + "fifo_cap=" + fifo_caps);
        }

        public void SetMode(uint width, uint height, uint bpp)
        {
            this.width = width;
            this.height = height;
            this.bpp = bpp;

            writeSVGARegister(SVGA_REG.WIDTH, width);
            writeSVGARegister(SVGA_REG.HEIGHT, height);
            writeSVGARegister(SVGA_REG.BITS_PER_PIXEL, bpp);
            writeSVGARegister(SVGA_REG.ENABLE, 1);
            this.pitch = readSVGARegister(SVGA_REG.BYTES_PER_LINE);

            // Initialize the command FIFO
            this.fifoMem.Write32(0, 1164); // MIN = NUM_REGS * 4
            this.fifoMem.Write32(4, this.fifoSize); // MAX = end of FIFO mem
            this.fifoMem.Write32(8, 1164); // NEXT_CMD = MIN
            this.fifoMem.Write32(12, 1164); // STOP = MIN

            if ((hasFIFOCap(SVGA_CAP.EXTENDED_FIFO) == true) && (isFIFORegValid(1152) == true)) // FIFO_GUEST_3D_HW_VERSION
            {
                fifoMem.Write32(1152, SVGA3D_HWVERSION_CURRENT);// FIFO_GUEST_3D_HW_VERSION
            }

            // Enable the FIFO
            writeSVGARegister(SVGA_REG.CONFIG_DONE, 1);

            this.fbSize = readSVGARegister(SVGA_REG.FB_SIZE);
            this.fifoSize = readSVGARegister(SVGA_REG.MEM_SIZE);

            uint fbOffset = readSVGARegister(SVGA_REG.FB_OFFSET);
            DebugUtil.SendMessage("VMWare", "Adapter Properties: versionID=" + (deviceVersionID & 0xFF) + ", fbSize=" + fbSize
                + ", fifoSize=" + fifoSize + ", capabilities=" + capabilities + ", fbOffset=" + fbOffset);
        }

        public BackBuffer CreateBackBuffer()
        {
            BackBuffer tmp = new BackBuffer();
            //tmp.buffer = new MemoryAddressSpace(this.fbMem.Offset + (this.width * this.height * (this.bpp / 8)), (this.width * this.height * (this.bpp / 8)));
            tmp.buffer = new ManagedMemorySpace(this.width * this.height * (this.bpp / 8));

            return tmp;
        }

        public void SyncBuffers(BackBuffer back)
        {
            //DebugUtil.SendMessage("VMWare", "Buffer Sync: NumRects=" + back.dirtyRecs.Count);
            for (int rectNum = 0; rectNum < back.dirtyRecs.Count; rectNum++)
            {
                Rect r = back.dirtyRecs[rectNum];
                /*for (int y = 0; y < r.h; y++)
                {
                    uint offset = (uint)(((r.y + y) * this.width) + r.x);
                    for (uint x = 0; x < r.w; x++)
                    {
                        fbMem[offset + x] = back.buffer[offset + x];
                    }
                }*/
                CopyRect(r, back.buffer);
                //fbMem.CopyFrom(back.buffer);

                update(r);
            }

            back.dirtyRecs.Clear();

            SyncToFence(InsertFence());
        }

        internal unsafe void CopyRect(Rect r, MemoryAddressSpace src)
        {
            uint bytesPerPixel = this.bpp / 8;
            uint bytesPerLine = this.width * bytesPerPixel;

            for (int y = 0; y < r.h; y++)
            {
                uint offset = (uint)(((r.y + y) * bytesPerLine) + (r.x * bytesPerPixel));
                for (uint x = 0; x < (r.w * bytesPerPixel); x++)
                {
                    (*(byte*)(fbMem.Offset + offset + x)) = *(byte*)(src.Offset + offset + x);
                    //fbMem[offset + x] = back.buffer[offset + x];
                }
            }
        }

        private void SyncToFence(uint fence)
        {
            if (fence == 0)
            {
                DebugUtil.SendError("VMWare", "SyncToFence fence = 0!!");
                return;
            }

            if (!hasFIFOCap(SVGA_FIFO_CAP.FENCE))
            {
                /*
                 * Fall back on the legacy sync if the host does not support
                 * fences.  This is the old sync mechanism that has been
                 * supported in the SVGA device pretty much since the dawn of
                 * time: write to the SYNC register, then read from BUSY until
                 * it's nonzero. This will drain the entire FIFO.
                 *
                 * The parameter we write to SVGA_REG_SYNC is an arbitrary
                 * nonzero value which can be used for debugging, but which is
                 * ignored by release builds of VMware products.
                 */

                //DebugUtil.SendMessage("VMWare", "SyncToFence old style sync!!");

                writeSVGARegister(SVGA_REG.SYNC, 1);
                while (readSVGARegister(SVGA_REG.BUSY) != 0) { }
                return;
            }

            if (hasFencePassed(fence))
            {
                return;
            }

            if (isFIFORegValid(1156) && ((this.capabilities & (uint)SVGA_CAP.IRQMASK) != 0))
            {
                // Interrupt based handling of fences... Not supported yet
            }
            //else
            {
                bool busy = true;

                writeSVGARegister(SVGA_REG.SYNC, 1);
                while (hasFencePassed(fence) && busy)
                {
                    busy = (readSVGARegister(SVGA_REG.BUSY) != 0);
                }
            }

            if (!hasFencePassed(fence))
            {
                /*
                 * This shouldn't happen. If it does, there might be a bug in
                 * the SVGA device.
                 */
                DebugUtil.SendError("VMWare", "SyncToFence failed!!");
            }
        }

        private bool hasFencePassed(uint fence)
        {
            if (fence == 0)
            {
                DebugUtil.SendError("VMWare", "hasFencePassed fence = 0!!");
                return true;
            }

            if (!hasFIFOCap(SVGA_FIFO_CAP.FENCE))
            {
                DebugUtil.SendError("VMWare", "hasFencePassed no FENCE capability!!");
                return false;
            }

            //DebugUtil.SendMessage("VMWare", "hasFencePassed FIFO_FENCE=" + fifoMem.Read32(24));
            return (((Int32)(fifoMem.Read32(24) - fence)) >= 0);
        }

        private uint InsertFence()
        {
            if (!hasFIFOCap(SVGA_FIFO_CAP.FENCE))
            {
                return 1;
            }

            if (fifo.nextFence == 0)
            {
                fifo.nextFence = 1;
            }

            uint fence = fifo.nextFence++;

            MemoryAddressSpace cmd = FIFOReserve(8);
            cmd.Write32(0, (uint)SVGA_CMD.FENCE);
            cmd.Write32(4, fence);

            /*byte[] temp = new byte[8];
            for (uint b = 0; b < temp.Length; b++)
            {
                temp[b] = cmd[b];
            }*/
            //DebugUtil.WriteBinary("VMware", "Fence CMD contents", temp);
            //DebugUtil.SendMessage("VMWare", "SVGA InsertFence: cmd.Offset=" + cmd.Offset + ", cmd.size=" + cmd.Size);
            //DebugUtil.SendMessage("VMWare", "InsertFence fence=" + fence);

            FIFOCommitAll();

            return fence;
        }

        private void update(Rect r)
        {
            MemoryAddressSpace cmd = FIFOReserveCmd(SVGA_CMD.UPDATE, 16);

            cmd.Write32(4, (uint)r.x);
            cmd.Write32(8, (uint)r.y);
            cmd.Write32(12, (uint)r.w);
            cmd.Write32(16, (uint)r.h);

            /*byte[] temp = new byte[20];
            for (uint b = 0; b < temp.Length; b++)
            {
                temp[b] = cmd[b];
            }*/
            //DebugUtil.WriteBinary("VMware", "Update CMD contents", temp);
            //DebugUtil.SendMessage("VMWare", "SVGA Update: cmd.Offset=" + cmd.Offset + ", cmd.size=" + cmd.Size);

            FIFOCommitAll();
        }

        private void FIFOCommitAll()
        {
            FIFOCommit(fifo.reservedSize);
        }

        private void FIFOCommit(uint bytes)
        {
            uint min = fifoMem.Read32(0);
            uint max = fifoMem.Read32(4);
            uint nextCmd = fifoMem.Read32(8);
            bool reserveable = hasFIFOCap(SVGA_FIFO_CAP.RESERVE);
            /*DebugUtil.SendMessage("VMWare", "FIFOCommit bytes=" + bytes + ", min=" + min + ", max=" + max
                                    + ", nextCmd=" + nextCmd + ", reserveable=" + reserveable + ", usingBounceEffect=" + fifo.usingBounceEffect);*/

            if (fifo.reservedSize == 0)
            {
                DebugUtil.SendError("VMWare","FIFOCommit before FIFOReserve!!");
                return;
            }

            fifo.reservedSize = 0;

            if (fifo.usingBounceEffect)
            {
                /*
                 * Slow paths: copy out of a bounce buffer.
                 */
                MemoryAddressSpace buffer = fifo.bounceBuffer;
                if (reserveable)
                {
                    /*
                     * Slow path: bulk copy out of a bounce buffer in two chunks.
                     *
                     * Note that the second chunk may be zero-length if the reserved
                     * size was large enough to wrap around but the commit size was
                     * small enough that everything fit contiguously into the FIFO.
                     *
                     * Note also that we didn't need to tell the FIFO about the
                     * reservation in the bounce buffer, but we do need to tell it
                     * about the data we're bouncing from there into the FIFO.
                     */

                    uint chunkSize = Math.Min(bytes, max - nextCmd);
                    fifoMem.Write32(54, bytes); // FIFO_RESERVED
                    fifoMem.CopyFrom(buffer, 0, nextCmd, chunkSize);
                    fifoMem.CopyFrom(buffer, chunkSize, min, bytes - chunkSize);
                }
                else
                {
                    /*
                     * Slowest path: copy one dword at a time, updating NEXT_CMD as
                     * we go, so that we bound how much data the guest has written
                     * and the host doesn't know to checkpoint.
                     */

                    uint index = 0;
                    while (bytes > 0)
                    {
                        fifoMem.Write32(nextCmd, buffer.Read32(index));
                        nextCmd += 4;
                        index += 4;
                        if (nextCmd == max)
                        {
                            nextCmd = min;
                        }
                        fifoMem.Write32(8, nextCmd); // NEXT_CMD
                        bytes -= 4;
                    }
                }
            } // if usingBounceEffect

            // Atomically update NEXT_CMD, if we didn't already
            if ((!fifo.usingBounceEffect) || (reserveable))
            {
                nextCmd += bytes;
                if (nextCmd >= max)
                {
                    nextCmd -= (max - min);
                }
                fifoMem.Write32(8, nextCmd); // NEXT_CMD
            }

            // Clear reservation in the FIFO
            if (reserveable)
            {
                fifoMem.Write32(54, 0);// FIFO_RESERVED
            }
        }

        private MemoryAddressSpace FIFOReserveCmd(SVGA_CMD type, uint bytes)
        {
            MemoryAddressSpace cmd = FIFOReserve(bytes + 4);
            cmd.Write32(0, (uint)type);

            return cmd;
        }

        private MemoryAddressSpace FIFOReserve(uint bytes)
        {
            uint min = fifoMem.Read32(0);
            uint max = fifoMem.Read32(4);
            uint nextCmd = fifoMem.Read32(8);
            bool reserveable = hasFIFOCap(SVGA_FIFO_CAP.RESERVE);

            /*DebugUtil.SendMessage("VMWare", "FIFOReserve bytes=" + bytes + ", min=" + min + ", max=" + max
                                    + ", nextCmd=" + nextCmd + ", reserveable=" + reserveable);*/

            if ((bytes > fifo.bounceBuffer.Size) || (bytes > (max - min)))
            {
                DebugUtil.SendError("VMWare","FIFO command too large");
            }
            if ((bytes % 4) != 0)
            {
                DebugUtil.SendError("VMWare","FIFO command length not 32-bit aligned");
            }
            if (fifo.reservedSize != 0)
            {
                DebugUtil.SendError("VMWare","FIFOReserve before FIFOCommit");
            }

            fifo.reservedSize = bytes;
            while (true)
            {
                uint stop = fifoMem.Read32(12);
                bool reserveInPlace = false;
                bool needBounce = false;

                #region Pick strategy for dealing with data
                if (nextCmd >= stop)
                {
                    // No valid FIFO data between nextCmd and max
                    if ((nextCmd + bytes < max) || ((nextCmd + bytes == max) && (stop > min)))
                    {
                        /*
                         * Fastest path 1: There is already enough contiguous space
                         * between nextCmd and max (the end of the buffer).
                         *
                         * Note the edge case: If the "<" path succeeds, we can
                         * quickly return without performing any other tests. If
                         * we end up on the "==" path, we're writing exactly up to
                         * the top of the FIFO and we still need to make sure that
                         * there is at least one unused DWORD at the bottom, in
                         * order to be sure we don't fill the FIFO entirely.
                         *
                         * If the "==" test succeeds, but stop <= min (the FIFO
                         * would be completely full if we were to reserve this
                         * much space) we'll end up hitting the FIFOFull path below.
                         */
                        reserveInPlace = true;
                        //DebugUtil.SendMessage("VMWare", "FIFOReserve Path 1");
                    }
                    else if ((max - nextCmd) + (stop - min) <= bytes)
                    {
                        /*
                         * We have to split the FIFO command into two pieces,
                         * but there still isn't enough total free space in
                         * the FIFO to store it.
                         *
                         * Note the "<=". We need to keep at least one DWORD
                         * of the FIFO free at all times, or we won't be able
                         * to tell the difference between full and empty.
                         */
                        //DebugUtil.SendMessage("VMWare", "FIFOReserve Path 2");
                        FIFOFull();
                    }
                    else
                    {
                        //DebugUtil.SendMessage("VMWare", "FIFOReserve Path 3");
                        needBounce = true;
                    }
                }
                else
                {
                    // There is FIFO Data between nextCmd and max
                    if (nextCmd + bytes < stop)
                    {
                        /*
                         * Fastest path 2: There is already enough contiguous space
                         * between nextCmd and stop.
                         */
                        reserveInPlace = true;
                        //DebugUtil.SendMessage("VMWare", "FIFOReserve Path 4");
                    }
                    else
                    {
                        /*
                         * There isn't enough room between nextCmd and stop.
                         * The FIFO is too full to accept this command.
                         */
                        FIFOFull();
                        //DebugUtil.SendMessage("VMWare", "FIFOReserve Path 5");
                    }
                }
                #endregion

                // If we decide we can write directly to the FIFO, ensure VM can support this
                if (reserveInPlace)
                {
                    if ((reserveable) || (bytes <= 4))
                    {
                        fifo.usingBounceEffect = false;
                        if (reserveable)
                        {
                            fifoMem.Write32(54, bytes); // FIFO_RESERVED
                            //DebugUtil.SendMessage("VMWare", "FIFOReserve in cmd");
                        }

                        return new MemoryAddressSpace(fifoMem.Offset + nextCmd, bytes);
                    }
                    else
                    {
                        needBounce = true;
                    }
                }

                if (needBounce)
                {
                    fifo.usingBounceEffect = true;
                    //DebugUtil.SendMessage("VMWare", "FIFOReserve in bounce buffer");
                    return fifo.bounceBuffer;
                }
            } // while
        }

        private void FIFOFull()
        {
            // No support for interrupts yet, so lets just handle it the old way for now
            writeSVGARegister(SVGA_REG.SYNC, 1);
            readSVGARegister(SVGA_REG.BUSY);
        }

        private void writeSVGARegister(SVGA_REG register, uint value)
        {
            ioBase.Write32(SVGA_INDEX_PORT, (uint)register);
            ioBase.Write32(SVGA_VALUE_PORT, value);
        }

        private uint readSVGARegister(SVGA_REG register)
        {
            ioBase.Write32(SVGA_INDEX_PORT, (uint)register);
            return ioBase.Read32(SVGA_VALUE_PORT);
        }

        private bool hasFIFOCap(SVGA_CAP cap)
        {
            return ((this.fifoMem.Read32(16) & (uint)cap) != 0); // Read FIFO_CAPABILITIES to check
        }
        private bool hasFIFOCap(SVGA_FIFO_CAP fifo_cap)
        {
            return ((this.fifoMem.Read32(16) & (uint)fifo_cap) != 0); // Read FIFO_CAPABILITIES to check
        }

        private bool isFIFORegValid(uint reg)
        {
            return (fifoMem.Read32(0) > reg); // Read FIFO_MIN to check
        }
    }
}
