using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Assembler {
  public abstract class Code {
    protected Assembler mAssembler;
    public abstract void Assemble();

    public Code(Assembler aAssembler) {
      mAssembler = aAssembler;
    }

    // Assembles all descendants of this class in specified assembly.
    static public void Assemble(Assembler aAssembler, Assembly aAssembly) {
      foreach (var xType in aAssembly.GetTypes()) {
        if (xType.IsSubclassOf(typeof(Code))) {
          var xCtor = xType.GetConstructor(new Type[] { typeof(Assembler) });
          var xCode = (Code)(xCtor.Invoke(new Object[] { aAssembler }));
          xCode.Assemble();
        }
      }
    }
  }
}
