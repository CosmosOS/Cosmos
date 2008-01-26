using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Storage {
    public abstract class Storage: Hardware {
		public abstract uint BlockSize {
			get;
		}

		public abstract unsafe bool ReadBlock(uint aBlock, byte* aBuffer);
		public abstract bool WriteBlock(uint aBlock, byte[] aData);
    }
}
