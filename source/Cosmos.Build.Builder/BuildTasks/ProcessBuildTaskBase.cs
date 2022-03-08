using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Cosmos.Build.Builder.BuildTasks
{
    internal abstract class ProcessBuildTaskBase : IBuildTask
    {
        public abstract string Name { get; }

        private readonly bool _waitForExit;
        private readonly bool _createWindow;
        private static List<string> Lines = new List<string>();

        protected ProcessBuildTaskBase(bool waitForExit, bool createWindow)
        {
            _waitForExit = waitForExit;
            _createWindow = createWindow;
        }

        public Task RunAsync(ILogger logger)
        {
            var exePath = GetExePath();
            var args = GetArguments();

            logger.LogMessage($"\"{exePath}\" {args}");

            Lines.Clear();
            var process = new Process();
            var processStartInfo = new ProcessStartInfo(exePath, args);

            processStartInfo.CreateNoWindow = !_createWindow;
            processStartInfo.UseShellExecute = false;

            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.StandardErrorEncoding = Encoding.GetEncoding(858);
            processStartInfo.StandardOutputEncoding = Encoding.GetEncoding(858);

            process.StartInfo = processStartInfo;
            process.Start();

            if (_waitForExit)
            {
                var standardOutputReaderTask = Task.Run(() => ReadOutputAsync(process.StandardOutput, logger));
                var standardErrorReaderTask = Task.Run(() => ReadOutputAsync(process.StandardError, logger));

                var waitForExitTask = Task.Run(() => WaitForExit(process));

                return Task.WhenAll(waitForExitTask, standardOutputReaderTask, standardErrorReaderTask);
            }

            return Task.CompletedTask;
        }

        protected abstract string GetExePath();
        protected abstract string GetArguments();

        private static void WaitForExit(Process process)
        {
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                if (Lines.Count == 0)
                {
                    throw new Exception("The process failed to execute!");
                }
                else
                {
                    string error = "";
                    foreach (var item in Lines)
                    {
                        if (item.ToLower().Contains("error"))
                        {
                            error += item + "\n";
                        }
                    }
                    if (error == "")
                    {
                        error = Lines[Lines.Count - 1];
                    }
                    throw new Exception("The process failed to execute!\nError: \n"+error);
                }
            }
        }

        private static async Task ReadOutputAsync(StreamReader reader, ILogger logger)
        {
            while (true)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                         new Action(delegate { }));
                var line = await reader.ReadLineAsync().ConfigureAwait(false);

                if (line == null)
                {
                    return;
                }
                Lines.Add(line);
                logger.LogMessage(line);
            }
        }
    }
}
