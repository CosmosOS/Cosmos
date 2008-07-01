using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4;

namespace FrodeTest.Test
{
    public class IPv4AddressTest
    {
        public static void RunTest()
        {
            //Test parsing of IPv4 address

            Console.WriteLine("-- TESTING IPv4Address --");

            Console.Write("Parsing IP address: ");
            Console.WriteLine(IPv4Address.Parse("172.28.5.10"));
        }
    }
}
