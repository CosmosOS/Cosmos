using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.XSharp;

namespace Cosmos.Compiler.DebugStub {
    public class DebugStub : CodeGroup {

        public class WriteALToComPort : CodeBlock {
            // Input: AL
            // Output: None
            // Modifies: EAX, EDX, ESI
            public override void Assemble() {
                EAX.Push();
                ESI = ESP;
                Call("WriteByteToComPort");
                EAX.Pop(); // Is a local, cant use Return(4)
            }

        }

        public class Cls : CodeBlock {
            public override void Assemble() {
                uint xBase = 0xB8000;
                
                ESI = xBase;
                // TODO: X# upgrade this
                Label = "DebugStub_Cls_More";
                    //TODO: Fix to direct memory write after we fix the X# bug with Memory[ESI, 8] = 0x0A;
                    AL = 0x0A;
                    Memory[ESI, 8] = AL; // Colour
                    ESI++;
                    AL = 0x00;
                    Memory[ESI, 8] = AL; // Text
                    ESI++;
                    ESI.Compare(xBase + 25 * 80 * 2);
                JumpIf(Flags.LessThan, "DebugStub_Cls_More");
            }
        }

    }
}
