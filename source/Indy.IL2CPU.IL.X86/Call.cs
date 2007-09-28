using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Call)]
	public class Call: Op {
		private string LabelName;
		private bool HasResult;
		private int? TotalArgumentSize = null;
		public Call(MethodReference aMethod)
			: base(null, null) {
			if (aMethod == null) {
				throw new ArgumentNullException("aMethod");
			}
			Initialize(aMethod);
		}

		private void Initialize(MethodReference aMethod) {
			HasResult = !aMethod.ReturnType.ReturnType.FullName.Contains("System.Void");
			MethodDefinition xMethodDef = Engine.GetDefinitionFromMethodReference(aMethod);
			LabelName = new Asm.Label(xMethodDef).Name;
			Engine.QueueMethodRef(xMethodDef);
			bool needsCleanup = false;
			foreach (ParameterDefinition xParam in xMethodDef.Parameters) {
				if (xParam.IsOut) {
					needsCleanup = true;
					break;
				}
			}
			if (needsCleanup) {
				TotalArgumentSize = xMethodDef.Parameters.Count * 4;
				if (!xMethodDef.IsStatic) {
					TotalArgumentSize += 4;
				}
			}
			// todo: add support for other argument sizes
		}

		public Call(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodReference xMethod = ((MethodReference)aInstruction.Operand);
			Initialize(xMethod);
		}
		public void Assemble(string aMethod) {
			Call(aMethod);
			if (HasResult) {
				Push(Assembler, "eax");
			}
		}

		public override void DoAssemble() {
			Assemble(LabelName);
		}
	}
}