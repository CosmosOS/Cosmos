///*
//Copyright (c) 2012-2013, dewitcher Team
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification,
//are permitted provided that the following conditions are met:

//* Redistributions of source code must retain the above copyright notice
//   this list of conditions and the following disclaimer.

//* Redistributions in binary form must reproduce the above copyright notice,
//  this list of conditions and the following disclaimer in the documentation
//  and/or other materials provided with the distribution.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
//IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
//IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
//THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//*/

//using Cosmos.IL2CPU.Plugs;
//using DuNodes.Kernel.Base.IO;
//using CPUx86 = Cosmos.Assembler.x86;
//using Global = Cosmos.Core.Global;
//using IRQContext = Cosmos.Core.INTs.IRQContext;
//// Credits for STIEnabler goes to Grunt =)
//namespace DuNodes.Kernel.Base.Core
//{
//    [Plug(Target = typeof(Cosmos.Core.INTs))]
//    public partial class INTs
//    {
//        public delegate void IRQ0called();
//        public static IRQ0called onCalled = delegate { PIT.called = true; };
//        public static void HandleInterrupt_Default(ref IRQContext aContext)
//        {
//            if (aContext.Interrupt >= 0x20 && aContext.Interrupt <= 0x2F)
//            {
//                if (aContext.Interrupt >= 0x28)
//                {
//                    Global.PIC.EoiSlave();
//                }
//                else
//                {
//                    Global.PIC.EoiMaster();
//                }
//            }
//        }

//        public static void HandleInterrupt_00(ref IRQContext aContext)
//        {
           
//            //Bluescreen.Init("Divide by zero Exception", "YOU JUST CREATED A BLACK HOLE!", true); 
//        }

//        public static void HandleInterrupt_01(ref IRQContext aContext)
//        {
//            //Bluescreen.Init("Debug Exception", " ", true); 
//        }

//        public static void HandleInterrupt_02(ref IRQContext aContext)
//        {
//            //Bluescreen.Init("Non Maskable Interrupt Exception", " ", true); 
//        }

//        public static void HandleInterrupt_03(ref IRQContext aContext)
//        {
//           // Bluescreen.Init("Breakpoint Exception", " ", true); 
//        }

//        public static void HandleInterrupt_04(ref IRQContext aContext)
//        {
//            //Bluescreen.Init("Into Detected Overflow Exception", "" , true); 
//        }

//        public static void HandleInterrupt_05(ref IRQContext aContext)
//        {
//           // Bluescreen.Init("Out of Bounds Exception", " ", true); 
//        }

//        public static void HandleInterrupt_06(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("Invalid Opcode", " ", true); 
//        }

//        public static void HandleInterrupt_07(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("No Coprocessor Exception", " ", true); 
//        }

//        public static void HandleInterrupt_08(ref IRQContext aContext)
//        {
//            //   Bluescreen.Init("Double Fault Exception", " ", true); 
//        }

//        public static void HandleInterrupt_09(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("Coprocessor Segment Overrun Exception", " ", true); 
//        }

//        public static void HandleInterrupt_0A(ref IRQContext aContext)
//        {
//            // Bluescreen.Init("Bad TSS Exception", " ", true); 
//        }

//        public static void HandleInterrupt_0B(ref IRQContext aContext)
//        {
//            // Bluescreen.Init("Segment not present", " ", true); 
//        }

//        public static void HandleInterrupt_0C(ref IRQContext aContext)
//        {
//            // Bluescreen.Init("Stack Fault Exception", " ", true); 
//        }

//        public static void HandleInterrupt_0E(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("Page Fault Exception", " ", true); 
//        }

//        public static void HandleInterrupt_0F(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("Unknown Interrupt Exception", " ", true); 
//        }

//        public static void HandleInterrupt_10(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("Coprocessor Fault Exception", " ", true); 
//        }

//        public static void HandleInterrupt_11(ref IRQContext aContext)
//        {
//            //  Bluescreen.Init("Alignment Exception", " ", true); 
//        }

//        public static void HandleInterrupt_12(ref IRQContext aContext)
//        {
//            //   Bluescreen.Init("Machine Check Exception", " ", true); 
//        }

//        // IRQ0
//        public static void HandleInterrupt_20(ref IRQContext aContext)
//        {
//            Global.PIC.EoiMaster();
//            PortIO.outb(0x20, 0x20);
//            onCalled();
//        }
//    }




//    public class STIEnabler
//    {
//        public void Enable()
//        {

//        }
//    }
//    [Plug(Target = typeof(STIEnabler))]
//    public class Enable : AssemblerMethod
//    {
//        public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
//        {
//            new CPUx86.Sti();
//        }
//    }
//}
