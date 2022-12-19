using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Debugger = Cosmos.Debug.Kernel.Debugger;
using Cosmos.Common.Extensions;

namespace Cosmos.HAL
{
    public static class TextScreenHelpers
    {
        private static Debugger mDebugger = new Debugger("TextScreen", "Debug");

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

            mDebugger.Send("TextScreen Debug: " + xMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage)
        {
            mDebugger.Send("TextScreen Debug: " + aMessage);
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
            mDebugger.Send("TextScreen DevDebug: " + message);
        }
    }
}
