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
        public static readonly Debugger mDebugger = new Debugger("System", "Global");

        public static readonly Debugger mFileSystemDebugger = new Debugger("System", "FileSystem");

        public static Console Console = new Console(null);

        public static Keyboard Keyboard;

        public static bool NumLock
        {
            get { return Keyboard.NumLock; }
            set { Keyboard.NumLock = value; }
        }

        public static bool CapsLock
        {
            get { return Keyboard.CapsLock; }
            set { Keyboard.CapsLock = value; }
        }

        public static bool ScrollLock
        {
            get { return Keyboard.ScrollLock; }
            set { Keyboard.ScrollLock = value; }
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

            mDebugger.Send("Creating Keyboard");
            if(keyboard != null)
            {
                Keyboard = keyboard;
            }
            else
            {
                Debugger.DoSend("Keyboard is null. Using default Keyboard: PS2Keyboard with US_Standard scan map.");
                Keyboard = new Keyboard(new PS2Keyboard(), new ScanMaps.US_Standard());
            }

            mDebugger.Send("HW Init");
            HAL.Global.Init(textScreen);
            NumLock = false;
            CapsLock = false;
            ScrollLock = false;
            //Network.NetworkStack.Init();
        }

        public static void ChangeKeyLayout(ScanMapBase scanMap)
        {
            if (scanMap != null && Keyboard != null)
            {
                Keyboard.SetKeyLayout(scanMap);
            }
        }
    }
}
