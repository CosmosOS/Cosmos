using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class DebugStub : X.Y86 {
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;
        protected enum Tracing { Off = 0, On = 1 };
        protected enum Command { TraceOff = 1, TraceOn = 2, Break = 3 }

        protected void Commands() {
            Label = "DebugStub_TraceOff";
            Memory["DebugTraceMode", 32] = (int)Tracing.Off;
            Return();

            Label = "DebugStub_TraceOn";
            Memory["DebugTraceMode", 32] = (int)Tracing.On;
            Return();
        }

        protected void Break() {
            Label = "DebugStub_Break";
            //Memory["DebugStatus", 32] = 1;
            Label = "DebugStub_Break_WaitCmd";
            Call("DebugPoint_WaitCmd");
            DX = mComAddr;
            AL = Port[DX];
            AL.Compare((byte)Command.TraceOff);
                CallIf(Flags.Equal, "DebugStub_TraceOff");
                JumpIf(Flags.Equal, "DebugStub_Break_WaitCmd");
            AL.Compare((byte)Command.TraceOn);
                CallIf(Flags.Equal, "DebugStub_TraceOn");
                JumpIf(Flags.Equal, "DebugStub_Break_WaitCmd");
            //AL.Compare((byte)Command.Break);
            // Break command is also the continue command, so we just ignore comparison
            // Actually any unrecognized command would continue, but Break is the proper one
                
            Return();
        }

        protected void SendTrace() {
            Label = "DebugStub_SendTrace";
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
            DX = mComStatusAddr;
            Label = "DebugPoint_WaitCmd";
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
            Commands();
            Executing();
            SendTrace();
            WriteByteToDebugger();
            WaitCmd();
            DebugSuspend();
            DebugResume();
            Break();
        }

        protected void Executing() {
            Label = "DebugStub_Executing";
            EAX = Memory["DebugTraceMode"];
            //TODO: Change this to support CallIf(AX == 1, "DebugStub_SendTrace");
            AL.Compare((int)Tracing.On);
            CallIf(Flags.Equal, "DebugStub_SendTrace");

            // Is there a new incoming command?
            Label = "DebugPoint_CheckCmd";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugStub_Executing_Exit");

            DX = mComAddr;
            AL = Port[DX];
            AL.Compare((byte)Command.TraceOff);
                CallIf(Flags.Equal, "DebugStub_TraceOff");
            AL.Compare((byte)Command.TraceOn);
                CallIf(Flags.Equal, "DebugStub_TraceOn");
            AL.Compare((byte)Command.Break);
                CallIf(Flags.Equal, "DebugStub_Break");

            Label = "DebugStub_Executing_Exit";
            Return();
        }

        public void Main(UInt16 aComAddr) {
            mComAddr = aComAddr;
            mComStatusAddr = (UInt16)(aComAddr + 5);
            Emit();
            // For Unique Labels
            //  Assembler.GetIdentifier
            // For System..Break
            //  public class BreakAssembler: AssemblerMethod

            //"DebugStatus dd 0");
            //"DebugSuspendLevel dd 0");

            Label = "DebugPoint__";
            PushAll32();
            EBP = ESP;
            EBP.Add(32);

            //if tracemode = 4
            //   SendTrace
            //   Wait for some command that continues
            //else
            Call("DebugStub_Executing");
            
            //EAX = Memory["DebugTraceMode"];
            //AL.Compare(4);
            //JumpIf(Flags.NotEqual, "DebugPoint_NoBreak");
            //    Call("DebugStub_Break");
            //    Jump("DebugStub_Exit");
            //Label = "DebugPoint_NoBreak";

            Label = "DebugStub_Exit";
            PopAll32();
            Return();
        }
    }
}
