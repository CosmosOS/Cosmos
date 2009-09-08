using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    public class AssemblerBin : CosmosAssembler
    {
      
    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

    protected override void MethodBegin(MethodInfo aMethod) {
      base.MethodBegin(aMethod);
    }

    protected override void MethodEnd(MethodInfo aMethod) {
      base.MethodEnd(aMethod);
    }

    public AssemblerBin() : base(0) { } 
  }
}
