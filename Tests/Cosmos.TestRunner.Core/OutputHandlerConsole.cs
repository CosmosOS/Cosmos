using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cosmos.TestRunner.Core
{
    public class OutputHandlerConsole: OutputHandlerBase
    {
        private readonly Stopwatch mCurrentTaskStopwatch=new Stopwatch();
        private readonly Stopwatch mCurrentKernelStopwatch = new Stopwatch();
        private readonly Stopwatch mExecutionStopwatch = new Stopwatch();

        public override void TaskStart(string taskName)
        {
            Log("Running task '" + taskName + "'");
            mCurrentTaskStopwatch.Reset();
            mCurrentTaskStopwatch.Start();
            mLogLevel ++;
        }

        public override void TaskEnd(string taskName)
        {
            mCurrentTaskStopwatch.Stop();
            mLogLevel --;
            Log("Done running task '" + taskName + "'. Took " + mCurrentTaskStopwatch.Elapsed);
        }

        public override void UnhandledException(Exception exception)
        {
            Log("Unhandled exception: "+ exception.ToString());
        }

        public override void ExecutionEnd()
        {
            mLogLevel = 0;
            Log("Done executing");
            Log("Took " + mExecutionStopwatch.Elapsed);

            Log(String.Format("{0} kernels succeeded their tests", mNumberOfSuccesses));
            Log(String.Format("{0} kernels failed their tests", mNumberOfFailures));
        }

        public override void ExecutionStart()
        {
            mLogLevel = 0;
            Log("Start executing");
            mExecutionStopwatch.Reset();
            mExecutionStopwatch.Start();
            mLogLevel = 1;
        }

        public override void LogDebugMessage(string message)
        {

        }

        public override void RunConfigurationStart(RunConfiguration configuration)
        {
            Log(string.Format("Start configuration. IsELF = {0}", configuration.IsELF));
            mLogLevel++;
        }

        public override void RunConfigurationEnd(RunConfiguration configuration)
        {
            mLogLevel --;
        }

        public override void LogError(string message)
        {
            Log("Error: " + message);
        }

        public override void LogMessage(string message)
        {
        }

        public override void ExecuteKernelEnd(string assemblyName)
        {
            mCurrentKernelStopwatch.Stop();
            Log("Done running kernel. Took " + mCurrentKernelStopwatch.Elapsed);
            mLogLevel--;
        }

        public override void ExecuteKernelStart(string assemblyName)
        {
            Log("Starting kernel '" + assemblyName + "'");
            mCurrentKernelStopwatch.Reset();
            mCurrentKernelStopwatch.Start();
            mLogLevel++;
        }

        private int mLogLevel;
        private void Log(string message)
        {
            Console.Write(DateTime.Now.ToString("hh:mm:ss.ffffff "));
            Console.Write(new String(' ', mLogLevel * 2));
            Console.WriteLine(message);
        }

        public override void SetKernelTestResult(bool succeeded, string message)
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

        public override void SetKernelSucceededAssertionsCount(int succeededAssertions)
        {
        }
    }
}
