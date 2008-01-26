using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
    public class Global {
        protected static Processor mProcessor;
        public static Processor Processor {
            get { return mProcessor; }
        }

        protected static List<Device> mDevices = new List<Device>();
        public static List<Device> Devices {
            get { return mDevices; }
        }

    }
}
