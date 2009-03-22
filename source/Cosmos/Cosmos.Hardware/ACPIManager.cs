//Doku: Blocked as if it lacks of some plugs
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86.X;

namespace Cosmos.Hardware
{
    public static class  ACPIManager
    {
        private class RSD
        {
            //byte[] Signature;
            //byte Checksum;
            //byte[] OemID = new byte[6];
            //byte Revision;
            public AddressDirect RsdtAddress;
            public uint Length;
            public AddressDirect XsdtAddress;
            //byte ExtendedChecksum;
            //byte[] reserved = new byte[3];
        }

        private class FACP
        {
            //byte[] Signature = new byte[4];
            public uint Length;
            //byte[] unneded1 = new byte[32];
            public AddressDirect DSDT;
            //byte[] unneded2 = new byte[4];
            public AddressDirect SMI_CMD;
            public byte ACPI_ENABLE;
            public byte ACPI_DISABLE;
            //byte[] unneded3 = new byte[10];
            public AddressDirect PM1_a_CNT_BLK;
            public AddressDirect PM1_b_CNT_BLK;
            //byte[] unneded3 = new byte[17];
            public byte PM1_CNT_LEN;
        }
        public static void Init()
        {
            RSD aRSD;
            try
            {
                aRSD = sysRD();
            }
            catch (Exception)
            {
                Console.WriteLine("ACPI is not supported in this system!");
            }

        }

        private static RSD sysRD()
        {
            AddressDirect aAddress;
            RSD aRSD; 
            
            aAddress = new AddressDirect(0x000E0000);
            while (aAddress.Address < 0x00100000){
                aRSD = checkRSD(aAddress);
                if (aRSD != null)
                    return aRSD;
                aAddress = new AddressDirect(aAddress.Address + 0x10);
            }
            AddressDirect aEbda = new AddressDirect(0x40E * 0x10 & 0x000FFFFF);
            aAddress=aEbda;
            while (aAddress.Address < aEbda.Address + 1024)
            {
                aRSD = checkRSD(aAddress);
                if (aRSD.RsdtAddress != null)
                    return aRSD;
                aAddress = new AddressDirect(aAddress.Address + 0x10);
            }
            //throw new Exception();
            return null;
        }

        private static RSD checkRSD(AddressDirect aAddress)
        {
            return null;
        }

        
    }
}
*/