using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class DebugStub : X.Y86 {
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;

        protected void ProcessCmd() {
            Label = "DebugPoint_ProcessCmd";
            DX = mComAddr;
            AL = Port[DX];
            AL.Compare(1);
            JumpIf(Flags.Equal, "DebugStub_TraceOff");
            AL.Compare(2);
            JumpIf(Flags.Equal, "DebugStub_TraceOn");
            AL.Compare(3);
            JumpIf(Flags.Equal, "DebugStub_Step");
            AL.Compare(4);
            JumpIf(Flags.Equal, "DebugStub_Break");
            Return();

            Label = "DebugStub_TraceOff";
            Memory["DebugTraceMode", 32] = 0;
            Return();

            Label = "DebugStub_TraceOn";
            Memory["DebugTraceMode", 32] = 1;
            Return();

            Label = "DebugStub_Step";
            Memory["DebugTraceMode", 32] = 4;
            Return();

            Label = "DebugStub_Break";
            Memory["DebugTraceMode", 32] = 4;
            Call("DebugPoint_WaitCmd");
            Jump("DebugPoint_ProcessCmd");
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

        protected void Emit() {
            ProcessCmd();
            SendTrace();
            WriteByteToDebugger();
            WaitCmd();
            DebugSuspend();
            DebugResume();
        }

        public void Main(UInt16 aComAddr) {
            mComAddr = aComAddr;
            mComStatusAddr = (UInt16)(aComAddr + 5);
            Emit();
            // For Unique Labels
            //  Assembler.GetIdentifier
            // For System..Break
            //  public class BreakAssembler: AssemblerMethod

            //"DebugTraceMode dd 1");
            //"DebugStatus dd 0");
            //"DebugSuspendLevel dd 0");

            Label = "DebugPoint__";
            PushAll32();
            EBP = ESP;
            EBP.Add(32);

            // Check DebugTraceMode
            EAX = Memory["DebugTraceMode"];
            //TODO: Change this to support CallIf(AX == 0, "SendTrace");
            AL.Compare(0);
            CallIf(Flags.NotEqual, "SendTrace");

            //EAX = Memory["DebugTraceMode"];
            //AL.Compare(4);
            //JumpIf(Flags.NotEqual, "DebugPoint_NoBreak");
            //    Call("DebugStub_Break");
            //    Jump("DebugStub_Exit");
            //Label = "DebugPoint_NoBreak";

            // Is there a new incoming command?
            Label = "DebugPoint_CheckCmd";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            CallIf(Flags.NotZero, "DebugPoint_ProcessCmd");

            Label = "DebugStub_Exit";
            PopAll32();
            Return();
        }
    }
}
