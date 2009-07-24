using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldftn)]
	public class Ldftn: ILOpX86
	{
		public Ldftn(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.Linq;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldftn)]
		// 	public class Ldftn: Op {
		// 		private string mFunctionLabel;
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    MethodBase xMethodRef = aReader.OperandValueMethod;
		//         //    if (xMethodRef == null)
		//         //    {
		//         //        throw new Exception("Unable to determine Method!");
		//         //    }
		//         //    Engine.QueueMethod(xMethodRef);
		//         //}
		// 
		// 		public Ldftn(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			MethodBase xMethodRef = aReader.OperandValueMethod;
		// 			if (xMethodRef == null) {
		// 				throw new Exception("Unable to determine Method!");
		// 			}
		// 			mFunctionLabel = MethodInfoLabelGenerator.GenerateLabelName(xMethodRef);
		// 		}
		// 
		// 		public override void DoAssemble() {
		//             new CPUx86.Push { DestinationRef = ElementReference.New(mFunctionLabel) };
		// 			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
