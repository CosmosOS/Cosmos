using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU {
	public class Engine {
		protected OpCodeMap mMap = new OpCodeMap();

		public void Execute(string aAssembly, string opAssembly, BinaryWriter aOutput) {
			if (aOutput == null) {
				throw new ArgumentNullException("aOutput");
			}
			mMap.LoadOpMapFromAssembly(opAssembly);
			AssemblyDefinition xAD = AssemblyFactory.GetAssembly(aAssembly);
			if (xAD.EntryPoint == null) {
				throw new NotSupportedException("Libraries are not yet supported!");
			}
			foreach (Instruction xInstruction in xAD.EntryPoint.Body.Instructions) {
				mMap.GetOpForOpCode(xInstruction.OpCode.Code).Assemble(xInstruction, aOutput);
			}
		}
	}
}