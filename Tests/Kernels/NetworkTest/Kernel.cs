using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;
using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace NetworkTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
        }

        protected override void Run()
        {
            /** IPConfig Enable test **/
            mDebugger.Send($"Testing IPConfig.Enable");
            //get nic by name
            NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0");
            if (Equals(nic, null))
            {
                mDebugger.Send($"eth0 not found.");
                TestController.Failed();
            }
            //Enable network config
            IPConfig.Enable(nic, new Address(192, 168, 1, 69), new Address(255, 255, 255, 0), new Address(192, 168, 1, 254));

            Address ip = new Address(192, 168, 1, 70);
            int port = 4242;
            string message = "Hello from Cosmos!";

            /** Udp test **/
            var xClient = new UdpClient(port);
            xClient.Connect(ip, port);
            xClient.Send(Encoding.ASCII.GetBytes(message));
            mDebugger.Send("Sent UDP packet to " + ip.ToString() + ":" + port);
            xClient.Close();

            TestController.Completed();
        }
    }
}
