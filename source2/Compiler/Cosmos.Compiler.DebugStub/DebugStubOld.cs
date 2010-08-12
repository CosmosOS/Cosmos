using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Debug;
using Cosmos.Compiler.XSharp;

//TODO: The asm code here is not efficient. Our first priority is to make it functionally robust and working
// Later we can optimize it.
namespace Cosmos.Compiler.DebugStub {
    public class DebugStubOld : CodeBlock {
        public override void Assemble() {
        }

        //TODO: Move com port init to debugstub asm
        protected UInt16 mComAddr;
        protected UInt16 mComStatusAddr;
        protected enum Tracing { Off = 0, On = 1 };
        // Current status of OS Debug Stub
        public enum Status { Run = 0, Break = 1 }

        // A bit of a hack as a static? Other ideas?
        public static void EmitDataSection() {
            Assembler.Assembler.CurrentInstance.DataMembers.AddRange(new DataMember[]{
                // 0 on start, set to 1 after Started signal is sent.
                new DataMember("DebugStartedSent", 0),

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
                new DataMember("DebugBreakOnNextTrace", 0)
                // Command ID of last command received
                , new DataMember("DebugStub_CommandID", 0)
                // Breakpoint addresses
                , new DataMember("DebugBPs", new int[256])
             });
        }

        // INLINE
        // Modifies: Stack, EDI, AL
        // TODO: Modify X# to allow inlining better by using dynamic labels otherwise
        // repeated use of an inline will fail with conflicting labels.
        // TODO: Allow methods to emit a start label and return automatically
        // and mark inlines so this does not happen.
        protected void ReadComPortX32toStack() {
            // Make room on the stack for the address
            Push(0);
            // ReadByteFromComPort writes to EDI, then increments
            EDI = ESP;

            // Read address to stack via EDI
            Call("ReadByteFromComPort");
            Call("ReadByteFromComPort");
            Call("ReadByteFromComPort");
            Call("ReadByteFromComPort");
        }

        // Sets a breakpoint
        // Serial Params:
        //   1: x32 - EIP to break on, or 0 to disable breakpoint.
        protected void BreakOnAddress() {
            Label = "DebugStub_BreakOnAddress";
            PushAll32();

            // BP Address
            ReadComPortX32toStack();
            ECX.Pop();

            // BP ID Number
            // BP ID Number is sent after BP Address, becuase
            // reading BP address uses AL (EAX).
            EAX = 0;
            Call("ReadALFromComPort");

            // Calculate location in table
            // Mov [EBX + EAX * 4], ECX would be better, but our asm doesn't handle this yet
            EBX = AddressOf("DebugBPs");
            EAX = EAX << 2;
            EBX.Add(EAX);

            Memory[EBX, 32] = ECX;

            PopAll32();
            Return();
        }

        protected void Break() {
            // Should only be called internally by DebugStub. Has a lot of preconditions
            // Externals should use BreakOnNextTrace instead
            Label = "DebugStub_Break";
            // Reset request in case we are currently responding to one
            Memory["DebugBreakOnNextTrace", 32] = 0;
            // Set break status
            Memory["DebugStatus", 32] = (int)Status.Break;
            Call("DebugStub_SendTrace");

            // Wait for a command
            Label = "DebugStub_WaitCmd";
            // Check for common commands
            Call("DebugStub_ProcessCommand");

            // Now check for commands that are only valid in break state
            // or commands that require additional handling while in break
            // state.

            AL.Compare((byte)Command.Continue);
            JumpIf(Flags.Equal, "DebugStub_Break_Exit");

            AL.Compare((byte)Command.Step);
            JumpIf(Flags.NotEqual, "DebugStub_Break_Step_After");
            Memory["DebugBreakOnNextTrace", 32] = 1;
            Jump("DebugStub_Break_Exit");

            Label = "DebugStub_Break_Step_After";
            // Loop around and wait for another command
            Jump("DebugStub_WaitCmd");

            Label = "DebugStub_Break_Exit";
            Memory["DebugStatus", 32] = (int)Status.Run;
            Return();
        }

        // Modifies: EAX, ESI
        protected void SendTrace() {
            Label = "DebugStub_SendTrace";

            Memory["DebugStatus", 32].Compare((int)Status.Run);
            JumpIf(Flags.Equal, "DebugStub_SendTrace_Normal");
            AL = (int)MsgType.BreakPoint;
            Jump("DebugStub_SendTraceType");

            Label = "DebugStub_SendTrace_Normal";
            AL = (int)MsgType.TracePoint;

            Label = "DebugStub_SendTraceType";
            DebugStub.WriteALToComPort.Call();

            // Send Calling EIP.
            ESI = AddressOf("DebugEIP");
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
            DebugStub.WriteALToComPort.Call();

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
        protected void SendPtr() {
            Label = "DebugStub_SendPtr";

            // Write the type
            AL = (int)MsgType.Pointer;
            DebugStub.WriteALToComPort.Call();

            // pointer value
            ESI = Memory[EBP + 8];

            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");
            Call("WriteByteToComPort");

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

        //Modifies: AL, DX
        protected void ReadALFromComPort() {
            Label = "ReadALFromComPort";
            DX = mComStatusAddr;

            // Wait for port to be ready
            Label = "ReadALFromComPort_Wait";
            AL = Port[DX];
            AL.Test(0x01);
            JumpIf(Flags.Zero, "ReadALFromComPort_Wait");

            Label = "ReadALFromComPortNoCheck";
            // Set address of port
            DX = mComAddr;
            // Read byte
            AL = Port[DX];
            Return();
        }

        // Input: EDI
        // Output: [EDI]
        // Modified: AL, DX, EDI (+1)
        //
        // Reads a byte into [EDI] and does EDI + 1
        // http://wiki.osdev.org/Serial_ports
        // We dont worry about byte over writing because:
        //  -The UART will handle flow control for us - except its not turned on right now.... :)
        //  -All modern UARTs have at least a 16 byte buffer.
        protected void ReadByteFromComPort() {
            Label = "ReadByteFromComPort";
            Call("ReadALFromComPort");
            Memory[EDI, 8] = AL;
            EDI++;
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

        // This does not run during Cosmos execution
        // This is only used by the compiler to force emission of each of our routines.
        // Each routine must be listed here, else it wont be emitted.
        protected void Emit() {
            Executing();
            SendTrace();
            SendText();
            SendPtr();
            WriteByteToComPort();
            ReadByteFromComPort();
            ReadALFromComPort();

            DebugSuspend();
            DebugResume();
            Break();
            BreakOnAddress();
            ProcessCommand();
            ProcessCommandBatch();
            WaitForSignature();
        }

        // This is the secondary stub routine. After the primary (main) has decided we should do some debug
        // activities, this one is called.
        protected void Executing() {
            Label = "DebugStub_Executing";

            // The very first time, we send a one time Started signal back to the host
            Memory["DebugStartedSent", 32].Compare(1);
            JumpIf(Flags.Equal, "DebugStub_AfterStarted");
            Memory["DebugStartedSent", 32] = 1; // Set flag so we don't send Ready again

                // "Clear" the UART out
                AL = 0;
                DebugStub.WriteALToComPort.Call();

                // QEMU has junk in the buffer when it first
                // boots. VMWare doesn't...
                // So we use this to "clear" it by doing 16
                // reads. UART buffers are 16 bytes and 
                // usually there are only a few junk bytes.
                //for (int i = 1; i <= 16; i++) {
                //    Call("ReadALFromComPortNoCheck");
                //}

                // QEMU (and possibly others) send some garbage across the serial line first.
                // Actually they send the garbage in bound, but garbage could be inbound as well so we 
                // keep this.
                // To work around this we send a signature. DC then discards everything before the signature.
                Push(Consts.SerialSignature);
                ESI = ESP;
                Call("WriteByteToComPort");
                Call("WriteByteToComPort");
                Call("WriteByteToComPort");
                Call("WriteByteToComPort");
                // Restore ESP, we actually dont care about EAX or the value on the stack anymore.
                EAX.Pop();

                // We could use the signature as the start signal, but I prefer
                // to keep the logic separate, especially in DC.
                AL = (int)MsgType.Started; // Send the actual started signal
                DebugStub.WriteALToComPort.Call();

                Call("DebugStub_WaitForSignature");
                Call("DebugStub_ProcessCommandBatch");
            Label = "DebugStub_AfterStarted";

            // Look for a possible matching BP
            EAX = Memory["DebugEIP", 32];
            EDI = AddressOf("DebugBPs");
            ECX = 256;
            new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 };
            JumpIf(Flags.Equal, "DebugStub_Break");

            Label = "DebugStub_Executing_AfterBreakOnAddress";
            // See if there is a requested break
            Memory["DebugBreakOnNextTrace", 32].Compare(1);
            CallIf(Flags.Equal, "DebugStub_Break");

            //TODO: Change this to support CallIf(AL == 1, "DebugStub_SendTrace");
            Memory["DebugTraceMode", 32].Compare((int)Tracing.On);
            CallIf(Flags.Equal, "DebugStub_SendTrace");

            Label = "DebugStub_Executing_Normal";
            // Is there a new incoming command? We dont want to wait for one
            // if there isn't one already here. This is a passing check.
            Label = "DebugStub_CheckForCmd";
            DX = mComStatusAddr;
            AL = Port[DX];
            AL.Test(0x01);
            // If no command waiting, break from loop
            JumpIf(Flags.Zero, "DebugStub_CheckForCmd_Break");
            Call("DebugStub_ProcessCommand");
            // See if there are more commands waiting
            Jump("DebugStub_CheckForCmd");
            Label = "DebugStub_CheckForCmd_Break";

            Return();
        }

        public void WaitForSignature() {
            Label = "DebugStub_WaitForSignature";
            EBX = 0;

            Label = "DebugStub_WaitForSignature_Read";
            Call("ReadALFromComPort");
            BL = AL;
            EBX.RotateRight(8);
            EBX.Compare(Consts.SerialSignature);
            JumpIf(Flags.NotEqual, "DebugStub_WaitForSignature_Read");

            Label = "DebugStub_WaitForSignature_Exit";
            Return();
        }

        public void ProcessCommandBatch() {
            Label = "DebugStub_ProcessCommandBatch";
            Call("DebugStub_ProcessCommand");

            // See if batch is complete
            AL.Compare((byte)Command.BatchEnd);
            JumpIf(Flags.Equal, "DebugStub_ProcessCommandBatch_Exit");

            // Loop and wait
            Jump("DebugStub_ProcessCommandBatch");

            Label = "DebugStub_ProcessCommandBatch_Exit";
            Return();
        }

        // Modifies: AL, DX (ReadALFromComPort)
        // Returns: AL
        public void ProcessCommand() {
            Label = "DebugStub_ProcessCommand";
            Call("ReadALFromComPort");
            // Some callers expect AL to be returned, so we preserve it
            // in case any commands modify AL.
            //TODO: But in ASM wont let us push AL, so we push EAX for now
            EAX.Push();

            // Noop has no data at all (see notes in client DebugConnector), so skip Command ID
            AL.Compare((byte)Command.Noop);
            JumpIf(Flags.Equal, "DebugStub_ProcessCmd_Exit");

            // Read Command ID
            Call("ReadALFromComPort");
            Memory["DebugStub_CommandID", 32] = EAX;

            // Get AL back so we can compare it, but also put it back for later
            EAX.Pop();
            EAX.Push();

            AL.Compare((byte)Command.TraceOff);
            JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_TraceOff_After");
            Memory["DebugTraceMode", 32] = (int)Tracing.Off;
            Jump("DebugStub_ProcessCmd_ACK");
            Label = "DebugStub_ProcessCmd_TraceOff_After";

            AL.Compare((byte)Command.TraceOn);
            JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_TraceOn_After");
            Memory["DebugTraceMode", 32] = (int)Tracing.On;
            Jump("DebugStub_ProcessCmd_ACK");
            Label = "DebugStub_ProcessCmd_TraceOn_After";

            AL.Compare((byte)Command.Break);
            JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_Break_After");
            Call("DebugStub_Break");
            Jump("DebugStub_ProcessCmd_ACK");
            Label = "DebugStub_ProcessCmd_Break_After";

            AL.Compare((byte)Command.BreakOnAddress);
            JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_BreakOnAddress_After");
            Call("DebugStub_BreakOnAddress");
            Jump("DebugStub_ProcessCmd_ACK");
            Label = "DebugStub_ProcessCmd_BreakOnAddress_After";

            Label = "DebugStub_ProcessCmd_ACK";
                // We acknowledge receipt of the command, not processing of it.
                // We have to do this because sometimes callers do more processing
                // We ACK even ones we dont process here, but do not ACK Noop.
                // The buffers should be ok becuase more wont be sent till after our NACK
                // is received.
                // Right now our max cmd size is 2 + 5 = 7. UART buffer is 16.
                // We may need to revisit this in the future to ack not commands, but data chunks
                // and move them to a buffer.
                AL = (int)MsgType.CmdCompleted;
                DebugStub.WriteALToComPort.Call();
                EAX = Memory["DebugStub_CommandID", 32];
                DebugStub.WriteALToComPort.Call();
            Label = "DebugStub_ProcessCmd_After";

            Label = "DebugStub_ProcessCmd_Exit";
            // Restore AL for callers who check the command and do
            // further processing, or for commands not handled by this routine.
            EAX.Pop();
            Return();
        }

        // This is the main debug stub routine. The parameter is used to generate it and 
        // the code is embedded.
        // This routine is called repeatedly by Cosmos code and it checks various flags
        // to decide the state and what to do.
        public void Main(UInt16 aComAddr) {
            mComAddr = aComAddr;
            mComStatusAddr = (UInt16)(aComAddr + 5);
            EmitDataSection();
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
            // DebugStub is already running, so exit.
            // But we need to see if IRQs are diabled.
            // If IRQ disabled, we dont reenable them after our disable
            // in this routine.
            Memory["InterruptsEnabledFlag", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_Return");
            EnableInterrupts();
            Jump("DebugStub_Return");

            Label = "DebugStub_Running";
            Memory["DebugRunning", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_Start");
            // DebugStub is already running, so exit.
            // But we need to see if IRQs are diabled.
            // If IRQ disabled, we dont reenable them after our disable
            // in this routine.
            Memory["InterruptsEnabledFlag", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_Return");
            EnableInterrupts();
            Jump("DebugStub_Return");

            // All clear, mark that we are entering the debug stub
            Label = "DebugStub_Start";
            Memory["DebugRunning", 32] = 1;
            Memory["InterruptsEnabledFlag", 32].Compare(0);
            JumpIf(Flags.Equal, "DebugStub_NoSTI");
            EnableInterrupts();

            // IRQ reenabled, call secondary debug stub
            Label = "DebugStub_NoSTI";
            PushAll32();
            // We just pushed all registers to the stack so we can use them
            // So we get the stack pointer and add 32. This skips over the
            // registers we just pushed.
            EBP = ESP;
            EBP.Add(32);
            // Get actual EIP of caller.
            EAX = Memory[EBP];
            // EIP is pointer to op after our call. We subtract 5 (the size of our call + address)
            // so we get the EIP as IL2CPU records it. Its also useful for when we will
            // be changing ops that call this stub.
            EAX.Sub(5);
            // Store it for later use.
            Memory["DebugEIP", 32] = EAX;

            // Call secondary stub
            Call("DebugStub_Executing");
            PopAll32();
            // Complete, mark that DebugStub is complete
            Memory["DebugRunning", 32] = 0;

            Label = "DebugStub_Return";
            Return();
        }
    }
}
