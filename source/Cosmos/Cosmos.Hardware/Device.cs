using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
    // This is the base class for all hardware devices. Hardware devices 
    // are defined here as abstracts and overridden in specific implementations
    // later
    public abstract class Device {
        public enum DeviceType { Unknown, Other, Keyboard, Mouse, Storage };

        public Device() {
        }

        static protected List<Device> mDevices = new List<Device>();
        static public List<Device> Devices {
            get { return mDevices; }
        }

        static public void Add(Device aDevice) {
			mDevices.Add(aDevice);
        }

        static public List<Device> Find(DeviceType aType) {
            var xResult = new List<Device>();
            foreach (var xDevice in mDevices) {
                if (xDevice.Type == aType) {
                    xResult.Add(xDevice);
                }
            }
            return xResult;
        }

        protected DeviceType mType = DeviceType.Unknown;
        public DeviceType Type {
            get { return mType; }
        }
    }
}
