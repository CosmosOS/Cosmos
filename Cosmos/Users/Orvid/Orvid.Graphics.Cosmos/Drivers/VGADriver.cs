using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.Drivers
{
    /// <summary>
    /// The default implementation of a VGA driver.
    /// </summary>
    public class VGADriver : GraphicsDriver
    {
        private readonly Cosmos.Core.IOGroup.VGA mIO = new Cosmos.Core.IOGroup.VGA();
        private GraphicsMode curMode;

        /// <summary>
        /// The name of the driver.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Standard VGA Driver";
            }
        }

        /// <summary>
        /// The version of the driver.
        /// </summary>
        public override string Version
        {
            get
            {
                return "v 1.0";
            }
        }

        /// <summary>
        /// The company responsible for the driver.
        /// </summary>
        public override string Company
        {
            get
            {
                return "The Cosmos Community";
            }
        }

        /// <summary>
        /// The Author of the driver.
        /// </summary>
        public override string Author
        {
            get
            {
                return "Orvid King";
            }
        }

        /// <summary>
        /// The current GraphicsMode.
        /// </summary>
        public override GraphicsMode Mode
        {
            get
            {
                return curMode;
            }
        }

        private byte GetForColorizer(Pixel p)
        {
            byte b = unchecked((byte)(Math.Floor((double)(getForPallet(p.R) / (byte)42)) * Math.Floor((double)(getForPallet(p.G) / (byte)42)) * Math.Floor((double)(getForPallet(p.B) / (byte)42))));
            if (b == 0)
            {
                return 1;
            }
            else if (b == 1)
            {
                return 0;
            }
            else
            {
                return unchecked((byte)(b + (byte)2));
            }
        }

        private int[] Colorize(Image i)
        {
            int[] arr = new int[i.Height * i.Width];
            int indx = 0;
            for (int y = 0; y < i.Height; y++)
            {
                for (int x = 0; x < i.Width; x++)
                {
                    arr[indx] = GetForColorizer(i.Data[indx]);
                    //Console.WriteLine("Value: " + i.Data[indx].R.ToString());
                    //Console.WriteLine("Colorized value: " + GetForColorizer(i.Data[indx]).ToString());
                    indx++;
                }
            }
            return arr;
        }

        public override void Update(Image i)
        {

            //Console.WriteLine("Well Update is getting called....");
            // TODO: Switch to an delegate based draw mechanism,
            // it would be much faster than switch, case like statements.

            int[] data = Colorize(i);
            for (uint i2 = 0; i2 < 64000 /* 320 Times 200 is 64,000 so we need to loop 64k times. */; i2++)
            {
                mIO.VGAMemoryBlock[i2] = (byte)(data[i2] & 0xFF);
            }
        }

        public override List<GraphicsMode> GetSupportedModes()
        {
            List<GraphicsMode> modes = new List<GraphicsMode>();


            modes.Add(new GraphicsMode(Resolution.Size320x200, ColorDepth.Bit8));


            return modes;
        }

        public override void SetMode(GraphicsMode mode)
        {
            if (SupportsMode(mode))
            {
                curMode = mode;
            }
        }

        public override bool Supported()
        {
            return true;
        }

        private byte getForPallet(byte i)
        {
            if (i > 210)
            {
                return 255;
            }
            else
            {
                return i;
            }
        }

        public override void Initialize()
        {
            curMode = new GraphicsMode(Resolution.Size320x200, ColorDepth.Bit8);
            SetMode320x200x8();

            SetPaletteEntry(0, 0, 0, 0);
            SetPaletteEntry(1, 255, 255, 255);
            int i = 2;
            for (int r = 0; r < 6; r++)
            {
                for (int g = 0; g < 6; g++)
                {
                    SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 0);
                    i++;
                    SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 42);
                    i++;
                    SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 84);
                    i++;
                    SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 126);
                    i++;
                    SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 168);
                    i++;
                    SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 210);
                    i++;
                    if (i < 256)
                    {
                        SetPaletteEntry(i, getForPallet((byte)(r * 42)), getForPallet((byte)(g * 42)), 255);
                        i++;
                    }
                }
            }
        }

        private const byte NumSeqRegs = 5;
        private const byte NumCRTCRegs = 25;
        private const byte NumGCRegs = 9;
        private const byte NumACRegs = 21;

        #region WriteVGARegisters
        private void WriteVGARegisters(byte[] registers)
        {
            int xIdx = 0;
            byte i;

            /* write MISCELLANEOUS reg */
            mIO.MiscellaneousOutput_Write.Byte = registers[xIdx];
            xIdx++;
            /* write SEQUENCER regs */
            for (i = 0; i < NumSeqRegs; i++)
            {
                mIO.Sequencer_Index.Byte = i;
                mIO.Sequencer_Data.Byte = registers[xIdx];
                xIdx++;
            }
            /* unlock CRTC registers */
            mIO.CRTController_Index.Byte = 0x03;
            mIO.CRTController_Data.Byte = (byte)(mIO.CRTController_Data.Byte | 0x80);
            mIO.CRTController_Index.Byte = 0x11;
            mIO.CRTController_Data.Byte = (byte)(mIO.CRTController_Data.Byte & 0x7F);

            /* make sure they remain unlocked */
            registers[0x03] |= 0x80;
            registers[0x11] &= 0x7f;

            /* write CRTC regs */
            for (i = 0; i < NumCRTCRegs; i++)
            {
                mIO.CRTController_Index.Byte = i;
                mIO.CRTController_Data.Byte = registers[xIdx];
                xIdx++;
            }
            /* write GRAPHICS CONTROLLER regs */
            for (i = 0; i < NumGCRegs; i++)
            {
                mIO.GraphicsController_Index.Byte = i;
                mIO.GraphicsController_Data.Byte = registers[xIdx];
                xIdx++;
            }
            /* write ATTRIBUTE CONTROLLER regs */
            for (i = 0; i < NumACRegs; i++)
            {
                var xDoSomething = mIO.Instat_Read.Byte;
                mIO.AttributeController_Index.Byte = i;
                mIO.AttributeController_Write.Byte = registers[xIdx];
                xIdx++;
            }
            /* lock 16-color palette and unblank display */
            var xNothing = mIO.Instat_Read.Byte;
            mIO.AttributeController_Index.Byte = 0x20;
        }
        #endregion

        #region 300x200x4
        /*
        private static byte[] g_320x200x4 = new byte[]
        {
            /* MISC *
            0x63,
            /* SEQ *
            0x03, 0x09, 0x03, 0x00, 0x02,
            /* CRTC *
            0x2D, 0x27, 0x28, 0x90, 0x2B, 0x80, 0xBF, 0x1F,
            0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x9C, 0x0E, 0x8F, 0x14, 0x00, 0x96, 0xB9, 0xA3,
            0xFF,
            /* GC *
            0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x02, 0x00,
            0xFF,
            /* AC *
            0x00, 0x13, 0x15, 0x17, 0x02, 0x04, 0x06, 0x07,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x01, 0x00, 0x03, 0x00, 0x00
        };
        */
        #endregion

        #region 320x200x8
        public void SetMode320x200x8()
        {
            WriteVGARegisters(g_320x200x256);
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
        #endregion

        #region 640x480x2
        /*
        private static byte[] g_640x480x2 = new byte[]
        {
            /* MISC *
            0xE3,
            /* SEQ *
            0x03, 0x01, 0x0F, 0x00, 0x06,
            /* CRTC *
            0x5F, 0x4F, 0x50, 0x82, 0x54, 0x80, 0x0B, 0x3E,
            0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xEA, 0x0C, 0xDF, 0x28, 0x00, 0xE7, 0x04, 0xE3,
            0xFF,
            /* GC *
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x0F,
            0xFF,
            /* AC *
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x14, 0x07,
            0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x01, 0x00, 0x0F, 0x00, 0x00
        };
        */
        #endregion

        #region 640x480x16
        /*
        public void SetPixel640x480x16(uint width, uint height, uint c)
        {
            var xSegment = GetFramebufferSegment();
            var xOffset = (height * 32) + width >> 1;

            c = c & 0xf;

            if ((width & 1) == 0)
            {
                xSegment[xOffset] = (byte)((xSegment[xOffset] & 0xf) | (c << 4));
            }
            else
            {
                xSegment[xOffset] = (byte)((xSegment[xOffset] & 0xf0) | c);
            }
        }

        private static byte[] g_640x480x16 = new byte[]
        {
            /* MISC *
            0xE3,
            /* SEQ *
            0x03, 0x01, 0x08, 0x00, 0x06,
            /* CRTC *
            0x5F, 0x4F, 0x50, 0x82, 0x54, 0x80, 0x0B, 0x3E,
            0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xEA, 0x0C, 0xDF, 0x28, 0x00, 0xE7, 0x04, 0xE3,
            0xFF,
            /* GC *
            0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x05, 0x0F,
            0xFF,
            /* AC *
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x14, 0x07,
            0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x01, 0x00, 0x0F, 0x00, 0x00
        };
        */
        #endregion

        #region 720x480x16
        /*
        private static byte[] g_720x480x16 = new byte[]
        {
            /* MISC *
            0xE7,
            /* SEQ *
            0x03, 0x01, 0x08, 0x00, 0x06,
            /* CRTC *
            0x6B, 0x59, 0x5A, 0x82, 0x60, 0x8D, 0x0B, 0x3E,
            0x00, 0x40, 0x06, 0x07, 0x00, 0x00, 0x00, 0x00,
            0xEA, 0x0C, 0xDF, 0x2D, 0x08, 0xE8, 0x05, 0xE3,
            0xFF,
            /* GC *
            0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x05, 0x0F,
            0xFF,
            /* AC *
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x01, 0x00, 0x0F, 0x00, 0x00,
        };
        */
        #endregion

        #region Palette Operations
        public void SetPaletteEntry(int index, System.Drawing.Color color)
        {
            SetPaletteEntry(index, color.R, color.G, color.B);
        }

        public void SetPalette(int index, byte[] pallete)
        {
            mIO.DACIndex_Write.Byte = (byte)index;
            for (int i = 0; i < pallete.Length; i++)
                mIO.DAC_Data.Byte = (byte)(pallete[i] >> 2);
        }

        public void SetPaletteEntry(int index, byte r, byte g, byte b)
        {
            mIO.DACIndex_Write.Byte = (byte)index;
            mIO.DAC_Data.Byte = (byte)(r >> 2);
            mIO.DAC_Data.Byte = (byte)(g >> 2);
            mIO.DAC_Data.Byte = (byte)(b >> 2);
        }
        #endregion

        public override void Shutdown()
        {

        }
    }
}
