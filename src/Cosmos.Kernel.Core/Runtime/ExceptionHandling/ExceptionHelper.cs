using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Runtime.GcInfo;
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

    // Guard against recursive exception handling.
    private static bool s_isHandlingException = false;

    // Whether every throw is told on the serial port (its message and where it was thrown), and
    // the handler found for it: for debugging the dispatcher, whose crash would otherwise lose
    // the exception. Off, only an exception nothing catches is told: a line costs ~90 µs per
    // character over a 115200-baud UART, and code that catches what it throws (a script engine
    // running a page) throws dozens of times a second.
    internal static bool TraceThrows;

    // The catch clauses whose funclets are running, recorded just before control transfers to
    // each funclet, with the ExInfo of the exception that entered them. A funclet runs on top of
    // the dispatcher, itself on top of the frames of the throw, so an exception thrown while it
    // runs — a `throw;`, or a new exception — walks back from the funclet through that throw's
    // frames: they are unwound as far as the program goes, and must not catch it, nor must the
    // clause that is running re-enter. A chain (not a single record) because funclets nest: a
    // catch that throws from its handler runs the next catch on top of itself.
    //
    // A record lives as long as its funclet: RhpThrowEx links an ExInfo into a chain for every
    // throw, and RhpCallCatchFunclet unlinks those of the frames it unwinds once the funclet
    // returns, so the funclets still running are those whose ExInfo the chain still holds (see
    // PruneFinishedCatches). Telling a rethrow by its exception object instead let a new
    // exception thrown from a catch re-enter that catch, and a completed catch's object, thrown
    // again later, skip it.
    private const int MaxActiveCatchDepth = 64;
    private static readonly nuint[] s_activeCatchFramePointers = new nuint[MaxActiveCatchDepth];
    private static readonly nuint[] s_activeCatchHandlers = new nuint[MaxActiveCatchDepth];
    private static readonly nuint[] s_activeCatchExInfos = new nuint[MaxActiveCatchDepth];
    private static int s_activeCatchCount;

    private static bool IsActiveCatchClause(nuint framePointer, nuint handlerAddress)
    {
        for (int i = 0; i < s_activeCatchCount; i++)
        {
            if (s_activeCatchFramePointers[i] == framePointer && s_activeCatchHandlers[i] == handlerAddress)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether a frame belongs to a throw whose catch funclet is running: it lies between that
    /// throw's ExInfo, on the stack of <c>RhpThrowEx</c>, and the frame that caught it.
    /// </summary>
    private static bool IsUnwoundFrame(nuint framePointer)
    {
        for (int i = 0; i < s_activeCatchCount; i++)
        {
            if (s_activeCatchExInfos[i] < framePointer && framePointer < s_activeCatchFramePointers[i])
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Drops the records of the catches whose funclets returned: those whose ExInfo the chain
    /// behind <paramref name="pExInfo"/> (this throw's, linked first) no longer holds.
    /// </summary>
    private static void PruneFinishedCatches(void* pExInfo)
    {
        int kept = 0;
        for (int i = 0; i < s_activeCatchCount; i++)
        {
            bool inFlight = false;
            for (nuint* p = pExInfo == null ? null : *(nuint**)pExInfo; p != null; p = *(nuint**)p)
            {
                if ((nuint)p == s_activeCatchExInfos[i])
                {
                    inFlight = true;
                    break;
                }
            }

            if (inFlight)
            {
                s_activeCatchFramePointers[kept] = s_activeCatchFramePointers[i];
                s_activeCatchHandlers[kept] = s_activeCatchHandlers[i];
                s_activeCatchExInfos[kept] = s_activeCatchExInfos[i];
                kept++;
            }
        }

        s_activeCatchCount = kept;
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
    /// REGDISPLAY a filter funclet runs against.
    /// </summary>
    private static partial void PinPass1FrameRegister(REGDISPLAY* regDisplay, nuint framePointer);

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
    /// dispatcher when it hands a REGDISPLAY to a filter/catch funclet.
    /// </summary>
    internal static partial void ProjectRegDisplay(ref UnwindState s, REGDISPLAY* rd);

    /// <summary>
    /// Seed an <see cref="UnwindState"/> from a <see cref="REGDISPLAY"/> and an instruction pointer,
    /// ready for <see cref="UnwindOneFrameWithCFI"/>. All register rules start as <c>SameValue</c>.
    /// </summary>
    internal static partial void SeedUnwindStateFromRegDisplay(ref UnwindState s, REGDISPLAY* rd, nuint ip);

    /// <summary>
    /// Managed entry point for a thrown exception, called from <c>RhThrowEx</c> (the assembly stub)
    /// with the throw-site register context. Walks the stack, runs filters, and transfers to the
    /// catch handler; if nothing catches it, halts.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowExceptionWithContext(Exception ex, nuint throwAddress, nuint throwRbp, nuint throwRsp, void* pExInfo)
    {
        if (ex is null)
        {
            FailFast("Null exception", ex);
            return;
        }

        // Guard against an exception thrown while we're already dispatching one.
        if (s_isHandlingException)
        {
            FailFast("Recursive exception", ex);
            return;
        }
        s_isHandlingException = true;

        PruneFinishedCatches(pExInfo);

        // Before the stack walk, in case the walk crashes.
        if (TraceThrows)
        {
            WriteThrow(ex, throwAddress, throwRbp, throwRsp);
        }

        DispatchExceptionWithContext(ex, throwAddress, throwRbp, throwRsp, pExInfo);

        // DispatchExceptionWithContext transfers to the handler on success; it only returns here
        // when no handler covered the throw.
        if (!TraceThrows)
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
    /// handler (unwinding the register state at each frame so filters can run); pass 2 transfers to
    /// the catch handler via <see cref="InvokeCatchHandler"/>, which does not return. Returns only
    /// when no handler was found (or the throw-site frame pointer was missing).
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

        // Pass 1: walk the frame-pointer chain looking for a handler, unwinding registers as we go.
        StackFrame catchFrame = default;
        EHClause catchClause = default;
        bool foundHandler = false;

        StackFrame frame;
        frame.FramePointer = throwRbp;
        frame.StackPointer = throwRsp;
        frame.ReturnAddress = throwAddress;

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
                PinPass1FrameRegister(pRegDisplay, frame.FramePointer);
                regDisplay.SP = frame.FramePointer;
            }

            // Does this frame have a handler covering the call that threw? Not if it is a frame
            // of a throw whose catch is running, and not that catch's clause again.
            // Probe one byte back into the call: every ReturnAddress here — including frame 0's
            // throwAddress, captured as RhpThrowEx's return address — points at the instruction
            // AFTER the call, and clause try-ends are exclusive, so a call ending a try region
            // would otherwise miss its handler (#387). RyuJIT pads such calls with nop/int3
            // today, making this adjustment behavior-preserving on the current codegen.
            if (!IsUnwoundFrame(frame.FramePointer)
                && TryFindHandler(ex, frame.ReturnAddress - 1, frame.FramePointer, out EHClause clause, pRegDisplay))
            {
                if (TraceThrows)
                {
                    Serial.WriteString("[EH] Handler found at 0x");
                    Serial.WriteHex((nuint)clause.HandlerAddress);
                    Serial.WriteString("\n");
                }

                catchFrame = frame;
                catchClause = clause;
                foundHandler = true;
                break;
            }

            // Capture this frame's IP before stepping up.
            nuint currentFrameIP = frame.ReturnAddress;

            // Step to the caller. CFI (the .eh_frame unwinder) is the source of truth: it works
            // regardless of whether the current method keeps RBP/X29 (RyuJIT omits the frame
            // pointer on methods with no EH, small frames, no localloc, no address-taken locals —
            // the managed equivalent of `-fomit-frame-pointer`; tail-called methods don't even
            // get their own frame). The frame-pointer chain breaks across any such intermediate,
            // so we drive the walk from CFI and reserve `UnwindOneFrame` for the (defensive)
            // case where there is no throw context to seed the unwind state with.
            if (pThrowContext != null)
            {
                if (!UnwindOneFrameWithCFI(ref unwindState, currentFrameIP))
                {
                    break;   // No CFI for this IP — can't unwind further precisely.
                }
                frame.ReturnAddress = unwindState.ReturnAddress;
                frame.FramePointer = unwindState.GetRegValue(CfiArch.FramePointerReg);
                frame.StackPointer = unwindState.StackPointer;
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

        // Build the REGDISPLAY the catch funclet runs with — and that RhpCallCatchFunclet resumes
        // the catching method from once the funclet returns. The CFI walk that landed on the
        // catching frame already reconstructed its body register state at the call that threw, so
        // `unwindState` carries the callee-saved registers and the resume SP directly — that's what
        // `ProjectRegDisplay` copies in (and, on x64, what it wires the pRxx pointers to point at).
        // The frame pointer is overridden separately: `catchFrame.FramePointer` was derived from
        // the same CFI walk, so the two should agree, but we pin it explicitly to match what the
        // funclet expects to find at function entry (`push rbp; mov rbp, rsp` semantics on x64;
        // the establisher's X29 on ARM64).
        if (pThrowContext != null)
        {
            pRegDisplay = &regDisplay;
            ProjectRegDisplay(ref unwindState, pRegDisplay);
            PinPass2RegDisplay(pRegDisplay, catchFrame.FramePointer);
        }

        // Record the clause about to run so an exception thrown from inside its funclet is
        // dispatched past it instead of re-entering it (see s_activeCatch* above).
        if (s_activeCatchCount < MaxActiveCatchDepth)
        {
            s_activeCatchFramePointers[s_activeCatchCount] = catchFrame.FramePointer;
            s_activeCatchHandlers[s_activeCatchCount] = (nuint)catchClause.HandlerAddress;
            s_activeCatchExInfos[s_activeCatchCount] = (nuint)pExInfo;
            s_activeCatchCount++;
        }
        else
        {
            Serial.WriteString("[EH] Too many nested catch funclets to track\n");
        }

        // Pass 2: transfer to the catch handler.
        // TODO: execute finally handlers between the throw and the catch first.
        InvokeCatchHandler(ex, ref catchClause, pExInfo, pRegDisplay);
        // InvokeCatchHandler jumps to the resume address and does not return.
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
    /// Looks for an exception handler covering <paramref name="instructionPointer"/> by parsing the
    /// method's LSDA. <paramref name="pRegDisplay"/> (if non-null) lets a <c>when</c>-filter clause
    /// be evaluated. Also appends the method name to the exception's stack trace.
    /// </summary>
    private static bool TryFindHandler(Exception ex, nuint instructionPointer, nuint framePointer, out EHClause clause, REGDISPLAY* pRegDisplay)
    {
        clause = default;

        if (!TryGetMethodLSDA(instructionPointer, out nuint methodStart, out byte* pLSDA))
        {
            return false;
        }

        if (StackTraceMetadata.IsSupported
            && StackTraceMetadata.TryGetMethodNameFromStartAddress(methodStart, out string methodName))
        {
            ref string? stackTraceString = ref StackTraceMetadata.GetStackTraceString(ex);
            stackTraceString = stackTraceString is null
                ? methodName
                : stackTraceString + Environment.NewLine + "at " + methodName;
        }

        uint codeOffset = (uint)(instructionPointer - methodStart);
        return TryFindHandlerInLSDA(ex, pLSDA, methodStart, codeOffset, framePointer, out clause, pRegDisplay);
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
    /// Parses the LSDA EH-clause table for <paramref name="codeOffset"/> and returns the first
    /// clause that covers it: a typed catch, or a <c>when</c>-filter clause whose filter funclet
    /// (called via <see cref="RhpCallFilterFunclet"/>) returns non-zero. Fault clauses are
    /// recognised but not yet executed. Returns <c>false</c> if no clause matches.
    /// </summary>
    private static bool TryFindHandlerInLSDA(Exception ex, byte* pLSDA, nuint methodStart, uint codeOffset, nuint framePointer, out EHClause clause, REGDISPLAY* pRegDisplay)
    {
        clause = default;

        if (pLSDA == null)
        {
            return false;
        }

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
            return false;
        }

        // Follow the EH-info offset to the clause table.
        int ehInfoOffset = *(int*)p;
        byte* pEHInfo = p + ehInfoOffset;

        uint nClauses = ReadUnsigned(ref pEHInfo);
        for (uint i = 0; i < nClauses; i++)
        {
            uint tryStartOffset = ReadUnsigned(ref pEHInfo);
            uint tryEndDeltaAndKind = ReadUnsigned(ref pEHInfo);

            EHClauseKind kind = (EHClauseKind)(tryEndDeltaAndKind & 0x3);
            uint tryEndOffset = tryStartOffset + (tryEndDeltaAndKind >> 2);

            uint handlerOffset = 0;
            byte* filterAddress = null;
            void* targetType = null;

            switch (kind)
            {
                case EHClauseKind.EH_CLAUSE_TYPED:
                    handlerOffset = ReadUnsigned(ref pEHInfo);
                    byte* pTypeBase = pEHInfo;
                    int typeRelAddr = ReadInt32(ref pEHInfo);
                    targetType = pTypeBase + typeRelAddr;
                    break;

                case EHClauseKind.EH_CLAUSE_FAULT:
                    handlerOffset = ReadUnsigned(ref pEHInfo);
                    break;

                case EHClauseKind.EH_CLAUSE_FILTER:
                    handlerOffset = ReadUnsigned(ref pEHInfo);
                    uint filterOffset = ReadUnsigned(ref pEHInfo);
                    filterAddress = (byte*)(methodStart + filterOffset);
                    break;
            }

            if (codeOffset < tryStartOffset || codeOffset >= tryEndOffset)
            {
                continue;
            }

            // Fault handlers run during unwinding, not as a catch — not implemented yet.
            if (kind == EHClauseKind.EH_CLAUSE_FAULT)
            {
                continue;
            }

            // A clause whose funclet is running must not catch what that funclet throws.
            if (IsActiveCatchClause(framePointer, methodStart + handlerOffset))
            {
                continue;
            }

            // A typed clause catches its type and the types derived from it, nothing else:
            // `catch (A) {} catch (B) {}` must leave a B to the second clause.
            if (kind == EHClauseKind.EH_CLAUSE_TYPED
                && Casting.RhTypeCast_IsInstanceOfClass(ex, (Internal.Runtime.MethodTable*)targetType) is null)
            {
                continue;
            }

            // A `when`-filter clause matches only if its filter funclet returns non-zero, and that
            // needs a register context to run the funclet against.
            if (kind == EHClauseKind.EH_CLAUSE_FILTER)
            {
                if (pRegDisplay == null || filterAddress == null)
                {
                    continue;
                }

                nint exceptionPtr = Unsafe.As<Exception, nint>(ref ex);
                if (RhpCallFilterFunclet(exceptionPtr, filterAddress, pRegDisplay) == 0)
                {
                    continue;
                }
            }

            clause.ClauseKind = kind;
            clause.TryStartOffset = tryStartOffset;
            clause.TryEndOffset = tryEndOffset;
            clause.HandlerAddress = (byte*)(methodStart + handlerOffset);
            clause.FilterAddress = filterAddress;
            clause.TargetType = targetType;
            return true;
        }

        return false;
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
        s_isHandlingException = false;
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
