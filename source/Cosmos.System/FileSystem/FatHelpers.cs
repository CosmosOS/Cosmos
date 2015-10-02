using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.FileSystem
{
    public static class FatHelpers
    {
        private static Debugger mDebugger = new Debugger("FAT", "Debug");
        public static void Debug(string message)
        {
            mDebugger.Send("FAT Debug: " + message);
        }

        public static void DevDebug(string message)
        {
            mDebugger.Send("FAT DevDebug: " + message);
        }
    }
}
