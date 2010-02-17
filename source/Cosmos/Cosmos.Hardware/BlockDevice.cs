using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
    public abstract class BlockDevice : Device {
        protected BlockDevice() {
            mType = DeviceType.Storage;
        }

        public abstract uint BlockSize {
            get;
        }

        public abstract uint BlockCount {
            get;
        }

        public abstract void ReadBlock(uint aBlock,
                                       byte[] aContents);

        public abstract void WriteBlock(uint aBlock,
                                        byte[] aContents);

        /// <summary>
        /// Tells whether this storage device is already used by for example a FS implementation or a partitioning implementation.
        /// </summary>
        public virtual bool Used {
            get;
            set;
        }
    }
}