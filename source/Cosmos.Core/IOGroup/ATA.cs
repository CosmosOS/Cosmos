using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.IOGroup
{
    public class ATA
    {
        /// <summary>
        /// Error IOPort
        /// </summary>
        public readonly int Error; // BAR0 + 1 - read only
        /// <summary>
        /// Features IOPort.
        /// </summary>
        public readonly int Features; // BAR0 + 1 - write only
        /// <summary>
        /// Data IOPort.
        /// </summary>
        public readonly int Data; // BAR0
        /// <summary>
        /// Sector Count IOPort.
        /// </summary>
        public readonly int SectorCount; // BAR0 + 2
        /// <summary>
        /// LBA0 IOPort.
        /// </summary>
        public readonly int LBA0; // BAR0 + 3
        /// <summary>
        /// LBA1 IOPort.
        /// </summary>
        public readonly int LBA1; // BAR0 + 4
        /// <summary>
        /// LBA2 IOPort.
        /// </summary>
        public readonly int LBA2; // BAR0 + 5
        /// <summary>
        /// Device select IOPort.
        /// </summary>
        public readonly int DeviceSelect; // BAR0 + 6
        /// <summary>
        /// Command IOPort.
        /// </summary>
        public readonly int Command; // BAR0 + 7 - write only
        /// <summary>
        /// Status IOPort.
        /// </summary>
        public readonly int Status; // BAR0 + 7 - read only
        /// <summary>
        /// Sector count IOPort.
        /// </summary>
        public readonly int SectorCountLBA48; // BAR0 + 8
        /// <summary>
        /// LBA3 IOPort.
        /// </summary>
        public readonly int LBA3; // BAR0 + 9
        /// <summary>
        /// LBA4 IOPort.
        /// </summary>
        public readonly int LBA4; // BAR0 + 10
        /// <summary>
        /// LBA5 IOPort.
        /// </summary>
        public readonly int LBA5; // BAR0 + 11
        /// <summary>
        /// Alternate Status IOPort.
        /// </summary>
        public readonly int AlternateStatus; // BAR1 + 2 - read only
        /// <summary>
        /// Control IOPort.
        /// </summary>
        public readonly int Control; // BAR1 + 2 - write only

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
            Error = Features = xBAR0 + 1;
            Data = xBAR0;
            SectorCount = xBAR0 + 2;
            LBA0 = xBAR0 + 3;
            LBA1 = xBAR0 + 4;
            LBA2 = xBAR0 + 5;
            DeviceSelect = xBAR0 + 6;
            Status = Command = xBAR0 + 7;
            SectorCountLBA48 = xBAR0 + 8;
            LBA3 = xBAR0 + 9;
            LBA4 = xBAR0 + 10;
            LBA5 = xBAR0 + 11;
            AlternateStatus = Control = xBAR1 + 2;
        }
        /// <summary>
        /// Waits for IO operations to complete.
        /// </summary>
        public void Wait()
        {
            // Used for the PATA and IOPort latency
            // Widely accepted method is to read the status register 4 times - approx. 400ns delay.
            IOPort.Wait();
            IOPort.Wait();
            IOPort.Wait();
            IOPort.Wait();
        }
        /// <summary>
        /// Get control base address.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        /// <returns>ushort value.</returns>
        private static int GetBAR1(bool aSecondary)
        {
            return aSecondary ? 0x0374 : 0x03F4;
        }
        /// <summary>
        /// Get command base address.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        /// <returns>ushort value.</returns>
        private static int GetBAR0(bool aSecondary)
        {
            return aSecondary ? 0x0170 : 0x01F0;
        }
    }
}
