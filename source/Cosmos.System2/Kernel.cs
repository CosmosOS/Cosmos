using System;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.ScanMaps;

namespace Cosmos.System
{
    // MtW: if the fullname (namespace + name) of this class changes, please also change IL2CPU msbuild task
    /// <summary>
    /// Provides a base kernel class for a Cosmos-based system
    /// </summary>
    public abstract class Kernel
    {
        public readonly Debugger mDebugger = new Debugger("User", "Kernel");

        public bool ClearScreen = true;

        // Set after initial start. Can be started and stopped at same time
        protected bool mStarted = false;
        // Set to signal stopped
        protected bool mStopped = false;

        protected virtual TextScreenBase GetTextScreen()
        {
            // null means use default
            return null;
        }

        protected ScanMapBase GetKeyboardScanMap()
        {
            return KeyboardManager.GetKeyLayout();
        }

        protected void SetKeyboardScanMap(ScanMapBase ScanMap)
        {
            KeyboardManager.SetKeyLayout(ScanMap);
        }

        

        /// <summary>
        /// Start the system up using the properties for configuration.
        /// </summary>
        public virtual void Start()
        {
            try
            {
                HAL.Global.logs.Add("Aura Operating System v0.4.3 - Created by Valentin Charbonnier and Alexy Da Cruz");
                HAL.Global.logs.Add("[Info]  Starting Cosmos kernel..");
                Global.mDebugger.Send("Starting kernel");
                if (mStarted)
                {
                    HAL.Global.logs.Add("[Error] Kernel Already Started");
                    Global.mDebugger.Send("ERROR: Kernel Already Started");
                    throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
                }
                mStarted = true;

                if (string.Empty == null)
                {
                    HAL.Global.logs.Add("[Error] Compiler didn't initialize System.String.Empty!");
                    throw new Exception("Compiler didn't initialize System.String.Empty!");
                }

                Global.mDebugger.Send("HW Bootstrap Init");
                HAL.Bootstrap.Init();
                HAL.Global.logs.Add("[ OK ] HW Bootstrap Initialized");

                Global.mDebugger.Send("Global Init");
                Global.Init(GetTextScreen());
                HAL.Global.logs.Add("[ OK ] Global Initialized");

                //Start with a PS2Keyboard
                KeyboardManager.AddKeyboard(new PS2Keyboard());
                HAL.Global.logs.Add("[ OK ] PS2 Keyboard added");

                // Provide the user with a clear screen if they requested it
                if (ClearScreen)
                {
                    Global.mDebugger.Send("Cls");
                    //Global.Console.Clear();
                }

                Global.mDebugger.Send("Before Run");
                BeforeRun();
                HAL.Global.logs.Add("[ OK ] Before Run");

                // now enable interrupts:
                HAL.Global.EnableInterrupts();
                HAL.Global.logs.Add("[ OK ] Interrupts Enabled");

                Global.mDebugger.Send("Run");
                if (mStopped)
                {
                    Global.mDebugger.Send("Already stopped");
                }
                else
                {
                    Global.mDebugger.Send("Not yet stopped");
                }
                while (!mStopped)
                {
                    //Network.NetworkStack.Update();
                    Global.mDebugger.Send("Really before Run");
                    Run();
                    Global.mDebugger.Send("Really after Run");
                }
                Global.mDebugger.Send("AfterRun");
                AfterRun();
                //bool xTest = 1 != 3;
                //while (xTest) {
                //}
            }
            catch (Exception E)
            {
                // todo: better ways to handle?
                global::System.Console.WriteLine("Exception occurred while running kernel:");
                global::System.Console.WriteLine(E.ToString());
            }
        }

        /// <summary>
        /// Pre-run events
        /// </summary>
        protected virtual void BeforeRun() { }

        /// <summary>
        /// Main kernel loop
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// After the Run() method is exited (?)
        /// </summary>
        protected virtual void AfterRun() { }

        /// <summary>
        /// Shut down the system and power off
        /// </summary>
        public void Stop()
        {
            mStopped = true;
        }

        public Kernel()
        {
            Global.mDebugger.Send("In Cosmos.System.Kernel..ctor");
        }

        // Shutdown and restart
        public void Restart()
        {
        }

        public static void PrintDebug(string message)
        {
            Global.mDebugger.Send(message);
        }

        public static bool InterruptsEnabled
        {
            get
            {
                return HAL.Global.InterruptsEnabled;
            }
        }
    }
}
