using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    // MtW: if the fullname (namespace + name) of this class changes, please also change IL2CPU msbuild task
    public abstract class Kernel {
        public readonly Debug.Kernel.Debugger Dbg = new Debug.Kernel.Debugger("User", "");

        public bool ClearScreen = true;
        // Set to true to hide messages during boot.
        public bool Silent = false;

        // Set after initial start. Can be started and stopped at same time
        protected bool mStarted = false;
        // Set to signal stopped
        protected bool mStopped = false;

        // Start the system up using the properties for configuration.
        public void Start() {
            Global.Dbg.Send("Starting kernel");
            if (mStarted)
            {
                Global.Dbg.Send("ERROR: Kernel Already Started");
                throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
            }
            mStarted = true;

            //TODO - Set and document the Console class (and its supporting classes) to default to 80x25
            //Hardware.VGAScreen.SetTextMode(VGAScreen.TextSize.Size80x25);

            //TODO: System inits hardware, and hardware inits core
            Global.Init();

            // Clear before booting
            Global.Dbg.Send("Clearing screen");
            Global.Console.Clear();
            WriteLine("Cosmos kernel boot initiated.");

            WriteLine("Cosmos kernel boot completed.");
            // Provide the user with a clear scree if they requested it
            if (ClearScreen)
            {
                Global.Console.Clear();
            }

            BeforeRun();
            while (!mStopped)
            {
                Run();
            }
            AfterRun();
            while (true)
                ;
        }

        protected virtual void BeforeRun() { }
        protected abstract void Run();
        protected virtual void AfterRun() { }

        protected void WriteLine(string aMsg) {
            if (!Silent) {
                Global.Console.WriteLine(aMsg);
            }
        }

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
