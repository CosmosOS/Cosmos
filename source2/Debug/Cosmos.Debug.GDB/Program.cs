using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    static class Program {
        [STAThread]
        static void Main() {
            var xArgs = System.Environment.GetCommandLineArgs();
            if (xArgs.Length > 1) {
                Settings.Load(xArgs[1]);
            }
            if (xArgs.Length > 2) {
                //Settings.AutoConnect = string.Compare(xArgs[2], "/Connect", true) == 0;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
