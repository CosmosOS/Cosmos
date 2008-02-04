using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.IL {
	public abstract class CustomMethodImplementationProxyOp: Op {
		public readonly MethodInformation MethodInfo;
		public CustomMethodImplementationProxyOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			MethodInfo = aMethodInfo;
		}

		public MethodBase ProxiedMethod;

		protected abstract void Ldarg(int aIndex);
		protected abstract void Ldflda(TypeInformation aType, TypeInformation.Field aField);
		protected abstract void CallProxiedMethod();
		protected abstract void Ldloc(int index);

		public sealed override void DoAssemble() {
			bool isFirst = true;
			int curIndex = 0;
			ParameterInfo[] xParams = ProxiedMethod.GetParameters();
			foreach (var xParam in xParams) {
				if (isFirst && (!ProxiedMethod.IsStatic)) {
					isFirst = false;
					Ldarg(curIndex++);
				} else {
					FieldAccessAttribute xFieldAccess = (FieldAccessAttribute)xParam.GetCustomAttributes(typeof(FieldAccessAttribute), true).FirstOrDefault();
					if (xFieldAccess != null) {
						Ldarg(0);
						Ldflda(MethodInfo.TypeInfo, MethodInfo.TypeInfo.Fields[xFieldAccess.Name]);
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
