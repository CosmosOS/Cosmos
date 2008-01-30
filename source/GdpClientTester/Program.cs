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
            Console.WriteLine("Running, press a key to break.");
            Console.ReadLine();
            new BreakCommand().Send();
            X86Registers regs = X86Registers.FromString(new GetRegistersCommand().Send());

            //Console.WriteLine(Convert.ToBase64String(new ReadMemoryCommand(100, 20).Send()));

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
