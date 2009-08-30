using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldflda)]
	public class Ldflda: ILOp
	{
		public Ldflda(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      throw new NotImplementedException();
    }

    
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// 
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;	    
		// using System.Reflection;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldflda)]
		// 	public class Ldflda: Op {
		//         private Type mType;
		// 		private TypeInformation mTypeInfo;
		// 		private TypeInformation.Field mField;
		//         private string mFieldId;
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
		//         //{
		//         //    FieldInfo xField = aReader.OperandValueField;
		//         //    if (xField == null)
		//         //    {
		//         //        throw new Exception("Field not found!");
		//         //    }
		//         //    Engine.RegisterType(xField.DeclaringType);
		//         //    Engine.RegisterType(xField.FieldType);
		//         //}
		// 
		// 		public Ldflda(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			FieldInfo xField = aReader.OperandValueField;
		// 			if (xField == null) {
		// 					throw new Exception("Field not found!");
		// 			}
		// 			mFieldId = xField.GetFullName();
		//             mType = xField.DeclaringType;
		// 		}
		// 
		// 		public override void DoAssemble() {
		//             mTypeInfo = GetService<IMetaDataInfoService>().GetTypeInfo(mType);
		//             mField = mTypeInfo.Fields[mFieldId];
		// 			Ldflda(Assembler, mTypeInfo, mField);
		// 		}
		// 	}
		// }
		
	}
}
