using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    static public class Global {
        static private MemoryBlock mTextScreenMemory;
        static public MemoryBlock TextScreenMemory {
            get { return mTextScreenMemory; }
        }

        static public void Init() {
            // Temp
            Kernel.Global.Init();
            // End Temp

            // These are common/fixed pieces of hardware. PCI, USB etc should be self discovering
            // and not hardcoded like this.
            // Further more some kind of security needs to be applied to these, but even now
            // at least we have isolation between the consumers that use these.
            mTextScreenMemory = new MemoryBlock(0xB8000, 80 * 25 * 2 / 4);
        }
    }
}
