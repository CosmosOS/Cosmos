using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Platform {
    static public class TempHack {
        // Need to hack up Cosmos a bit
        // Remove hard ref from System to HAL.
        // Cant dyn load yet, so use IL2CPU to drag it in dynamically to prevent compile time usage
        // Dont have reflection yet, but can make a Cosmos specific method to at least allow activationon by type.
        static public void Init() {
            SATA.InitHAL();
        }
    }
}
