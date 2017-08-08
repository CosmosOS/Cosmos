using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.Devices;

namespace Cosmos.HAL {
    public abstract class DeviceMgr {
        protected readonly List<Devices.Device> mAll = new List<Devices.Device>();
        protected readonly IList<Devices.Device> mAllRO;
        public IList<Devices.Device> All {
            get { return mAllRO; }
        }

        public DeviceMgr() {
            mAllRO = mAll.AsReadOnly();
        }

        protected Processor mProcessor;
        public Processor Processor {
            get { return mProcessor; }
        }

        public void Add(Devices.Device aDevice) {
            mAll.Add(aDevice);

            if (aDevice is Processor) {
                mProcessor = (Processor)aDevice;
            }
        }
    }
}
