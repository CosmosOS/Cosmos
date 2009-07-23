using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldsfld)]
	public class Ldsfld: ILOpProfiler
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using Indy.IL2CPU.Assembler;
		// 
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Ldsfld)]
		// 	public class Ldsfld: ILOpProfiler {
		// 		private string mDataName;
		// 		private bool mNeedsGC;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 
		//         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
		//         //    FieldInfo xField = aReader.OperandValueField;
		//         //    Engine.QueueStaticField(xField);
		//         //}
		// 
		// 		public Ldsfld(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mField = aReader.OperandValueField;
		//             // todo: improve, strings need gc?
		// 			mNeedsGC = !mField.FieldType.IsValueType;
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		//         private FieldInfo mField;
		// 		public override void DoAssemble() {
		//             var xSize = GetService<IMetaDataInfoService>().GetFieldStorageSize(mField.FieldType);
		//             mDataName = GetService<IMetaDataInfoService>().GetStaticFieldLabel(mField);
		// 		    if (xSize >= 4) {
		// 				for (int i = 1; i <= (xSize / 4); i++) {
		// 					//	Pop("eax");
		// 					//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
		//                     new CPUx86.Push { DestinationRef = ElementReference.New(mDataName), DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize - (i * 4)) };
		// 				}
		// 				switch (xSize % 4) {
		// 					case 1: {
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
		//                             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 							break;
		// 						}
		// 					case 2: {
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
		// 							new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
		// 							break;
		// 						}
		// 					case 0: {
		// 							break;
		// 						}
		// 					default:
		//                         EmitNotImplementedException(Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + (xSize % 4) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                         break;
		// 				}
		// 			} else {
		// 				switch (xSize) {
		// 					case 1: {
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
		//                             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 							break;
		// 						}
		// 					case 2: {
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
		//                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
		// 							new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
		// 							break;
		// 						}
		// 					case 0: {
		// 							break;
		// 						}
		// 					default:
		//                         EmitNotImplementedException(Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + (xSize % 4) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                         break;
		// 				}
		// 			}
		// 			Assembler.StackContents.Push(new StackContent((int)xSize, null));
		// 			if (mNeedsGC) {
		// 				new Dup(null, null) {
		// 					Assembler = this.Assembler
		// 				}.Assemble();
		// 				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
		// 				Assembler.StackContents.Pop();
		// 			}
		// 		}
		// 	}
		// }
		#endregion
	}
}
