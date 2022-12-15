using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.IOGroup
{
    public class ATA : IOGroup
    {
        /// <summary>
        /// Error IOPort
        /// </summary>
        public readonly ushort Error; // BAR0 + 1 - read only
        /// <summary>
        /// Features IOPort.
        /// </summary>
        public readonly ushort Features; // BAR0 + 1 - write only
        /// <summary>
        /// Data IOPort.
        /// </summary>
        public readonly ushort Data; // BAR0
        /// <summary>
        /// Sector Count IOPort.
        /// </summary>
        public readonly ushort SectorCount; // BAR0 + 2
        /// <summary>
        /// LBA0 IOPort.
        /// </summary>
        public readonly ushort LBA0; // BAR0 + 3
        /// <summary>
        /// LBA1 IOPort.
        /// </summary>
        public readonly ushort LBA1; // BAR0 + 4
        /// <summary>
        /// LBA2 IOPort.
        /// </summary>
        public readonly ushort LBA2; // BAR0 + 5
        /// <summary>
        /// Device select IOPort.
        /// </summary>
        public readonly ushort DeviceSelect; // BAR0 + 6
        /// <summary>
        /// Command IOPort.
        /// </summary>
        public readonly ushort Command; // BAR0 + 7 - write only
        /// <summary>
        /// Status IOPort.
        /// </summary>
        public readonly ushort Status; // BAR0 + 7 - read only
        /// <summary>
        /// Sector count IOPort.
        /// </summary>
        public readonly ushort SectorCountLBA48; // BAR0 + 8
        /// <summary>
        /// LBA3 IOPort.
        /// </summary>
        public readonly ushort LBA3; // BAR0 + 9
        /// <summary>
        /// LBA4 IOPort.
        /// </summary>
        public readonly ushort LBA4; // BAR0 + 10
        /// <summary>
        /// LBA5 IOPort.
        /// </summary>
        public readonly ushort LBA5; // BAR0 + 11
        /// <summary>
        /// Alternate Status IOPort.
        /// </summary>
        public readonly ushort AlternateStatus; // BAR1 + 2 - read only
        /// <summary>
        /// Control IOPort.
        /// </summary>
        public readonly ushort Control; // BAR1 + 2 - write only

        /// <summary>
        /// Constructor for ATA-spec device (including ATAPI?)
        /// aSecondary boolean to check if Primary or Secondary channel, used in modern ATA controllers
        /// </summary>
        /// <param name="aSecondary"></param>
        public ATA(bool aSecondary)
        {
            if (aSecondary)
            {
                Global.mDebugger.Send("Creating Secondary ATA IOGroup");
            }
            else
            {
                Global.mDebugger.Send("Creating Primary ATA IOGroup");
            }

            var xBAR0 = GetBAR0(aSecondary);
            var xBAR1 = GetBAR1(aSecondary);
            Error = Features = (ushort)(xBAR0 + 1);
            Data = xBAR0;
            SectorCount = (ushort)(xBAR0 + 2);
            LBA0 = (ushort)(xBAR0 + 3);
            LBA1 = (ushort)(xBAR0 + 4);
            LBA2 = (ushort)(xBAR0 + 5);
            DeviceSelect = (ushort)(xBAR0 + 6);
            Status = Command = (ushort)(xBAR0 + 7);
            SectorCountLBA48 = (ushort)(xBAR0 + 8);
            LBA3 = (ushort)(xBAR0 + 9);
            LBA4 = (ushort)(xBAR0 + 10);
            LBA5 = (ushort)(xBAR0 + 11);
            AlternateStatus = Control = (ushort)(xBAR1 + 2);
        }
        /// <summary>
        /// Waits for IO operations to complete.
        /// </summary>
        public void Wait()
        {
            // Used for the PATA and IOPort latency
            // Widely accepted method is to read the status register 4 times - approx. 400ns delay.
            var wait = IOPort.Read8(Status);
            wait = IOPort.Read8(Status);
            wait = IOPort.Read8(Status);
            wait = IOPort.Read8(Status);
        }
        /// <summary>
        /// Get control base address.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        /// <returns>ushort value.</returns>
        private static ushort GetBAR1(bool aSecondary)
        {
            ushort xBAR1 = (ushort)(aSecondary ? 0x0374 : 0x03F4);
            return xBAR1;
        }
        /// <summary>
        /// Get command base address.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        /// <returns>ushort value.</returns>
        private static ushort GetBAR0(bool aSecondary)
        {
            ushort xBAR0 = (ushort)(aSecondary ? 0x0170 : 0x01F0);
            return xBAR0;
        }
    }
}
