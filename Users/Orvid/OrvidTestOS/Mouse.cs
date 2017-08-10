using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPUBus = Cosmos.Core_Plugs.IOPortImpl;
using Cosmos.Core;
using GuessKernel;
//using Cosmos.Kernel;

namespace Cosmos.Hardware
{
    /// <summary>
    /// This class describes the mouse.
    /// </summary>
    public class Mouse
    {
        private Cosmos.Core.IOGroup.Mouse g = new Core.IOGroup.Mouse();

        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public uint X;
        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public uint Y;
        /// <summary>
        /// The state the mouse is currently in.
        /// </summary>
        public MouseState Buttons;

        /// <summary>
        /// This is the required call to start
        /// the mouse receiving interrupts.
        /// </summary>
        public void Initialize()
        {
            Cosmos.Core_Plugs.IOPortImpl.Read8(0X60);
            g.p60.Byte = 0x20;
            byte statusByte = Read();
            byte modstatusByte = (byte)(((statusByte >> 2) << 1) + 1); // Enable status byte 2
            byte final = (byte)(((modstatusByte >> 5) << 1) + ((modstatusByte << 4) >> 4)); // Disable status byte 5
            g.p60.Byte = 0x60;
            Write(final);


            //Enable the auxiliary mouse device
            WaitSignal();
            g.p64.Byte = 0xA8;

            //Tell the mouse to use default settings 
            Write(0xF6);
            Read();  //Acknowledge

            //Set Remote Mode
            Write(0xF0);
            Read();  //Acknowledge


            ////Enable the auxiliary mouse device
            //WaitSignal();
            //g.p64.Byte = 0xA8;

            ////// enable interrupt
            //WaitSignal();
            //g.p64.Byte = 0x20;
            //WaitData();
            //byte tmpst = g.p60.Byte;
            //tmpst = (byte)(((tmpst >> 2) << 1) + 1);
            //byte status = (byte)(((tmpst >> 5) << 1) + ((byte)(tmpst << 4) >> 4));
            ////byte status = (byte)(g.p60.Byte | 2);
            //WaitSignal();
            //g.p64.Byte = 0x60;
            //WaitSignal();
            //g.p60.Byte = status;

            ////Tell the mouse to use default settings 
            //Write(0xF6);
            //Read();  //Acknowledge

            ////Set Remote Mode
            //Write(0xF0);
            //Read();  //Acknowledge
        }

        private byte Read()
        {
            WaitData();
            return g.p60.Byte;
        }

        private void Write(byte b)
        {
            //Wait to be able to send a command
            WaitSignal();
            //Tell the mouse we are sending a command
            g.p64.Byte = 0xD4;
            //Wait for the final part
            WaitSignal();
            //Finally write
            g.p60.Byte = b;
        }

        public bool TimedOut = false;
        public bool TimedOut2 = false;
        public bool TimedOut3 = false;
        public bool TimedOut4 = false;
        public bool TimedOut5 = false;

        private void WaitData()
        {
            for (int i = 0; i < 100000 && ((g.p64.Byte & 1) != 1); i++)
            {
                if (i == 99999)
                {
                    if (!TimedOut)
                        TimedOut = true;
                    else if (!TimedOut2)
                        TimedOut2 = true;
                    else if (!TimedOut3)
                        TimedOut3 = true;
                    else if (!TimedOut4)
                        TimedOut3 = true;
                    else if (!TimedOut5)
                        TimedOut3 = true;
                }
            }
        }

        private void WaitSignal()
        {
            for (int i = 0; i < 100000 && ((g.p64.Byte & 2) != 0); i++)
            {
            }
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

        public void HandleMouse()
        {
            Write(0xEB);
            uint i = Read();
            if (i != 0xFA) // this means it didn't respond with an acknowledge
            {
                if (i == 0xFE) // The mouse send back 'Resend'
                    TimedOut4 = true;
                else
                {
                    TimedOut3 = true;
                }
            }
            else
            {
                TimedOut4 = false;

                #region Read Bytes
                mouse_byte[0] = Read();
                mouse_byte[1] = Read();
                mouse_byte[2] = Read();
                #endregion

                #region Process Bytes
                X += mouse_byte[1];
                Y += mouse_byte[2];


                if (X < 0)
                {
                    X = 0;
                }
                else if (X > 319)
                {
                    X = 319;
                }

                if (Y < 0)
                {
                    Y = 0;
                }
                else if (Y > 199)
                {
                    Y = 199;
                }

                if ((mouse_byte[0] & 1) == 1)
                {
                    Buttons = MouseState.Left;
                }
                else if ((mouse_byte[0] & 2) == 1)
                {
                    Buttons = MouseState.Right;
                }
                else if ((mouse_byte[0] & 3) == 1)
                {
                    Buttons = MouseState.Middle;
                }
                #endregion

                //Console.WriteLine("X: " + X + " Y: " + Y);
                if (GuessOS.MouseX != X || GuessOS.MouseY != Y)
                {
                    GuessOS.MouseX = X;
                    GuessOS.MouseY = Y;
                    Cosmos.System.Global.Console.WriteLine("X: " + X + " Y: " + Y);
                }
            }
        }
    }
}
