using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.Common.CDebugger
{
    public class Global {
        public static void Call(string aEXEPathname, string aArgLine, string aWorkDir)
        {
            Call(aEXEPathname, aArgLine, aWorkDir, true, true, false);
        }

        public static void Call(string aEXEPathname, string aArgLine, string aWorkDir, bool aElevate)
        {
            Call(aEXEPathname, aArgLine, aWorkDir, true, true, aElevate);
        }

        public static Process Call(string aEXEPathname, string aArgLine, string aWorkDir, bool aWait, bool aCapture)
        {
            return Call(aEXEPathname, aArgLine, aWorkDir, aWait, aCapture, false);
        }

        public static Process Call(string aEXEPathname, string aArgLine, string aWorkDir, bool aWait, bool aCapture, bool aElevate)
        {
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = aEXEPathname;
            xStartInfo.Arguments = aArgLine;
            xStartInfo.WorkingDirectory = aWorkDir;
            xStartInfo.CreateNoWindow = false;
            xStartInfo.UseShellExecute = !aCapture;
            xStartInfo.RedirectStandardError = aCapture;
            xStartInfo.RedirectStandardOutput = aCapture;
            if (aElevate)
            {
                //TODO: May need to check for XP, and if XP not do this.
                //TODO: CsUAC at http://cfx.codeplex.com/sourcecontrol/changeset/view/25903?projectName=cfx#604822
                xStartInfo.UseShellExecute = true;
                xStartInfo.Verb = "runas";
                // The Process object must have the UseShellExecute property set to false in order to redirect IO streams.
                // So for now we cant capture output of elevated callees
                xStartInfo.RedirectStandardError = false;
                xStartInfo.RedirectStandardOutput = false;
            }
            var xProcess = Process.Start(xStartInfo);
            Console.WriteLine();
            Console.WriteLine("Executing:");
            Console.WriteLine("  " + xStartInfo.FileName);
            Console.WriteLine("Arguments:");
            Console.WriteLine("  " + xStartInfo.Arguments);
            Console.WriteLine("Working Directory:");
            Console.WriteLine("  " + xStartInfo.WorkingDirectory);
            Console.WriteLine();
            
            if (!aWait && aCapture) {
                // we arent gonna wait till it has finished by default. 
                // but if there was an error the app may exit quickly and we should display it
                // wait a small amount of time then check
                Thread.Sleep(500);
            }

            if (aWait || (aCapture && xProcess.HasExited)) {
                var xIsQemu = aEXEPathname.Contains("qemu.exe");
                if (!xProcess.WaitForExit(120 * 1000) || xIsQemu|| xProcess.ExitCode != 0) {
                    //TODO: Fix
                    if (aCapture) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error when executing: " + xStartInfo.FileName + " " + 
                            xStartInfo.Arguments + " from directory " + xStartInfo.WorkingDirectory);
                        Console.Write(xProcess.StandardOutput.ReadToEnd());
                        if (Environment.UserInteractive) {
													System.Windows.Forms.MessageBox.Show(xProcess.StandardError.ReadToEnd());
												} else {
													Console.Write(xProcess.StandardError.ReadToEnd());
												}
                        if (Environment.UserInteractive) {
													Console.WriteLine();
													Console.WriteLine("Press any key to continue");
													Console.ReadLine();
												} else {
													throw new Exception("Error while running program");
												}
                    } else {
                        throw new Exception("Call failed");
                    }
                }
            }
            return xProcess;
        }
    }
}
