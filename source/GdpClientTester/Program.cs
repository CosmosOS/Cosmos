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

            ContinueCommand cmd = new ContinueCommand(controller);
            cmd.BeginSync();
            Console.WriteLine("Done");
        }
    }
}
