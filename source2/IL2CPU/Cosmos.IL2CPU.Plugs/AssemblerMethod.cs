using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.IL2CPU.Plugs {
    public abstract class AssemblerMethod
    {
      public abstract void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo);
    }
}