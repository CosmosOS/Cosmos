using System;
using System.IO;


using CPUx86 = Indy.IL2CPU.Assembler.X86;	    
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldflda)]
	public class Ldflda: Op {
		private TypeInformation mType;
		private TypeInformation.Field mField;

		public Ldflda(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			FieldInfo xField = aReader.OperandValueField;
			if (xField == null) {
					throw new Exception("Field not found!");
			}
			string xFieldId = xField.GetFullName();
			mType = Engine.GetTypeInfo(xField.DeclaringType);
			mField = mType.Fields[xFieldId];
		}

		public override void DoAssemble() {
			Ldflda(Assembler, mType, mField);
		}
	}
}