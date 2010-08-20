using System;
using System.Collections.Generic;
using System.Text;

namespace BreakpointsKernel {
    public class Kernel {
        // This is a temp wrapper to bridge from existing project format 
        // to the new coming project format.
        public static void Boot() {
            var xKernel = new BreakpointsOS();
            xKernel.Start();
        }
    }
}
