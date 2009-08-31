using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Newobj)]
	public class Newobj: ILOp
	{
		public Newobj(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      // Is this checking for plugs?
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

      // Instance versus static ctor?
      //             if (aCtorDeclTypeInfo.NeedsGC) {
      //                 uint xObjectSize = aCtorDeclTypeInfo.StorageSize;
      //                 for (int i = 1; i < aCtorMethodInfo.Arguments.Length; i++) {
      //                     aAssembler.Stack.Pop();
      //                 }
      //                 int xExtraSize = 20;
      //                 new Push { DestinationValue = (uint)(xObjectSize + xExtraSize) };
      //                 new Assembler.X86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef) };
      //                 //new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
      //                 new Test { DestinationReg = Registers.ECX, SourceValue = 2 };
      //                 //new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_1");
      //                 //for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
      //                 //    new CPUx86.Add(CPUx86.Registers_Old.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
      //                 //}
      //                 //new CPUx86.Add("esp", "4");
      //                 //Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_1", false);
      //                 //new CPU.Label(aCurrentLabel + "_NO_ERROR_1");
      //                 new Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true };
      //                 new Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true };
      //                 new Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true };
      //                 new Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true };
      //                 var xIncRefInfo =
      //                     aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(
      //                         GCImplementationRefs.IncRefCountRef, false);
      //                 new Assembler.X86.Call { DestinationLabel = xIncRefInfo.LabelName };
      //                 //new CPUx86.Test("ecx", "2");
      //                 //new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_2");
      //                 //for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
      //                 //    new CPUx86.Add(CPUx86.Registers_Old.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
      //                 //}
      //                 //new CPUx86.Add("esp", "16");
      //                 //Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_2", false);
      //                 //new CPU.Label(aCurrentLabel + "_NO_ERROR_2");
      //                 new Assembler.X86.Call { DestinationLabel = xIncRefInfo.LabelName };
      //                 //new CPUx86.Test("ecx", "2");
      //                 //new CPUx86.JumpIfEquals(aCurrentLabel + "_NO_ERROR_3");
      //                 //for (int i = 1; i < xCtorInfo.Arguments.Length; i++) {
      //                 //    new CPUx86.Add(CPUx86.Registers_Old.ESP, (xCtorInfo.Arguments[i].Size % 4 == 0 ? xCtorInfo.Arguments[i].Size : ((xCtorInfo.Arguments[i].Size / 4) * 4) + 1).ToString());
      //                 //}
      //                 //new CPUx86.Add("esp", "12");
      //                 //Call.EmitExceptionLogic(aAssembler, aCurrentMethodInformation, aCurrentLabel + "_NO_ERROR_3", false);
      //                 //new CPU.Label(aCurrentLabel + "_NO_ERROR_3");
      //                 uint xObjSize = 0;
      //                 int xGCFieldCount = (from item in aCtorDeclTypeInfo.Fields.Values
      //                                      where item.NeedsGC
      //                                      select item).Count();
      //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
      //                 new Move { DestinationReg = Registers.EBX, SourceRef = ElementReference.New(aTypeId), SourceIsIndirect = true };
      //                 new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceReg=CPUx86.Registers.EBX};
      //                 new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.NormalObject, Size = 32 };
      //                 new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 8, SourceValue = (uint)xGCFieldCount, Size = 32 };
      //                 uint xSize = (uint)(((from item in aCtorMethodInfo.Arguments
      //                                       let xQSize = item.Size + (item.Size % 4 == 0
      //                                                             ? 0
      //                                                             : (4 - (item.Size % 4)))
      //                                       select (int)xQSize).Take(aCtorMethodInfo.Arguments.Length - 1).Sum()));
      //                 for (int i = 1; i < aCtorMethodInfo.Arguments.Length; i++)
      //                 {
      //                     new Comment(String.Format("Arg {0}: {1}", i, aCtorMethodInfo.Arguments[i].Size));
      //                     for (int j = 0; j < aCtorMethodInfo.Arguments[i].Size; j += 4)
      //                     {
      //                         new Push { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize + 4) };
      //                     }
      //                 }
      // 
      //                 new Assembler.X86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aCtorDef) };
      //                 new Test { DestinationReg = Registers.ECX, SourceValue = 2 };
      //                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = aCurrentLabel + "_NO_ERROR_4" };
      //                 for (int i = 1;i < aCtorMethodInfo.Arguments.Length;i++)
      //                 {
      //                     new Assembler.X86.Add
      //                     {
      //                         DestinationReg = Registers.ESP,
      //                         SourceValue = (aCtorMethodInfo.Arguments[i].Size % 4 == 0
      //                              ? aCtorMethodInfo.Arguments[i].Size
      //                              : ((aCtorMethodInfo.Arguments[i].Size / 4) * 4) + 1)
      //                     };
      //                 }
      //                 new Assembler.X86.Add { DestinationReg = Registers.ESP, SourceValue = 4 };
      //                 foreach (StackContent xStackInt in aAssembler.StackContents)
      //                 {
      //                     new Assembler.X86.Add { DestinationReg = Registers.ESP, SourceValue=(uint)xStackInt.Size };
      //                 }
      //                 Call.EmitExceptionLogic(aAssembler,
      //                                         (uint)aCurrentILOffset,
      //                                         aCurrentMethodInformation,
      //                                         aCurrentLabel + "_NO_ERROR_4",
      //                                         false,
      //                                         null);
      //                 new Label(aCurrentLabel + "_NO_ERROR_4");
      //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
      //                 //				aAssembler.StackSizes.Pop();
      //                 //	new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
      //                 for (int i = 1; i < aCtorMethodInfo.Arguments.Length; i++)
      //                 {
      //                     new Assembler.X86.Add
      //                     {
      //                         DestinationReg = Registers.ESP,
      //                         SourceValue = (aCtorMethodInfo.Arguments[i].Size % 4 == 0
      //                              ? aCtorMethodInfo.Arguments[i].Size
      //                              : ((aCtorMethodInfo.Arguments[i].Size / 4) * 4) + 1)
      //                     };
      //                 }
      //                 new Push { DestinationReg=Registers.EAX };
      //                 aAssembler.Stack.Push(new StackContent(4,
      //                                                                aCtorDef.DeclaringType));
      //             } else {
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
      //             }
      throw new NotImplementedException();
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
