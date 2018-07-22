using System;

namespace Cosmos.TestRunner.Core
{
    public abstract class OutputHandlerBase: OutputHandlerBasic
    {
        protected abstract void OnExecuteKernelStart(string assemblyName);
        protected abstract void OnExecuteKernelEnd(string assemblyName);
        protected abstract void OnLogDebugMessage(string message);
        protected abstract void OnLogMessage(string message);
        protected abstract void OnLogError(string message);
        protected abstract void OnExecutionStart();
        protected abstract void OnExecutionEnd();
        protected abstract void OnUnhandledException(Exception exception);
        protected abstract void OnTaskStart(string taskName);
        protected abstract void OnTaskEnd(string taskName);
        protected abstract void OnSetKernelTestResult(bool succeeded, string message);

        protected abstract void OnRunConfigurationStart(RunConfiguration configuration);
        protected abstract void OnRunConfigurationEnd(RunConfiguration configuration);

        protected abstract void OnSetKernelSucceededAssertionsCount(int succeededAssertions);


        private readonly object mLockObject = new Object();
        private void RunLocked(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            lock (mLockObject)
            {
                action();
            }
        }

        public override void ExecuteKernelStart(string assemblyName)
        {
            RunLocked(() => OnExecuteKernelStart(assemblyName));
        }

        public override void ExecuteKernelEnd(string assemblyName)
        {
            RunLocked(() => OnExecuteKernelEnd(assemblyName));
        }

        public override void ExecutionEnd()
        {
            RunLocked(() => OnExecutionEnd());
        }

        public override void ExecutionStart()
        {
            RunLocked(() => OnExecutionStart());
        }

        public override void LogDebugMessage(string message)
        {
            RunLocked(() => OnLogDebugMessage(message));
        }

        public override void LogError(string message)
        {
            RunLocked(() => OnLogError(message));
        }

        public override void LogMessage(string message)
        {
            RunLocked(() => OnLogMessage(message));
        }

        public override void RunConfigurationEnd(RunConfiguration configuration)
        {
            RunLocked(() => OnRunConfigurationEnd(configuration));
        }

        public override void RunConfigurationStart(RunConfiguration configuration)
        {
            RunLocked(() => OnRunConfigurationStart(configuration));
        }

        public override void SetKernelSucceededAssertionsCount(int succeededAssertions)
        {
            RunLocked(() => OnSetKernelSucceededAssertionsCount(succeededAssertions));
        }

        public override void SetKernelTestResult(bool succeeded, string message)
        {
            RunLocked(() => OnSetKernelTestResult(succeeded, message));
        }

        public override void TaskEnd(string taskName)
        {
            RunLocked(() => OnTaskEnd(taskName));
        }

        public override void TaskStart(string taskName)
        {
            RunLocked(() => OnTaskStart(taskName));
        }

        public override void UnhandledException(Exception exception)
        {
            RunLocked(() => OnUnhandledException(exception));
        }
    }
}
