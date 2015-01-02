using System;
using Cosmos.HAL;

namespace Cosmos.System
{
    // MtW: if the fullname (namespace + name) of this class changes, please also change IL2CPU msbuild task
    /// <summary>
    /// Provides a base kernel class for a Cosmos-based system
    /// </summary>
    public abstract class Kernel
    {
        public readonly Debug.Kernel.Debugger Dbg = new Debug.Kernel.Debugger("User", "");

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

        /// <summary>
        /// Start the system up using the properties for configuration.
        /// </summary>
        public virtual void Start() {
            try {
                Global.Dbg.Send("Starting kernel");
                if (mStarted) {
                    Global.Dbg.Send("ERROR: Kernel Already Started");
                    throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
                }
                mStarted = true;

                if (String.Empty == null) {
                    throw new Exception("Compiler didn't initialize System.String.Empty!");
                }

                Global.Dbg.Send("HW Bootstrap Init");
                HAL.Bootstrap.Init();

                Global.Dbg.Send("Global Init");
                Global.Init(GetTextScreen());

                // Provide the user with a clear screen if they requested it
                if (ClearScreen) {
                    Global.Dbg.Send("Cls");
                    Global.Console.Clear();
                }

                Global.Dbg.Send("Before Run");
                BeforeRun();

                Global.Dbg.Send("Run");
                while (!mStopped) {
                    //Network.NetworkStack.Update();
                    Run();
                }

                AfterRun();
                //bool xTest = 1 != 3;
                //while (xTest) {
                //}
            }
            catch (Exception E) {
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
        public void Stop() {
            mStopped = true;
        }

        public Kernel() { }

        // Shutdown and restart
        public void Restart() {
        }

        public static void PrintDebug(string message)
        {
            Global.Dbg.Send(message);
        }
    }
}
