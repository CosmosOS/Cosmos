using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSUtilities = Cosmos.VS.DebugEngine.Utilities.VS;

namespace Cosmos.VS.DebugEngine
{
    public static class AD7Util
    {
        public static void Log(string message, params object[] args)
        {
            // this method doesn't do anything normally, but keep it for debugging
            //  File.AppendAllText(@"c:\data\sources\ad7.log", DateTime.Now.ToString("HH:mm:ss.ffffff: ") + String.Format(message, args) + Environment.NewLine);
        }

        public static async Task ShowMessageAsync(string message, string title = "Cosmos Debug Engine")
        {
            await VSUtilities.MessageBox.ShowAsync(title, message);
        }

        public static void ShowMessage(string message, string title = "Cosmos Debug Engine")
        {
            VSUtilities.MessageBox.Show(title, message);
        }

        public static async Task ShowWarningAsync(string message, string title = "Cosmos Debug Engine")
        {
            await VSUtilities.MessageBox.ShowWarningAsync(title, message);
        }

        public static void ShowWarning(string message, string title = "Cosmos Debug Engine")
        {
            VSUtilities.MessageBox.ShowWarning(title, message);
        }

        public static async Task ShowErrorAsync(string message, string title = "Cosmos Debug Engine")
        {
            await VSUtilities.MessageBox.ShowErrorAsync(title, message);
        }

        public static void ShowError(string message, string title = "Cosmos Debug Engine")
        {
            VSUtilities.MessageBox.ShowError(title, message);
        }
    }
}
