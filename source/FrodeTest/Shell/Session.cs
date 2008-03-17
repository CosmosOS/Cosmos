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
            else if (command.Equals("timer"))
            {
                Console.WriteLine("NIC TimerCount: " + nic.TimerCount);
            }
            else if (command.Equals("reset"))
            {
                nic.TimerCount = 1;
                Console.WriteLine("NIC TimerCount: " + nic.TimerCount);
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
