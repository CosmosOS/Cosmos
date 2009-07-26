using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
  public class AssemblerNasm : Assembler {

    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

    // These are all temp functions until we move to the new assembler.
    // They are used to clean up the old assembler slightly while retaining compatibiltiy for now
    public static string TmpPosLabel(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Change to Hex output, will be smaller and slightly faster for NASM
      return "_" + aMethodUID + "_" + aOpCode.Position + "__";
    }

    public static string TmpBranchLabel(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Change to Hex output, will be smaller and slightly faster for NASM
      return "_" + aMethodUID + "_" + ((ILOpCodes.OpBranch)aOpCode).Value + "__";
    }

  }
}
