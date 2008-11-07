using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86CustomMethodImplementationProxyOp: CustomMethodImplementationProxyOp {
		private readonly bool mDebugMode;
		public X86CustomMethodImplementationProxyOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			mDebugMode = aMethodInfo.DebugMode;
		}

		protected override void Ldarg(int aIndex) {
			Op.Ldarg(Assembler, MethodInfo.Arguments[aIndex]);
		}

		protected override void Ldflda(TypeInformation aType, TypeInformation.Field aField) {
			Op.Ldflda(Assembler, aType, aField, false);
		}

		protected override void CallProxiedMethod() {
			Op x = new Call(ProxiedMethod, 0, mDebugMode, (uint)MethodInfo.ExtraStackSize, ".AfterProxyCall");
			x.Assembler = Assembler;
			x.Assemble();
		    new Label(".AfterProxyCall");
		}

		protected override void Ldloc(int index) {
			Op x = new Ldloc(MethodInfo, index);
			x.Assembler = Assembler;
			x.Assemble();
		}
	}
}
