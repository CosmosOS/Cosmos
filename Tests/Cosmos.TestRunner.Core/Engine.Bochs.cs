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
        private void RunIsoInBochs(string iso)
        {
            var xBochsConfig = Path.Combine(mBaseWorkingDirectory, "Kernel.bochsrc");
            var xParams = new NameValueCollection();

            xParams.Add(BuildProperties.EnableBochsDebugString, "false");
            xParams.Add("ISOFile", iso);
            xParams.Add(BuildProperties.VisualStudioDebugPortString, "Pipe: Cosmos\\Serial");

            var xDebugConnector = new DebugConnectorPipeServer(DebugConnectorPipeServer.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xBochs = new Bochs(xParams, false, new FileInfo(xBochsConfig));
            xBochs.OnShutDown = (a, b) =>
                                {
                                };
            xBochs.RedirectOutput = true;
            xBochs.LogError = s => OutputHandler.LogDebugMessage(s);
            xBochs.LogOutput = s => OutputHandler.LogDebugMessage(s);

            mBochsRunning = true;

            HandleRunning(xDebugConnector, xBochs);
        }

        private volatile bool mBochsRunning = true;

    }
}
