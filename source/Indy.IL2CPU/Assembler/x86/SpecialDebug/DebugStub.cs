using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;

//TODO: The asm code here is not efficient. Our first priority is to make it functionally robust and working
// We will later optimize this
namespace Indy.IL2CPU.Assembler.X86 {
    public class DebugStub : X.Y86 {
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;
        protected enum Tracing { Off = 0, On = 1 };
        // Current status of OS Debug Stub
        public enum Status { Run = 0, Break = 1 }
        
        // A bit of a hack as a static? Other ideas?
        public static void EmitDataSection(System.IO.TextWriter aWriter) {
             // Tracing: 0=Off, 1=On
            aWriter.WriteLine("DebugTraceMode dd 0");
            // enum Status
            aWriter.WriteLine("DebugStatus dd 0"); 
            // 0 = Not in, 1 = already running
            aWriter.WriteLine("DebugRunning dd 0"); 
            // Nesting control for non steppable routines
            aWriter.WriteLine("DebugSuspendLevel dd 0");
            // Nesting control for non steppable routines 
            aWriter.WriteLine("DebugResumeLevel dd 0"); 
            // Last EIP value
            aWriter.WriteLine("DebugEIP dd 0"); 
            aWriter.WriteLine("InterruptsEnabledFlag dd 0");
            // If set to 1, on next trace a break will occur
            aWriter.WriteLine("DebugBreakOnNextTrace dd 0");
        }

        protected void Commands() {
            Label = "DebugStub_TraceOff";
            Memory["DebugTraceMode", 32] = (int)Tracing.Off;
            Return();

            Label = "DebugStub_TraceOn";
            Memory["DebugTraceMode", 32] = (int)Tracing.On;
            Return();

            Label = "DebugStub_Step";
            Memory["DebugBreakOnNextTrace", 32] = 1;
            Return();
        }

        protected void Break() {
            // Should only be called internally by DebugStub. Has a lot of preconditions
            // Externals should use BreakOnNextTrace instead
            Label = "DebugStub_Break";
            // Reset request in case there was one
            Memory["DebugBreakOnNextTrace", 32] = 0;
            // Set break status
            Memory["DebugStatus", 32] = (int)Status.Break;
            Call("DebugStub_SendTrace");

            // Wait for a command
            DX = mComStatusAddr;
            Label = "DebugStub_WaitCmd";
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "DebugStub_WaitCmd");

            // Read command from port
            DX = mComAddr;
            AL = Port[DX];

            AL.Compare((byte)Command.TraceOff);
                CallIf(Flags.Equal, "DebugStub_TraceOff", "DebugStub_WaitCmd");
            AL.Compare((byte)Command.TraceOn);
                CallIf(Flags.Equal, "DebugStub_TraceOn", "DebugStub_WaitCmd");
            AL.Compare((byte)Command.Break);
                // Break command is also the continue command
                // If received while in break, then it continues
                JumpIf(Flags.Equal, "DebugStub_Break_Exit");
            AL.Compare((byte)Command.Step);
                CallIf(Flags.Equal, "DebugStub_Step", "DebugStub_Break_Exit");

            Label = "DebugStub_Break_Exit";
            Memory["DebugStatus", 32] = (int)Status.Run;
            Return();
        }

        // Modifies: EAX
        protected void SendTrace() {
            Label = "DebugStub_SendTrace";

            Memory["DebugStatus", 32].Compare((int)Status.Run);
            JumpIf(Flags.Equal, "DebugStub_SendTrace_Normal");
            AL = (int)MsgType.BreakPoint;
            Jump("DebugStub_SendTraceType");
            
            Label = "DebugStub_SendTrace_Normal";
            AL = (int)MsgType.TracePoint;

            Label = "DebugStub_SendTraceType";
            Call("WriteALToComPort");
                        
            // Send EIP. EBP points to location with EIP
            ESI = EBP;
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");

            Return();
        }

        // Input: Stack
        // Output: None
        // Modifies: EAX, ECX, EDX, ESI
        protected void SendText() {
            Label = "DebugStub_SendText";

            // Write the type
            AL = (int)MsgType.Message;
            Call("WriteALToComPort");
            
            // Write Length
            ESI = EBP;
            new Add("ESI", 12);
            ECX = Memory[ESI];
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
        
            // Address of string
            ESI = Memory[EBP + 8];
            Label = "DebugStub_SendTextWriteChar";
            ECX.Compare(0);
                JumpIf(Flags.Equal, "DebugStub_SendTextExit");
            Call("WriteByteToComPort");
            new X86.Dec("ECX");
            // We are storing as 16 bits, but for now I will transmit 8 bits
            // So we inc again to skip the 0
            new X86.Inc("ESI");
            Jump("DebugStub_SendTextWriteChar");
   
            Label = "DebugStub_SendTextExit";
            Return();
        }

        // Input AL
        // Output: None
        // Modifies: EAX, EDX, ESI
        protected void WriteALToComPort() {
            Label = "WriteALToComPort";
            //TODO: Make a data point to put this in instead of using stack
            EAX.Push();
            ESI = ESP;
            Call("WriteByteToComPort");
            EAX.Pop(); // Is a local, cant use Return(4)
            Return();
        }
        
        // Input: ESI
        // Output: None
        // Modifies: EAX, EDX
        //
        // Sends byte at [ESI] to com port and does esi + 1
        protected void WriteByteToComPort() {
            // This sucks to use the stack, but x86 can only read and write ports from AL and
            // we need to read a port before we can write out the value to another port.
            // The overhead is a lot, but compared to the speed of the serial and the fact
            // that we wait on the serial port anyways, its a wash.
            //
            // This could be changed to use interrupts, but that then copmlicates
            // the code and causes interaction with other code. DebugStub should be
            // as isolated as possible from any other code.
            Label = "WriteByteToComPort";
            // Sucks again to use DX just for this, but x86 only supports
            // 8 bit address for literals on ports
            DX = mComStatusAddr;
            
            // Wait for serial port to be ready
            Label = "WriteByteToComPort_Wait";
            AL = Port[DX];
            AL.Test(0x20);
                JumpIf(Flags.Zero, "WriteByteToComPort_Wait");
            
            // Set address of port
            DX = mComAddr;
            // Get byte to send
            AL = Memory[ESI];
            // Send the byte
            Port[DX] = AL;
            
            new Inc("ESI");
            Return();
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
            SendText();
            WriteALToComPort();
            WriteByteToComPort();
            DebugSuspend();
            DebugResume();
            Break();
        }

        protected void Executing() {
            Label = "DebugStub_Executing";
            
            // See if there is a requested break
            //TODO: Change this to support CallIf(AL == 1, "DebugStub_SendTrace");
            Memory["DebugBreakOnNextTrace", 32].Compare(1);
                CallIf(Flags.Equal, "DebugStub_Break");
            
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

            // Main entry point that IL2CPU generated code calls
            Label = "DebugStub_TracerEntry";

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
