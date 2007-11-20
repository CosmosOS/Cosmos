using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Plugs {
    //Attrib here to mark this as a plug class for system.console
    class Console {
        // we dont need attrib here - all methods marked public will be plugged
        public void WriteLine(string aOut) {
        }
    }
}
