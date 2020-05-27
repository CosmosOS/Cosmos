using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.DebugEngine
{
    public static class AD7Util
    {
        public static void Log(string message, params object[] args)
        {
            // this method doesn't do anything normally, but keep it for debugging
            //  File.AppendAllText(@"c:\data\sources\ad7.log", DateTime.Now.ToString("HH:mm:ss.ffffff: ") + String.Format(message, args) + Environment.NewLine);
        }

        public static void MessageBox(string message, string title = "Cosmos Debug Engine")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, message, title,
                OLEMSGICON.OLEMSGICON_NOICON, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
