using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System.Runtime.CompilerServices {
	[Plug(Target = typeof(RuntimeHelpers))]
	public static class RuntimeHelpersImpl {
		[PlugMethod(Signature = "System_Void__System_Runtime_CompilerServices_RuntimeHelpers__cctor__")]
		public static void CCtor() {
			//todo: do something
		}

		[PlugMethod(MethodAssembler = typeof(InitializeArrayAssembler))]
		public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle) {
		}
	}

	public class InitializeArrayAssembler: AssemblerMethod {
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
			new Assembler.X86.Move(Assembler.X86.Registers.EDI, "[ebp+0xC]"); // aArray
			new Assembler.X86.Move(Assembler.X86.Registers.ESI, "[ebp+8]"); // aFieldHandle
			new Assembler.X86.Add(Assembler.X86.Registers.EDI, "8");
			new Assembler.X86.Push("dword [edi]");
			new Assembler.X86.Add(Assembler.X86.Registers.EDI, "4");
			new Assembler.X86.Move("eax", "[edi]");
			new Assembler.X86.Multiply("dword [esp]");
			new Assembler.X86.Pop("ecx");
			new Assembler.X86.Move("ecx", "eax");
			new Assembler.X86.Move(Assembler.X86.Registers.EAX, "0");
			new Assembler.X86.Add(Assembler.X86.Registers.EDI, "4");

			new Assembler.Label(".StartLoop");
			new Assembler.X86.Move("byte", Assembler.X86.Registers.DL, Assembler.X86.Registers.AtESI);
			new Assembler.X86.Move("byte", Assembler.X86.Registers.AtEDI, Assembler.X86.Registers.DL);
			new Assembler.X86.Add(Assembler.X86.Registers.EAX, "1");
			new Assembler.X86.Add(Assembler.X86.Registers.ESI, "1");
			new Assembler.X86.Add(Assembler.X86.Registers.EDI, "1");
			new Assembler.X86.Compare(Assembler.X86.Registers.EAX, Assembler.X86.Registers.ECX);
			new Assembler.X86.JumpIfEqual(".EndLoop");
			new Assembler.X86.Jump(".StartLoop");

			new Assembler.Label(".EndLoop");
		}
	}
}