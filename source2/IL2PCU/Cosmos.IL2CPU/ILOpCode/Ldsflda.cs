using System;

namespace Cosmos.IL2CPU.ILOpCodes
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldsflda)]
	public class Ldsflda: ILOpCode
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using Indy.IL2CPU.Compiler;
		// using CPU = Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldsflda)]
		// 	public class Ldsflda: ILOpCode {
		// 		private string mDataName;
		// 	    private FieldInfo mField;
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    FieldInfo xField = aReader.OperandValueField;
		//         //    Engine.QueueStaticField(xField);
		//         //}
		// 
		// 		public Ldsflda(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mField = aReader.OperandValueField;
		//             
		// 		}
		// 
		// 		public override void DoAssemble() {
		// 		    mDataName = GetService<IMetaDataInfoService>().GetStaticFieldLabel(mField);
		//             new CPU.Push { DestinationRef = ElementReference.New(mDataName) };
		// 			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		// 		}
		// 	}
		// }
		#endregion
	}
}
