using System;
using System.Collections.Generic;
using HW = Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys.Network;
using System.Text;

namespace Cosmos.Playground.SSchocke
{
    public class NetworkTest
    {
        class VncClient
        {
            public enum Status { CONNECTED, RECVD_VERSION, RECVD_SECURITY, READY };

            public Status ClientStatus;

            public VncClient()
            {
                ClientStatus = Status.CONNECTED;
            }
        }

        static String webPage;
        static String error404;
        static HW.TempDictionary<VncClient> vncClients = new HW.TempDictionary<VncClient>();

        public static void Test()
        {
            HW.Network.NetworkDevice nic = null;
            if (HW.Network.NetworkDevice.NetworkDevices.Count < 1)
            {
                Console.WriteLine("No Network Interface found!!");
                Console.WriteLine("Press a key to shutdown...");
                Console.Read();
                Cosmos.Sys.Deboot.ShutDown();
            }

            nic = HW.Network.NetworkDevice.NetworkDevices[0];
            Console.WriteLine("Using NIC " + nic.Name + " [" + nic.MACAddress.ToString() + "]");

            Console.WriteLine("Initializing NIC...");
            nic.Enable();

            Console.WriteLine("Initializing TCP Stack...");
            TCPIPStack.Init();
            TCPIPStack.ConfigIP(nic, new IPv4Config(new IPv4Address(192, 168, 20, 123),
                                                    new IPv4Address(255, 255, 255, 0),
                                                    new IPv4Address(192, 168, 20, 100)));

            Console.WriteLine("Initializing TCP Port 80...");
            TCPIPStack.AddTcpListener(80, WebServerConnect);
            TCPIPStack.AddTcpListener(5900, VNCServerConnect);
            Console.WriteLine("Servers initialized");

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
            Console.WriteLine("Done. Handling requests");

            //TcpClient client = new TcpClient(new IPv4Address(192, 168, 20, 100), 80);
            bool requestDone = false;
            while (true)
            {
                /*if ((requestDone == false) && (client.Connected == true))
                {
                    client.SendString("GET /");
                    requestDone = true;
                }*/
                TCPIPStack.Update();
            }
        }

        private static void WebServerConnect(TcpClient client)
        {
            Console.WriteLine("Client(" + client.RemoteEndpoint.ToString() + ") Connected to port 80...");
            client.DataReceived = WebServer_RecvData;
            client.Disconnect = WebServerDisconnect;
        }

        private static void VNCServerConnect(TcpClient client)
        {
            Console.WriteLine("VNC Client(" + client.RemoteEndpoint.ToString() + ") Connected...");
            client.DataReceived = VNCServer_RecvData;
            client.Disconnect = VNCServerDisconnect;

            VncClient vnc = new VncClient();
            vncClients.Add(client.RemoteEndpoint.Port, vnc);

            client.SendString("RFB 003.008\n");
        }

        private static void WebServerDisconnect(TcpClient client)
        {
            Console.WriteLine("Client(" + client.RemoteEndpoint.ToString() + ") disconnected...");
            client.Close();
        }

        private static void VNCServerDisconnect(TcpClient client)
        {
            Console.WriteLine("Client(" + client.RemoteEndpoint.ToString() + ") disconnected...");
            client.Close();
        }

        private static void WebServer_RecvData(TcpClient client, byte[] data)
        {
            Console.WriteLine("Received request from " + client.RemoteEndpoint.ToString());
            var xDataChars = new char[data.Length];
            for (int b = 0; b < data.Length; b++)
            {
                xDataChars[b] = ((char)data[b]);
            }
            String dataString = new String(xDataChars);

            Console.WriteLine("Request : " + dataString);
            if (dataString.StartsWith("GET / ") == true)
            {
                client.SendString(webPage);
            }
            else
            {
                client.SendString(error404);
            }
        }

        private static void VNCServer_RecvData(TcpClient client, byte[] data)
        {
            Console.WriteLine("Received VNC packet from " + client.RemoteEndpoint.ToString());

            VncClient vnc = vncClients[client.RemoteEndpoint.Port];

            if (vnc.ClientStatus == VncClient.Status.CONNECTED)
            {
                int xLength = 0;
                var xChars = new char[data.Length + 10];
                for (int b = 0; b < data.Length; b++)
                {
                    if (data[b] == 0x0A)
                    {
                        xChars[xLength] ='<';
                        xChars[xLength+1] ='L';
                        xChars[xLength+2] ='F';
                        xChars[xLength+3] ='>';
                        xLength += 4;
                    }
                    else if (data[b] == 0x0D)
                    {
                        xChars[xLength] ='<';
                        xChars[xLength+1] ='C';
                        xChars[xLength+2] ='R';
                        xChars[xLength+3] ='>';
                        xLength += 4;
                    }
                    else
                    {
                        xChars[xLength] = (char)data[b];
                        xLength++;
                    }
                }
                String dataString = new String(xChars, 0, xLength);
                if (dataString == "RFB 003.008<LF>")
                {
                    vnc.ClientStatus = VncClient.Status.RECVD_VERSION;

                    byte[] security = new byte[] { 0x01, 0x01 };
                    client.SendData(security);

                    Console.WriteLine("Sent Security Init...");
                }
                return;
            }

            if ((vnc.ClientStatus == VncClient.Status.RECVD_VERSION))
            {
                if (data[0] == 0x01)
                {
                    Console.WriteLine("Client Accepted no security...");

                    byte[] securityResult = new byte[] { 0, 0, 0, 0 };
                    client.SendData(securityResult);

                    Console.WriteLine("Sent Security Result...");
                    vnc.ClientStatus = VncClient.Status.RECVD_SECURITY;
                    Console.WriteLine("Starting Init Phase...");
                }
                return;
            }

            if ((vnc.ClientStatus == VncClient.Status.RECVD_SECURITY))
            {
                Console.WriteLine("Client Init Received...");
                byte[] serverInit = new byte[30];

                // Frame Buffer Width = 800;
                serverInit[0] = 0x03;
                serverInit[1] = 0x20;

                // Frame Buffer Height = 600;
                serverInit[2] = 0x02;
                serverInit[3] = 0x58;

                // Server-pixel-format structure
                // 32 bits-per-pixel
                serverInit[4] = 32;
                // Depth
                serverInit[5] = 24;
                // True-Color
                serverInit[7] = 1;
                // Red-Max = 255
                serverInit[8] = 0;
                serverInit[9] = 255;
                // Green-Max = 255
                serverInit[10] = 0;
                serverInit[11] = 255;
                // Blue-Max = 255
                serverInit[12] = 0;
                serverInit[13] = 255;
                // Red-shift
                serverInit[14] = 16;
                // Green-shift
                serverInit[15] = 8;
                // Blue-shift
                serverInit[16] = 0;

                //Name Length
                serverInit[20] = 0;
                serverInit[21] = 0;
                serverInit[22] = 0;
                serverInit[23] = 6;

                // Name
                serverInit[24] = (byte)'C';
                serverInit[25] = (byte)'o';
                serverInit[26] = (byte)'s';
                serverInit[27] = (byte)'m';
                serverInit[28] = (byte)'o';
                serverInit[29] = (byte)'s';

                client.SendData(serverInit);
                Console.WriteLine("Sent Server Init...");

                vnc.ClientStatus = VncClient.Status.READY;
                return;
            }
            if (vnc.ClientStatus == VncClient.Status.READY)
            {
                byte cmd = data[0];

                switch (cmd)
                {
                    case 0:
                        Console.WriteLine("Recvd SetPixelFormat");
                        break;
                    case 2:
                        Console.WriteLine("Recvd SetEncodings");
                        break;
                    case 3:
                        Console.WriteLine("Recvd FramebufferUpdateRequest");
                        byte incremental = data[1];
                        UInt16 xpos = (UInt16)((data[2] << 8) | data[3]);
                        UInt16 ypos = (UInt16)((data[4] << 8) | data[5]);
                        UInt16 width = (UInt16)((data[6] << 8) | data[7]);
                        UInt16 height = (UInt16)((data[8] << 8) | data[9]);

                        Console.WriteLine("Request is " + (incremental == 0 ? "not incremental" : "incremental") + " update of (" +
                            xpos.ToString() + "," + ypos.ToString() + ") Width=" + width.ToString() + ", Height=" + height.ToString());

                        if( incremental == 0 )
                        {
                            byte[] update = new byte[4 + 12 + 4 + 10];
                            //Cmd = FrameBufferUpdate
                            update[0] = 0;
                            // Number-of-Rectangles
                            update[2] = 0;
                            update[3] = 1;

                            // Rectangle 1
                            // X-pos
                            update[4] = 0;
                            update[5] = 0;
                            // Y-pos
                            update[6] = 0;
                            update[7] = 0;
                            // Width
                            update[8] = 0;
                            update[9] = 0;
                            // Height
                            update[10] = 0;
                            update[11] = 0;
                            //Encoding (Try CosmosGUI)
                            update[12] = 0;
                            update[13] = 0;
                            update[14] = 0;
                            update[15] = 0x22;

                            // Rectangle Data
                            // Num Controls
                            update[16] = 0;
                            update[17] = 0;
                            update[18] = 0;
                            update[19] = 1;

                            // Control 1
                            // Control Type
                            update[20] = 1;
                            update[21] = 0;

                            update[22] = 128;
                            update[23] = 128;
                            update[24] = 128;
                            update[25] = 0;
                            update[26] = 192;
                            update[27] = 192;
                            update[28] = 192;
                            update[29] = 0;

                            client.SendData(update);
                            Console.WriteLine("Sent FrameBufferUpdate...");
                        }
                        break;
                }
            }
        }

/*        private static void WebClient_RecvData(TcpClient client, byte[] data)
        {
            Console.WriteLine("Received reply from " + client.RemoteEndpoint.ToString());
            StringBuilder sb = new StringBuilder(data.Length);
            for (int b = 0; b < data.Length; b++)
            {
                sb.Append((char)data[b]);
            }
            String dataString = sb.ToString();

            Console.WriteLine(dataString);
        }*/
    }
}
