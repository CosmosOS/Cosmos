using System;
using Cosmos.Compiler.Builder;
using HW = Cosmos.Hardware;
using Cosmos.Kernel;
using System.Collections.Generic;
using Cosmos.Sys.Network;
using System.Text;

namespace Cosmos.Playground.SSchocke {
    class Program
    {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
            BuildUI.Run();
        }
		#endregion

        static String webPage;
        static String error404;

		// Main entry point of the kernel
		public static void Init() {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            Console.WriteLine("Congratulations! You just booted SSchocke's C# code.");
            Console.WriteLine("Scanning for AMD PCNET Networks Cards...");

            HW.Network.NetworkDevice nic = null;
            foreach (HW.PCIDevice dev in HW.PCIBus.Devices)
            {
                if ((dev.VendorID == 0x1022) && (dev.DeviceID == 0x2000))
                {
                    nic = new HW.Network.Devices.AMDPCNetII.AMDPCNet(dev);
                    Console.WriteLine("Found AMD PCNet NIC on PCI " + dev.Bus + ":" + dev.Slot + ":" + dev.Function);
                    Console.WriteLine("NIC IRQ: " + dev.InterruptLine);
                    Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());
                    break;
                }
            }
            Console.WriteLine("Initializing NIC...");
            nic.Enable();

            Console.WriteLine("Initializing TCP Stack...");
            TCPIPStack.Init();
            TCPIPStack.ConfigIP(nic, new IPv4Config(new IPv4Address(192, 168, 20, 123), new IPv4Address(255, 255, 255, 0)));

            Console.WriteLine("Initializing TCP Port 80...");
            TCPIPStack.AddTcpListener(80, WebServerConnect);

            #region Setup WebServer strings
            webPage = "<html><body><h1>It works! This is a web page being hosted by your Cosmos Operating System</h1></body></html>";
            int webPageLength = webPage.Length;

            webPage = "HTTP/1.1 200 OK\r\n";
            webPage += "Content-Length: " + webPageLength + "\r\n";
            webPage += "Content-Type: text/html\r\n\r\n";
            webPage += "<html><body><h1>It works! This is a web page being hosted by your Cosmos Operating System</h1></body></html>";

            error404 = "<html><body>404 URL Not found</html>";
            int error404Length = error404.Length;

            error404 = "HTTP/1.1 404 Not Found\r\n";
            error404 += "Content-Length: " + error404Length + "\r\n";
            error404 += "Content-Type: text/html\r\n\r\n";
            error404 += "<html><body>404 URL Not found</html>";
            #endregion

            while (true)
            {
                TCPIPStack.Update();
            }

            Console.WriteLine("Press a key to shutdown...");
            Console.Read();
            Cosmos.Sys.Deboot.ShutDown();
		}

        private static void WebServerConnect(TcpClient client)
        {
            Console.WriteLine("Client(" + client.RemoteEndpoint.ToString() + ") Connected to port 80...");
            client.DataReceived = WebServer_RecvData;
        }

        private static void WebServer_RecvData(TcpClient client, byte[] data)
        {
            Console.WriteLine("Received request from " + client.RemoteEndpoint.ToString());
            StringBuilder sb = new StringBuilder(data.Length);
            for (int b = 0; b < data.Length; b++)
            {
                sb.Append((char)data[b]);
            }
            String dataString = sb.ToString();

            if (dataString.StartsWith("GET / ") == true)
            {
                client.SendString(webPage);
            }
            else
            {
                client.SendString(error404);
            }
        }
	}
}