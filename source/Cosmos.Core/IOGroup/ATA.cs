using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// IOGruop ATA.
    /// </summary>
    public class ATA : IOGroup
    {
        /// <summary>
        /// Data IOPort.
        /// </summary>
        public readonly IOPort Data;

        /// <summary>
        /// Error IOPort.
        /// </summary>
        public readonly IOPortRead Error;

        /// <summary>
        /// Features IOPort.
        /// </summary>
        public readonly IOPortWrite Features;

        /// <summary>
        /// Sector count IOPort.
        /// </summary>
        public readonly IOPort SectorCount;

        /// <summary>
        /// Sector number IOPort.
        /// </summary>
        public readonly IOPort SectorNumber;
        /// <summary>
        /// LBA low IOPort.
        /// </summary>
        public readonly IOPort LBALow;

        /// <summary>
        /// Cylinder low IOPort.
        /// </summary>
        public readonly IOPort CylinderLow;
        /// <summary>
        /// LBA mid IOPort.
        /// </summary>
        public readonly IOPort LBAMid;

        /// <summary>
        /// Cylinder high IOPort.
        /// </summary>
        public readonly IOPort CylinderHigh;
        /// <summary>
        /// LBA high IOPort.
        /// </summary>
        public readonly IOPort LBAHigh;

        /// <summary>
        /// Drive/Head select IOPort.
        /// </summary>
        public readonly IOPortWrite DriveSelect;

        /// <summary>
        /// Command IOPort.
        /// </summary>
        public readonly IOPortWrite Command;

        /// <summary>
        /// Status IOPort.
        /// </summary>
        public readonly IOPortRead Status;

        /// <summary>
        /// Control IOPort.
        /// </summary>
        public readonly IOPortWrite Control;

        /// <summary>
        /// Alternate status IOPort.
        /// </summary>
        public readonly IOPortRead AlternateStatus;

        /// <summary>
        /// Create new instance of the <see cref="ATA"/> class.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        internal ATA(bool aSecondary)
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
            Data = new IOPort(xBAR0);
            Error = new IOPortRead(xBAR0, 1);
            Features = new IOPortWrite(xBAR0, 1);
            SectorCount = new IOPort(xBAR0, 2);
            SectorNumber = new IOPort(xBAR0, 3);
            LBALow = new IOPort(xBAR0, 3);
            CylinderLow = new IOPort(xBAR0, 4);
            LBAMid = new IOPort(xBAR0, 4);
            CylinderHigh = new IOPort(xBAR0, 5);
            LBAHigh = new IOPort(xBAR0, 5);
            DriveSelect = new IOPortWrite(xBAR0, 6);
            Status = new IOPortRead(xBAR0, 7);
            Command = new IOPortWrite(xBAR0, 7);

            var xBAR1 = GetBAR1(aSecondary);
            Control = new IOPortWrite(xBAR1, 2);
            AlternateStatus = new IOPortRead(xBAR1, 2);
        }

        /// <summary>
        /// Get control base address.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        /// <returns>ushort value.</returns>
        private static ushort GetBAR1(bool aSecondary)
        {
            UInt16 xBAR1 = (UInt16)(aSecondary ? 0x0374 : 0x03F4);
            return xBAR1;
        }

        /// <summary>
        /// Get command base address.
        /// </summary>
        /// <param name="aSecondary">True if secondary ATA.</param>
        /// <returns>ushort value.</returns>
        private static ushort GetBAR0(bool aSecondary)
        {
            UInt16 xBAR0 = (UInt16)(aSecondary ? 0x0170 : 0x01F0);
            return xBAR0;
        }
    }
}
