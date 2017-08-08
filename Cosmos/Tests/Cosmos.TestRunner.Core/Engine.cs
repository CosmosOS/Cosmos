using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Cosmos.Build.Common;

namespace Cosmos.TestRunner.Core
{
    public partial class Engine
    {
        // configuration: in process eases debugging, but means certain errors (like stack overflow) kill the test runner.
        public bool DebugIL2CPU = false;
        public bool RunWithGDB = false;
        public bool StartBochsDebugGui = false;
        public bool EnableStackCorruptionChecks = true;
        public string KernelPkg = "";
        public TraceAssemblies TraceAssembliesLevel = TraceAssemblies.User;
        public StackCorruptionDetectionLevel StackCorruptionChecksLevel = StackCorruptionDetectionLevel.MethodFooters;
        public List<string> References = new List<string>();
        public List<string> AdditionalSearchDirs = new List<string>();
        public List<string> AdditionalReferences = new List<string>();

        public List<string> KernelsToRun { get; } = new List<string>();

        public void AddKernel(string assemblyFile)
        {
            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("Kernel file not found!", assemblyFile);
            }
            KernelsToRun.Add(assemblyFile);
        }

        private string mBaseWorkingDirectory;

        public OutputHandlerBasic OutputHandler;

        public bool Execute()
        {
            if (OutputHandler == null)
            {
                throw new InvalidOperationException("No OutputHandler set!");
            }

            if (RunTargets.Count == 0)
            {
                RunTargets.AddRange((RunTargetEnum[])Enum.GetValues(typeof(RunTargetEnum)));
            }

            OutputHandler.ExecutionStart();
            try
            {
                var xResult = true;
                foreach (var xConfig in GetRunConfigurations())
                {
                    OutputHandler.RunConfigurationStart(xConfig);
                    try
                    {
                        foreach (var xAssemblyFile in KernelsToRun)
                        {
                            mBaseWorkingDirectory = Path.Combine(Path.GetDirectoryName(typeof(Engine).GetTypeInfo().Assembly.Location), "WorkingDirectory");
                            if (Directory.Exists(mBaseWorkingDirectory))
                            {
                                Directory.Delete(mBaseWorkingDirectory, true);
                            }
                            Directory.CreateDirectory(mBaseWorkingDirectory);

                            xResult &= ExecuteKernel(xAssemblyFile, xConfig);
                        }
                    }
                    catch (Exception e)
                    {
                        if (!mKernelResultSet)
                        {
                            OutputHandler.SetKernelTestResult(false, e.ToString());
                            mKernelResult = false;
                            xResult = false;
                        }
                        OutputHandler.UnhandledException(e);
                    }
                    finally
                    {
                        OutputHandler.RunConfigurationEnd(xConfig);
                    }
                }
                return xResult;
            }
            catch (Exception E)
            {
                OutputHandler.UnhandledException(E);
                return false;
            }
            finally
            {
                OutputHandler.ExecutionEnd();
            }

            // todo: now report summary
            //DoLog("NotImplemented, summary?");
        }

        private IEnumerable<RunConfiguration> GetRunConfigurations()
        {
            foreach (RunTargetEnum xTarget in RunTargets)
            {
                yield return new RunConfiguration { IsELF = true, RunTarget = xTarget };
                //yield return new RunConfiguration { IsELF = false, RunTarget = xTarget };
            }
        }
    }
}
