using System;
using System.Collections.Generic;
using Cosmos.Core;
using Cosmos.Core.Memory.Old;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers.PCI.Controllers;

namespace Cosmos.HAL.BlockDevice
{
    #region Registers

    //TODO: Must be struct
    public class GenericRegisters
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

    public class PortRegisters
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

    public class HBACommandHeader
    {
        // DWord 0
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

        // DWord 1
        public uint PRDBC;        // Physical region descriptor byte count transferred

        public uint CTBA;         // Command table descriptor base address
        public uint CTBAU;        // Command table descriptor base address upper 32 bits (4 bytes)

        // Reserved1;  // [4]
    }

    public class HBACommandTable
    {
        // 0x00
        public byte CFIS;     // [64] // = 64

        // 0x40
        public byte ACMD;     // [16] // = 16 + 64 = 80

        // 0x50
        public byte Reserved; // [48] // = 48 + 80 = 128

        public HBAPRDTEntry[] PRDTEntry; // [1] // = 44 + 128 = 172
    }
    public class HBAPRDTEntry
    {
        public uint DBA;                   // Data base address
        public uint DBAU;                  // Data base address upper 32 bits
        public uint Reserved0;

        public uint DBC;                   // Byte count, 4M max // 22 bits (2.75 Bytes)
        public uint Reserved1;             // Reserved // 9 bits
        public int InterruptOnCompletion;  // Interrupt on completion // 1 bit
    }

    public enum PortType
    {
        Nothing = 0x00,
        SATA = 0x01,
        SATAPI = 0x02,
        SEMB = 0x03,
        PM = 0x04
    }

    public class FISRegisterH2D
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

        public static explicit operator byte(FISRegisterH2D v)
        {
            byte xValue = sizeof(byte) * 16;
            return xValue;
        }
    }

    public class FISRegisterD2H
    {
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

        public static explicit operator byte(FISRegisterD2H v)
        {
            byte xValue = sizeof(byte) * 15;
            return xValue;
        }
    }

    public class FISData
    {
        public byte FISType;

        public byte Payload;
    }
    #endregion

    #region Enums
    public enum DriveSignature : uint // Drive Signature to identify what drive is plugged to Port X:X
    {
        SATADrive = 0x00000101,
        PMDrive = 0x96690101,
        SATAPIDrive = 0xEB140101,
        SEMBDrive = 0xC33C0101,
        NullDrive = 0xFFFFFFFF
    }

    public enum InterfacePowerManagementStatus : uint // SATA Status: Interface Power Management Status 
    {
        NotPresent = 0x00,
        Active = 0x01,
        Partial = 0x02,
        Slumber = 0x06,
        DeviceSleep = 0x08
    }

    public enum CurrentInterfaceSpeedStatus : uint // SATA Status: Current Interface Speed
    {
        NotPresent = 0x00,
        Gen1Rate = 0x01,
        Gen2Rate = 0x02,
        Gen3Rate = 0x03
    }

    public enum DeviceDetectionStatus : uint // SATA Status: Device Detection Status
    {
        NotDetected = 0x00,
        DeviceDetectedNoPhy = 0x01,
        DeviceDetectedWithPhy = 0x03,
        PhyOffline = 0x04
    }

    public enum ATADeviceStatus : uint
    {
        Busy = 0x80,
        DRQ = 0x08
    }

    public enum CommandAndStatus : uint
    {
        ICC_Reserved0 = 0x0000000F,
        ICC_DevSleep = 0x00000008,
        ICC_Slumber = 0x00000006,
        ICC_Partial = 0x00000002,
        ICC_Active = 0x00000001,
        ICC_Idle = 0x00000000,
        ASP = (01 << 27),
        ALPE = (01 << 26),
        EnableATAPILED = (01 << 25),
        ATAPIDevice = (01 << 24),
        APSTE = (01 << 23),
        FISSwitchPort = (01 << 22),
        ExternalSATAPort = (01 << 21),
        ColdPresenceDetect = (01 << 20),
        MPSP = (01 << 19),
        HotPlugCapPort = (01 << 18),
        PortMultAttach = (01 << 17),
        ColdPresenceState = (01 << 16),
        CMDListRunning = (01 << 15),
        FISRecieveRunning = (01 << 14),
        MPSS = (01 << 13),
        CurrentCMDSlot = (01 << 12),
        Reserved0 = (01 << 07),
        FISRecieveEnable = (01 << 04),
        CMDListOverride = (01 << 03),
        PowerOnDevice = (01 << 02),
        SpinUpDevice = (01 << 01),
        StartProccess = (01 << 00),
        Null = 0xFFFF
    }

    public enum InterruptStatus : int
    {
        ColdPortDetectStatus = (01 << 31),
        TaskFileErrorStatus = (01 << 30),
        HostBusFatalErrorStatus = (01 << 29),
        HostBusDataErrorStatus = (01 << 28),
        InterfaceFatalErrorStatus = (01 << 27),
        InterfaceNFatalErrorStatus = (01 << 26),
        OverflowStatus = (01 << 24),
        IncorrectPMStatus = (01 << 23),
        PhyRdyChangeStatus = (01 << 22),
        DevMechanicalPresenceStatus = (01 << 07),
        PortConnectChangeStatus = (01 << 06),
        DescriptorProcessed = (01 << 05),
        UnknownFISInterrupt = (01 << 04),
        SetDeviceBitsInterrupt = (01 << 03),
        DMASetupFISInterrupt = (01 << 02),
        PIOSetupFISInterrupt = (01 << 01),
        D2HRegFISInterrupt = (01 << 00),
        Null = 0xFFFF
    }

    public enum InterruptEnable : uint
    {
        OverflowEnable = (01 << 24),
        IncorrectPMEnable = (01 << 23),
        PhyRdyChangeInterruptEnable = (01 << 22),
        DevMechanicalPresenceEnable = (01 << 07),
        PortChangeInterruptEnable = (01 << 06),
        DescProcessedInterruptEnable = (01 << 05),
        UnknownFISInterruptEnable = (01 << 04),
        SetDeviceBitsInterruptEnable = (01 << 03),
        DMASetupFISInterruptEnable = (01 << 02),
        PIOSetupFISInterruptEnable = (01 << 01),
        D2HRegFISInterruptEnable = (01 << 00),
        Null = 0xFFFF
    }

    public enum SATACommands : uint
    {
        ReadPio = AtaPio.Cmd.ReadPio,
        ReadPioExt = AtaPio.Cmd.ReadPioExt,
        ReadDma = AtaPio.Cmd.ReadDma,
        ReadDmaExt = AtaPio.Cmd.ReadDmaExt,
        WritePio = AtaPio.Cmd.WritePio,
        WritePioExt = AtaPio.Cmd.WritePioExt,
        WriteDma = AtaPio.Cmd.WriteDma,
        WriteDmaExt = AtaPio.Cmd.WriteDmaExt,
        CacheFlush = AtaPio.Cmd.CacheFlush,
        CacheFlushExt = AtaPio.Cmd.CacheFlushExt,
        Packet = AtaPio.Cmd.Packet,
        IdentifyPacket = AtaPio.Cmd.IdentifyPacket,
        Identify = AtaPio.Cmd.Identify,
        Read = AtaPio.Cmd.Read,
        Eject = AtaPio.Cmd.Eject
    }

    public enum Bases : uint { SATA = 0x00400000 }
    #endregion

    public class SATA : BlockDevice
    {
        private static uint mPortLocation;
        private static uint[] mPorts = new uint[32];

        private static uint mABAR;
        private static MemoryBlock mSATAMemory;
        internal static Debugger mSATADebugger = new Debugger("HAL", "SATA");

        public static GenericRegisters mGeneric;

        public List<int> SATAPorts = new List<int>(32);

        public static InterfacePowerManagementStatus mIPM;
        public static DeviceDetectionStatus mDET;
        private int xPort;
        private bool is64DMA;

        #region Registers
                internal class Registers
                {
                    public static PortRegisters PortRegisters(int aPortNumber)
                    {
                        mPortLocation = 0x100 + (0x80 * (uint)aPortNumber);
                        PortRegisters xPortReg = new PortRegisters();
                        xPortReg.CLB = mSATAMemory[mPortLocation + 0x00];
                        xPortReg.CLBU = mSATAMemory[mPortLocation + 0x04];
                        xPortReg.FB = mSATAMemory[mPortLocation + 0x08];
                        xPortReg.FBU = mSATAMemory[mPortLocation + 0x0C];
                        xPortReg.IS = mSATAMemory[mPortLocation + 0x10];
                        xPortReg.IE = mSATAMemory[mPortLocation + 0x14];
                        xPortReg.CMD = mSATAMemory[mPortLocation + 0x18];
                        xPortReg.Reserved0 = mSATAMemory[mPortLocation + 0x1C];
                        xPortReg.TFD = mSATAMemory[mPortLocation + 0x20];
                        xPortReg.SIG = mSATAMemory[mPortLocation + 0x24];
                        xPortReg.SSTS = mSATAMemory[mPortLocation + 0x28];
                        xPortReg.SCTL = mSATAMemory[mPortLocation + 0x2C];
                        xPortReg.SERR = mSATAMemory[mPortLocation + 0x30];
                        xPortReg.SACT = mSATAMemory[mPortLocation + 0x34];
                        xPortReg.CI = mSATAMemory[mPortLocation + 0x38];
                        xPortReg.SNTF = mSATAMemory[mPortLocation + 0x3C];
                        xPortReg.FBS = mSATAMemory[mPortLocation + 0x40];
                        xPortReg.Reserved1 = GetValueArray(0x44, 11);
                        xPortReg.VendorSpecific = GetValueArray(0x70, 4);
                        mIPM = (InterfacePowerManagementStatus)((byte)((xPortReg.SSTS >> 8) & 0x0F));
                        mDET = (DeviceDetectionStatus)((byte)(xPortReg.SSTS & 0x0F));
                        return xPortReg;
                    }
                    public static PortRegisters PortRegisters(int[] aPortsArray)
                    {
                        foreach (int xPortNumber in aPortsArray)
                        {
                            mPortLocation = 0x100 + (0x80 * (uint)xPortNumber);
                            PortRegisters xPortReg = new PortRegisters();
                            xPortReg.CLB = mSATAMemory[mPortLocation + 0x00];
                            xPortReg.CLBU = mSATAMemory[mPortLocation + 0x04];
                            xPortReg.FB = mSATAMemory[mPortLocation + 0x08];
                            xPortReg.FBU = mSATAMemory[mPortLocation + 0x0C];
                            xPortReg.IS = mSATAMemory[mPortLocation + 0x10];
                            xPortReg.IE = mSATAMemory[mPortLocation + 0x14];
                            xPortReg.CMD = mSATAMemory[mPortLocation + 0x18];
                            xPortReg.Reserved0 = mSATAMemory[mPortLocation + 0x1C];
                            xPortReg.TFD = mSATAMemory[mPortLocation + 0x20];
                            xPortReg.SIG = mSATAMemory[mPortLocation + 0x24];
                            xPortReg.SSTS = mSATAMemory[mPortLocation + 0x28];
                            xPortReg.SCTL = mSATAMemory[mPortLocation + 0x2C];
                            xPortReg.SERR = mSATAMemory[mPortLocation + 0x30];
                            xPortReg.SACT = mSATAMemory[mPortLocation + 0x34];
                            xPortReg.CI = mSATAMemory[mPortLocation + 0x38];
                            xPortReg.SNTF = mSATAMemory[mPortLocation + 0x3C];
                            xPortReg.FBS = mSATAMemory[mPortLocation + 0x40];
                            xPortReg.Reserved1 = GetValueArray(0x44, 11);
                            xPortReg.VendorSpecific = GetValueArray(0x70, 4);
                            return xPortReg;
                        }
                        PortRegisters mNullPort = new PortRegisters();
                        return mNullPort;
                    }

                    /// <summary>
                    /// Note: This method only work when SATA is initialized!
                    /// </summary>
                    public static int GetPortNumber()
                    {
                        foreach (int mPort in mPorts)
                        {
                            if (mPort == 1)
                            {
                                return mPort; // Return mPort SATA Type
                            }
                            else
                            {
                                // If mPort's value is not 1 then it is a CD/DVD Drive Port, Port Multiplier, SEMB Port or Nothing in the Port
                            }
                        }
                        return -1;
                    }
                    public static uint[] GetValueArray(uint aStartAddress, int aAmount)
                    {
                        uint[] FinishedArray = new uint[aAmount];

                        int i = 0;
                        for (uint ui = aStartAddress; ui < aAmount; ui += 0x04)
                        {
                            while (i < aAmount)
                            {
                                FinishedArray[i] = mSATAMemory[ui];
                                i++;
                                break;
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
                                FinishedArray[i] = mSATAMemory.Bytes[ui];
                            }
                        }
                        return FinishedArray;
                    }
                }
                #endregion

        public SATA(uint ABAROffset)
        {
            // Setting Offset arg to Global offset
            mABAR = ABAROffset;
            mSATAMemory = new MemoryBlock(mABAR, 0x10FF);

            mGeneric = new GenericRegisters
            {
                CAP = mSATAMemory[0x00],
                GHC = mSATAMemory[0x04],
                IS = mSATAMemory[0x08],
                PI = mSATAMemory[0x0C],
                VS = mSATAMemory[0x10],
                CCC_CTL = mSATAMemory[0x14],
                CCC_PORTS = mSATAMemory[0x18],
                EM_LOC = mSATAMemory[0x1C],
                EM_CTL = mSATAMemory[0x20],
                CAP2 = mSATAMemory[0x24],
                BOHC = mSATAMemory[0x28],
                //Reserved0 = Registers.GetByteValueArray(0x2C, 29),
                //VendorSpecific = Registers.GetByteValueArray(0xA0, 20),
                Ports = new PortRegisters[32]
            };
            while (((mSATAMemory[0x04] >> 31) & 1) == 0) ;
            mBlockSize = 512;
            mBlockCount = 256;
            Console.WriteLine("-- 00 --");
            SearchForDisks(mGeneric);
        }

        

        private void SearchForDisks(GenericRegisters aGeneric)
        {
            // Search for disks
            var xImplementedPort = aGeneric.PI;
            var xPort = 0;
            var xPortType = 0U;
            while(xPort < 32)
            {
                if ((xImplementedPort & 1) != 0)
                {
                    PortType PortType = CheckPortType(Registers.PortRegisters(xPort));
                    var xPortString = "0:" + xPort;
                    if (PortType == PortType.SATA) // If Port Type was SATA.
                    {
                        Console.WriteLine("SATA drive at port " + xPortString + " found");
                        Console.WriteLine("SATA Drive at port " + xPortString);
                        PortRebase(Registers.PortRegisters(xPort), xPort);
                        ushort ReadedBits = new ushort();
                        SATAPorts.Add(xPort);
                        mSATADebugger.Send("Readen bits: " + ReadedBits);
                        xPortType = 0x01;
                    }
                    else if (PortType == PortType.SATAPI) // If Port Type was SATAPI.
                    {
                        Console.WriteLine("SATAPI drive at port " + xPortString + " found, which is not supported yet!");
                        Console.WriteLine("CD/DVD Drive at port " + xPortString + " found, which is not supported yet!");
                        xPortType = 0x02;
                    }
                    else if (PortType == PortType.SEMB) // If Port Type was SEMB.
                    {
                        //Console.WriteLine("SEMB drive at port " + xPortString + " found, which is not supported yet!");
                        Console.WriteLine("SEMB Drive at port " + xPortString + " found, which is not supported yet!");
                        xPortType = 0x03;
                    }
                    else if (PortType == PortType.PM) // If Port Type was Port Mulitplier.
                    {
                        //Console.WriteLine("Port Multiplier drive at port " + xPortString + " found, which is not supported yet!");
                        Console.WriteLine("Port Multiplier Drive at port " + xPortString + " found, which is not supported yet!");
                        xPortType = 0x04;
                    }
                    //else if (PortType == PortType.Nothing) // If Nothing in this Port.
                    //else // If Implemented Port value was not zero and non of the above.
                }
                mPorts[xPort] = xPortType;
                xImplementedPort >>= 1;
                xPort++;
            }
        }

        private PortType CheckPortType(PortRegisters Port)
        {
            DriveSignature xSignature = (DriveSignature)Port.SIG;
            uint xSATAStatus = Port.SSTS;

            if (mIPM != InterfacePowerManagementStatus.Active)
                return PortType.Nothing;
            if (mDET != DeviceDetectionStatus.DeviceDetectedWithPhy)
                return PortType.Nothing;

            switch (xSignature)
            {
                case DriveSignature.SATAPIDrive:
                    return PortType.SATAPI;
                case DriveSignature.SEMBDrive:
                    return PortType.SEMB;
                case DriveSignature.PMDrive:
                    return PortType.PM;
                case DriveSignature.NullDrive:
                    return PortType.Nothing;
                default:
                    return PortType.SATA;
            }
        }

        public void PortRebase(PortRegisters aPort, int aPortNumber)
        {
            StopCMD(aPort);

            var CLBAddress = Heap.MemAlloc(1024);
            var FBAddress = Heap.MemAlloc(256);
            Console.WriteLine(CLBAddress.ToString());
            mSATAMemory[mPortLocation] = CLBAddress;
            new MemoryBlock(mSATAMemory[mPortLocation], 1024).Fill(0);
            //if(is64DMA)
            //{
            //    mSATAMemory[mPortLocation + 0x04] |= CLBUAddress;
            //    new MemoryBlock(mSATAMemory[mPortLocation + 0x04], 1024).Fill(0);
            //}
            
            mSATAMemory[mPortLocation + 0x08] = FBAddress;
            new MemoryBlock(mSATAMemory[mPortLocation + 0x08], 256).Fill(0);
            //if (is64DMA)
            //{
            //    mSATAMemory[mPortLocation + 0x0C] |= FBUAddress;
            //    new MemoryBlock(mSATAMemory[mPortLocation + 0x0C], 256).Fill(0);
            //}

            GetCommandHeader(aPort, aPortNumber); // Rebasing Command Header

            StartCMD(aPort);
        }

        public static HBACommandHeader[] GetCommandHeader(PortRegisters aPort, int aPortNumber)
        {
            HBACommandHeader[] xCMDHeader = new HBACommandHeader[32];
            for (int i = 0; i < xCMDHeader.Length; i++)
            {
                xCMDHeader[i] = new HBACommandHeader();
                xCMDHeader[i].PRDTL = 8; // 8 prdt entries per command table
                                         // 256 bytes per command table, 64+16+48+16*8
                xCMDHeader[i].CTBA = Heap.MemAlloc(256);
                xCMDHeader[i].CTBAU = 0;
                new MemoryBlock(xCMDHeader[i].CTBA, 256).Fill(0);
                var xCMDHeaderMem = new MemoryBlock(mSATAMemory[mPortLocation + 0x00], 1024);
                xCMDHeaderMem[0x02] = xCMDHeader[i].PRDTL;
                xCMDHeaderMem[0x08] = xCMDHeader[i].CTBA << 7;
                xCMDHeaderMem[0x0C] = xCMDHeader[i].CTBAU;
            }
            return xCMDHeader;
        }

        public void StartCMD(PortRegisters aPort)
        {
            while ((mSATAMemory[mPortLocation + 0x18] & (uint)CommandAndStatus.CMDListRunning) != 0)
            {
                //Console.WriteLine("Command list is already running");
                continue;
            }
            Console.WriteLine("Starting Process");
            mSATAMemory[mPortLocation + 0x18] |= (uint)CommandAndStatus.FISRecieveEnable;
            mSATAMemory[mPortLocation + 0x18] |= (uint)CommandAndStatus.StartProccess;
        }

        public void StopCMD(PortRegisters aPort)
        {
            Console.WriteLine("Stopping Process");
            mSATAMemory[mPortLocation + 0x18] &= ~(uint)CommandAndStatus.FISRecieveEnable;
            mSATAMemory[mPortLocation + 0x18] &= ~(uint)CommandAndStatus.StartProccess;

            while ((mSATAMemory[mPortLocation + 0x18] & (uint)(CommandAndStatus.FISRecieveRunning | CommandAndStatus.CMDListRunning)) != 0) ;
        }

        private bool InternalTransfer(PortRegisters aPort, uint aStartLow, uint aStartHigh, uint aCount, ushort aBuffer, bool aWrite = false)
        {
            //mSATAMemory[mPortLocation + 0x10] = unchecked((uint)-1);
            new MemoryBlock(mSATAMemory.Base + mPortLocation + 0x10, sizeof(uint)).Fill(0);
            int xSpin = 0; // Spin lock Timeout Counter
            int xSlot = FindCMDSlot(aPort);
            Console.WriteLine("My Slot: " + xSlot);
            if (xSlot == -1)
                return false;

            HBACommandHeader xCMDHeader = GetCommandHeader(aPort, Registers.GetPortNumber())[xSlot];
            //mSATAMemory[mPortLocation + 0x00] += (uint)xSlot;
            xCMDHeader.CFL = (byte)(17 / sizeof(uint));
            xCMDHeader.Write = (aWrite ? (byte)1 : (byte)0);
            xCMDHeader.PRDTL = (ushort)(((aCount - 1) >> 4) + 1);
            var xCMDHeaderMem = new MemoryBlock(mSATAMemory[mPortLocation + 0x00], 1024);
            xCMDHeaderMem[0x00] = (uint)xCMDHeader.PRDTL << 16;
            xCMDHeaderMem[0x00] = (uint)xCMDHeader.Write << 6;
            xCMDHeaderMem[0x00] = (uint)xCMDHeader.CFL << 0;

            Console.WriteLine(xCMDHeaderMem[0x00].ToString());
            Console.WriteLine(xCMDHeaderMem.Base.ToString());
            Console.WriteLine(xCMDHeader.CTBA.ToString());

            Console.WriteLine("Before Command Table Initialization");
            HBACommandTable xCMDTable = new HBACommandTable();  // xCMDHeader.CTBA
            var xCMDTableMem = new MemoryBlock(xCMDHeader.CTBA, 256);
            xCMDTableMem.Fill(0, (((158 / 8) + (xCMDHeader.PRDTL - 1U)) * 24), 0);
            xCMDTable.PRDTEntry = new HBAPRDTEntry[1];
            xCMDTable.PRDTEntry[0] = new HBAPRDTEntry();
            Console.WriteLine("After Command Table Initialization");

            Console.WriteLine("Before MemAlloc");
            var xDataBA = Heap.MemAlloc(sizeof(ushort));
            Console.WriteLine(xDataBA);
            var xDataMem = new MemoryBlock(xDataBA, sizeof(ushort));
            xDataMem[0x00] = aBuffer;
            Console.WriteLine(xDataMem[0x00]);
            Console.WriteLine("After MemAlloc");
            uint i = 1;
            while (i - 1 < xCMDHeader.PRDTL - 1)
            {

                Console.WriteLine("On Loop Command Table Initialization");
                xCMDTable.PRDTEntry[i - 1].DBA = (uint)(xDataBA);
                xCMDTable.PRDTEntry[i - 1].DBC = 8 * 1024 - 1;
                xCMDTable.PRDTEntry[i - 1].InterruptOnCompletion = 1;
                xCMDTableMem[(0x80 * i) + 0x00] = xCMDTable.PRDTEntry[i - 1].DBA << 1;
                xCMDTableMem[(0x80 * i) + 0x0C] = xCMDTable.PRDTEntry[i - 1].DBC;
                xCMDTableMem[(0x80 * i) + 0x0C] = (uint)xCMDTable.PRDTEntry[i - 1].InterruptOnCompletion << 31;
                aBuffer += 4 * 1024;
                aCount -= 16;
                i++;
            }
            Console.WriteLine("Last Command Table Initialization");

            // Last entry
            xCMDTable.PRDTEntry[i - 1].DBA = xDataBA;
            xCMDTable.PRDTEntry[i - 1].DBC = (aCount << 9) - 1;
            xCMDTable.PRDTEntry[i - 1].InterruptOnCompletion = 1;
            xCMDTableMem[(0x80 * i) + 0x00] = xCMDTable.PRDTEntry[i - 1].DBA << 1;
            xCMDTableMem[(0x80 * i) + 0x0C] = xCMDTable.PRDTEntry[i - 1].DBC;
            xCMDTableMem[(0x80 * i) + 0x0C] = (uint)xCMDTable.PRDTEntry[i - 1].InterruptOnCompletion << 31;

            Console.WriteLine("Setup Command FIS");
            // Setup the command
            uint xCMDFISAddress = Heap.MemAlloc(0x10);

            xCMDTableMem[0x00] = (uint)xCMDFISAddress;
            FISRegisterH2D xCMDFIS = new FISRegisterH2D();
            xCMDFIS.FISType = (byte)FISType.FIS_Type_RegisterH2D;
            xCMDFIS.IsCommand = 1;
            xCMDFIS.Command = (aWrite ? (byte)SATACommands.WriteDmaExt : (byte)SATACommands.ReadDmaExt);

            xCMDFIS.LBA0 = (byte)aStartLow;
            xCMDFIS.LBA1 = (byte)(aStartLow >> 8);
            xCMDFIS.LBA2 = (byte)(aStartLow >> 16);
            xCMDFIS.Device = 1 << 6; // LBA Mode
            
            xCMDFIS.LBA3 = 0x00;
            xCMDFIS.LBA4 = 0x00;
            xCMDFIS.LBA5 = 0x00;

            xCMDFIS.CountL = (byte)(aCount & 0xFF);
            xCMDFIS.CountH = (byte)((aCount >> 8) & 0xFF);
            var xCMDFISMem = new MemoryBlock((uint)xCMDTableMem[0x00], 0x10);
            Console.WriteLine("Setting values to memory offset: " + xCMDTableMem[0x00]);

            // DWord 0
            xCMDFISMem[0x00] = (byte)(xCMDFIS.FISType);         // FIS Type
            xCMDFISMem[0x00] = (byte)(xCMDFIS.IsCommand >> 15); // IsCommand(or)Control
            xCMDFISMem[0x00] = (byte)(xCMDFIS.Command >> 16);   // Command register

            // DWord 1
            xCMDFISMem[0x04] = (byte)(xCMDFIS.LBA0);            // LBA Low register
            xCMDFISMem[0x04] = (byte)(xCMDFIS.LBA1 >> 8);       // LBA Mid register
            xCMDFISMem[0x04] = (byte)(xCMDFIS.LBA2 >> 16);      // LBA Hgh register
            xCMDFISMem[0x04] = (byte)(xCMDFIS.Device >> 24);    //  Device register

            // DWord 2
            xCMDFISMem[0x08] = (byte)(xCMDFIS.LBA3);            // LBA register 
            xCMDFISMem[0x08] = (byte)(xCMDFIS.LBA4 >> 8);       // LBA register
            xCMDFISMem[0x08] = (byte)(xCMDFIS.LBA5 >> 16);      // LBA register

            // DWord 3
            xCMDFISMem[0x0C] = (byte)(xCMDFIS.CountL);          // Count Low register
            xCMDFISMem[0x0C] = (byte)(xCMDFIS.CountH);          // Count Hgh register

            Console.WriteLine(Cosmos.Common.Extensions.ToHexString.ToHex(new MemoryBlock(xCMDFISAddress, 0x10)[0x00]));

            Console.WriteLine("Spinning on xSpin");
            do
            {
                xSpin++;
                Console.WriteLine(xSpin);
            } while (((mSATAMemory[mPortLocation + 0x20] & (uint)(ATADeviceStatus.Busy | ATADeviceStatus.DRQ)) > 0 && xSpin < 1000000));

            if (xSpin == 1000000)
            {
                Console.WriteLine("Port timed out");
                return false;
            }

            mSATAMemory[mPortLocation + 0x38] = (uint)(1 << xSlot); // Issue with Command

            // Wait for Completion
            do
            {
                // If error occured
                if (((mSATAMemory[mPortLocation + 0x10] >> 30) & 1) != 0)
                {
                    Console.WriteLine("Write or Read Disk Error!");
                    return false;
                }
            } while (((mSATAMemory[mPortLocation + 0x38] >> xSlot) & 1) != 0);

            // Fill in the data
            if (!aWrite) 
            {
                aBuffer = xDataMem.Words[0x00];
            }

            return true;
        }

        public bool Read(PortRegisters aPort, uint aStartLow, uint aStartHigh, uint aCount, ushort aBuffer)
            => InternalTransfer(aPort, aStartLow, aStartHigh, aCount, aBuffer, false);

        public bool Write(PortRegisters aPort, uint aStartLow, uint aStartHigh, uint aCount, ushort aBuffer)
            => InternalTransfer(aPort, aStartLow, aStartHigh, aCount, aBuffer, true);

        public int FindCMDSlot(PortRegisters aPort)
        {
            // If not set in SACT and CI, the slot is free
            var xSlots = (mSATAMemory[mPortLocation + 0x34] | mSATAMemory[mPortLocation + 0x38]);
            //for(int i = 0; i < aPort.VendorSpecific.Length; i++)
            ////mSATADebugger.SendInternalNumber(aPort.VendorSpecific[i]);
            for (int i = 0; i < 32; i++)
            {
                if ((xSlots & 1) == 0)
                {
                    return i;
                }
                xSlots >>= 1;
            }
            //Console.WriteLine("Cannot find free Command list Entry");
            return -1;
        }

        public SATA(int xPort)
        {
        }

        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            CheckDataSize(aData, aBlockCount);
            if(SATAPorts.Count > 0)
            {
                var xPort = Registers.PortRegisters(SATAPorts[0]);
                for(int i = 0; i < aData.Length; i += 2)
                {
                    ushort xData = (ushort)((aData[i + 1] << 8) | aData[i]);
                    Read(xPort, (uint)aBlockNo, 0, (uint)aBlockCount, xData);
                    for (int ui = 0; ui < 200; ui++) ;
                    aData[i+0] = (byte)(xData);
                    aData[i+1] = (byte)(xData >> 8);
                }
            }
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            CheckDataSize(aData, aBlockCount);
            var xPort = Registers.PortRegisters(SATAPorts[0]);
            for (int i = 0; i < aData.Length; i += 2)
            {
                ushort xData = (ushort)((aData[i + 1] << 8) | aData[i]);
                Write(xPort, (uint)aBlockNo, 0, (uint)aBlockCount, xData);
                for (int ui = 0; ui < 200; ui++) ;
            }
            // Might not work for some reasons :|
        }
    }
}
