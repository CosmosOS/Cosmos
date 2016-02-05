using System;

namespace Cosmos.IL2CPU.Plugs.System {
  [Plug(Target=typeof(OutOfMemoryException))]
  public static class OutOfMemoryExceptionImpl {
    public static void Ctor(OutOfMemoryException aThis) {
      //
    }
  }
}
