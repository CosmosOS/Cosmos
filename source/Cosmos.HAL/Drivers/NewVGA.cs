/*--------------------------------------------------------

                Contributors:
                - Cyber4

--------------------------------------------------------*/


/*
        Please note, this does NOT replace the text driver!
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.Drivers
{
    class NewVGA
    {


        #region Enums
        public enum ColorDepth
        {
            /// <summary>
            /// 2-bit graphics (4 colors)
            /// </summary>
            BitDepth2,

            /// <summary>
            /// 4-bit graphics (16 colors)
            /// </summary>
            BitDepth4,

            /// <summary>
            /// 8-bit graphics (256 colors)
            /// </summary>
            BitDepth8,

            /// <summary>
            /// 16-bit graphics (65,536 colors)
            /// </summary>
            BitDepth16
        };

        public enum Register : ushort
        {
            ID = 0,
            Enable = 1,
            Width = 2,
            Height = 3,
            MaxWidth = 4,
            MaxHeight = 5,
            Depth = 6,
        }

        #endregion

        #region variables

            #region Other
                private readonly Core.IOGroup.VGA mIO = new Core.IOGroup.VGA();
            #endregion

            #region Unsigned
                private uint height;
                private uint width;
        #endregion

        #region Bytes
            private const byte NumSeqRegs = 5;
            private const byte NumCRTCRegs = 25;
            private const byte NumGCRegs = 9;
            private const byte NumACRegs = 21;
        #endregion

        #endregion

        #region Functions and Constructors

        public NewVGA()
        {

        }

        #endregion

        #region methods

//Initialize VGA

        public static void InitVGA()
        {

        }

//Define Graphics

        public void DefineGraphics(uint width, uint height, ColorDepth depth)
        {
            this.width = width;
            this.height = height;
            if (depth == ColorDepth.BitDepth2)
            {

            }
            else if (depth == ColorDepth.BitDepth4)
            {

            }
            else if (depth == ColorDepth.BitDepth8)
            {

            }
            else if (depth == ColorDepth.BitDepth16)
            {

            }
        }

//Write Registers

        public void WriteRegister()
        {

        }

        //Write VGA Registers

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

    }
}

/* Documentation, TODOs, Communication
                    [---------------------]
                    ( TODO: Goals & Stuff )
                    [---------------------]

[---------------------------------------------------------------------]

    (We need to organize things by category, and in order of Priority.

    1 = Low
    5 = URGENT

    Example:
    - Item 1 (*ExamplePriority* 4)
    - Item 2 (3)

[---------------------------------------------------------------------]

    The Basics:
    - Get Basic I/O (5)




*/