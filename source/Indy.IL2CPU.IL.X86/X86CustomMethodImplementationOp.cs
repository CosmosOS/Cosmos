using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class X86CustomMethodImplementationOp: CustomMethodImplementationOp {
		public X86CustomMethodImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		/// <summary>
		/// Passes the call directly to an equal method. 
		/// </summary>
		/// <param name="aMethod"></param>
		protected void PassCall(MethodReference aMethod) {
			for (int i = 0; i < aMethod.Parameters.Count; i++) {
				Ldarg.Ldarg(Assembler, MethodInfo.Arguments[i].VirtualAddresses, MethodInfo.Arguments[i].Size);
			}
			Engine.QueueMethodRef(aMethod);
			Op xOp = new Call(aMethod);
			xOp.Assembler = Assembler;
			xOp.Assemble();
		}

		protected override void Assemble_System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___() {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
			new Indy.IL2CPU.Assembler.X86.Move("eax", "0");
			new Indy.IL2CPU.Assembler.X86.Move("edi", "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");
			new Indy.IL2CPU.Assembler.X86.Move("esi", "[" + MethodInfo.Arguments[1].VirtualAddresses[0] + "]");
			new Indy.IL2CPU.Assembler.X86.Add("dword esi", "8");
			new Indy.IL2CPU.Assembler.X86.Move("ecx", "[esi]");
			new Indy.IL2CPU.Assembler.X86.Add("dword esi", "4");
			new Indy.IL2CPU.Assembler.X86.Add("dword edi", "12");

			new Assembler.Label(".StartLoop");
			new Indy.IL2CPU.Assembler.X86.Move("edx", "[esi]");
			new Indy.IL2CPU.Assembler.X86.Move("[edi]", "edx");
			new Indy.IL2CPU.Assembler.X86.Add("eax", "4");
			new Indy.IL2CPU.Assembler.X86.Add("dword esi", "4");
			new Indy.IL2CPU.Assembler.X86.Add("dword edi", "4");
			new Indy.IL2CPU.Assembler.X86.Compare("eax", "ecx");
			new Indy.IL2CPU.Assembler.X86.JumpIfEquals(".EndLoop");
			new Indy.IL2CPU.Assembler.X86.JumpAlways(".StartLoop");

			new Assembler.Label(".EndLoop");
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___() {
			// arg 0 == this
			AssembleOp(new Ldarg(MethodInfo, 0));
			new Indy.IL2CPU.Assembler.X86.Pop("eax");
			if (!mAssembler.InMetalMode) {
				int xStorageSize;
				SortedList<string, TypeInformation.Field> xFields = Engine.GetTypeFieldInfo(Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]"), out xStorageSize);
				new Indy.IL2CPU.Assembler.X86.Add("eax", "0" + xFields["$$Storage$$"].Offset.ToString("X") + "h");
				new Indy.IL2CPU.Assembler.X86.Move("eax", "[eax]");
				new Indy.IL2CPU.Assembler.X86.Add("eax", "12");
				new Indy.IL2CPU.Assembler.X86.Pushd("eax");
			}
		}

		private void AssembleOp(X86.Op aOp) {
			aOp.Assembler = Assembler;
			aOp.Assemble();
		}

		protected override void Assemble_System_Object___System_Array_GetValue___System_Int32___() {
			// arg 0 == this
			// arg 1 == Index
			AssembleOp(new Ldarg(MethodInfo, 0));
			AssembleOp(new Ldarg(MethodInfo, 1));
			AssembleOp(new Ldelem_I4(null, MethodInfo));
			new Indy.IL2CPU.Assembler.X86.Pop("eax");
		}

		protected override void Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___() {
			// arg 0 == this
			// arg 1 == value
			// arg 2 == index
			AssembleOp(new Ldarg(MethodInfo, 0));
			AssembleOp(new Ldarg(MethodInfo, 2));
			AssembleOp(new Ldarg(MethodInfo, 1));
			Stelem_Any.Assemble(Assembler, 4);
		}

		protected override void Assemble_System_Int32_System_Array_get_Length____() {
			AssembleOp(new Ldarg(MethodInfo, 0));
			new Indy.IL2CPU.Assembler.X86.Popd("eax");
			new Indy.IL2CPU.Assembler.X86.Pushd("[eax + 8]");
			new Indy.IL2CPU.Assembler.X86.Popd("eax");
		}
	}
}
