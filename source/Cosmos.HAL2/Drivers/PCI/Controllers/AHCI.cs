using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.Drivers.PCI.SATA
{
    public struct GenericRegisters
    {
        public uint CAP;
        public uint GHC;
        public uint IS;
        public uint PI;
        public uint VS;
        public uint CCC_CTL;
        public uint CCC_PORTS;
        public uint EM_LOC;
        public uint EM_CTL;
        public uint CAP2;
        public uint BOHC;
        public byte[] Reserved0;
        public byte[] VendorSpecific;
        public PortRegisters[] Ports;
    }

    public struct PortRegisters
    {
        public uint CLB;
        public uint CLBU;
        public uint FB;
        public uint FBU;
        public uint IS;
        public uint IE;
        public uint CMD;
        public uint Reserved0;
        public uint TFD;
        public uint SIG;
        public uint SSTS;
        public uint SCTL;
        public uint SERR;
        public uint SACT;
        public uint CI;
        public uint SNTF;
        public uint FBS;
        public uint[] Reserved1;
        public uint[] VendorSpecific;
    }

    public enum FISType
    {
        FIS_Type_RegisterH2D = 0x27, // Register FIS: Host to Device
        FIS_Type_RegisterD2H = 0x34, // Register FIS: Device to Host
        FIS_Type_DMA_Activate = 0x39, // DMA Activate
        FIS_Type_DMA_Setup = 0x41, // DMA Setup: Device to Host
        FIS_Type_Data = 0x46, // Data FIS: Bidirectional
        FIS_Type_BIST = 0x58, // BIST
        FIS_Type_PIO_Setup = 0x5F, // PIO Setup: Device to Host
        FIS_Type_DeviceBits = 0xA1  // Device bits
    }

    public enum PortType
    {
        Nothing = 0x00,
        SATA = 0x01,
        SATAPI = 0x02,
        SEMB = 0x03,
        PM = 0x04
    }
    
    //public struct FIS_REG_H2D
    //{
    //    public byte FISType;
    //
    //    public byte Options;
    //    // Reserved
    //    //public byte CMDCTRL;
    //    public byte Command;
    //    public byte FeatureL;
    //
    //    public byte LBA0;
    //    public byte LBA1;
    //    public byte LBA2;
    //    public byte Device;
    //
    //    public byte LBA3;
    //    public byte LBA4;
    //    public byte LBA5;
    //    public byte FeatureH;
    //
    //    public byte CountL;
    //    public byte CountH;
    //    public byte ICC;
    //    public byte Control;
    //
    //    public byte Res2;
    //}

    //public struct FIS_REG_D2H {
    //    public byte FISType;
    //
    //    public byte Options;
    //    // Reserved
    //    //public byte InterruptBit;
    //    // Rseerved
    //    public byte Status;
    //    public byte Error;
    //
    //    public byte LBA0;
    //    public byte LBA1;
    //    public byte LBA2;
    //    public byte Device;
    //
    //    public byte LBA3;
    //    public byte LBA4;
    //    public byte LBA5;
    //    // Reserved
    //
    //    public byte CountL;
    //    public byte CountH;
    //    public byte ICC;
    //    public ushort Res2;
    //    // Reserved
    //    public uint Res3;
    //    // Reserved
    //}

    //public struct FIS_DATA {
    //    public byte FISType;
    //
    //    public byte Options;
    //    // Reserved
    //
    //    // Reserved
    //    public byte Payload;
    //}

    // Unused
    //public struct PIO_SETUP {
    //public byte FISType;
    //
    //public byte Options;
    //// Reserved
    //public byte DataTransferDirection;
    //public byte InterruptBit;
    //// Reserved
    //
    //public byte Status;
    //public byte Error;
    //
    //public byte LBA0;
    //public byte LBA1;
    //public byte LBA2;
    //public byte Device;
    //
    //public byte LBA3;
    //public byte LBA4;
    //public byte LBA5;
    //// Reserved
    //
    //public byte CountL;
    //public byte CountH;
    //// Reserved
    //public byte NewStatus;
    //
    //public byte TransferCount; // System.int16 = WORD!
    //// Reserved
    //}

    //public struct DMA_SETUP {
    //public byte FISType;
    //
    //public byte PortMultiplier;
    //// Reserved
    //public byte DataTransferDirection;
    //public byte InterruptBit;
    //public byte AutoActivate; // Specifies if DMA Activate FIS is needed
    //
    //// Reserved
    //
    //public byte DMABufferIdentifier; // System.int64 = QWORD!
    //
    //// Reserved
    //
    //public byte DMABufferOffset; // System.int32 = DWORD!
    //
    //public byte TransferCount; // System.int32 = DWORD!
    //
    //// Reserved
    //}

    public class AHCI
    {
        public const uint SSTS_ADDRESS        = 0x00000133;
        public const uint SATA_SIG_ATA        = 0x00000101; // SATA drive
        public const uint SATA_SIG_ATAPI      = 0xEB140101; // SATAPI drive
        public const uint SATA_SIG_SEMB       = 0xC33C0101; // Enclosure management bridge
        public const uint SATA_SIG_PM         = 0x96690101; // Port multiplier
        public const uint PORT_DET_PRESENT     = 0x00000003;
        public const uint PORT_IPM_ACTIVE      = 0x00000001;
        public static uint PortLocation;

        public static PCIDevice mAHCIDevice = HAL.PCI.GetDeviceClass(0x01, 0x06);
        public static uint BAR5 = mAHCIDevice.BaseAddressBar[5].BaseAddress;
        public static MemoryBlock mAHCIMemory = new MemoryBlock(BAR5, 0x10FF);
        public static MemoryBlock mAHCIPortMemory = new MemoryBlock(BAR5 + PortLocation, 0x10FF);
        internal static Debugger mAHCIDebugger = new Debugger("HAL", "AHCI");

        internal class PortHelper
        {
            public static PortRegisters GetPort(int aPortNumber)
            {
                PortLocation = (uint)(aPortNumber);
                if (aPortNumber == 00) PortLocation = 0x0100;
                else if (aPortNumber == 01) PortLocation = 0x0180;
                else if (aPortNumber == 02) PortLocation = 0x0200;
                else if (aPortNumber == 03) PortLocation = 0x0280;
                else if (aPortNumber == 04) PortLocation = 0x0300;
                else if (aPortNumber == 05) PortLocation = 0x0380;
                else if (aPortNumber == 06) PortLocation = 0x0400;
                else if (aPortNumber == 07) PortLocation = 0x0480;
                else if (aPortNumber == 08) PortLocation = 0x0500;
                else if (aPortNumber == 09) PortLocation = 0x0580;
                else if (aPortNumber == 10) PortLocation = 0x0600;
                else if (aPortNumber == 11) PortLocation = 0x0680;
                else if (aPortNumber == 12) PortLocation = 0x0700;
                else if (aPortNumber == 13) PortLocation = 0x0780;
                else if (aPortNumber == 14) PortLocation = 0x0800;
                else if (aPortNumber == 15) PortLocation = 0x0880;
                else if (aPortNumber == 16) PortLocation = 0x0900;
                else if (aPortNumber == 17) PortLocation = 0x0980;
                else if (aPortNumber == 18) PortLocation = 0x0A00;
                else if (aPortNumber == 19) PortLocation = 0x0A80;
                else if (aPortNumber == 20) PortLocation = 0x0B00;
                else if (aPortNumber == 21) PortLocation = 0x0B80;
                else if (aPortNumber == 22) PortLocation = 0x0C00;
                else if (aPortNumber == 23) PortLocation = 0x0C80;
                else if (aPortNumber == 24) PortLocation = 0x0D00;
                else if (aPortNumber == 25) PortLocation = 0x0D80;
                else if (aPortNumber == 26) PortLocation = 0x0E00;
                else if (aPortNumber == 27) PortLocation = 0x0E80;
                else if (aPortNumber == 28) PortLocation = 0x0F00;
                else if (aPortNumber == 29) PortLocation = 0x0F80;
                else if (aPortNumber == 30) PortLocation = 0x1000;
                else if (aPortNumber == 31) PortLocation = 0x1080;
                PortRegisters Port = new PortRegisters()
                {
                    CLB            = mAHCIPortMemory[0x00],
                    CLBU           = mAHCIPortMemory[0x04],
                    FB             = mAHCIPortMemory[0x08],
                    FBU            = mAHCIPortMemory[0x0C],
                    IS             = mAHCIPortMemory[0x10],
                    IE             = mAHCIPortMemory[0x14],
                    CMD            = mAHCIPortMemory[0x18],
                    Reserved0      = mAHCIPortMemory[0x1C],
                    TFD            = mAHCIPortMemory[0x20],
                    SIG            = mAHCIPortMemory[0x24],
                    SSTS           = mAHCIPortMemory[0x28],
                    SCTL           = mAHCIPortMemory[0x2C],
                    SERR           = mAHCIPortMemory[0x30],
                    SACT           = mAHCIPortMemory[0x34],
                    CI             = mAHCIPortMemory[0x38],
                    SNTF           = mAHCIPortMemory[0x3C],
                    FBS            = mAHCIPortMemory[0x40],
                    Reserved1      = GetValueArray(0x44, 11),
                    VendorSpecific = GetValueArray(0x70, 4)
                };
                return Port;
            }
            public static uint[] GetValueArray(uint aStartAddress, int aAmount)
            {
                uint[] FinishedArray = new uint[aAmount];

                for (uint ui = aStartAddress; ui < aAmount; ui += 0x04)
                {
                    for (int i = 0; i < aAmount; i++)
                    {
                        FinishedArray[i] = mAHCIPortMemory[ui];
                    }
                }
                return FinishedArray;
            }
            public static byte[] GetByteValueArray(uint aStartAddress, int aAmount)
            {
                byte[] FinishedArray = new byte[aAmount];

                for (uint ui = aStartAddress; ui < aAmount; ui += 0x04)
                {
                    for (int i = 0; i < aAmount; i++)
                    {
                        FinishedArray[i] = mAHCIPortMemory.Bytes[ui];
                    }
                }
                return FinishedArray;
            }
        }

        private void SearchForDisks(GenericRegisters aGeneric)
        {
            // Search for disks
            var xImplementedPort = aGeneric.PI;
            var xPort = 0;
            var xSupportedPorts = 0x1F;
            while (xPort <= xSupportedPorts)
            {
                if (xImplementedPort != 0)
                {
                    PortType dt = CheckPortType(PortHelper.GetPort(xPort));
                    var xPortString = "0:" + xPort;
                    if (dt == PortType.SATA) // If Port Type was SATA.
                    {
                        mAHCIDebugger.Send("SATA drive found at port " + xPortString);
                        Console.WriteLine("SATA Drive found at port " + xPortString);
                    }
                    else if (dt == PortType.SATAPI) // If Port Type was SATAPI.
                    {
                        mAHCIDebugger.Send("SATAPI drive found at port " + xPortString);
                        Console.WriteLine("CD/DVD Drive found at port " + xPortString);
                    }
                    else if (dt == PortType.SEMB) // If Port Type was SEMB.
                    {
                        mAHCIDebugger.Send("SEMB drive found at port " + xPortString);
                        Console.WriteLine("SEMB Drive found at port " + xPortString);
                    }
                    else if (dt == PortType.PM) // If Port Type was Port Mulitplier.
                    {
                        mAHCIDebugger.Send("Port Multiplier drive found at port " + xPortString);
                        Console.WriteLine("Port Multiplier Drive found at port " + xPortString);
                    }
                    else if (dt == PortType.Nothing) // If Nothing in this Port.
                        mAHCIDebugger.Send("No drive found at port " + xPortString);
                    else // If Implemented Port value was not zero and not one of the above.
                        mAHCIDebugger.Send("Unknown drive found at port " + xPortString);
                }
                xPort++;
                xImplementedPort >>= 1;
            }
        }

        private PortType CheckPortType(PortRegisters Port) 
        {
            uint Signature = Port.SIG;
            uint SATAStatus = Port.SSTS;

            var xIPM = (byte)((SATAStatus >> 8) & 0x0F);
            var xDET = (byte)(SATAStatus & 0x0F);
            mAHCIDebugger.SendNumber(xIPM);
            mAHCIDebugger.SendNumber(xDET);

            if (xIPM != PORT_IPM_ACTIVE)
                return PortType.Nothing;
            if (xDET != PORT_DET_PRESENT)
                return PortType.Nothing;

            switch (Signature)
            {
                case SATA_SIG_ATAPI:
                    return PortType.SATAPI;
                case SATA_SIG_SEMB:
                    return PortType.SEMB;
                case SATA_SIG_PM:
                    return PortType.PM;
                default:
                    return PortType.SATA;
            }
        }

        public static void InitSATA()
        {
            GenericRegisters mGeneric = new GenericRegisters()
            {
                CAP            = mAHCIMemory[0x00],
                GHC            = mAHCIMemory[0x04],
                IS             = mAHCIMemory[0x08],
                PI             = mAHCIMemory[0x0C],
                VS             = mAHCIMemory[0x10],
                CCC_CTL        = mAHCIMemory[0x14],
                CCC_PORTS      = mAHCIMemory[0x18],
                EM_LOC         = mAHCIMemory[0x1C],
                EM_CTL         = mAHCIMemory[0x20],
                CAP2           = mAHCIMemory[0x24],
                BOHC           = mAHCIMemory[0x28],
                Reserved0      = PortHelper.GetByteValueArray(0x2C, 29),
                VendorSpecific = PortHelper.GetByteValueArray(0xA0, 20),
                Ports = new PortRegisters[32]
            };
            var xSelf = new AHCI();
            xSelf.SearchForDisks(mGeneric);
        }
    }
}
