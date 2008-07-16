using System;
using System.Collections;
using System.Collections.Generic;		 
using System.Linq;


using CPU = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stfld)]
	public class Stfld: Op {
		private readonly TypeInformation.Field mField;
		private readonly TypeInformation mType;
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            FieldInfo xField = aReader.OperandValueField;
            if (xField == null)
            {
                throw new Exception("Field not found!");
            }
            Engine.RegisterType(xField.FieldType);
        }
		public Stfld(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			if (aReader == null) {
				throw new ArgumentNullException("aReader");
			}
			if (aMethodInfo == null) {
				throw new ArgumentNullException("aMethodInfo");
			}
			FieldInfo xField = aReader.OperandValueField;
			if (xField == null) {
				throw new Exception("Field not found!");
			}
			string xFieldId = xField.GetFullName();
			mType = Engine.GetTypeInfo(xField.DeclaringType);
			mField = mType.Fields[xFieldId];
		}

		public override void DoAssemble() {
			Stfld(Assembler, mType, mField);
		}
	}
}