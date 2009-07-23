using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Localloc)]
	public class Localloc: ILOpX86
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// using Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Compiler;
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Localloc)]
		// 	public class Localloc: ILOpX86 {
		//         public const string LocAllocCountMethodDataEntry = "LocAllocCount";
		//         public const string LocAllicItemMethodDataEntryTemplate = "LocAllocItem_L{0}";
		// 
		//         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aServiceProvider) {
		//             // xCurrentMethodLocallocCount contains the number of LocAlloc occurrences
		//             int xCurrentMethodLocallocCount = 0;
		//             if (aMethodData.ContainsKey(LocAllocCountMethodDataEntry))
		//             {
		//                 xCurrentMethodLocallocCount = (int)aMethodData[LocAllocCountMethodDataEntry];
		//             }
		//             xCurrentMethodLocallocCount++;
		//             aMethodData[LocAllocCountMethodDataEntry] = xCurrentMethodLocallocCount;
		//             string xCurrentItem = String.Format(LocAllicItemMethodDataEntryTemplate,
		//                                                 aReader.Position);
		// #if DEBUG
		//             if (aMethodData.ContainsKey(xCurrentItem))
		//             {
		//                 throw new Exception("Localloc item already exists in MethodData!");
		//             }
		// #endif
		//             aMethodData.Add(xCurrentItem, xCurrentMethodLocallocCount);
		//         }
		// 
		// 	    private readonly int mLocallocOffset = 0;
		// 		public Localloc(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 		    mLocallocOffset = (int)aMethodInfo.MethodData[String.Format(LocAllicItemMethodDataEntryTemplate,
		// 		                                                                aReader.Position)];
		// 		    mLocallocOffset *= 4;
		// 		    mLocallocOffset += aMethodInfo.LocalsSize;
		// 
		// 		}
		//         public override void DoAssemble() {
		//             var xId = GetService<IMetaDataInfoService>().GetMethodInfo(RuntimeEngineRefs.Heap_AllocNewObjectRef, false);
		//             new CPUx86.Call { DestinationLabel = xId.LabelName };
		//             new CPUx86.Move {
		//                 DestinationReg = CPUx86.Registers.EBP,
		//                 DestinationIsIndirect = true,
		//                 DestinationDisplacement = mLocallocOffset,
		//                 SourceReg = Registers.ESP,
		//                 SourceIsIndirect = true
		//             };
		//         }
		// 	}
		// }
		#endregion
	}
}
