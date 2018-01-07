using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Cosmos.Build.Common;

namespace Cosmos.TestRunner.Core
{
    public partial class Engine
    {
        // configuration: in process eases debugging, but means certain errors (like stack overflow) kill the test runner.
        protected bool DebugIL2CPU => mConfiguration.DebugIL2CPU;
        protected string KernelPkg => mConfiguration.KernelPkg;
        protected TraceAssemblies TraceAssembliesLevel => mConfiguration.TraceAssembliesLevel;
        protected bool EnableStackCorruptionChecks => mConfiguration.EnableStackCorruptionChecks;
        protected StackCorruptionDetectionLevel StackCorruptionChecksLevel => mConfiguration.StackCorruptionChecksLevel;

        protected bool RunWithGDB => mConfiguration.RunWithGDB;
        protected bool StartBochsDebugGui => mConfiguration.StartBochsDebugGUI;

        public IEnumerable<Type> KernelsToRun => mConfiguration.KernelTypesToRun;

        private IEngineConfiguration mConfiguration;
        private string mBaseWorkingDirectory;

        public OutputHandlerBasic OutputHandler;

        public Engine(IEngineConfiguration aEngineConfiguration)
        {
            mConfiguration = aEngineConfiguration;
        }

        public bool Execute()
        {
            if (OutputHandler == null)
            {
                throw new InvalidOperationException("No OutputHandler set!");
            }

            if (!RunTargets.Any())
            {
                throw new InvalidOperationException("No run targets were specified!");
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
                        foreach (var xKernelType in KernelsToRun)
                        {
                            mBaseWorkingDirectory = Path.Combine(Path.GetDirectoryName(typeof(Engine).Assembly.Location), "WorkingDirectory");
                            if (Directory.Exists(mBaseWorkingDirectory))
                            {
                                Directory.Delete(mBaseWorkingDirectory, true);
                            }
                            Directory.CreateDirectory(mBaseWorkingDirectory);

                            xResult &= ExecuteKernel(xKernelType.Assembly.Location, xConfig);
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
