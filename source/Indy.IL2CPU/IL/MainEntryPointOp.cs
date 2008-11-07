using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.IL {
	public abstract class MainEntryPointOp: Op {
		protected MainEntryPointOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		public abstract void Enter(string aName);
		public abstract void Exit();
		public abstract void Push(uint aValue);
		public abstract void Call(MethodBase aMethod);
		public abstract void Call(string aLabelName);

		public override void DoAssemble() {
			throw new NotImplementedException();
		}
	}
}
