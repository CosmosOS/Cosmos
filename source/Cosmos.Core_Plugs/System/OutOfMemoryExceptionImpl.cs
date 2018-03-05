using System;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System {
  [Plug(Target=typeof(OutOfMemoryException))]
  public static class OutOfMemoryExceptionImpl {
    public static void Ctor(OutOfMemoryException aThis) {
      //
    }
  }
}
