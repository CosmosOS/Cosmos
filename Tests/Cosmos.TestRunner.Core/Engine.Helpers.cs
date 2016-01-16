using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
            var xArgsString = arguments.Aggregate("", (a, b) => a + " \"" + b + "\"");
            var xResult = BaseToolTask.ExecuteTool(workingDirectory, fileName, xArgsString, "IL2CPU", OutputHandler.LogError, OutputHandler.LogMessage);
            if (!xResult)
            {
                throw new Exception("Error running process!");
            }
        }

        private void RunExtractMapFromElfFile(string workingDir, string kernelFileName)
        {
            ExtractMapFromElfFile.RunObjDump(CosmosPaths.Build, workingDir, kernelFileName, OutputHandler.LogError, OutputHandler.LogMessage);
        }

        private void RunIL2CPU(string kernelFileName, string outputFile)
        {
            var xArguments = new[]
                             {
                                 "DebugEnabled:true",
                                 "StackCorruptionDetectionEnabled:" + EnableStackCorruptionChecks,
                                 "StackCorruptionDetectionLevel:" + StackCorruptionChecksLevel,
                                 "DebugMode:Source",
                                 "TraceAssemblies:" + TraceAssembliesLevel,
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
                             };

            if (RunIL2CPUInProcess)
            {
                if (mKernelsToRun.Count > 1)
                {
                    throw new Exception("Cannot run multiple kernels with in-process compilation!");
                }
                // ensure we're using the referenced (= solution) version
                Assembler.Assembler.ReadDebugStubFromDisk = false;
                var xResult = Program.Run(xArguments, OutputHandler.LogMessage, OutputHandler.LogError);
                if (xResult != 0)
                {
                    throw new Exception("Error running IL2CPU");
                }
            }
            else
            {
                RunProcess(typeof(Program).Assembly.Location,
                           mBaseWorkingDirectory,
                           xArguments);
            }
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
