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
            get { return KeyboardManager.NumLock; }
            set { KeyboardManager.NumLock = value; }
        }

        public static bool CapsLock
        {
            get { return KeyboardManager.CapsLock; }
            set { KeyboardManager.CapsLock = value; }
        }

        public static bool ScrollLock
        {
            get { return KeyboardManager.ScrollLock; }
            set { KeyboardManager.ScrollLock = value; }
        }

        public static void Init(TextScreenBase textScreen)
        {
            // We must init Console before calling Inits.
            // This is part of the "minimal" boot to allow output.
            mDebugger.Send("Creating Console");
            if (textScreen != null)
            {
                Console = new Console(textScreen);
            }

            mDebugger.Send("Creating Keyboard");

            mDebugger.Send("HW Init");
            HAL.Global.Init(textScreen);
            NumLock = false;
            CapsLock = false;
            ScrollLock = false;
            //Network.NetworkStack.Init();
        }

        public static void ChangeKeyLayout(ScanMapBase scanMap)
        {
            KeyboardManager.SetKeyLayout(scanMap);
        }
    }
}
