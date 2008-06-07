using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public class MACAddressTest
    {
        public static void RunTest()
        {
            Console.WriteLine("--Testing MACAddress--");

            byte[] b = new byte[6];
            b[0] = (byte)0;
            b[1] = (byte)1;
            b[2] = (byte)2;
            b[3] = (byte)3;
            b[4] = (byte)4;
            b[5] = (byte)5;

            Cosmos.Hardware.Network.MACAddress mac = new Cosmos.Hardware.Network.MACAddress(b);

            Console.WriteLine("Mac address: " + mac.ToString());
            //Console.WriteLine("Mac address: " + mac);
            

        }
        
    }
}
