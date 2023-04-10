
using Cosmos.Debug.Kernel;
using System;

namespace Cosmos.TestRunner
{
    public static class TestController
    {
        static readonly Debugger debugger = new("TestController");

        internal static Debugger Debugger
        {
            get
            {
                return debugger;
            }
        }

        public const byte TestChannel = 255;
        public static void Completed()
        {
            Console.WriteLine("Sending test completed now");
            Debugger.SendChannelCommand(TestChannel, (byte)TestChannelCommandEnum.TestCompleted);
            Debugger.Send("Test completed");
            Console.WriteLine("Test completed");
            while (true) ;
        }

        public static void Failed()
        {
            Debugger.Send("Failed");
            Debugger.SendChannelCommand(TestChannel, (byte)TestChannelCommandEnum.TestFailed);
            Debugger.DoBochsBreak();
            while (true) ;
        }

        internal static void AssertionSucceeded()
        {
            Debugger.SendChannelCommand(TestChannel, (byte)TestChannelCommandEnum.AssertionSucceeded);
        }
    }
}
