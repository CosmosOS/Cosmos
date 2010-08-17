using System;
using System.Collections.Generic;
using System.Text;

namespace BreakpointsKernel
{
    public class Kernel
    {
        // This is a temp wrapper to bridge from existing project format 
        // to the new coming project format.
        public static void Boot() {
            Cosmos.Debug.Debugger.Send("Starting kernel boot now");
            Cosmos.Debug.Debugger.Send("Value 2"); 
            Cosmos.Debug.Debugger.Send("Value: " + MyTestClass.Value.ToString());
            Cosmos.Debug.Debugger.Send("Value 3");
            //var xKernel = new BreakpointsOS();
            //xKernel.Start();
        }
    }
}
