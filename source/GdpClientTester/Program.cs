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
                connection.Open();
            }
            catch
            {
                Console.WriteLine("GDB is not running");
                Console.ReadLine();
            }

            string s = "";
            while (s != "!q")
            {
                s = Console.ReadLine();
                if (s.StartsWith("!"))
                    connection.Send(s.Substring(1));
                else
                    controller.Enqueue(new GdbPacket(s));
            }
        }
    }
}
