using System;

namespace Cosmos.Compiler.IL.X86
{
	[OpCode(OpCodeEnum.Newarr)]
	public class Newarr: Op
	{



		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Newarr)]
		// 	public class Newarr: Op {
		// 		private uint mElementSize;
		// 		private string mCtorName;
		//         private Type mType;
		//         private string mBaseLabel;
		//         private string mNextLabel;
		//         private uint mCurOffset;
		//         private MethodInformation mCurrentMethodInfo;
		//         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aServiceProvider)
		//         {
		//             Type xTypeRef = aReader.OperandValueType;
		//             if (xTypeRef == null)
		//             {
		//                 throw new Exception("No TypeRef found!");
		//             }
		//             aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xTypeRef);
		//             Type xArrayType = typeof (Array);
		//             aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xTypeRef);
		//             MethodBase xCtor = xArrayType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)[0];
		//             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(xCtor, false);
		//         }
		// 
		//         public Newarr(Type aTypeRef, uint aCurOffset, MethodInformation aCurrentMethodInfo, string aNextLabel)
		//             : base(null, null)
		//         {
		//             mType = aTypeRef;
		//             mBaseLabel = GetInstructionLabel(aCurOffset);
		//             mCurrentMethodInfo = aCurrentMethodInfo;
		//             mCurOffset = aCurOffset;
		//             mNextLabel = aNextLabel;
		//         }
		// 
		// 		public Newarr(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			mType = aReader.OperandValueType;
		// 			if (mType == null) {
		// 				throw new Exception("No TypeRef found!");
		// 			}
		//             mBaseLabel = GetInstructionLabel(aReader);
		//             mCurOffset = aReader.Position;
		//             mNextLabel = GetInstructionLabel(aReader.NextPosition);
		//             mCurrentMethodInfo = aMethodInfo;
		// 		}
		// 
		// 		private void Initialize(Type aTypeRef, string aBaseLabelName) {
		// 			Type xTypeDef = aTypeRef;
		// 			mElementSize = GetService<IMetaDataInfoService>().GetFieldStorageSize(aTypeRef);
		// 			Type xArrayType = ReflectionUtilities.GetType("mscorlib", "System.Array");
		// 			MethodBase xCtor = xArrayType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)[0];
		// 		    mCtorName = GetService<IMetaDataInfoService>().GetMethodInfo(xCtor, false).LabelName;
		// 		}
		// 
		// 		public override void DoAssemble() {
		//             Initialize(mType, mBaseLabel);
		// 
		// 			new CPU.Comment("Element Size = " + mElementSize);
		// 			// element count is on the stack
		// 			int xElementCountSize = Assembler.StackContents.Pop().Size;
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.ESI};
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.ESI };
		// 			//Assembler.StackSizes.Push(xElementCountSize);
		//             new CPUx86.Push { DestinationValue = mElementSize };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		//             Multiply(Assembler, GetServiceProvider(),
		//                 mBaseLabel, mCurrentMethodInfo, (uint)mCurOffset, mNextLabel);
		// 			// the total items size is now on the stack
		//             new CPUx86.Push { DestinationValue = (ObjectImpl.FieldDataOffset + 4) };
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		//             Add(Assembler, GetServiceProvider(), mBaseLabel, mCurrentMethodInfo, mCurOffset, mNextLabel);
		// 			// the total array size is now on the stack.
		// 		    new CPUx86.Call { DestinationLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef) };
		// 			new CPUx86.Push{DestinationReg=CPUx86.Registers.ESP, DestinationIsIndirect=true};
		//             new CPUx86.Push{DestinationReg=CPUx86.Registers.ESP, DestinationIsIndirect=true};
		//             new CPUx86.Push{DestinationReg=CPUx86.Registers.ESP, DestinationIsIndirect=true};
		//             new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
		// 			//new CPUx86.Pushd(CPUx86.Registers_Old.EDI);
		//             var xIncRef = GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.IncRefCountRef, false).LabelName;
		//             new CPUx86.Call { DestinationLabel = xIncRef };
		//             new CPUx86.Call { DestinationLabel = xIncRef };
		// 			//new CPUx86.Pop(CPUx86.Registers_Old.ESI);
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(Array)));
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceRef = ElementReference.New(GetService<IMetaDataInfoService>().GetTypeIdLabel(typeof(Array))), SourceIsIndirect = true };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg=CPUx86.Registers.EBX};
		//             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceValue = (uint)InstanceTypeEnum.Array, Size = 32 };
		//             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.ESI, Size = 32 };
		//             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
		//             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceValue = (uint)mElementSize, Size = 32 };
		//             new CPUx86.Call { DestinationLabel = mCtorName };
		// 		}
		// 	}
		// }
		#endregion
	}
}
