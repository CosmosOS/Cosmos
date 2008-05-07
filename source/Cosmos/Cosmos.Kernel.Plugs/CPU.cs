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
		public static void CreateIDT() {
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
			Console.Write("Clearing ");
			Cosmos.Hardware.Storage.ATAOld.WriteNumber(aLength, 32);
			Console.Write(" bytes at ");
			Cosmos.Hardware.Storage.ATAOld.WriteNumber(aStartAddress, 32);
			Console.WriteLine("");
			uint* xPtr = (uint*)aStartAddress;
			for (uint i = 0; i < (aLength / 4); i++) {
				xPtr[i] = 0;
				if (i % (256 * 1024) == 0) {
					Console.Write("Cleared Megabyte ");
					Cosmos.Hardware.Storage.ATAOld.WriteNumber(i / (256 * 1024), 16);
					Console.WriteLine();
				}
			}
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
	}

    [Plug(TargetName = "MatthijsTest.Program, MatthijsTest")]
    public class TestPlug
    {
        [PlugMethod(MethodAssembler = typeof(GetResumeAndResumeAssembler))]
        public static void GetResumeAndResume(ref uint aSuspend)
        {
            aSuspend = 0;
        }
    }

    public class GetResumeAndResumeAssembler : AssemblerMethod
    {
        public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler)
        {
            new Move("eax",
                     "[ebp + 8]");
            new Move("ebx", "[DebugSuspendLevel]");
            new Move("[eax]", "ebx");
        }
    }
}
