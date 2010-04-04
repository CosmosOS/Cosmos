using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cosmos.Compiler.Debug; 
//TODO: The asm code here is not efficient. Our first priority is to make it functionally robust and working
// We will later optimize this
using Cosmos.IL2CPU;
namespace Cosmos.IL2CPU.X86
{
    public class DebugStub : X.Y86 {
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;
        protected enum Tracing { Off = 0, On = 1 };
        // Current status of OS Debug Stub
        public enum Status { Run = 0, Break = 1 }
        
        // A bit of a hack as a static? Other ideas?
        public static void EmitDataSection() {
            Assembler.CurrentInstance.DataMembers.AddRange(new DataMember[]{
                // Tracing: 0=Off, 1=On
                new DataMember("DebugTraceMode", 0),
                // enum Status
                new DataMember("DebugStatus", 0),
                    
                // 0 = Not in, 1 = already running
                new DataMember("DebugRunning", 0),
                // Nesting control for non steppable routines
                new DataMember("DebugSuspendLevel", 0),
                // Nesting control for non steppable routines 
                new DataMember("DebugResumeLevel", 0),
                // Last EIP value
                new DataMember("DebugEIP", 0),
                new DataMember("InterruptsEnabledFlag", 0),
                // If set to 1, on next trace a break will occur
                new DataMember("DebugBreakOnNextTrace", 0),
                // breakpoint address
                new DataMember("DebugBreakpointAddress", 0)
             });
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

        protected void BreakOnAddress() {
            Label = "DebugStub_BreakOnAddress";
            PushAll32();

            new Push { DestinationValue = 0 };
            EDI = ESP;
            //Label = "DebugStub_BreakOnAddress2";
            //Jump("DebugStub_BreakOnAddress2");
            //new Sub { DestinationReg = RegistersEnum.EDI, SourceValue = 3 };

            // Read address to stack
            Call("ReadByteFromComPort");
            Call("ReadByteFromComPort");
            Call("ReadByteFromComPort");
            Call("ReadByteFromComPort");

            // Pop of 4 bytes read from port to stack as EAX
            new Pop { DestinationReg = RegistersEnum.EAX };
            // Set it to our breakpoint address
            Memory["DebugBreakpointAddress", 32] = EAX;

            PopAll32();
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
            new Add { DestinationReg = Registers.ESI, SourceValue = 12 };
            ECX = Memory[ESI];
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
        
            // Address of string
            ESI = Memory[EBP + 8];
            Label = "DebugStub_SendTextWriteChar";
            ECX.Compare(0);
                JumpIf(Flags.Equal, "DebugStub_SendTextExit");
            Call("WriteByteToComPort");
            new Dec { DestinationReg = Registers.ECX };
            // We are storing as 16 bits, but for now I will transmit 8 bits
            // So we inc again to skip the 0
            new Inc { DestinationReg = Registers.ESI };
            Jump("DebugStub_SendTextWriteChar");
   
            Label = "DebugStub_SendTextExit";
            Return();
        }

        
        // Input: Stack
        // Output: None
        // Modifies: EAX, ECX, EDX, ESI
        protected void SendPtr()
        {
            Label = "DebugStub_SendPtr";

            // Write the type
            AL = (int)MsgType.Pointer;
            Call("WriteALToComPort");

            // pointer value
            ESI = Memory[EBP + 8];

            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            
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

            new Inc { DestinationReg = Registers.ESI };
            Return();
        }

        // input: EDI
        // Modified: EAX, EDX, EDI (-1)
        //
        // Reads a byte into [EDI] and does EDI - 1
        protected void ReadByteFromComPort()
        {
            Label = "ReadByteFromComPort";
            DX = mComStatusAddr;
            
            // wait for port to be ready
            Label = "ReadByteFromComPort_Wait";
            AL = Port[DX];
            AL.Test(1);
            /**/JumpIf(Flags.Zero, "ReadByteFromComPort_Wait");

            // set address of port
            DX = mComAddr;
            // read byte
            AL = 0;
            AL = Port[DX];
            Memory[EDI, 8] = AL;
            new Inc { DestinationReg = Registers.EDI };
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
            SendPtr();
            WriteALToComPort();
            WriteByteToComPort();
            ReadByteFromComPort();

            DebugSuspend();
            DebugResume();
            Break();
            BreakOnAddress();
        }

        protected void Executing() {
            Label = "DebugStub_Executing";

            new Compare{ DestinationRef = ElementReference.New("DebugBreakpointAddress"), DestinationIsIndirect = true,
                SourceValue = 0, Size = 32 };
            JumpIf(Flags.Equal, "DebugStub_Executing_AfterBreakOnAddress");
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBP, SourceIsIndirect=true };
            new Sub { DestinationReg = Registers.EAX, SourceValue = 5 };
            
            // if there's a pending BreakOnAddress, compare it to the original
            new Compare
            {
                DestinationRef = ElementReference.New("DebugBreakpointAddress"), DestinationIsIndirect=true,
                SourceReg = Registers.EAX};
                JumpIf(Flags.Equal, "DebugStub_Break");

                Label = "DebugStub_Executing_AfterBreakOnAddress";

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

            Label = "DebugStub_Executing_CommandComingIn";
            // Process command
            DX = mComAddr;
            AL = Port[DX];
            AL.Compare((byte)Command.TraceOff);
                JumpIf(Flags.Equal, "DebugStub_TraceOff");
            AL.Compare((byte)Command.TraceOn);
                JumpIf(Flags.Equal, "DebugStub_TraceOn");
            AL.Compare((byte)Command.Break);
                JumpIf(Flags.Equal, "DebugStub_Break");
            AL.Compare((byte)Command.BreakOnAddress);
                JumpIf(Flags.Equal, "DebugStub_BreakOnAddress");

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
