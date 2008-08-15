using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(Kernel.CPU))]
    public static class CPU {
		[PlugMethod(MethodAssembler = typeof(Assemblers.CreateIDT))]
		public static void CreateIDT(bool aEnableInterruptsImmediately) {
		}

        [PlugMethod(MethodAssembler = typeof(Assemblers.CreateGDT))]
        public static void CreateGDT() { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.GetAmountOfRAM))]
        public static uint GetAmountOfRAM() {
            return 0;
        }

        [PlugMethod(MethodAssembler = typeof(Assemblers.GetEndOfKernel))]
        public static uint GetEndOfKernel() {
            return 0;
        }
        
        [PlugMethod(MethodAssembler = typeof(Assemblers.ZeroFill))]
		// TODO: implement this using REP STOSB and REPO STOSD
		public static unsafe void ZeroFill(uint aStartAddress, uint aLength) {
		}

		[PlugMethod(MethodAssembler = typeof(Assemblers.GetEndOfStack))]
		public static uint GetEndOfStack() {
			return 0;
		}

		[PlugMethod(MethodAssembler = typeof(Assemblers.GetCurrentESP))]
		public static uint GetCurrentESP() {
			return 0;
		}

		[PlugMethod(MethodAssembler=typeof(Assemblers.DoTest))]
		public static void DoTest() {
		}


        [PlugMethod(MethodAssembler = typeof(Assemblers.CPUIDSupport))]
        public static uint HasCPUIDSupport()
        {
            return 0;
        }

        [PlugMethod(MethodAssembler=typeof(Assemblers.GetCPUIDInternal))]
        public static void GetCPUId(out uint d, out uint c, out uint b, out uint a, uint v)
        {
            d = 0;
            c = 0;
            b = 0;
            a = 0;
        }

        [PlugMethod(MethodAssembler = typeof(Assemblers.Halt))]
        public static void Halt()
        {

        }

        [PlugMethod(MethodAssembler = typeof(Assemblers.Interrupt30))]
        public static void Interrupt30(ref uint aEAX, ref uint aEBX, ref uint aECX, ref uint aEDX) {
            aEAX = 0;
        }

        [PlugMethod(MethodAssembler = typeof(Assemblers.LoadTSS))]
        public static void LoadTSS() {
            //
        }
    }
}