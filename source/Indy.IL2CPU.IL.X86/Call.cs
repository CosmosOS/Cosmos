using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class Call: IL.Call {
		public void Assemble(string aMethod) {
			new CPU.Call(aMethod);
		}

		public override void Assemble(Instruction aInstruction) {
			MethodDefinition xMethod = (MethodDefinition)aInstruction.Operand;
			string[] xParams = new string[xMethod.Parameters.Count];
			for (int i = 0; i < xParams.Length; i++) {
				xParams[i] = xMethod.Parameters[i].ParameterType.FullName;
			}
			Assemble(Assembler.Assembler.GetLabelName(xMethod.DeclaringType.FullName, xMethod.ReturnType.ReturnType.FullName, xMethod.Name, xParams));
		}
	}
}