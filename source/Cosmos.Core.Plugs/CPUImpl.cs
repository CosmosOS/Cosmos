using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using Assembler = Cosmos.Assembler.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs {
	[Plug(Target = typeof(Core.CPU))]
	public class CPUImpl {
		[PlugMethod(Assembler = typeof(UpdateIDT))]
	  public static void UpdateIDT(CPU aThis, bool aEnableInterruptsImmediately) {
		}

		public class GetAmountOfRAMAsm : AssemblerMethod {
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
				XS.Set(XSRegisters.EAX, "MultiBootInfo_Memory_High", sourceIsIndirect: true);
				XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), 1024);
				XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
				XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 1);
				XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
			}
		}

		[PlugMethod(Assembler = typeof(GetAmountOfRAMAsm))]
		public static uint GetAmountOfRAM() { return 0; }

		public class GetEndOfKernelAsm : AssemblerMethod {
			public override void AssembleNew(CPUAll.Assembler aAssembler, object aMethodInfo) {
				new CPUx86.Push { DestinationRef = CPUAll.ElementReference.New("_end_code") };
			}
		}

		[PlugMethod(Assembler = typeof(GetEndOfKernelAsm))]
		public static uint GetEndOfKernel() {
		  return 0;
		}

		public class ZeroFillAsm : AssemblerMethod {
			public override void AssembleNew(CPUAll.Assembler aAssembler, object aMethodInfo) {
				new CPUx86.ClrDirFlag();
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0xC); //address
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBP), sourceDisplacement: 0x8); //length
				// set EAX to value of fill (zero)
				XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
				XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), 1);
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotBelow, DestinationLabel = ".step2" };
				new CPUx86.StoreByteInString();
				new CPUAll.Label(".step2");
				XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), 1);
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotBelow, DestinationLabel = ".step3" };
				new CPUx86.StoreWordInString();
				new CPUAll.Label(".step3");
				new CPUx86.Stos { Size = 32, Prefixes = CPUx86.InstructionPrefixes.Repeat };
			}
		}
		[PlugMethod(Assembler = typeof(ZeroFillAsm))]
		// TODO: implement this using REP STOSB and REPO STOSD
		public static void ZeroFill(uint aStartAddress, uint aLength) { }

		public class InitFloatAsm : AssemblerMethod {
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
				new CPUx86.x87.FloatInit();
			}
		}

	  [PlugMethod(Assembler = typeof(InitFloatAsm))]
		public static void InitFloat(CPU aThis) { }

		public class HaltAsm : AssemblerMethod {
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
				new CPUx86.Halt();
			}
		}

	  [PlugMethod(Assembler = typeof(HaltAsm))]
		public static void Halt(CPU aThis) { }


	//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetEndOfStack))]
	//    public static uint GetEndOfStack() {
	//        return 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetCurrentESP))]
	//    public static uint GetCurrentESP() {
	//        return 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.DoTest))]
	//    public static void DoTest() {
	//    }


	//    [PlugMethod(Assembler = typeof(P2.Assemblers.CPUIDSupport))]
	//    public static uint HasCPUIDSupport() {
	//        return 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetCPUIDInternal))]
	//    public static void GetCPUId(out uint d, out uint c, out uint b, out uint a, uint v) {
	//        d = 0;
	//        c = 0;
	//        b = 0;
	//        a = 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.Halt))]
	//    public static void Halt() { }

		[PlugMethod(Assembler = typeof(DisableINTsAsm))]
		public static void DoDisableInterrupts()
		{
		}

		[PlugMethod(Assembler = typeof(EnableINTsAsm))]
		public static void DoEnableInterrupts() {
		}

		public class DisableINTsAsm : AssemblerMethod
		{
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
			{
				new CPUx86.ClearInterruptFlag();
			}
		}

		public class EnableINTsAsm : AssemblerMethod
		{
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
			{
				new CPUx86.Sti();
			}
		}

		//    [PlugMethod(Assembler = typeof(P2.Assemblers.Interrupt30))]
		//    public static void Interrupt30(ref uint aEAX, ref uint aEBX, ref uint aECX, ref uint aEDX) {
		//        aEAX = 0;
		//    }

		//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetMBIAddress))]
		//    public static uint GetMBIAddress() {
		//        return 0;
		//    }
	}
}
