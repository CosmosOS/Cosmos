using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cosmos.Build.Common;
using Cosmos.Build.MSBuild;
using Cosmos.IL2CPU;
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

        // TODO: Move this and GetProjectReferences to an msbuild task.
        private string GetProjectJsonLockFilePath(string kernelFileName)
        {
            string xProjectJsonPath = null;
            var xParent = Directory.GetParent(kernelFileName);
            while (xParent != null)
            {
                var xProjectJson = xParent.GetFiles("project.lock.json");
                if (xProjectJson.Any())
                {
                    xProjectJsonPath = xProjectJson[0].FullName;
                    break;
                }
                xParent = xParent.Parent;
            }
            if (!string.IsNullOrWhiteSpace(xProjectJsonPath))
            {
                return xProjectJsonPath;
            }
            throw new FileNotFoundException("Project json lock file not found!");
        }

        private void GetProjectReferences(string kernelFileName)
        {
            string xLockFilePath = GetProjectJsonLockFilePath(kernelFileName);
            if (File.Exists(xLockFilePath))
            {
                var xLockFile = NuGet.ProjectModel.LockFileUtilities.GetLockFile(xLockFilePath, null);
                if (xLockFile != null)
                {
                    var pathContext = NuGet.Configuration.NuGetPathContext.Create(Path.GetDirectoryName(xLockFilePath));
                    if (xLockFile.Libraries.Any())
                    {
                        var lockFileTarget = xLockFile.Targets.First(x => x.RuntimeIdentifier != null);
                        foreach (var lockFileTargetLibrary in lockFileTarget.Libraries)
                        {
                            if (!lockFileTargetLibrary.RuntimeAssemblies.Any())
                            {
                                foreach (var assembly in lockFileTargetLibrary.CompileTimeAssemblies)
                                {
                                    var lockFileLibrary = xLockFile.GetLibrary(lockFileTargetLibrary.Name, lockFileTargetLibrary.Version);
                                    string xAssemblyPath = Path.Combine(pathContext.UserPackageFolder, lockFileLibrary.Path, assembly.Path);
                                    var fileInfo = new FileInfo(xAssemblyPath);
                                    if (fileInfo.Exists && fileInfo.Length > 0 && !AdditionalReferences.Contains(fileInfo.FullName))
                                    {
                                        AdditionalReferences.Add(fileInfo.FullName);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var assembly in lockFileTargetLibrary.RuntimeAssemblies)
                                {
                                    var lockFileLibrary = xLockFile.GetLibrary(lockFileTargetLibrary.Name, lockFileTargetLibrary.Version);
                                    string xAssemblyPath = Path.Combine(pathContext.UserPackageFolder, lockFileLibrary.Path, assembly.Path);
                                    var fileInfo = new FileInfo(xAssemblyPath);
                                    if (fileInfo.Exists && fileInfo.Length > 0 && !AdditionalReferences.Contains(fileInfo.FullName))
                                    {
                                        AdditionalReferences.Add(fileInfo.FullName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RunIL2CPU(string kernelFileName, string outputFile)
        {
            References = new List<string>();
            AdditionalReferences = new List<string>();
            AdditionalSearchDirs = new List<string>();

            GetProjectReferences(kernelFileName);

            AdditionalSearchDirs.Add(Path.GetDirectoryName(kernelFileName));
            References.Add(kernelFileName);
            // TODO: Need a better way to do this.
            foreach (var xFile in new DirectoryInfo(Path.GetDirectoryName(kernelFileName)).GetFiles("*Plugs*.dll"))
            {
                References.Add(xFile.FullName);
            }
            //References.Add(typeof(Cosmos.Debug.Kernel.Plugs.Asm.DebugBreak).Assembly.Location);
            //References.Add(typeof(Cosmos.Core.Plugs.Asm.ArrayImpl).Assembly.Location);

            var xArguments = new List<string>
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
                                 "IgnoreDebugStubAttribute:False"
                             };
            xArguments.AddRange(References.Select(xRef => "References:" + xRef));
            xArguments.AddRange(AdditionalSearchDirs.Select(xDir => "AdditionalSearchDirs:" + xDir));
            xArguments.AddRange(AdditionalReferences.Select(xRef => "AdditionalReferences:" + xRef));

            //xArguments.Add("References:" + typeof(CPUImpl).Assembly.Location);
            //xArguments.Add("References:" + typeof(DebugBreak).Assembly.Location);

            if (RunIL2CPUInProcess)
            {
                if (KernelsToRun.Count > 1)
                {
                    throw new Exception("Cannot run multiple kernels with in-process compilation!");
                }
                // ensure we're using the referenced (= solution) version
                CosmosAssembler.ReadDebugStubFromDisk = false;
                var xResult = Program.Run(xArguments.ToArray(), OutputHandler.LogMessage, OutputHandler.LogError);
                if (xResult != 0)
                {
                    throw new Exception("Error running IL2CPU");
                }
            }
            else
            {
                RunProcess(typeof(Program).Assembly.Location,
                           mBaseWorkingDirectory,
                           xArguments.ToArray());
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
