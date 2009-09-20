using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPUx86 = Cosmos.IL2CPU.X86;
using System.Reflection;
using System.IO;

namespace Cosmos.IL2CPU.X86 {
    public class AssemblerNasm : CosmosAssembler
    {
      public const string EndOfMethodLabelNameNormal = ".END__OF__METHOD_NORMAL";
      public const string EndOfMethodLabelNameException = ".END__OF__METHOD_EXCEPTION";

    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

    public AssemblerNasm() : base( 0 ) { }

    protected override void MethodBegin(MethodInfo aMethod) {
      base.MethodBegin(aMethod);
      new Label(aMethod.MethodBase);
      new Push { DestinationReg = Registers.EBP };
      new Move { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      //new CPUx86.Push("0");
      //if (!(aLabelName.Contains("Cosmos.Kernel.Serial") || aLabelName.Contains("Cosmos.Kernel.Heap"))) {
      //    new CPUx86.Push(LdStr.GetContentsArrayName(aAssembler, aLabelName));
      //    MethodBase xTempMethod = Engine.GetMethodBase(Engine.GetType("Cosmos.Kernel", "Cosmos.Kernel.Serial"), "Write", "System.Byte", "System.String");
      //    new CPUx86.Call(MethodInfoLabelGenerator.GenerateLabelName(xTempMethod));
      //    Engine.QueueMethod(xTempMethod);
      //}
      if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null) {
        var xBody = aMethod.MethodBase.GetMethodBody();
        if (xBody != null) {
          foreach (var xLocal in xBody.LocalVariables) {
            new Comment("Local " + xLocal.LocalIndex);
            new Sub { DestinationReg = Registers.ESP, SourceValue = ILOp.Align(ILOp.SizeOfType(xLocal.LocalType), 4) };
          }
        }
      }
      //foreach (var xLocal in aLocals) {
      //  aAssembler.StackContents.Push(new StackContent(xLocal.Size, xLocal.VariableType));
      //  for (int i = 0; i < (xLocal.Size / 4); i++) {
      //    new CPUx86.Push { DestinationValue = 0 };
      //  }
      //}
      //if (aDebugMode && aIsNonDebuggable) {
      //  new CPUx86.Call { DestinationLabel = "DebugPoint_DebugSuspend" };
      //}
    }

    protected override void MethodEnd(MethodInfo aMethod) {
      base.MethodEnd(aMethod);
      uint xReturnSize = 0;
      var xMethInfo = aMethod.MethodBase as System.Reflection.MethodInfo;
      if (xMethInfo != null) {
        xReturnSize = ILOp.Align(ILOp.SizeOfType(xMethInfo.ReturnType), 4);
      }
      new Label(MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + EndOfMethodLabelNameNormal);
      new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
      var xTotalArgsSize = (from item in aMethod.MethodBase.GetParameters()
                            select (int)ILOp.Align(ILOp.SizeOfType(item.ParameterType), 4)).Sum();
      if (!aMethod.MethodBase.IsStatic) {
        xTotalArgsSize += (int)ILOp.Align(ILOp.SizeOfType(aMethod.MethodBase.DeclaringType), 4);
      }

      if (xReturnSize > 0) {
        //var xArgSize = (from item in aArgs
        //                let xSize = item.Size + item.Offset
        //                select xSize).FirstOrDefault();
        //new Comment(String.Format("ReturnSize = {0}, ArgumentSize = {1}",
        //                          aReturnSize,
        //                          xArgSize));
        //int xOffset = 4;
        //if(xArgSize>0) {
        //    xArgSize -= xReturnSize;
        //    xOffset = xArgSize;
        //}
        int xOffset = 4;
        xOffset += xTotalArgsSize;
        if ((xTotalArgsSize - xReturnSize) < 0) {
          //xOffset += (int)(0 - (xTotalArgsSize - xReturnSize));
          xOffset = 8;
        }
        for (int i = 0; i < xReturnSize / 4; i++) {
          new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
          new CPUx86.Move {
            DestinationReg = CPUx86.Registers.EBP,
            DestinationIsIndirect = true,
            DestinationDisplacement = (int)(xOffset + ((i + 1) * 4) + 0 - xReturnSize),
            SourceReg = Registers.EAX
          };
        }
        // extra stack space is the space reserved for example when a "public static int TestMethod();" method is called, 4 bytes is pushed, to make room for result;
      }
      new Label(MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + EndOfMethodLabelNameException);
      //for (int i = 0; i < aLocAllocItemCount; i++) {
      //  new CPUx86.Call { DestinationLabel = aHeapFreeLabel };
      //}
      //if (aDebugMode && aIsNonDebuggable) {
      //  new CPUx86.Call { DestinationLabel = "DebugPoint_DebugResume" };
      //}

      //if ((from xLocal in aLocals
      //     where xLocal.IsReferenceType
      //     select 1).Count() > 0 || (from xArg in aArgs
      //                               where xArg.IsReferenceType
      //                               select 1).Count() > 0) {
      //  new CPUx86.Push { DestinationReg = Registers.ECX };
      //  //foreach (MethodInformation.Variable xLocal in aLocals) {
      //  //  if (xLocal.IsReferenceType) {
      //  //    Op.Ldloc(aAssembler,
      //  //             xLocal,
      //  //             false,
      //  //             aGetStorageSizeDelegate(xLocal.VariableType));
      //  //    new CPUx86.Call { DestinationLabel = aDecRefLabel };
      //  //  }
      //  //}
      //  //foreach (MethodInformation.Argument xArg in aArgs) {
      //  //  if (xArg.IsReferenceType) {
      //  //    Op.Ldarg(aAssembler,
      //  //             xArg,
      //  //             false);
      //  //    //,                                 aGetStorageSizeDelegate(xArg.ArgumentType)
      //  //    new CPUx86.Call { DestinationLabel = aDecRefLabel };
      //  //  }
      //  //}
      //  // todo: add GC code
      //  new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
      //}
      if (aMethod.MethodAssembler == null && aMethod.PlugMethod == null) {
        var xBody = aMethod.MethodBase.GetMethodBody();
        if (xBody != null) {
          for (int j = xBody.LocalVariables.Count - 1; j >= 0; j--) {
            int xLocalSize = (int)ILOp.Align(ILOp.SizeOfType(xBody.LocalVariables[j].LocalType), 4);
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = (uint)xLocalSize };
          }
        }
      }
      //new CPUx86.Add(CPUx86.Registers_Old.ESP, "0x4");
      //new CPUx86.Compare { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
      //new CPUx86.ConditionalJump { Condition = ConditionalTestEnum.Equal, DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + EndOfMethodLabelNameException + "__2" };
      //new CPUx86.Xchg { DestinationReg = Registers.BX, SourceReg = Registers.BX };
      //new CPUx86.Halt();
      // TODO: not nice coding, still a test
      new CPUx86.Move { DestinationReg = Registers.ESP, SourceReg = Registers.EBP };

      new Label(MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + EndOfMethodLabelNameException + "__2");
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBP };
      var xRetSize = ((int)xTotalArgsSize) - ((int)xReturnSize);
      if (xRetSize < 0) {
        xRetSize = 0;
      }
      WriteDebug(aMethod.MethodBase, (uint)xRetSize, IL.Call.GetStackSizeToReservate(aMethod.MethodBase));
      new CPUx86.Return { DestinationValue = (uint)xRetSize };
    }

      

    private static HashSet<string> mDebugLines = new HashSet<string>();
    private static void WriteDebug(MethodBase aMethod, uint aSize, uint aSize2) {
      var xLine = String.Format("{0}\t{1}\t{2}", MethodInfoLabelGenerator.GenerateFullName(aMethod), aSize, aSize2);
      if (!mDebugLines.Contains(xLine)) {
        mDebugLines.Add(xLine);
        File.AppendAllText(@"e:\tempdebug.txt", xLine + "\r\n");
      }
    }

    static AssemblerNasm() {
      if (File.Exists(@"e:\tempdebug.txt")) {
        File.Delete(@"e:\tempdebug.txt");
      }
    }

    protected override void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode) {
      base.BeforeOp(aMethod, aOpCode);
      new Label(TmpPosLabel(aMethod, aOpCode));
    }

    protected override void AfterOp(MethodInfo aMethod, ILOpCode aOpCode) {
      base.AfterOp(aMethod, aOpCode);
      var xContents = "";
      foreach (var xStackItem in Stack) {
        xContents += ILOp.Align((uint)xStackItem.Size, 4);
        xContents += ", ";
      }
      if (xContents.EndsWith(", ")) {
        xContents = xContents.Substring(0, xContents.Length - 2);
      }
      new Comment("Stack contains " + Stack.Count + " items: (" + xContents + ")");
    }

    // These are all temp functions until we move to the new assembler.
    // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
    public static string TmpPosLabel(MethodInfo aMethod, int aOffset) {
      // todo: fix to be small again. 
      return ILOp.GetLabel(aMethod, aOffset);
      //TODO: Change to Hex output, will be smaller and slightly faster for NASM      
      //return "POS_" + aMethod.UID + "_" + aOffset;
    }

    public static string TmpPosLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return TmpPosLabel(aMethod, aOpCode.Position);
    }

    public static string TmpBranchLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return TmpPosLabel(aMethod, ((ILOpCodes.OpBranch)aOpCode).Value);
    }

  }
}
