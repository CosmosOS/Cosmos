using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class CustomMethodImplementationProxyOp: Op {
		public readonly MethodInformation MethodInfo;
		public CustomMethodImplementationProxyOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodInfo = aMethodInfo;
		}

		public MethodDefinition ProxiedMethod;

		protected abstract void Ldarg(int index);
		protected abstract void Ldflda(TypeInformation.Field aField);
		protected abstract void CallProxiedMethod();
		protected abstract void Ldloc(int index);

		public sealed override void DoAssemble() {
			bool isFirst = true;
			int curIndex = 0;
			foreach (ParameterDefinition xParam in ProxiedMethod.Parameters) {
				if (isFirst && (!ProxiedMethod.IsStatic)) {
					isFirst = false;
					Ldarg(curIndex++);
				} else {
					string xFieldName = null;
					foreach (CustomAttribute xAttrib in xParam.CustomAttributes) {
						if (xAttrib.Constructor.DeclaringType.FullName == "Indy.IL2CPU.FieldAccessAttribute") {
							xFieldName = (string)xAttrib.Fields["Name"];
							break;
						}
					}
					if (xFieldName != null) {
						Ldarg(0);
						Ldflda(MethodInfo.TypeInfo.Fields[xFieldName]);
					} else {
						Ldarg(curIndex++);
					}
				}
			}
			CallProxiedMethod();
			DoQueueMethod(ProxiedMethod);
		}
	}
}
