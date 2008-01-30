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
            GdbController.Instance = new GdbController(connection);
            try
            {
                GdbController.Instance.Extended();
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
            Console.WriteLine(new GetRegistersCommand().Send());
            new ReadMemoryCommand(100, 1025).Send();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
