using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.HAL.Devices {
    public abstract class Processor : Device {
        // Generic method that is specific to each CPU ring.
        // Designed to allow limited extra API communication to CPU from HAL.
        // Optionally implemented.
        public virtual UInt64 SetOption(UInt32 aID, UInt64 aValue = 0) {
            return 0;
        }
    }
}
