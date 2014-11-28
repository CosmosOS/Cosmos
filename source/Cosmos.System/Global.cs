using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
  static public class Global {
    static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
    static public Console Console = new Console();

    static public void Init() {
      // We must init Console before calling Inits. This is part of the 
      // "minimal" boot to allow output
      Global.Dbg.Send("Creating Console");
//        Console;

      Global.Dbg.Send("HW Init");
      Cosmos.HAL.Global.Init();
      //Network.NetworkStack.Init();
    }
  }
}
