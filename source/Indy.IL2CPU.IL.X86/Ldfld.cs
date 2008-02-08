using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldfld)]
	public class Ldfld: Op {
		private readonly TypeInformation.Field mField;
		private readonly TypeInformation mType;
		public Ldfld(TypeInformation.Field aField)
			: base(null, null) {
			mField = aField;
		}
		public Ldfld(ILReader aReader, MethodInformation aMethodInfo)
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
			Ldfld(Assembler, mType, mField);
		}
	}
}