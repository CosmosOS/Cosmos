using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Platform.PC.Devices {
    public class Processor : HAL.Devices.Processor {
        public override ulong SetOption(uint aID, ulong aValue = 0) {
            CPU.x86.TempDebug.ShowText('D');
            if (aID == 1) {
                CPU.x86.TempDebug.ShowText('E');
            }
            return 0;
        }
    }
}
