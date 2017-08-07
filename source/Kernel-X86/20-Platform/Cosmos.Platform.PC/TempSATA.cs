using System;

namespace Cosmos.Platform.PC {
    public class SATA : Cosmos.HAL.SATA {
        static public void InitHAL() {
            Cosmos.HAL.SATA.mSataHalType = typeof(SATA);
        }

        public override string GetSomething() {
            return "HAL.SATA";
        }
    }
}
