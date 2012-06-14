using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Assembler {
  public abstract class Code {
    public abstract void Assemble();

    // Assembles all descendants of this class in specified assembly.
    static public void Assemble(Assembly aAssembly) {
      foreach (var xType in aAssembly.GetTypes()) {
        if (xType.IsSubclassOf(typeof(Code))) {
          var xCtor = xType.GetConstructor(new Type[0]);
          var xCode = (Code)(xCtor.Invoke(new Object[0]));
          xCode.Assemble();
        }
      }
    }
  }
}
