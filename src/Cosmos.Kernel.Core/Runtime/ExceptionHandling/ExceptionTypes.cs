using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// Runtime exception IDs.
/// </summary>
internal enum ExceptionIDs
{
    OutOfMemory = 1,
    NullReference = 2,
    DivideByZero = 3,
    InvalidCast = 4,
    IndexOutOfRange = 5,
    ArrayTypeMismatch = 6,
    Overflow = 7,
    Arithmetic = 8,
}

/// <summary>
/// Exception handling clauses as defined in ICodeManager.h.
/// </summary>
internal enum EHClauseKind
{
    EH_CLAUSE_TYPED = 0,   // Catch handler for specific exception type
    EH_CLAUSE_FAULT = 1,   // Fault handler (like finally, runs on exception)
    EH_CLAUSE_FILTER = 2,  // Filter expression before catch
}

/// <summary>
/// Exception handling clause structure matching NativeAOT format.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct EHClause
{
    public EHClauseKind ClauseKind;
    public uint TryStartOffset;
    public uint TryEndOffset;
    public byte* FilterAddress;
    public byte* HandlerAddress;
    public void* TargetType;  // MethodTable* for the exception type to catch
}

/// <summary>
/// Stack frame information for the frame-pointer chain walker.
/// </summary>
internal unsafe struct StackFrame
{
    public nuint ReturnAddress;   // Where this frame returns to
    public nuint FramePointer;    // RBP/FP value for this frame
    public nuint StackPointer;    // RSP/SP value for this frame
}
