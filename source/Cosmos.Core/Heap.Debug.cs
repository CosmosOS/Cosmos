using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    partial class Heap
    {
        public static bool EnableDebug = false;
        private static void Debug(string message)
        {
            if (!EnableDebug)
            {
                return;
            }

            Debugger.DoSend(message);
        }

        private static int mConsoleX = 0;

        private static void DebugHex(string message, uint value)
        {
            if (!EnableDebug)
            {
                return;
            }
            Debugger.DoSend(message);
            Debugger.DoSendNumber(value);
            //Console.Write(message);
            //WriteNumberHex(value, bits);
            //NewLine();
        }

        private static void DebugAndHalt(string message)
        {
            Debugger.DoSend(message);
            while (true)
                ;
        }
    }
}
