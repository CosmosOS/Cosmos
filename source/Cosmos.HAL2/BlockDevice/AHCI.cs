using System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.HAL.BlockDevice.Ports;
using Cosmos.HAL.BlockDevice.Registers;
using Cosmos.Core;
using Cosmos.Core.Memory.Old;

namespace Cosmos.HAL.BlockDevice
{
    public class AHCI
    {
        internal static Debugger mAHCIDebugger = new Debugger("HAL", "AHCI");
        internal static PCIDevice xDevice = HAL.PCI.GetDeviceClass(HAL.ClassID.MassStorageController,
                                                                   HAL.SubclassID.SATAController,
                                                                   HAL.ProgramIF.SATA_AHCI);

        private static List<StoragePort> mPorts = new List<StoragePort>();
        private static GenericRegisters mGeneric;
        private static ulong mABAR;

        // Capabilities
        #region Capabilities
        private static bool Supports64bitAddressing;
        private static bool SupportsNativeCommandQueuing;
        private static bool SupportsSNotificationRegister;
        private static bool SupportsMechanicalPresenceSwitch;
        private static bool SupportsStaggeredSpinup;
        private static bool SupportsAggressiveLinkPowerManagement;
        private static bool SupportsActivityLED;
        private static bool SupportsCommandListOverride;
        private static uint InterfaceSpeedSupport;
        private static bool SupportsAHCIModeOnly;
        private static bool SupportsPortMutliplier;
        private static bool FISBasedSwitchingSupported;
        private static bool PIOMultipleDRQBlock;
        private static bool SlumberStateCapable;
        private static bool PartialStateCapable;
        private static uint NumOfCommandSlots;
        private static bool CommandCompletionCoalsecingSupported;
        private static bool EnclosureManagementSupported;
        private static bool SupportsExternalSATA;
        private static uint NumOfPorts;
        #endregion

        // Constants
        public const ulong RegularSectorSize = 512UL;

        // Informations
        public string SerialNo;
        public string Version
        {
            get => ((byte)mGeneric.AHCIVersion >> 24) + (byte)(mGeneric.AHCIVersion >> 16) + "." + (byte)(mGeneric.AHCIVersion >> 8) + ((byte)(mGeneric.AHCIVersion) > 0 ? "." + (byte)mGeneric.AHCIVersion : "");
        }

        internal static void InitDriver()
        {

            if (xDevice != null)
            {
                AHCI Driver = new AHCI(xDevice);
            }
        }

        internal PCIDevice GetDevice() => xDevice;

        public AHCI(PCIDevice aAHCIDevice)
        {
            aAHCIDevice.EnableBusMaster(true);
            aAHCIDevice.EnableMemory(true);

            mABAR = aAHCIDevice.BaseAddressBar[5].BaseAddress;
            mGeneric = new GenericRegisters(aAHCIDevice.BaseAddressBar[5].BaseAddress);
            mGeneric.GlobalHostControl |= (1U << 31); // Enable AHCI

            GetCapabilities();
            mPorts.Capacity = (int)NumOfPorts;
            GetPorts();

            foreach(StoragePort xPort in mPorts)
            {
                if(xPort.mPortType == PortType.SATA)
                {
                    mAHCIDebugger.Send($"{xPort.mPortName} Port 0:{xPort.mPortNumber}");
                    var xMBRData = new byte[512];
                    xPort.ReadBlock(0UL, 1U, xMBRData);
                    var xMBR = new MBR(xMBRData);
                    
                    if (xMBR.EBRLocation != 0)
                    {
                        // EBR Detected!
                        mAHCIDebugger.Send("EBR Detected within MBR code");
                        var xEBRData = new byte[512];
                        xPort.ReadBlock(xMBR.EBRLocation, 1U, xEBRData);
                        var xEBR = new EBR(xEBRData);
                        for (int i = 0; i < xEBR.Partitions.Count; i++)
                        {
                            //var xPart = xEBR.Partitions[i];
                            //var xPartDevice = new Partition(xSATA, xPart.StartSector, xPart.SectorCount);
                            //Devices.Add(xPartDevice);
                        }
                    }
                    
                    mAHCIDebugger.Send($"Number of MBR partitions found on port 0:{xPort.mPortNumber} ");
                    mAHCIDebugger.SendNumber(xMBR.Partitions.Count);
                    for (int i = 0; i < xMBR.Partitions.Count; i++)
                    {
                        var xPart = xMBR.Partitions[i];
                        if (xPart == null)
                        {
                            Console.WriteLine("Null partition found at idx: " + i);
                        }
                        else
                        {
                            var xPartDevice = new Partition(xPort, xPart.StartSector, xPart.SectorCount);
                            BlockDevice.Devices.Add(xPartDevice);
                            Console.WriteLine("Found partition at idx: " + i);
                        }
                    }
                }
                else if (xPort.mPortType == PortType.SATAPI)
                {
                    mAHCIDebugger.Send($"{xPort.mPortName} Port 0:{xPort.mPortNumber}");

                    // Just to test Read Sector!
                    
                    //byte[] xMBRData = new byte[512];
                    //xPort.ReadBlock(0UL, 1U, xMBRData);
                    //MBR xMBR = new MBR(xMBRData);

                    //mAHCIDebugger.Send($"Number of corrupted MBR partitions found on port 0:{xPort.mPortNumber} ");
                    //mAHCIDebugger.SendNumber(xMBR.Partitions.Count);
                    //for (int i = 0; i < xMBR.Partitions.Count; i++)
                    //{
                    //    var xPart = xMBR.Partitions[i];
                    //    if (xPart == null)
                    //    {
                    //        Console.WriteLine("Null partition found at idx: " + i);
                    //    }
                    //    else
                    //    {
                    //        var xPartDevice = new Partition(xPort, xPart.StartSector, xPart.SectorCount);
                    //        BlockDevice.Devices.Add(xPartDevice);
                    //        Console.WriteLine("Found corrupted partition at idx: " + i);
                    //    }
                    //}
                }
            }
        }

        public static void HBAReset()
        {
            mGeneric.GlobalHostControl = 1;
            uint HR = 0;
            do
            {
                Wait(1);
                HR = mGeneric.GlobalHostControl & 1;
            } while (HR != 0);
        }

        public static void Wait(int microsecondsTimeout)
        {
            byte xVoid;
            for (int i = 0; i < microsecondsTimeout; i++)
            {
                // Random IOPort
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
                xVoid = Core.Global.BaseIOGroups.TextScreen.Data1.Byte;
            }
        }

        private void GetCapabilities()
        {
            NumOfPorts                            = mGeneric.Capabilities & 0x1F;
            SupportsExternalSATA                  = (mGeneric.Capabilities >> 5 & 1) == 1;
            EnclosureManagementSupported          = (mGeneric.Capabilities >> 6 & 1) == 1;
            CommandCompletionCoalsecingSupported  = (mGeneric.Capabilities >> 7 & 1) == 1;
            NumOfCommandSlots                     = mGeneric.Capabilities >> 8 & 0x1F;
            PartialStateCapable                   = (mGeneric.Capabilities >> 13 & 1) == 1;
            SlumberStateCapable                   = (mGeneric.Capabilities >> 14 & 1) == 1;
            PIOMultipleDRQBlock                   = (mGeneric.Capabilities >> 15 & 1) == 1;
            FISBasedSwitchingSupported            = (mGeneric.Capabilities >> 16 & 1) == 1;
            SupportsPortMutliplier                = (mGeneric.Capabilities >> 17 & 1) == 1;
            SupportsAHCIModeOnly                  = (mGeneric.Capabilities >> 18 & 1) == 1;
            InterfaceSpeedSupport                 = mGeneric.Capabilities >> 20 & 0x0F;
            SupportsCommandListOverride           = (mGeneric.Capabilities >> 24 & 1) == 1;
            SupportsActivityLED                   = (mGeneric.Capabilities >> 25 & 1) == 1;
            SupportsAggressiveLinkPowerManagement = (mGeneric.Capabilities >> 26 & 1) == 1;
            SupportsStaggeredSpinup               = (mGeneric.Capabilities >> 27 & 1) == 1;
            SupportsMechanicalPresenceSwitch      = (mGeneric.Capabilities >> 28 & 1) == 1;
            SupportsSNotificationRegister         = (mGeneric.Capabilities >> 29 & 1) == 1;
            SupportsNativeCommandQueuing          = (mGeneric.Capabilities >> 30 & 1) == 1;
            Supports64bitAddressing               = (mGeneric.Capabilities >> 31 & 1) == 1;
        }

        private void GetPorts()
        {
            // Search for disks
            var xImplementedPort = mGeneric.ImplementedPorts;
            var xPort = 0;
            for(; xPort < 32; xPort++)
            {
                if ((xImplementedPort & 1) != 0)
                {
                    PortRegisters xPortReg = new PortRegisters((uint)mABAR + 0x100, (uint)xPort);
                    PortType PortType = CheckPortType(xPortReg);
                    xPortReg.mPortType = PortType;
                    var xPortString = "0:" + ((xPort.ToString().Length <= 1) ? xPort.ToString().PadLeft(1, '0') : xPort.ToString());
                    if (PortType == PortType.SATA) // If Port type was SATA.
                    {
                        mAHCIDebugger.Send("Initializing Port " + xPortString + " with type SATA");
                        PortRebase(xPortReg, (uint)xPort);
                        var xSATAPort = new SATA(xPortReg);
                        mPorts.Add(xSATAPort);
                    }
                    else if (PortType == PortType.SATAPI) // If Port type was SATAPI.
                    {
                        mAHCIDebugger.Send("Initializing Port " + xPortString + " with type Serial ATAPI");
                        //PortRebase(xPortReg, (uint)xPort);
                        //var xSATAPIPort = new SATAPI(xPortReg);
                        //mPorts.Add(xSATAPIPort);
                    }
                    else if (PortType == PortType.SEMB) // If Port type was SEMB.
                    {
                        mAHCIDebugger.Send("SEMB Drive at port " + xPortString + " found, which is not supported yet!");
                    }
                    else if (PortType == PortType.PM) // If Port type was Port Mulitplier.
                    {
                        mAHCIDebugger.Send("Port Multiplier Drive at port " + xPortString + " found, which is not supported yet!");
                    }
                    else if (PortType != PortType.Nothing)
                        mAHCIDebugger.Send("Unknown drive found with signature: 0x" + xPortReg.SIG);// If Implemented Port value was not zero and non of the above.
                }
                xImplementedPort >>= 1;
            }
        }

        private PortType CheckPortType(PortRegisters aPort)
        {
            var xIPM = (InterfacePowerManagementStatus)((aPort.SSTS >> 8) & 0x0F);
            var xSPD = (CurrentInterfaceSpeedStatus)((aPort.SSTS >> 4) & 0x0F);
            var xDET = (DeviceDetectionStatus)(aPort.SSTS & 0x0F);
            var xSignature = aPort.SIG;

            // Check if the port is active!
            if (xIPM != InterfacePowerManagementStatus.Active)
                return PortType.Nothing;
            if (xDET != DeviceDetectionStatus.DeviceDetectedWithPhy)
                return PortType.Nothing;

            switch ((AHCISignature)xSignature >> 16)
            {
                case AHCISignature.SATA: return PortType.SATA;
                case AHCISignature.SATAPI: return PortType.SATAPI;
                case AHCISignature.SEMB: return PortType.SEMB;
                case AHCISignature.PortMultiplier: return PortType.PM;
                case AHCISignature.Nothing: return PortType.Nothing;
            }
        }

        private void PortRebase(PortRegisters aPort, uint aPortNumber)
        {
            mAHCIDebugger.Send("Stop");
            if (!StopCMD(aPort)) SATA.PortReset(aPort);

            aPort.CLB = (uint)Base.AHCI + (0x400 * aPortNumber);
            aPort.FB = (uint)Base.AHCI + 0x8000 + (0x100 * aPortNumber);

            aPort.SERR = 1;
            aPort.IS = 0;
            aPort.IE = 0;

            new MemoryBlock(aPort.CLB, 1024).Fill(0);
            new MemoryBlock(aPort.FB, 256).Fill(0);

            GetCommandHeader(aPort); // Rebase Command header
            
            if (!StartCMD(aPort)) SATA.PortReset(aPort);

            aPort.IS = 0;
            aPort.IE = 0xFFFFFFFF;

            mAHCIDebugger.Send("Finished!");
        }

        private static HBACommandHeader[] GetCommandHeader(PortRegisters aPort)
        {
            HBACommandHeader[] xCMDHeader = new HBACommandHeader[32];
            for (uint i = 0; i < xCMDHeader.Length; i++)
            {
                xCMDHeader[i] = new HBACommandHeader(aPort.CLB, i)
                {
                    PRDTL = 8, 
                    
                    CTBA = (uint)(Base.AHCI + 0xA000) + (0x2000 * aPort.mPortNumber) + (0x100 * i),

                    CTBAU = 0
                };
                new MemoryBlock(xCMDHeader[i].CTBA, 0x100).Fill(0);
            }
            return xCMDHeader;
        }

        private bool StartCMD(PortRegisters aPort)
        {
            int xSpin;
            for (xSpin = 0; xSpin < 101; xSpin++)
            {
                if ((aPort.CMD & (uint)CommandAndStatus.CMDListRunning) == 0) break;
                Wait(5000);
            }
            if (xSpin == 101) return false;

            aPort.CMD |= (1 << 4);
            aPort.CMD |= (1 << 0);

            return true;
        }

        // Thanks to Microsoft for the detailed info about stopping command process!
        private bool StopCMD(PortRegisters aPort)
        {
            int xSpin;
            aPort.CMD &= ~(1U << 0); //Bit 0

            for (xSpin = 0; xSpin < 101; xSpin++)
            {
                if ((aPort.CMD & (uint)CommandAndStatus.CMDListRunning) == 0) break;
                Wait(5000);
            }
            if (xSpin == 101) return false;

            for (xSpin = 0; xSpin < 101; xSpin++)
            {
                if ((aPort.CI == 0)) break;
                Wait(50);
            }
            if (xSpin == 101) return false;

            aPort.CMD &= ~(1U << 4); //Bit 4

            if (SupportsCommandListOverride)
            {
                if ((aPort.TFD & (uint)ATADeviceStatus.Busy) != 0)
                {
                    aPort.CMD |= (1U << 3);
                }
            }

            for (xSpin = 0; xSpin < 101; xSpin++)
            {
                if ((aPort.CMD & (uint)CommandAndStatus.CMDListRunning) == 0 &&
                    (aPort.CMD & (uint)CommandAndStatus.FISRecieveRunning) == 0 &&
                    (aPort.CMD & (uint)CommandAndStatus.StartProccess) == 0 &&
                    (aPort.CMD & (uint)CommandAndStatus.FISRecieveEnable) == 0) break;
                Wait(5000);
            }
            if (xSpin == 101)
            {
                if (SupportsCommandListOverride)
                    aPort.CMD |= (1U << 3);
                else
                    aPort.CMD &= ~(1U << 3);
                return false;
            }

            return true;
        }
    }
}
