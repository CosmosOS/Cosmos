using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.Debug.VSDebugEngine.Host;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private const int AllowedSecondsInKernel = 30;

        private void RunIsoInBochs(string iso)
        {
            var xBochsConfig = Path.Combine(mBaseWorkingDirectory, "Kernel.bochsrc");
            var xParams = new NameValueCollection();

            xParams.Add(BuildProperties.EnableBochsDebugString, "false");
            xParams.Add("ISOFile", iso);
            xParams.Add(BuildProperties.VisualStudioDebugPortString, "Pipe: Cosmos\\Serial");

            var xDebugConnector = new DebugConnectorPipeServer("Cosmos\\Serial");

            xDebugConnector.CmdChannel = ChannelPacketReceived;
            xDebugConnector.CmdStarted = () =>
                                         {
                                             OutputHandler.LogMessage("DC: Started");
                                             xDebugConnector.SendCmd(Vs2Ds.BatchEnd);
                                         };
            xDebugConnector.Error = e =>
                                    {
                                        OutputHandler.LogMessage("DC Error: " + e.ToString());
                                        OutputHandler.SetKernelTestResult(false, "DC Error");
                                        mKernelResultSet = true;
                                        mBochsRunning = false;
                                    };
            xDebugConnector.CmdText += s =>
                                       {
                                           if (s == "SYS_TestKernel_Completed")
                                           {
                                               KernelTestCompleted();
                                               return;
                                           }
                                           else if (s == "SYS_TestKernel_Failed")
                                           {
                                               KernelTestFailed();
                                               return;
                                           }
                                           else if (s == "SYS_TestKernel_AssertionSucceeded")
                                           {
                                               KernelAssertionSucceeded();
                                               return;
                                           }
                                           OutputHandler.LogMessage("Text from kernel: " + s);
                                       };
            xDebugConnector.CmdMessageBox = s =>
                                            {
                                                //OutputHandler.LogMessage("MessageBox from kernel: " + s)
                                            };
            xDebugConnector.CmdTrace = t =>
                                       {
                                       };
            xDebugConnector.CmdBreak = t =>
                                       {
                                       };

            var xBochs = new Bochs(xParams, false, new FileInfo(xBochsConfig));
            xBochs.OnShutDown = (a, b) =>
                                {
                                };
            xBochs.RedirectOutput = true;
            xBochs.LogError = s => OutputHandler.LogDebugMessage(s);
            xBochs.LogOutput = s => OutputHandler.LogDebugMessage(s);

            mBochsRunning = true;
            xBochs.Start();
            try
            {
                var xStartTime = DateTime.Now;
                mKernelResultSet = false;
                Interlocked.Exchange(ref mSucceededAssertions, 0);

                while (mBochsRunning)
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
                xBochs.Stop();
                xDebugConnector.Dispose();
                Thread.Sleep(50);
            }
        }

        private volatile bool mBochsRunning = true;
        private volatile bool mKernelResultSet;
        private int mSucceededAssertions;

        private void ChannelPacketReceived(byte arg1, byte arg2, byte[] arg3)
        {
            OutputHandler.LogMessage(String.Format("ChannelPacketReceived, Channel = {0}, Command = {1}", arg1, arg2));
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
        }

        private void KernelAssertionSucceeded()
        {
            Interlocked.Increment(ref mSucceededAssertions);
        }

        private void KernelTestFailed()
        {
            OutputHandler.SetKernelTestResult(false, "Test failed");
            mKernelResultSet = true;
            mBochsRunning = false;
        }

        private void KernelTestCompleted()
        {
            mBochsRunning = false;
        }
    }
}
