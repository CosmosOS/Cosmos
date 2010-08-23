using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class ATA : IOGroup {

        [FlagsAttribute]
        enum Status : byte {
            ATA_SR_BSY = 0x80
            , ATA_SR_DRD = 0x40
            , ATA_SR_DF  = 0x20
            , ATA_SR_DSC = 0x10
            , ATA_SR_DRQ = 0x08
            , ATA_SR_COR = 0x04
            , ATA_SR_IDX = 0x02
            , ATA_SR_ERR = 0x01
        };

        public readonly IOPort PortControl;
        public readonly IOPort PortData;

        internal ATA(bool aSecondary) {
            if (!aSecondary) {
                PortControl = new IOPort(0x3F4);
                PortData = new IOPort(0x1F0);
            } else {
                PortControl = new IOPort(0x374);
                PortData = new IOPort(0x170);
            }
        }
    }
}
