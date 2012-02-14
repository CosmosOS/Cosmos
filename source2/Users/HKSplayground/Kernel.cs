using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace HKSplayground
{
    public class Kernel : Sys.Kernel
    {
        byte[] ba = {1,2,3};

        protected override void BeforeRun()
        {
            Cosmos.Hardware2.PCIBus.Init();
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }
        int i = 1;
        protected override void Run()
        {
            while (true)
            {
                if (i == 1)
                {
                    Cosmos.Hardware2.Network.Devices.AMDPCNetII.AMDPCNet.FindAll();
                    i++;
                }
                else
                {
                }
            }
        }

    }
}
