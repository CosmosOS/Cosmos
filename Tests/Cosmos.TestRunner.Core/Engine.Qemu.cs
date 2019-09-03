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

            var xParams = new Dictionary<string, string>
            {
                {BuildPropertyNames.IsoFileString, iso}
            };

            var xDebugConnector = new DebugConnectorPipeClient("Cosmos\\Serial");
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
