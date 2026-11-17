using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Runtime.GcInfo;
using Cosmos.Kernel.Core.Scheduler;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// Core exception handling implementation.
/// </summary>
internal static unsafe partial class ExceptionHelper
{
    // Maximum stack frames to walk: a guard against a corrupt frame chain, the walk ends at
    // the bottom of the stack first. Deep enough for a throw a few hundred frames below its
    // catch, as an interpreter's error raised from nested callbacks is.
    private const int MAX_STACK_FRAMES = 4096;

    // Assembly funclet callers - use nint for object reference since P/Invoke doesn't support object.
    [LibraryImport("*", EntryPoint = "RhpCallCatchFunclet")]
    [SuppressGCTransition]
    private static partial void* RhpCallCatchFunclet(nint exceptionPtr, void* handlerAddress, void* pRegDisplay, void* pExInfo);

    [LibraryImport("*", EntryPoint = "RhpCallFilterFunclet")]
    [SuppressGCTransition]
    private static partial nint RhpCallFilterFunclet(nint exceptionPtr, void* filterAddress, void* pRegDisplay);

    [LibraryImport("*", EntryPoint = "RhpCallFinallyFunclet")]
    [SuppressGCTransition]
    private static partial void RhpCallFinallyFunclet(void* handlerAddress, void* pRegDisplay);

    // The dispatcher's state for the boot code, before it is a scheduler thread (see State).
    private static ExceptionDispatchState s_bootState;

    // Whether every throw is told on the serial port (its message and where it was thrown), and
    // the handler found for it: for debugging the dispatcher, whose crash would otherwise lose
    // the exception. Off, only an exception nothing catches is told: a line costs ~90 µs per
    // character over a 115200-baud UART, and code that catches what it throws (a script engine
    // running a page) throws dozens of times a second.
    internal static bool s_traceThrows;

    // The funclets running on top of the dispatcher — a catch, or a finally the second pass
    // entered — recorded just before control transfers to each, with the index of the clause it
    // runs and the ExInfo of the exception that entered it. A funclet runs on top of the
    // dispatcher, itself on top of the frames of the throw, so an exception thrown while it runs
    // — a `throw;`, or a new exception — walks back from the funclet through that throw's frames:
    // they are unwound as far as the program goes, and must not see it. In the funclet's own
    // frame, the clauses the first dispatch went past — the funclet's, and those listed before it,
    // nested inside its try region — must neither catch it nor run again. A chain (not a single
    // record) because funclets nest: a catch that throws from its handler runs the next catch on
    // top of itself.
    //
    // A catch's record lives as long as its funclet: RhpThrowEx links an ExInfo into a chain for
    // every throw, and RhpCallCatchFunclet unlinks those of the frames it unwinds once the funclet
    // returns, so the catches still running are those whose ExInfo the chain still holds (see
    // PruneFinishedFunclets). A finally's record is dropped when its funclet returns to the
    // dispatcher, or with its ExInfo when an exception it throws is caught. Telling a rethrow by
    // its exception object instead let a new exception thrown from a catch re-enter that catch,
    // and a completed catch's object, thrown again later, skip it.
    //
    // The records, like the chain and the guard against a throw from inside a dispatch, are the
    // throwing thread's (ExceptionDispatchState): a funclet runs on the stack of the thread that
    // threw, and what one thread's dispatch keeps must not be seen by another thread's throw.

    /// <summary>
    /// The dispatcher's state for the running thread: the scheduler thread's own once one runs,
    /// the boot code's before. A thread throws, unwinds and catches on its own stack, so its
    /// ExInfo chain, its dispatch guard and the funclets running on it are its own; shared, a
    /// thread preempted inside a dispatch made the next thread's throw a "recursive" one, and a
    /// <c>throw;</c> rethrew whichever thread had thrown last (valentinbreiz/nativeaot-patcher#440).
    /// </summary>
    internal static ref ExceptionDispatchState State
    {
        get
        {
            if (CosmosFeatures.SchedulerEnabled && SchedulerManager.CurrentCpuState?.CurrentThread is { } current)
            {
                return ref current.GetExceptionDispatchState();
            }

            return ref s_bootState;
        }
    }

    /// <summary>
    /// The index of the last clause of the frame at <paramref name="framePointer"/> that the
    /// dispatch of a funclet running in it went past, the clause that funclet runs, or -1 when no
    /// funclet runs there.
    /// </summary>
    private static int LastPassedClause(nuint framePointer)
    {
        ref ExceptionDispatchState state = ref State;
        int lastPassed = -1;
        for (int i = 0; i < state.FuncletCount; i++)
        {
            ref RunningFunclet funclet = ref state.Funclets[i];
            if (funclet.FramePointer == framePointer && funclet.ClauseIndex > lastPassed)
            {
                lastPassed = funclet.ClauseIndex;
            }
        }

        return lastPassed;
    }

    /// <summary>
    /// Whether a frame belongs to a throw whose funclet is running: it lies between that throw's
    /// ExInfo, on the stack of <c>RhpThrowEx</c>, and the frame of the funclet.
    /// </summary>
    private static bool IsUnwoundFrame(nuint framePointer)
    {
        ref ExceptionDispatchState state = ref State;
        for (int i = 0; i < state.FuncletCount; i++)
        {
            ref RunningFunclet funclet = ref state.Funclets[i];
            if (funclet.ExInfo < framePointer && framePointer < funclet.FramePointer)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Drops the records of the funclets that finished: those whose ExInfo the chain behind
    /// <paramref name="pExInfo"/> (this throw's, linked first) no longer holds.
    /// </summary>
    private static void PruneFinishedFunclets(void* pExInfo)
    {
        ref ExceptionDispatchState state = ref State;
        int kept = 0;
        for (int i = 0; i < state.FuncletCount; i++)
        {
            bool inFlight = false;
            for (nuint* p = pExInfo == null ? null : *(nuint**)pExInfo; p != null; p = *(nuint**)p)
            {
                if ((nuint)p == state.Funclets[i].ExInfo)
                {
                    inFlight = true;
                    break;
                }
            }

            if (inFlight)
            {
                state.Funclets[kept] = state.Funclets[i];
                kept++;
            }
        }

        state.FuncletCount = kept;
    }

    /// <summary>
    /// Records a funclet about to run clause <paramref name="clauseIndex"/> of the frame at
    /// <paramref name="framePointer"/>, so an exception thrown while it runs is dispatched past it.
    /// </summary>
    private static void EnterFunclet(nuint framePointer, int clauseIndex, void* pExInfo)
    {
        ref ExceptionDispatchState state = ref State;
        if (state.FuncletCount < ExceptionDispatchState.MaxRunningFunclets)
        {
            ref RunningFunclet funclet = ref state.Funclets[state.FuncletCount];
            funclet.FramePointer = framePointer;
            funclet.ClauseIndex = clauseIndex;
            funclet.ExInfo = (nuint)pExInfo;
            state.FuncletCount++;
        }
        else
        {
            Serial.WriteString("[EH] Too many nested funclets to track\n");
        }
    }

    // Arch-specific hooks — defined in ExceptionHelper.X64.cs / ExceptionHelper.ARM64.cs.

    /// <summary>
    /// Seed an <see cref="UnwindState"/> from the throw-site <see cref="PAL_LIMITED_CONTEXT"/>
    /// captured by <c>RhpThrowEx</c>: copy the callee-saved registers (plus SP and IP) into the
    /// register table, then set every per-register rule to <c>SameValue</c>.
    /// </summary>
    private static partial void InitUnwindStateFromContext(ref UnwindState state, PAL_LIMITED_CONTEXT* pContext);

    /// <summary>
    /// Pin the frame register (RBP on x64, FP on ARM64) to <paramref name="framePointer"/> in the
    /// REGDISPLAY a filter or finally funclet runs against.
    /// </summary>
    private static partial void PinFrameRegister(REGDISPLAY* regDisplay, nuint framePointer);

    /// <summary>
    /// Pin the REGDISPLAY for the catch funclet that <c>RhpCallCatchFunclet</c> will run with. x64
    /// keeps SP at the CFA (set by <see cref="ProjectRegDisplay"/>); ARM64 sets both FP and SP to
    /// the catching frame's frame pointer (known follow-up — CFA alignment for non-trivial frames).
    /// </summary>
    private static partial void PinPass2RegDisplay(REGDISPLAY* regDisplay, nuint framePointer);

    /// <summary>
    /// Fill a <see cref="REGDISPLAY"/> from the current register values in <paramref name="s"/>.
    /// On x64 the <c>pRxx</c> save-location pointers are wired to point at the value slots inside
    /// <paramref name="rd"/> itself. Used by the precise GC stack scan and by the exception
    /// dispatcher when it hands a REGDISPLAY to a filter/finally/catch funclet.
    /// </summary>
    internal static partial void ProjectRegDisplay(ref UnwindState s, REGDISPLAY* rd);

    /// <summary>
    /// Seed an <see cref="UnwindState"/> from a <see cref="REGDISPLAY"/> and an instruction pointer,
    /// ready for <see cref="UnwindOneFrameWithCFI"/>. All register rules start as <c>SameValue</c>.
    /// </summary>
    internal static partial void SeedUnwindStateFromRegDisplay(ref UnwindState s, REGDISPLAY* rd, nuint ip);

    /// <summary>
    /// Managed entry point for a thrown exception, called from <c>RhThrowEx</c> (the assembly stub)
    /// with the throw-site register context. Walks the stack, runs filters and finally funclets,
    /// and transfers to the catch handler; if nothing catches it, halts.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowExceptionWithContext(Exception ex, nuint throwAddress, nuint throwRbp, nuint throwRsp, void* pExInfo)
    {
        if (ex is null)
        {
            FailFast("Null exception", ex);
            return;
        }

        // Guard against an exception thrown while this thread is already dispatching one.
        ref ExceptionDispatchState state = ref State;
        if (state.IsHandlingException)
        {
            FailFast("Recursive exception", ex);
            return;
        }
        state.IsHandlingException = true;

        PruneFinishedFunclets(pExInfo);

        // Before the stack walk, in case the walk crashes.
        if (s_traceThrows)
        {
            WriteThrow(ex, throwAddress, throwRbp, throwRsp);
        }

        DispatchExceptionWithContext(ex, throwAddress, throwRbp, throwRsp, pExInfo);

        // DispatchExceptionWithContext transfers to the handler on success; it only returns here
        // when no handler covered the throw.
        if (!s_traceThrows)
        {
            WriteThrow(ex, throwAddress, throwRbp, throwRsp);
        }

        Serial.WriteString("\n*** UNHANDLED EXCEPTION ***\n");
        Serial.WriteString("No catch handler found. System halting...\n");
        FailFast("Unhandled exception", ex);
    }

    /// <summary>Tells the serial port what was thrown, and where.</summary>
    private static void WriteThrow(Exception ex, nuint throwAddress, nuint throwRbp, nuint throwRsp)
    {
        Serial.WriteString("\n=== DOTNET EXCEPTION THROWN ===\n");
        // Avoid GetType().Name — it allocates.
        string? msg = ex.Message;
        if (msg is not null)
        {
            Serial.WriteString("Message: ");
            Serial.WriteString(msg);
            Serial.WriteString("\n");
        }
        Serial.WriteString("Exception at 0x");
        Serial.WriteNumber(throwAddress);
        Serial.WriteString("\n");
        Serial.WriteString("RBP: 0x");
        Serial.WriteNumber(throwRbp);
        Serial.WriteString("\n");
        Serial.WriteString("RSP: 0x");
        Serial.WriteNumber(throwRsp);
        Serial.WriteString("\n");
    }

    /// <summary>
    /// Two-pass exception dispatch from the throw-site context: pass 1 walks the stack to find a
    /// handler (unwinding the register state at each frame so filters can run); pass 2 walks it
    /// again to run the finally funclets of the try regions the exception leaves
    /// (<see cref="RunFinallyFunclets"/>), then transfers to the catch handler via
    /// <see cref="InvokeCatchHandler"/>, which does not return. Returns only when no handler was
    /// found (or the throw-site frame pointer was missing); the finally funclets do not run then,
    /// as in NativeAOT, which fails fast on an unhandled exception before its second pass.
    /// </summary>
    private static void DispatchExceptionWithContext(Exception ex, nuint throwAddress, nuint throwRbp, nuint throwRsp, void* pExInfo)
    {
        // No frame pointer from the throw-site context → can't walk the stack.
        if (throwRbp == 0)
        {
            return;
        }

        // REGDISPLAY built from the unwound register state, passed to filter / catch funclets.
        REGDISPLAY regDisplay = default;
        REGDISPLAY* pRegDisplay = null;

        // Seed the CFI unwind state from the throw-site context.
        UnwindState unwindState = default;
        PAL_LIMITED_CONTEXT* pThrowContext = null;
        if (pExInfo != null)
        {
            pThrowContext = *(PAL_LIMITED_CONTEXT**)((byte*)pExInfo + 0x08);
            if (pThrowContext != null)
            {
                InitUnwindStateFromContext(ref unwindState, pThrowContext);
            }
        }

        StackFrame throwFrame;
        throwFrame.FramePointer = throwRbp;
        throwFrame.StackPointer = throwRsp;
        throwFrame.ReturnAddress = throwAddress;

        // Pass 1: walk the frame-pointer chain looking for a handler, unwinding registers as we go.
        StackFrame catchFrame = default;
        EHClause catchClause = default;
        int catchClauseIndex = -1;
        int catchFrameIndex = -1;
        bool foundHandler = false;

        StackFrame frame = throwFrame;
        int frameCount = 0;
        while (frameCount < MAX_STACK_FRAMES && frame.FramePointer != 0)
        {
            // Refresh the REGDISPLAY from the current unwind state so a filter can be evaluated.
            // Filter funclets run on the current frame's establisher, so we pin the frame pointer
            // and SP to `frame.FramePointer` regardless of what the CFI unwind resolved them to.
            if (pThrowContext != null)
            {
                pRegDisplay = &regDisplay;
                ProjectRegDisplay(ref unwindState, pRegDisplay);
                PinFrameRegister(pRegDisplay, frame.FramePointer);
                regDisplay.SP = frame.FramePointer;
            }

            // Does this frame have a handler covering the call that threw? Not if it is a frame
            // of a throw whose funclet is running, and not that funclet's clause again.
            // Probe one byte back into the call: every ReturnAddress here — including frame 0's
            // throwAddress, captured as RhpThrowEx's return address — points at the instruction
            // AFTER the call, and clause try-ends are exclusive, so a call ending a try region
            // would otherwise miss its handler (#387). RyuJIT pads such calls with nop/int3
            // today, making this adjustment behavior-preserving on the current codegen.
            if (!IsUnwoundFrame(frame.FramePointer)
                && TryFindHandler(ex, frame.ReturnAddress - 1, frame.FramePointer, out EHClause clause, out int clauseIndex, pRegDisplay))
            {
                if (s_traceThrows)
                {
                    Serial.WriteString("[EH] Handler found at 0x");
                    Serial.WriteHex((nuint)clause.HandlerAddress);
                    Serial.WriteString("\n");
                }

                catchFrame = frame;
                catchClause = clause;
                catchClauseIndex = clauseIndex;
                catchFrameIndex = frameCount;
                foundHandler = true;
                break;
            }

            // Step to the caller. CFI (the .eh_frame unwinder) is the source of truth: it works
            // regardless of whether the current method keeps RBP/X29 (RyuJIT omits the frame
            // pointer on methods with no EH, small frames, no localloc, no address-taken locals —
            // the managed equivalent of `-fomit-frame-pointer`; tail-called methods don't even
            // get their own frame). The frame-pointer chain breaks across any such intermediate,
            // so we drive the walk from CFI and reserve `UnwindOneFrame` for the (defensive)
            // case where there is no throw context to seed the unwind state with.
            if (pThrowContext != null)
            {
                if (!UnwindToCaller(ref unwindState, ref frame))
                {
                    break;   // No CFI for this IP — can't unwind further precisely.
                }
            }
            else if (!UnwindOneFrame(ref frame))
            {
                break;
            }

            frameCount++;
        }

        if (!foundHandler)
        {
            return;
        }

        // Pass 2, from the throw-site context the walk rebuilds each frame's registers from:
        // without it, the catch below fails fast anyway.
        if (pThrowContext != null)
        {
            RunFinallyFunclets(pThrowContext, throwFrame, catchFrameIndex, catchClauseIndex, pExInfo);

            // Build the REGDISPLAY the catch funclet runs with — and that RhpCallCatchFunclet
            // resumes the catching method from once the funclet returns. The pass 1 walk that
            // landed on the catching frame already reconstructed its body register state at the
            // call that threw, so `unwindState` carries the callee-saved registers and the resume
            // SP directly — that's what `ProjectRegDisplay` copies in (and, on x64, what it wires
            // the pRxx pointers to point at). The frame pointer is overridden separately:
            // `catchFrame.FramePointer` was derived from the same CFI walk, so the two should
            // agree, but we pin it explicitly to match what the funclet expects to find at
            // function entry (`push rbp; mov rbp, rsp` semantics on x64; the establisher's X29 on
            // ARM64).
            pRegDisplay = &regDisplay;
            ProjectRegDisplay(ref unwindState, pRegDisplay);
            PinPass2RegDisplay(pRegDisplay, catchFrame.FramePointer);
        }

        // Record the clause about to run so an exception thrown from inside its funclet is
        // dispatched past it instead of re-entering it (see the running-funclet records above).
        EnterFunclet(catchFrame.FramePointer, catchClauseIndex, pExInfo);

        InvokeCatchHandler(ex, ref catchClause, pExInfo, pRegDisplay);
        // InvokeCatchHandler jumps to the resume address and does not return.
    }

    /// <summary>
    /// The first half of pass 2: walks the frames again from the throw site to the catching frame
    /// and runs, innermost first, the finally (and fault) funclets of the try regions covering
    /// each frame's call site — in the catching frame only those listed before the catching
    /// clause, which are nested inside its try region. The frames and clauses pass 1 went past
    /// for a running funclet are passed again.
    /// </summary>
    private static void RunFinallyFunclets(PAL_LIMITED_CONTEXT* pThrowContext, StackFrame frame, int catchFrameIndex, int catchClauseIndex, void* pExInfo)
    {
        UnwindState unwindState = default;
        InitUnwindStateFromContext(ref unwindState, pThrowContext);

        REGDISPLAY regDisplay = default;
        for (int frameIndex = 0; ; frameIndex++)
        {
            if (!IsUnwoundFrame(frame.FramePointer)
                && TryGetFrameClauses(frame.ReturnAddress - 1, frame.FramePointer, out FrameClauses clauses))
            {
                // A finally funclet runs on its frame's registers, as a filter does in pass 1.
                ProjectRegDisplay(ref unwindState, &regDisplay);
                PinFrameRegister(&regDisplay, frame.FramePointer);

                int clauseLimit = frameIndex == catchFrameIndex ? catchClauseIndex : int.MaxValue;
                while (clauses.MoveNext(out EHClause clause, out int clauseIndex) && clauseIndex < clauseLimit)
                {
                    if (clause.ClauseKind == EHClauseKind.EH_CLAUSE_FAULT)
                    {
                        InvokeFinallyHandler(clause.HandlerAddress, clauseIndex, frame.FramePointer, &regDisplay, pExInfo);
                    }
                }
            }

            if (frameIndex == catchFrameIndex || !UnwindToCaller(ref unwindState, ref frame))
            {
                return;
            }
        }
    }

    /// <summary>
    /// Steps <paramref name="frame"/> to its caller with the CFI unwinder, carrying
    /// <paramref name="state"/> along. Returns <c>false</c> when no CFI covers the frame's IP.
    /// </summary>
    private static bool UnwindToCaller(ref UnwindState state, ref StackFrame frame)
    {
        if (!UnwindOneFrameWithCFI(ref state, frame.ReturnAddress))
        {
            return false;
        }

        frame.ReturnAddress = state.ReturnAddress;
        frame.FramePointer = state.GetRegValue(CfiArch.FramePointerReg);
        frame.StackPointer = state.StackPointer;
        return true;
    }

    /// <summary>
    /// Steps one frame up the frame-pointer chain: <c>[RBP]</c> holds the caller's saved RBP and
    /// <c>[RBP+8]</c> the return address. Returns <c>false</c> at the bottom of the chain or on a
    /// frame pointer / return address that doesn't look valid.
    /// </summary>
    private static bool UnwindOneFrame(ref StackFrame frame)
    {
        nuint* rbpPtr = (nuint*)frame.FramePointer;
        if (rbpPtr == null || frame.FramePointer < 0x1000)
        {
            Serial.WriteString("[EH] Corrupt frame pointer, stopping stack walk\n");
            return false;
        }

        nuint savedRbp = rbpPtr[0];
        nuint returnAddr = rbpPtr[1];

        // savedRbp == 0 is the bottom of the chain.
        if (savedRbp == 0)
        {
            return false;
        }

        if (returnAddr == 0 || returnAddr < 0x1000)
        {
            Serial.WriteString("[EH] Corrupt return address, stopping stack walk\n");
            return false;
        }

        frame.FramePointer = savedRbp;
        frame.StackPointer = savedRbp + 16;  // approximate RSP after `pop rbp; ret`
        frame.ReturnAddress = returnAddr;
        return true;
    }

    /// <summary>
    /// Looks for the clause that catches <paramref name="ex"/> among those covering
    /// <paramref name="instructionPointer"/>: a typed catch, or a <c>when</c>-filter clause whose
    /// filter funclet (called via <see cref="RhpCallFilterFunclet"/>, against
    /// <paramref name="pRegDisplay"/> when non-null) returns non-zero. Finally and fault clauses do
    /// not catch: pass 2 runs them on the way to the catch. <paramref name="clauseIndex"/> is the
    /// clause's position in the method's clause table. Also appends the method name to the
    /// exception's stack trace.
    /// </summary>
    private static bool TryFindHandler(Exception ex, nuint instructionPointer, nuint framePointer, out EHClause clause, out int clauseIndex, REGDISPLAY* pRegDisplay)
    {
        if (!TryGetFrameClauses(instructionPointer, framePointer, out FrameClauses clauses))
        {
            clause = default;
            clauseIndex = -1;
            return false;
        }

        if (StackTraceMetadata.IsSupported
            && StackTraceMetadata.TryGetMethodNameFromStartAddress(clauses.MethodStart, out string methodName))
        {
            ref string? stackTraceString = ref StackTraceMetadata.GetStackTraceString(ex);
            stackTraceString = stackTraceString is null
                ? methodName
                : $"{stackTraceString}{Environment.NewLine}at {methodName}";
        }

        while (clauses.MoveNext(out clause, out clauseIndex))
        {
            if (clause.ClauseKind == EHClauseKind.EH_CLAUSE_FAULT)
            {
                continue;
            }

            // A typed clause catches its type and the types derived from it, nothing else:
            // `catch (A) {} catch (B) {}` must leave a B to the second clause.
            if (clause.ClauseKind == EHClauseKind.EH_CLAUSE_TYPED
                && Casting.RhTypeCast_IsInstanceOfClass(ex, (Internal.Runtime.MethodTable*)clause.TargetType) is null)
            {
                continue;
            }

            // A `when`-filter clause matches only if its filter funclet returns non-zero, and that
            // needs a register context to run the funclet against.
            if (clause.ClauseKind == EHClauseKind.EH_CLAUSE_FILTER)
            {
                if (pRegDisplay == null || clause.FilterAddress == null)
                {
                    continue;
                }

                nint exceptionPtr = Unsafe.As<Exception, nint>(ref ex);
                if (RhpCallFilterFunclet(exceptionPtr, clause.FilterAddress, pRegDisplay) == 0)
                {
                    continue;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// The EH clauses of the frame at <paramref name="framePointer"/> covering
    /// <paramref name="instructionPointer"/>, past those the dispatch of a funclet running in the
    /// frame went past. Returns <c>false</c> when no LSDA describes the method.
    /// </summary>
    private static bool TryGetFrameClauses(nuint instructionPointer, nuint framePointer, out FrameClauses clauses)
    {
        if (!TryGetMethodLSDA(instructionPointer, out nuint methodStart, out byte* pLSDA))
        {
            clauses = default;
            return false;
        }

        clauses = new FrameClauses(pLSDA, methodStart, (uint)(instructionPointer - methodStart), LastPassedClause(framePointer));
        return true;
    }

    /// <summary>
    /// Try to find the FDE PC-begin and LSDA pointer for a given IP. Delegates to the shared
    /// .eh_frame / LSDA-header parser in <see cref="MethodGcInfoLookup"/>.
    /// </summary>
    private static bool TryGetMethodLSDA(nuint ip, out nuint methodStart, out byte* pLSDA)
        => MethodGcInfoLookup.TryGetMethodLSDA(ip, out methodStart, out pLSDA);

    /// <summary>
    /// Unwind one frame using DWARF CFI: looks up the method's FDE/CIE via the
    /// <c>.eh_frame</c> walker, seeds the CFA + RA rule for function entry, replays the CIE initial
    /// instructions then the FDE's body up to <paramref name="returnAddress"/>, and resolves the
    /// resulting rules into caller-side register values.
    /// </summary>
    internal static bool UnwindOneFrameWithCFI(ref UnwindState state, nuint returnAddress)
    {
        if (!MethodGcInfoLookup.TryGetMethodCFI(returnAddress, out MethodGcInfoLookup.MethodCFIInfo cfi))
        {
            return false;
        }

        if (!DwarfCfiParser.ParseCIE(cfi.pCIE, out int codeAlignFactor, out int dataAlignFactor,
                                     out byte returnAddressReg, out byte* initialInstructions, out byte* initialInstructionsEnd))
        {
            return false;
        }

        // Each frame's FDE describes only its own saves, so rules replayed for the previous
        // frame must not leak into this one: a frameless shim (e.g. RhCollect's two-instruction
        // `push rax; call` body) would inherit its callee's AtCfaOffset rules and "restore"
        // registers from slots its frame doesn't have — handing the GC's precise stack scan and
        // the exception dispatcher garbage register values (return addresses, spilled scratch)
        // as callee-saved state. Reset every rule to SameValue; this FDE re-adds its own saves.
        DwarfCfiParser.InitRegRulesSameValue(ref state);

        // Default CFA + RA rule at function entry. On x64 the RA pseudo-reg lives at CFA-8; on
        // ARM64 LR is a normal reg and SameValue is the seeded default, so the SetRegLocation call
        // is a no-op there, letting the FDE's prologue rules describe where LR was spilled.
        state.CfaRegister = CfiArch.CfaRegAtEntry;
        state.CfaOffset = CfiArch.CfaOffsetAtEntry;
        state.SetRegLocation(returnAddressReg, CfiArch.RaInitRule, CfiArch.RaInitOffset);

        if (initialInstructions != null && initialInstructionsEnd != null)
        {
            DwarfCfiParser.ParseCFIInstructions(initialInstructions, initialInstructionsEnd,
                                                cfi.MethodStart, returnAddress,
                                                codeAlignFactor, dataAlignFactor, ref state);
        }
        DwarfCfiParser.ParseCFIInstructions(cfi.pFDEInstrs, cfi.pFDEInstrsEnd,
                                            cfi.MethodStart, returnAddress,
                                            codeAlignFactor, dataAlignFactor, ref state);

        DwarfCfiParser.ApplyUnwindRules(ref state);
        return true;
    }

    /// <summary>
    /// The EH clauses of one frame whose try region covers its call site, innermost first, read
    /// from the method's LSDA clause table (NativeAOT's encoding: ILCompiler's EncodeEHInfo). The
    /// clauses the dispatch of a funclet running in the frame went past are left out: that
    /// funclet's clause and those listed before it, then the clauses sharing its try region, the
    /// other catches of its try (ILCompiler separates distinct try regions at the same native
    /// offsets with an empty marker clause).
    /// </summary>
    private struct FrameClauses
    {
        public readonly nuint MethodStart;
        private readonly uint _codeOffset;
        private readonly uint _count;
        private byte* _next;
        private int _index;
        private int _lastPassed;
        private uint _passedTryStart;
        private uint _passedTryEnd;

        public FrameClauses(byte* pLSDA, nuint methodStart, uint codeOffset, int lastPassed)
        {
            MethodStart = methodStart;
            _codeOffset = codeOffset;
            _lastPassed = lastPassed;
            _next = FindClauseTable(pLSDA, out _count);
        }

        /// <summary>
        /// Steps to the next clause covering the call site; <paramref name="index"/> is its
        /// position in the clause table. Returns <c>false</c> past the last.
        /// </summary>
        public bool MoveNext(out EHClause clause, out int index)
        {
            while ((uint)_index < _count)
            {
                index = _index++;
                clause = ReadClause(ref _next, MethodStart);

                if (_lastPassed >= 0)
                {
                    if (index <= _lastPassed)
                    {
                        _passedTryStart = clause.TryStartOffset;
                        _passedTryEnd = clause.TryEndOffset;
                        continue;
                    }

                    if (clause.TryStartOffset == _passedTryStart && clause.TryEndOffset == _passedTryEnd)
                    {
                        continue;
                    }

                    _lastPassed = -1;
                }

                if (_codeOffset >= clause.TryStartOffset && _codeOffset < clause.TryEndOffset)
                {
                    return true;
                }
            }

            clause = default;
            index = -1;
            return false;
        }

        /// <summary>
        /// The clause table of the method whose LSDA is <paramref name="pLSDA"/>, with its clause
        /// <paramref name="count"/>; null with a count of 0 when the method has no EH clauses.
        /// </summary>
        private static byte* FindClauseTable(byte* pLSDA, out uint count)
        {
            count = 0;

            byte* p = pLSDA;
            byte unwindBlockFlags = *p++;

            // Skip the funclet→main redirect (present only in a funclet's LSDA).
            if ((unwindBlockFlags & MethodGcInfoLookup.UBF_FUNC_KIND_MASK) != MethodGcInfoLookup.UBF_FUNC_KIND_ROOT)
            {
                p += sizeof(int); // main-LSDA offset
                p += sizeof(int); // method-start offset
            }

            // Skip the associated-data offset if present.
            if ((unwindBlockFlags & MethodGcInfoLookup.UBF_FUNC_HAS_ASSOCIATED_DATA) != 0)
            {
                p += sizeof(int);
            }

            if ((unwindBlockFlags & MethodGcInfoLookup.UBF_FUNC_HAS_EHINFO) == 0)
            {
                return null;
            }

            // Follow the EH-info offset to the clause table.
            byte* pEHInfo = p + *(int*)p;
            count = ReadUnsigned(ref pEHInfo);
            return pEHInfo;
        }

        /// <summary>Reads the clause at <paramref name="p"/> and steps past it.</summary>
        private static EHClause ReadClause(ref byte* p, nuint methodStart)
        {
            EHClause clause = default;
            clause.TryStartOffset = ReadUnsigned(ref p);
            uint tryEndDeltaAndKind = ReadUnsigned(ref p);
            clause.ClauseKind = (EHClauseKind)(tryEndDeltaAndKind & 0x3);
            clause.TryEndOffset = clause.TryStartOffset + (tryEndDeltaAndKind >> 2);
            clause.HandlerAddress = (byte*)(methodStart + ReadUnsigned(ref p));

            switch (clause.ClauseKind)
            {
                case EHClauseKind.EH_CLAUSE_TYPED:
                    byte* pTypeBase = p;
                    clause.TargetType = pTypeBase + ReadInt32(ref p);
                    break;

                case EHClauseKind.EH_CLAUSE_FILTER:
                    clause.FilterAddress = (byte*)(methodStart + ReadUnsigned(ref p));
                    break;
            }

            return clause;
        }
    }

    /// <summary>
    /// Read variable-length encoded unsigned integer (NativePrimitiveDecoder format used in LSDA).
    /// </summary>
    private static uint ReadUnsigned(ref byte* p)
    {
        uint val = *p;
        uint value;

        if ((val & 1) == 0)
        {
            value = val >> 1;
            p += 1;
        }
        else if ((val & 2) == 0)
        {
            value = (val >> 2) | ((uint)*(p + 1) << 6);
            p += 2;
        }
        else if ((val & 4) == 0)
        {
            value = (val >> 3) | ((uint)*(p + 1) << 5) | ((uint)*(p + 2) << 13);
            p += 3;
        }
        else if ((val & 8) == 0)
        {
            value = (val >> 4) | ((uint)*(p + 1) << 4) | ((uint)*(p + 2) << 12) | ((uint)*(p + 3) << 20);
            p += 4;
        }
        else
        {
            value = (uint)(*(p + 1) | (*(p + 2) << 8) | (*(p + 3) << 16) | (*(p + 4) << 24));
            p += 5;
        }

        return value;
    }

    /// <summary>
    /// Read a little-endian 32-bit signed integer (used in LSDA).
    /// </summary>
    private static int ReadInt32(ref byte* p)
    {
        int value = *p | (*(p + 1) << 8) | (*(p + 2) << 16) | (*(p + 3) << 24);
        p += 4;
        return value;
    }

    /// <summary>
    /// Runs a finally (or fault) funclet of the frame at <paramref name="framePointer"/> via
    /// <see cref="RhpCallFinallyFunclet"/>, which returns once the funclet does.
    /// </summary>
    private static void InvokeFinallyHandler(byte* handlerAddress, int clauseIndex, nuint framePointer, REGDISPLAY* pRegDisplay, void* pExInfo)
    {
        if (s_traceThrows)
        {
            Serial.WriteString("[EH] Finally at 0x");
            Serial.WriteHex((nuint)handlerAddress);
            Serial.WriteString("\n");
        }

        // The funclet is program code: an exception it throws, or one thrown and caught below it,
        // is a dispatch of its own rather than a recursive one, and goes past the frames and
        // clauses this dispatch went past. Once the funclet returns, every funclet entered while
        // it ran has returned too.
        ref ExceptionDispatchState state = ref State;
        int runningFuncletCount = state.FuncletCount;
        EnterFunclet(framePointer, clauseIndex, pExInfo);
        state.IsHandlingException = false;
        RhpCallFinallyFunclet(handlerAddress, pRegDisplay);
        state.IsHandlingException = true;
        state.FuncletCount = runningFuncletCount;
    }

    /// <summary>
    /// Hands the in-flight exception to its catch funclet via <see cref="RhpCallCatchFunclet"/>,
    /// which restores the establisher frame's registers, runs the funclet, then resumes the catching
    /// method at the address the funclet returns. Does not return.
    /// </summary>
    private static void InvokeCatchHandler(Exception ex, ref EHClause clause, void* pExInfo, REGDISPLAY* pRegDisplay)
    {
        if (pRegDisplay == null)
        {
            Serial.WriteString("[EH] No register context for the catch funclet\n");
            FailFast("Invalid exception context", ex);
            return;
        }

        nint exceptionPtr = Unsafe.As<Exception, nint>(ref ex);

        // The exception is about to be handled — clear the recursion guard before transferring out
        // (RhpCallCatchFunclet jumps to the resume address and never comes back here).
        State.IsHandlingException = false;
        RhpCallCatchFunclet(exceptionPtr, clause.HandlerAddress, pRegDisplay, pExInfo);
    }

    /// <summary>
    /// Fail fast - terminate the system.
    /// </summary>
    public static void FailFast(string message, Exception? exception = null)
    {
        Serial.WriteString("[FAILFAST] ");
        Serial.WriteString(message);
        Serial.WriteString("\n");

        if (exception is not null)
        {
            Serial.WriteString("Exception: ");
            Serial.WriteString(exception.Message);
            Serial.WriteString("\n");
            if (exception.StackTrace is not null)
            {
                Serial.WriteString("Stack Trace:\n");
                Serial.WriteString(exception.StackTrace);
                Serial.WriteString("\n");
            }
        }

        // Halt the system - infinite loop.
        while (true) { }
    }

    /// <summary>
    /// Create a runtime exception from exception ID.
    /// </summary>
    public static Exception GetRuntimeException(ExceptionIDs id)
    {
        return id switch
        {
            ExceptionIDs.NullReference => new NullReferenceException(),
            ExceptionIDs.DivideByZero => new DivideByZeroException(),
            ExceptionIDs.IndexOutOfRange => new IndexOutOfRangeException(),
            ExceptionIDs.InvalidCast => new InvalidCastException(),
            ExceptionIDs.Overflow => new OverflowException(),
            ExceptionIDs.ArrayTypeMismatch => new ArrayTypeMismatchException(),
            _ => new Exception($"Unknown runtime exception: {id}"),
        };
    }
}
