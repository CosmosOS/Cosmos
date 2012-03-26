using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Debug.Consts;
using Cosmos.Assembler.XSharp;

namespace Cosmos.Debug.DebugStub {
  public partial class DebugStub : CodeGroup {
    // Caller's Registers
    static public DataMember32 CallerEBP;
    static public DataMember32 CallerEIP;
    static public DataMember32 CallerESP;

    public class AckCommand : CodeBlock {
      public override void Assemble() {
        // We acknowledge receipt of the command AND the processing of it.
        //   -In the past the ACK only acknowledged receipt.
        // We have to do this because sometimes callers do more processing.
        // We ACK even ones we dont process here, but do not ACK Noop.
        // The buffers should be ok because more wont be sent till after our NACK
        // is received.
        // Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7. 
        // UART buffer is 16.
        // We may need to revisit this in the future to ack not commands, but data chunks
        // and move them to a buffer.
        // The buffer problem exists only to inbound data, not outbound data (relative to DebugStub).
        AL = DsVsip.CmdCompleted;
        Call<WriteALToComPort>();
        //
        EAX = DebugStub_CommandID.Value;
        Call<WriteALToComPort>();
      }
    }

    public class ProcessCommand : CodeBlock {
      // Modifies: AL, DX (ReadALFromComPort)
      // Returns: AL
      public override void Assemble() {
        Call<ReadALFromComPort>();
        // Some callers expect AL to be returned, so we preserve it
        // in case any commands modify AL.
        // We push EAX to keep stack aligned. 
        EAX.Push();

        // Noop has no data at all (see notes in client DebugConnector), so skip Command ID
        // Noop also does not send ACK.
        AL.Compare(VsipDs.Noop);
        JumpIf(Flags.Equal, ".End");

        // Read Command ID
        Call<ReadALFromComPort>();
        DebugStub_CommandID.Value = EAX;

        // Get AL back so we can compare it, but also put it back for later
        EAX = ESP[0];
        CheckCmd(VsipDs.TraceOff, typeof(TraceOff));
        CheckCmd(VsipDs.TraceOn, typeof(TraceOn));
        CheckCmd(VsipDs.Break, typeof(Break));
        CheckCmd(VsipDs.BreakOnAddress, typeof(BreakOnAddress));
        CheckCmd(VsipDs.SendMethodContext, typeof(SendMethodContext));
        CheckCmd(VsipDs.SendMemory, typeof(SendMemory));
        CheckCmd(VsipDs.SendRegisters, typeof(SendRegisters));
        CheckCmd(VsipDs.SendFrame, typeof(SendFrame));
        CheckCmd(VsipDs.SendStack, typeof(SendStack));
        CheckCmd(VsipDs.SetAsmBreak, typeof(SetAsmBreak));
        CheckCmd(VsipDs.Ping, typeof(Ping));

        Label = ".End";
        // Restore AL for callers who check the command and do
        // further processing, or for commands not handled by this routine.
        EAX.Pop();
      }

      protected void CheckCmd(byte aCmd, Type aFunction) {
        AL.Compare(aCmd);
        string xAfterLabel = NewLabel();
        JumpIf(Flags.NotEqual, xAfterLabel);
        Call(aFunction);
        Call<AckCommand>();
        Jump(".End");
        Label = xAfterLabel;
      }
    }

    // Tracing: 0=Off, 1=On
    static protected DataMember32 DebugTraceMode;
    // enum Status
    static protected DataMember32 DebugStatus;
    // Nesting control for non steppable routines
    static protected DataMember32 DebugSuspendLevel;
    // Ptr to the push all data. It points to the "bottom" after a PushAll op.
    // Walk up to find the 8 x 32 bit registers.
    static protected DataMember32 DebugPushAllPtr;
    // State of Interrupts on entry
    static protected DataMember32 InterruptsEnabledFlag;
    // If set non 0, on next trace a break will occur
    static protected DataMember32 DebugBreakOnNextTrace;
    // For step out and over this is used to determine where the initial request was made
    // EBP is logged when the trace is started and can be used to determine 
    // what level we are "at" relative to the original step start location.
    static protected DataMember32 DebugBreakEBP;
    // Command ID of last command received
    static protected DataMember32 DebugStub_CommandID;

    public class BreakOnAddress : Inlines {
      // Sets a breakpoint
      // Serial Params:
      //   1: x32 - EIP to break on, or 0 to disable breakpoint.
      [XSharp(PreserveStack = true)]
      public override void Assemble() {
        // BP Address
        ReadComPortX32toStack(1);
        ECX.Pop();

        // BP ID Number
        // BP ID Number is sent after BP Address, becuase
        // reading BP address uses AL (EAX).
        EAX = 0;
        Call<ReadALFromComPort>();

        // Calculate location in table
        // Mov [EBX + EAX * 4], ECX would be better, but our asm doesn't handle this yet
        EBX = AddressOf("DebugBPs");
        EAX = EAX << 2;
        EBX.Add(EAX);

        EBX[0] = ECX;
      }
    }

    public class ProcessCommandBatch : CodeBlock {
      public override void Assemble() {
        Call<ProcessCommand>();

        // See if batch is complete
        AL.Compare(VsipDs.BatchEnd);
        JumpIf(Flags.Equal, "DebugStub_ProcessCommandBatch_Exit");

        // Loop and wait
        Jump("DebugStub_ProcessCommandBatch");

        Label = "DebugStub_ProcessCommandBatch_Exit";
        Call<AckCommand>();
      }
    }

    // Location where INT3 has been injected
    // 0 if no INT3 is active
    static public DataMember32 AsmBreakEIP;
    // Old byte before INT1 was injected
    // Only 1 byte is used
    static public DataMember32 AsmOrigByte;
    //
    public class SetAsmBreak : Inlines {
      public override void Assemble() {
        ReadComPortX32toStack(1);
        EDI.Pop();
        // Save the old byte
        EAX = EDI[0];
        AsmOrigByte.Value = EAX;
        // Inject INT3
        EDI[0] = 0xCC;
        // Save EIP of the break
        AsmBreakEIP.Value = EDI;
      }
    }

    public class ClearAsmBreak : Inlines {
      public override void Assemble() {
        EDI = AsmBreakEIP.Value;
        EDI.Compare(0);
        // If 0, we don't need to clear an older one.
        JumpIf(Flags.Equal, ".Done");
        // Clear old break point and set back to original opcode / partial opcode
        EAX = AsmOrigByte.Value;
        EDI[0] = EAX;
        AsmOrigByte.Value = 0;

        Label = ".Done";
      }
    }

    public class Executing : CodeBlock {
      // This is the secondary stub routine. After the primary (main) has decided we should do some debug
      // activities, this one is called.
      //
      // Modifies: EAX, EDI, ECX
      public override void Assemble() {
        // Look for a possible matching BP
        // TODO: This is slow on every Int3...
        //   -Find a faster way - a list of 256 straight compares and code modifation?
        //   -Count BPs and modify ECX since we usually dont have 256 of them?
        //   -Move this scan earlier?
        //   -If there are 0 BPs, skip scan - easy and should have a good increase
        EAX = CallerEIP.Value;
        EDI = AddressOf("DebugBPs");
        ECX = 256;
        new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 };
        JumpIf(Flags.NotEqual, "DebugStub_Executing_AfterBreakOnAddress");
        Call<Break>();
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_Executing_AfterBreakOnAddress";

        // See if we are stepping
        //
        // F11
        DebugBreakOnNextTrace.Value.Compare(StepTrigger.Into);
        //TODO: I think we can use a using statement to create this type of block
        // and emit asm
        // using (var xBlock = new AsmBlock()) {
        //   JumpIf(something, xBlock.End/Begin);
        //   also can do xBlock.Break();
        // }
        //TODO: If statements can probably be done with anonymous delegates...
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepIntoAfter");
        Call<Break>();
        //TODO: Allow creating labels but issuing them later, then we can call them with early binding
        //TODO: End - can be exit label for each method, allowing Jump(Begin/End) etc... Also make a label type and allwo Jump overload to the label itself. Or better yet, End.Jump()
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepIntoAfter";
        
        // F10
        DebugBreakOnNextTrace.Value.Compare(StepTrigger.Over);
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepOverAfter");
        Label = "Debug__StepOver__";
        EAX = CallerEBP.Value;
        EAX.Compare(DebugBreakEBP.Value);
        // If EBP and start EBP arent equal, dont break
        // Dont use Equal because we aslo need to stop above if the user starts
        // the step at the end of a method and next item is after a return
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepOverAfter";
        
        // Shift-F11
        DebugBreakOnNextTrace.Value.Compare(StepTrigger.Out);
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepOutAfter");

        EAX = CallerEBP.Value; 
        EAX.Compare(DebugBreakEBP.Value); // TODO: X# JumpIf(EAX == Memory[...... or better yet if(EAX==Memory..., new Delegate { Jump.... Jump should be handled specially so we dont jump around jumps... TODO: Also allow Compare(EAX, 0), in fact force this new syntax
        JumpIf(Flags.Equal, "DebugStub_Executing_Normal");
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepOutAfter";

        Label = "DebugStub_Executing_Normal";
        
        // If tracing is on, send a trace message
        // Tracing isnt really used any more, was used
        // by the old stand alone debugger. Might be upgraded
        // and resused in the future.
        DebugTraceMode.Value.Compare(Tracing.On);
        CallIf(Flags.Equal, "DebugStub_SendTrace");

        // Is there a new incoming command? We dont want to wait for one
        // if there isn't one already here. This is a passing check.
        Label = "DebugStub_CheckForCmd"; //TODO: ".CheckForCmd" and make it local to our class
        DX = (ushort)(mComAddr + 5u);
        AL = Port[DX];
        AL.Test(0x01);
        // If no command waiting, break from loop
        JumpIf(Flags.Zero, "DebugStub_CheckForCmd_Break");
        Call<ProcessCommand>();
        // See if there are more commands waiting
        Jump("DebugStub_CheckForCmd");
        Label = "DebugStub_CheckForCmd_Break";
      }
    }

    public class Break : CodeBlock {
      // Should only be called internally by DebugStub. Has a lot of preconditions.
      // Externals should use BreakOnNextTrace instead.
      public override void Assemble() {
        // Reset request in case we are currently responding to one or we hit a fixed breakpoint
        // before our request could be serviced (if one existed)
        DebugBreakOnNextTrace.Value = StepTrigger.None;
        DebugBreakEBP.Value = 0;
        // Set break status
        DebugStatus.Value = Status.Break;
        Call<SendTrace>();

        // Wait for a command
        Label = "DebugStub_WaitCmd";
        
        // Check for common commands first
        Call<ProcessCommand>();

        // Now check for commands that are only valid in break state
        // or commands that require special handling while in break
        // state.

        AL.Compare(VsipDs.Continue);
        JumpIf(Flags.Equal, "DebugStub_Break_Exit");

        AL.Compare(VsipDs.StepInto);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepInto_After");
        DebugBreakOnNextTrace.Value = StepTrigger.Into;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepInto_After";

        AL.Compare(VsipDs.StepOver);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepOver_After");
        DebugBreakOnNextTrace.Value = StepTrigger.Over;
        EAX = CallerEBP.Value;
        DebugBreakEBP.Value = EAX;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepOver_After";

        AL.Compare(VsipDs.StepOut);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepOut_After");
        DebugBreakOnNextTrace.Value = StepTrigger.Out;
        EAX = CallerEBP.Value;
        DebugBreakEBP.Value = EAX;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepOut_After";

        // Loop around and wait for another command
        Jump("DebugStub_WaitCmd");

        Label = "DebugStub_Break_Exit";
        Call<AckCommand>();
        DebugStatus.Value = Status.Run;
      }
    }

    public class TracerEntry : CodeBlock {
      // 0 = Not in, 1 = already running
      public DataMember32 IsRunning;

      [XSharp(IsInteruptHandler = true)]
      // Main entry point for the DebugStub which is executed by INT3.
      public override void Assemble() {
        // Test only for now
        // Wont work... need to use a register
        //new MoveToDR6(0);

        // EBP is restored by PopAll, but SendFrame uses it. Could
        // get it from the PushAll data, but this is easier.
        CallerEBP.Value = EBP;

        // Could also get ESP from PushAll but this is easier
        // Another reason to do it here is that some day we may need to use 
        // the stack before PushAll.
        //
        // We cant modify any registers since we havent done PushAll yet
        // Maybe we could do a sub(4) on memory direct.. 
        // But for now we remove from ESP which the call to us produces,
        // store ESP, then restore ESP so we don't cause stack corruption.
        ESP = ESP + 12; // 12 bytes for EFLAGS, CS, EIP
        CallerESP.Value = ESP;
        ESP = ESP - 12;

        // If debug stub is in break, and then an IRQ happens, the IRQ
        // can call DebugStub again. This causes two DebugStubs to 
        // run which causes havoc. So we only allow one to run.
        // We arent multi threaded yet, so this works fine.
        // IRQ's are disabled between Compare and JumpIf so an IRQ cant
        // happen in between them which could also cause double entry.
        DisableInterrupts();
        DebugSuspendLevel.Value.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Running");
        // DebugStub is already running, so exit.
        // But we need to see if IRQs are disabled.
        // If IRQ disabled, we dont reenable them after our disable
        // in this routine.
        InterruptsEnabledFlag.Value.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Return");
        EnableInterrupts();
        Jump("DebugStub_Return");

        Label = "DebugStub_Running";
        IsRunning.Value.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Start");
        // If we made it this far we exit because DebugStub is already running.
        // We need to see if IRQs were originally enabled or disabled and
        // re-enable them if they were enabled on entry.
        Jump("DebugStub_CheckIntAndReturn");

        // All clear, mark that we are entering the debug stub
        Label = "DebugStub_Start";
        IsRunning.Value = 1;

        // DS is now marked not to re-enter, so re-enable interrupts if
        // they were enabled on entry
        InterruptsEnabledFlag.Value.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_NoSTI");
        EnableInterrupts();

        // Call secondary debug stub
        Label = "DebugStub_NoSTI";
        PushAll();
        DebugPushAllPtr.Value = ESP;
        // We just pushed all registers to the stack so we can use them
        // So we get the stack pointer and add 32. This skips over the
        // registers we just pushed.
        EBP = ESP;
        EBP = EBP + 32; // We dont need to restore this becuase it was pushed as part of PushAll32

        // Get actual EIP of caller.
        EAX = EBP[0];
        // EIP is pointer to op after our call. We subtract 1 for the opcode size of Int3
        // Note - when we used call it was 5 (the size of our call + address)
        // so we get the EIP as IL2CPU records it. Its also useful for when we will
        // be changing ops that call this stub.
        EAX--; 
        // Store it for later use.
        CallerEIP.Value = EAX;

        // Call secondary stub
        Call<Executing>();

        // Restore registers
        PopAll();

        // Setting the DebugRuning flag is atomic, but in the future
        // we might have other code as we do in the entry to check.
        // So just to be safe, we disable interrupts while we do this.
        DisableInterrupts();
        // Complete, mark that DebugStub is complete
        IsRunning.Value = 0;

        Label = "DebugStub_CheckIntAndReturn";
        // Re-enable interrupts if needed. This happens on normal exit, or call from above
        // when there would have been a re-entry to DS.
        InterruptsEnabledFlag.Value.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Return");
        EnableInterrupts();

        Label = "DebugStub_Return";
      }
    }

    public class DebugSuspend : CodeBlock {
      public override void Assemble() {
        DebugSuspendLevel.Value++;
      }
    }

    public class DebugResume : CodeBlock {
      public override void Assemble() {
        DebugSuspendLevel.Value--;
      }
    }
  }

}
