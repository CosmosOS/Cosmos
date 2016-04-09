using System;
using System.Threading;
using Cosmos.Debug.Common;
using Cosmos.Debug.VSDebugEngine.Host;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        // this file contains code handling situations when a kernel is running
        // most of this is debug stub related

        private void InitializeDebugConnector(DebugConnector debugConnector)
        {
            if (debugConnector == null)
            {
                throw new ArgumentNullException("debugConnector");
            }
            debugConnector.OnDebugMsg = s => OutputHandler.LogDebugMessage(s);
            debugConnector.CmdChannel = ChannelPacketReceived;
            debugConnector.CmdStarted = () =>
                {
                    OutputHandler.LogMessage("DC: Started");
                    debugConnector.SendCmd(Vs2Ds.BatchEnd);
                };
            debugConnector.Error = e =>
                {
                    OutputHandler.LogMessage("DC Error: " + e.ToString());
                    OutputHandler.SetKernelTestResult(false, "DC Error");
                    mKernelResultSet = true;
                    mKernelRunning = false;
                };
            debugConnector.CmdText += s => OutputHandler.LogMessage("Text from kernel: " + s);
            debugConnector.CmdSimpleNumber += n => OutputHandler.LogMessage("Number from kernel: 0x" + n.ToString("X8").ToUpper());
            debugConnector.CmdSimpleLongNumber += n => OutputHandler.LogMessage("Number from kernel: 0x" + n.ToString("X8").ToUpper());
            debugConnector.CmdComplexNumber += f => OutputHandler.LogMessage("Number from kernel: " + f);
            debugConnector.CmdComplexLongNumber += d => OutputHandler.LogMessage("Number from kernel: " + d);
            debugConnector.CmdMessageBox = s => OutputHandler.LogMessage("MessageBox from kernel: " + s);
            debugConnector.CmdTrace = t => { };
            debugConnector.CmdBreak = t => { };
            debugConnector.CmdStackCorruptionOccurred = a =>
                {
                    OutputHandler.LogMessage("Stackcorruption occurred at: 0x" + a.ToString("X8"));
                    OutputHandler.SetKernelTestResult(false, "Stackcorruption occurred at: 0x" + a.ToString("X8"));
                    mKernelResultSet = true;
                    mKernelRunning = false;
                };
            debugConnector.CmdNullReferenceOccurred = a =>
                {
                    OutputHandler.LogMessage("Null Reference Exception occurred at: 0x" + a.ToString("X8"));
                    OutputHandler.SetKernelTestResult(
                        false,
                        "Null Reference Exception occurred at: 0x" + a.ToString("X8"));
                    mKernelResultSet = true;
                    mKernelRunning = false;
                };
        }

        private void HandleRunning(DebugConnector debugConnector, Base host)
        {
            if (debugConnector == null)
            {
                throw new ArgumentNullException("debugConnector");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            mKernelRunning = true;

            host.Start();
            try
            {
                var xStartTime = DateTime.Now;
                mKernelResultSet = false;
                Interlocked.Exchange(ref mSucceededAssertions, 0);

                while (mKernelRunning)
                {
                    Thread.Sleep(50);

                    if (Math.Abs(DateTime.Now.Subtract(xStartTime).TotalSeconds) > AllowedSecondsInKernel)
                    {
                        OutputHandler.SetKernelTestResult(false, "Timeout exceeded");
                        mKernelResultSet = true;
                        break;
                    }
                }
                if (!mKernelResultSet)
                {
                    OutputHandler.SetKernelTestResult(true, null);
                }
                OutputHandler.SetKernelSucceededAssertionsCount(mSucceededAssertions);
            }
            finally
            {
                Console.WriteLine("Stopping now");
                host.Stop();
                debugConnector.Dispose();
                Thread.Sleep(50);
            }
        }

        private volatile bool mKernelResultSet;
        private volatile bool mKernelResult;
        private int mSucceededAssertions;

        private void ChannelPacketReceived(byte arg1, byte arg2, byte[] arg3)
        {
            if (arg1 == 129)
            {
                // for now, skip
                return;
            }
            if (arg1 == TestController.TestChannel)
            {
                switch (arg2)
                {
                    case (byte)TestChannelCommandEnum.TestCompleted:
                        KernelTestCompleted();
                        break;
                    case (byte)TestChannelCommandEnum.TestFailed:
                        KernelTestFailed();
                        break;
                    case (byte)TestChannelCommandEnum.AssertionSucceeded:
                        KernelAssertionSucceeded();
                        break;
                }
            }
            else
            {
                OutputHandler.LogMessage(String.Format("ChannelPacketReceived, Channel = {0}, Command = {1}", arg1, arg2));
            }
        }

        private void KernelAssertionSucceeded()
        {
            Interlocked.Increment(ref mSucceededAssertions);
        }

        private void KernelTestFailed()
        {
            OutputHandler.SetKernelTestResult(false, "Test failed");
            mKernelResultSet = true;
            mKernelResult = false;
            mKernelRunning = false;
        }

        private void KernelTestCompleted()
        {
            Thread.Sleep(50);
            mKernelResultSet = true;
            mKernelResult = true;
            mKernelRunning = false;
            Console.WriteLine("Test completed");
        }
    }
}
