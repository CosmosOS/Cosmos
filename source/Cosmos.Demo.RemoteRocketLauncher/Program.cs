using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;
using Cosmos.Sys.Network;

namespace Cosmos.Demo.RemoteRocketLauncher {
    class Program {
        public enum Command:byte {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3, 
            Stop = 4,
            Fire = 5
        }
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args) {
            BuildUI.Run();
        }
        #endregion

        public static void Init() {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            var xNICs = Cosmos.Hardware.Network.NetworkDevice.NetworkDevices;
            var xNIC = xNICs[0];

            xNIC.Enable();
            while (true) {
                char xCommand = (char)Console.Read();
                switch (xCommand) {
                    case 'i': {
                            SendUp();
                            break;
                        }
                    case 'k': {
                            SendDown();
                            break;
                        }
                    case 'j': {
                            SendLeft();
                            break;
                        }
                    case 'l': {
                            SendRight();
                            break;
                        }
                    case 's': {
                            SendStop();
                            break;
                        }
                    case 'f': {
                            SendFire();
                            break;
                        }
                }
            }
        }

        private static void SendStop() {
            SendMessage(Command.Stop);
            Console.WriteLine("Stopping");
        }

        private static void SendFire() {
            SendMessage(Command.Fire);
            Console.WriteLine("Firing");
        }

        private static void SendRight() {
            SendMessage(Command.Right);
            Console.WriteLine("Moving Right");
        }

        private static void SendLeft() {
            SendMessage(Command.Left);
            Console.WriteLine("Moving Left");
        }

        private static void SendDown() {
            SendMessage(Command.Down);
            Console.WriteLine("Moving Down");
        }

        private static void SendUp() {
            SendMessage(Command.Up);
            Console.WriteLine("Moving Up");
        }

        private static void SendMessage(Command aCommand) {
            var xUDP = new Cosmos.Sys.Network.UDPPacket(
                // Use a different port so it does not conflict wtih listener since we
                // are using the same IP on host for testing
                0x0A00020F, 32001 // 10.0.2.15
                , 0xFFFFFFFF, 32000 // 255.255.255.255, Broadcast
                , new byte[] { (byte)aCommand});
            var xEthernet = new EthernetPacket(xUDP.GetData()
                , 0x525400123457, 0xFFFFFFFFFFFF
                , EthernetPacket.PacketType.IP);

            var xNICs = Cosmos.Hardware.Network.NetworkDevice.NetworkDevices;
            var xNIC = xNICs[0];

            //xNIC.Enable();
            //xNIC.InitializeDriver();

            var xBytes = xEthernet.GetData();
            xNIC.QueueBytes(xBytes);
        }
    }
}