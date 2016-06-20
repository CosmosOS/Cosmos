using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Cosmos.Debug.Kernel;
using Cosmos.HAL;

using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.System
{
    public static class Global
    {
        static Global()
        {

        }
        public static readonly Debugger mDebugger = new Debugger("System", "Global");

        public static readonly Debugger mFileSystemDebugger = new Debugger("System", "FileSystem");

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
            // We must init Console before calling Inits.
            // This is part of the "minimal" boot to allow output.
            mDebugger.Send("Creating Console");
            if (textScreen != null)
            {
                Console = new Console(textScreen);
            }

            mDebugger.Send("HW Init");
            HAL.Global.Init(textScreen, keyboard);
            NumLock = false;
            CapsLock = false;
            ScrollLock = false;
            //Network.NetworkStack.Init();
        }
    }
}
