using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.System {
  [Plug(Target = typeof(EncoderFallbackImpl))]
  public static class EncoderFallbackImpl {
    // Encoders use this, but we plug their methods anwyays so we just fill empty for now.
    public static object get_InternalSyncObject() {
      return new object();
    }
  }
}
