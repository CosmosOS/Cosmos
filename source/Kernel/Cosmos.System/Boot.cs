using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Cosmos.System {
    public abstract class Boot {
        public void EntryPoint() {
            Run();
        }

        protected abstract void Run();
    }
}
