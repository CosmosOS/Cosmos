using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.BlockDevice
{
    public abstract class Ata : BlockDevice
    {
        internal static readonly Debugger AtaDebugger = new Debugger("HAL", "Ata");

        protected Ata()
        {
            mBlockSize = 512;
        }

        [Flags]
        protected enum ErrorFlags : byte
        {
            AddressMarkNotFound = 0x00,
            TrackZeroNotFound = 0x01,
            AbortedCommand = 0x02,
            MediaChangeRequest = 0x03,
            IDNotFound = 0x04,
            MediaChanged = 0x05,
            UncorrectableData = 0x06,
            BadBlockDetected = 0x07
        }

        [Flags]
        protected enum StatusFlags : byte
        {
            None = 0x00,
            Error = 0x01,
            Index = 0x02,
            CorrectedError = 0x04,
            DRQ = 0x08,
            DSC = 0x10,
            DriveFault = 0x20,
            DRD = 0x40,
            Idle = 0x50,
            Busy = 0x80
        }

        [Flags]
        protected enum DeviceSelectFlags : byte
        {
            // Bits 0-3: Head Number for CHS.
            // Bit 4: Slave Bit. (0: Selecting Master Drive, 1: Selecting Slave Drive).
            Slave = 0x10,
            //* Bit 6: LBA (0: CHS, 1: LBA).
            LBA = 0x40,
            //* Bit 5: Obsolete and isn't used, but should be set.
            //* Bit 7: Obsolete and isn't used, but should be set.
            Default = 0xA0
        }

        protected enum Cmd : byte
        {
            ReadPio = 0x20,
            ReadPioExt = 0x24,
            ReadDma = 0xC8,
            ReadDmaExt = 0x25,
            WritePio = 0x30,
            WritePioExt = 0x34,
            WriteDma = 0xCA,
            WriteDmaExt = 0x35,
            CacheFlush = 0xE7,
            CacheFlushExt = 0xEA,
            Identify = 0xEC,
            IdentifyPacket = 0xA1,
            // ATAPI
            Atapi_Eject = 0x1B,
            Atapi_ReadCapacity = 0x25,
            Atapi_Read = 0x28,
            Atapi_ReadToc = 0x43,
            Atapi_Packet = 0xA0
        }

        public enum DriveTypeEnum
        {
            Null,
            ATA,
            ATAPI
        }

        // In future may need to add a None for PCI ATA controllers.
        // Or maybe they all have Primary and Secondary on them as well.
        public enum ControllerIdEnum
        {
            Primary,
            Secondary
        }

        public enum BusPositionEnum
        {
            Master,
            Slave
        }

        public override string ToString()
        {
            return "Ata (Abstract)";
        }
    }
}
