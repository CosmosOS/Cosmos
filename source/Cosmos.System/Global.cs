using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System {
  static public class Global {
    static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
    static public Console Console = new Console(null);

    static public void Init(TextScreenBase textScreen, Keyboard keyboard){
      // We must init Console before calling Inits. This is part of the
      // "minimal" boot to allow output
      Global.Dbg.Send("Creating Console");
      if (textScreen != null)
      {
        Console = new Console(textScreen);
      }
        var x = new String('a', 1);
      Global.Dbg.Send("HW Init");
            x = new String('a', 1);
            Cosmos.HAL.Global.Init(textScreen, keyboard);
            x = new String('a', 1);
            Global.Dbg.Send("After HW Init");
        //Network.NetworkStack.Init();
    }
  }
}
