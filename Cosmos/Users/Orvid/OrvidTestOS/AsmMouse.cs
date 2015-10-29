//#define ASMMouse

#if ASMMouse
//#define DebugMouse

using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
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
                new Move
                {
                    DestinationReg = RegistersEnum.BL,
                    SourceValue = 0xa8
                };

                new Call
                {
                    DestinationLabel = "send_mouse_cmd"
                };

                new Call
                {
                    DestinationLabel = "mouse_read"
                };

                new Noop();

                new Move
                {
                    DestinationReg = RegistersEnum.BL,
                    SourceValue = 0x20
                };

                new Call
                {
                    DestinationLabel = "send_mouse_cmd"
                };

                new Call
                {
                    DestinationLabel = "mouse_read"
                };

                new Or
                {
                    DestinationReg = RegistersEnum.AL,
                    SourceValue = 3
                };

                new Move
                {
                    DestinationReg = RegistersEnum.BL,
                    SourceValue = 0x60
                };

                new Push
                {
                    DestinationReg = RegistersEnum.EAX
                };

                new Call
                {
                    DestinationLabel = "send_mouse_cmd"
                };

                new Pop
                {
                    DestinationReg = RegistersEnum.EAX
                };

                new Call
                {
                    DestinationLabel = "mouse_write"
                };

                new Noop();

                new Move
                {
                    DestinationReg = RegistersEnum.BL,
                    SourceValue = 0xd4
                };

                new Call
                {
                    DestinationLabel = "send_mouse_cmd"
                };

                new Move
                {
                    DestinationReg = RegistersEnum.AL,
                    SourceValue = 0xf4
                };

                new Call
                {
                    DestinationLabel = "mouse_write"
                };

                new Call
                {
                    DestinationLabel = "mouse_read"
                };

        #region mouse_read
                new Label("mouse_read");
                {
                    new Push
                    {
                        DestinationReg = RegistersEnum.ECX
                    };

                    new Push
                    {
                        DestinationReg = RegistersEnum.EDX
                    };

                    new Move
                    {
                        DestinationReg = RegistersEnum.ECX,
                        SourceValue = 0xffff
                    };

                    new Label("mouse_read_loop");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 1
                        };

                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.NotZero,
                            DestinationLabel = "mouse_read_ready"
                        };

                        new Loop
                        {
                            DestinationLabel = "mouse_read_loop"
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceValue = 1
                        };

                        new Jump
                        {
                            DestinationLabel = "mouse_read_exit"
                        };
                    }

                    new Label("mouse_read_ready");
                    {
                        new Push
                        {
                            DestinationReg = RegistersEnum.ECX
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.ECX,
                            SourceValue = 32
                        };
                    }

                    new Label("mouse_read_delay");
                    {
                        new Loop
                        {
                            DestinationLabel = "mouse_read_delay"
                        };

                        new Pop
                        {
                            DestinationReg = RegistersEnum.ECX
                        };

                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x60,
                            Size = 8
                        };

                        new Xor
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceReg = RegistersEnum.AH
                        };
                    }

                    new Label("mouse_read_exit");
                    {
                        new Pop
                        {
                            DestinationReg = RegistersEnum.EDX
                        };

                        new Pop
                        {
                            DestinationReg = RegistersEnum.ECX
                        };

                        new Return();
                    }
                }
        #endregion

        #region mouse_write
                new Label("mouse_write");
                {
                    new Push
                    {
                        DestinationReg = RegistersEnum.ECX
                    };

                    new Push
                    {
                        DestinationReg = RegistersEnum.EDX
                    };

                    new Move
                    {
                        DestinationReg = RegistersEnum.BH,
                        SourceReg = RegistersEnum.AL
                    };

                    new Move
                    {
                        DestinationReg = RegistersEnum.ECX,
                        SourceValue = 0xffff
                    };

                    new Label("mouse_write_loop1");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 32
                        };

                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.Zero,
                            DestinationLabel = "mouse_write_ok1"
                        };

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop1"
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceValue = 1
                        };

                        new Jump
                        {
                            DestinationLabel = "mouse_write_exit"
                        };
                    }

                    new Label("mouse_write_ok1");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x60,
                            Size = 8
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.ECX,
                            SourceValue = 0xffff
                        };
                    }

                    new Label("mouse_write_loop");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 2
                        };

                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.Zero,
                            DestinationLabel = "mouse_write_ok"
                        };

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop"
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceValue = 1
                        };

                        new Jump
                        {
                            DestinationLabel = "mouse_write_exit"
                        };
                    }

                    new Label("mouse_write_ok");
                    {
                        new Move
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceReg = RegistersEnum.BH
                        };

                        new Out2Port
                        {
                            DestinationValue = 0x60,
                            SourceReg = RegistersEnum.AL,
                            Size = 8
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.ECX,
                            SourceValue = 0xffff
                        };
                    }

                    new Label("mouse_write_loop3");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 2
                        };

                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.Zero,
                            DestinationLabel = "mouse_write_ok3"
                        };

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop3"
                        };

                        new Move
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceValue = 1
                        };

                        new Jump
                        {
                            DestinationLabel = "mouse_write_exit"
                        };
                    }

                    new Label("mouse_write_ok3");
                    {
                        new Move
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceValue = 0x08
                        };
                    }

                    new Label("mouse_write_loop4");
                    {
                        new Move
                        {
                            DestinationReg = RegistersEnum.ECX,
                            SourceValue = 0xffff
                        };
                    }

                    new Label("mouse_write_loop5");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };

                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 1
                        };

                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.NotZero,
                            DestinationLabel = "mouse_write_ok4"
                        };

                        new Loop
                        {
                            DestinationLabel = "mouse_write_loop5"
                        };

                        new Dec
                        {
                            DestinationReg = RegistersEnum.AH
                        };

                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.NotZero,
                            DestinationLabel = "mouse_write_loop4"
                        };
                    }

                    new Label("mouse_write_ok4");
                    {
                        new Xor
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceReg = RegistersEnum.AH
                        };
                    }

                    new Label("mouse_write_exit");
                    {
                        new Pop
                        {
                            DestinationReg = RegistersEnum.EDX
                        };

                        new Pop
                        {
                            DestinationReg = RegistersEnum.ECX
                        };

                        new Return();
                    }
                }
        #endregion

        #region send_mouse_cmd
                new Label("send_mouse_cmd");
                {

                    new Move
                    {
                        DestinationReg = RegistersEnum.ECX,
                        SourceValue = 0xffff
                    };

                    new Label("mouse_cmd_wait");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };
                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 2
                        };
                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.Zero,
                            DestinationLabel = "mouse_cmd_send"
                        };
                        new Loop
                        {
                            DestinationLabel = "mouse_cmd_wait"
                        };
                        new Jump
                        {
                            DestinationLabel = "mouse_cmd_error"
                        };
                    }

                    new Label("mouse_cmd_send");
                    {
                        new Move
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceReg = RegistersEnum.BL
                        };
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
                        new Move
                        {
                            DestinationReg = RegistersEnum.ECX,
                            SourceValue = 0xffff
                        };
                    }

                    new Label("mouse_cmd_accept");
                    {
                        new In2Port
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x64,
                            Size = 8
                        };
                        new Test
                        {
                            DestinationReg = RegistersEnum.AL,
                            SourceValue = 0x02
                        };
                        new ConditionalJump
                        {
                            Condition = ConditionalTestEnum.Zero,
                            DestinationLabel = "mouse_cmd_ok"
                        };
                        new Loop
                        {
                            DestinationLabel = "mouse_cmd_accept"
                        };
                    }

                    new Label("mouse_cmd_error");
                    {
                        new Move
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceValue = 0x01
                        };
                        new Jump
                        {
                            DestinationLabel = "mouse_cmd_exit"
                        };
                    }

                    new Label("mouse_cmd_ok");
                    {
                        new Xor
                        {
                            DestinationReg = RegistersEnum.AH,
                            SourceReg = RegistersEnum.AH
                        };
                    }

                    new Label("mouse_cmd_exit");
                    {
                        new Return();
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