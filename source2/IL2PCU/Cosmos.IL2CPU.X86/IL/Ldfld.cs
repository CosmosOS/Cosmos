using System;
// using System.Collections.Generic;
// using System.IO;
// 
// using CPU = Indy.IL2CPU.Assembler;
// using System.Reflection;
// using Indy.IL2CPU.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldfld)]
	public class Ldfld: ILOp
	{
		public Ldfld(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xType = aMethod.MethodBase.DeclaringType;
      var xOpCode = (ILOpCodes.OpField)aOpCode;
      
      //             if (!xType.Fields.ContainsKey(mFieldId))
      //             {
      //                 Console.Write("");
      //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
      //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
      //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
      //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
      //             }
      //             mFieldInfo = xType.Fields[mFieldId];
      // 			Ldfld(Assembler, xType, mFieldId);
      throw new NotImplementedException();
    }

		// 	public class Ldfld: Op {
		//         private Type mDeclaringType;
		// 		private TypeInformation.Field mFieldInfo;
		// 		private readonly TypeInformation mTypeInfo;
		//         private string mFieldId;
		// 
		// 		public Ldfld(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			FieldInfo xField = aReader.OperandValueField;
		// 			if (xField == null) {
		// 					throw new Exception("Field not found!");
		// 			}
		// 			mFieldId = xField.GetFullName();
		//             mDeclaringType = xField.DeclaringType;
		// 		}
		
	}
}
