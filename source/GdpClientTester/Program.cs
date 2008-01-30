using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.GdbClient;
using Cosmos.GdbClient.BasicCommands;
using Cosmos.GdbClient.Tools;

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
            Console.WriteLine("Running, press a key to trace.");
            Console.ReadLine();
            new BreakCommand().Send();

            while (true)
            {
                
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
