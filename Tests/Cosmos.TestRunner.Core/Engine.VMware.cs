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
        private void RunIsoInVMware(string iso)
        {
            var xParams = new NameValueCollection();

            xParams.Add("ISOFile", iso);
            xParams.Add(BuildPropertyNames.VisualStudioDebugPortString, "Pipe: Cosmos\\Serial");
            xParams.Add(BuildPropertyNames.VMwareEditionString, "Workstation");

            var xDebugConnector = new DebugConnectorPipeServer(DebugConnectorPipeServer.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xVMware = new VMware(xParams, RunWithGDB);
            xVMware.OnShutDown = (a, b) =>
            {
            };

            HandleRunning(xDebugConnector, xVMware);
        }
    }
}
