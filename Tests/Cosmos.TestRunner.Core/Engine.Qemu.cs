using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Build.Common;
using Cosmos.Debug.DebugConnectors;
using Cosmos.Debug.Hosts;

namespace Cosmos.TestRunner.Core
{
    partial class Engine
    {
        private void RunIsoInQemu(string iso, string harddisk, string workingDir)
        {
            if (!File.Exists(harddisk))
            {
                throw new FileNotFoundException("Harddisk file not found!", harddisk);
            }

            var xBochsConfig = Path.Combine(workingDir, "Kernel.bochsrc");
            var xParams = new Dictionary<string, string>();

            xParams.Add("ISOFile", iso);
            xParams.Add(BuildPropertyNames.VisualStudioDebugPortString, @"Pipe: Cosmos\Serial");
            xParams.Add(BuildPropertyNames.EnableBochsDebugString, RunWithGDB.ToString());
            var xDebugConnector = new DebugConnectorPipeServer(DebugConnectorPipeServer.DefaultCosmosPipeName);
            InitializeDebugConnector(xDebugConnector);

            var xQemu = new Qemu(xParams, RunWithGDB, harddisk)
            {
                OnShutDown = (a, b) => { mKernelRunning = false; },
                RedirectOutput = false,
                LogError = s => OutputHandler.LogDebugMessage(s),
                LogOutput = s => OutputHandler.LogDebugMessage(s)
            };

            HandleRunning(xDebugConnector, xQemu);
        }
    }
}
