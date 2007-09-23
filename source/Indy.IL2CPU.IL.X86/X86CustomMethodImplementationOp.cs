using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	public class X86CustomMethodImplementationOp: CustomMethodImplementationOp {
		public X86CustomMethodImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		protected override void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___() {
			// arg 0 == this
			AssembleOp(new Ldarg(MethodInfo, 0));
			Assembler.Add(new CPUx86.Pop("eax"));
			uint xStorageSize;
			SortedList<string, TypeInformation.Field> xFields = Engine.GetTypeFieldInfo(Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]"), out xStorageSize);
			Assembler.Add(new CPUx86.Add("eax", "0" + xFields["$$Storage$$"].Offset.ToString("X") + "h"));
			Assembler.Add(new CPUx86.Move("eax", "[eax]"));
			Assembler.Add(new CPUx86.Add("eax", "12"));
			Assembler.Add(new CPUx86.Pushd("eax"));
		}

		private void AssembleOp(Op aOp) {
			aOp.Assembler = Assembler;
			aOp.Assemble();
		}

		protected override void Assemble_System_Object___System_Array_GetValue___System_Int32___() {
			// arg 0 == this
			// arg 1 == Index
			AssembleOp(new Ldarg(MethodInfo, 0));
			AssembleOp(new Ldarg(MethodInfo, 1));
			AssembleOp(new Ldelem_I4(null, MethodInfo));
			Assembler.Add(new CPUx86.Pop("eax"));
		}

		protected override void Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___() {
			// arg 0 == this
			// arg 1 == value
			// arg 2 == index
			AssembleOp(new Ldarg(MethodInfo, 0));
			AssembleOp(new Ldarg(MethodInfo, 2));
			AssembleOp(new Ldarg(MethodInfo, 1));
			Stelem_I4.Assemble(Assembler);
		}

		protected override void Assemble_System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___() {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
			Assembler.Add(new CPUx86.Pushd(MethodInfo.Arguments[0].VirtualAddress));
			Assembler.Add(new CPUx86.Pushd(MethodInfo.Arguments[1].VirtualAddress));
			Op x = new Call(Engine.GetMethodDefinition(Engine.GetTypeDefinition("", "Indy.IL2CPU.CustomImplementation.CompilerServices.RuntimeHelpers"), "InitializeArrayImpl", "System.Int32[]", "System.Int32[]"));
			x.Assembler = Assembler;
			x.Assemble();
//			Assembler.Add(new CPUx86.Move("eax", "0"));
//			Assembler.Add(new CPUx86.Move("edi", "[" + MethodInfo.Arguments[0].VirtualAddress + "]"));
//			Assembler.Add(new CPUx86.Move("esi", "[" + MethodInfo.Arguments[1].VirtualAddress + "]"));
//			Assembler.Add(new CPUx86.Move("ecx", "[esi]"));
//			Assembler.Add(new CPUx86.Add("dword esi", "12"));
//			Assembler.Add(new CPUx86.Add("dword edi", "12"));
//
//			Assembler.Add(new CPU.Label(".StartLoop"));
//			Assembler.Add(new CPUx86.Move("edx", "[esi]"));
//			Assembler.Add(new CPUx86.Move("[edi]", "edx"));
//			Assembler.Add(new CPUx86.Add("eax", "4"));
//			Assembler.Add(new CPUx86.Add("dword esi", "4"));
//			Assembler.Add(new CPUx86.Add("dword edi", "4"));
//			Assembler.Add(new CPUx86.Compare("eax", "ecx"));
//			Assembler.Add(new CPUx86.JumpIfEquals(".EndLoop"));
//			Assembler.Add(new CPUx86.JumpAlways(".StartLoop"));
//
//			Assembler.Add(new CPU.Label(".EndLoop"));
		}
	}
}
