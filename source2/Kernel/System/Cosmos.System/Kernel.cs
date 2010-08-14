using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    public class Kernel {
        public bool ClearScreen = true;
        // Set to true to hide messages during boot.
        public bool Silent = false;

        // Start the system up using the properties for configuration.
        public void Start() {
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
        }

        protected void WriteLine(string aMsg) {
            if (!Silent) {
                Console.WriteLine(aMsg);
            }
        }

        // Shut down the system and power off
        static public void Stop() {
        }

        // Shutdown and restart
        static public void Restart() {
        }
    }
}
