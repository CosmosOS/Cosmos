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
            Jump("DebugStub_Exit");
        }

        protected void TraceOn() {
            Label = "DebugStub_TraceOn";
            Memory["DebugTraceMode", 32] = 1;
            Jump("DebugStub_Exit");
        }

        protected void Break() {
            Label = "DebugStub_Break";
            Memory["DebugTraceMode", 32] = 4;
            Call("DebugPoint_WaitCmd");
            Jump("DebugPoint_ProcessCmd");
        }

        protected void Step() {
            Label = "DebugStub_Step";
            Memory["DebugTraceMode", 32] = 4;
            Jump("DebugStub_Exit");
        }

        protected void SendTrace() {
            Label = "SendTrace";
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
            Return();
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
            // Assembler.GetIdentifier

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
                Call("SendTrace");

                EAX = Memory["DebugTraceMode"];
                AL.Compare(4);
                JumpIf(Flags.NotEqual, "DebugPoint_NoTrace");
                Call("DebugPoint_WaitCmd");
                Jump("DebugPoint_ProcessCmd");
            Label = "DebugPoint_NoTrace";

            // Is there a new incoming command?
            Label = "DebugPoint_CheckCmd";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);

            //separate command structure, when in break Wait for unbreak only but process others
            JumpIf(Flags.Zero, "DebugStub_Exit");
            Jump("DebugPoint_ProcessCmd");
            Label = "DebugStub_Exit";

            PopAll32();
            Return();

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
            Jump("DebugStub_Exit");
        }
    }
}
