using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2 {
    // This class is currently intended for 'slow' serial devices
    // like the keyboard, mouse etc.
    // May need to change or add another class in the future
    // for higher speed serial devices. However I suspect
    // higher speed serial devices will use a block transfer
    // type.
    public abstract class DeviceSerial : Device {
        public delegate void ByteReceivedDelegate(byte aValue);
        public ByteReceivedDelegate ByteReceived;
    }
}
