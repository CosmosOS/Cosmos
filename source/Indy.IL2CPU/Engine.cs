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

		protected OpCodeMap mMap = new OpCodeMap();

		public void Execute(string aAssembly, string opAssembly) {
			mMap.LoadOpMapFromAssembly(opAssembly);
            AssemblyDefinition xAD = AssemblyFactory.GetAssembly(aAssembly);
            if (xAD.EntryPoint == null) {
                throw new NotSupportedException("Libraries are not yet supported!");
            }
			foreach (Instruction xInstruction in xAD.EntryPoint.Body.Instructions) {
                mMap.GetOpForOpCode(xInstruction.OpCode.Code).Assemble(xInstruction);
			}
		}
	}
}
