using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Cosmos.System {
    public abstract class Boot {
        // IL2CPU finds this method by name and this is where Cosmos takes the hand off from the bootloader.
        public void EntryPoint() {
            Run();
        }

        protected abstract void Run();
    }
}
