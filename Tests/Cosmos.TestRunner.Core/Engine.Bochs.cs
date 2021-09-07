using System.Collections.Generic;
using System.IO;

using Cosmos.Build.Common;
using Cosmos.Debug.DebugConnectors;
using Cosmos.Debug.Hosts;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunIsoInBochs(string iso, string harddisk, string workingDir)
        {
            if (!File.Exists(harddisk))
            {
                throw new FileNotFoundException("Harddisk file not found!", harddisk);
            }

            var xBochsConfig = Path.Combine(workingDir, "Kernel.bochsrc");
            var xParams = new Dictionary<string, string>();

            xParams.Add("ISOFile", iso);
            xParams.Add(BuildPropertyNames.VisualStudioDebugPortString, "Pipe: Cosmos\\Serial");
            xParams.Add(BuildPropertyNames.EnableBochsDebugString, RunWithGDB.ToString());
            xParams.Add(BuildPropertyNames.StartBochsDebugGui, StartBochsDebugGui.ToString());
            var xDebugConnector = new DebugConnectorPipeServer(DebugConnectorPipeServer.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xBochs = new Bochs(xParams, RunWithGDB, new FileInfo(xBochsConfig), harddisk);

            xBochs.OnShutDown = (a, b) =>
            {
                mKernelRunning = false;
            };

            xBochs.RedirectOutput = false;
            xBochs.LogError = s => OutputHandler.LogDebugMessage(s);
            xBochs.LogOutput = s => OutputHandler.LogDebugMessage(s);

            HandleRunning(xDebugConnector, xBochs);
        }
    }
}
