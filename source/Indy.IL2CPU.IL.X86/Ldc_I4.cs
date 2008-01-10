using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;
using System.Collections.Generic;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldc_I4)]
	public class Ldc_I4: Op {
		private int mValue;
		private bool mOptimizedArrayFieldCode = false;
		private FieldDefinition mTokenField;
		private FieldDefinition mStaticField;
		private int mCount;
		private int mElementSize;
		protected void SetValue(int aValue) {
			mValue = aValue;
		}

		protected void SetValue(string aValue) {
			SetValue(Int32.Parse(aValue));
		}

		public Ldc_I4(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aInstruction != null && aInstruction.Operand != null) {
				SetValue(aInstruction.Operand.ToString());
			}
		}

		public int Value {
			get {
				return mValue;
			}
		}
		public override sealed void DoAssemble() {
			new CPU.Pushd("0" + mValue.ToString("X") + "h");
			Assembler.StackSizes.Push(4);
		}
	}
}