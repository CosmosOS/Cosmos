using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL
{
    /// <summary>
    /// This class describes the mouse.
    /// </summary>
    public class Mouse
    {
        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public int Y;
        /// <summary>
        /// The state the mouse is currently in.
        /// </summary>
        public MouseState Buttons;

        /// <summary>
        /// The screen width (i.e. max value of X)
        /// </summary>
        public uint ScreenWidth;
        /// <summary>
        /// The screen height (i.e. max value of Y)
        /// </summary>
        public uint ScreenHeight;

        /// <summary>
        /// This is the required call to start
        /// the mouse receiving interrupts.
        /// </summary>
        public void Initialize(uint screenWidth, uint screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            ////enable mouse
            WaitSignal();
            BaseIOGroups.Mouse.p64.Byte = (byte)0xA8;

            //// enable interrupt
            WaitSignal();
            BaseIOGroups.Mouse.p64.Byte = (byte)0x20;
            WaitData();
            //byte status1 = (byte)(BaseIOGroups.Mouse.p60.Byte);
            byte status = (byte)(BaseIOGroups.Mouse.p60.Byte | 2);
            WaitSignal();
            BaseIOGroups.Mouse.p64.Byte = (byte)0x60;
            WaitSignal();
            BaseIOGroups.Mouse.p60.Byte = (byte)status;

            ////default 
            Write(0xF6);
            Read();  //Acknowledge

            ////Enable the mouse
            Write(0xF4);
            Read();  //Acknowledge

            //Set the IRQ 12, to the method HandleMouse
            INTs.SetIrqHandler(12, HandleMouse);
        }

        private byte Read()
        {
            WaitData();
            return BaseIOGroups.Mouse.p60.Byte;
        }

        private void Write(byte b)
        {
            WaitSignal();
            BaseIOGroups.Mouse.p64.Byte = 0xD4;
            WaitSignal();
            BaseIOGroups.Mouse.p60.Byte = b;
        }

        private void WaitData()
        {
            for (int i = 0; i < 100 & ((BaseIOGroups.Mouse.p64.Byte & 1) == 1); i++)
                ;
        }

        private void WaitSignal()
        {
            for (int i = 0; i < 100 & ((BaseIOGroups.Mouse.p64.Byte & 2) != 0); i++)
                ;
        }

        /// <summary>
        /// The possible states of a mouse.
        /// </summary>
        public enum MouseState
        {
            /// <summary>
            /// No button is pressed.
            /// </summary>
            None = 0,
            /// <summary>
            /// The left mouse button is pressed.
            /// </summary>
            Left = 1,
            /// <summary>
            /// The right mouse button is pressed.
            /// </summary>
            Right = 2,
            /// <summary>
            /// The middle mouse button is pressed.
            /// </summary>
            Middle = 4
        }


        private byte[] mouse_byte = new byte[4];
        private static byte mouse_cycle = 0;

        public void HandleMouse(ref INTs.IRQContext context)
        {
            switch (mouse_cycle)
            {
                case 0:
                    mouse_byte[0] = Read();

                    //Bit 3 of byte 0 is 1, then we have a good package
                    if ((mouse_byte[0] & 0x8) == 0x8)
                        mouse_cycle++;

                    break;
                case 1:
                    mouse_byte[1] = Read();
                    mouse_cycle++;
                    break;
                case 2:
                    mouse_byte[2] = Read();
                    mouse_cycle = 0;

                    if ((mouse_byte[0] & 0x10) == 0x10)
                        X -= (mouse_byte[1] ^ 0xff);
                    else
                        X += mouse_byte[1];

                    if ((mouse_byte[0] & 0x20) == 0x20)
                        Y += (mouse_byte[2] ^ 0xff);
                    else
                        Y -= mouse_byte[2];

                    if (X < 0)
                        X = 0;
                    else if (X > ScreenWidth - 1)
                        X = (int)ScreenWidth - 1;

                    if (Y < 0)
                        Y = 0;
                    else if (Y > ScreenHeight - 1)
                        Y = (int)ScreenHeight - 1;

                    Buttons = (MouseState)(mouse_byte[0] & 0x7);

                    break;
            }

        }
    }
}
