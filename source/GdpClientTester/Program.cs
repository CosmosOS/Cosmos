using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.GdbClient;

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

            string s = "";
            while (s != "!q")
            {
                s = Console.ReadLine();
                controller.Enqueue(new GdbPacket(s));
            }
        }
    }
}
