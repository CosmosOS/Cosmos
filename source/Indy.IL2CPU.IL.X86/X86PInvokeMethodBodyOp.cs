using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;

using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	public class X86PInvokeMethodBodyOp: PInvokeMethodBodyOp {
		public X86PInvokeMethodBodyOp(MethodBase aTheMethod, MethodInformation aMethodInfo)
			: base(aTheMethod, aMethodInfo) {
		}

		private void MakeSureMethodIsRegistered(string aDllName, string aDllFileName, string aMethodName) {
			ImportMember xTheMember = null;
			foreach (ImportMember xMember in (from item in Assembler.ImportMembers
												  select item.Value)) {
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
				Assembler.ImportMembers.Add(new KeyValuePair<string, ImportMember>(Assembler.CurrentGroup, xTheMember));
			}
			xTheMember.Methods.Add(new ImportMethodMember(aMethodName));
		}

		public override void DoAssemble() {
			throw new Exception("PInvoke not supported at the moment");
			//string xDllFileName = TheMethod.PInvokeInfo.Module.Name;
			//string xDllName = xDllFileName.Replace('.', '_');
			//string xMethodName = TheMethod.PInvokeInfo.EntryPoint;
			//if (String.IsNullOrEmpty(xMethodName)) {
			//    xMethodName = TheMethod.Name;
			//}
			//new Comment("PInvokeAttributes = '" + TheMethod.PInvokeInfo.Attributes.ToString("G") + "'");
			//string xStringMethodSuffix = "W";
			//if (!TheMethod.PInvokeInfo.IsNoMangle) {
			//    if (TheMethod.PInvokeInfo.IsCharSetUnicode) {
			//        xMethodName += xStringMethodSuffix;
			//    } else {
			//        if (TheMethod.PInvokeInfo.IsCharSetAnsi) {
			//            xMethodName += xStringMethodSuffix;
			//        } else {
			//            if (TheMethod.PInvokeInfo.IsCharSetAuto) {
			//                xMethodName += xStringMethodSuffix;
			//            }
			//        }
			//    }
			//}
			////			}
			//if (String.IsNullOrEmpty(xDllName)) {
			//    throw new Exception("Unable to determine what dll to use!");
			//}
			//MakeSureMethodIsRegistered(xDllName, xDllFileName, xMethodName);
			//for (int i = MethodInfo.Arguments.Length - 1; i >= 0; i--) {
			//    Op.Ldarg(Assembler, MethodInfo.Arguments[i]);
			//    if (MethodInfo.Arguments[i].ArgumentType.FullName == "System.String") {
			//        string xGetStorageMethodName = "GetStorage";
			//        new Call(Engine.GetMethodBase(Engine.GetType("", "Indy.IL2CPU.CustomImplementation.System.StringImpl"), xGetStorageMethodName, "System.UInt32")) {
			//            Assembler = Assembler
			//        }.Assemble();
			//    }
			//}
			//new CPUx86.Call("[" + xMethodName + "]");
			//foreach(object xItem in MethodInfo.Arguments) {
			//    Assembler.StackSizes.Pop();
			//}
			//if (MethodInfo.ReturnSize > 0) {
			//    new CPUx86.Pushd(CPUx86.Registers.EAX);
			//    Assembler.StackSizes.Push(MethodInfo.ReturnSize);
			//}
		}
	}
}
