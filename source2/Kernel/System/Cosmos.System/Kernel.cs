using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    public abstract class Kernel {
        public bool ClearScreen = true;
        // Set to true to hide messages during boot.
        public bool Silent = false;

        // Set after initial start. Can be started and stopped at same time
        protected bool mStarted = false;
        // Set to signal stopped
        protected bool mStopped = false;

        // Start the system up using the properties for configuration.
        public void Start() {
            if (mStarted) {
                throw new Exception("Kernel has already been started. A kernel cannot be started twice.");
            }
            mStarted = true;

            //TODO - Set and document the Console class (and its supporting classes) to default to 80x25
            //Hardware.VGAScreen.SetTextMode(VGAScreen.TextSize.Size80x25);

            // Clear before booting
            Console.Clear();
            WriteLine("Cosmos kernel boot initiated.");

            //TODO: System inits hardware, and hardware inits core
            Global.Init();

            WriteLine("Cosmos kernel boot completed.");
            // Provide the user with a clear scree if they requested it
            if (ClearScreen) {
                Console.Clear();
            }

            BeforeRun();
            while (!mStopped) {
                Run();
            }
            AfterRun();
        }

        protected virtual void BeforeRun() { }
        protected abstract void Run();
        protected virtual void AfterRun() { }

        protected void WriteLine(string aMsg) {
            if (!Silent) {
                Console.WriteLine(aMsg);
            }
        }

        // Shut down the system and power off
        public void Stop() {
            mStopped = true;
        }

        // Shutdown and restart
        public void Restart() {
        }
    }
}
