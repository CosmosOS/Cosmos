using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Shell
{
    public class Session
    {
        Security.User xUser = null;
        static ushort xSessionId;

        public Session(Security.User user)
        {
            xSessionId++;
            xUser = user;
        }

        internal void Run()
        {
            Console.Write(Prompt.LoadPrompt(xUser).PromptText());
            string command = Console.ReadLine();
            Cosmos.Driver.RTL8139.RTL8139 nic = null;

            if (command.Equals("exit"))
                return;
            else if (command.Equals("nic"))
            {
                Cosmos.Hardware.PC.Bus.PCIDevice pciNic = Cosmos.Hardware.PC.Bus.PCIBus.GetPCIDevice(0, 3, 0);
                nic = new Cosmos.Driver.RTL8139.RTL8139(pciNic);
                nic.Enable();
                Console.WriteLine("Enabled Network card");
            }
            else if (command.Equals("send"))
            {
                //Console.WriteLine("NIC TimerCount: " + nic.TimerCount);
                //Cosmos.Hardware.PC.Bus.PCIDevice pciNic = Cosmos.Hardware.PC.Bus.PCIBus.GetPCIDevice(0, 3, 0);
                Cosmos.Driver.RTL8139.PacketHeader head = new Cosmos.Driver.RTL8139.PacketHeader(0xFF);
                byte[] data = FrodeTest.Test.Mock.FakeBroadcastPacket.GetFakePacketAllHigh();
                Cosmos.Driver.RTL8139.Packet packet = new Cosmos.Driver.RTL8139.Packet(head, data);

                //nic = new Cosmos.Driver.RTL8139.RTL8139(pciNic);
                if (nic == null)
                {
                    Console.WriteLine("Enable NIC with command nic first");
                    return;
                }
                nic.Enable();
                nic.EnableRecieve();
                nic.EnableTransmit();
                nic.Transmit(packet);
            }
            else if (command.Equals("reset"))
            {
                //nic.TimerCount = 1;
                nic.SoftReset();
                nic.EnableTransmit();
                nic.EnableRecieve();
                Console.WriteLine("NIC reset");
            }
            else
                Console.WriteLine("No such systemcommand or application: " + command);

            Run(); //Recursive call
        }

        internal static Session CreateSession(FrodeTest.Security.User currentUser)
        {
            return new Session(currentUser);
        }
    }
}
