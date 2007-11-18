using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

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
				Ldarg.Ldarg(Assembler, MethodInfo.Arguments[i]);
			}
			Engine.QueueMethodRef(aMethod);
			Op xOp = new Call(aMethod);
			xOp.Assembler = Assembler;
			xOp.Assemble();
		}

		protected override void Assemble_System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___() {
			// Arguments:
			//    Array aArray, RuntimeFieldHandle aFieldHandle
			new Assembler.X86.Move(CPUx86.Registers.EAX, "0");
			new Assembler.X86.Move(CPUx86.Registers.EDI, "[" + MethodInfo.Arguments[0].VirtualAddresses[0] + "]");
			new Assembler.X86.Move(CPUx86.Registers.ESI, "[" + MethodInfo.Arguments[1].VirtualAddresses[0] + "]");
			new Assembler.X86.Add(CPUx86.Registers.ESI, "8");
			new Assembler.X86.Move(CPUx86.Registers.ECX, CPUx86.Registers.AtESI);
			new Assembler.X86.Add(CPUx86.Registers.ESI, "4");
			new Assembler.X86.Add(CPUx86.Registers.EDI, "12");

			new Assembler.Label(".StartLoop");
			new Assembler.X86.Move(CPUx86.Registers.EDX, CPUx86.Registers.AtESI);
			new Assembler.X86.Move(CPUx86.Registers.AtEDI, CPUx86.Registers.EDX);
			new Assembler.X86.Add(CPUx86.Registers.EAX, "4");
			new Assembler.X86.Add(CPUx86.Registers.ESI, "4");
			new Assembler.X86.Add(CPUx86.Registers.EDI, "4");
			new Assembler.X86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.ECX);
			new Assembler.X86.JumpIfEquals(".EndLoop");
			new Assembler.X86.JumpAlways(".StartLoop");

			new Assembler.Label(".EndLoop");
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___() {
			// arg 0 == this
			AssembleOp(new Ldarg(MethodInfo, 0));
			new Indy.IL2CPU.Assembler.X86.Pop(CPUx86.Registers.EAX);
			if (!mAssembler.InMetalMode) {
				int xStorageSize;
				SortedList<string, TypeInformation.Field> xFields = Engine.GetTypeFieldInfo(Engine.GetMethodDefinition(Engine.GetTypeDefinition("mscorlib", "System.String"), ".ctor", "System.Char[]"), out xStorageSize);
				new Indy.IL2CPU.Assembler.X86.Add(CPUx86.Registers.EAX, "0" + xFields["$$Storage$$"].Offset.ToString("X") + "h");
				new Indy.IL2CPU.Assembler.X86.Move(CPUx86.Registers.EAX, CPUx86.Registers.AtEAX);
				new Indy.IL2CPU.Assembler.X86.Add(CPUx86.Registers.EAX, "12");
				new Indy.IL2CPU.Assembler.X86.Pushd(CPUx86.Registers.EAX);
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
			new Indy.IL2CPU.Assembler.X86.Pop(CPUx86.Registers.EAX);
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
			new Indy.IL2CPU.Assembler.X86.Popd(CPUx86.Registers.EAX);
			new Indy.IL2CPU.Assembler.X86.Pushd("[eax + 8]");
			new Indy.IL2CPU.Assembler.X86.Popd(CPUx86.Registers.EAX);
		}
	}
}
