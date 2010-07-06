using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;

namespace Cosmos.Debug.HostProcess
{
    public class Program
    {
        static int Main(string[] args)
        {
            // This is here to allow this process to start, but pause till the caller tells it to continue. 
            Console.ReadLine();

            // Arg[0] - ShellExecute
            bool xShellExecute = (string.Compare(args[0], "true", true) == 0);

            // Target exe or file
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = args[1];
            var xArgSB = new StringBuilder();

            // Skip other arguments and "rearg" the rest
            foreach (var arg in args.Skip(2))
            {
                xArgSB.AppendFormat("\"{0}\" ", arg);
            }
            xStartInfo.Arguments = xArgSB.ToString();
            xStartInfo.RedirectStandardError = !xShellExecute;
            xStartInfo.RedirectStandardOutput = !xShellExecute;
            xStartInfo.UseShellExecute = xShellExecute;
            var xProcess = Process.Start(xStartInfo);

            xProcess.WaitForExit();
            if (!xShellExecute) {
                Console.WriteLine(xProcess.StandardError.ReadToEnd());
                Console.WriteLine(xProcess.StandardOutput.ReadToEnd());
            }
            return xProcess.ExitCode;
        }
    }
}
