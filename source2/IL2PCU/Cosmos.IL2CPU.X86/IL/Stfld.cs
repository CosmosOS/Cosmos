using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Stfld)]
	public class Stfld: ILOp
	{
		public Stfld(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      //TODO: Implement this Op
    }

    
		// using System;
		// using System.Collections;
		// using System.Collections.Generic;		 
		// using System.Linq;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Stfld)]
		// 	public class Stfld: Op {
		// 		private TypeInformation.Field mField;
		// 		private TypeInformation mType;
		//         private Type mDeclaringType;
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    FieldInfo xField = aReader.OperandValueField;
		//         //    if (xField == null)
		//         //    {
		//         //        throw new Exception("Field not found!");
		//         //    }
		//         //    Engine.RegisterType(xField.FieldType);
		//         //}
		// 
		// 		public Stfld(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			if (aReader == null) {
		// 				throw new ArgumentNullException("aReader");
		// 			}
		// 			if (aMethodInfo == null) {
		// 				throw new ArgumentNullException("aMethodInfo");
		// 			}
		// 			FieldInfo xField = aReader.OperandValueField;
		// 			if (xField == null) {
		// 				throw new Exception("Field not found!");
		// 			}
		// 			mFieldId = xField.GetFullName();
		//             mDeclaringType = xField.DeclaringType;
		// 			
		// 		}
		// 
		//         private string mFieldId;
		// 
		// 		public override void DoAssemble() {
		//             mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
		//             if (!mType.Fields.ContainsKey(mFieldId))
		//             {
		//                 Console.Write("");
		//                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
		//                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
		//                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
		//                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
		//                 throw new Exception("Field not found!");
		//             }
		//             mField = mType.Fields[mFieldId];
		// 			Stfld(Assembler, mType, mField);
		// 		}
		// 	}
		// }
		
	}
}
