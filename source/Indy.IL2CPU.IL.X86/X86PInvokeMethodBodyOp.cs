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

		public override void DoAssemble() {
			string xDllFileName = TheMethod.PInvokeInfo.Module.Name;
			string xDllName = xDllFileName.Replace('.', '_');
			string xMethodName = TheMethod.PInvokeInfo.EntryPoint;
			if (String.IsNullOrEmpty(xMethodName)) {
				xMethodName = TheMethod.Name;
			}
			new Comment("PInvokeAttributes = '" + TheMethod.PInvokeInfo.Attributes.ToString("G") + "'");
			string xStringMethodSuffix = "W";
			if(Assembler.InMetalMode) {
				xStringMethodSuffix = "A";
			}
			if (!TheMethod.PInvokeInfo.IsNoMangle) {
				if (TheMethod.PInvokeInfo.IsCharSetUnicode) {
					xMethodName += xStringMethodSuffix;
				} else {
					if (TheMethod.PInvokeInfo.IsCharSetAnsi) {
						xMethodName += xStringMethodSuffix;
					} else {
						if (TheMethod.PInvokeInfo.IsCharSetAuto) {
							xMethodName += xStringMethodSuffix;
						}
					}
				}
			}
			//			}
			if (String.IsNullOrEmpty(xDllName)) {
				throw new Exception("Unable to determine what dll to use!");
			}
			MakeSureMethodIsRegistered(xDllName, xDllFileName, xMethodName);
			for (int i = MethodInfo.Arguments.Length - 1; i >= 0; i--) {
				Op.Ldarg(Assembler, MethodInfo.Arguments[i]);
				if (TheMethod.Parameters[i].ParameterType.FullName == "System.String") {
					string xGetStorageMethodName = "GetStorage";
					new Call(Engine.GetMethodDefinition(Engine.GetTypeDefinition("", "Indy.IL2CPU.CustomImplementation.System.StringImpl"), xGetStorageMethodName, "System.UInt32")) {
						Assembler = Assembler
					}.Assemble();
				}
			}
			new CPUx86.Call("[" + xMethodName + "]");
			foreach(object xItem in MethodInfo.Arguments) {
				Assembler.StackSizes.Pop();
			}
			if (MethodInfo.ReturnSize > 0) {
				new CPUx86.Pushd("eax");
				Assembler.StackSizes.Push(MethodInfo.ReturnSize);
			}
		}
	}
}
