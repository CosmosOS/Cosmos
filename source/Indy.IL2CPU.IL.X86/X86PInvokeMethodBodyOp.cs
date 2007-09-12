using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;


namespace Indy.IL2CPU.IL.X86 {
	public class X86PInvokeMethodBodyOp: PInvokeMethodBodyOp {
		public X86PInvokeMethodBodyOp(MethodDefinition aTheMethod, MethodInformation aMethodInfo)
			: base(aTheMethod, aMethodInfo) {
		}

		private void MakeSureMethodIsRegistered(string aDllName, string aDllFileName, string aMethodName) {
			ImportMember xTheMember = null;
			foreach (ImportMember xMember in Assembler.ImportMembers) {
				if (xMember.Name == aDllName && xMember.FileName == aDllFileName) {
					xTheMember = xMember;
					foreach (ImportMethodMember xMethod in xMember.Methods) {
						if (xMethod.Name == aMethodName) {
							return;
						}
					}
					break;
				}
			}
			if (xTheMember == null) {
				xTheMember = new ImportMember(aDllName, aDllFileName);
				Assembler.ImportMembers.Add(xTheMember);
			}
			xTheMember.Methods.Add(new ImportMethodMember(aMethodName));
		}

		public override void Assemble() {
			string xDllFileName = TheMethod.PInvokeInfo.Module.Name;
			string xDllName = xDllFileName.Replace('.', '_');
			string xMethodName = TheMethod.PInvokeInfo.EntryPoint;
			if (String.IsNullOrEmpty(xMethodName)) {
				xMethodName = TheMethod.Name;
			}
			if (TheMethod.PInvokeInfo.IsCharSetNotSpec) {
				foreach (ParameterDefinition xParam in TheMethod.Parameters) {
					if (xParam.ParameterType.FullName.StartsWith("System.String")) {
						xMethodName += "W";
						break;
					}
				}
			}
			if (String.IsNullOrEmpty(xDllName)) {
				throw new Exception("Unable to determine what dll to use!");
			}
			MakeSureMethodIsRegistered(xDllName, xDllFileName, xMethodName);
			for (int i = MethodInfo.Arguments.Length - 1; i >= 0; i--) {
				Op.Ldarg(Assembler, MethodInfo.Arguments[i].VirtualAddress);
			}
			Assembler.Add(new CPUx86.Call("[" + xMethodName + "]"));
//			if (MethodInfo.HasReturnValue) {
//				Assembler.Add(new CPUx86.Pushd("0"));
//				Assembler.Add(new CPUx86.Pop("eax"));
//			}
		}
	}
}
