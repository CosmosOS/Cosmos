using System;

namespace Cosmos.TestRunner.Core
{
    public abstract class OutputHandlerBase
    {
        public abstract void ExecuteKernelStart(string assemblyName);
        public abstract void ExecuteKernelEnd(string assemblyName);
        public abstract void LogMessage(string message);
        public abstract void LogError(string message);
        public abstract void ExecutionStart();
        public abstract void ExecutionEnd();
        public abstract void UnhandledException(Exception exception);
        public abstract void TaskStart(string taskName);
        public abstract void TaskEnd(string taskName);
        public abstract void SetKernelTestResult(bool succeeded, string message);
    }
}
