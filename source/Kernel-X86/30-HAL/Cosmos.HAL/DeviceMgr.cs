using System;
using System.Collections.Generic;
using System.Text;

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

        public void Add(Devices.Device aDevice) {
            mAll.Add(aDevice);
        }
    }
}
