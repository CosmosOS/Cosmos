using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsfld)]
	public class Ldsfld: Op {
		private bool IsIntPtrZero = false;
		private string mDataName;
		private bool mIsValueType;

		public Ldsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			FieldReference xField = (FieldReference)aInstruction.Operand;
			mIsValueType = xField.FieldType.IsValueType;
			Engine.QueueStaticField(xField, out mDataName);
			if(String.IsNullOrEmpty(mDataName)) {
				throw new Exception("No name generated for field '" + xField.GetFullName() + "'");
			}
			//DoQueueStaticField(xField.DeclaringType.Module.Assembly.Name.FullName, xField.DeclaringType.FullName, xField.Name, out mDataName);
		}
		public override void DoAssemble() {
//			if (mIsValueType) {
//				Pushd("[" + mDataName + "]");
//			}else {
				Pushd(mDataName);
//			}
		}
	}
}