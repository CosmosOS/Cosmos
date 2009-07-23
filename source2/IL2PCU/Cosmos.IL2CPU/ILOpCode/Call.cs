using System;

namespace Cosmos.IL2CPU.ILOpCodes {
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Call)]
	public class Call: ILOpCode
	{
    //TODO: Lets lock down this method signature. The current arguments are not a good final choice.
    // Requirements
    // - Some methods need to pass information back to scanner. Can we make a list of these needs?
    //    - QueueMethod
    //    - QueueType
    //    - What else?
    // - Ability to read operand arguments. Operands are 8 bytes maximum. Reading from a buffer into new vars is inefficient. 
    //   Can we determine all the argument combinatioins, ie int, int + int, int64 and make an enum and attribute that scanner can
    //   use to preparse the arguments in the most efficient manner.
    //TODO: 
    // - Change the scan method to use constructor arguments and force constructor dependency. Pass data in contructor, then use separate assemble
    // method? This would allow whole methods to be created and then assembled in one go. Is there any advantage to this versus immediate assembly?
    // Could be used later to optimize per method, would give an optimizer a chance to look at the ops in a single method. I had planned to allow this
    // by pattern matching before the ops were created though.
    // - If not separate methods, will ops be disposed of immediately after create? If so is there any use in instantiating them in the first place?
    // Would a static be more appropriate?
    // - Pass scanner and reader in base constructor and then have empty argument assemble method? Requires more copying of pointer to heap though.
        public override void Scan(ILReader aReader, ILScanner aScanner) {
            base.Scan(aReader, aScanner);
            aScanner.QueueMethod(aReader.OperandValueMethod);
        }


		#region Old code
		// using System;
		// using System.Collections.Generic;
		// using System.IO;
		// using System.Linq;
		// 
		// using IL2CPU=Indy.IL2CPU;
		// using CPU=Indy.IL2CPU.Assembler;
		// using CPUx86=Indy.IL2CPU.Assembler.X86;
		// using System.Reflection;
		// using Indy.IL2CPU.Assembler;
		// using Indy.IL2CPU.Compiler;
		// 
		// namespace Indy.IL2CPU.IL.X86
		// {
		//     [Cosmos.IL2CPU.OpCode(ILOp.Code.Call)]
		//     public class Call : ILOpCode
		//     {
		//         private string LabelName;
		//         private uint mResultSize;
		//         private uint? TotalArgumentSize = null;
		//         private bool mIsDebugger_Break = false;
		//         private uint[] ArgumentSizes = new uint[0];
		//         private MethodInformation mMethodInfo;
		//         private MethodInformation mTargetMethodInfo;
		//         private string mNextLabelName;
		//         private uint mCurrentILOffset;
		//         private MethodBase mMethod;
		// 
		//         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aServiceProvider)
		//         {
		//             MethodBase xMethod = aReader.OperandValueMethod;
		//             ScanOp(xMethod, aServiceProvider);
		//         }
		// 
		//         public static void ScanOp(MethodBase aTargetMethod, IServiceProvider provider)
		//         {
		//             provider.GetService<IMetaDataInfoService>().GetMethodInfo(aTargetMethod, false);
		//             foreach (ParameterInfo xParam in aTargetMethod.GetParameters())
		//             {
		//                 provider.GetService<IMetaDataInfoService>().GetTypeInfo(xParam.ParameterType);
		//             }
		//             var xTargetMethodInfo = provider.GetService<IMetaDataInfoService>().GetMethodInfo(aTargetMethod, 
		//                             aTargetMethod, MethodInfoLabelGenerator.GenerateLabelName(aTargetMethod), 
		//                             provider.GetService<IMetaDataInfoService>().GetTypeInfo(aTargetMethod.DeclaringType), false);
		//             provider.GetService<IMetaDataInfoService>().GetTypeInfo(xTargetMethodInfo.ReturnType);
		//         }
		// 
		//         public Call(MethodBase aMethod,
		//                     uint aCurrentILOffset,
		//                     bool aDebugMode,
		//                     uint aExtraStackSpace,
		//                     string aNormalNext)
		//             : base(null, null)
		//         {
		//             if (aMethod == null)
		//             {
		//                 throw new ArgumentNullException("aMethod");
		//             }
		//             mMethod = aMethod;
		//             mNextLabelName = aNormalNext;
		//             mCurrentILOffset = aCurrentILOffset;
		//             mDebugMode = aDebugMode;
		//         }
		// 
		//         public Call(MethodBase aMethod, uint aCurrentILOffset, bool aDebugMode, string aNormalNext)
		//             : this(aMethod, aCurrentILOffset, aDebugMode, 0, aNormalNext)
		//         {
		//         }
		// 
		//         public static void EmitExceptionLogic(Assembler.Assembler aAssembler, uint aCurrentOpOffset, MethodInformation aMethodInfo, string aNextLabel, bool aDoTest, Action aCleanup)
		//         {
		//             string xJumpTo = MethodFooterOp.EndOfMethodLabelNameException;
		//             if (aMethodInfo != null && aMethodInfo.CurrentHandler != null)
		//             {
		//                 // todo add support for nested handlers, see comment in Engine.cs
		//                 //if (!((aMethodInfo.CurrentHandler.HandlerOffset < aCurrentOpOffset) || (aMethodInfo.CurrentHandler.HandlerLength + aMethodInfo.CurrentHandler.HandlerOffset) <= aCurrentOpOffset)) {
		//                 new CPU.Comment(String.Format("CurrentOffset = {0}, HandlerStartOffset = {1}", aCurrentOpOffset, aMethodInfo.CurrentHandler.HandlerOffset));
		//                 if (aMethodInfo.CurrentHandler.HandlerOffset > aCurrentOpOffset)
		//                 {
		//                     switch (aMethodInfo.CurrentHandler.Flags)
		//                     {
		//                         case ExceptionHandlingClauseOptions.Clause:
		//                             {
		//                                 xJumpTo = Op.GetInstructionLabel(aMethodInfo.CurrentHandler.HandlerOffset);
		//                                 break;
		//                             }
		//                         case ExceptionHandlingClauseOptions.Finally:
		//                             {
		//                                 xJumpTo = Op.GetInstructionLabel(aMethodInfo.CurrentHandler.HandlerOffset);
		//                                 break;
		//                             }
		//                         default:
		//                             {
		//                                 throw new Exception("ExceptionHandlerType '" + aMethodInfo.CurrentHandler.Flags.ToString() + "' not supported yet!");
		//                             }
		//                     }
		//                 }
		//             }
		//             if (!aDoTest)
		//             {
		//                 //new CPUx86.Call("_CODE_REQUESTED_BREAK_");
		//                 new CPUx86.Jump { DestinationLabel = xJumpTo };
		//             }
		//             else
		//             {
		//                 new CPUx86.Test { DestinationReg = CPUx86.Registers.ECX, SourceValue = 2 };
		//                 if (aCleanup != null)
		//                 {
		//                     new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = aNextLabel };
		//                     aCleanup();
		//                     new CPUx86.Jump { DestinationLabel = xJumpTo };
		//                 }
		//                 else
		//                 {
		//                     new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = xJumpTo };
		//                 }
		//             }
		//         }
		// 
		//         private void Initialize(MethodBase aMethod, uint aCurrentILOffset, bool aDebugMode)
		//         {
		//             mIsDebugger_Break = aMethod.GetFullName() == "System.Void  System.Diagnostics.Debugger.Break()";
		//             if (mIsDebugger_Break)
		//             {
		//                 return;
		//             }
		//             mCurrentILOffset = aCurrentILOffset;
		//             bool HasDynamic = DynamicMethodEmit.GetHasDynamicMethod(aMethod);
		//             if (HasDynamic)
		//             {
		//                 aMethod = DynamicMethodEmit.GetDynamicMethod(aMethod);
		//             }
		//             try
		//             {
		//                 mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(aMethod, aMethod,
		//                                                                                      MethodInfoLabelGenerator.
		//                                                                                          GenerateLabelName(aMethod),
		//                                                                                      GetService<IMetaDataInfoService>().
		//                                                                                          GetTypeInfo(
		//                                                                                          aMethod.DeclaringType),
		//                                                                                      aDebugMode);
		//             }
		//             catch
		//             {
		//                 Console.WriteLine(aMethod.GetFullName());
		//                 Console.Write("");
		//                 mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(aMethod, aMethod,
		//                                                                                      MethodInfoLabelGenerator.
		//                                                                                          GenerateLabelName(aMethod),
		//                                                                                      GetService<IMetaDataInfoService>().
		//                                                                                          GetTypeInfo(
		//                                                                                          aMethod.DeclaringType),
		//                                                                                      aDebugMode);
		// 
		//             }
		//             mResultSize = 0;
		//             if (mTargetMethodInfo != null)
		//             {
		//                 mResultSize = mTargetMethodInfo.ReturnSize;
		//             }
		//             LabelName = CPU.MethodInfoLabelGenerator.GenerateLabelName(aMethod);
		//             //if (!HasDynamic)
		//             //    Engine.QueueMethod(aMethod);
		//             bool needsCleanup = false;
		//             List<uint> xArgumentSizes = new List<uint>();
		//             ParameterInfo[] xParams = aMethod.GetParameters();
		//             foreach (ParameterInfo xParam in xParams)
		//             {
		//                 xArgumentSizes.Add(GetService<IMetaDataInfoService>().GetFieldStorageSize(xParam.ParameterType));
		//             }
		//             if (!aMethod.IsStatic)
		//             {
		//                 xArgumentSizes.Insert(0, 4);
		//             }
		//             ArgumentSizes = xArgumentSizes.ToArray();
		//             foreach (ParameterInfo xParam in xParams)
		//             {
		//                 if (xParam.IsOut)
		//                 {
		//                     needsCleanup = true;
		//                     break;
		//                 }
		//             }
		//             if (needsCleanup)
		//             {
		//                 TotalArgumentSize = 0;
		//                 foreach (var xArgSize in ArgumentSizes)
		//                 {
		//                     TotalArgumentSize += xArgSize;
		//                 }
		//             }
		//             // todo: add support for other argument sizes
		//         }
		// 
		//         public Call(ILReader aReader, MethodInformation aMethodInfo)
		//             : base(aReader, aMethodInfo)
		//         {
		//             mMethod = aReader.OperandValueMethod;
		//             mMethodInfo = aMethodInfo;
		//             if (aMethodInfo.LabelName == "System_Void__System_Array_Sort_System_Double__System_Double___"
		//                 && aReader.Position==0x1A)
		//             {                            
		//                 Console.Write("");
		//             }
		//             if (!aReader.EndOfStream)
		//             {
		//                 mNextLabelName = GetInstructionLabel(aReader.NextPosition);
		//             }
		//             else
		//             {
		//                 mNextLabelName = X86MethodFooterOp.EndOfMethodLabelNameNormal;
		//             }
		//             mCurrentILOffset = aReader.Position;
		//             mDebugMode = mMethodInfo.DebugMode;
		//         }
		//         private bool mDebugMode;
		// 
		//         private void Assemble(string aMethod, int aArgumentCount)
		//         {
		//             if (mTargetMethodInfo.ExtraStackSize > 0)
		//             {
		//                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)mTargetMethodInfo.ExtraStackSize };
		//             }
		//             new CPUx86.Call { DestinationLabel = aMethod };
		//             //if (mResultSize != 0) {
		//             //new CPUx86.Pop("eax");
		//             //}
		//             EmitExceptionLogic(Assembler,
		//                                mCurrentILOffset,
		//                                mMethodInfo,
		//                                mNextLabelName,
		//                                true,
		//                                delegate()
		//                                {
		//                                    var xResultSize = mTargetMethodInfo.ReturnSize;
		//                                    if (xResultSize %4!=0)
		//                                    {
		//                                        xResultSize += 4 - (xResultSize % 4);
		//                                    }
		//                                    for (int i = 0;i< xResultSize/4;i++)
		//                                    {
		//                                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                                    }
		//                                });
		//             for (int i = 0;i < aArgumentCount;i++)
		//             {
		//                 Assembler.StackContents.Pop();
		//             }
		//             if (mResultSize == 0)
		//             {
		//                 return;
		//             }
		// 
		//             Assembler.StackContents.Push(new StackContent((int)mResultSize,
		//                                                           ((MethodInfo)mTargetMethodInfo.Method).ReturnType));
		//         }
		// 
		//         protected virtual void HandleDebuggerBreak()
		//         {
		//             new CPUx86.Call { DestinationLabel = "DebugStub_Step" };
		//         }
		// 
		//         public override void DoAssemble()
		//         {
		//             Initialize(mMethod, mCurrentILOffset, mDebugMode);
		//             if (mIsDebugger_Break)
		//             {
		//                 HandleDebuggerBreak();
		//             }
		//             else
		//             {
		//                 Assemble(LabelName, ArgumentSizes.Length);
		//             }
		//         }
		//     }
		// }
		#endregion
	}
}
