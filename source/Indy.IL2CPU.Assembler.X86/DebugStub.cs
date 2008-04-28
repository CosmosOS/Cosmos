using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class DebugStub : X.Y86 {
        public void Main(UInt16 aComAddr) {
            UInt16 xComStatusAddr = (UInt16)(aComAddr + 5);
            Label = "WriteByteToComPort";
            Label = "WriteByteToComPort_Wait";
            DX = xComStatusAddr;
            AL = Port[DX];
            AL.Test(0x20);
            JumpIf(Flags.Zero, "WriteByteToComPort_Wait");
            DX = aComAddr;
            AL = Memory[ESP + 4];
            Port[DX] = AL;
            Return(4);

            Label = "DebugWriteEIP";
            AL = Memory[EBP + 3];
            EAX.Push();
            Call("WriteByteToComPort");
            AL = Memory[EBP + 2];
            EAX.Push();
            Call("WriteByteToComPort");
            AL = Memory[EBP + 1];
            EAX.Push();
            Call("WriteByteToComPort");
            AL = Memory[EBP];
            EAX.Push();
            Call("WriteByteToComPort");
            Return();

            Label = "DebugPoint_WaitCmd";
            DX = xComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugPoint_WaitCmd");
            Jump("DebugPoint_ProcessCmd");

            Label = "DebugPoint__";
            PushAll32();
            EBP = ESP;
            EBP.Add(32);

            // Check DebugTraceMode
            EAX = Memory["DebugTraceMode"];
            AL.Compare(1);
            JumpIf(Flags.Equal, "DebugPoint_NoTrace");
            //
            Call("DebugWriteEIP");
            //
            EAX = Memory["DebugTraceMode"];
            AL.Compare(4);
            JumpIf(Flags.Equal, "DebugPoint_WaitCmd");
            Label = "DebugPoint_NoTrace";

            // Is there a new incoming command?
            Label = "DebugPoint_CheckCmd";
            DX = xComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugPoint_AfterCmd");

            Label = "DebugPoint_ProcessCmd";
            DX = aComAddr;
            AL = Port[DX];
            AL.Compare(1);
            JumpIf(Flags.NotEqual, "DebugPoint_Cmd02");
            Memory["DebugTraceMode", 32] = 1;
            Jump("DebugPoint_CheckCmd");
            //
            Label = "DebugPoint_Cmd02";
            AL.Compare(2);
            JumpIf(Flags.NotEqual, "DebugPoint_Cmd03");
            Memory["DebugTraceMode", 32] = 2;
            Jump("DebugPoint_CheckCmd");
            //
            Label = "DebugPoint_Cmd03";
            AL.Compare(3);
            JumpIf(Flags.NotEqual, "DebugPoint_Cmd04");
            Memory["DebugTraceMode", 32] = 4;
            Jump("DebugPoint_AfterCmd");
            //
            Label = "DebugPoint_Cmd04";
            AL.Compare(4);
            JumpIf(Flags.NotEqual, "DebugPoint_Cmd05");
            Memory["DebugTraceMode", 32] = 4;
            Jump("DebugPoint_WaitCmd");
            //
            Label = "DebugPoint_Cmd05";
            // -Evaluate variables
            // -Step to next debug call
            // Break points
            // Immediate break
            Label = "DebugPoint_AfterCmd";

            // DebugTraceMode
            // 1 - No tracing
            // 2 - Tracing
            // 3 - 
            // 4 - Break and wait

            PopAll32();
            Return();
        }
    }
}
