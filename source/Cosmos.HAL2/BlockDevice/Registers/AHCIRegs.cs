using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice.Registers
{
    public enum Base : uint
    {
        AHCI = 0x00400000
    }
    // Registers
    public class GenericRegisters
    {
        private MemoryBlock xBlock;
        private uint xAddress;
        public GenericRegisters(uint aAddress)
        {
            xAddress = aAddress;
            xBlock = new MemoryBlock(aAddress, 0x100);
        }

        public uint Capabilities
        {
            get { return xBlock[0x00]; }
            set { xBlock[0x00] = value; }
        }

        public uint GlobalHostControl
        {
            get { return xBlock[0x04]; }
            set { xBlock[0x04] = value; }
        }
        public uint InterruptStatus
        {
            get { return xBlock[0x08]; }
            set { xBlock[0x08] = value; }
        }

        public uint ImplementedPorts
        {
            get { return xBlock[0x0C]; }
            set { xBlock[0x0C] = value; }
        }

        public uint AHCIVersion
        {
            get { return xBlock[0x10]; }
            set { xBlock[0x10] = value; }
        }

        public uint CCC_Control
        {
            get { return xBlock[0x14]; }
            set { xBlock[0x14] = value; }
        }

        public uint CCC_Ports
        {
            get { return xBlock[0x18]; }
            set { xBlock[0x18] = value; }
        }

        public uint EM_Location
        {
            get { return xBlock[0x1C]; }
            set { xBlock[0x1C] = value; }
        }

        public uint EM_Control
        {
            get { return xBlock[0x20]; }
            set { xBlock[0x20] = value; }
        }

        public uint ExtendedCapabilities
        {
            get { return xBlock[0x24]; }
            set { xBlock[0x24] = value; }
        }

        public uint BIOSHandOffStatus
        {
            get { return xBlock[0x28]; }
            set { xBlock[0x28] = value; }
        }
    }

    public class PortRegisters
    {
        private MemoryBlock xBlock;
        private uint xAddress;
        public uint mPortNumber;
        public PortType mPortType = PortType.Nothing;
        public bool Active;
        public PortRegisters(uint aAddress, uint aPortNumber)
        {
            xAddress = aAddress;
            mPortNumber = aPortNumber;
            xBlock = new MemoryBlock(aAddress + (0x80 * mPortNumber), 0x80);
            Active = false;
        }

        public uint CLB
        {
            get { return xBlock[0x00]; }
            set { xBlock[0x00] = value; }
        }

        public uint CLBU
        {
            get { return xBlock[0x04]; }
            set { xBlock[0x04] = value; }
        }

        public uint FB
        {
            get { return xBlock[0x08]; }
            set { xBlock[0x08] = value; }
        }

        public uint FBU
        {
            get { return xBlock[0x0C]; }
            set { xBlock[0x0C] = value; }
        }

        public uint IS
        {
            get { return xBlock[0x10]; }
            set { xBlock[0x10] = value; }
        }

        public uint IE
        {
            get { return xBlock[0x14]; }
            set { xBlock[0x14] = value; }
        }

        public uint CMD
        {
            get { return xBlock[0x18]; }
            set { xBlock[0x18] = value; }
        }

        public uint Reserved
        {
            get { return xBlock[0x1C]; }
        }

        public uint TFD
        {
            get { return xBlock[0x20]; }
            set { xBlock[0x20] = value; }
        }

        public uint SIG
        {
            get { return xBlock[0x24]; }
            set { xBlock[0x24] = value; }
        }

        public uint SSTS
        {
            get { return xBlock[0x28]; }
            set { xBlock[0x28] = value; }
        }

        public uint SCTL
        {
            get { return xBlock[0x2C]; }
            set { xBlock[0x2C] = value; }
        }

        public uint SERR
        {
            get { return xBlock[0x30]; }
            set { xBlock[0x30] = value; }
        }

        public uint SACT
        {
            get { return xBlock[0x34]; }
            set { xBlock[0x34] = value; }
        }

        public uint CI
        {
            get { return xBlock[0x38]; }
            set { xBlock[0x38] = value; }
        }

        public uint SNTF
        {
            get { return xBlock[0x3C]; }
            set { xBlock[0x3C] = value; }
        }

        public uint FBS
        {
            get { return xBlock[0x40]; }
            set { xBlock[0x40] = value; }
        }

        public uint DEVSLP
        {
            get { return xBlock[0x44]; }
            set { xBlock[0x44] = value; }
        }
    }

    // Command List
    public class HBACommandHeader
    {
        private MemoryBlock xBlock;
        private uint xAddress;//
        private uint xSlot;
        public HBACommandHeader(uint aAddress, uint aSlot)
        {
            xAddress = aAddress;
            xSlot = aSlot;
            xBlock = new MemoryBlock(aAddress + (32 * aSlot), 0x20);
            xBlock.Fill(0);
        }

        public byte CFL
        {
            get { return (byte)(xBlock.Bytes[0x00] & 0x1F); }
            set { xBlock.Bytes[0x00] = value; }
        }
        public byte ATAPI
        {
            get { return (byte)((xBlock.Bytes[0x00] >> 5) & 1); }
            set { xBlock.Bytes[0x00] |= (byte)(value << 5); }
        }
        public byte Write
        {
            get { return (byte)((xBlock.Bytes[0x00] >> 6) & 1); }
            set { xBlock.Bytes[0x00] |= (byte)(value << 6); }
        }
        public byte Prefetchable
        {
            get { return (byte)((xBlock.Bytes[0x00] >> 7) & 1); }
            set { xBlock.Bytes[0x00] |= (byte)(value << 7); }
        }
        public byte Reset
        {
            get { return (byte)((xBlock.Bytes[0x01]) & 1); }
            set { xBlock.Bytes[0x01] |= (byte)(value); }
        }
        public byte BIST
        {
            get { return (byte)((xBlock.Bytes[0x01] >> 1) & 1); }
            set { xBlock.Bytes[0x01] |= (byte)(value << 1); }
        }
        public byte ClearBusy
        {
            get { return (byte)((xBlock.Bytes[0x01] >> 2) & 1); }
            set { xBlock.Bytes[0x01] |= (byte)(value << 2); }
        }
        public byte Reserved
        {
            get { return (byte)((xBlock.Bytes[0x01] >> 3) & 1); }
        }
        public byte PMP
        {
            get { return (byte)((xBlock.Bytes[0x01] >> 4) & 0x0F); }
            set { xBlock.Bytes[0x01] = (byte)(value << 4); }
        }
        public ushort PRDTL
        {
            get { return xBlock.Words[0x02]; }
            set { xBlock.Words[0x02] = value; }
        }

        public uint PRDBC
        {
            get { return xBlock[0x04]; }
            set { xBlock[0x04] = value; }
        }

        public uint CTBA
        {
            get { return xBlock[0x08]; }
            set { xBlock[0x08] = value; }
        }

        public uint CTBAU
        {
            get { return xBlock[0x0C]; }
            set { xBlock[0x0C] = value; }
        }

        public uint Reserved1
        {
            get { return xBlock[0x10]; }
        }

        public uint Reserved2
        {
            get { return xBlock[0x14]; }
        }

        public uint Reserved3
        {
            get { return xBlock[0x18]; }
        }

        public uint Reserved4
        {
            get { return xBlock[0x1C]; }
        }
    }

    public class HBACommandTable
    {
        private MemoryBlock xBlock;
        private uint xAddress;
        public HBACommandTable(uint aAddress, uint aPRDTCount)
        {
            xAddress = aAddress;
            xBlock = new MemoryBlock(aAddress, 0x80);
            xBlock.Fill(0);
            PRDTEntry = new HBAPRDTEntry[aPRDTCount];
            for(uint i = 0; i < aPRDTCount; i++)
            {
                PRDTEntry[i] = new HBAPRDTEntry(aAddress + 0x80, i);
            }
        }

        public uint CFIS
        {
            get { return xAddress; }
        }

        public uint ACMD
        {
            get { return xAddress + 0x40; }
        }

        public uint Reserved
        {
            get { return xBlock[0x50]; }
        }

        public HBAPRDTEntry[] PRDTEntry;
    }

    public class HBAPRDTEntry
    {
        private MemoryBlock xBlock;
        private uint xAddress;
        private uint xEntry;
        public HBAPRDTEntry(uint aAddress, uint aEntry)
        {
            xAddress = aAddress;
            xEntry = aEntry;
            xBlock = new MemoryBlock(aAddress + (0x10 * xEntry), 0x10);
            xBlock.Fill(0);
        }

        public uint DBA
        {
            get { return xBlock[0x00]; }
            set { xBlock[0x00] = value; }
        }

        public uint DBAU
        {
            get { return xBlock[0x04]; }
            set { xBlock[0x04] = value; }
        }

        public uint Reserved
        {
            get { return xBlock[0x08]; }
        }

        public uint DBC
        {
            get { return xBlock[0x0C] & 0x3FFFFF; }
            set { xBlock[0x0C] = value; }
        }
        public uint Reserved1
        {
            get { return xBlock[0x0E] << 6; }
        }
        public byte InterruptOnCompletion
        {
            get { return (byte)(xBlock.Bytes[0x0F] >> 7); }
            set { xBlock.Bytes[0x0F] = (byte)(value << 7); }
        }
    }

    // FISes
    public class FISRegisterH2D
    {
        private MemoryBlock xBlock;
        private uint xAddress;
        public FISRegisterH2D(uint aAddress)
        {
            xAddress = aAddress;
            xBlock = new MemoryBlock(aAddress, 20);
            xBlock.Fill(0);
        }

        public byte FISType
        {
            get { return (byte)(xBlock.Bytes[0x00]); }
            set { xBlock.Bytes[0x00] = value; }
        }

        public byte IsCommand
        {
            get { return (byte)((xBlock.Bytes[0x01] >> 7)); }
            set { xBlock.Bytes[0x01] = (byte)(value << 7); }
        }
        public byte Command
        {
            get { return xBlock.Bytes[0x02]; }
            set { xBlock.Bytes[0x02] = value; }
        }
        public byte FeatureLow
        {
            get { return xBlock.Bytes[0x03]; }
            set { xBlock.Bytes[0x03] = value; }
        }

        public byte LBA0
        {
            get { return xBlock.Bytes[0x04]; }
            set { xBlock.Bytes[0x04] = value; }
        }
        public byte LBA1
        {
            get { return xBlock.Bytes[0x05]; }
            set { xBlock.Bytes[0x05] = value; }
        }
        public byte LBA2
        {
            get { return xBlock.Bytes[0x06]; }
            set { xBlock.Bytes[0x06] = value; }
        }
        public byte Device
        {
            get { return xBlock.Bytes[0x07]; }
            set { xBlock.Bytes[0x07] = value; }
        }

        public byte LBA3
        {
            get { return xBlock.Bytes[0x08]; }
            set { xBlock.Bytes[0x08] = value; }
        }
        public byte LBA4
        {
            get { return xBlock.Bytes[0x09]; }
            set { xBlock.Bytes[0x09] = value; }
        }
        public byte LBA5
        {
            get { return xBlock.Bytes[0x0A]; }
            set { xBlock.Bytes[0x0A] = value; }
        }
        public byte FeatureHigh
        {
            get { return xBlock.Bytes[0x0B]; }
            set { xBlock.Bytes[0x0B] = value; }
        }

        public byte CountL
        {
            get { return xBlock.Bytes[0x0C]; }
            set { xBlock.Bytes[0x0C] = value; }
        }
        public byte CountH
        {
            get { return xBlock.Bytes[0x0D]; }
            set { xBlock.Bytes[0x0D] = value; }
        }
        public byte ICC
        {
            get { return xBlock.Bytes[0x0E]; }
            set { xBlock.Bytes[0x0E] = value; }
        }
        public byte Control
        {
            get { return xBlock.Bytes[0x0F]; }
            set { xBlock.Bytes[0x0F] = value; }
        }

        public byte Reserved1
        {
            get { return xBlock.Bytes[0x10]; }
        }
        public byte Reserved2
        {
            get { return xBlock.Bytes[0x11]; }
        }
        public byte Reserved3
        {
            get { return xBlock.Bytes[0x12]; }
        }
        public byte Reserved4
        {
            get { return xBlock.Bytes[0x13]; }
        }
    }

    public class FISRegisterD2H
    {
        private MemoryBlock xBlock;
        private uint xAddress;
        public FISRegisterD2H(uint aAddress)
        {
            xAddress = aAddress;
            xBlock = new MemoryBlock(aAddress, 20);
        }

        public FISType FISType
        {
            get { return (FISType)xBlock.Bytes[0x00]; }
            set { xBlock.Bytes[0x00] = (byte)value; }
        }
        public byte PortMultiplier
        {
            get { return (byte)(xBlock.Bytes[0x00] << 8); }
            set { xBlock.Bytes[0x00] = (byte)value; }
        }
        public byte Reserved
        {
            get { return (byte)(xBlock.Bytes[0x00] << 12); }
        }
        public byte InterruptBit
        {
            get { return (byte)(xBlock.Bytes[0x00] << 14); }
            set { xBlock.Bytes[0x00] = (byte)(value << 14); }
        }
        public byte Reserved1
        {
            get { return (byte)(xBlock.Bytes[0x00] << 15); }
        }

        public byte Status
        {
            get { return (byte)(xBlock.Bytes[0x00] << 16); }
            set { xBlock.Bytes[0x00] = (byte)(value << 16); }
        }
        public byte Error
        {
            get { return (byte)(xBlock.Bytes[0x00] << 24); }
            set { xBlock.Bytes[0x00] = (byte)(value << 24); }
        }

        public byte LBA0
        {
            get { return xBlock.Bytes[0x04]; }
            set { xBlock.Bytes[0x04] = value; }
        }
        public byte LBA1
        {
            get { return (byte)(xBlock.Bytes[0x04] << 8); }
            set { xBlock.Bytes[0x04] = (byte)(value << 8); }
        }
        public byte LBA2
        {
            get { return (byte)(xBlock.Bytes[0x04] << 16); }
            set { xBlock.Bytes[0x04] = (byte)(value << 16); }
        }
        public byte Device
        {
            get { return (byte)(xBlock.Bytes[0x04] << 24); }
            set { xBlock.Bytes[0x04] = (byte)(value << 24); }
        }

        public byte LBA3
        {
            get { return xBlock.Bytes[0x08]; }
            set { xBlock.Bytes[0x08] = value; }
        }
        public byte LBA4
        {
            get { return (byte)(xBlock.Bytes[0x08] << 8); }
            set { xBlock.Bytes[0x08] = (byte)(value << 8); }
        }
        public byte LBA5
        {
            get { return (byte)(xBlock.Bytes[0x08] << 16); }
            set { xBlock.Bytes[0x08] = (byte)(value << 16); }
        }
        public byte Reserved2
        {
            get { return (byte)(xBlock.Bytes[0x08] << 24); }
        }

        public byte CountL
        {
            get { return xBlock.Bytes[0x0C]; }
            set { xBlock.Bytes[0x0C] = value; }
        }
        public byte CountH
        {
            get { return (byte)(xBlock.Bytes[0x0C] << 8); }
            set { xBlock.Bytes[0x0C] = (byte)(value << 8); }
        }
        public byte Reserved3
        {
            get { return (byte)(xBlock.Bytes[0x0C] << 16); }
        }
        public byte Reserved4
        {
            get { return (byte)(xBlock.Bytes[0x0C] << 24); }
        }

        public byte Reserved5
        {
            get { return xBlock.Bytes[0x10]; }
        }
        public byte Reserved6
        {
            get { return xBlock.Bytes[0x11]; }
        }

        public byte Reserved7
        {
            get { return xBlock.Bytes[0x12]; }
        }
        public byte Reserved8
        {
            get { return xBlock.Bytes[0x13]; }
        }
    }

    // Enums
    public enum PortType
    {
        Nothing = 0x00,
        SATA = 0x01,
        SATAPI = 0x02,
        SEMB = 0x03,
        PM = 0x04
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

    public enum FISSize : byte
    {
        //FISRegisterH2D = Marshal.SizeOf(FISRegisterH2D);
        FISRegisterH2D = 40 / sizeof(uint)
    }

    public enum AHCISignature : uint // Drive Signature to identify what drive is plugged to Port X:X
    {
        SATA = 0x0000,
        PortMultiplier = 0x9669,
        SATAPI = 0xEB14,
        SEMB = 0xC33C,
        Nothing = 0xFFFF
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

    public enum ATACommands : byte
    {
        ReadDma = 0xC8,
        ReadDmaExt = 0x25,
        WriteDma = 0xCA,
        WriteDmaExt = 0x35,
        CacheFlush = 0xE7,
        CacheFlushExt = 0xEA,
        Packet = 0xA0,
        IdentifyPacket = 0xA1,
        IdentifyDMA = 0xEE,
        Identify = 0xEC,
        Read = 0xA8,
        Eject = 0x1B
    }

    public enum Bases : uint
    {
        SATA = 0x00400000
    }
}
