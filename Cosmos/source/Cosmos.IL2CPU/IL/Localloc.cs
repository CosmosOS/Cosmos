using System;
using System.Collections.Generic;
using System.IO;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using System.Reflection;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;



namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Localloc)]
  public class Localloc : ILOp
  {
    public Localloc(Cosmos.Assembler.Assembler aAsmblr)
      : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      //TODO: free heap in method footer.
      string xCurrentMethodLabel = GetLabel(aMethod, aOpCode);
      IL.Call.DoExecute(Assembler, aMethod, GCImplementationRefs.AllocNewObjectRef, aOpCode, xCurrentMethodLabel, DebugEnabled);
    }
  }
}
