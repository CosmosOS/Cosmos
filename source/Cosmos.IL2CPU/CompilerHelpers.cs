using global::System.Diagnostics;

namespace Cosmos.IL2CPU
{
    public static class CompilerHelpers
    {
        private static global::Cosmos.Debug.Kernel.Debugger mDebugger = new global::Cosmos.Debug.Kernel.Debugger("IL2CPU", "Debug");

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage, params object[] aParams)
        {
            string xMessage = aMessage;

            if (aParams != null)
            {
                aMessage = aMessage + " : ";
                for (int i = 0; i < aParams.Length; i++)
                {
                    string xParam = aParams[i].ToString();
                    if (!string.IsNullOrWhiteSpace(xParam))
                    {
                        aMessage = aMessage + " " + xParam;
                    }
                }
            }

            mDebugger.Send("FileSystem Trace: " + aMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage)
        {
            mDebugger.Send("FAT Debug: " + aMessage);
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
            mDebugger.Send("FAT DevDebug: " + message);
        }
    }
}
