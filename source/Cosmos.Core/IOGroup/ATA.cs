﻿using System;
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
        //* Error Register: BAR0 + 1; // Read Only
        //* Features Register: BAR0 + 1; // Write Only
        /// <summary>
        /// Sector count IOPort.
        /// </summary>
        public readonly IOPortWrite SectorCount;
        // ATA_REG_SECCOUNT1  0x08 - HOB
        /// <summary>
        /// LBA0 IOPort.
        /// </summary>
        public readonly IOPort LBA0;
        /// <summary>
        /// LBA1 IOPort.
        /// </summary>
        public readonly IOPort LBA1;
        /// <summary>
        /// LBA2 IOPort.
        /// </summary>
        public readonly IOPort LBA2;
        // ATA_REG_LBA3       0x09 - HOB
        // ATA_REG_LBA4       0x0A - HOB
        // ATA_REG_LBA5       0x0B - HOB
        /// <summary>
        /// Device select IOPort.
        /// </summary>
        public readonly IOPortWrite DeviceSelect;
        /// <summary>
        /// Command IOPort.
        /// </summary>
        public readonly IOPortWrite Command;
        /// <summary>
        /// Status IOPort.
        /// </summary>
        public readonly IOPortRead Status;
        //* Alternate Status Register: BAR1 + 2; // Read Only.
        /// <summary>
        /// Control IOPort.
        /// </summary>
        public readonly IOPortWrite Control;
        //* DEVADDRESS: BAR1 + 2; // I don't know what is the benefit from this register

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
            var xBAR1 = GetBAR1(aSecondary);
            Data = new IOPort(xBAR0);
            SectorCount = new IOPortWrite(xBAR0, 2);
            LBA0 = new IOPort(xBAR0, 3);
            LBA1 = new IOPort(xBAR0, 4);
            LBA2 = new IOPort(xBAR0, 5);
            Command = new IOPortWrite(xBAR0, 7);
            Status = new IOPortRead(xBAR0, 7);
            DeviceSelect = new IOPortWrite(xBAR0, 6);
            Control = new IOPortWrite(xBAR1, 2);
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
