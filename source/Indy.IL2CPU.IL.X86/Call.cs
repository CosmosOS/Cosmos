using System;
using System.Collections.Generic;
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
		private bool mIsDebugger_Break = false;
		private int[] ArgumentSizes = new int[0];
		public Call(MethodReference aMethod)
			: base(null, null) {
			if (aMethod == null) {
				throw new ArgumentNullException("aMethod");
			}
			Initialize(aMethod);
		}

		private void Initialize(MethodReference aMethod) {
			mIsDebugger_Break = aMethod.GetFullName() == "System.Void System.Diagnostics.Debugger.Break()";
			HasResult = !aMethod.ReturnType.ReturnType.FullName.Contains("System.Void");
			int xResultSize = Engine.GetFieldStorageSize(aMethod.ReturnType.ReturnType);
			if (xResultSize > 4 && HasResult) {
				throw new Exception("ReturnValues of sizes larger than 4 bytes not supported yet (" + xResultSize + ")");
			}
			MethodDefinition xMethodDef = Engine.GetDefinitionFromMethodReference(aMethod);
			LabelName = new Asm.Label(xMethodDef).Name;
			Engine.QueueMethodRef(xMethodDef);
			bool needsCleanup = false;
			List<int> xArgumentSizes = new List<int>();
			foreach (ParameterDefinition xParam in xMethodDef.Parameters) {
				xArgumentSizes.Add(Engine.GetFieldStorageSize(xParam.ParameterType));
			}
			if (!xMethodDef.IsStatic) {
				xArgumentSizes.Insert(0, 4);
			}
			ArgumentSizes = xArgumentSizes.ToArray();
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
		public void Assemble(string aMethod, int aArgumentCount) {
			Call(aMethod);
			for (int i = 0; i < aArgumentCount; i++) {
				Assembler.StackSizes.Pop();
			}
			if (HasResult) {
				Push(Assembler, 4, "eax");
			}
		}

		public override void DoAssemble() {
			if (mIsDebugger_Break) {
				mAssembler.Add(new Asm.Literal("xchg bx, bx"));
			} else {
				Assemble(LabelName, ArgumentSizes.Length);
			}
		}
	}
}