using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;

namespace SteveKernel
{
    public class VGA : Hardware
    {
        private const ushort VGA_AC_INDEX = 0x3C0;
        private const ushort VGA_AC_WRITE = 0x3C0;
        private const ushort VGA_AC_READ = 0x3C1;
        private const ushort VGA_MISC_WRITE = 0x3C2;
        private const ushort VGA_SEQ_INDEX = 0x3C4;
        private const ushort VGA_SEQ_DATA = 0x3C5;
        private const ushort VGA_DAC_READ_INDEX = 0x3C7;
        private const ushort VGA_DAC_WRITE_INDEX = 0x3C8;
        private const ushort VGA_DAC_DATA = 0x3C9;
        private const ushort VGA_MISC_READ = 0x3CC;
        private const ushort VGA_GC_INDEX = 0x3CE;
        private const ushort VGA_GC_DATA = 0x3CF;
        /*			COLOR emulation		MONO emulation */
        private const ushort VGA_CRTC_INDEX = 0x3D4;		/* 0x3B4 */
        private const ushort VGA_CRTC_DATA = 0x3D5;		/* 0x3B5 */
        private const ushort VGA_INSTAT_READ = 0x3DA;

        private const ushort VGA_NUM_SEQ_REGS = 5;
        private const ushort VGA_NUM_CRTC_REGS = 25;
        private const ushort VGA_NUM_GC_REGS = 9;
        private const ushort VGA_NUM_AC_REGS = 21;
        private const ushort VGA_NUM_REGS = (1 + VGA_NUM_SEQ_REGS + VGA_NUM_CRTC_REGS +
                        VGA_NUM_GC_REGS + VGA_NUM_AC_REGS);

        private static void outportb(ushort port, byte value)
        {
            CPUBus.Write8(port, value);
        }
        private static byte inportb(ushort port)
        {
            return CPUBus.Read8(port);
        }


        private unsafe static void write_regs(byte* regs)
        {
            byte i;

            /* write MISCELLANEOUS reg */
            outportb(VGA_MISC_WRITE, *regs);
            regs++;
            /* write SEQUENCER regs */
            for (i = 0; i < VGA_NUM_SEQ_REGS; i++)
            {
                outportb(VGA_SEQ_INDEX, i);
                outportb(VGA_SEQ_DATA, *regs);
                regs++;
            }
            /* unlock CRTC registers */
            outportb(VGA_CRTC_INDEX, 0x03);
            outportb(VGA_CRTC_DATA, (byte)(inportb(VGA_CRTC_DATA) | 0x80));
            outportb(VGA_CRTC_INDEX, 0x11);
            outportb(VGA_CRTC_DATA, (byte)(inportb(VGA_CRTC_DATA) & 0x7f));
            /* make sure they remain unlocked */
            regs[0x03] |= 0x80;
            regs[0x11] &= 0x7f;
            /* write CRTC regs */
            for (i = 0; i < VGA_NUM_CRTC_REGS; i++)
            {
                outportb(VGA_CRTC_INDEX, i);
                outportb(VGA_CRTC_DATA, *regs);
                regs++;
            }
            /* write GRAPHICS CONTROLLER regs */
            for (i = 0; i < VGA_NUM_GC_REGS; i++)
            {
                outportb(VGA_GC_INDEX, i);
                outportb(VGA_GC_DATA, *regs);
                regs++;
            }
            /* write ATTRIBUTE CONTROLLER regs */
            for (i = 0; i < VGA_NUM_AC_REGS; i++)
            {
                inportb(VGA_INSTAT_READ);
                outportb(VGA_AC_INDEX, i);
                outportb(VGA_AC_WRITE, *regs);
                regs++;
            }
            /* lock 16-color palette and unblank display */
            inportb(VGA_INSTAT_READ);
            outportb(VGA_AC_INDEX, 0x20);
        }

        private static byte[] g_320x200x256 = new byte[]
{
/* MISC */
	0x63,
/* SEQ */
	0x03, 0x01, 0x0F, 0x00, 0x0E,
/* CRTC */
	0x5F, 0x4F, 0x50, 0x82, 0x54, 0x80, 0xBF, 0x1F,
	0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x9C, 0x0E, 0x8F, 0x28,	0x40, 0x96, 0xB9, 0xA3,
	0xFF,
/* GC */
	0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x05, 0x0F,
	0xFF,
/* AC */
	0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
	0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
	0x41, 0x00, 0x0F, 0x00,	0x00
};

        private static byte[] g_640x480x16 = new byte[]
{
/* MISC */
	0xE3,
/* SEQ */
	0x03, 0x01, 0x08, 0x00, 0x06,
/* CRTC */
	0x5F, 0x4F, 0x50, 0x82, 0x54, 0x80, 0x0B, 0x3E,
	0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0xEA, 0x0C, 0xDF, 0x28, 0x00, 0xE7, 0x04, 0xE3,
	0xFF,
/* GC */
	0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x05, 0x0F,
	0xFF,
/* AC */
	0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x14, 0x07,
	0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
	0x01, 0x00, 0x0F, 0x00, 0x00
};

        public unsafe static void Test()
        {
            Console.WriteLine("setting mode...");
            fixed (byte* b = g_640x480x16)
            {
                write_regs(b);
            }
            byte* c = (byte*)(0xa0000);

            Console.WriteLine("filling screeen...");
            MemoryAddressSpace mr = new MemoryAddressSpace(0xa0000, 0x10000);

            for (uint y = 0; y < 200; y++)
                for (uint x = 0; x < 320; x++)
                    c[y * 320 + x] = (byte)((x + y) & 0xff);
            Console.WriteLine("done...");

        }
    }

}
