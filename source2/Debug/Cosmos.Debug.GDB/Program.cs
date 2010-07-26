using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    static class Program {
        [STAThread]
        static void Main() {
            var xCLine = System.Environment.GetCommandLineArgs();
            Settings.Filename = xCLine[1];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
