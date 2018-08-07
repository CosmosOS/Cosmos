using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Common;
using Cosmos.Debug.DebugConnectors;
using Cosmos.Debug.Hosts;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunIsoInVMware(string iso, string harddisk)
        {
            if (!File.Exists(harddisk))
            {
                throw new FileNotFoundException("Harddisk file not found!", harddisk);
            }

            var xParams = new Dictionary<string, string>();

            xParams.Add("ISOFile", iso);
            xParams.Add(BuildPropertyNames.VisualStudioDebugPortString, "Pipe: Cosmos\\Serial");
            xParams.Add(BuildPropertyNames.VMwareEditionString, "Workstation");

            var xDebugConnector = new DebugConnectorPipeServer(DebugConnectorPipeServer.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xVMware = new VMware(xParams, RunWithGDB, harddisk);
            xVMware.OnShutDown = (a, b) =>
            {
                mKernelRunning = false;
            };

            HandleRunning(xDebugConnector, xVMware);
        }
    }
}
