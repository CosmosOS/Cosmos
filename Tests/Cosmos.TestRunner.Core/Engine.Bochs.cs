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
        private const int AllowedSecondsInKernel = 10;

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
                                        mBochsRunning = false;
                                    };
            xDebugConnector.CmdText += s => OutputHandler.LogMessage("Text from kernel: " + s);
            xDebugConnector.CmdMessageBox = s => OutputHandler.LogMessage("MessageBox from kernel: " + s);

            var xBochs = new Bochs(xParams, false, new FileInfo(xBochsConfig));
            xBochs.OnShutDown = (a, b) =>
                                {
                                };

            mBochsRunning = true;
            xBochs.Start();
            try
            {
                var xStartTime = DateTime.Now;
                var xKernelResultSet = false;

                Console.WriteLine("Bochs started");
                while (mBochsRunning)
                {
                    Thread.Sleep(50);

                    if (Math.Abs(DateTime.Now.Subtract(xStartTime).TotalSeconds) > AllowedSecondsInKernel)
                    {
                        OutputHandler.SetKernelTestResult(false, "Timeout exceeded");
                        xKernelResultSet = true;
                        break;
                    }
                }
                if (!xKernelResultSet)
                {
                    OutputHandler.SetKernelTestResult(true, null);
                }
                Console.WriteLine("Stopping bochs now");
            }
            finally
            {
                xBochs.Stop();
                xDebugConnector.Dispose();
                Thread.Sleep(50);
            }
        }

        private bool mBochsRunning = true;

        private void ChannelPacketReceived(byte arg1, byte arg2, byte[] arg3)
        {
            Console.WriteLine("ChannelPacket received. Channel = {0}, command = {1}", arg1, arg2);
        }
    }
}
