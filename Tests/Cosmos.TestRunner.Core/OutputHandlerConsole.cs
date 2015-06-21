using System;
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
            mLogLevel = 2;
        }

        public override void TaskEnd(string taskName)
        {
            mCurrentTaskStopwatch.Stop();
            mLogLevel = 1;
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
        }

        public override void ExecutionStart()
        {
            mLogLevel = 0;
            Log("Start executing");
            mExecutionStopwatch.Reset();
            mExecutionStopwatch.Start();
            mLogLevel = 1;
        }

        public override void LogError(string message)
        {
            Log("Error: " + message);
        }

        public override void LogMessage(string message)
        {
            Log(message);
        }

        public override void ExecuteKernelEnd(string assemblyName)
        {
            mCurrentKernelStopwatch.Stop();
            Log("Done running kernel. Took " + mCurrentKernelStopwatch.Elapsed);
        }

        public override void ExecuteKernelStart(string assemblyName)
        {
            Log("Starting kernel '" + assemblyName + "'");
            mCurrentKernelStopwatch.Reset();
            mCurrentKernelStopwatch.Start();
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
        }
    }
}
