using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Indy.IL2CPU.Assembler;
using Cil = Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public abstract class Op {
		private readonly string mCurrentInstructionLabel;
		private readonly string mILComment;
		public static string GetInstructionLabel(Cil.Instruction aInstruction) {
			return ".L" + aInstruction.Offset.ToString("X8");
		}

		public delegate void QueueMethodHandler(string aAssembly, string aType, string aMethod);

		public void Assemble() {
			if (!String.IsNullOrEmpty(mCurrentInstructionLabel)) {
				Assembler.Add(new Label(mCurrentInstructionLabel));
			}
			if (!String.IsNullOrEmpty(mILComment)) {
				Assembler.Add(new Literal(mILComment));
			}
			DoAssemble();
		}

		public abstract void DoAssemble();

		public Op(Cil.Instruction aInstruction, MethodInformation aMethodInfo) {
			if (aInstruction != null) {
				mCurrentInstructionLabel = GetInstructionLabel(aInstruction);
				mILComment = "; IL: " + aInstruction.OpCode.Code + " " + aInstruction.Operand;
			}
		}

		// This is a prop and not a constructor arg for two reasons. Ok, mostly for one
		// 1 - Adding a parameterized triggers annoying C# constructor hell, every descendant we'd have to reimplement it
		// 2 - This helps to allow changing of assembler while its in use. Currently no idea why we would ever want to do that
		// rather than construct a new one though....
		// If we end up with more things we need, probably will change to Initialize(x, y), or someone can go thorugh and add
		// all the friggin constructors
		protected Assembler.Assembler mAssembler;
		public Assembler.Assembler Assembler {
			get {
				return mAssembler;
			}
			set {
				mAssembler = value;
			}
		}

		protected void DoQueueMethod(string aAssembly, string aType, string aMethod) {
			if (QueueMethod == null) {
				throw new Exception("IL Op wants to queue a method, but no QueueMethod handler supplied!");
			}
			QueueMethod(aAssembly, aType, aMethod);
		}

		public static QueueMethodHandler QueueMethod;
	}
}
