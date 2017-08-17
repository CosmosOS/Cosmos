using Environment = System.Environment;
using STAThread = System.STAThreadAttribute;
using Application = System.Windows.Forms.Application;

namespace Cosmos.Debug.GDB {
    static class Program {
        [STAThread]
        static void Main() {
            var xArgs = Environment.GetCommandLineArgs();
            if (xArgs.Length > 1) {
				if (false == Settings.Load(xArgs[1]))
					return;
            }
            if (xArgs.Length > 2) {
                Settings.AutoConnect = string.Compare(xArgs[2], "/Connect", true) == 0;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}