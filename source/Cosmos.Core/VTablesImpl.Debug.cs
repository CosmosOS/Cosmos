using Cosmos.Debug.Kernel;

namespace Cosmos.Core;

partial class VTablesImpl
{
    public static bool EnableDebug;
    private static readonly Debugger mDebugger = new("IL2CPU", "VTablesImpl");

    private static void Debug(string message)
    {
        if (!EnableDebug)
        {
            return;
        }

        mDebugger.Send(message);
    }

    private static void DebugHex(string message, uint value)
    {
        if (!EnableDebug)
        {
            return;
        }

        mDebugger.Send(message);
        mDebugger.SendNumber(value);
    }

    private static void DebugAndHalt(string message)
    {
        Debug(message);
        while (true)
        {
        }

        //Debugger.DoRealHalt();
    }
}
