using Microsoft.VisualStudio.Shell.Interop;

using Cosmos.Debug.Common;
using Cosmos.Debug.DebugConnectors;

namespace Cosmos.VS.Windows
{
    public static class Global
    {
        /// <summary>A pipe used to send requests to the AD7Process.</summary>
        public static PipeClient PipeUp = new PipeClient(Pipes.UpName);

        public static IVsOutputWindowPane OutputPane;

        public static PipeClient ConsoleTextChannel;
    }
}
