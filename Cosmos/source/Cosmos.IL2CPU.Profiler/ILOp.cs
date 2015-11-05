﻿using System;

namespace Cosmos.IL2CPU.Profiler {
  public class ILOp : Cosmos.IL2CPU.ILOp {
    public ILOp(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr) {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      // Do Nothing
    }
  }
}
