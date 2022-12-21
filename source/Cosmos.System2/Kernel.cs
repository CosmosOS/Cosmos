using System;
using sysIO = System.IO;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.ScanMaps;

namespace Cosmos.System
{
    // MtW: if the fullname (namespace + name) of this class changes, please also change IL2CPU msbuild task
    /// <summary>
    /// Provides a basic kernel class for a Cosmos-based system
    /// </summary>
    public abstract class Kernel
    {
        /// <summary>
        /// Provides kernel debugger in User ring
        /// </summary>
        public readonly Debugger mDebugger = new Debugger("User", "Kernel");

        /// <summary>
        /// Specifies if screen should be cleared on startup.
        /// </summary>
        public bool ClearScreen = true;

        // Set after initial start. Can be started and stopped at same time
        /// <summary>
        /// Set on true, if kernel is started.
        /// </summary>
        protected bool mStarted = false;
        // Set to signal stopped
        /// <summary>
        /// Set on true, if kernel is stopped.
        /// </summary>
        protected bool mStopped = false;

        /// <summary>
        /// Returns current text screen device. Set to null by default
        /// </summary>
        /// <returns>null</returns>
        protected virtual TextScreenBase GetTextScreen()
        {
            // null means use default
            return null;
        }

        /// <summary>
        /// Returns currently set keyboard scan map.
        /// </summary>
        /// <returns>Currently set keyboard scan map.</returns>
        protected ScanMapBase GetKeyboardScanMap()
        {
            return KeyboardManager.GetKeyLayout();
        }

        /// <summary>
        /// Sets new keyboard scan map. Basically, switches keyboard language
        /// </summary>
        /// <param name="ScanMap">New keyboard scan map.</param>
        protected void SetKeyboardScanMap(ScanMapBase ScanMap)
        {
            KeyboardManager.SetKeyLayout(ScanMap);
        }

        /// <summary>
        /// Cosmos entrypoint. Initializes hardware and gives control to user kernel. Shouldn't be called from user code
        /// </summary>
        /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>
        public virtual void Start()
        {
            try
            {
                Global.mDebugger.Send("Starting Cosmos...");
                if (mStarted)
                {
                    Global.mDebugger.Send("Error: Kernel has already been started");
                    throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
                }
                mStarted = true;

                if (string.Empty == null)
                {
                    Global.mDebugger.Send("Error: Kernel has already been started");
                    throw new Exception("Error: Compiler didn't initialize System.String.Empty!");
                }

                Global.mDebugger.Send("Initializing HAL...");
                HAL.Bootstrap.Init();
                Global.mDebugger.Send("Initializing hardware...");
                OnBoot();
                // Provide the user with a clear screen if they requested it
                if (ClearScreen)
                {
                    Global.mDebugger.Send("Clearing screen...");
                    Console.Clear();
                }

                // now enable interrupts:
                HAL.Global.EnableInterrupts();

                Global.mDebugger.Send("Executing BeforeRun...");
                BeforeRun();

                Global.mDebugger.Send("Executing Run...");
                if (mStopped)
                {
                    Global.mDebugger.Send("Kernel already stopped!");
                }
                while (!mStopped)
                {
                    //Network.NetworkStack.Update();
                    Run();
                }
                Global.mDebugger.Send("Executing AfterRun...");
                AfterRun();

                Global.mDebugger.Send("Shutting down...");
                Power.Shutdown();
            }
            catch (Exception E)
            {
                // todo: better ways to handle?
                global::System.Console.WriteLine("An exception occurred while running kernel:");
                global::System.Console.WriteLine(E.ToString());
                while (true){}
            }
        }

        /// <summary>
        /// Called on boot and initializes Cosmos, hardware. Can be overriden and be made to load another drivers
        /// </summary>
        protected virtual void OnBoot() {
            Global.Init(GetTextScreen());
        }
        /// <summary>
        /// Your pre-run code. Will be called once and before Run method
        /// </summary>
        protected virtual void BeforeRun() { }

        /// <summary>
        /// Your kernel main loop code. Will be called in loop, virtually infinity times.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Your kernel shutdown code. Will be called once mStopped will be equal to true
        /// </summary>
        protected virtual void AfterRun() { }

        /// <summary>
        /// Shutdown shortcut. Stops kernel by setting mStopped to true
        /// </summary>
        public void Stop()
        {
            mStopped = true;
        }

        /// <summary>
        /// Basic constructor for kernel.
        /// </summary>
        public Kernel()
        {
            Global.mDebugger.Send("In Cosmos.System.Kernel..ctor");
        }

        // Shutdown and restart
        /// <summary>
        /// Restarts system. Doesn't call AfterRun, be aware
        /// </summary>
        public void Restart()
        {
            Power.Reboot();
        }

        /// <summary>
        /// Print message to the debbuger at system ring with "Global"-tag.
        /// </summary>
        /// <param name="message">A message to print.</param>
        public static void PrintDebug(string message)
        {
            Global.mDebugger.Send(message);
        }

        /// <summary>
        /// Gets current CPU interrupts status.
        /// </summary>
        public static bool InterruptsEnabled
        {
            get
            {
                return HAL.Global.InterruptsEnabled;
            }
        }
    }
}
