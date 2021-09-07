# Debug Commands

## DS to VS

```cs
Noop = 0;
TracePoint = 1;
Message = 2;
BreakPoint = 3;
Error = 4;
Pointer = 5;
// This is sent once on start up. The first call to debug stub sends this.
// Host can then respond with a series of set breakpoints etc, ie ones that were set before running.
Started = 6;
MethodContext = 7;
MemoryData = 8;
// Sent after commands to acknowledge receipt during batch mode
CmdCompleted = 9;
Registers = 10;
Frame = 11;
Stack = 12;
Pong = 13;
BreakPointAsm = 14;
```

## VS to DS

```cs
Noop = 0;

TraceOff = 1; // Dont think currently used
TraceOn = 2; // Dont think currently used

Break = 3;
Continue = 4; // After a Break
BreakOnAddress = 6;

BatchBegin = 7;
BatchEnd = 8;

StepInto = 5;
StepOver = 11;
StepOut = 12;

SendMethodContext = 9; // Sends data from stack, relative to EBP (in x86)
SendMemory = 10;
SendRegisters = 13; // Send the register values to DC
SendFrame = 14;
SendStack = 15;

    // Set an assembly level break point
    // Only one can be active at a time. BreakOnAddress can have multiple.
    // User must call continue after.
SetAsmBreak = 16;

Ping = 17;
    // Make sure this is always the last entry. Used by DebugStub to verify commands.
Max = 18;
```

## Debug channel
We support channels, which are prefixed with anything prefixed with 192 and up. 192 is used for a debug view.
