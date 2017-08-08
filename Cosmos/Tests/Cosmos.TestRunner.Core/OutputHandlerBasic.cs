using System;

namespace Cosmos.TestRunner.Core
{
    public abstract class OutputHandlerBasic
    {
        public abstract void ExecuteKernelStart(string assemblyName);
        public abstract void ExecuteKernelEnd(string assemblyName);
        public abstract void LogDebugMessage(string message);
        public abstract void LogMessage(string message);
        public abstract void LogError(string message);
        public abstract void ExecutionStart();
        public abstract void ExecutionEnd();
        public abstract void UnhandledException(Exception exception);
        public abstract void TaskStart(string taskName);
        public abstract void TaskEnd(string taskName);
        public abstract void SetKernelTestResult(bool succeeded, string message);
        public abstract void RunConfigurationStart(RunConfiguration configuration);
        public abstract void RunConfigurationEnd(RunConfiguration configuration);
        public abstract void SetKernelSucceededAssertionsCount(int succeededAssertions);
    }
}
