using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.XSharp;

namespace Cosmos.Compiler.DebugStub {
    public class DebugStub : CodeGroup {
        protected const uint VidBase = 0xB8000;

        // Called before Kernel runs. Inits debug stub, etc
        public class Init : CodeBlock {
            public override void Assemble() {
                Call<Cls>();
                // 10 lines down, 20 cols in
                ESI = DebugStub.VidBase + (10 * 80 + 20) * 2;

                //TODO: X# upgrade this to accept char
                AL = (byte)'H';
                Memory[ESI, 8] = AL;
                ESI++;
                ESI++;

                AL = (byte)'e';
                Memory[ESI, 8] = AL; 
                ESI++;
                ESI++;

                AL = (byte)'l';
                Memory[ESI, 8] = AL;
                ESI++;
                ESI++;

                AL = (byte)'l';
                Memory[ESI, 8] = AL; 
                ESI++;
                ESI++;

                AL = (byte)'o';
                Memory[ESI, 8] = AL; 
                ESI++;
                ESI++;

                Label = "temp2";
                //Jump("temp2");
            }
        }

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
                ESI = DebugStub.VidBase;
                // TODO: X# upgrade this
                Label = "DebugStub_Cls_More";
                    //TODO: Fix to direct memory write after we fix the X# bug with Memory[ESI, 8] = 0x0A;
                    AL = 0x00;
                    Memory[ESI, 8] = AL; // Text
                    ESI++;

                    AL = 0x0A;
                    Memory[ESI, 8] = AL; // Colour
                    ESI++;

                    ESI.Compare(DebugStub.VidBase + 25 * 80 * 2);
                JumpIf(Flags.LessThan, "DebugStub_Cls_More");
            }
        }

    }
}
