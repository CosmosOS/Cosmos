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
                var list = RTL8139.FindRTL8139Devices();
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
            else if (command.Equals("reset"))
            {
                //nic.TimerCount = 1;
                nic.SoftReset();
                //nic.InitializeDriver();
                Console.WriteLine("NIC has been reset");
            }
            else if(command.Equals("loop"))
            {
                Console.WriteLine("Toggeling loopback mode from : " + nic.GetLoopbackMode().ToString());
                nic.SetLoopbackMode(!nic.GetLoopbackMode());
                Console.WriteLine("to: " + nic.GetLoopbackMode().ToString());
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
