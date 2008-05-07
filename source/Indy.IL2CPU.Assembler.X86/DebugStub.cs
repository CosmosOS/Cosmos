using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class DebugStub : X.Y86 {
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;
        protected enum Tracing { Off = 0, On = 1 };
        protected enum Command { TraceOff = 1, TraceOn = 2, Break = 3, Step = 4 }
        protected enum Status { Run = 0, Break = 1, Stepping = 2 }

        protected void Commands() {
            Label = "DebugStub_TraceOff";
            Memory["DebugTraceMode", 32] = (int)Tracing.Off;
            Return();

            Label = "DebugStub_TraceOn";
            Memory["DebugTraceMode", 32] = (int)Tracing.On;
            Return();

            Label = "DebugStub_Continue";
            Memory["DebugStatus", 32] = (int)Status.Run;
            Return();

            Label = "DebugStub_Step";
            Memory["DebugStatus", 32] = (int)Status.Stepping;
            Return();
        }

        protected void Break() {
            Label = "DebugStub_Break";
            Memory["DebugStatus", 32] = (int)Status.Break;
            Call("DebugStub_SendTrace");

            DX = mComStatusAddr;
            Label = "DebugStub_WaitCmd";
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugStub_WaitCmd");

            DX = mComAddr;
            AL = Port[DX];

            //TODO: AL could also change... need to jump and not compare further
            AL.Compare((byte)Command.TraceOff);
                CallIf(Flags.Equal, "DebugStub_TraceOff", "DebugStub_WaitCmd");
            AL.Compare((byte)Command.TraceOn);
                CallIf(Flags.Equal, "DebugStub_TraceOn", "DebugStub_WaitCmd");
            AL.Compare((byte)Command.Break);
                // Break command is also the continue command
                CallIf(Flags.Equal, "DebugStub_Continue", "DebugStub_Break_Exit");
            AL.Compare((byte)Command.Step);
                CallIf(Flags.Equal, "DebugStub_Step", "DebugStub_Break_Exit");

            Label = "DebugStub_Break_Exit";
            Return();
        }

        protected void SendTrace() {
            Label = "DebugStub_SendTrace";

            Memory["DebugTraceSent", 32].Compare(1);
            JumpIf(Flags.Equal, "DebugStub_SendTrace_Exit");
                Memory["DebugTraceSent", 32] = 1;

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
            Label = "DebugStub_SendTrace_Exit";
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

        protected void DebugSuspend() {
            Label = "DebugPoint_DebugSuspend";
            Memory["DebugSuspendLevel", 32]++;
            Return();
        }

        protected void DebugResume() {
            Label = "DebugPoint_DebugResume";
            Memory["DebugSuspendLevel", 32]--;
            Return();
        }

        protected void Emit() {
            Commands();
            Executing();
            SendTrace();
            WriteByteToDebugger();
            DebugSuspend();
            DebugResume();
            Break();
        }

        protected void Executing() {
            Label = "DebugStub_Executing";
            //TODO: Change this to support CallIf(AL == 1, "DebugStub_SendTrace");
            Memory["DebugTraceMode", 32].Compare((int)Tracing.On);
                CallIf(Flags.Equal, "DebugStub_SendTrace");

            // Is there a new incoming command?
            Label = "DebugStub_Executing_Normal";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugStub_Executing_Exit");

            // Process command
            DX = mComAddr;
            AL = Port[DX];
            AL.Compare((byte)Command.TraceOff);
                JumpIf(Flags.Equal, "DebugStub_TraceOff");
            AL.Compare((byte)Command.TraceOn);
                JumpIf(Flags.Equal, "DebugStub_TraceOn");
            AL.Compare((byte)Command.Break);
                JumpIf(Flags.Equal, "DebugStub_Break");

            Label = "DebugStub_Executing_Exit";
            Return();
        }

        public void Main(UInt16 aComAddr) {
            mComAddr = aComAddr;
            mComStatusAddr = (UInt16)(aComAddr + 5);
            Emit();
            // For System..Break
            //  public class BreakAssembler: AssemblerMethod

            Label = "DebugPoint__";

            // If debug stub is in break, and then an IRQ happens, the IRQ
            // can call debug stub again. This causes two debug stubs to 
            // run which causes havoc. So we only allow one to run.
            // We arent multi threaded yet, so this works fine.
            // IRQ's are disabled between Compare and JumpIf so an IRQ cant
            // happen in between them which could then cause double entry again
            DisableInterrupts();

            Memory["DebugSuspendLevel", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_Running");
                Memory["InterruptsEnabledFlag", 32].Compare(0);
                JumpIf(Flags.Equal, "DebugStub_Return");
                EnableInterrupts();
                Jump("DebugStub_Return");

            Label = "DebugStub_Running";
            Memory["DebugRunning", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_Start");
                Memory["InterruptsEnabledFlag", 32].Compare(0);
                JumpIf(Flags.Equal, "DebugStub_Return");
                EnableInterrupts();
                Jump("DebugStub_Return");

            Label = "DebugStub_Start";
            Memory["DebugRunning", 32] = 1;
            Memory["InterruptsEnabledFlag", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_NoSTI");
            EnableInterrupts();
            Label = "DebugStub_NoSTI";
            //
            PushAll32();
            EBP = ESP;
            EBP.Add(32);
            //
            EAX = Memory[EBP];
            Memory["DebugEIP"] = EAX;
            //
            Memory["DebugTraceSent", 32] = 0;

            //if tracemode = 4
            //   SendTrace
            //   Wait for some command that continues
            //else
            Call("DebugStub_Executing");
            
            PopAll32();
            Memory["DebugRunning", 32] = 0;
            Label = "DebugStub_Return";
            Return();
        }
    }
}
