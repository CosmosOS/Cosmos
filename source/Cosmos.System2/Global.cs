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
    /// <summary>
    /// Cosmos global class.
    /// Used to init the console, screen and debugger and get/set keyboard keys.
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Create new instance of the <see cref="Global"/> class.
        /// </summary>
        static Global()
        {

        }

        /// <summary>
        /// System ring debugger instance, with the tag "Global".
        /// </summary>
        public static readonly Debugger mDebugger = DebuggerFactory.CreateDebugger("System", "Global");

        /// <summary>
        /// System ring debugger instance, with the tag "FileSystem".
        /// </summary>
        public static readonly Debugger mFileSystemDebugger = DebuggerFactory.CreateDebugger("System", "FileSystem");

        /// <summary>
        /// Console instance.
        /// </summary>
        public static Console Console;

        /// <summary>
        /// Get and set keyboard NumLock value.
        /// </summary>
        public static bool NumLock
        {
            get { return KeyboardManager.NumLock; }
            set { KeyboardManager.NumLock = value; }
        }

        /// <summary>
        /// Get and set keyboard CapsLock value.
        /// </summary>
        public static bool CapsLock
        {
            get { return KeyboardManager.CapsLock; }
            set { KeyboardManager.CapsLock = value; }
        }

        /// <summary>
        /// Get and set keyboard ScrollLock value.
        /// </summary>
        public static bool ScrollLock
        {
            get { return KeyboardManager.ScrollLock; }
            set { KeyboardManager.ScrollLock = value; }
        }

        // TODO: continue adding exceptions to the list, as HAL and Core would be documented.
        /// <summary>
        /// Init console, screen and keyboard.
        /// </summary>
        /// <param name="textScreen">A screen device.</param>
        public static void Init(TextScreenBase textScreen, bool InitScroolWheel = true, bool InitPS2 = true, bool InitNetwork = true, bool IDEInit = true)
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
            HAL.Global.Init(textScreen, InitScroolWheel, InitPS2, InitNetwork, IDEInit);

            Network.NetworkStack.Init();
            mDebugger.Send("Network Stack Init");

            NumLock = false;
            CapsLock = false;
            ScrollLock = false;
        }

        /// <summary>
        /// Change keyboard layout. Initially set to US_Standard.
        /// <para>
        /// Currently available:
        /// <list type="bullet">
        /// <item>US_Standard.</item>
        /// <item>FR_Standard.</item>
        /// <item>DE_Standard.</item>
        /// <item>TR_StandardQ.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="scanMap">A key mapping.</param>
        public static void ChangeKeyLayout(ScanMapBase scanMap)
        {
            KeyboardManager.SetKeyLayout(scanMap);
        }
    }
}
