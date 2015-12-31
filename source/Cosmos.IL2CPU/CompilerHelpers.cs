using System;

using global::System.Diagnostics;

namespace Cosmos.IL2CPU
{
    public static class CompilerHelpers
    {
        public static event Action<string> DebugEvent;

        private static void DoDebug(string message)
        {
            if (DebugEvent != null)
            {
                DebugEvent(message);
            }
            else
            {
                global::System.Diagnostics.Debug.WriteLine(message);
            }
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage, params object[] aParams)
        {
            string xMessage = aMessage;

            if (aParams != null)
            {
                xMessage = xMessage + " : ";
                for (int i = 0; i < aParams.Length; i++)
                {
                    string xParam = aParams[i].ToString();
                    if (!string.IsNullOrWhiteSpace(xParam))
                    {
                        xMessage = xMessage + " " + xParam;
                    }
                }
            }

            DoDebug(xMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage)
        {
            DoDebug(aMessage);
        }
    }
}
