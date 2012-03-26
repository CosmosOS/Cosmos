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
        JumpIf(Flags.Equal, ".Exit");
        // Clear old break point and set back to original opcode / partial opcode
        EAX = AsmOrigByte.Value;
        EDI[0] = EAX;
        AsmOrigByte.Value = 0;
      }
    }

    public class Executing : CodeBlock {
      void CheckForBreakpoint() {
        // Look for a possible matching BP
        // TODO: This is slow on every Int3...
        //   -Find a faster way - a list of 256 straight compares and code modifation?
        //   -Count BPs and modify ECX since we usually dont have 256 of them?
        //   -Move this scan earlier - Have to set a global flag when anything (StepTriggers, etc below) is going on at all
        //     A selective disable of the DS
        //   -If there are 0 BPs, skip scan - easy and should have a good increase
        EAX = CallerEIP.Value;
        EDI = AddressOf("DebugBPs");
        ECX = 256;
        new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 };
        JumpIf(Flags.NotEqual, ".AfterBreakOnAddress");
        Call<Break>();
        Jump(".Normal");
        Label = ".AfterBreakOnAddress";
      }

      // This is the secondary stub routine. After the primary has decided we should do some debug
      // activities, this one is called.
      public override void Assemble() {
        CheckForBreakpoint();

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
        JumpIf(Flags.NotEqual, ".StepIntoAfter");
        Call<Break>();
        //TODO: Allow creating labels but issuing them later, then we can call them with early binding
        //TODO: End - can be exit label for each method, allowing Jump(Begin/End) etc... Also make a label type and allwo Jump overload to the label itself. Or better yet, End.Jump()
        Jump(".Normal");
        Label = ".StepIntoAfter";

        // F10
        DebugBreakOnNextTrace.Value.Compare(StepTrigger.Over);
        JumpIf(Flags.NotEqual, ".StepOverAfter");
        //Label = "Debug__StepOver__";
        EAX = CallerEBP.Value;
        EAX.Compare(DebugBreakEBP.Value);
        // If EBP and start EBP arent equal, dont break
        // Dont use Equal because we aslo need to stop above if the user starts
        // the step at the end of a method and next item is after a return
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump(".Normal");
        Label = ".StepOverAfter";

        // Shift-F11
        DebugBreakOnNextTrace.Value.Compare(StepTrigger.Out);
        JumpIf(Flags.NotEqual, ".StepOutAfter");

        EAX = CallerEBP.Value;
        EAX.Compare(DebugBreakEBP.Value); // TODO: X# JumpIf(EAX == Memory[...... or better yet if(EAX==Memory..., new Delegate { Jump.... Jump should be handled specially so we dont jump around jumps... TODO: Also allow Compare(EAX, 0), in fact force this new syntax
        JumpIf(Flags.Equal, ".Normal");
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump(".Normal");
        Label = ".StepOutAfter";

        Label = ".Normal";

        // If tracing is on, send a trace message
        // Tracing isnt really used any more, was used
        // by the old stand alone debugger. Might be upgraded
        // and resused in the future.
        DebugTraceMode.Value.Compare(Tracing.On);
        CallIf(Flags.Equal, "DebugStub_SendTrace");

        // Is there a new incoming command? We dont want to wait for one
        // if there isn't one already here. This is a passing check.
        Label = ".CheckForCmd"; //TODO: ".CheckForCmd" and make it local to our class
        DX = (ushort)(mComAddr + 5u);
        AL = Port[DX];
        AL.Test(0x01);
        // If no command waiting, break from loop
        JumpIf(Flags.Zero, ".CheckForCmd_Break");
        Call<ProcessCommand>();
        // See if there are more commands waiting
        Jump(".CheckForCmd");
        Label = ".CheckForCmd_Break";
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
        {
          // Check for common commands first
          Call<ProcessCommand>();

          // Now check for commands that are only valid in break state
          // or commands that require special handling while in break state.

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
        }

        Label = "DebugStub_Break_Exit";
        Call<AckCommand>();
        DebugStatus.Value = Status.Run;
      }
    }
  }
}
