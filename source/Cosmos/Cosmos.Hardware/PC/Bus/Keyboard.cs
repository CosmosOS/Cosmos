using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus.CPU {
    public class Keyboard : Cosmos.Hardware.SerialDevice {
        public Keyboard() {
            mType = DeviceType.Keyboard;
        }

        public void InterruptReceived() {
            byte xByte = Kernel.CPUBus.Read8(0x60);
            ByteReceived(xByte);
        }
		public override string Name {
			get {
				return "Keyboard";
			}
		}
    }
}
