using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldtoken)]
	public class Ldtoken: ILOp
	{
		public Ldtoken(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      //TODO: Implement this Op
    }

    
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// using Indy.IL2CPU.Assembler;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldtoken)]
		// 	public class Ldtoken: Op {
		// 		private string mTokenAddress;
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    FieldInfo xFieldDef = aReader.OperandValueField;
		//         //    if (xFieldDef != null)
		//         //    {
		//         //        if (!xFieldDef.IsStatic)
		//         //        {
		//         //            throw new Exception("Nonstatic field-backed tokens not supported yet!");
		//         //        }
		//         //        Engine.QueueStaticField(xFieldDef);
		//         //        return;
		//         //    }
		//         //    Type xTypeRef = aReader.OperandValueType;
		//         //    if (xTypeRef != null)
		//         //    {
		//         //        return;
		//         //    }
		//         //    throw new Exception("Token type not supported yet!");
		//         //}
		// 
		// 		public Ldtoken(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			// todo: add support for type tokens and method tokens
		// 			FieldInfo xFieldDef = aReader.OperandValueField;
		// 			if (xFieldDef != null) {
		// 				if (!xFieldDef.IsStatic) {
		// 					throw new Exception("Nonstatic field-backed tokens not supported yet!");
		// 				}
		// 				mTokenAddress = DataMember.GetStaticFieldName(xFieldDef);
		// 				return;
		// 			}
		// 			mType= aReader.OperandValueType;
		//             if (mType != null)
		//             {
		//                 return;
		//             }
		// 			throw new Exception("Token type not supported yet!");
		// 		}
		// 
		//         private Type mType;
		// 
		// 		public override void DoAssemble() {
		//             if (mType != null)
		//             {
		//                 mTokenAddress = GetService<IMetaDataInfoService>().GetTypeIdLabel(mType);
		//             }
		//             new CPU.Push { DestinationRef = ElementReference.New(mTokenAddress) };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		// 		}
		// 	}
		// }
		
	}
}
