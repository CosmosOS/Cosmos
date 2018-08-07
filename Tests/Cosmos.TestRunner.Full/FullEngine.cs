using System;
using System.Collections.Generic;
using System.Linq;

using Cosmos.TestRunner.Core;

using Cosmos.IL2CPU;

namespace Cosmos.TestRunner.Full
{
    public class FullEngine : Engine
    {
        public FullEngine(IEngineConfiguration aEngineConfiguration)
            : base(aEngineConfiguration)
        {
        }

        protected override void RunIL2CPUInProc(
            IEnumerable<string> args,
            Action<string> logMessage,
            Action<string> logError)
        {
            // ensure we're using the referenced (= solution) version
            CosmosAssembler.ReadDebugStubFromDisk = false;

            Program.Run(args.ToArray(), logMessage, logError);
        }
    }
}
