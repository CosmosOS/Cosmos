using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class ATA : IOGroup {
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
