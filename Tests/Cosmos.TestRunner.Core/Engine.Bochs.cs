using System;
using System.Collections.Specialized;
using System.IO;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.Debug.VSDebugEngine.Host;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunIsoInBochs(string iso)
        {
            var xBochsConfig = Path.Combine(mBaseWorkingDirectory, "Kernel.bochsrc");
            var xParams = new NameValueCollection();

            xParams.Add(BuildProperties.EnableBochsDebugString, "false");
            xParams.Add("ISOFile", iso);
            xParams.Add(BuildProperties.VisualStudioDebugPortString, "Pipe: Cosmos\\Serial");

            var xDebugConnector = new DebugConnectorPipeServer("Cosmos\\Serial");

            xDebugConnector.CmdChannel += ChannelPacketReceived;
            xDebugConnector.CmdStarted += () =>
                                          {
                                              DoLog("DC: Started");
                                              xDebugConnector.SendCmd(Vs2Ds.BatchEnd);
                                          };
            xDebugConnector.CmdText += s => DoLog("Text from kernel: " + s);
            xDebugConnector.CmdMessageBox += s => DoLog("MessageBox from kernel: " + s);

            var xBochs = new Bochs(xParams, false, new FileInfo(xBochsConfig));
            xBochs.Start();
            try
            {
                Console.WriteLine("Bochs started");
                Console.ReadLine();
            }
            catch
            {
                xBochs.Stop();
            }
        }

        private void ChannelPacketReceived(byte arg1, byte arg2, byte[] arg3)
        {
            Console.WriteLine("ChannelPacket received. Channel = {0}, command = {1}", arg1, arg2);
        }
    }
}
