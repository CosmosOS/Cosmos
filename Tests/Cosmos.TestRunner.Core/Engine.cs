using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Cosmos.Build.Common;

namespace Cosmos.TestRunner.Core
{
    public partial class Engine
    {
        private static readonly string WorkingDirectoryBase = Path.Combine(
            Path.GetDirectoryName(typeof(Engine).Assembly.Location), "WorkingDirectory");

        // configuration: in process eases debugging, but means certain errors (like stack overflow) kill the test runner.
        protected bool DebugIL2CPU => mConfiguration.DebugIL2CPU;
        protected string KernelPkg => mConfiguration.KernelPkg;
        protected TraceAssemblies TraceAssembliesLevel => mConfiguration.TraceAssembliesLevel;
        protected bool EnableStackCorruptionChecks => mConfiguration.EnableStackCorruptionChecks;
        protected StackCorruptionDetectionLevel StackCorruptionDetectionLevel => mConfiguration.StackCorruptionDetectionLevel;

        protected bool RunWithGDB => mConfiguration.RunWithGDB;
        protected bool StartBochsDebugGui => mConfiguration.StartBochsDebugGUI;

        public IEnumerable<string> KernelsAssembliesToRun => mConfiguration.KernelAssembliesToRun;

        private IEngineConfiguration mConfiguration;
        private TestResultOutputHandler mTestResultOutputHandler;

        public OutputHandlerBasic OutputHandler { get; private set; }

        public Engine(IEngineConfiguration aEngineConfiguration)
        {
            mConfiguration = aEngineConfiguration;
            mTestResultOutputHandler = new TestResultOutputHandler();
        }

        public ITestResult Execute() => Execute(CancellationToken.None);

        public ITestResult Execute(CancellationToken cancellationToken)
        {
            // todo: implement cancellation

            if (!RunTargets.Any())
            {
                throw new InvalidOperationException("No run targets were specified!");
            }

            var xTestResult = new TestResult();

            OutputHandler.ExecutionStart();

            foreach (var xConfig in GetRunConfigurations())
            {
                OutputHandler.RunConfigurationStart(xConfig);

                foreach (var xKernelAssembly in KernelsAssembliesToRun)
                {
                    var xKernelName = Path.GetFileNameWithoutExtension(xKernelAssembly);
                    var xKernelTestResult = new KernelTestResult(xKernelName, xConfig);

                    var xWorkingDirectory = Path.Combine(WorkingDirectoryBase, xKernelName);

                    if (Directory.Exists(xWorkingDirectory))
                    {
                        Directory.Delete(xWorkingDirectory, true);
                    }

                    Directory.CreateDirectory(xWorkingDirectory);

                    try
                    {
                        xKernelTestResult.Result = ExecuteKernel(
                            xKernelAssembly, xWorkingDirectory, xConfig, xKernelTestResult);
                    }
                    catch (Exception e)
                    {
                        OutputHandler.UnhandledException(e);
                    }

                    xKernelTestResult.TestLog = mTestResultOutputHandler.TestLog;

                    xTestResult.AddKernelTestResult(xKernelTestResult);
                }

                OutputHandler.RunConfigurationEnd(xConfig);
            }

            OutputHandler.ExecutionEnd();

            var xPassedTestsCount = xTestResult.KernelTestResults.Count(r => r.Result);
            var xFailedTestsCount = xTestResult.KernelTestResults.Count(r => !r.Result);

            OutputHandler.LogMessage($"Done executing: {xPassedTestsCount} test(s) passed, {xFailedTestsCount} test(s) failed.");

            return xTestResult;
        }

        public void SetOutputHandler(OutputHandlerBasic outputHandler) =>
            OutputHandler = new MultiplexingOutputHandler(outputHandler, mTestResultOutputHandler);

        private IEnumerable<RunConfiguration> GetRunConfigurations()
        {
            foreach (var xTarget in RunTargets)
            {
                yield return new RunConfiguration(isElf: true, runTarget: xTarget);
                //yield return new RunConfiguration(isElf: false, runTarget: xTarget);
            }
        }
    }
}
