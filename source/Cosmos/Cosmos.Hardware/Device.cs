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

        protected DeviceType mType = DeviceType.Unknown;
        public DeviceType Type {
            get { return mType; }
        }
    }
}
