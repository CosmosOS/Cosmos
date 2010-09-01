using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;

namespace Cosmos.System {
    // MtW: if the fullname (namespace + name) of this class changes, please also change IL2CPU msbuild task
    public abstract class Kernel {
        public readonly Debug.Kernel.Debugger Dbg = new Debug.Kernel.Debugger("User", "");

        public bool ClearScreen = true;

        // Set after initial start. Can be started and stopped at same time
        protected bool mStarted = false;
        // Set to signal stopped
        protected bool mStopped = false;

        // Start the system up using the properties for configuration.
        public void Start() {
            Global.Dbg.Send("Starting kernel");
            if (mStarted) {
                Global.Dbg.Send("ERROR: Kernel Already Started");
                throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
            }
            mStarted = true;

            Hardware.Bootstrap.Init();
            Global.Init();

            // Provide the user with a clear screen if they requested it
            if (ClearScreen) {
                Global.Console.Clear();
            }

            BeforeRun();
            while (!mStopped) {
                Run();
            }
            AfterRun();
            while (true) {
            }
        }

        protected virtual void BeforeRun() { }
        protected abstract void Run();
        protected virtual void AfterRun() { }

        // Shut down the system and power off
        public void Stop() {
            mStopped = true;
        }

        // Shutdown and restart
        public void Restart()
        {
        }
    }
}
