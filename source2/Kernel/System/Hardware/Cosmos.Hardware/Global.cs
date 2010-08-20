using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {
    static public class Global {
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Hardware", "");

        static public Keyboard Keyboard = new Keyboard();
        static public PIT PIT = new PIT();
        static public TextScreen TextScreen = new TextScreen();

        static public void Init() {
            Global.Dbg.Send("Cosmos.Hardware.Global.Init");
            Cosmos.Core.Global.Init();
        }

    }
}
