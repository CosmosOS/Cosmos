using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using HW = Cosmos.Hardware2;
using P2 = Cosmos.Kernel.Plugs;

namespace Cosmos.Core.Plugs {
    [Plug(Target = typeof(Kernel.CPU))]
    public static class CPU {
        //TODO: Change this to allow plugs directly on a class intead of needing this "proxy" class
        [PlugMethod(Assembler = typeof(Assemblers.CreateIDT))]
        public static void CreateIDT(bool aEnableInterruptsImmediately) {
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.CreateGDT))]
        public static void CreateGDT() { }

        [PlugMethod(Assembler = typeof(P2.Assemblers.GetAmountOfRAM))]
        public static uint GetAmountOfRAM() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.GetEndOfKernel))]
        public static uint GetEndOfKernel() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.ZeroFill))]
        // TODO: implement this using REP STOSB and REPO STOSD
        public static unsafe void ZeroFill(uint aStartAddress, uint aLength) {
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.GetEndOfStack))]
        public static uint GetEndOfStack() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.GetCurrentESP))]
        public static uint GetCurrentESP() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.DoTest))]
        public static void DoTest() {
        }


        [PlugMethod(Assembler = typeof(P2.Assemblers.CPUIDSupport))]
        public static uint HasCPUIDSupport() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.GetCPUIDInternal))]
        public static void GetCPUId(out uint d, out uint c, out uint b, out uint a, uint v) {
            d = 0;
            c = 0;
            b = 0;
            a = 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.Halt))]
        public static void Halt() { }

        [PlugMethod(Assembler = typeof(P2.Assemblers.DisableInterrupts))]
        public static void DisableInterrupts() {
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.EnableInterrupts))]
        public static void EnableInterrupts() {
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.Interrupt30))]
        public static void Interrupt30(ref uint aEAX, ref uint aEBX, ref uint aECX, ref uint aEDX) {
            aEAX = 0;
        }

        [PlugMethod(Assembler = typeof(P2.Assemblers.GetMBIAddress))]
        public static uint GetMBIAddress() {
            return 0;
        }
    }
}
