using System;
using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Plugs.System {
  [Plug(Target=typeof(OutOfMemoryException))]
  public static class OutOfMemoryExceptionImpl {
    public static void Ctor(OutOfMemoryException aThis) {
      //
    }
  }
}
