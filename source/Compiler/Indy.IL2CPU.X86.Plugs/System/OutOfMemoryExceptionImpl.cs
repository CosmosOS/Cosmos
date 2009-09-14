using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.X86.Plugs.System {
  [Plug(Target=typeof(OutOfMemoryException))]
  public static class OutOfMemoryExceptionImpl {
    public static void Ctor(OutOfMemoryException aThis) {
      //
    }
  }
}
