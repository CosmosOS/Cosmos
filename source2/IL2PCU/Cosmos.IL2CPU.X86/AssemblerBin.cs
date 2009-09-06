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
    public AssemblerBin() : base( 0 ) { } 
  }
}
