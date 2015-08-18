using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System
{
    public static class Global
    {
        public static readonly Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
        public static Console Console = new Console(null);

        public static bool NumLock
        {
            get { return HAL.Global.NumLock; }
            set { HAL.Global.NumLock = value; }
        }

        public static bool CapsLock
        {
            get { return HAL.Global.CapsLock; }
            set { HAL.Global.CapsLock = value; }
        }

        public static bool ScrollLock
        {
            get { return HAL.Global.ScrollLock; }
            set { HAL.Global.ScrollLock = value; }
        }

        public static void Init(TextScreenBase textScreen, Keyboard keyboard)
        {
            // We must init Console before calling Inits. This is part of the
            // "minimal" boot to allow output
            Global.Dbg.Send("Creating Console");
            if (textScreen != null)
            {
                Console = new Console(textScreen);
            }

            Global.Dbg.Send("HW Init");
            Cosmos.HAL.Global.Init(textScreen, keyboard);
            NumLock = false;
            CapsLock = false;
            ScrollLock = false;
            //Network.NetworkStack.Init();
        }
    }
}
