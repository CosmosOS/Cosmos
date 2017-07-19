using System;

namespace Cosmos.System {
    public class FileSystem {
        public string GetFileSomething() {
            var x = Cosmos.HAL.SATA.New();
            return x.GetSomethingNonPlatform() + " via FileSystem";
        }

    }
}
