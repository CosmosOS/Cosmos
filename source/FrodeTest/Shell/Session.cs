using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.Devices.RTL8139;

namespace FrodeTest.Shell
{
    public class Session
    {
        Security.User xUser = null;
        static ushort xSessionId;
        static Cosmos.Hardware.Network.Devices.RTL8139.RTL8139 nic = null;

        public Session(Security.User user)
        {
            xSessionId++;
            xUser = user;
        }

        internal void Run()
        {
            Console.Write(Prompt.LoadPrompt(xUser).PromptText());
            string command = Console.ReadLine();


            if (command.Equals("exit"))
                return;
/*            else if (command.Equals("nic"))
            {
                Cosmos.Hardware.PC.Bus.PCIDevice pciNic = Cosmos.Hardware.PC.Bus.PCIBus.GetPCIDevice(0, 3, 0);
                nic = new Cosmos.Driver.RTL8139.RTL8139(pciNic);
                nic.Enable();
                Console.WriteLine("Enabled Network card");
            }
*/            else if(command.Equals("load"))
            {
                var list = RTL8139.FindAll();
                if (list.Count != 0)
                    nic = list[0];
                else
                {
                    Console.WriteLine("Unable to find RTL8139 network card!");
                }

                Console.WriteLine("Enabling network card!");
                Console.WriteLine(nic.Name);
                Console.WriteLine("Revision: " + nic.GetHardwareRevision());
                Console.WriteLine("MAC: " + nic.MACAddress);

                
                nic.Enable();
                nic.InitializeDriver();
            }
            else if (command.Equals("send"))
            {
                if (nic == null)
                {
                    Console.WriteLine("Enable NIC with command 'load' first");
                    return;
                }               
                
                var head = new PacketHeader(0xFF);
                byte[] data = FrodeTest.Test.Mock.FakeBroadcastPacket.GetFakePacket();
                var packet = new Packet(head, data);
                
                nic.Transmit(packet);
            }
            else if (command.Equals("read"))
            {
                if (nic == null)
                {
                    Console.WriteLine("Enable NIC with command 'load' first");
                    return;
                }
                Console.WriteLine("Data in RX Buffer:");
                foreach (byte item in nic.ReadReceiveBuffer())
                {
                    Console.Write(item + ":");
                }
                //List<Packet> incomingPackets = nic.Recieve();
            }
            else if (command.Equals("info"))
            {
                if (nic == null)
                {
                    Console.WriteLine("Network card not initialized yet.");
                    return;
                }
                Console.WriteLine("Network card: " + nic.Name);
                Console.WriteLine("Hardware revision: " + nic.GetHardwareRevision());
                Console.WriteLine("MAC Address: " + nic.MACAddress);
                Console.WriteLine();
                Console.WriteLine("Loopback enabled?: " + nic.LoopbackMode.ToString());
                Console.WriteLine("NIC enabled?: " + nic.IsEnabled.ToString());
                Console.WriteLine("TX enabled?: " + nic.IsTxEnabled().ToString());
                Console.WriteLine("RX enabled?: " + nic.IsRxEnabled().ToString());
                Console.WriteLine("Promiscuous mode?: " + nic.PromiscuousMode.ToString());
                
                int xByteCount = 0;
                foreach (byte b in nic.ReadReceiveBuffer())
                {
                    if (b != 0x00)
                        xByteCount++;
                }
                Console.WriteLine("Read buffer contains " + xByteCount.ToString() + " non-zero bytes with data.");
                Console.WriteLine("Read buffer empty flag? : " + nic.IsReceiveBufferEmpty());
            }
            else if (command.Equals("reset"))
            {
                //nic.TimerCount = 1;
                nic.SoftReset();
                nic.InitializeDriver();
                Console.WriteLine("NIC has been reset");
            }
            else if (command.Equals("loop"))
            {
                Console.WriteLine("Toggeling loopback mode from : " + nic.LoopbackMode.ToString());
                nic.LoopbackMode = !nic.LoopbackMode;
                Console.WriteLine("to: " + nic.LoopbackMode.ToString());
            }

            else if (command.Equals("crash"))
            {
                throw new Exception("User forced an Exception", new Exception("Inner bug"));
            }
            else if (command.Equals("prom"))
            {
                nic.PromiscuousMode = !nic.PromiscuousMode;
            }

            else if (command.Equals("help"))
            {
                Console.WriteLine("Valid commands: load, send, loop, reset or exit");
            }
            else
                Console.WriteLine("No such systemcommand or application: " + command + ". Try typing 'help'.");

            Run(); //Recursive call
        }

        internal static Session CreateSession(FrodeTest.Security.User currentUser)
        {
            return new Session(currentUser);
        }
    }
}
