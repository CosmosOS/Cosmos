using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System {
  static public class Global {
    static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
    static public Console Console = new Console(null);

    static public void Init(TextScreenBase textScreen){
      // We must init Console before calling Inits. This is part of the 
      // "minimal" boot to allow output
      Global.Dbg.Send("Creating Console");
      if (textScreen != null)
      {
        Console = new Console(textScreen);
      }

      Global.Dbg.Send("HW Init");
      Cosmos.HAL.Global.Init(textScreen);
      //Network.NetworkStack.Init();
    }
  }
}
