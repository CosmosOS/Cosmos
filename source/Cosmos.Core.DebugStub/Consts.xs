namespace DebugStub

const Signature = $19740807

const Tracing_Off = 0
const Tracing_On = 1

// Current status of OS Debug Stub
const Status_Run = 0
const Status_Break = 1

const StepTrigger_None = 0
const StepTrigger_Into = 1
const StepTrigger_Over = 2
const StepTrigger_Out = 3

const Vs2Ds_Noop = 0
const Vs2Ds_TraceOff = 1
const Vs2Ds_TraceOn = 2
const Vs2Ds_Break = 3
const Vs2Ds_Continue = 4
const Vs2Ds_BreakOnAddress = 6
const Vs2Ds_BatchBegin = 7
const Vs2Ds_BatchEnd = 8
const Vs2Ds_StepInto = 5
const Vs2Ds_StepOver = 11
const Vs2Ds_StepOut = 12
const Vs2Ds_SendMethodContext = 9
const Vs2Ds_SendMemory = 10
const Vs2Ds_SendRegisters = 13
const Vs2Ds_SendFrame = 14
const Vs2Ds_SendStack = 15
// Set an assembly level break point
// Only one can be active at a time. BreakOnAddress can have multiple.
// User must call continue after.
const Vs2Ds_SetAsmBreak = 16
const Vs2Ds_Ping = 17
const Vs2Ds_AsmStepInto = 18
const Vs2Ds_SetINT3 = 19
const Vs2Ds_ClearINT3 = 20
const Vs2Ds_Max = 21

const Ds2Vs_Noop = 0
const Ds2Vs_TracePoint = 1
const Ds2Vs_Message = 192
const Ds2Vs_BreakPoint = 3
const Ds2Vs_Error = 4
const Ds2Vs_Pointer = 5
const Ds2Vs_Started = 6
const Ds2Vs_MethodContext = 7
const Ds2Vs_MemoryData = 8
const Ds2Vs_CmdCompleted = 9
const Ds2Vs_Registers = 10
const Ds2Vs_Frame = 11
const Ds2Vs_Stack = 12
const Ds2Vs_Pong = 13
const Ds2Vs_BreakPointAsm = 14
const Ds2Vs_StackCorruptionOccurred = 15
const Ds2Vs_MessageBox = 16
const Ds2Vs_NullReferenceOccurred = 17
const Ds2Vs_SimpleNumber = 18
const Ds2Vs_SimpleLongNumber = 19
const Ds2Vs_ComplexNumber = 20
const Ds2Vs_ComplexLongNumber = 21
const Ds2Vs_StackOverflowOccurred = 22
const Ds2Vs_InterruptOccurred = 23
const Ds2Vs_CoreDump = 24
const Ds2Vs_KernelPanic = 25
