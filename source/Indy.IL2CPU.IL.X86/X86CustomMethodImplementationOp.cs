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
	}
}
