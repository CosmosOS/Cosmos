using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public abstract class BlockDevice: Device {
		public abstract uint BlockSize {
			get;
		}

		public abstract ulong BlockCount {
			get;
		}

		public abstract byte[] ReadBlock(ulong aBlock);
		public abstract void WriteBlock(ulong aBlock, byte[] aContents);
	}
}
