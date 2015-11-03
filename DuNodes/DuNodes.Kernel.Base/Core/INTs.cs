//TODO : Rewrite class
/////*
////Copyright (c) 2012-2013, dewitcher Team
////All rights reserved.

////Redistribution and use in source and binary forms, with or without modification,
////are permitted provided that the following conditions are met:

////* Redistributions of source code must retain the above copyright notice
////   this list of conditions and the following disclaimer.

////* Redistributions in binary form must reproduce the above copyright notice,
////  this list of conditions and the following disclaimer in the documentation
////  and/or other materials provided with the distribution.

////THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
////IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
////FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
////CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
////DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
////DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
////IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
////THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////*/

//using Cosmos.IL2CPU.Plugs;
//using CPUx86 = Cosmos.Assembler.x86;

// Credits for STIEnabler goes to Grunt =)
//namespace DuNodes.Kernel.Base.Core
//{
//    [Plug(Target = typeof(Cosmos.Core.INTs))]
//    public partial class INTs
//    {
//        static bool already = false;
//        private static string[] errs = new string[] { "DIVIDE_BY_ZERO", "SINGLE_STEP", "NON_MASKABLE_INTERRUPT", "BREAK_FLOW", "OVERFLOW", "NULL", "INVALID_OPCODE", "", "DOUBLE_FAULT_EXCEPTION", "INVALID_TSS", "SEGMENT_NOT_PRESENT", "STACK_EXCEPTION", "GENERAL_PROTECTION_FAULT" };

//        public static void HandleInterrupt_Default(ref Cosmos.Core.INTs.IRQContext aContext)
//        {

//            API.HandleInt0x80(ref aContext);
//            Interrupts.Handlers[aContext.Interrupt](ref aContext);

//        }
//        public static void HandleInterrupt_00(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[0]);
//        }


//        public static void HandleInterrupt_01(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[1]);
//        }
//        public static void HandleInterrupt_02(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[2]);
//        }
//        public static void HandleInterrupt_03(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[3]);
//        }
//        public static void HandleInterrupt_04(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[4]);
//        }
//        public static void HandleInterrupt_05(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[5]);
//        }
//        public static void HandleInterrupt_06(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[6]);
//        }
//        public static void HandleInterrupt_07(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[7]);
//        }
//        public static void HandleInterrupt_08(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[8]);
//        }
//        public static void HandleInterrupt_09(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[9]);
//        }
//        public static void HandleInterrupt_0A(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[10]);
//        }
//        public static void HandleInterrupt_0B(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[11]);

//        }
//        public static void HandleInterrupt_0C(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[12]);
//        }
//        public static void HandleInterrupt_0D(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[13]);
//        }
//        public static void HandleInterrupt_0E(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[14]);
//        }
//        public static void HandleInterrupt_0F(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            FlowDOS.BSOD.Panic(errs[15]);
//        }
//    }


//    public class API
//    {
//        public static void STI()
//        {
//        }
//        Basically the way this works is a number is stored in EAX, this is the function
//         we want to use. All of these can be accessed through software interrupt 0x80
//        public unsafe static void HandleInt0x80(ref Cosmos.Core.INTs.IRQContext aContext)
//        {
//            if (aContext.EAX == 1) // Print
//            {
//                byte* ptr = (byte*)aContext.ESI;
//                for (int i = 0; ptr[i] != 0; i++)
//                {
//                    Console.Write((char)ptr[i]);
//                }
//            }
//            else if (aContext.EAX == 2) // Read
//            {
//                STIEnabler se = new STIEnabler();
//                se.Enable();
//                STI(); // We need to enable interrupts so we can read, but for some reason this does not work :(

//                byte* ptr = (byte*)aContext.EDI; // Input buffer
//                string str = Console.ReadLine();
//                for (int i = 0; i < str.Length; i++)
//                    ptr[i] = (byte)str[i];
//            }
//        }
//    }
//    class STIEnabler
//    {
//        public void Enable()
//        {
//        }
//    }
//    [Plug(Target = typeof(STIEnabler))]
//    public class Enable : AssemblerMethod
//    {
//        public override void AssembleNew(object aAssembler, object aMethodInfo)
//        {
//            new CPUx86.Sti();

//        }
//    }

//}
