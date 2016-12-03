using System.Collections.Specialized;
using System.IO;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.Debug.VSDebugEngine.Host;

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

            var xParams = new NameValueCollection();

            xParams.Add("ISOFile", iso);
            xParams.Add(BuildPropertyNames.VisualStudioDebugPortString, "Pipe: CosmosSerial");

            var xDebugConnector = new DebugConnectorPipeClient(DebugConnectorPipeClient.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xHyperV = new HyperV(xParams, RunWithGDB, harddisk); // harddisk passed in is in vmdk format and therefore not compatible
            //var xHyperV = new HyperV(xParams, RunWithGDB);
            xHyperV.OnShutDown = (a, b) =>
            {
                mKernelRunning = false;
            };

            HandleRunning(xDebugConnector, xHyperV);
        }
    }
}
