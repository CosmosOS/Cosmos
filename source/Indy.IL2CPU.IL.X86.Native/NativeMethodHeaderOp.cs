using System;
using System.Linq;
using Indy.IL2CPU.IL.X86;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeMethodHeaderOp: X86MethodHeaderOp {
		public const string ISR_Suffix = "______ISR____WRAPPER";
		private bool mIsInterruptHandler = false;
		private byte mInterruptValue;
		public NativeMethodHeaderOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			foreach (CustomAttribute xAttrib in aMethodInfo.MethodDefinition.CustomAttributes) {
				if (xAttrib.Constructor.DeclaringType.FullName == typeof(InterruptServiceRoutineAttribute).FullName) {
					mIsInterruptHandler = true;
					mInterruptValue = (byte)xAttrib.ConstructorParameters[0];
					break;
				}
			}
		}

		public override void DoAssemble() {
			if (mIsInterruptHandler) {
				string xWrapperName = LabelName + ISR_Suffix;
				Assembler.Add(new CPU.Label(xWrapperName));
				Assembler.Add(new CPU.Literal(";" + mInterruptValue));
				Assembler.Add(new CPUNative.Pushad());
				Assembler.Add(new CPUNative.Cli());
				Assembler.Add(new CPUx86.Call(LabelName));
				Assembler.Add(new CPUNative.Popad());
				Assembler.Add(new CPUNative.IRet());
			}
			// TODO: add support for variables with a diff datasize, other than 32bit
			AssembleHeader(Assembler, LabelName, LocalsCount);
		}
	}
}