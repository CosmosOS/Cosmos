using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    public class AssemblerNasm : CosmosAssembler
    {

    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

    public AssemblerNasm() : base( 0 ) { }

    protected override void MethodBegin(MethodInfo aMethod) {
      base.MethodBegin(aMethod);
      new Label(aMethod.MethodBase);
    }

    protected override void MethodEnd(MethodInfo aMethod) {
      base.MethodEnd(aMethod);
      // TODO: This is a temp hack, disable this when we reenable real exception handling
      new Label(MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + "___EXCEPTION___EXIT");
    }

    protected override void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode) {
      base.BeforeOp(aMethod, aOpCode);
      new Label(TmpPosLabel(aMethod, aOpCode));
    }

    // These are all temp functions until we move to the new assembler.
    // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
    public static string TmpPosLabel(MethodInfo aMethod, int xOffset) {
      //TODO: Change to Hex output, will be smaller and slightly faster for NASM
      return "POS_" + aMethod.UID + "_" + xOffset;
    }

    public static string TmpPosLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return TmpPosLabel(aMethod, aOpCode.Position);
    }

    public static string TmpBranchLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      return TmpPosLabel(aMethod, ((ILOpCodes.OpBranch)aOpCode).Value);
    }

  }
}
