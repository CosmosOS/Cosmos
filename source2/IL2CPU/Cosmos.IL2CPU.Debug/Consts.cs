using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Debug {
  static public class Consts {
    static public UInt32 SerialSignature = 0x19740807;
  }

  // Messages from Guest (Cosmos) to Host (VS)
  static public class DsMsg {
    public const byte Noop = 0;
    public const byte TracePoint = 1;
    public const byte Message = 2;
    public const byte BreakPoint = 3;
    public const byte Error = 4;
    public const byte Pointer = 5;
    // This is sent once on start up. The first call to debug stub sends this. 
    // Host can then respond with a series of set breakpoints etc, ie ones that were set before running.
    public const byte Started = 6;
    public const byte MethodContext = 7;
    public const byte MemoryData = 8;
    // Sent after commands to acknowledge receipt during batch mode
    public const byte CmdCompleted = 9;
    public const byte Registers = 10;
    public const byte Frame = 11;
    public const byte Stack = 12;
    public const byte Pong = 13;
  }

  // Messages from Host (VS) to Guest (Cosmos)
  static public class DsCmd {
    public const byte Noop = 0;
    public const byte TraceOff = 1;
    public const byte TraceOn = 2;
    public const byte Break = 3;
    public const byte Continue = 4; // After a Break
    public const byte StepInto = 5;
    public const byte BreakOnAddress = 6;
    public const byte BatchBegin = 7; // Not used yet
    public const byte BatchEnd = 8; // Not used yet
    public const byte SendMethodContext = 9; // sends data from stack, relative to EBP (in x86)
    public const byte SendMemory = 10;
    public const byte StepOver = 11;
    public const byte StepOut = 12;
    public const byte SendRegisters = 13; // Send the register values to DC
    public const byte SendFrame = 14;
    public const byte SendStack = 15;
    public const byte SetAsmBreak = 16; // Set an assembly level break point
    public const byte Ping = 17; 
    //
    // Make sure this is always the last entry. Used by DebugStub to verify commands.
    public const byte Max = 18;
  }

  static public class Pipes {
    public static readonly string DownName;
    public static readonly string UpName;

    static Pipes() {
      int xPID = System.Diagnostics.Process.GetCurrentProcess().Id;
      DownName = @"Cosmos\DebugDown-" + xPID;
      UpName = @"Cosmos\DebugUp-" + xPID;
    }
  }

  // Commands from VS Debug Engine to VS Debug Window
  static public class DwMsg {
    public const byte Noop = 0;
    public const byte Registers = 1;
    public const byte AssemblySource = 3;
    public const byte Quit = 4;
    public const byte Frame = 5;
    public const byte Stack = 6;
    public const byte Pong = 7;
  }

  // Commands from VS Debug Window to VS Debug Engine
  static public class DwCmd {
    public const byte Noop = 0;
    public const byte Ping = 1;
  }

}
