using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware2
{
    public static class Mouse
    {
        public static int X, Y;
        public static MouseState Buttons;

        public static void Initialize()
        {

            ////enable mouse
            //WaitSignal();
            //CPUBus.Write8(0x64, 0xA8);

            //// enable interrupt
            //WaitSignal();
            //CPUBus.Write8(0x64, 0x20);
            //WaitData();
            //byte status = (byte)(CPUBus.Read8(0x60) | 2);
            //WaitSignal();
            //CPUBus.Write8(0x64, 0x60);
            //WaitSignal();
            //CPUBus.Write8(0x60, status);

            ////default 
            //Write(0xF6);
            //Read();  //Acknowledge

            ////Enable the mouse
            //Write(0xF4);
            //Read();  //Acknowledge

            //Interrupts.AddIRQHandler(12, new Interrupts.InterruptDelegate(HandleMouse));

        }

        private static byte Read()
        {
            WaitData();
            return CPUBus.Read8(0x60);
        }

        private static void Write(byte b)
        {
            //Wait to be able to send a command
            WaitSignal();
            //Tell the mouse we are sending a command
            CPUBus.Write8(0x64, 0xD4);
            //Wait for the final part
            WaitSignal();
            //Finally write
            CPUBus.Write8(0x60, b);
        }

        private static void WaitData()
        {
            for (int i = 0; i < 1000 & ((CPUBus.Read8(0x64) & 1) == 1); i++)
                ;
        }

        private static void WaitSignal()
        {
            for (int i = 0; i < 1000 & ((CPUBus.Read8(0x64) & 2) != 0); i++)
                ;
        }

        public enum MouseState
        {
            None=0,
            Left=1,
            Right =2,
            Middle =4
        }

        private static byte mouse_cycle =0;
        private static int[] mouse_byte = new int[4];

    //    public static void HandleMouse(ref IRQContext context)
    //    {
    //        switch (mouse_cycle)
    //        {
    //            case 0:
    //                mouse_byte[0] = CPUBus.Read8(0x60);

    //                if ((mouse_byte[0] & 0x8) == 0x8)
    //                    mouse_cycle++;

    //                break;
    //            case 1:
    //                mouse_byte[1] = CPUBus.Read8(0x60);
    //                mouse_cycle++;
    //                break;
    //            case 2:
    //                mouse_byte[2] = CPUBus.Read8(0x60);
    //                mouse_cycle = 0;

    //                if ((mouse_byte[0] & 0x10) == 0x10)
    //                    X -= mouse_byte[1] ^ 0xff;
    //                else
    //                    X += mouse_byte[1];

    //                if ((mouse_byte[0] & 0x20) == 0x20)
    //                    Y += mouse_byte[2] ^ 0xff;
    //                else
    //                    Y -= mouse_byte[2];

    //                if (X < 0)
    //                    X = 0;
    //                else if (X > 319)
    //                    X = 319;

    //                if (Y < 0)
    //                    Y = 0;
    //                else if (Y > 199)
    //                    Y = 199;

    //                Buttons = (MouseState)(mouse_byte[0] & 0x7);

    //                break;
    //        }
    //    }
    }
}
