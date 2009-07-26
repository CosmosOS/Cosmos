using System;

namespace Cosmos.IL2CPU.Profiler {
  public class ILOp : Cosmos.IL2CPU.ILOp {
    public ILOp(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      // Do Nothing
    }
  }
}
