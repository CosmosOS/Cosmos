using System;
using Cosmos.IL2CPU.ILOpCodes;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// 
// using IL2CPU=Cosmos.IL2CPU;
using CPU = Cosmos.Compiler.Assembler.X86;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.IL2CPU.X86;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Cosmos.Compiler.Assembler;
// using System.Reflection;
// using Cosmos.IL2CPU.X86;
// using Cosmos.IL2CPU.Compiler;

namespace Cosmos.IL2CPU.X86.IL {
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Call)]
  public class Call: ILOp {
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

    public Call(Cosmos.Compiler.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public static uint GetStackSizeToReservate(MethodBase aMethod) {
      
      var xMethodInfo = aMethod as System.Reflection.MethodInfo;
      uint xReturnSize = 0;
      if (xMethodInfo != null) {
        xReturnSize = SizeOfType(xMethodInfo.ReturnType);
      }
      if (xReturnSize == 0) {
        return 0;
      }
      // todo: implement exception support
      int xExtraStackSize = (int)Align(xReturnSize, 4);
      var xParameters = aMethod.GetParameters();
      foreach (var xItem in xParameters) {
        xExtraStackSize -= (int)Align(SizeOfType(xItem.GetType()), 4);
      }
      if (!xMethodInfo.IsStatic) {
        xExtraStackSize -= (int)Align(SizeOfType(xMethodInfo.DeclaringType), 4);
      }
      if (xExtraStackSize > 0) {
        return (uint)xExtraStackSize;
      }
      return 0;
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpMethod = aOpCode as OpMethod;
      DoExecute(Assembler, aMethod, xOpMethod.Value, xOpMethod.ValueUID, aOpCode.Position);
    }

    public static void DoExecute(Assembler Assembler, MethodInfo aCurrentMethod, MethodBase aTargetMethod, uint aTargetMethodUID, int aCurrentPosition) {
      //if (aTargetMethod.IsVirtual) {
      //  Callvirt.DoExecute(Assembler, aCurrentMethod, aTargetMethod, aTargetMethodUID, aCurrentPosition);
      //  return;
      //}
      var xMethodInfo = aTargetMethod as System.Reflection.MethodInfo;
      string xCurrentMethodLabel = MethodInfoLabelGenerator.GenerateLabelName(aCurrentMethod.MethodBase);

      // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
      //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
      string xNormalAddress = "";
      if (aTargetMethod.IsStatic || !aTargetMethod.IsVirtual || aTargetMethod.IsFinal) {
        xNormalAddress = MethodInfoLabelGenerator.GenerateLabelName(aTargetMethod);
      } else {
          xNormalAddress = MethodInfoLabelGenerator.GenerateLabelName(aTargetMethod);
        //throw new Exception("Call: non-concrete method called!");
      }
      var xParameters = aTargetMethod.GetParameters();
      int xArgCount = xParameters.Length;
      // todo: implement exception support
      uint xExtraStackSize = GetStackSizeToReservate(aTargetMethod);
      if (xExtraStackSize > 0) {
        new CPUx86.Sub {
          DestinationReg = CPUx86.Registers.ESP,
          SourceValue = (uint)xExtraStackSize
        };
      }
      new CPUx86.Call {
        DestinationLabel = xNormalAddress
      };
      ////if (mResultSize != 0) {
      ////new CPUx86.Pop("eax");
      ////}
      //EmitExceptionLogic(Assembler,
      //           mCurrentILOffset,
      //           mMethodInfo,
      //           mNextLabelName,
      //           true,
      //           delegate() {
      //             var xResultSize = mTargetMethodInfo.ReturnSize;
      //             if (xResultSize % 4 != 0) {
      //               xResultSize += 4 - (xResultSize % 4);
      //             }
      //             for (int i = 0; i < xResultSize / 4; i++) {
      //               new CPUx86.Add {
      //                 DestinationReg = CPUx86.Registers.ESP,
      //                 SourceValue = 4
      //               };
      //             }
      //           });
      for (int i = 0; i < xParameters.Length; i++) {
        Assembler.Stack.Pop();
      }
      if (!aTargetMethod.IsStatic) {
        Assembler.Stack.Pop();
      }
      if (xMethodInfo == null || SizeOfType(xMethodInfo.ReturnType) == 0) {
        return;
      }

      Assembler.Stack.Push((int)SizeOfType(xMethodInfo.ReturnType),
                              xMethodInfo.ReturnType);
    }

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
    //                 xArgumentSizes.Add(GetService<IMetaDataInfoService>().SizeOfType(xParam.ParameterType));
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

  }
}
