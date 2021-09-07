//#define ASMMouse

#if ASMMouse
//#define DebugMouse

using System;
using System.Collections.Generic;
using System.Text;
using IL2CPU.API;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using Cosmos.Core;

namespace Cosmos.Hardware
{
    class AsmMouse
    {

        #region Native mouse implementation
        #region EnableMouseASM
        private class EnableMouseASM : AssemblerMethod
        {

            public override void AssembleNew(object aAssembler, object aMethodInfo)
            {
                XS.Mov(XSRegisters.BL, 0xa8);

                XS.Call("send_mouse_cmd");

                XS.Call("mouse_read");

                XS.Noop();

                XS.Mov(XSRegisters.BL, 0x20);

                XS.Call("send_mouse_cmd");

                XS.Call("mouse_read");

                XS.Or(XSRegisters.AL, 3);

                XS.Mov(XSRegisters.BL, 0x60);

                XS.Push(XSRegisters.EAX);

                XS.Call("send_mouse_cmd");

                XS.Pop(XSRegisters.EAX);

                XS.Call("mouse_write");

                XS.Noop();

                XS.Mov(XSRegisters.BL, 0xd4);

                XS.Call("send_mouse_cmd");

                XS.Mov(XSRegisters.AL, 0xf4);

                XS.Call("mouse_write");

                XS.Call("mouse_read");

        #region mouse_read
                XS.Label("mouse_read");
                {
                    XS.Push(XSRegisters.ECX);

                    XS.Push(XSRegisters.EDX);

                    XS.Mov(XSRegisters.ECX, 0xffff);

                    XS.Label("mouse_read_loop");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        XS.Test(XSRegisters.AL, 1);

                        XS.Jump(ConditionalTestEnum.NotZero, "mouse_read_ready");

                        new Loop
                        {
                            DestinationLabel = "mouse_read_loop"
                        };

                        XS.Mov(XSRegisters.AH, 1);

                        XS.Jump("mouse_read_exit");
                    }

                    XS.Label("mouse_read_ready");
                    {
                        XS.Push(XSRegisters.ECX);

                        XS.Mov(XSRegisters.ECX, 32);
                    }

                    XS.Label("mouse_read_delay");
                    {
                        new Loop
                        {
                            DestinationLabel = "mouse_read_delay"
                        };

                        XS.Pop(XSRegisters.ECX);

                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x60,
                            Size = 8
                        };

                        XS.Xor(XSRegisters.AH, XSRegisters.RegistersEnum.AH);
                    }

                    XS.Label("mouse_read_exit");
                    {
                        XS.Pop(XSRegisters.EDX);

                        XS.Pop(XSRegisters.ECX);

                        XS.Return();
                    }
                }
        #endregion

        #region mouse_write
                XS.Label("mouse_write");
                {
                    XS.Push(XSRegisters.ECX);

                    XS.Push(XSRegisters.EDX);

                    XS.Mov(XSRegisters.BH, XSRegisters.RegistersEnum.AL);

                    XS.Mov(XSRegisters.ECX, 0xffff);

                    XS.Label("mouse_write_loop1");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        XS.Test(XSRegisters.AL, 32);

                        XS.Jump(ConditionalTestEnum.Zero, "mouse_write_ok1");

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop1"
                        };

                        XS.Mov(XSRegisters.AH, 1);

                        XS.Jump("mouse_write_exit");
                    }

                    XS.Label("mouse_write_ok1");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x60,
                            Size = 8
                        };

                        XS.Mov(XSRegisters.ECX, 0xffff);
                    }

                    XS.Label("mouse_write_loop");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        XS.Test(XSRegisters.AL, 2);

                        XS.Jump(ConditionalTestEnum.Zero, "mouse_write_ok");

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop"
                        };

                        XS.Mov(XSRegisters.AH, 1);

                        XS.Jump("mouse_write_exit");
                    }

                    XS.Label("mouse_write_ok");
                    {
                        XS.Mov(XSRegisters.AL, XSRegisters.RegistersEnum.BH);

                        new Out2Port
                        {
                            DestinationValue = 0x60,
                            SourceReg = RegistersEnum.AL,
                            Size = 8
                        };

                        XS.Mov(XSRegisters.ECX, 0xffff);
                    }

                    XS.Label("mouse_write_loop3");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        XS.Test(XSRegisters.AL, 2);

                        XS.Jump(ConditionalTestEnum.Zero, "mouse_write_ok3");

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop3"
                        };

                        XS.Mov(XSRegisters.AH, 1);

                        XS.Jump("mouse_write_exit");
                    }

                    XS.Label("mouse_write_ok3");
                    {
                        XS.Mov(XSRegisters.AH, 0x08);
                    }

                    XS.Label("mouse_write_loop4");
                    {
                        XS.Mov(XSRegisters.ECX, 0xffff);
                    }

                    XS.Label("mouse_write_loop5");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        XS.Test(XSRegisters.AL, 1);

                        XS.Jump(ConditionalTestEnum.NotZero, "mouse_write_ok4");

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop5"
                        };

                        XS.Dec(XSRegisters.AH);

                        XS.Jump(ConditionalTestEnum.NotZero, "mouse_write_loop4");
                    }

                    XS.Label("mouse_write_ok4");
                    {
                        XS.Xor(XSRegisters.AH, XSRegisters.RegistersEnum.AH);
                    }

                    XS.Label("mouse_write_exit");
                    {
                        XS.Pop(XSRegisters.EDX);

                        XS.Pop(XSRegisters.ECX);

                        XS.Return();
                    }
                }
        #endregion

        #region send_mouse_cmd
                XS.Label("send_mouse_cmd");
                {

                    XS.Mov(XSRegisters.ECX, 0xffff);

                    XS.Label("mouse_cmd_wait");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };
                        XS.Test(XSRegisters.AL, 2);
                        XS.Jump(ConditionalTestEnum.Zero, "mouse_cmd_send");
                        new Loop
                        {
                            DestinationLabel = "mouse_cmd_wait"
                        };
                        XS.Jump("mouse_cmd_error");
                    }

                    XS.Label("mouse_cmd_send");
                    {
                        XS.Mov(XSRegisters.AL, XSRegisters.RegistersEnum.BL);
                        new Out2Port
                        {
#if DebugMouse
                            SourceValue = 0x64,
                            DestinationReg = RegistersEnum.AL,
#else
                            DestinationValue = 0x64,
                            SourceReg = RegistersEnum.AL,
#endif
                            Size = 8
                        };
                        XS.Mov(XSRegisters.ECX, 0xffff);
                    }

                    XS.Label("mouse_cmd_accept");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };
                        XS.Test(XSRegisters.AL, 0x02);
                        XS.Jump(ConditionalTestEnum.Zero, "mouse_cmd_ok");
                        new Loop
                        {
                            DestinationLabel = "mouse_cmd_accept"
                        };
                    }

                    XS.Label("mouse_cmd_error");
                    {
                        XS.Mov(XSRegisters.AH, 0x01);
                        XS.Jump("mouse_cmd_exit");
                    }

                    XS.Label("mouse_cmd_ok");
                    {
                        XS.Xor(XSRegisters.AH, XSRegisters.RegistersEnum.AH);
                    }

                    XS.Label("mouse_cmd_exit");
                    {
                        XS.Return();
                    }
                }
        #endregion

            }
        }
        #endregion

        private static class InternalMouseEnable
        {
            public static void EnableMouse() { }
        }

        [Plug(Target = typeof(global::Cosmos.Hardware.AsmMouse.InternalMouseEnable))]
        private static class InternalMousePlugged
        {
            [PlugMethod(Assembler = typeof(global::Cosmos.Hardware.AsmMouse.EnableMouseASM))]
            public static void EnableMouse() { }
        }

        #endregion

        private IOPort p60 = new IOPort(0x60);
        private IOPort p64 = new IOPort(0x64);
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
        /// This is the required call to start
        /// the mouse receiving interrupts.
        /// </summary>
        public void Initialize()
        {
            AsmMouse.InternalMouseEnable.EnableMouse();
            Cosmos.Core.INTs.SetIrqHandler(12, new INTs.InterruptDelegate(HandleMouse));
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

        private byte mouse_cycle = 0;
        private int[] mouse_byte = new int[4];


        /// <summary>
        /// This is the default mouse handling code.
        /// </summary>
        /// <param name="context"></param>
        public void HandleMouse(ref INTs.IRQContext context)
        {
            switch (mouse_cycle)
            {
                case 0:
                    mouse_byte[0] = p60.Byte;

                    if ((mouse_byte[0] & 0x8) == 0x8)
                        mouse_cycle++;

                    break;
                case 1:
                    mouse_byte[1] = p60.Byte;
                    mouse_cycle++;
                    break;
                case 2:
                    mouse_byte[2] = p60.Byte;
                    mouse_cycle = 0;

                    if ((mouse_byte[0] & 0x10) == 0x10)
                        X -= mouse_byte[1] ^ 0xff;
                    else
                        X += mouse_byte[1];

                    if ((mouse_byte[0] & 0x20) == 0x20)
                        Y += mouse_byte[2] ^ 0xff;
                    else
                        Y -= mouse_byte[2];

                    if (X < 0)
                        X = 0;
                    else if (X > 319)
                        X = 319;

                    if (Y < 0)
                        Y = 0;
                    else if (Y > 199)
                        Y = 199;

                    Buttons = (MouseState)(mouse_byte[0] & 0x7);

                    break;
            }
        }

    }
}

#endif