using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.TestRunner
{
    public static class TestController
    {
        static Debugger debugger = new("Tests", "TestController");
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
            while (true)
                ;
        }

        public static void Failed()
        {
            Debugger.Send("Failed");
            Debugger.SendChannelCommand(TestChannel, (byte)TestChannelCommandEnum.TestFailed);
            Debugger.DoBochsBreak();
            while (true)
                ;
        }

        internal static void AssertionSucceeded()
        {
            Debugger.SendChannelCommand(TestChannel, (byte)TestChannelCommandEnum.AssertionSucceeded);
        }
    }
}
