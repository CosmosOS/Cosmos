using System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.HAL.BlockDevice.Ports;
using Cosmos.HAL.BlockDevice.Registers;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice
{
    public class AHCI
    {
        internal static Debugger ahciDebugger = new Debugger("AHCI");
        internal static PCIDevice device = PCI.GetDeviceClass(ClassID.MassStorageController,
                                                                   SubclassID.SATAController,
                                                                   ProgramIF.SATA_AHCI);

        private static List<StoragePort> ports = new List<StoragePort>();
        private static GenericRegisters generic;
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
        public string Version => ((byte)generic.AHCIVersion >> 24) + (byte)(generic.AHCIVersion >> 16) + "." + (byte)(generic.AHCIVersion >> 8) + ((byte)generic.AHCIVersion > 0 ? "." + (byte)generic.AHCIVersion : "");

        internal static void InitDriver()
        {

            if (device != null)
            {
                AHCI Driver = new(device);
            }
        }

        internal PCIDevice GetDevice() => device;

        public AHCI(PCIDevice aAHCIDevice)
        {
            aAHCIDevice.EnableBusMaster(true);
            aAHCIDevice.EnableMemory(true);

            mABAR = aAHCIDevice.BaseAddressBar[5].BaseAddress;
            generic = new GenericRegisters(aAHCIDevice.BaseAddressBar[5].BaseAddress);
            generic.GlobalHostControl |= 1U << 31; // Enable AHCI

            GetCapabilities();
            ports.Capacity = (int)NumOfPorts;
            GetPorts();

            foreach (StoragePort xPort in ports)
            {
                if (xPort.mPortType == PortType.SATA)
                {
                    ahciDebugger.Send($"{xPort.mPortName} Port 0:{xPort.mPortNumber}");

                    IDE.ScanAndInitPartitions(xPort);
                }
                else if (xPort.mPortType == PortType.SATAPI)
                {
                    ahciDebugger.Send($"{xPort.mPortName} Port 0:{xPort.mPortNumber}");

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
            generic.GlobalHostControl = 1;
            uint HR = 0;
            do
            {
                Wait(1);
                HR = generic.GlobalHostControl & 1;
            } while (HR != 0);
        }

        public static void Wait(int microsecondsTimeout)
        {
            for (int i = 0; i < microsecondsTimeout; i++)
            {
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
                IOPort.Wait();
            }
        }

        private void GetCapabilities()
        {
            NumOfPorts = generic.Capabilities & 0x1F;
            SupportsExternalSATA = (generic.Capabilities >> 5 & 1) == 1;
            EnclosureManagementSupported = (generic.Capabilities >> 6 & 1) == 1;
            CommandCompletionCoalsecingSupported = (generic.Capabilities >> 7 & 1) == 1;
            NumOfCommandSlots = generic.Capabilities >> 8 & 0x1F;
            PartialStateCapable = (generic.Capabilities >> 13 & 1) == 1;
            SlumberStateCapable = (generic.Capabilities >> 14 & 1) == 1;
            PIOMultipleDRQBlock = (generic.Capabilities >> 15 & 1) == 1;
            FISBasedSwitchingSupported = (generic.Capabilities >> 16 & 1) == 1;
            SupportsPortMutliplier = (generic.Capabilities >> 17 & 1) == 1;
            SupportsAHCIModeOnly = (generic.Capabilities >> 18 & 1) == 1;
            InterfaceSpeedSupport = generic.Capabilities >> 20 & 0x0F;
            SupportsCommandListOverride = (generic.Capabilities >> 24 & 1) == 1;
            SupportsActivityLED = (generic.Capabilities >> 25 & 1) == 1;
            SupportsAggressiveLinkPowerManagement = (generic.Capabilities >> 26 & 1) == 1;
            SupportsStaggeredSpinup = (generic.Capabilities >> 27 & 1) == 1;
            SupportsMechanicalPresenceSwitch = (generic.Capabilities >> 28 & 1) == 1;
            SupportsSNotificationRegister = (generic.Capabilities >> 29 & 1) == 1;
            SupportsNativeCommandQueuing = (generic.Capabilities >> 30 & 1) == 1;
            Supports64bitAddressing = (generic.Capabilities >> 31 & 1) == 1;
        }

        private void GetPorts()
        {
            // Search for disks
            var xImplementedPort = generic.ImplementedPorts;
            var xPort = 0;
            for (; xPort < 32; xPort++)
            {
                if ((xImplementedPort & 1) != 0)
                {
                    PortRegisters xPortReg = new PortRegisters((uint)mABAR + 0x100, (uint)xPort);
                    PortType PortType = CheckPortType(xPortReg);
                    xPortReg.mPortType = PortType;
                    var xPortString = "0:" + (xPort.ToString().Length <= 1 ? xPort.ToString().PadLeft(1, '0') : xPort.ToString());
                    if (PortType == PortType.SATA) // If Port type was SATA.
                    {
                        ahciDebugger.Send("Initializing Port " + xPortString + " with type SATA");
                        PortRebase(xPortReg, (uint)xPort);
                        SATA xSATAPort = new(xPortReg);
                        ports.Add(xSATAPort);
                    }
                    else if (PortType == PortType.SATAPI) // If Port type was SATAPI.
                    {
                        ahciDebugger.Send("Initializing Port " + xPortString + " with type Serial ATAPI");
                        //PortRebase(xPortReg, (uint)xPort);
                        //var xSATAPIPort = new SATAPI(xPortReg);
                        //mPorts.Add(xSATAPIPort);
                    }
                    else if (PortType == PortType.SEMB) // If Port type was SEMB.
                    {
                        ahciDebugger.Send("SEMB Drive at port " + xPortString + " found, which is not supported yet!");
                    }
                    else if (PortType == PortType.PM) // If Port type was Port Mulitplier.
                    {
                        ahciDebugger.Send("Port Multiplier Drive at port " + xPortString + " found, which is not supported yet!");
                    }
                    else if (PortType != PortType.Nothing)
                    {
                        throw new Exception("SATA Error");
                    }
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

            xSignature >>= 16;

            switch ((AHCISignature)xSignature)
            {
                case AHCISignature.SATA: return PortType.SATA;
                case AHCISignature.SATAPI: return PortType.SATAPI;
                case AHCISignature.SEMB: return PortType.SEMB;
                case AHCISignature.PortMultiplier: return PortType.PM;
                case AHCISignature.Nothing: return PortType.Nothing;
                default: throw new Exception("SATA Error: Unknown drive found at port: " + aPort.mPortNumber); ;
            }
        }

        private void PortRebase(PortRegisters aPort, uint aPortNumber)
        {
            ahciDebugger.Send("Stop");
            if (!StopCMD(aPort)) SATA.PortReset(aPort);

            aPort.CLB = (uint)Base.AHCI + 0x400 * aPortNumber;
            aPort.FB = (uint)Base.AHCI + 0x8000 + 0x100 * aPortNumber;

            aPort.SERR = 1;
            aPort.IS = 0;
            aPort.IE = 0;

            new MemoryBlock(aPort.CLB, 1024).Fill(0);
            new MemoryBlock(aPort.FB, 256).Fill(0);

            GetCommandHeader(aPort); // Rebase Command header

            if (!StartCMD(aPort)) SATA.PortReset(aPort);

            aPort.IS = 0;
            aPort.IE = 0xFFFFFFFF;

            ahciDebugger.Send("Finished!");
        }

        private static HBACommandHeader[] GetCommandHeader(PortRegisters aPort)
        {
            HBACommandHeader[] xCMDHeader = new HBACommandHeader[32];
            for (uint i = 0; i < xCMDHeader.Length; i++)
            {
                xCMDHeader[i] = new HBACommandHeader(aPort.CLB, i)
                {
                    PRDTL = 8,

                    CTBA = (uint)(Base.AHCI + 0xA000) + 0x2000 * aPort.mPortNumber + 0x100 * i,

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

            aPort.CMD |= 1 << 4;
            aPort.CMD |= 1 << 0;

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
                if (aPort.CI == 0) break;
                Wait(50);
            }
            if (xSpin == 101) return false;

            aPort.CMD &= ~(1U << 4); //Bit 4

            if (SupportsCommandListOverride)
            {
                if ((aPort.TFD & (uint)ATADeviceStatus.Busy) != 0)
                {
                    aPort.CMD |= 1U << 3;
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
                    aPort.CMD |= 1U << 3;
                else
                    aPort.CMD &= ~(1U << 3);
                return false;
            }

            return true;
        }
    }
}
