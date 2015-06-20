using System;
using System.IO;
using Cosmos.Build.MSBuild;
using Cosmos.Core.Plugs;
using Cosmos.Debug.Kernel;
using Cosmos.Debug.Kernel.Plugs;
using Cosmos.System.Plugs.System;
using IL2CPU;
using Microsoft.Win32;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void ExecuteKernel(string assemblyFileName)
        {
            var xAssemblyFile = Path.Combine(mBaseWorkingDirectory, "Kernel.asm");
            var xObjectFile = Path.Combine(mBaseWorkingDirectory, "Kernel.obj");

            mLogLevel = 1;
            DoLog(string.Format("Testing '{0}'", assemblyFileName));
            mLogLevel = 2;
            RunIL2CPU(assemblyFileName, xAssemblyFile);
            mLogLevel = 2;
            RunNasm(xAssemblyFile, xObjectFile, true);
        }

        private void RunIL2CPU(string kernelFileName, string outputFile)
        {
            DoLog("Running IL2CPU");
            mLogLevel++;

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
            DoLog("Running Nasm");
            mLogLevel++;

            var xNasmTask = new NAsmTask();
            xNasmTask.InputFile = inputFile;
            xNasmTask.OutputFile = outputFile;
            xNasmTask.IsELF = isElf;
            xNasmTask.ExePath = Path.Combine(GetCosmosUserkitFolder(), "build", "tools", "nasm", "nasm.exe");
            xNasmTask.LogMessage = DoLog;
            xNasmTask.LogError = DoLog;
            if (!xNasmTask.Execute())
            {
                throw new Exception("Error running nasm!");
            }
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
    }
}
