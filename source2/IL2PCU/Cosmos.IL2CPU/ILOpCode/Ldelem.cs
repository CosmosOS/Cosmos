using System;

namespace Cosmos.IL2CPU.ILOpCodes
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldelem)]
	public class Ldelem: ILOpCode
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.Linq;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldelem)]
		// 	public class Ldelem: ILOpCode {
		//         private Type mType;
		//         
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
		//         //{
		//         //    Type xType = aReader.OperandValueType;
		//         //    if (xType == null)
		//         //        throw new Exception("Unable to determine Type!");
		//         //    Engine.RegisterType(xType);
		//         //}
		// 
		//         public Ldelem(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mType = aReader.OperandValueType;
		// 			if (mType == null)
		// 				throw new Exception("Unable to determine Type!");
		// 			
		// 		}
		// 
		// 		public override void DoAssemble() {
		//             var xElementSize = GetService<IMetaDataInfoService>().GetFieldStorageSize(mType);
		//             new Comment("Element size: " + xElementSize);
		// 			Ldelem_Ref.Assemble(Assembler, xElementSize);
		// 		}
		// 	}
		// }
		#endregion
	}
}
