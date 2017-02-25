using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Cosmos.Build.Common;
//using Cosmos.Build.MSBuild;
using Cosmos.IL2CPU;
using IL2CPU;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        //private void RunProcess(string fileName, string workingDirectory, string[] arguments)
        //{
        //    if (arguments == null)
        //    {
        //        throw new ArgumentNullException("arguments");
        //    }
        //    var xArgsString = arguments.Aggregate("", (a, b) => a + " \"" + b + "\"");
        //    var xResult = BaseToolTask.ExecuteTool(workingDirectory, fileName, xArgsString, "IL2CPU", OutputHandler.LogError, OutputHandler.LogMessage);
        //    if (!xResult)
        //    {
        //        throw new Exception("Error running process!");
        //    }
        //}

        private void RunIL2CPUProcess(string fileName, string workingDirectory, string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            var xArgsString = arguments.Aggregate("", (a, b) => a + " \"" + b + "\"");

            Action<string> errorReceived = OutputHandler.LogError;
            Action<string> outputReceived = OutputHandler.LogMessage;

            bool xResult;
            string name = "IL2CPU";

            var xProcessStartInfo = new ProcessStartInfo();
            xProcessStartInfo.WorkingDirectory = workingDirectory;
            xProcessStartInfo.FileName = fileName;
            xProcessStartInfo.Arguments = xArgsString;
            xProcessStartInfo.UseShellExecute = false;
            xProcessStartInfo.RedirectStandardOutput = true;
            xProcessStartInfo.RedirectStandardError = true;
            xProcessStartInfo.CreateNoWindow = true;

            outputReceived(string.Format("Executing command line \"{0}\"{1}", fileName, xArgsString));
            outputReceived(string.Format("Working directory = '{0}'", workingDirectory));

            using (var xProcess = new Process())
            {
                xProcess.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        errorReceived(e.Data);
                    }
                };
                xProcess.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        outputReceived(e.Data);
                    }
                };
                xProcess.StartInfo = xProcessStartInfo;
                xProcess.Start();
                xProcess.BeginErrorReadLine();
                xProcess.BeginOutputReadLine();
                xProcess.WaitForExit(15 * 60 * 1000); // wait 15 minutes
                if (!xProcess.HasExited)
                {
                    xProcess.Kill();
                    errorReceived(string.Format("{0} timed out.", name));
                    xResult = false;
                }
                else
                {
                    if (xProcess.ExitCode != 0)
                    {
                        errorReceived(string.Format("Error occurred while invoking {0}.", name));
                        xResult = false;
                    }
                }
                xResult = true;
            }

            if (!xResult)
            {
                throw new Exception("Error running process!");
            }
        }

        public static string RunObjDump(string cosmosBuildDir, string workingDir, string inputFile, Action<string> errorReceived, Action<string> outputReceived)
        {
            var xMapFile = Path.ChangeExtension(inputFile, "map");
            File.Delete(xMapFile);
            if (File.Exists(xMapFile))
            {
                throw new Exception("Could not delete " + xMapFile);
            }

            var xTempBatFile = Path.Combine(workingDir, "ExtractElfMap.bat");
            File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(cosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + inputFile + "\" > \"" + Path.GetFileName(xMapFile) + "\"");

            var xProcessStartInfo = new ProcessStartInfo();
            xProcessStartInfo.WorkingDirectory = workingDir;
            xProcessStartInfo.FileName = xTempBatFile;
            xProcessStartInfo.Arguments = "";
            xProcessStartInfo.UseShellExecute = false;
            xProcessStartInfo.RedirectStandardOutput = true;
            xProcessStartInfo.RedirectStandardError = true;
            xProcessStartInfo.CreateNoWindow = true;

            var xProcess = Process.Start(xProcessStartInfo);

            xProcess.WaitForExit(20000);

            File.Delete(xTempBatFile);

            return xMapFile;
        }

        private void RunExtractMapFromElfFile(string workingDir, string kernelFileName)
        {
            RunObjDump(CosmosPaths.Build, workingDir, kernelFileName, OutputHandler.LogError, OutputHandler.LogMessage);
        }

        // TODO: Move this and GetProjectReferences to an msbuild task.
        //private string GetProjectJsonLockFilePath(string kernelFileName)
        //{
        //    string xProjectJsonPath = null;
        //    var xParent = Directory.GetParent(kernelFileName);
        //    while (xParent != null)
        //    {
        //        var xProjectJson = xParent.GetFiles("project.lock.json");
        //        if (xProjectJson.Any())
        //        {
        //            xProjectJsonPath = xProjectJson[0].FullName;
        //            break;
        //        }
        //        xParent = xParent.Parent;
        //    }
        //    if (!string.IsNullOrWhiteSpace(xProjectJsonPath))
        //    {
        //        return xProjectJsonPath;
        //    }
        //    throw new FileNotFoundException("Project json lock file not found!");
        //}

        //private void GetProjectReferences(string kernelFileName)
        //{
        //    string xLockFilePath = GetProjectJsonLockFilePath(kernelFileName);
        //    if (File.Exists(xLockFilePath))
        //    {
        //        var xLockFile = NuGet.ProjectModel.LockFileUtilities.GetLockFile(xLockFilePath, null);
        //        if (xLockFile != null)
        //        {
        //            var pathContext = NuGet.Configuration.NuGetPathContext.Create(Path.GetDirectoryName(xLockFilePath));
        //            if (xLockFile.Libraries.Any())
        //            {
        //                var lockFileTarget = xLockFile.Targets.First(x => x.RuntimeIdentifier != null);
        //                if (lockFileTarget == null)
        //                {
        //                    throw new Exception("No runtime targets found in the jernel project.");
        //                }

        //                foreach (var lockFileTargetLibrary in lockFileTarget.Libraries)
        //                {
        //                    if (!lockFileTargetLibrary.NativeLibraries.Any())
        //                    {
        //                        foreach (var assembly in lockFileTargetLibrary.RuntimeAssemblies)
        //                        {
        //                            var lockFileLibrary = xLockFile.GetLibrary(lockFileTargetLibrary.Name, lockFileTargetLibrary.Version);
        //                            string xAssemblyPath = Path.Combine(pathContext.UserPackageFolder, lockFileLibrary.Path, assembly.Path);
        //                            var fileInfo = new FileInfo(xAssemblyPath);
        //                            if (fileInfo.Exists && fileInfo.Length > 0 && AdditionalReferences.FirstOrDefault(x => x.Contains(fileInfo.Name)) == null)
        //                            {
        //                                AdditionalReferences.Add(fileInfo.FullName);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        foreach (var assembly in lockFileTargetLibrary.NativeLibraries)
        //                        {
        //                            var lockFileLibrary = xLockFile.GetLibrary(lockFileTargetLibrary.Name, lockFileTargetLibrary.Version);
        //                            string xAssemblyPath = Path.Combine(pathContext.UserPackageFolder, lockFileLibrary.Path, assembly.Path);
        //                            var fileInfo = new FileInfo(xAssemblyPath);
        //                            if (fileInfo.Exists && fileInfo.Length > 0 && AdditionalReferences.FirstOrDefault(x => x.Contains(fileInfo.Name)) == null)
        //                            {
        //                                AdditionalReferences.Add(fileInfo.FullName);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private void RunIL2CPU(string kernelFileName, string outputFile)
        {
            string xPath = Path.GetDirectoryName(kernelFileName);
            References = new List<string>();
            References.Add(kernelFileName);
            References.Add(Path.Combine(xPath, "Cosmos.Core.Plugs.dll"));
            References.Add(Path.Combine(xPath, "Cosmos.Core.Plugs.Asm.dll"));
            References.Add(Path.Combine(xPath, "Cosmos.System.Plugs.dll"));
            References.Add(Path.Combine(xPath, "Cosmos.Debug.Kernel.Plugs.Asm.dll"));

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
                string xRootPath = @"..\";

                Directory.SetCurrentDirectory(AppContext.BaseDirectory);

                while (new DirectoryInfo(xRootPath).GetFiles().Where(file => file.Name == "Cosmos.sln").SingleOrDefault() == null)
                {
                    xRootPath += @"..\";
                }

                File.Copy(xRootPath + @"source\IL2CPU\bin\Debug\netcoreapp1.0\IL2CPU.deps.json", @"IL2CPU.deps.json", true);
                File.Copy(xRootPath + @"source\IL2CPU\bin\Debug\netcoreapp1.0\IL2CPU.runtimeconfig.json", @"IL2CPU.runtimeconfig.json", true);
                File.Copy(xRootPath + @"source\IL2CPU\bin\Debug\netcoreapp1.0\IL2CPU.runtimeconfig.dev.json", @"IL2CPU.runtimeconfig.dev.json", true);

                xArguments.Insert(0, typeof(Program).GetTypeInfo().Assembly.Location);
                RunIL2CPUProcess("dotnet",
                                 mBaseWorkingDirectory,
                                 xArguments.ToArray());
            }
        }

        private void RunNasm(string inputFile, string outputFile, bool isElf)
        {
            NASM.Program.LogMessage = OutputHandler.LogMessage;
            NASM.Program.LogError = OutputHandler.LogError;
            int xResult =NASM.Program.Run(new[]
                             {
                                 $"InputFile:{inputFile}",
                                 $"OutputFile:{outputFile}",
                                 $"IsElf:{isElf}",
                                 $"ExePath:{Path.Combine(GetCosmosUserkitFolder(), "build", "tools", "nasm", "nasm.exe")}"
                             }, OutputHandler.LogMessage, OutputHandler.LogError);
            if (xResult != 0)
            {
                throw new Exception("Error running Nasm");
            }
        }

        private void RunLd(string inputFile, string outputFile)
        {
            string[] arguments = new[]
                       {
                           "-Ttext", "0x2000000",
                           "-Tdata", " 0x1000000",
                           "-e", "Kernel_Start",
                           "-o",outputFile.Replace('\\', '/'),
                           inputFile.Replace('\\', '/')
                       };

            var xArgsString = arguments.Aggregate("", (a, b) => a + " \"" + b + "\"");

            var xProcess = Process.Start(Path.Combine(GetCosmosUserkitFolder(), "build", "tools", "cygwin", "ld.exe"), xArgsString);

            xProcess.WaitForExit(10000);

            //RunProcess(Path.Combine(GetCosmosUserkitFolder(), "build", "tools", "cygwin", "ld.exe"),
            //           mBaseWorkingDirectory,
            //           new[]
            //           {
            //               "-Ttext", "0x2000000",
            //               "-Tdata", " 0x1000000",
            //               "-e", "Kernel_Start",
            //               "-o",outputFile.Replace('\\', '/'),
            //               inputFile.Replace('\\', '/')
            //           });
        }

        private static string GetCosmosUserkitFolder()
        {
            CosmosPaths.Initialize();
            return CosmosPaths.UserKit;
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
