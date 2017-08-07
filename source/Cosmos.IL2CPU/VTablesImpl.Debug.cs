﻿using Cosmos.Debug.Kernel;

namespace Cosmos.IL2CPU
{
    partial class VTablesImpl
    {
        public static bool EnableDebug = false;
        private static Debugger mDebugger = new Debugger("IL2CPU", "VTablesImpl");

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
                ;

            //Debugger.DoRealHalt();
        }
    }
}
