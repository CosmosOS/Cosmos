using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class DebugStub : X.Y86 {
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;

        protected void TraceOff() {
            Label = "DebugStub_TraceOff";
            Memory["DebugTraceMode", 32] = 0;
            Jump("DebugStub_AfterCmd");
        }

        protected void TraceOn() {
            Label = "DebugStub_TraceOn";
            Memory["DebugTraceMode", 32] = 1;
            Jump("DebugStub_AfterCmd");
        }

        protected void Break() {
            Label = "DebugStub_Break";
            Memory["DebugTraceMode", 32] = 4;
            Jump("DebugPoint_WaitCmd");
        }

        protected void Step() {
            Label = "DebugStub_Step";
            Memory["DebugTraceMode", 32] = 4;
            Jump("DebugStub_AfterCmd");
        }

        protected void SendTrace() {
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
        }

        protected void WriteByteToDebugger() {
            Label = "WriteByteToComPort";
            Label = "WriteByteToComPort_Wait";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x20);
            JumpIf(Flags.Zero, "WriteByteToComPort_Wait");
            DX = mComAddr;
            AL = Memory[ESP + 4];
            Port[DX] = AL;
            Return(4);
        }

        protected void WaitCmd() {
            Label = "DebugPoint_WaitCmd";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugPoint_WaitCmd");
            Jump("DebugPoint_ProcessCmd");
        }

        protected void DebugSuspend() {
            Label = "DebugPoint_DebugSuspend";
            Return();
        }

        protected void DebugResume() {
            Label = "DebugPoint_DebugResume";
            Return();
        }

        public void Main(UInt16 aComAddr) {
            mComAddr = aComAddr;
            mComStatusAddr = (UInt16)(aComAddr + 5);

            TraceOff();
            TraceOn();
            Break();
            Step();
            SendTrace();
            WriteByteToDebugger();
            WaitCmd();
            DebugSuspend();
            DebugResume();

            //"DebugTraceMode dd 1");
            //"DebugStatus dd 0");
            //"DebugSuspendLevel dd 0");

            Label = "DebugPoint__";
            PushAll32();
            EBP = ESP;
            EBP.Add(32);

            // Check DebugTraceMode
            EAX = Memory["DebugTraceMode"];
            AL.Compare(0);
            JumpIf(Flags.Equal, "DebugPoint_NoTrace");
                Call("DebugWriteEIP");

                EAX = Memory["DebugTraceMode"];
                AL.Compare(4);
                JumpIf(Flags.Equal, "DebugPoint_WaitCmd");
            Label = "DebugPoint_NoTrace";

            // Is there a new incoming command?
            Label = "DebugPoint_CheckCmd";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);

            //Test current state, then separate command structure, when in break Wait for unbreak only but process others
            JumpIf(Flags.Zero, "DebugStub_AfterCmd");
                Label = "DebugPoint_ProcessCmd";
                DX = aComAddr;
                AL = Port[DX];
                AL.Compare(1);
                JumpIf(Flags.Equal, "DebugStub_TraceOff");
                AL.Compare(2);
                JumpIf(Flags.Equal, "DebugStub_TraceOn");
                AL.Compare(3);
                JumpIf(Flags.Equal, "DebugStub_Step");
                AL.Compare(4);
                JumpIf(Flags.Equal, "DebugStub_Break");
                // -Evaluate variables
                // -Step to next debug call
                // Break points
                // Immediate break
            Label = "DebugStub_AfterCmd";

            PopAll32();
            Return();
        }
    }
}
