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
			if (aMethodInfo == null) {
				throw new ArgumentNullException("aMethodInfo");
			}
			if (aMethodInfo.MethodDefinition == null)
				return;
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
				new CPU.Label(xWrapperName);
				new CPU.Comment(mInterruptValue.ToString());
				new CPUNative.Pushad();
				new CPUNative.Cli();
				new CPUx86.Call(LabelName);
				new CPUNative.Popad();
				new CPUNative.IRet();
			}
			// TODO: add support for variables with a diff datasize, other than 32bit
			AssembleHeader(Assembler, LabelName, Locals, Args);
		}
	}
}