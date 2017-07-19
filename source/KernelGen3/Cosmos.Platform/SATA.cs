using System;

namespace Cosmos.Platform {
    public class SATA : Cosmos.HAL.SATA {
        public void InitHAL() {
            Cosmos.HAL.SATA.mSataHalType = typeof(SATA);
        }

        public override string GetSomething() {
            return "HAL.SATA";
        }
    }
}
