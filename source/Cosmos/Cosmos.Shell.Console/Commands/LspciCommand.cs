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
            
        var xDeviceIDs = new Cosmos.Hardware.Bus.PCIBus.DeviceIDs();
      
            foreach (PCIDevice xPCIDevice in Cosmos.Hardware.PC.Bus.PCIBus.Devices.ToArray())
            {
                string xVendor = xDeviceIDs.FindVendor(xPCIDevice.VendorID);

                if (xVendor == default(string))
                    xVendor = ToHex(xPCIDevice.VendorID, 4);

                System.Console.Write(xPCIDevice.Bus + "-" + xPCIDevice.Slot + "-" + xPCIDevice.Function);
                System.Console.Write(" " + xVendor + ":" + ToHex(xPCIDevice.DeviceID, 4));
                System.Console.WriteLine(" Type: " + xPCIDevice.HeaderType);
                //                                /*Enum.GetName(typeof(PCIHeaderType), */ xPCIDevice.HeaderType/*) */);
                //                           Console.WriteLine(" Status: " + xPCIDevice.Status + " " +
                //                               /*Enum.GetName(typeof(PCIStatus),  */xPCIDevice.Status/*) */);
                //                            Console.WriteLine(" Command: " + xPCIDevice.Command + " " +
                //                                /*Enum.GetName(typeof(PCICommand), */xPCIDevice.Command /* ) */);
                System.Console.Write(" Class [" + ToHex((UInt32)((xPCIDevice.ClassCode << 8) | xPCIDevice.SubClass), 4) + "] " + xPCIDevice.GetClassInfo());
                System.Console.WriteLine();
                System.Console.Write(" Memory: ");

                for (byte i = 0; i < xPCIDevice.GetNumberOfBaseAddresses(); i++)
                {
                    System.Console.Write(ToHex(xPCIDevice.GetBaseAddress(i), 8));
                    System.Console.Write(" ");
                }

                System.Console.WriteLine();

                System.Console.Write(" Flags: ");

                for (byte i = 0; i < xPCIDevice.GetNumberOfBaseAddresses(); i++)
                {
                    UInt32 addr = xPCIDevice.GetBaseAddress(i);
                    xPCIDevice.SetBaseAddress(i, 0xffffffff);
                    System.Console.Write(ToHex(xPCIDevice.GetBaseAddress(i), 8));
                    xPCIDevice.SetBaseAddress(i, addr);
                    System.Console.Write(" ");
                }

                System.Console.WriteLine();


            }
        }
          static string ToHex(UInt32 num, int length)
        {
            char[] ret = new char[length];
            UInt32 cpy = num;

            for (int index = length - 1; index >= 0; index--)
            {
                ret[index] = hex(cpy & 0xf);
                cpy = cpy / 16;
            }

            return "0x" + new string(ret);
        }

        private static char hex(uint p)
        {
            switch (p)
            {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'a';
                case 11:
                    return 'b';
                case 12:
                    return 'c';
                case 13:
                    return 'd';
                case 14:
                    return 'e';
                case 15:
                    return 'f';
            }
            return ' ';
        }

    


        public override void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.WriteLine(" " + Summary);
        }
    }
}
