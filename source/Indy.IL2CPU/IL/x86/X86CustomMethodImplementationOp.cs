using System;
using System.Collections.Generic;
using System.Linq;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class X86CustomMethodImplementationOp: CustomMethodImplementationOp {
		public X86CustomMethodImplementationOp(ILReader aReader, MethodInformation aMethodInfo): base(aReader, aMethodInfo) {
		}

		/// <summary>
		/// Passes the call directly to an equal method. 
		/// </summary>
		/// <param name="aMethod"></param>
		protected void PassCall(MethodBase aMethod) {
			for (int i = 0; i < MethodInfo.Arguments.Length; i++) {
				Ldarg.Ldarg(Assembler, MethodInfo.Arguments[i]);
			}
			Engine.QueueMethod(aMethod);
		    throw new NotImplementedException();
			//Op xOp = new Call(aMethod, 0, MethodInfo.DebugMode);
			//xOp.Assembler = Assembler;
			//xOp.Assemble();
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___() {
			// arg 0 == this
			AssembleOp(new Ldarg(MethodInfo, 0));
            new Indy.IL2CPU.Assembler.X86.Pop { DestinationReg = CPUx86.Registers.EAX };
			uint xStorageSize;
			Dictionary<string, TypeInformation.Field> xFields = Engine.GetTypeFieldInfo(typeof (string), out xStorageSize);
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)xFields["$$Storage$$"].Offset };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 12 };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
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
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		}

		protected override void Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___() {
			// arg 0 == this
			// arg 1 == value
			// arg 2 == index
			AssembleOp(new Ldarg(MethodInfo, 0));
			AssembleOp(new Ldarg(MethodInfo, 2));
			AssembleOp(new Ldarg(MethodInfo, 1));
			Stelem_Ref.Assemble(Assembler, 4);
		}

		protected override void Assemble_System_Int32_System_Array_get_Length____() {
			AssembleOp(new Ldarg(MethodInfo, 0));
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new Indy.IL2CPU.Assembler.X86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8 };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		}
	}
}