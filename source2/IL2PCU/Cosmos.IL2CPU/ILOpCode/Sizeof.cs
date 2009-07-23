using System;

namespace Cosmos.IL2CPU.ILOpCodes
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Sizeof)]
	public class Sizeof: ILOpCode
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Sizeof)]
		// 	public class Sizeof: ILOpCode {
		//         private Type mType;
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    Type xTypeRef = aReader.OperandValueType;
		//         //    if (xTypeRef == null)
		//         //    {
		//         //        throw new Exception("Type not found!");
		//         //    }
		//         //    Engine.RegisterType(xTypeRef);
		//         //}
		// 
		// 		public Sizeof(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mType = aReader.OperandValueType;
		// 			if (mType == null) {
		// 				throw new Exception("Type not found!");}
		// 		}
		// 		public override void DoAssemble() {
		//             uint xSize;
		//             GetService<IMetaDataInfoService>().GetTypeFieldInfo(mType, out xSize);
		//             new CPU.Push { DestinationValue =xSize};
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(int)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
