using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.VS.ProjectSystem
{
    internal class Logger
    {
        public static event Action<string> LogEvent;

        private static void DoDebug(string message)
        {
            message = $"COSMOS: {message}";
            if (LogEvent != null)
            {
                LogEvent(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        [Conditional("COSMOSDEBUG")]
        public static void Debug(string aMessage, params object[] aParams)
        {
            string xMessage = $"DEBUG {aMessage}";
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
            string xMessage = $"DEBUG {aMessage}";
            DoDebug(xMessage);
        }

        [Conditional("COSMOSDEBUG")]
        public static void TraceMethod(MethodBase aMethod)
        {
            string xMessage = $"TRACE Class {aMethod.DeclaringType}, Method {aMethod}";
            DoDebug(xMessage);
        }
    }
}
