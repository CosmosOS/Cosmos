using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class Op {
		public delegate void QueueMethodHandler(string aAssembly, string aType, string aMethod);
		public abstract void Assemble(Instruction aInstruction);

		protected void DoQueueMethod(string aAssembly, string aType, string aMethod) {
			if (QueueMethod==null) {
				throw new Exception("IL Op wants to queue a method, but no QueueMethod handler supplied!");
			}
			QueueMethod(aAssembly, aType, aMethod);
		}

		public static QueueMethodHandler QueueMethod;
	}
}
