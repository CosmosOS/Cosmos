using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL {
    // This and Core.Bootstrap are static on purpose to prevent
    // memalloc. Although kernel has already used it, in the future we should call this pre kernel alloc so we
    // can better control the heap init.
    public static class Bootstrap {
        // The goal of init is to just "barely" get the system up
        // plus the console and debug stub (its self upping). Nothing more....
        // In the future it might also bring up very basic devices such as serial.
        public static void Init() {
            Core.Bootstrap.Init();
        }
    }
}
