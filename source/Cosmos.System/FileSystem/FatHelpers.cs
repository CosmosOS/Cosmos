using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.System.FileSystem
{
    using Cosmos.Common.Extensions;

    public static class FatHelpers
    {
        private static Debugger mDebugger = new Debugger("FAT", "Debug");

        //[Conditional("EXCLUDE")]
        public static void Debug(string message)
        {
            mDebugger.Send("FAT Debug: " + message);
        }

        //[Conditional("EXCLUDE")]
        public static void DebugNumber(uint value)
        {
            mDebugger.SendNumber(value);
        }

        public static void DebugNumber(ulong value)
        {
            mDebugger.Send(((uint)value).ToString() + ((uint)value >> 32).ToString());
        }

        //[Conditional("EXCLUDE")]
        public static void DevDebug(string message)
        {
            mDebugger.Send("FAT DevDebug: " + message);
        }
    }
}
