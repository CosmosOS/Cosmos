using System;
using System.Diagnostics;

namespace Cosmos.TestRunner.Core
{
    public abstract class OutputHandlerTextBase: OutputHandlerBase
    {
        private readonly Stopwatch mCurrentTaskStopwatch = new Stopwatch();
        private readonly Stopwatch mCurrentKernelStopwatch = new Stopwatch();
        private readonly Stopwatch mExecutionStopwatch = new Stopwatch();

        protected override void OnTaskStart(string taskName)
        {
            Log("Running task '" + taskName + "'");
            mCurrentTaskStopwatch.Reset();
            mCurrentTaskStopwatch.Start();
            mLogLevel++;
        }

        protected override void OnTaskEnd(string taskName)
        {
            mCurrentTaskStopwatch.Stop();
            mLogLevel--;
            Log("Done running task '" + taskName + "'. Took " + mCurrentTaskStopwatch.Elapsed);
        }

        protected override void OnUnhandledException(Exception exception)
        {
            Log("Unhandled exception: " + exception.ToString());
        }

        protected override void OnExecutionEnd()
        {
            mLogLevel = 0;
            Log("Done executing");
            Log("Took " + mExecutionStopwatch.Elapsed);

            Log(String.Format("{0} kernels succeeded their tests", mNumberOfSuccesses));
            Log(String.Format("{0} kernels failed their tests", mNumberOfFailures));
        }

        protected override void OnExecutionStart()
        {
            mLogLevel = 0;
            Log("Start executing");
            mExecutionStopwatch.Reset();
            mExecutionStopwatch.Start();
            mLogLevel = 1;
        }

        protected override void OnLogDebugMessage(string message)
        {

        }

        protected override void OnRunConfigurationStart(RunConfiguration configuration)
        {
            Log(string.Format("Start configuration. IsELF = {0}, Target = {1}", configuration.IsELF, configuration.RunTarget));
            mLogLevel++;
        }

        protected override void OnRunConfigurationEnd(RunConfiguration configuration)
        {
            mLogLevel--;
        }

        protected override void OnLogError(string message)
        {
            Log("Error: " + message);
        }

        protected override void OnLogMessage(string message)
        {
        }

        protected override void OnExecuteKernelEnd(string assemblyName)
        {
            mCurrentKernelStopwatch.Stop();
            Log("Done running kernel. Took " + mCurrentKernelStopwatch.Elapsed);
            mLogLevel--;
        }

        protected override void OnExecuteKernelStart(string assemblyName)
        {
            Log("Starting kernel '" + assemblyName + "'");
            mCurrentKernelStopwatch.Reset();
            mCurrentKernelStopwatch.Start();
            mLogLevel++;
        }

        protected int mLogLevel;

        protected abstract void Log(string message);

        protected override void OnSetKernelTestResult(bool succeeded, string message)
        {
            Log(string.Format("Success = {0}, Message = '{1}'", succeeded, message));
            if (succeeded)
            {
                mNumberOfSuccesses++;
            }
            else
            {
                mNumberOfFailures++;
            }
        }

        private int mNumberOfSuccesses = 0;
        private int mNumberOfFailures = 0;

        protected override void OnSetKernelSucceededAssertionsCount(int succeededAssertions)
        {
        }

    }
}
