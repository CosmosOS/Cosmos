using System;
using System.Diagnostics;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.HAL;

public static class TextScreenHelpers
{
    private static readonly Debugger mDebugger = new("TextScreen", "Debug");

    [Conditional("COSMOSDEBUG")]
    public static void Debug(string aMessage, params object[] aParams)
    {
        var xMessage = aMessage;

        if (aParams != null)
        {
            aMessage = aMessage + " : ";
            for (var i = 0; i < aParams.Length; i++)
            {
                if (aParams[i] != null)
                {
                    var xParam = aParams[i].ToString();
                    if (!String.IsNullOrWhiteSpace(xParam))
                    {
                        xMessage = xMessage + " " + xParam;
                    }
                }
            }
        }

        mDebugger.Send("TextScreen Debug: " + xMessage);
    }

    [Conditional("COSMOSDEBUG")]
    public static void Debug(string aMessage) => mDebugger.Send("TextScreen Debug: " + aMessage);

    [Conditional("COSMOSDEBUG")]
    public static void DebugNumber(uint aValue) => mDebugger.SendNumber(aValue);

    [Conditional("COSMOSDEBUG")]
    public static void DebugNumber(ulong aValue) => mDebugger.Send((uint)aValue + ((uint)aValue >> 32).ToString());

    [Conditional("COSMOSDEBUG")]
    public static void DevDebug(string message) => mDebugger.Send("TextScreen DevDebug: " + message);
}
