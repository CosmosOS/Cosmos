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
      [XSharp(IsInteruptHandler = true)]
      // Int3 entry point
      public override void Assemble() {
        // We need to make sure Int3 can never run more than one instance at a time.
        // We are not threaded yet, when we are we have to change stuff to thread vars and a lot of other stuff.
        // Two Int3s can never be called at the same time normally, but IRQs can happen while the DebugStub is
        // running. We also need to make sure IRQs are allowed to run during DebugStub as DebugStub can wait for
        // a long time on commands.
        // So we need to disable interrupts immediately and set a flag, then reenable interrupts if they were enabled
        // when we disabled them. Later this can be replaced by some kind of critical section / lock around this code.
        // Currently IRQs are disabled - we need to fix DS before we can reenable them and add support for critical sections / locks here.
        // -http://www.codemaestro.com/reviews/8
        // -http://en.wikipedia.org/wiki/Spinlock - Uses a register which is a problem for us
        // -http://wiki.osdev.org/Spinlock
        //   -Looks good and also allows testing intead of waiting
        //   -Wont require us to disable / enable IRQs

        // This code is temporarily disabled as IRQs are not enabled right now.
        // LockOrExit()
        {
          SaveExecuteRestore();
        }
        // Unlock();
      }

      protected void SaveExecuteRestore() {
        // EBP is restored by PopAll, but SendFrame uses it. Could
        // get it from the PushAll data, but this is easier.
        CallerEBP.Value = EBP;

        // Could also get ESP from PushAll but this is easier.
        // Another reason to do it here is that some day we may need to use 
        // the stack before PushAll.
        //
        // We cant modify any registers since we havent done PushAll yet
        // Maybe we could do a sub(4) on memory direct.. 
        // But for now we remove from ESP which the Int3 produces,
        // store ESP, then restore ESP so we don't cause stack corruption.
        ESP = ESP + 12; // 12 bytes for EFLAGS, CS, EIP
        CallerESP.Value = ESP;
        ESP = ESP - 12;

        PushAll();
        {
          // Save current ESP so we can look at the results of PushAll later
          DebugPushAllPtr.Value = ESP;

          // Get current ESP and add 32. This will skip over the PushAll and point us at the call data from Int3.
          EBP = ESP;
          EBP = EBP + 32;

          // Caller EIP
          EAX = EBP[0];
          // EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.
          // Note - when we used call it was 5 (the size of our call + address)
          // so we get the EIP as IL2CPU records it. Its also useful for when we will
          // be changing ops that call this stub.
          EAX--;
          // Store it for later use.
          CallerEIP.Value = EAX;

          // Call secondary stub
          Call<Executing>();
        }
        PopAll(); // Restore registers
      }
    }

  }

}
