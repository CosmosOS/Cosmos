using System;

namespace Cosmos.TestRunner.Core
{
    public class MultiplexingOutputHandler: OutputHandlerBasic
    {
        private readonly OutputHandlerBasic[] mTargets;

        public MultiplexingOutputHandler(params OutputHandlerBasic[] targets)
        {
            mTargets = targets;
        }

        private void RunOnTargets(Action<OutputHandlerBasic> action)
        {
            foreach (var xTarget in mTargets)
            {
                action(xTarget);
            }
        }

        public override void ExecuteKernelStart(string assemblyName)
        {
            RunOnTargets(t => t.ExecuteKernelStart(assemblyName));
        }

        public override void ExecuteKernelEnd(string assemblyName)
        {
            RunOnTargets(t => t.ExecuteKernelEnd(assemblyName));
        }

        public override void LogDebugMessage(string message)
        {
            RunOnTargets(t => t.LogDebugMessage(message));
        }

        public override void LogMessage(string message)
        {
            RunOnTargets(t => t.LogMessage(message));
        }

        public override void LogError(string message)
        {
            RunOnTargets(t => t.LogError(message));
        }

        public override void ExecutionStart()
        {
            RunOnTargets(t => t.ExecutionStart());
        }

        public override void ExecutionEnd()
        {
            RunOnTargets(t => t.ExecutionEnd());
        }

        public override void UnhandledException(Exception exception)
        {
            RunOnTargets(t => t.UnhandledException(exception));
        }

        public override void TaskStart(string taskName)
        {
            RunOnTargets(t => t.TaskStart(taskName));
        }

        public override void TaskEnd(string taskName)
        {
            RunOnTargets(t => t.TaskEnd(taskName));
        }

        public override void SetKernelTestResult(bool succeeded, string message)
        {
            RunOnTargets(t => t.SetKernelTestResult(succeeded, message));
        }

        public override void RunConfigurationStart(RunConfiguration configuration)
        {
            RunOnTargets(t => t.RunConfigurationStart(configuration));
        }

        public override void RunConfigurationEnd(RunConfiguration configuration)
        {
            RunOnTargets(t => t.RunConfigurationEnd(configuration));
        }

        public override void SetKernelSucceededAssertionsCount(int succeededAssertions)
        {
            RunOnTargets(t => t.SetKernelSucceededAssertionsCount(succeededAssertions));
        }
    }
}
