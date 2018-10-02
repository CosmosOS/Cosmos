using System.Diagnostics;

namespace Cosmos.Build.Common
{
    public static class ProcessExtension
    {
        public static string LaunchApplication(string app, string args, bool waitForExit)
        {
            var start = new ProcessStartInfo();
            start.FileName = app;
            start.Arguments = args;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            var process = Process.Start(start);

            if (waitForExit)
            {
                var output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                var error = process.StandardError.ReadToEnd();
                return output + error;
            }

            return string.Empty;
        }
    }
}
