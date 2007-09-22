using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86RuntimeHelpers_InitializeArrayOp: Op {
		private readonly MethodInformation MethodInfo;
		public X86RuntimeHelpers_InitializeArrayOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodInfo = aMethodInfo;
		}

		public override void DoAssemble() {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
			Move(Assembler, "eax", "0");
			Move(Assembler, "edi", "[" + MethodInfo.Arguments[0].VirtualAddress + "]");
			Move(Assembler, "esi", "[" + MethodInfo.Arguments[1].VirtualAddress + "]");
			Move(Assembler, "ecx", "[esi]");
			Assembler.Add(new CPUx86.Add("dword esi", "4"));
			Assembler.Add(new CPUx86.Add("dword edi", "12"));

			Assembler.Add(new CPU.Label(".StartLoop"));
			Move(Assembler, "edx", "[esi]");
			Move(Assembler, "[edi]", "edx");
			Assembler.Add(new CPUx86.Add("eax", "4"));
			Assembler.Add(new CPUx86.Add("dword esi", "4"));
			Assembler.Add(new CPUx86.Add("dword edi", "4"));
			Assembler.Add(new CPUx86.Compare("eax", "ecx"));
			JumpIfEquals(".EndLoop");
			JumpAlways(".StartLoop");

			Assembler.Add(new CPU.Label(".EndLoop"));
		}
	}
}
