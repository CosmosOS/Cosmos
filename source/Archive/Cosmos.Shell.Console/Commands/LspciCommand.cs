using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.PC.Bus;


namespace Cosmos.Shell.Console.Commands
{
    public class LspciCommand : CommandBase
    {
        public override string Name
        {
            get { return "lspci"; }
        }

        public override string Summary
        {
            get { return "Lists pci devices."; }
        }

        public override void Execute(string param)
        {
            //Cosmos.Hardware.PC.Bus.PCIBus.DebugLSPCI();
            
        }  
    


        public override void Help()
        {
            //System.Console.WriteLine(Name);
            //System.Console.WriteLine(" " + Summary);

        }
    }
}
