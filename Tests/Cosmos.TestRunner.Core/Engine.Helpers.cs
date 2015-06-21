using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security;
using Cosmos.Build.Common;
using Cosmos.Build.MSBuild;
using Cosmos.Core.Plugs;
using Cosmos.Debug.Kernel.Plugs;
using Cosmos.System.Plugs.System;
using IL2CPU;
using Microsoft.Win32;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunProcess(string fileName, string workingDirectory, string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            var xStartInfo = new ProcessStartInfo(fileName);
            xStartInfo.WorkingDirectory = workingDirectory;
            xStartInfo.Arguments = arguments.Aggregate("", (a, b) => a + " \"" + b + "\"");
            xStartInfo.RedirectStandardError = true;
            xStartInfo.RedirectStandardOutput = true;
            xStartInfo.UseShellExecute = false;

            var xProcess = new Process();
            xProcess.StartInfo = xStartInfo;

            xProcess.OutputDataReceived += (sender, e) => OutputHandler.LogMessage(e.Data);
            xProcess.ErrorDataReceived += (sender, e) => OutputHandler.LogError(e.Data);
            xProcess.Start();
            xProcess.BeginErrorReadLine();
            xProcess.BeginOutputReadLine();
            xProcess.WaitForExit(30000); // max 30 seconds
            if (xProcess.ExitCode != 0)
            {
                throw new Exception("Error running process!");
            }
        }

        private void RunIL2CPU(string kernelFileName, string outputFile)
        {
            RunProcess(typeof(Program).Assembly.Location,
                       mBaseWorkingDirectory,
                       new[]
                       {
                           "DebugEnabled:True",
                           "StackCorruptionDetectionEnabled:False",
                           "DebugMode:Source",
                           "TraceAssemblies:",
                           "DebugCom:1",
                           "UseNAsm:True",
                           "OutputFilename:" + outputFile,
                           "EnableLogging:True",
                           "EmitDebugSymbols:True",
                           "IgnoreDebugStubAttribute:False",
                           "References:" + kernelFileName,
                           "References:" + typeof(CPUImpl).Assembly.Location,
                           "References:" + typeof(DebugBreak).Assembly.Location,
                           "References:" + typeof(ConsoleImpl).Assembly.Location
                       });
        }

        private void RunNasm(string inputFile, string outputFile, bool isElf)
        {
            var xNasmTask = new NAsmTask();
            xNasmTask.InputFile = inputFile;
            xNasmTask.OutputFile = outputFile;
            xNasmTask.IsELF = isElf;
            xNasmTask.ExePath = Path.Combine(GetCosmosUserkitFolder(), "build", "tools", "nasm", "nasm.exe");
            xNasmTask.LogMessage = OutputHandler.LogMessage;
            xNasmTask.LogError = OutputHandler.LogError;
            if (!xNasmTask.Execute())
            {
                throw new Exception("Error running nasm!");
            }
        }

        private void RunLd(string inputFile, string outputFile)
        {
            RunProcess(Path.Combine(GetCosmosUserkitFolder(), "build", "tools", "cygwin", "ld.exe"),
                       mBaseWorkingDirectory,
                       new[]
                       {
                           "-Ttext", "0x2000000",
                           "-Tdata", " 0x1000000",
                           "-e", "Kernel_Start",
                           "-o",outputFile.Replace('\\', '/'),
                           inputFile.Replace('\\', '/')
                       });
        }

        private static string GetCosmosUserkitFolder()
        {
            //$([MSBuild]::GetRegistryValue("HKEY_LOCAL_MACHINE\Software\Cosmos", "UserKit"))
            using (var xReg = Registry.LocalMachine.OpenSubKey("Software\\Cosmos"))
            {
                var xResult = (xReg.GetValue("UserKit") ?? "").ToString();
                if (!Directory.Exists(xResult))
                {
                    throw new Exception("Unable to retrieve Cosmos userkit folder!");
                }
                return xResult;
            }
        }

        private void MakeIso(string objectFile, string isoFile)
        {
            IsoMaker.Generate(objectFile, isoFile);
            if (!File.Exists(isoFile))
            {
                throw new Exception("Error building iso");
            }
        }
    }
}
