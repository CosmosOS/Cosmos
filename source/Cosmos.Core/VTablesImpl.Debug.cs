using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    partial class VTablesImpl
    {
        private static Debugger debugger = new("VTablesImpl");
        public static bool EnableDebug = false;

        private static void Debug(string message)
        {
            if (!EnableDebug)
            {
                return;
            }
            debugger.Send(message);
        }

        private static void DebugHex(string message, uint value)
        {
            if (!EnableDebug)
            {
                return;
            }

            debugger.Send(message);
            debugger.SendNumber(value);
        }

        private static void DebugAndHalt(string message)
        {
            Debug(message);
            while (true) ;

            //Debugger.DoRealHalt();
        }
    }
}