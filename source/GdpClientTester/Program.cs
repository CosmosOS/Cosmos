using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.GdbClient;
using Cosmos.GdbClient.BasicCommands;

namespace GdpClientTester
{
    class Program
    {
        static void Main(string[] args)
        {
            GdbConnection connection = new GdbConnection();
            GdbController controller = new GdbController(connection);
            try
            {
                controller.Extended();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("GDB not running");
                return;
            }

            new ContinueCommand().Send();
            Console.WriteLine("Running, press a key to break.");
            Console.ReadLine();
            new BreakCommand().Send();
            new GetRegistersCommand().Send();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
