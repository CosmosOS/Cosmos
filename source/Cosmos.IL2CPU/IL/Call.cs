using System;
using Cosmos.IL2CPU.ILOpCodes;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
//
// using IL2CPU=Cosmos.IL2CPU;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Cosmos.Assembler;

using SysReflection = System.Reflection;


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

    public Call(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public static uint GetStackSizeToReservate(MethodBase aMethod) {

      var xMethodInfo = aMethod as SysReflection.MethodInfo;
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
        xExtraStackSize -= (int)Align(SizeOfType(xItem.ParameterType), 4);
      }
      if (!xMethodInfo.IsStatic) {
		  xExtraStackSize -= GetNativePointerSize(xMethodInfo);
      }
      if (xExtraStackSize > 0) {
        return (uint)xExtraStackSize;
      }
      return 0;
    }

	private static int GetNativePointerSize(SysReflection.MethodInfo xMethodInfo)
	{
		// old code, which goof up everything for structs
		//return (int)Align(SizeOfType(xMethodInfo.DeclaringType), 4);
		// TODO native pointer size, so that COSMOS could be 64 bit OS
		return 4;
	}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      var xOpMethod = aOpCode as OpMethod;
      DoExecute(Assembler, aMethod, xOpMethod.Value, aOpCode, LabelName.Get(aMethod.MethodBase), DebugEnabled);
    }

    public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aCurrentMethod, MethodBase aTargetMethod, ILOpCode aCurrent, string currentLabel, bool debugEnabled)
    {
      DoExecute(Assembler, aCurrentMethod, aTargetMethod, aCurrent, currentLabel, ILOp.GetLabel(aCurrentMethod, aCurrent.NextPosition), debugEnabled);
    }

    public static void DoExecute(Cosmos.Assembler.Assembler Assembler, MethodInfo aCurrentMethod, MethodBase aTargetMethod, ILOpCode aCurrent, string currentLabel, string nextLabel, bool debugEnabled) {
      //if (aTargetMethod.IsVirtual) {
      //  Callvirt.DoExecute(Assembler, aCurrentMethod, aTargetMethod, aTargetMethodUID, aCurrentPosition);
      //  return;
      //}
      var xMethodInfo = aTargetMethod as SysReflection.MethodInfo;

      // mTargetMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(mMethod
      //   , mMethod, mMethodDescription, null, mCurrentMethodInfo.DebugMode);
      string xNormalAddress = "";
      if (aTargetMethod.IsStatic || !aTargetMethod.IsVirtual || aTargetMethod.IsFinal) {
        xNormalAddress = LabelName.Get(aTargetMethod);
      } else {
          xNormalAddress = LabelName.Get(aTargetMethod);
        //throw new Exception("Call: non-concrete method called: '" + aTargetMethod.GetFullName() + "'");
      }
      var xParameters = aTargetMethod.GetParameters();
      int xArgCount = xParameters.Length;
      // todo: implement exception support
      uint xExtraStackSize = GetStackSizeToReservate(aTargetMethod);
        if (!aTargetMethod.IsStatic && debugEnabled)
        {
            uint xThisOffset = 0;
            foreach (var xItem in xParameters)
            {
                xThisOffset += Align(SizeOfType(xItem.ParameterType), 4);
            }
            var stackOffsetToCheck = xThisOffset;
            DoNullReferenceCheck(Assembler, debugEnabled, stackOffsetToCheck);
        }

        if (xExtraStackSize > 0) {
        new CPUx86.Sub {
          DestinationReg = CPUx86.Registers.ESP,
          SourceValue = (uint)xExtraStackSize
        };
      }
      new CPUx86.Call {
        DestinationLabel = xNormalAddress
      };

      uint xReturnSize=0;
      if (xMethodInfo != null)
      {
          xReturnSize = SizeOfType(xMethodInfo.ReturnType);
      }
      if (aCurrentMethod != null)
      {
          EmitExceptionLogic(Assembler, aCurrentMethod, aCurrent, true,
                     delegate()
                     {
                         var xResultSize = xReturnSize;
                         if (xResultSize % 4 != 0)
                         {
                             xResultSize += 4 - (xResultSize % 4);
                         }
                         for (int i = 0; i < xResultSize / 4; i++)
                         {
                             new CPUx86.Add
                             {
                                 DestinationReg = CPUx86.Registers.ESP,
                                 SourceValue = 4
                             };
                         }
                     }, nextLabel);

      }
      if (xMethodInfo == null || SizeOfType(xMethodInfo.ReturnType) == 0) {
        return;
	  }

    }
  }
}
