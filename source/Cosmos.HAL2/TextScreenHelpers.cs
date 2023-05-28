using Debugger = Cosmos.Debug.Kernel.Debugger;
using System.Diagnostics;

namespace Cosmos.HAL
{
    public static class TextScreenHelpers
    {
        private static Debugger debugger = new("Debug");

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage, params object[] aParams)
        {
            string xMessage = aMessage;

            if (aParams != null)
            {
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

            debugger.Send("TextScreen Debug: " + xMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage)
        {
            debugger.Send("TextScreen Debug: " + aMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void DebugNumber(uint aValue)
        {
            debugger.SendNumber(aValue);
        }

        [Conditional("COSMOSDEBUG")]
        public static void DebugNumber(ulong aValue)
        {
            debugger.Send(((uint)aValue).ToString() + ((uint)aValue >> 32).ToString());
        }

        [Conditional("COSMOSDEBUG")]
        public static void DevDebug(string message)
        {
            debugger.Send("TextScreen DevDebug: " + message);
        }
    }
}