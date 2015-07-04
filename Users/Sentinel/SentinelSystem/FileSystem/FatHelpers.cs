using System;
using Cosmos.Debug.Kernel;

namespace SentinelKernel.System.FileSystem
{
    public static class FatHelpers
    {
        private static Debugger mDebugger = new Debugger("FAT", "Debug");
        public static void Debug(string message)
        {
            //mDebugger.Send("FAT Debug: " + message);
        }
    }
}
