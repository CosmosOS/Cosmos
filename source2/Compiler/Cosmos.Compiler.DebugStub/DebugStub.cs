using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.XSharp;

namespace Cosmos.Compiler.DebugStub {
    public class DebugStub : CodeGroup {
        // Input AL
        // Output: None
        // Modifies: EAX, EDX, ESI
        public class WriteALToComPort : CodeBlock {
            // inherited .Call()

            public override void Assemble() {
                // Implicit: Write out DebugStub.WriteALToComPort
                //Label = "WriteALToComPort";

                //TODO: Make a data point to put this in instead of using stack
                //EAX.Push();
                //ESI = ESP;
                //Call("WriteByteToComPort");
                //EAX.Pop(); // Is a local, cant use Return(4)
                
                // Return is implicit
                //Return();
            }
        }
    }
}
