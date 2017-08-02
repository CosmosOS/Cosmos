using System;

namespace Cosmos.System {
    public class FileSystem {
        public string GetFileSomething() {
            // Need to change to dynamic load to prevent compile time access
            // and then call init.
            //Cosmos.Platform.TempHack.Init();

            var x = Cosmos.HAL.SATA.New();
            return x.GetSomethingNonPlatform() + " via FileSystem";
        }

    }
}
