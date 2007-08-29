using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU {
	public class Engine {
		private OpCodeMap mMap = new OpCodeMap();
		public void Execute(string assembly) {
			AssemblyDefinition xAD = AssemblyFactory.GetAssembly(assembly);
			if (xAD.EntryPoint == null)
				throw new NotSupportedException("Libraries are not yet supported!");
			foreach (Instruction xInstruction in xAD.EntryPoint.Body.Instructions) {
				mMap.GetOpForOpCode(xInstruction.OpCode.Code).Process(xInstruction);
			}
		}
	}
}
