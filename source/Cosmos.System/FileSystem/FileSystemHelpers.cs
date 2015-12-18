using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Debugger = Cosmos.Debug.Kernel.Debugger;
using Cosmos.Common.Extensions;

namespace Cosmos.System.FileSystem
{
    public static class FileSystemHelpers
    {
        private static Debugger mDebugger = new Debugger("FAT", "Debug");

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage, params object[] aParams)
        {
            string xMessage = aMessage;

            if (aParams != null)
            {
                aMessage = aMessage + " : ";
                for (int i = 0; i < aParams.Length; i++)
                {
                    if (aParams[i] != null)
                    {
                        string xParam = aParams[i].ToString();
                        if (!string.IsNullOrWhiteSpace(xParam))
                        {
                            xMessage = xMessage + " " + xParam;
                        }
                    }
                }
            }

            mDebugger.Send("FileSystem Debug: " + xMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage)
        {
            mDebugger.Send("FileSystem Debug: " + aMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void DebugNumber(uint aValue)
        {
            mDebugger.SendNumber(aValue);
        }

        [Conditional("COSMOSDEBUG")]
        public static void DebugNumber(ulong aValue)
        {
            mDebugger.Send(((uint)aValue).ToString() + ((uint)aValue >> 32).ToString());
        }

        [Conditional("COSMOSDEBUG")]
        public static void DevDebug(string message)
        {
            mDebugger.Send("FileSystem DevDebug: " + message);
        }
    }
}
