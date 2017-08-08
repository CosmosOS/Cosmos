using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Platform.PC {
    public class DeviceMgr : HAL.DeviceMgr {
        public DeviceMgr() {
            var xProcessor = new Devices.Processor();
            Add(xProcessor);
        }
    }
}
