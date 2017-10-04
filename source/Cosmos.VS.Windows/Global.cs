using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Common;
using Cosmos.Debug.DebugConnectors;

namespace Cosmos.VS.Windows
{
    public static class Global
    {
        /// <summary>A pipe used to send requests to the AD7Process.</summary>
        public static PipeClient PipeUp;

        public static EnvDTE.OutputWindowPane OutputPane;

        static Global()
        {
            PipeUp = new PipeClient(Pipes.UpName);
        }

        public static PipeClient ConsoleTextChannel;
    }
}