using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Threading {
  //TODO: We dont support threading at all, but a lot of .NET calls these
  // and therefore interferes with us using them. Since we don't support threading
  // currently we can just ignore them by creating empty plugs.
  [Plug(Target = typeof(global::System.Threading.Monitor))]
  public class MonitorImpl {
    public static void Enter(object aObj) {
    }
    
    public static void Exit(object aObj) {
    }
    
    public static void ReliableEnter(object obj, ref bool tookLock) {
      tookLock = true;
    }
  }
}
