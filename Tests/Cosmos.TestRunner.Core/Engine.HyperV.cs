using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Common;
using Cosmos.Debug.DebugConnectors;
using Cosmos.Debug.Hosts;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunIsoInHyperV(string iso, string harddisk)
        {
            if (!File.Exists(harddisk))
            {
                throw new FileNotFoundException("Harddisk file not found!", harddisk);
            }

            var xParams = new Dictionary<string, string>();

            xParams.Add("ISOFile", iso);
            xParams.Add(BuildPropertyNames.VisualStudioDebugPortString, "Pipe: CosmosSerial");

            var xDebugConnector = new DebugConnectorPipeClient(DebugConnectorPipeClient.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xHyperV = new HyperV(xParams, RunWithGDB, harddisk);
            xHyperV.OnShutDown = (a, b) =>
            {
                mKernelRunning = false;
            };

            HandleRunning(xDebugConnector, xHyperV);
        }
    }
}
