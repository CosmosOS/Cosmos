using System;
using System.Text;
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
            debugConnector.ConnectionLost = ex =>
            {
                OutputHandler.LogError($"DC: Connection lost. {ex.Message}");
            };
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
            debugConnector.CmdSimpleLongNumber += n => OutputHandler.LogMessage("Number from kernel: 0x" + n.ToString("X16").ToUpper());
            debugConnector.CmdComplexNumber += f => OutputHandler.LogMessage("Number from kernel: 0x" + f.ToString("X8").ToUpper());
            debugConnector.CmdComplexLongNumber += d => OutputHandler.LogMessage("Number from kernel: 0x" + d.ToString("X16").ToUpper());
            debugConnector.CmdMessageBox = s => OutputHandler.LogMessage("MessageBox from kernel: " + s);
            debugConnector.CmdKernelPanic = n =>
                                            {
                                                OutputHandler.LogMessage("Kernel panic! Nummer = " + n);
                                                // todo: add core dump here, call stack.
                                            };
            debugConnector.CmdTrace = t => { };
            debugConnector.CmdBreak = t => { };
            debugConnector.CmdStackCorruptionOccurred = a =>
                {
                    OutputHandler.LogMessage("Stackcorruption occurred at: 0x" + a.ToString("X8"));
                    OutputHandler.SetKernelTestResult(false, "Stackcorruption occurred at: 0x" + a.ToString("X8"));
                    mKernelResultSet = true;
                    mKernelRunning = false;
                };
            debugConnector.CmdStackOverflowOccurred = a =>
                                                      {
                                                          OutputHandler.LogMessage("Stack overflow occurred at: 0x" + a.ToString("X8"));
                                                          OutputHandler.SetKernelTestResult(false, "Stack overflow occurred at: 0x" + a.ToString("X8"));
                                                          mKernelResultSet = true;
                                                          mKernelRunning = false;
                                                      };
            debugConnector.CmdNullReferenceOccurred = a =>
                {
                    OutputHandler.LogMessage("Null Reference Exception occurred at: 0x" + a.ToString("X8"));
                    OutputHandler.SetKernelTestResult(false, "Null Reference Exception occurred at: 0x" + a.ToString("X8"));
                    mKernelResultSet = true;
                    mKernelRunning = false;
                };
            debugConnector.CmdCoreDump = b =>
            {
                string xCallStack = "";
                string xRegisters = "";
                int i = 0;

                OutputHandler.LogMessage("Core dump:");
                string eax = "EAX = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string ebx = "EBX = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string ecx = "ECX = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string edx = "EDX = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string edi = "EDI = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string esi = "ESI = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string ebp = "EBP = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string eip = "EIP = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                string esp = "ESP = 0x" +
                             b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                             b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                i += 4;
                OutputHandler.LogMessage(eax + " " + ebx + " " + ecx + " " + edx);
                OutputHandler.LogMessage(edi + " " + esi);
                OutputHandler.LogMessage(ebp+ " " + esp+ " " + eip);
                OutputHandler.LogMessage("");

                while (i < b.Length)
                {
                    string xAddress = "0x" +
                                      b[i + 3].ToString("X2") + b[i + 2].ToString("X2") +
                                      b[i + 0].ToString("X2") + b[i + 1].ToString("X2");
                    xCallStack += xAddress + " ";
                    if ((i != 0) &&(i%12 == 0))
                    {
                        OutputHandler.LogMessage(xCallStack.Trim());
                        xCallStack = "";
                    }
                    i += 4;
                }
                if (xCallStack != "")
                {
                    OutputHandler.LogMessage(xCallStack.Trim());
                    xCallStack = "";
                }
            };

            if (RunWithGDB)
            {
                debugConnector.CmdInterruptOccurred = a =>
                                                      {
                                                          OutputHandler.LogMessage($"Interrupt {a} occurred");
                                                      };
            }
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

                if (mKernelResultSet)
                {
                    OutputHandler.SetKernelTestResult(true, null);
                    OutputHandler.SetKernelSucceededAssertionsCount(mSucceededAssertions);
                }
                else
                {
                    KernelTestFailed();
                }
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
            OutputHandler.SetKernelTestResult(true, "Test completed");
            mKernelResultSet = true;
            mKernelResult = true;
            mKernelRunning = false;
        }
    }
}
