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
    }

    // These are all temp functions until we move to the new assembler.
    // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
    public static string TmpPosLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      //TODO: Change to Hex output, will be smaller and slightly faster for NASM
      return "_" + aMethod.UID + "_" + aOpCode.Position + "__";
    }

    public static string TmpBranchLabel(MethodInfo aMethod, ILOpCode aOpCode) {
      //TODO: Change to Hex output, will be smaller and slightly faster for NASM
      return "_" + aMethod.UID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value + "__";
    }

  }
}
