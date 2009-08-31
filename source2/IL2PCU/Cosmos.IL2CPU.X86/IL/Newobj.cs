using System;
using System.Linq;
using Indy.IL2CPU;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Cosmos.IL2CPU.ILOpCodes;
namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Newobj)]
	public class Newobj : ILOp
	{
		public Newobj(Cosmos.IL2CPU.Assembler aAsmblr)
			: base(aAsmblr)
		{
		}

		public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
		{
			OpMethod xMethod = (OpMethod)aOpCode;
			string xCurrentLabel = GetLabel(aMethod, aOpCode);
			// TODO: enable this again
			// if (DynamicMethodEmit.GetHasDynamicMethod(CtorDef)) {
			//   CtorDef = DynamicMethodEmit.GetDynamicMethod(CtorDef);
			// }

			//var xAllocInfo = GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.AllocNewObjectRef, false);

			//             Assemble(
			//                 Assembler,
			//                 CtorDef,
			//                 GetService<IMetaDataInfoService>().GetTypeIdLabel(CtorDef.DeclaringType),
			//                 CurrentLabel,
			//                 MethodInformation,
			//                 (int)ILOffset,
			//                 mNextLabel,
			//                 GetService<IMetaDataInfoService>().GetTypeInfo(CtorDef.DeclaringType),
			//                 GetService<IMetaDataInfoService>().GetMethodInfo(CtorDef, false),
			//                 GetServiceProvider(),
			//                 xAllocInfo.LabelName
			//             );
			// }
			// 
			//         public static void Assemble(
			//             Assembler.Assembler aAssembler,
			//             MethodBase aCtorDef,
			//             string aTypeId,
			//             string aCurrentLabel,
			//             MethodInformation aCurrentMethodInformation,
			//             int aCurrentILOffset,
			//             string aNextLabel,
			//             TypeInformation aCtorDeclTypeInfo,
			//             MethodInformation aCtorMethodInfo,
			//             IServiceProvider aServiceProvider,
			//             string aAllocMemLabel
			//         )

			//TODO: What is this for?
			//             if (aCtorDef != null) {
			//                 //if (!aCtorDef.DeclaringType.FullName.StartsWith("Indy.IL2CPU.MultiArrayEmit.ContType"))
			//                 //    Engine.QueueMethod(aCtorDef);
			//             } else {
			//                 throw new ArgumentNullException("aCtorDef");
			//             }

			var xType = xMethod.Value.DeclaringType;

			// If not ValueType, then we need gc
			if (!xType.IsValueType)
			{
				var xParams = xMethod.Value.GetParameters();
				for (int i = 1; i < xParams.Length; i++)
				{
					Assembler.Stack.Pop();
				}

				uint xMemSize = GetFieldStorageSize(xType);
				int xExtraSize = 20;
				new CPUx86.Push { DestinationValue = (uint)(xMemSize + xExtraSize) };

				// todo: probably we want to check for exceptions after calling Alloc
				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef) };
				new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
				new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
				new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
				new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
				
				// We IncRef here twice because it's effectively pushed on the stack twice:
				// * once for the .ctor parameter
				// * once for the "returnvalue" of the newobj il op

				// todo: probably we want to check for exceptions after calling IncRef
				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };

				uint xObjSize = 0;
				//int xGCFieldCount = ( from item in aCtorDeclTypeInfo.Fields.Values
				//where item.NeedsGC
				//select item ).Count();

				//int xGCFieldCount = ( from item in aCtorDeclTypeInfo.Fields.Values
				//where item.NeedsGC
				//select item ).Count();
				int xGCFieldCount = xType.GetFields().Count(x => x.FieldType.IsValueType);

				string strTypeId = xMethod.Value.DeclaringType.FullName;

				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
				new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceRef = Indy.IL2CPU.Assembler.ElementReference.New(strTypeId), SourceIsIndirect = true };
				new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
				new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.NormalObject, Size = 32 };
				new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceValue = (uint)xGCFieldCount, Size = 32 };
				uint xSize = (uint)(((from item in xParams
									  let xQSize = Align(GetFieldStorageSize(item.GetType()), 4)
									  select (int)xQSize).Take(xParams.Length - 1).Sum()));

				foreach (var xParam in xParams) {
          uint xParamSize = GetFieldStorageSize(xParams.GetType());
					new Comment(String.Format("Arg {0}: {1}", xParam.Name, xParamSize));
          for (int i = 0; i < xParamSize; i += 4) {
						new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize + 4) };
					}
				}

				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(xMethod.Value) };
				new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = xCurrentLabel + "_NO_ERROR_4" };

				//for( int i = 1; i < aCtorMethodInfo.Arguments.Length; i++ )
				//{
				//    new CPUx86.Add
				//    {
				//        DestinationReg = CPUx86.Registers.ESP,
				//        SourceValue = ( aCtorMethodInfo.Arguments[ i ].Size % 4 == 0
				//             ? aCtorMethodInfo.Arguments[ i ].Size
				//             : ( ( aCtorMethodInfo.Arguments[ i ].Size / 4 ) * 4 ) + 1 )
				//    };
				//}
				PushAlignedParameterSize(xMethod.Value);
// an exception occurred, we need to cleanup the stack, and jump to the exit
				new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
				foreach (var xStackInt in Assembler.Stack)
				{
					new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xStackInt.Size };
				}
				Jump_Exception(aMethod);

				new Label(xCurrentLabel + "_NO_ERROR_4");
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };

				//for( int i = 1; i < aCtorMethodInfo.Arguments.Length; i++ )
				//{
				//    new CPUx86.Add
				//    {
				//        DestinationReg = CPUx86.Registers.ESP,
				//        SourceValue = ( aCtorMethodInfo.Arguments[ i ].Size % 4 == 0
				//             ? aCtorMethodInfo.Arguments[ i ].Size
				//             : ( ( aCtorMethodInfo.Arguments[ i ].Size / 4 ) * 4 ) + 1 )
				//    };
				//}
				PushAlignedParameterSize(xMethod.Value);

				new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
				Assembler.Stack.Push(4, xMethod.Value.DeclaringType);
			}
			else
			{
				//                 /*
				//                  * Current sitation on stack:
				//                  *   $ESP       Arg
				//                  *   $ESP+..    other items
				//                  *   
				//                  * What should happen:
				//                  *  + The stack should be increased to allow space to contain:
				//                  *         + .ctor arguments
				//                  *         + struct _pointer_ (ref to start of emptied space)
				//                  *         + empty space for struct
				//                  *  + arguments should be copied to the new place
				//                  *  + old place where arguments were should be cleared
				//                  *  + pointer should be set
				//                  *  + call .ctor
				//                  */                
				//                 var xStorageSize = aCtorDeclTypeInfo.StorageSize;
				//                 if (xStorageSize % 4 != 0)
				//                 {
				//                     xStorageSize += 4 - (xStorageSize % 4);
				//                 }
				//                 uint xArgSize = 0;
				//                 foreach (var xArg in aCtorMethodInfo.Arguments.Skip(1))
				//                 {
				//                     xArgSize += xArg.Size + (xArg.Size % 4 == 0
				//                                                             ? 0
				//                                                             : (4 - (xArg.Size % 4)));
				//                 }
				//                 int xExtraArgSize = (int)(xStorageSize - xArgSize);
				//                 if (xExtraArgSize < 0)
				//                 {
				//                     xExtraArgSize = 0;
				//                 }
				//                 if (xExtraArgSize>0)
				//                 {
				//                     new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xExtraArgSize };
				//                 }
				//                 new CPUx86.Push { DestinationReg = Registers.ESP };
				//                 aAssembler.Stack.Push(new StackContent(4));
				//                 //at this point, we need to move copy all arguments over. 
				//                 for (int i = 0;i<(xArgSize/4);i++)
				//                 {
				//                     new CPUx86.Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStorageSize + 4) }; // + 4 because the ptr is pushed too
				//                     new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStorageSize + 4 + 4), SourceValue = 0, Size = 32 };
				//                 }
				//                 var xCall = new Call(aCtorDef,
				//                                      (uint)aCurrentILOffset,
				//                                      true,
				//                                      aNextLabel);
				//                 xCall.SetServiceProvider(aServiceProvider);
				//                 xCall.Assembler = aAssembler;
				//                 xCall.Assemble();
				//                 aAssembler.Stack.Push(new StackContent((int)xStorageSize,
				//                                                                aCtorDef.DeclaringType));
				throw new NotImplementedException();
			}
		}

		private uint Align(uint aSize, uint aAlign)
		{
			return aSize % 4 == 0 ? aSize : ((aSize / aAlign) * aAlign) + 1;
		}

		private void PushAlignedParameterSize(System.Reflection.MethodBase aMethod)
		{
			System.Reflection.ParameterInfo[] xParams = aMethod.GetParameters();

			uint xSize;

			for (int i = 1; i < xParams.Length; i++)
			{
				xSize = GetFieldStorageSize(xParams[i].GetType());
				new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = Align(xSize, 4) };
			}
		}
		// using System.Collections.Generic;
		// using System.Diagnostics;
		// using System.Linq;
		// using System.Reflection;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Assembler.X86;
		// using CPU=Indy.IL2CPU.Assembler;
		// using CPUx86=Indy.IL2CPU.Assembler.X86;
		// using Asm=Indy.IL2CPU.Assembler;
		// using Assembler=Indy.IL2CPU.Assembler.Assembler;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86
		// {
		//     [OpCode(OpCodeEnum.Newobj)]
		//     public class Newobj : Op
		//     {
		//         public MethodBase CtorDef;
		//         public string CurrentLabel;
		//         public uint ILOffset;
		//         public MethodInformation MethodInformation;
		// 
		//         public static void ScanOp(MethodBase aCtor, IServiceProvider aProvider)
		//         {
		//             Call.ScanOp(aCtor, aProvider);
		//             Call.ScanOp(GCImplementationRefs.AllocNewObjectRef, aProvider);
		//             Call.ScanOp(CPU.Assembler.CurrentExceptionOccurredRef, aProvider);
		//             Call.ScanOp(GCImplementationRefs.IncRefCountRef, aProvider);
		//         }
		// 
		//         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aProvider)
		//         {
		//             var xCtorDef = aReader.OperandValueMethod;
		//             ScanOp(xCtorDef, aProvider);
		//         }
		// 
		//         public Newobj(ILReader aReader,
		//                       MethodInformation aMethodInfo)
		//             : base(aReader,
		//                    aMethodInfo)
		//         {
		//             CtorDef = aReader.OperandValueMethod;
		//             CurrentLabel = GetInstructionLabel(aReader);
		//             MethodInformation = aMethodInfo;
		//             ILOffset = aReader.Position;
		//             mNextLabel = GetInstructionLabel(aReader.NextPosition);
		//         }
		// 
		//         private string mNextLabel;
		// }
	}
}