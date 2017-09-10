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
        FIS_Type_RegisterH2D = 0x27,  // Register FIS: Host to Device
        FIS_Type_RegisterD2H = 0x34,  // Register FIS: Device to Host
        FIS_Type_DMA_Activate = 0x39, // DMA Activate
        FIS_Type_DMA_Setup = 0x41,    // DMA Setup: Device to Host
        FIS_Type_Data = 0x46,         // Data FIS: Bidirectional
        FIS_Type_BIST = 0x58,         // BIST
        FIS_Type_PIO_Setup = 0x5F,    // PIO Setup: Device to Host
        FIS_Type_DeviceBits = 0xA1    // Device bits
    }

    public struct HBACommandHeader
    {
        public byte CFL;          // 5 bits
        public byte ATAPI;        // 1 bit
        public byte Write;        // 1 = H2D | 0 = D2H // 1bit
        public byte Prefetchable; // 1 bit

        public byte Reset;        // 1 bit
        public byte BIST;         // 1 bit
        public byte ClearBusy;    // 1 bit
        public byte Reserved0;    // 1 bit
        public byte PMP;          // 4 bits

        public ushort PRDTL;      // Physical region descriptor table length in entries

        public uint PRDBC;        // Physical region descriptor byte count transferred

        public uint CTBA;         // Command table descriptor base address
        public uint CTBAU;        // Command table descriptor base address upper 32 bits (4 bytes)

        public uint[] Reserved1;  // [4]
    }

    public struct HBACommandTable
    {
        // 0x00
        public byte[] CFIS;     // [64] // = 64

        // 0x40
        public byte[] ACMD;     // [16] // = 16 + 64 = 80

        // 0x50
        public byte[] Reserved; // [48] // = 48 + 80 = 128

        public HBAPRDTEntry[] PRDTEntry; // [1] // = 24 + 128 = 152
    }
    public struct HBAPRDTEntry
    {                                      
        public uint DBA;                   // Data base address
        public uint DBAU;                  // Data base address upper 32 bits
        public uint Reserved0;

        public uint DBC;                   // Byte count, 4M max // 22 bits (2.75 Bytes)
        public uint Reserved1;             // Reserved // 9 bits
        public uint InterruptOnCompletion; // Interrupt on completion // 1 bit
    }

    public enum PortType
    {
        Nothing = 0x00,
        SATA = 0x01,
        SATAPI = 0x02,
        SEMB = 0x03,
        PM = 0x04
    }
    
    public struct FISRegisterH2D
    {
        public byte FISType;
    
        //public byte Options;
        // Reserved
        public byte IsCommand;
        public byte Command;
        public byte FeatureL;
    
        public byte LBA0;
        public byte LBA1;
        public byte LBA2;
        public byte Device;
    
        public byte LBA3;
        public byte LBA4;
        public byte LBA5;
        public byte FeatureH;
    
        public byte CountL;
        public byte CountH;
        public byte ICC;
        public byte Control;
    
        public byte Res2;
    }

    public struct FISRegisterD2H {
        public byte FISType;
    
        //public byte Options;
        // Reserved
        //public byte InterruptBit;
        // Rseerved
        public byte Status;
        public byte Error;
    
        public byte LBA0;
        public byte LBA1;
        public byte LBA2;
        public byte Device;
    
        public byte LBA3;
        public byte LBA4;
        public byte LBA5;
        // Reserved
    
        public byte CountL;
        public byte CountH;
        public byte ICC;
        public ushort Res2;
        // Reserved
        public uint Res3;
        // Reserved
    }

    public struct FISData {
        public byte FISType;
    
        //public byte Options;
        // Reserved
    
        // Reserved
        public byte Payload;
    }

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
        //___________________________________________________//
        // SATA Signatures Constants                         //
        public const uint SATA_SIG_ATA         = 0x00000101; // SATA drive
        public const uint SATA_SIG_ATAPI       = 0xEB140101; // SATAPI drive
        public const uint SATA_SIG_SEMB        = 0xC33C0101; // Enclosure management bridge
        public const uint SATA_SIG_PM          = 0x96690101; // Port multiplier
                                                             //
        // SATA Status Bits Constants                        //
        public const uint PORT_DET_PRESENT     = 0x00000003; // DET Present Value
        public const uint PORT_IPM_ACTIVE      = 0x00000001; // IPM Active Value
                                                             //
        public const uint AHCI_BASE            = 0x00400000; // AHCI Base
                                                             //
        // ATA Device Status                                 //
        public const uint ATA_DEV_BUSY         = 0x00000080; // ATA Device Busy
        public const uint ATA_DEV_DRQ          = 0x00000008; // ATA Device DRQ?
                                                             //
        // HBA PortX Command Constants                       //
        public const uint HBA_PxCMD_CR         = (01 << 15); // 
        public const uint HBA_PxCMD_FR         = (01 << 14); //
        public const uint HBA_PxCMD_FRE        = (01 << 04); //
        public const uint HBA_PxCMD_SUD        = (01 << 01); // 
        public const uint HBA_PxCMD_ST         = (01 << 00); //
        public const uint HBA_PxIS_TFES        = (01 << 30); //
                                                             //
        // ATA Command Constants                             //
        public const uint ATA_CMD_READ_DMA_EX  = 0x00000025; // 
        public const uint ATA_CMD_WRITE_DMA_EX = 0x00000025; //
                                                             //
        //___________________________________________________//
        private static uint mPortLocation;
        private static uint[] mPorts = new uint[32];

        public static PCIDevice mAHCIDevice = HAL.PCI.GetDeviceClass(0x01, 0x06);
        public static uint BAR5 = mAHCIDevice.BaseAddressBar[5].BaseAddress;
        public static MemoryBlock mAHCIMemory = new MemoryBlock(BAR5, 0x10FF);
        public static MemoryBlock mAHCIPortMemory = new MemoryBlock(BAR5 + mPortLocation, 0x10FF);
        internal static Debugger mAHCIDebugger = new Debugger("HAL", "AHCI");

        internal class PortHelper
        {
            public static PortRegisters GetPort(int aPortNumber)
            {
                mPortLocation = (uint)(aPortNumber);
                if (aPortNumber == 00) mPortLocation = 0x0100;
                else if (aPortNumber == 01) mPortLocation = 0x0180;
                else if (aPortNumber == 02) mPortLocation = 0x0200;
                else if (aPortNumber == 03) mPortLocation = 0x0280;
                else if (aPortNumber == 04) mPortLocation = 0x0300;
                else if (aPortNumber == 05) mPortLocation = 0x0380;
                else if (aPortNumber == 06) mPortLocation = 0x0400;
                else if (aPortNumber == 07) mPortLocation = 0x0480;
                else if (aPortNumber == 08) mPortLocation = 0x0500;
                else if (aPortNumber == 09) mPortLocation = 0x0580;
                else if (aPortNumber == 10) mPortLocation = 0x0600;
                else if (aPortNumber == 11) mPortLocation = 0x0680;
                else if (aPortNumber == 12) mPortLocation = 0x0700;
                else if (aPortNumber == 13) mPortLocation = 0x0780;
                else if (aPortNumber == 14) mPortLocation = 0x0800;
                else if (aPortNumber == 15) mPortLocation = 0x0880;
                else if (aPortNumber == 16) mPortLocation = 0x0900;
                else if (aPortNumber == 17) mPortLocation = 0x0980;
                else if (aPortNumber == 18) mPortLocation = 0x0A00;
                else if (aPortNumber == 19) mPortLocation = 0x0A80;
                else if (aPortNumber == 20) mPortLocation = 0x0B00;
                else if (aPortNumber == 21) mPortLocation = 0x0B80;
                else if (aPortNumber == 22) mPortLocation = 0x0C00;
                else if (aPortNumber == 23) mPortLocation = 0x0C80;
                else if (aPortNumber == 24) mPortLocation = 0x0D00;
                else if (aPortNumber == 25) mPortLocation = 0x0D80;
                else if (aPortNumber == 26) mPortLocation = 0x0E00;
                else if (aPortNumber == 27) mPortLocation = 0x0E80;
                else if (aPortNumber == 28) mPortLocation = 0x0F00;
                else if (aPortNumber == 29) mPortLocation = 0x0F80;
                else if (aPortNumber == 30) mPortLocation = 0x1000;
                else if (aPortNumber == 31) mPortLocation = 0x1080;
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
            var xPortType = 0U;
            var xSupportedPorts = 0x1F;
            while (xPort <= xSupportedPorts)
            {
                if (xImplementedPort != 0)
                {
                    PortType PortType = CheckPortType(PortHelper.GetPort(xPort));
                    var xPortString = "0:" + xPort;
                    if (PortType == PortType.SATA) // If Port Type was SATA.
                    {
                        mAHCIDebugger.Send("SATA drive found at port " + xPortString);
                        Console.WriteLine("SATA Drive found at port " + xPortString);
                        xPortType = 0x01;
                        PortRebase(PortHelper.GetPort(xPort), xPort);
                        Read(PortHelper.GetPort(xPort), 0, 0, 2, (ushort)BAR5);
                    }
                    else if (PortType == PortType.SATAPI) // If Port Type was SATAPI.
                    {
                        mAHCIDebugger.Send("SATAPI drive found at port " + xPortString);
                        Console.WriteLine("CD/DVD Drive found at port " + xPortString);
                        xPortType = 0x02;
                    }
                    else if (PortType == PortType.SEMB) // If Port Type was SEMB.
                    {
                        mAHCIDebugger.Send("SEMB drive found at port " + xPortString);
                        Console.WriteLine("SEMB Drive found at port " + xPortString);
                        xPortType = 0x03;
                    }
                    else if (PortType == PortType.PM) // If Port Type was Port Mulitplier.
                    {
                        mAHCIDebugger.Send("Port Multiplier drive found at port " + xPortString);
                        Console.WriteLine("Port Multiplier Drive found at port " + xPortString);
                        xPortType = 0x04;
                    }
                    else if (PortType == PortType.Nothing) // If Nothing in this Port.
                        mAHCIDebugger.Send("No drive found at port " + xPortString);
                    else // If Implemented Port value was not zero and not one of the above.
                        mAHCIDebugger.Send("Unknown drive found at port " + xPortString);
                }
                mPorts[xPort] = xPortType;
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

        public void PortRebase(PortRegisters aPort, int aPortNumber)
        {
            StopCMD(aPort);

            aPort.CLB = (uint)(AHCI_BASE + (aPortNumber << 10));
            aPort.CLBU = 0;
            mAHCIPortMemory.Fill(aPort.CLB, 0, 1024);

            // FIS offset: 32K+256*portno
            // FIS entry size = 256 bytes per port
            aPort.FB = (uint)(AHCI_BASE + (32 << 10) + (aPortNumber << 8));
            aPort.FBU = 0;
            mAHCIPortMemory.Fill(aPort.FB, 0, 256);

            HBACommandHeader[] xCMDHeader = new HBACommandHeader[32];
            for (int i = 0; i < 32; i++)
            {
                xCMDHeader[i].PRDTL = 8; // 8 prdt entries per command table
                                         // 256 bytes per command table, 64+16+48+16*8
                                         // Command table offset: 40K + 8K*portno + cmdheader_index*256
                xCMDHeader[i].CTBA = (uint)(AHCI_BASE + (40 << 10) + (aPortNumber << 13) + (i << 8));
                xCMDHeader[i].CTBAU = 0;
                mAHCIPortMemory.Fill(xCMDHeader[i].CTBA, 0, 256);
            }

            StartCMD(aPort);
        }
        public void StartCMD(PortRegisters aPort)
        {
            while ((aPort.CMD & HBA_PxCMD_CR) != 0) ;

            aPort.CMD |= HBA_PxCMD_FRE;
            aPort.CMD |= HBA_PxCMD_ST;
        }

        public void StopCMD(PortRegisters aPort)
        {
            aPort.CMD &= ~HBA_PxCMD_ST;

            while(true)
            {
                if ((aPort.CMD & HBA_PxCMD_FR) != 0)
                    continue;
                if ((aPort.CMD & HBA_PxCMD_CR) != 0)
                    continue;
                break;
            }

            aPort.CMD &= ~HBA_PxCMD_FRE;
        }

        public bool Read(PortRegisters aPort, uint aStartLow, uint aStartHigh, uint aCount, ushort aBuffer)
        {
            aPort.IS -= 1;
            int xSpin = 0; // Spin lock Timeout Counter
            int xSlot = FindCMDSlot(aPort);
            if (xSlot == -1)
                return false;

            HBACommandHeader xCMDHeader = new HBACommandHeader();
            aPort.CLB += (uint)xSlot;
            xCMDHeader.CFL = (byte)(17 / sizeof(uint));
            xCMDHeader.Write = 0;
            xCMDHeader.PRDTL = (ushort)(((aCount - 1) >> 4) + 1);

            HBACommandTable xCMDTable = new HBACommandTable();  // xCMDHeader.CTBA
            mAHCIPortMemory.Fill(xCMDHeader.CTBA, 0, (158 + (xCMDHeader.PRDTL - 1U)) * 24);
            xCMDTable.PRDTEntry = new HBAPRDTEntry[1];

            int i = 0; // i for PRDTEntry outside the loop?

            for (i = 0; i < xCMDHeader.PRDTL - 1; i++)
            {

                xCMDTable.PRDTEntry[i].DBA = (uint)(aBuffer);
                xCMDTable.PRDTEntry[i].DBC = 8 * 1024;
                xCMDTable.PRDTEntry[i].InterruptOnCompletion = 1;
                aBuffer += 4 * 1024;
                aCount -= 16;
            }
            // Last entry

            xCMDTable.PRDTEntry[i].DBA = (uint)aBuffer;
            xCMDTable.PRDTEntry[i].DBC = aCount << 9;
            xCMDTable.PRDTEntry[i].InterruptOnCompletion = 1;

            // Setup the command
            FISRegisterH2D CommandFIS = new FISRegisterH2D() // Address CMDTBL.CFIS;
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)ATA_CMD_READ_DMA_EX,

                LBA0 = (byte)aStartLow,
                LBA1 = (byte)(aStartLow >> 8),
                LBA2 = (byte)(aStartLow >> 16),
                Device = 1 << 6, // LBA Mode

                LBA3 = (byte)(aStartLow >> 24),
                LBA4 = (byte)aStartHigh,
                LBA5 = (byte)(aStartHigh >> 8),

                CountL = (byte)(aCount & 0xFF),
                CountH = (byte)(aCount >> 8),
            };
            while ((aPort.TFD & (ATA_DEV_BUSY | ATA_DEV_DRQ)) != 0 && xSpin < 1000000)
            {
                xSpin++;
            }
            if (xSpin == 1000000)
            {
                mAHCIDebugger.Send("Port timed out");
                return false;
            }

            aPort.CI = 1U << xSlot; // Issue with Command

            // Wait for Completion
            while (true)
            {
                // In some longer duration reads, It may be Helpful to Spin on the DPS bit
                // in the aPort.IS port field as well (1 << 5)
                if ((aPort.CI & (1 << xSlot)) == 0)
                    break;
                if ((aPort.IS == HBA_PxIS_TFES))
                {
                    Console.WriteLine("Error occured while Reading the Disk");
                    mAHCIDebugger.Send("Read disk error!");
                    return false;
                }
            }

            // Check Again
            if (aPort.IS == HBA_PxIS_TFES)
            {
                Console.WriteLine("Error occured while Reading the Disk");
                mAHCIDebugger.Send("Read disk error!");
                return false;
            }

            return true;
        }

        // Inverting Some codes (Read -> Write)
        public bool Write(PortRegisters aPort, uint aStartLow, uint aStartHigh, uint aCount, ushort aBuffer)
        {
            aPort.IS -= 1;
            int xSpin = 0; // Spin lock Timeout Counter
            int xSlot = FindCMDSlot(aPort);
            if (xSlot == -1)
                return false;

            HBACommandHeader xCMDHeader = new HBACommandHeader();
            aPort.CLB += (uint)xSlot;
            xCMDHeader.CFL = 15 / sizeof(uint);
            xCMDHeader.Write = 1;
            xCMDHeader.PRDTL = (ushort)(((aCount - 1) >> 4) + 1);

            HBACommandTable xCMDTable = new HBACommandTable();  // xCMDHeader.CTBA
            mAHCIPortMemory.Fill(xCMDHeader.CTBA, 0, (158 + (xCMDHeader.PRDTL - 1U)) * 24);
            xCMDTable.PRDTEntry = new HBAPRDTEntry[1];

            int i = 0; // i for PRDTEntry outside the loop?

            for (i = 0; i < xCMDHeader.PRDTL - 1; i++)
            {

                xCMDTable.PRDTEntry[i].DBA = (uint)(aBuffer);
                xCMDTable.PRDTEntry[i].DBC = 8 * 1024;
                xCMDTable.PRDTEntry[i].InterruptOnCompletion = 1;
                aBuffer += 4 * 1024;
                aCount -= 16;
            }
            // Last entry

            xCMDTable.PRDTEntry[i].DBA = (uint)aBuffer;
            xCMDTable.PRDTEntry[i].DBC = aCount << 9;
            xCMDTable.PRDTEntry[i].InterruptOnCompletion = 1;

            // Setup the command
            FISRegisterH2D CommandFIS = new FISRegisterH2D // Address CMDTBL.CFIS;
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)ATA_CMD_WRITE_DMA_EX,

                LBA0 = (byte)aStartLow,
                LBA1 = (byte)(aStartLow >> 8),
                LBA2 = (byte)(aStartLow >> 16),
                Device = 1 << 6, // LBA Mode

                LBA3 = (byte)(aStartLow >> 24),
                LBA4 = (byte)aStartHigh,
                LBA5 = (byte)(aStartHigh >> 8),

                CountL = (byte)(aCount & 0xFF),
                CountH = (byte)(aCount >> 8)
            };

            while ((aPort.TFD & (ATA_DEV_BUSY | ATA_DEV_DRQ)) != 0 && xSpin < 1000000)
            {
                xSpin++;
            }

            if (xSpin == 1000000)
            {
                mAHCIDebugger.Send("Port timed out");
                return false;
            }

            aPort.CI = 1U << xSlot; // Issue with Command

            // Wait for Completion
            while (true)
            {
                // In some longer duration writes, It may be Helpful to Spin on the DPS bit
                // in the aPort.IS port field as well (1 << 5)
                if ((aPort.CI & (1 << xSlot)) == 0)
                    break;
                if ((aPort.IS == HBA_PxIS_TFES))
                {
                    Console.WriteLine("Error occured while Writing to the Disk");
                    mAHCIDebugger.Send("Write disk error!");
                    return false;
                }
            }

            // Check Again
            if (aPort.IS == HBA_PxIS_TFES)
            {
                Console.WriteLine("Error occured while Writing to the Disk");
                mAHCIDebugger.Send("Write disk error!");
                return false;
            }

            return true;
        }

        public int FindCMDSlot(PortRegisters aPort)
        {
            // If not set in SACT and CI, the slot is free
            var xSlots = (aPort.SACT | aPort.CI);
            for (int i = 0; i < xSlots; i++)
            {
                if ((xSlots & 1) == 0)
                    mAHCIDebugger.Send("Found a command slot: ");
                    mAHCIDebugger.SendNumber(i);
                    return i;
                xSlots >>= 1;
            }
            mAHCIDebugger.Send("Cannot find free command list entry");
            return -1;
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
            //xSelf.GetPartitions(xPorts, );
        }
    }
}
