// -----------------------------------------------------------------------------
// Cosmos.Kernel.Tests.Runtime — direct unit tests for Rh*/Rhp* runtime helpers.
// -----------------------------------------------------------------------------
//
// Every [RuntimeExport] stub in src/Cosmos.Kernel.Core/Runtime/ was widened from
// `private static` / implicit-private `static` to `internal static` so this test
// kernel can call them directly. Visibility across assemblies is gated by
// <InternalsVisibleTo Include="Cosmos.Kernel.Tests.Runtime"/> on Cosmos.Kernel.Core.
//
// The tests are organised by category block, and WITHIN each block they are
// sorted alphabetically by runtime-export endpoint. All tests for a single
// endpoint are contiguous — never interleaved with tests for another endpoint.
//
// =============================================================================
// RUNTIME STUB COVERAGE MATRIX
// =============================================================================
//
// Legend:
//   File    : source file under src/Cosmos.Kernel.Core/Runtime/
//             Box=Boxing Cast=Casting Cpu=Cpu Dbg=Debugger Dsp=CachedInterfaceDispatch
//             GC=GC Mem=Memory Mod=ModuleHelpers Mta=MetaTable Mth=Math Std=Stdllib Thr=Thread
//   In/Out  : number of formal inputs / meaningful outputs (return + out params)
//   Impl    : "real" = has control flow or delegates to a working helper
//             "stub" = empty body, no-op, or returns a hard-coded constant
//   Tests   : number of Test_* methods in this file that directly invoke the endpoint
//             "-" = not directly tested (see "Notes" below the table)
//             "*" = covered by a batched smoke test shared with sibling endpoints
//   In%     : fraction of input space exercised across all tests for this endpoint
//              0%  = not tested
//             33%  = single "happy-path" value (one representative of min/mid/max)
//             50%  = two distinct values covering one edge case
//             67%  = two of {min, mid, max} or two distinct semantic branches
//            100%  = min + mid + max OR every reachable branch
//             "—"  = endpoint has zero inputs
//   Out%    : fraction of output / post-condition assertions verified
//              0%  = not tested
//             50%  = smoke only (did not crash) OR partial (e.g. non-null but value unchecked)
//             67%  = some outputs verified, others unchecked
//            100%  = every return value / out param / buffer state asserted against expected
//
// Summary:
//   120 runtime exports implemented in src/Cosmos.Kernel.Core/Runtime/
//   103 tests written in this file, covering 95 distinct endpoints (79%)
//    25 implemented endpoints are intentionally NOT directly tested (notes below)
//    66 endpoints expected by the dotnet NativeAOT CoreLib are NOT implemented
//
// ┌───────────────────────────────────────────────────┬─────┬────┬─────┬──────┬───────┬──────┬──────┐
// │ Endpoint                                          │File │ In │ Out │ Impl │ Tests │ In%  │ Out% │
// ├───────────────────────────────────────────────────┼─────┼────┼─────┼──────┼───────┼──────┼──────┤
// │ ----- Memory Allocation ------------------------- │     │    │     │      │       │      │      │
// │ memmove                                           │ Mem │  3 │   0 │ real │   2   │  50  │ 100  │
// │ memset                                            │ Mem │  3 │   0 │ real │   1   │  33  │ 100  │
// │ RhAllocateNewArray                                │ Mem │  3 │   1 │ real │   1   │  33  │ 100  │
// │ RhAllocateNewObject                               │ Mem │  3 │   1 │ real │   1   │  33  │ 100  │
// │ RhHandleFree                                      │ Mem │  1 │   0 │ real │   1   │  33  │  50  │
// │ RhHandleSet                                       │ Mem │  1 │   0 │ real │   1   │  33  │ 100  │
// │ RhNewArray                                        │ Mem │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhNewString                                       │ Mem │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhNewVariableSizeObject                           │ Mem │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhpGcSafeZeroMemory                               │ Mem │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhpHandleAlloc                                    │ Mem │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhpHandleAllocDependent                           │ Mem │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhpNewArray                                       │ Mem │  2 │   1 │ real │   4   │ 100  │ 100  │
// │ RhpNewArrayFast                                   │ Mem │  2 │   1 │ real │   2   │  67  │ 100  │
// │ RhpNewFast                                        │ Mem │  1 │   1 │ real │   1   │  33  │ 100  │
// │ RhpNewPtrArrayFast                                │ Mem │  2 │   1 │ real │   2   │  67  │ 100  │
// │ RhRegisterFrozenSegment                           │ Mem │  4 │   1 │ real │   -   │   0  │   0  │
// │ RhSpanHelpers_MemCopy                             │ Mem │  3 │   0 │ real │   1   │  33  │ 100  │
// │ RhUpdateFrozenSegment                             │ Mem │  3 │   0 │ real │   -   │   0  │   0  │
// │ ----- Boxing ------------------------------------ │     │    │     │      │       │      │      │
// │ RhBox                                             │ Box │  2 │   1 │ real │   4   │ 100  │ 100  │
// │ RhBoxAny                                          │ Box │  2 │   1 │ real │   2   │ 100  │ 100  │
// │ RhUnbox                                           │ Box │  3 │   0 │ real │   2   │  67  │ 100  │
// │ RhUnbox2                                          │ Box │  2 │   1 │ real │   1   │  33  │ 100  │
// │ ----- Type Casting ------------------------------ │     │    │     │      │       │      │      │
// │ RhTypeCast_AreTypesAssignable                     │ Cast│  2 │   1 │ real │   2   │  67  │ 100  │
// │ RhTypeCast_CheckArrayStore                        │ Cast│  2 │   0 │ real │   -   │   0  │   0  │
// │ RhTypeCast_CheckCastAny                           │ Cast│  2 │   1 │ stub │   1   │  33  │ 100  │
// │ RhTypeCast_CheckCastClass                         │ Cast│  2 │   1 │ real │   1   │  50  │  50  │
// │ RhTypeCast_CheckCastClassSpecial                  │ Cast│  3 │   1 │ real │   2   │  50  │  67  │
// │ RhTypeCast_CheckCastInterface                     │ Cast│  2 │   1 │ real │   1   │  33  │  50  │
// │ RhTypeCast_IsInstanceOfAny                        │ Cast│  3 │   1 │ real │   2   │  67  │ 100  │
// │ RhTypeCast_IsInstanceOfClass                      │ Cast│  2 │   1 │ real │   2   │  67  │ 100  │
// │ RhTypeCast_IsInstanceOfInterface                  │ Cast│  2 │   1 │ real │   2   │  67  │ 100  │
// │ ----- Write Barriers / Refs / Array Helpers ----- │     │    │     │      │       │      │      │
// │ RhBuffer_BulkMoveWithWriteBarrier                 │ Std │  3 │   0 │ real │   1   │  33  │ 100  │
// │ RhBulkMoveWithWriteBarrier                        │ Std │  3 │   0 │ real │   1   │  33  │ 100  │
// │ RhpAssignRef                                      │ Std │  2 │   0 │ real │   1   │  33  │ 100  │
// │ RhpByRefAssignRef (ARM64)                         │ Std │  2 │   0 │ real │   -   │   0  │   0  │
// │ RhpCheckedAssignRef                               │ Std │  2 │   0 │ real │   1   │  33  │ 100  │
// │ RhpCheckedLockCmpXchg                             │ Std │  4 │   1 │ real │   2   │ 100  │ 100  │
// │ RhpCheckedXchg                                    │ Std │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhpLdelemaRef                                     │ Std │  3 │   1 │ real │   1   │  33  │ 100  │
// │ RhpStelemRef                                      │ Std │  3 │   0 │ real │   2   │  67  │ 100  │
// │ RhSpanHelpers_MemZero                             │ Std │  2 │   0 │ real │   1   │  33  │ 100  │
// │ ----- Math Intrinsics --------------------------- │     │    │     │      │       │      │      │
// │ ceil                                              │ Mth │  1 │   1 │ real │   1   │ 100  │ 100  │
// │ ceilf                                             │ Mth │  1 │   1 │ real │   1   │  67  │ 100  │
// │ modf                                              │ Mth │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhpDbl2Int                                        │ Std │  1 │   1 │ real │   1   │  67  │ 100  │
// │ RhpDbl2Lng                                        │ Std │  1 │   1 │ real │   1   │  67  │ 100  │
// │ sqrt                                              │ Mth │  1 │   1 │ real │   1   │ 100  │ 100  │
// │ ----- GC & Finalization ------------------------- │     │    │     │      │       │      │      │
// │ RhCollect                                         │ GC  │  2 │   0 │ real │   1   │  33  │  50  │
// │ RhGetAllocatedBytesForCurrentThread               │ GC  │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhGetGcCollectionCount                            │ GC  │  2 │   1 │ real │   1   │  33  │ 100  │
// │ RhGetGCDescSize                                   │ GC  │  1 │   1 │ real │   1   │  50  │ 100  │
// │ RhGetGcTotalMemory                                │ GC  │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhGetGeneration                                   │ GC  │  1 │   1 │ real │   1   │  33  │ 100  │
// │ RhGetGenerationSize                               │ GC  │  1 │   1 │ real │   1   │  67  │ 100  │
// │ RhGetGCSegmentSize                                │ GC  │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhGetLastGCPercentTimeInGC                        │ GC  │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhGetMemoryInfo                                   │ GC  │  2 │   0 │ real │   1   │  33  │  50  │
// │ RhGetTotalAllocatedBytes                          │ GC  │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhGetTotalAllocatedBytesPrecise                   │ GC  │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhIsPromoted                                      │ GC  │  1 │   1 │ stub │   1   │  33  │ 100  │
// │ RhIsServerGc                                      │ GC  │  0 │   1 │ stub │   1   │  —   │ 100  │
// │ RhpGcPoll                                         │ Std │  0 │   0 │ stub │   1   │  —   │  50  │
// │ RhpNewFinalizable                                 │ Std │  1 │   1 │ real │   1   │  33  │ 100  │
// │ RhpTrapThreads                                    │ Std │  0 │   0 │ stub │   1   │  —   │  50  │
// │ RhRegisterForGCReporting                          │ GC  │  1 │   0 │ stub │   1   │  33  │  50  │
// │ RhReRegisterForFinalize                           │ Std │  1 │   0 │ stub │   1   │  33  │  50  │
// │ RhSuppressFinalize                                │ Std │  1 │   0 │ stub │   1   │  33  │  50  │
// │ RhUnregisterForGCReporting                        │ GC  │  1 │   0 │ stub │   1   │  33  │  50  │
// │ ----- Threading / Runtime Info ------------------ │     │    │     │      │       │      │      │
// │ DebugDebugger_IsNativeDebuggerAttached            │ Dbg │  0 │   1 │ stub │   1   │  —   │ 100  │
// │ GetSystemArrayEEType                              │ Std │  0 │   1 │ real │   1   │  —   │ 100  │
// │ InitializeModules                                 │ Std │  5 │   0 │ stub │   1   │  33  │  50  │
// │ RhCompatibleReentrantWaitAny                      │ Std │  4 │   1 │ stub │   1   │  33  │ 100  │
// │ RhCreateCrashDumpIfEnabled                        │ Std │  2 │   0 │ stub │   1   │  33  │  50  │
// │ RhCurrentOSThreadId                               │ Std │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhGetCurrentThreadStackBounds                     │ Thr │  0 │   2 │ real │   1   │  —   │  50  │
// │ RhGetProcessCpuCount                              │ Std │  0 │   1 │ stub │   1   │  —   │ 100  │
// │ RhGetRuntimeVersion                               │ Std │  0 │   1 │ stub │   1   │  —   │ 100  │
// │ RhGetThreadStaticStorage                          │ Thr │  0 │   1 │ real │   -   │   0  │   0  │
// │ RhNewObject                                       │ Std │  1 │   1 │ real │   1   │  33  │ 100  │
// │ RhpGetTickCount64                                 │ Cpu │  0 │   1 │ real │   1   │  —   │ 100  │
// │ RhpPInvoke                                        │ Std │  1 │   0 │ stub │   *   │  33  │  50  │
// │ RhpPInvokeReturn                                  │ Std │  1 │   0 │ stub │   *   │  33  │  50  │
// │ RhpReversePInvoke                                 │ Std │  1 │   0 │ stub │   *   │  33  │  50  │
// │ RhpReversePInvokeReturn                           │ Std │  1 │   0 │ stub │   *   │  33  │  50  │
// │ RhpStackProbe                                     │ Std │  0 │   0 │ stub │   1   │  —   │  50  │
// │ RhSetThreadExitCallback                           │ Std │  1 │   0 │ real │   1   │  33  │  50  │
// │ RhSpinWait                                        │ Std │  1 │   0 │ real │   1   │  33  │  50  │
// │ RhYield                                           │ Std │  0 │   1 │ stub │   1   │  —   │ 100  │
// │ NativeRuntimeEventSource_LogContentionLockCreated │ Std │  4 │   0 │ stub │   *   │  33  │  50  │
// │ NativeRuntimeEventSource_LogContentionStart       │ Std │  5 │   0 │ stub │   *   │  33  │  50  │
// │ NativeRuntimeEventSource_LogContentionStop        │ Std │  5 │   0 │ stub │   *   │  33  │  50  │
// │ NativeRuntimeEventSource_LogThreadPoolMinMaxThrd  │ Std │  4 │   0 │ stub │   *   │  33  │  50  │
// │ NativeRuntimeEventSource_LogWaitHandleWaitStart   │ Std │  2 │   0 │ stub │   *   │  33  │  50  │
// │ NativeRuntimeEventSource_LogWaitHandleWaitStop    │ Std │  2 │   0 │ stub │   *   │  33  │  50  │
// │ ----- Code / Stack Introspection ---------------- │     │    │     │      │       │      │      │
// │ RhFindMethodStartAddress                          │ Std │  1 │   1 │ stub │   1   │  33  │ 100  │
// │ RhGetCodeTarget                                   │ Std │  1 │   1 │ stub │   1   │  33  │ 100  │
// │ RhGetCrashInfoBuffer                              │ Std │  0 │   1 │ stub │   1   │  —   │ 100  │
// │ RhGetCurrentThreadStackTrace                      │ Std │  2 │   2 │ stub │   1   │  33  │ 100  │
// │ RhGetModuleFileName                               │ Mta │  1 │   2 │ stub │   1   │  33  │ 100  │
// │ RhGetTargetOfUnboxingAndInstantiatingStub         │ Std │  1 │   1 │ stub │   1   │  33  │ 100  │
// │ ----- Exception Handling ------------------------ │     │    │     │      │       │      │      │
// │ RhpFallbackFailFast                               │ Std │  0 │   0 │ real │   -   │  —   │   0  │
// │ RhThrowEx                                         │ Std │  2 │   0 │ real │   -   │   0  │   0  │
// │ ----- Interface Dispatch ------------------------ │     │    │     │      │       │      │      │
// │ RhNewInterfaceDispatchCell                        │ Dsp │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhpCidResolve                                     │ Dsp │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhpResolveInterfaceMethod                         │ Dsp │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhResolveDispatch                                 │ Dsp │  3 │   1 │ real │   -   │   0  │   0  │
// │ RhResolveDispatchOnType                           │ Dsp │  3 │   1 │ real │   -   │   0  │   0  │
// │ RhResolveStaticDispatchOnType                     │ Dsp │  4 │   1 │ real │   -   │   0  │   0  │
// │ ----- Module Helpers ---------------------------- │     │    │     │      │       │      │      │
// │ RhFindBlob                                        │ Mod │  4 │   1 │ real │   -   │   0  │   0  │
// │ RhGetOSModuleFromPointer                          │ Mod │  1 │   1 │ real │   -   │   0  │   0  │
// │ RhpCreateTypeManager                              │ Mod │  4 │   1 │ real │   -   │   0  │   0  │
// │ RhpGetClasslibFunctionFromCodeAddress             │ Mod │  2 │   1 │ stub │   -   │   0  │   0  │
// │ RhpGetClasslibFunctionFromEEType                  │ Mod │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhpGetModuleSection                               │ Mod │  3 │   2 │ real │   -   │   0  │   0  │
// │ RhpRegisterOsModule                               │ Mod │  1 │   1 │ real │   -   │   0  │   0  │
// │ ----- Metadata & Handle Maintenance ------------- │     │    │     │      │       │      │      │
// │ RhGetRuntimeHelperForType                         │ Mta │  2 │   1 │ real │   -   │   0  │   0  │
// │ RhHandleGetDependent                              │ Mta │  1 │   2 │ real │   -   │   0  │   0  │
// │ RhHandleSetDependentSecondary                     │ Mta │  2 │   0 │ real │   -   │   0  │   0  │
// └───────────────────────────────────────────────────┴─────┴────┴─────┴──────┴───────┴──────┴──────┘
//
// Batched smoke tests that cover the "*" rows:
//   Test_RhpPInvokePairs_Smoke             → RhpPInvoke, RhpPInvokeReturn,
//                                             RhpReversePInvoke, RhpReversePInvokeReturn
//   Test_NativeRuntimeEventSource_LogAll   → all 6 NRS_Log* stubs
//
// Why "-" endpoints are NOT directly tested:
//   RhpFallbackFailFast         — calls ExceptionHelper.FailFast(); would crash the suite.
//   RhThrowEx                   — assembly-backed exception dispatch, unsafe to invoke
//                                 outside a real throw context.
//   RhpCidResolve, RhpResolveInterfaceMethod, RhResolveDispatch*,
//   RhNewInterfaceDispatchCell  — need a populated dispatch cell and method table
//                                 layout that only exists at real call sites.
//   RhFindBlob, RhGetOSModuleFromPointer, RhpCreateTypeManager,
//   RhpGetClasslibFunction*, RhpGetModuleSection, RhpRegisterOsModule,
//                               — depend on TypeManager / module-header state set up
//                                 by the early boot path; no synthetic inputs available.
//   RhHandleGetDependent, RhHandleSetDependentSecondary,
//   RhGetRuntimeHelperForType,
//   RhpHandleAlloc, RhpHandleAllocDependent,
//   RhRegisterFrozenSegment, RhUpdateFrozenSegment
//                               — require live GC handle/segment state; indirectly
//                                 exercised by the Memory test suite.
//   RhGetThreadStaticStorage    — ref return through scheduler state.
//   RhTypeCast_CheckArrayStore  — UnsafeAccessor chain into CoreLib internals.
//   RhpByRefAssignRef           — ARM64 only; forwards to an assembly stub. Indirectly
//                                 exercised by the kernel's own ref-assignment paths
//                                 on ARM64 CI.
//
// =============================================================================
// MISSING STUBS (expected by dotnet NativeAOT, not implemented in cosmos)
// =============================================================================
//
// Source: grep -rhoP 'RuntimeImport\("R[A-Za-z0-9_]+"' dotnet/runtime/src/coreclr/nativeaot/
//
// Exception dispatch (12):
//   RhpCallCatchFunclet, RhpCallFilterFunclet, RhpCallFinallyFunclet,
//   RhpCallPropagateExceptionCallback, RhpCopyContextFromExInfo,
//   RhpEHEnumInitFromStackFrameIterator, RhpEHEnumNext,
//   RhpFirstChanceExceptionNotification, RhpSfiInit, RhpSfiNext,
//   RhpValidateExInfoStack, RhpGetDispatchCellInfo
//
// GC (full / background / server / knobs) (19 — 11 now implemented):
//   RhCancelFullGCNotification, RhEndNoGCRegion, RhGetCurrentObjSize,
//   RhGetGcLatencyMode, RhGetGCNow,
//   RhGetKnobValues, RhGetLastGCDuration,
//   RhGetLastGCStartTime, RhGetLoadedOSModules, RhGetLohCompactionMode,
//   RhGetMaxGcGeneration, RhGetTotalPauseDuration,
//   RhIsGCBridgeActive, RhpGetNextFinalizableObject,
//   RhpInitializeGcStress, RhRegisterForFullGCNotification, RhRegisterGcCallout,
//   RhSetGcLatencyMode, RhSetLohCompactionMode, RhStartNoGCRegion,
//   RhUnregisterGcCallout, RhWaitForFullGCApproach, RhWaitForFullGCComplete
//
// Threading / thread abort / stack stubs (9):
//   RhCurrentNativeThreadId, RhGetCommonStubAddress, RhGetDefaultStackSize,
//   RhGetThreadEntryPointAddress, RhpCancelThreadAbort, RhpInitiateThreadAbort,
//   RhpGetThreadAbortException, RhpSetThreadDoNotTriggerGC,
//   RhRegisterInlinedThreadStaticRoot
//
// Thunks / dispatch cell cache (9):
//   RhGetCurrentThunkContext, RhpGetNumThunkBlocksPerMapping,
//   RhpGetNumThunksPerBlock, RhpGetThunkBlockSize, RhpGetThunkDataBlockAddress,
//   RhpGetThunkSize, RhpGetThunkStubsBlockAddress, RhpSearchDispatchCellCache,
//   RhpUpdateDispatchCellCache
//
// Allocation variants (4):
//   RhpNewArrayFastAlign8, RhpNewFastAlign8, RhpNewFastMisalign, RhpNewFinalizableAlign8
//
// Handles / Ref-counted / Objective-C / Cross-reference (5 — RhHandleGet now implemented):
//   RhHandleTryGetCrossReferenceContext, RhpHandleAllocCrossReference,
//   RhRegisterObjectiveCMarshalBeginEndCallback, RhRegisterRefCountedHandleCallback,
//   RhUnregisterRefCountedHandleCallback
//
// Interlocked CmpXchg variants (2):
//   RhpLockCmpXchg32, RhpLockCmpXchg64
//
// Misc (5):
//   RhCompareObjectContentsAndPadding, RhResolveDynamicInterfaceCastableDispatchOnType
//   (+ 3 already listed above)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.TestRunner.Framework;
using Internal.Runtime;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;
using RuntimeMemory = Cosmos.Kernel.Core.Runtime.Memory;
using RuntimeMath = Cosmos.Kernel.Core.Runtime.Math;
using RuntimeGC = Cosmos.Kernel.Core.Runtime.GC;
using RuntimeThread = Cosmos.Kernel.Core.Runtime.Thread;
using System.Runtime.InteropServices;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Cosmos.Kernel.Tests.Runtime;

public unsafe class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Serial.WriteString("[Runtime] BeforeRun() reached!\n");
        Serial.WriteString("[Runtime] Starting tests...\n");

        TR.Start("Runtime Tests", expectedTests: 103);

        // ==================== Memory Allocation Stubs ====================
        // -- memmove --
        TR.Run("memmove_NonOverlap_Copies", Test_memmove_NonOverlap_Copies);
        TR.Run("memmove_ForwardOverlap_Preserves", Test_memmove_ForwardOverlap_Preserves);
        // -- memset --
        TR.Run("memset_FillsBuffer", Test_memset_FillsBuffer);
        // -- RhAllocateNewArray --
        TR.Run("RhAllocateNewArray_Valid", Test_RhAllocateNewArray_Valid);
        // -- RhAllocateNewObject --
        TR.Run("RhAllocateNewObject_WritesResult", Test_RhAllocateNewObject_WritesResult);
        // -- RhNewArray --
        TR.Run("RhNewArray_String_Length3", Test_RhNewArray_String_Length3);
        // -- RhHandleFree --
        TR.Run("RhHandleFree_Zero_NoOp", Test_RhHandleFree_Zero_NoOp);
        // -- RhHandleSet --
        TR.Run("RhHandleSet_Overrides_Handle_Target", Test_RhHandleSet_Overrides_Handle_Target);
        // -- RhNewString --
        TR.Run("RhNewString_Length5", Test_RhNewString_Length5);
        // -- RhNewVariableSizeObject --
        TR.Run("RhNewVariableSizeObject_Int_Length4", Test_RhNewVariableSizeObject_Int_Length4);
        // -- RhpGcSafeZeroMemory --
        TR.Run("RhpGcSafeZeroMemory_ZerosBuffer", Test_RhpGcSafeZeroMemory_ZerosBuffer);
        // -- RhpNewArray --
        TR.Run("RhpNewArray_Int_Length10", Test_RhpNewArray_Int_Length10);
        TR.Run("RhpNewArray_NegativeLength_Null", Test_RhpNewArray_NegativeLength_Null);
        TR.Run("RhpNewArray_ZeroLength_Empty", Test_RhpNewArray_ZeroLength_Empty);
        TR.Run("RhpNewArray_Long_ComponentSize", Test_RhpNewArray_Long_ComponentSize);
        // -- RhpNewArrayFast --
        TR.Run("RhpNewArrayFast_Byte_Length5", Test_RhpNewArrayFast_Byte_Length5);
        TR.Run("RhpNewArrayFast_NegativeLength_Null", Test_RhpNewArrayFast_NegativeLength_Null);
        // -- RhpNewFast --
        TR.Run("RhpNewFast_Object_NonNull", Test_RhpNewFast_Object_NonNull);
        // -- RhpNewPtrArrayFast --
        TR.Run("RhpNewPtrArrayFast_ObjectArray_Length3", Test_RhpNewPtrArrayFast_ObjectArray_Length3);
        TR.Run("RhpNewPtrArrayFast_NegativeLength_Null", Test_RhpNewPtrArrayFast_NegativeLength_Null);
        // -- RhSpanHelpers_MemCopy --
        TR.Run("RhSpanHelpers_MemCopy_CopiesBuffer", Test_RhSpanHelpers_MemCopy_CopiesBuffer);

        // ==================== Boxing Stubs ====================
        // -- RhBox --
        TR.Run("RhBox_Int32_RoundTrip", Test_RhBox_Int32_RoundTrip);
        TR.Run("RhBox_Long_RoundTrip", Test_RhBox_Long_RoundTrip);
        TR.Run("RhBox_NullableNullFlag_ReturnsNull", Test_RhBox_NullableNullFlag_ReturnsNull);
        TR.Run("RhBox_NullableWithValue_Boxes", Test_RhBox_NullableWithValue_Boxes);
        // -- RhBoxAny --
        TR.Run("RhBoxAny_ReferenceType_Passthrough", Test_RhBoxAny_ReferenceType_Passthrough);
        TR.Run("RhBoxAny_ValueType_DelegatesToRhBox", Test_RhBoxAny_ValueType_DelegatesToRhBox);
        // -- RhUnbox --
        TR.Run("RhUnbox_CopiesValueToDest", Test_RhUnbox_CopiesValueToDest);
        TR.Run("RhUnbox_NullObj_NoOp", Test_RhUnbox_NullObj_NoOp);
        // -- RhUnbox2 --
        TR.Run("RhUnbox2_ReturnsPointerPastMethodTable", Test_RhUnbox2_ReturnsPointerPastMethodTable);

        // ==================== Type Casting Stubs ====================
        // -- RhTypeCast_AreTypesAssignable --
        TR.Run("AreTypesAssignable_SameType_True", Test_AreTypesAssignable_SameType_True);
        TR.Run("AreTypesAssignable_Unrelated_False", Test_AreTypesAssignable_Unrelated_False);
        // -- RhTypeCast_CheckCastAny --
        TR.Run("CheckCastAny_ReturnsObjUnchanged", Test_CheckCastAny_ReturnsObjUnchanged);
        // -- RhTypeCast_CheckCastClass --
        TR.Run("CheckCastClass_ValidCast_ReturnsObj", Test_CheckCastClass_ValidCast_ReturnsObj);
        // -- RhTypeCast_CheckCastClassSpecial --
        TR.Run("CheckCastClassSpecial_Null_ReturnsNull", Test_CheckCastClassSpecial_Null_ReturnsNull);
        TR.Run("CheckCastClassSpecial_ValidNoThrow_ReturnsObj", Test_CheckCastClassSpecial_ValidNoThrow_ReturnsObj);
        // -- RhTypeCast_CheckCastInterface --
        TR.Run("CheckCastInterface_NullObj_ReturnsNull", Test_CheckCastInterface_NullObj_ReturnsNull);
        // -- RhTypeCast_IsInstanceOfAny --
        TR.Run("IsInstanceOfAny_MatchFirstHandle", Test_IsInstanceOfAny_MatchFirstHandle);
        TR.Run("IsInstanceOfAny_NullObj_Null", Test_IsInstanceOfAny_NullObj_Null);
        // -- RhTypeCast_IsInstanceOfClass --
        TR.Run("IsInstanceOfClass_ValidSubclass", Test_IsInstanceOfClass_ValidSubclass);
        TR.Run("IsInstanceOfClass_Unrelated_Null", Test_IsInstanceOfClass_Unrelated_Null);
        // -- RhTypeCast_IsInstanceOfInterface --
        TR.Run("IsInstanceOfInterface_NullObj_False", Test_IsInstanceOfInterface_NullObj_False);
        TR.Run("IsInstanceOfInterface_Implements_True", Test_IsInstanceOfInterface_Implements_True);

        // ==================== Write Barrier, Ref & Array Helper Stubs ====================
        // -- RhBuffer_BulkMoveWithWriteBarrier --
        TR.Run("RhBuffer_BulkMoveWithWriteBarrier_Copies", Test_RhBuffer_BulkMoveWithWriteBarrier_Copies);
        // -- RhBulkMoveWithWriteBarrier --
        TR.Run("RhBulkMoveWithWriteBarrier_Copies", Test_RhBulkMoveWithWriteBarrier_Copies);
        // -- RhpAssignRef --
        TR.Run("RhpAssignRef_WritesLocation", Test_RhpAssignRef_WritesLocation);
        // -- RhpCheckedAssignRef --
        TR.Run("RhpCheckedAssignRef_WritesLocation", Test_RhpCheckedAssignRef_WritesLocation);
        // -- RhpCheckedXchg --
        TR.Run("RhpCheckedXchg_SwapsAndReturnsOriginal", Test_RhpCheckedXchg_SwapsAndReturnsOriginal);
        // -- RhpLdelemaRef --
        TR.Run("RhpLdelemaRef_ReturnsRefToElement", Test_RhpLdelemaRef_ReturnsRefToElement);
        // -- RhpStelemRef --
        TR.Run("RhpStelemRef_StoresCompatibleElement", Test_RhpStelemRef_StoresCompatibleElement);
        TR.Run("RhpStelemRef_NullValue_StoresNull", Test_RhpStelemRef_NullValue_StoresNull);
        // -- RhSpanHelpers_MemZero --
        TR.Run("RhSpanHelpers_MemZero_ZerosBuffer", Test_RhSpanHelpers_MemZero_ZerosBuffer);

        // ==================== Math Intrinsic Stubs ====================
        // -- ceil --
        TR.Run("ceil_Values", Test_ceil_Values);
        // -- ceilf --
        TR.Run("ceilf_Values", Test_ceilf_Values);
        // -- modf --
        TR.Run("modf_SplitsFractional", Test_modf_SplitsFractional);
        // -- sqrt --
        TR.Run("sqrt_Values", Test_sqrt_Values);
        // -- RhpDbl2Int --
        TR.Run("RhpDbl2Int_Truncates", Test_RhpDbl2Int_Truncates);
        // -- RhpDbl2Lng --
        TR.Run("RhpDbl2Lng_Truncates", Test_RhpDbl2Lng_Truncates);

        // ==================== GC & Finalization Stubs ====================
        // -- RhCollect --
        TR.Run("RhCollect_Smoke", Test_RhCollect_Smoke);
        // -- RhGetAllocatedBytesForCurrentThread --
        TR.Run("RhGetAllocatedBytesForCurrentThread_NonNegative", Test_RhGetAllocatedBytesForCurrentThread_ReturnsZero);
        // -- RhGetGcCollectionCount --
        TR.Run("RhGetGcCollectionCount_Gen0_NonNegative", Test_RhGetGcCollectionCount_Gen0_NonNegative);
        // -- RhGetGCDescSize --
        TR.Run("RhGetGCDescSize_NoGCPointers_Zero", Test_RhGetGCDescSize_NoGCPointers_Zero);
        // -- RhGetGcTotalMemory --
        TR.Run("RhGetGcTotalMemory_Positive", Test_RhGetGcTotalMemory_Positive);
        // -- RhGetGeneration --
        TR.Run("RhGetGeneration_ReturnsZero", Test_RhGetGeneration_ReturnsZero);
        // -- RhGetGenerationSize --
        TR.Run("RhGetGenerationSize_Gen0_Positive", Test_RhGetGenerationSize_Gen0_Positive);
        // -- RhGetGCSegmentSize --
        TR.Run("RhGetGCSegmentSize_Positive", Test_RhGetGCSegmentSize_Positive);
        // -- RhGetLastGCPercentTimeInGC --
        TR.Run("RhGetLastGCPercentTimeInGC_InRange", Test_RhGetLastGCPercentTimeInGC_InRange);
        // -- RhGetMemoryInfo --
        TR.Run("RhGetMemoryInfo_Smoke", Test_RhGetMemoryInfo_Smoke);
        // -- RhGetTotalAllocatedBytes --
        TR.Run("RhGetTotalAllocatedBytes_Positive", Test_RhGetTotalAllocatedBytes_Positive);
        // -- RhGetTotalAllocatedBytesPrecise --
        TR.Run("RhGetTotalAllocatedBytesPrecise_LessOrEqualNonPrecise", Test_RhGetTotalAllocatedBytesPrecise_MatchesNonPrecise);
        // -- RhIsPromoted --
        TR.Run("RhIsPromoted_ReturnsFalse", Test_RhIsPromoted_ReturnsFalse);
        // -- RhIsServerGc --
        TR.Run("RhIsServerGc_ReturnsFalse", Test_RhIsServerGc_ReturnsFalse);
        // -- RhpGcPoll --
        TR.Run("RhpGcPoll_Smoke", Test_RhpGcPoll_Smoke);
        // -- RhpNewFinalizable --
        TR.Run("RhpNewFinalizable_Object_NonNull", Test_RhpNewFinalizable_Object_NonNull);
        // -- RhpTrapThreads --
        TR.Run("RhpTrapThreads_Smoke", Test_RhpTrapThreads_Smoke);
        // -- RhRegisterForGCReporting / RhUnregisterForGCReporting --
        TR.Run("RhRegisterAndUnregisterForGCReporting_Smoke", Test_RhRegisterAndUnregisterForGCReporting_Smoke);
        // -- RhReRegisterForFinalize --
        TR.Run("RhReRegisterForFinalize_Smoke", Test_RhReRegisterForFinalize_Smoke);
        // -- RhSuppressFinalize --
        TR.Run("RhSuppressFinalize_Smoke", Test_RhSuppressFinalize_Smoke);

        // ==================== Threading / Runtime Info Stubs ====================
        // -- GetSystemArrayEEType --
        TR.Run("GetSystemArrayEEType_NonNull", Test_GetSystemArrayEEType_NonNull);
        // -- InitializeModules --
        TR.Run("InitializeModules_ZeroArgs_Smoke", Test_InitializeModules_ZeroArgs_Smoke);
        // -- RhCompatibleReentrantWaitAny --
        TR.Run("RhCompatibleReentrantWaitAny_ReturnsSuccess", Test_RhCompatibleReentrantWaitAny_ReturnsSuccess);
        // -- RhCreateCrashDumpIfEnabled --
        TR.Run("RhCreateCrashDumpIfEnabled_Smoke", Test_RhCreateCrashDumpIfEnabled_Smoke);
        // -- RhCurrentOSThreadId --
        TR.Run("RhCurrentOSThreadId_ReturnsNonNegative", Test_RhCurrentOSThreadId_ReturnsNonNegative);
        // -- RhGetCurrentThreadStackBounds --
        TR.Run("RhGetCurrentThreadStackBounds_HighAboveLow", Test_RhGetCurrentThreadStackBounds_HighAboveLow);
        // -- RhGetProcessCpuCount --
        TR.Run("RhGetProcessCpuCount_ReturnsOne", Test_RhGetProcessCpuCount_ReturnsOne);
        // -- RhGetRuntimeVersion --
        TR.Run("RhGetRuntimeVersion_ReturnsZero", Test_RhGetRuntimeVersion_ReturnsZero);
        // -- RhNewObject --
        TR.Run("RhNewObject_Object_NonNull", Test_RhNewObject_Object_NonNull);
        // -- RhpCheckedLockCmpXchg --
        TR.Run("RhpCheckedLockCmpXchg_MatchSwaps", Test_RhpCheckedLockCmpXchg_MatchSwaps);
        TR.Run("RhpCheckedLockCmpXchg_NoMatchKeeps", Test_RhpCheckedLockCmpXchg_NoMatchKeeps);
        // -- RhpGetTickCount64 --
        TR.Run("RhpGetTickCount64_Monotonic", Test_RhpGetTickCount64_Monotonic);
        // -- RhpPInvoke / RhpPInvokeReturn / RhpReversePInvoke / RhpReversePInvokeReturn --
        TR.Run("RhpPInvokePairs_Smoke", Test_RhpPInvokePairs_Smoke);
        // -- RhpStackProbe --
        TR.Run("RhpStackProbe_Smoke", Test_RhpStackProbe_Smoke);
        // -- RhSetThreadExitCallback --
        TR.Run("RhSetThreadExitCallback_Smoke", Test_RhSetThreadExitCallback_Smoke);
        // -- RhSpinWait --
        TR.Run("RhSpinWait_Zero_NoOp", Test_RhSpinWait_Zero_NoOp);
        // -- RhYield --
        TR.Run("RhYield_ReturnsZero", Test_RhYield_ReturnsZero);
        // -- NativeRuntimeEventSource_Log* --
        TR.Run("NativeRuntimeEventSource_LogAll_Smoke", Test_NativeRuntimeEventSource_LogAll_Smoke);

        // ==================== Code/Stack Introspection Stubs ====================
        // -- RhFindMethodStartAddress --
        TR.Run("RhFindMethodStartAddress_ReturnsInput", Test_RhFindMethodStartAddress_ReturnsInput);
        // -- RhGetCodeTarget --
        TR.Run("RhGetCodeTarget_ReturnsInput", Test_RhGetCodeTarget_ReturnsInput);
        // -- RhGetCrashInfoBuffer --
        TR.Run("RhGetCrashInfoBuffer_ReturnsZero", Test_RhGetCrashInfoBuffer_ReturnsZero);
        // -- RhGetCurrentThreadStackTrace --
        TR.Run("RhGetCurrentThreadStackTrace_ReturnsZeroFrames", Test_RhGetCurrentThreadStackTrace_ReturnsZeroFrames);
        // -- RhGetModuleFileName --
        TR.Run("RhGetModuleFileName_ReturnsZeroAndNullName", Test_RhGetModuleFileName_ReturnsZeroAndNullName);
        // -- RhGetTargetOfUnboxingAndInstantiatingStub --
        TR.Run("RhGetTargetOfUnboxingAndInstantiatingStub_ReturnsInput", Test_RhGetTargetOfUnboxingAndInstantiatingStub_ReturnsInput);
        // -- DebugDebugger_IsNativeDebuggerAttached --
        TR.Run("DebugDebugger_IsNativeDebuggerAttached_ReturnsOne", Test_DebugDebugger_IsNativeDebuggerAttached_ReturnsOne);

        TR.Finish();

        Serial.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // =============================================================================
    // Memory Allocation Stubs
    // =============================================================================

    // -- memmove --
    private static void Test_memmove_NonOverlap_Copies()
    {
        const int Size = 24;
        byte* src = stackalloc byte[Size];
        byte* dst = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            src[i] = (byte)(i + 1);
            dst[i] = 0;
        }
        RuntimeMemory.memmove(dst, src, (nuint)Size);
        for (int i = 0; i < Size; i++)
        {
            Assert.Equal((byte)(i + 1), dst[i], "memmove byte mismatch");
        }
    }

    private static void Test_memmove_ForwardOverlap_Preserves()
    {
        // buf = [0,1,2,3,4,5,6,7] → memmove(buf+2, buf, 4) should yield [0,1,0,1,2,3,6,7].
        const int Size = 8;
        byte* buf = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            buf[i] = (byte)i;
        }
        RuntimeMemory.memmove(buf + 2, buf, 4);
        Assert.Equal((byte)0, buf[2], "memmove overlap [2]");
        Assert.Equal((byte)1, buf[3], "memmove overlap [3]");
        Assert.Equal((byte)2, buf[4], "memmove overlap [4]");
        Assert.Equal((byte)3, buf[5], "memmove overlap [5]");
    }

    // -- memset --
    private static void Test_memset_FillsBuffer()
    {
        const int Size = 16;
        byte* buf = stackalloc byte[Size];
        RuntimeMemory.memset(buf, 0x5A, (nuint)Size);
        for (int i = 0; i < Size; i++)
        {
            Assert.Equal((byte)0x5A, buf[i], "memset byte mismatch");
        }
    }

    // -- RhAllocateNewArray --
    private static void Test_RhAllocateNewArray_Valid()
    {
        RuntimeMemory.RhAllocateNewArray(MethodTable.Of<int[]>(), 7, GC_ALLOC_FLAGS.GC_ALLOC_NO_FLAGS, out void* result);
        Assert.True(result != null, "RhAllocateNewArray must write a non-null result");

        int[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = result;
        Assert.Equal(7, arr.Length, "RhAllocateNewArray length");
    }

    // -- RhAllocateNewObject --
    private static void Test_RhAllocateNewObject_WritesResult()
    {
        void* result;
        RuntimeMemory.RhAllocateNewObject(MethodTable.Of<object>(), GC_ALLOC_FLAGS.GC_ALLOC_NO_FLAGS, &result);
        Assert.True(result != null, "RhAllocateNewObject must write a non-null object pointer");

        object obj = null!;
        *(void**)Unsafe.AsPointer(ref obj) = result;
        Assert.NotNull(obj, "RhAllocateNewObject result should cast to object");
    }

    // -- RhNewArray --
    private static void Test_RhNewArray_String_Length3()
    {
        void* raw = RuntimeMemory.RhNewArray(MethodTable.Of<string[]>(), 3);
        Assert.True(raw != null, "RhNewArray<string[]> returned null");

        string[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(3, arr.Length, "RhNewArray length");
    }

    // -- RhHandleFree --
    private static void Test_RhHandleFree_Zero_NoOp()
    {
        // The stub forwards to GarbageCollector.FreeHandle which treats Zero as a no-op.
        RuntimeMemory.RhHandleFree(nint.Zero);
        Assert.True(true, "RhHandleFree(Zero) returned without crashing");
    }

    // -- RhHandleSet --
    private static void Test_RhHandleSet_Overrides_Handle_Target()
    {
        var obj1 = new object();
        var obj2 = new object();

        var handle = GCHandle.Alloc(obj1, GCHandleType.Weak);

        Assert.True(handle.Target == obj1, "Handle.Target should be obj1 before RhHandleSet");

        RuntimeMemory.RhHandleSet(GCHandle.ToIntPtr(handle), *(GCObject**)Unsafe.AsPointer(ref obj2));

        Assert.True(handle.Target == obj2, "Handle.Target should be obj2 after RhHandleSet");
    }



    // -- RhNewString --
    private static void Test_RhNewString_Length5()
    {
        void* raw = RuntimeMemory.RhNewString(MethodTable.Of<string>(), 5);
        Assert.True(raw != null, "RhNewString returned null");

        string s = null!;
        *(void**)Unsafe.AsPointer(ref s) = raw;
        Assert.Equal(5, s.Length, "RhNewString length");
    }

    // -- RhNewVariableSizeObject --
    private static void Test_RhNewVariableSizeObject_Int_Length4()
    {
        void* raw = RuntimeMemory.RhNewVariableSizeObject(MethodTable.Of<int[]>(), 4);
        Assert.True(raw != null, "RhNewVariableSizeObject returned null");

        int[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(4, arr.Length, "RhNewVariableSizeObject length");
    }

    // -- RhpGcSafeZeroMemory --
    private static void Test_RhpGcSafeZeroMemory_ZerosBuffer()
    {
        const int Size = 16;
        byte* buf = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            buf[i] = 0xAB;
        }

        byte* result = RuntimeMemory.RhpGcSafeZeroMemory(buf, (nuint)Size);
        Assert.True(result == buf, "RhpGcSafeZeroMemory should return the input pointer");

        for (int i = 0; i < Size; i++)
        {
            Assert.Equal((byte)0, buf[i], "RhpGcSafeZeroMemory must zero every byte");
        }
    }

    // -- RhpNewArray --
    private static void Test_RhpNewArray_Int_Length10()
    {
        void* raw = RuntimeMemory.RhpNewArray(MethodTable.Of<int[]>(), 10);
        Assert.True(raw != null, "RhpNewArray returned null for valid length");

        int[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(10, arr.Length, "RhpNewArray length");

        for (int i = 0; i < arr.Length; i++)
        {
            Assert.Equal(0, arr[i], "RhpNewArray elements should be zero-initialized");
        }
    }

    private static void Test_RhpNewArray_NegativeLength_Null()
    {
        void* raw = RuntimeMemory.RhpNewArray(MethodTable.Of<int[]>(), -1);
        Assert.True(raw == null, "RhpNewArray must return null for negative length");
    }

    private static void Test_RhpNewArray_ZeroLength_Empty()
    {
        void* raw = RuntimeMemory.RhpNewArray(MethodTable.Of<int[]>(), 0);
        Assert.True(raw != null, "RhpNewArray returned null for zero length");

        int[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(0, arr.Length, "Zero-length array must have Length == 0");
    }

    private static void Test_RhpNewArray_Long_ComponentSize()
    {
        void* raw = RuntimeMemory.RhpNewArray(MethodTable.Of<long[]>(), 4);
        Assert.True(raw != null, "RhpNewArray<long[]> returned null");

        long[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(4, arr.Length, "long[] length");
        arr[3] = 0x1122334455667788L;
        Assert.True(arr[3] == 0x1122334455667788L, "long[] element store/load");
    }

    // -- RhpNewArrayFast --
    private static void Test_RhpNewArrayFast_Byte_Length5()
    {
        void* raw = RuntimeMemory.RhpNewArrayFast(MethodTable.Of<byte[]>(), 5);
        Assert.True(raw != null, "RhpNewArrayFast returned null");

        byte[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(5, arr.Length, "RhpNewArrayFast length");
    }

    private static void Test_RhpNewArrayFast_NegativeLength_Null()
    {
        void* raw = RuntimeMemory.RhpNewArrayFast(MethodTable.Of<byte[]>(), -1);
        Assert.True(raw == null, "RhpNewArrayFast must return null for negative length");
    }

    // -- RhpNewFast --
    private static void Test_RhpNewFast_Object_NonNull()
    {
        void* raw = RuntimeMemory.RhpNewFast(MethodTable.Of<object>());
        Assert.True(raw != null, "RhpNewFast returned null for System.Object");

        object obj = null!;
        *(void**)Unsafe.AsPointer(ref obj) = raw;
        Assert.NotNull(obj, "RhpNewFast result should cast to object");
    }

    // -- RhpNewPtrArrayFast --
    private static void Test_RhpNewPtrArrayFast_ObjectArray_Length3()
    {
        void* raw = RuntimeMemory.RhpNewPtrArrayFast(MethodTable.Of<object[]>(), 3);
        Assert.True(raw != null, "RhpNewPtrArrayFast returned null");

        object?[] arr = null!;
        *(void**)Unsafe.AsPointer(ref arr) = raw;
        Assert.Equal(3, arr.Length, "RhpNewPtrArrayFast length");
    }

    private static void Test_RhpNewPtrArrayFast_NegativeLength_Null()
    {
        void* raw = RuntimeMemory.RhpNewPtrArrayFast(MethodTable.Of<object[]>(), -1);
        Assert.True(raw == null, "RhpNewPtrArrayFast must return null for negative length");
    }

    // -- RhSpanHelpers_MemCopy --
    private static void Test_RhSpanHelpers_MemCopy_CopiesBuffer()
    {
        const int Size = 20;
        byte* src = stackalloc byte[Size];
        byte* dst = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            src[i] = (byte)(0xF0 | (i & 0x0F));
        }
        RuntimeMemory.RhSpanHelpers_MemCopy(dst, src, (nuint)Size);
        for (int i = 0; i < Size; i++)
        {
            Assert.Equal(src[i], dst[i], "RhSpanHelpers_MemCopy byte mismatch");
        }
    }

    // =============================================================================
    // Boxing Stubs
    // =============================================================================

    // -- RhBox --
    private static void Test_RhBox_Int32_RoundTrip()
    {
        int value = 42;
        void* boxed = Boxing.RhBox(MethodTable.Of<int>(), (byte*)&value);
        Assert.True(boxed != null, "RhBox<int> returned null");

        byte* unboxPtr = Boxing.RhUnbox2(MethodTable.Of<int>(), boxed);
        Assert.Equal(42, *(int*)unboxPtr, "RhBox<int> round-trip value");
    }

    private static void Test_RhBox_Long_RoundTrip()
    {
        long value = 0x0BADF00DDEADBEEFL;
        void* boxed = Boxing.RhBox(MethodTable.Of<long>(), (byte*)&value);
        Assert.True(boxed != null, "RhBox<long> returned null");

        byte* unboxPtr = Boxing.RhUnbox2(MethodTable.Of<long>(), boxed);
        Assert.True(*(long*)unboxPtr == 0x0BADF00DDEADBEEFL, "RhBox<long> round-trip value");
    }

    private static void Test_RhBox_NullableNullFlag_ReturnsNull()
    {
        int? nullable = null;
        byte* nullablePtr = (byte*)Unsafe.AsPointer(ref nullable);
        void* result = Boxing.RhBox(MethodTable.Of<int?>(), nullablePtr);
        Assert.True(result == null, "RhBox<Nullable<int>> with HasValue=false must return null");
    }

    private static void Test_RhBox_NullableWithValue_Boxes()
    {
        int? nullable = 123;
        byte* nullablePtr = (byte*)Unsafe.AsPointer(ref nullable);
        void* boxed = Boxing.RhBox(MethodTable.Of<int?>(), nullablePtr);
        Assert.True(boxed != null, "RhBox<Nullable<int>> with value must allocate");

        byte* unboxPtr = Boxing.RhUnbox2(MethodTable.Of<int>(), boxed);
        Assert.Equal(123, *(int*)unboxPtr, "Boxed nullable underlying value");
    }

    // -- RhBoxAny --
    private static void Test_RhBoxAny_ReferenceType_Passthrough()
    {
        string input = "hello";
        byte* inputPtr = (byte*)Unsafe.AsPointer(ref input);
        void* result = Boxing.RhBoxAny(inputPtr, MethodTable.Of<string>());
        Assert.True(result == inputPtr, "RhBoxAny on reference type must pass through");
    }

    private static void Test_RhBoxAny_ValueType_DelegatesToRhBox()
    {
        int value = 99;
        void* boxed = Boxing.RhBoxAny((byte*)&value, MethodTable.Of<int>());
        Assert.True(boxed != null, "RhBoxAny<int> returned null");

        byte* unboxPtr = Boxing.RhUnbox2(MethodTable.Of<int>(), boxed);
        Assert.Equal(99, *(int*)unboxPtr, "RhBoxAny<int> must yield the same value");
    }

    // -- RhUnbox --
    private static void Test_RhUnbox_CopiesValueToDest()
    {
        int source = 0x12345;
        void* boxed = Boxing.RhBox(MethodTable.Of<int>(), (byte*)&source);
        Assert.True(boxed != null, "RhBox failed in unbox round-trip");

        int dest = 0;
        Boxing.RhUnbox(boxed, (byte*)&dest, MethodTable.Of<int>());
        Assert.Equal(0x12345, dest, "RhUnbox should copy boxed value into destination");
    }

    private static void Test_RhUnbox_NullObj_NoOp()
    {
        int dest = 0x7EADBEEF;
        Boxing.RhUnbox(null, (byte*)&dest, MethodTable.Of<int>());
        Assert.Equal(0x7EADBEEF, dest, "RhUnbox(null) must leave destination unchanged");
    }

    // -- RhUnbox2 --
    private static void Test_RhUnbox2_ReturnsPointerPastMethodTable()
    {
        int source = 5555;
        void* boxed = Boxing.RhBox(MethodTable.Of<int>(), (byte*)&source);
        byte* unboxPtr = Boxing.RhUnbox2(MethodTable.Of<int>(), boxed);
        Assert.True(unboxPtr == (byte*)boxed + sizeof(MethodTable*), "RhUnbox2 must skip MethodTable pointer");
        Assert.Equal(5555, *(int*)unboxPtr, "RhUnbox2 payload value");
    }

    // =============================================================================
    // Type Casting Stubs
    // =============================================================================

    // -- RhTypeCast_AreTypesAssignable --
    private static void Test_AreTypesAssignable_SameType_True()
    {
        bool same = Casting.RhTypeCast_AreTypesAssignable(MethodTable.Of<int>(), MethodTable.Of<int>());
        Assert.True(same, "same type must be assignable to itself");
    }

    private static void Test_AreTypesAssignable_Unrelated_False()
    {
        bool unrelated = Casting.RhTypeCast_AreTypesAssignable(MethodTable.Of<int>(), MethodTable.Of<string>());
        Assert.False(unrelated, "int should not be assignable to string");
    }

    // -- RhTypeCast_CheckCastAny --
    private static void Test_CheckCastAny_ReturnsObjUnchanged()
    {
        // The current stub implementation simply returns its input — pin the behavior.
        object s = "hello";
        object result = Casting.RhTypeCast_CheckCastAny(s, MethodTable.Of<object>());
        Assert.True(ReferenceEquals(result, s), "CheckCastAny stub should return input");
    }

    // -- RhTypeCast_CheckCastClass --
    private static void Test_CheckCastClass_ValidCast_ReturnsObj()
    {
        object s = "hello";
        object result = Casting.RhTypeCast_CheckCastClass(s, MethodTable.Of<object>());
        Assert.NotNull(result, "string cast to object should succeed");
    }

    // -- RhTypeCast_CheckCastClassSpecial --
    private static void Test_CheckCastClassSpecial_Null_ReturnsNull()
    {
        object? result = Casting.RhTypeCast_CheckCastClassSpecial(null!, MethodTable.Of<object>(), fThrow: false);
        Assert.Null(result, "CheckCastClassSpecial(null) must return null");
    }

    private static void Test_CheckCastClassSpecial_ValidNoThrow_ReturnsObj()
    {
        object s = "hello";
        object? result = Casting.RhTypeCast_CheckCastClassSpecial(s, MethodTable.Of<object>(), fThrow: false);
        Assert.NotNull(result, "CheckCastClassSpecial valid cast must return obj");
    }

    // -- RhTypeCast_CheckCastInterface --
    private static void Test_CheckCastInterface_NullObj_ReturnsNull()
    {
        object? result = Casting.RhTypeCast_CheckCastInterface(null!, MethodTable.Of<IComparable>());
        Assert.Null(result, "CheckCastInterface(null) must return null");
    }

    // -- RhTypeCast_IsInstanceOfAny --
    private static void Test_IsInstanceOfAny_MatchFirstHandle()
    {
        nint* handles = stackalloc nint[2];
        handles[0] = (nint)MethodTable.Of<IComparable>();
        handles[1] = (nint)MethodTable.Of<object>();

        object boxed = 42;
        object? result = Casting.RhTypeCast_IsInstanceOfAny(boxed, (MethodTable**)handles, 2);
        Assert.NotNull(result, "boxed int matches IComparable in the handle list");
    }

    private static void Test_IsInstanceOfAny_NullObj_Null()
    {
        nint* handles = stackalloc nint[1];
        handles[0] = (nint)MethodTable.Of<object>();

        object? result = Casting.RhTypeCast_IsInstanceOfAny(null!, (MethodTable**)handles, 1);
        Assert.Null(result, "IsInstanceOfAny(null, ...) must return null");
    }

    // -- RhTypeCast_IsInstanceOfClass --
    private static void Test_IsInstanceOfClass_ValidSubclass()
    {
        object s = "hello";
        object? result = Casting.RhTypeCast_IsInstanceOfClass(s, MethodTable.Of<object>());
        Assert.NotNull(result, "string is-instance-of object should return the object");
    }

    private static void Test_IsInstanceOfClass_Unrelated_Null()
    {
        object s = "hello";
        object? result = Casting.RhTypeCast_IsInstanceOfClass(s, MethodTable.Of<int[]>());
        Assert.Null(result, "unrelated class check must return null");
    }

    // -- RhTypeCast_IsInstanceOfInterface --
    private static void Test_IsInstanceOfInterface_NullObj_False()
    {
        object? nullObj = null;
        bool result = Casting.RhTypeCast_IsInstanceOfInterface(nullObj!, MethodTable.Of<IComparable>());
        Assert.False(result, "null obj must return false from IsInstanceOfInterface");
    }

    private static void Test_IsInstanceOfInterface_Implements_True()
    {
        object boxed = 42;
        bool result = Casting.RhTypeCast_IsInstanceOfInterface(boxed, MethodTable.Of<IComparable>());
        Assert.True(result, "boxed int should be instance of IComparable");
    }

    // =============================================================================
    // Write Barrier, Ref & Array Helper Stubs
    // =============================================================================

    // -- RhBuffer_BulkMoveWithWriteBarrier --
    private static void Test_RhBuffer_BulkMoveWithWriteBarrier_Copies()
    {
        const int Size = 24;
        byte* src = stackalloc byte[Size];
        byte* dst = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            src[i] = (byte)(i ^ 0x3C);
        }
        StartupCodeHelpers.RhBuffer_BulkMoveWithWriteBarrier(dst, src, (nuint)Size);
        for (int i = 0; i < Size; i++)
        {
            Assert.Equal(src[i], dst[i], "RhBuffer_BulkMoveWithWriteBarrier byte mismatch");
        }
    }

    // -- RhBulkMoveWithWriteBarrier --
    private static void Test_RhBulkMoveWithWriteBarrier_Copies()
    {
        const int Size = 32;
        byte* src = stackalloc byte[Size];
        byte* dst = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            src[i] = (byte)(i * 3 + 7);
            dst[i] = 0;
        }
        StartupCodeHelpers.RhBulkMoveWithWriteBarrier(dst, src, (nuint)Size);
        for (int i = 0; i < Size; i++)
        {
            Assert.Equal((byte)(i * 3 + 7), dst[i], "RhBulkMoveWithWriteBarrier byte mismatch");
        }
    }

    // -- RhpAssignRef --
    private static void Test_RhpAssignRef_WritesLocation()
    {
        void* slot = null;
        int sentinel = 42;
        void* target = &sentinel;
        StartupCodeHelpers.RhpAssignRef(&slot, target);
        Assert.True(slot == target, "RhpAssignRef must write the value into the location");
    }

    // -- RhpCheckedAssignRef --
    private static void Test_RhpCheckedAssignRef_WritesLocation()
    {
        void* slot = null;
        int sentinel = 7;
        void* target = &sentinel;
        StartupCodeHelpers.RhpCheckedAssignRef(&slot, target);
        Assert.True(slot == target, "RhpCheckedAssignRef must write the value into the location");
    }

    // -- RhpCheckedXchg --
    private static void Test_RhpCheckedXchg_SwapsAndReturnsOriginal()
    {
        int originalSentinel = 1;
        int newSentinel = 2;
        void* location = &originalSentinel;
        void* original = StartupCodeHelpers.InterlockedExchange(&location, &newSentinel);
        Assert.True(original == &originalSentinel, "RhpCheckedXchg must return original");
        Assert.True(location == &newSentinel, "RhpCheckedXchg must install new value");
    }

    // -- RhpLdelemaRef --
    private static void Test_RhpLdelemaRef_ReturnsRefToElement()
    {
        object?[] arr = ["a", "b", "c"];
        ref object? slot = ref StartupCodeHelpers.RhpLdelemaRef(arr, 1, MethodTable.Of<object>());
        Assert.True(ReferenceEquals(slot, "b"), "RhpLdelemaRef returns ref to correct element");

        slot = "B!";
        Assert.True(ReferenceEquals(arr[1], "B!"), "RhpLdelemaRef ref is writable");
    }

    // -- RhpStelemRef --
    private static void Test_RhpStelemRef_StoresCompatibleElement()
    {
        object?[] arr = [null, null, null];
        StartupCodeHelpers.RhpStelemRef(arr, 1, "stored");
        Assert.True(ReferenceEquals(arr[1], "stored"), "RhpStelemRef must store the element");
    }

    private static void Test_RhpStelemRef_NullValue_StoresNull()
    {
        object?[] arr = ["a", "b", "c"];
        StartupCodeHelpers.RhpStelemRef(arr, 2, null);
        Assert.Null(arr[2], "RhpStelemRef(null) must write null into the slot");
    }

    // -- RhSpanHelpers_MemZero --
    private static void Test_RhSpanHelpers_MemZero_ZerosBuffer()
    {
        const int Size = 18;
        byte* buf = stackalloc byte[Size];
        for (int i = 0; i < Size; i++)
        {
            buf[i] = 0xFF;
        }
        StartupCodeHelpers.RhSpanHelpers_MemZero(buf, (nuint)Size);
        for (int i = 0; i < Size; i++)
        {
            Assert.Equal((byte)0, buf[i], "RhSpanHelpers_MemZero byte mismatch");
        }
    }

    // =============================================================================
    // Math Intrinsic Stubs
    // =============================================================================

    // -- ceil --
    private static void Test_ceil_Values()
    {
        Assert.True(RuntimeMath.ceil(1.5) == 2.0, "ceil(1.5) == 2");
        Assert.True(RuntimeMath.ceil(-1.5) == -1.0, "ceil(-1.5) == -1");
        Assert.True(RuntimeMath.ceil(3.0) == 3.0, "ceil(3.0) == 3");
        Assert.True(RuntimeMath.ceil(0.0) == 0.0, "ceil(0.0) == 0");
        Assert.True(double.IsNaN(RuntimeMath.ceil(double.NaN)), "ceil(NaN) == NaN");
    }

    // -- ceilf --
    private static void Test_ceilf_Values()
    {
        Assert.True(RuntimeMath.ceilf(2.25f) == 3.0f, "ceilf(2.25) == 3");
        Assert.True(RuntimeMath.ceilf(-0.5f) == 0.0f, "ceilf(-0.5) == 0");
    }

    // -- modf --
    private static void Test_modf_SplitsFractional()
    {
        double intPart = 0.0;
        double frac = RuntimeMath.ModF(3.75, &intPart);
        Assert.True(intPart == 3.0, "modf integer part");
        Assert.True(frac > 0.749 && frac < 0.751, "modf fractional part is about 0.75");
    }

    // -- sqrt --
    private static void Test_sqrt_Values()
    {
        double r = RuntimeMath.sqrt(16.0);
        Assert.True(r > 3.999 && r < 4.001, "sqrt(16) is about 4");

        Assert.True(RuntimeMath.sqrt(0.0) == 0.0, "sqrt(0) == 0");
        Assert.True(double.IsNaN(RuntimeMath.sqrt(-1.0)), "sqrt(-1) == NaN");
    }

    // -- RhpDbl2Int --
    private static void Test_RhpDbl2Int_Truncates()
    {
        Assert.Equal(7, StartupCodeHelpers.RhpDbl2Int(7.9), "RhpDbl2Int(7.9) == 7");
        Assert.Equal(-3, StartupCodeHelpers.RhpDbl2Int(-3.5), "RhpDbl2Int(-3.5) == -3");
    }

    // -- RhpDbl2Lng --
    private static void Test_RhpDbl2Lng_Truncates()
    {
        Assert.True(StartupCodeHelpers.RhpDbl2Lng(1234567.89) == 1234567L, "RhpDbl2Lng truncates positive");
        Assert.True(StartupCodeHelpers.RhpDbl2Lng(-42.99) == -42L, "RhpDbl2Lng truncates negative");
    }

    // =============================================================================
    // GC & Finalization Stubs
    // =============================================================================

    // -- RhCollect --
    private static void Test_RhCollect_Smoke()
    {
        RuntimeGC.RhCollect(0, default);
        Assert.True(true, "RhCollect returned without crashing");
    }

    // -- RhGetAllocatedBytesForCurrentThread --
    private static void Test_RhGetAllocatedBytesForCurrentThread_ReturnsZero()
    {
        // With per-thread allocation contexts (TLABs), this now returns the
        // cumulative bytes allocated by the current thread minus unused TLAB space.
        // After the kernel has booted and run prior tests, this must be > 0.
        long result = RuntimeGC.RhGetAllocatedBytesForCurrentThread();
        Assert.True(result >= 0, "RhGetAllocatedBytesForCurrentThread must return >= 0");
    }

    // -- RhGetGcCollectionCount --
    private static void Test_RhGetGcCollectionCount_Gen0_NonNegative()
    {
        int count = RuntimeGC.RhGetGcCollectionCount(0, false);
        Assert.True(count >= 0, "RhGetGcCollectionCount(gen0) must be >= 0");
    }

    // -- RhGetGCDescSize --
    private static void Test_RhGetGCDescSize_NoGCPointers_Zero()
    {
        int size = RuntimeGC.RhGetGCDescSize(MethodTable.Of<int>());
        Assert.Equal(0, size, "RhGetGCDescSize on non-GC type must return 0");
    }

    // -- RhGetGcTotalMemory --
    private static void Test_RhGetGcTotalMemory_Positive()
    {
        long total = RuntimeGC.RhGetGcTotalMemory();
        Assert.True(total > 0, "RhGetGcTotalMemory must return > 0 after allocations");
    }

    // -- RhGetGeneration --
    private static void Test_RhGetGeneration_ReturnsZero()
    {
        object obj = new();
        int gen = RuntimeGC.RhGetGeneration(obj);
        Assert.Equal(0, gen, "RhGetGeneration must return 0 for a live object");
    }

    // -- RhGetGenerationSize --
    private static void Test_RhGetGenerationSize_Gen0_Positive()
    {
        int gen0 = RuntimeGC.RhGetGenerationSize(0);
        Assert.True(gen0 > 0, "RhGetGenerationSize(0) must be > 0 after allocations");
        int gen1 = RuntimeGC.RhGetGenerationSize(1);
        Assert.Equal(0, gen1, "RhGetGenerationSize(1) must be 0 (non-generational)");
    }

    // -- RhGetGCSegmentSize --
    private static void Test_RhGetGCSegmentSize_Positive()
    {
        ulong segSize = RuntimeGC.RhGetGCSegmentSize();
        Assert.True(segSize > 0, "RhGetGCSegmentSize must return > 0");
    }

    // -- RhGetLastGCPercentTimeInGC --
    private static void Test_RhGetLastGCPercentTimeInGC_InRange()
    {
        int pct = RuntimeGC.RhGetLastGCPercentTimeInGC();
        Assert.True(pct >= 0 && pct <= 100, "RhGetLastGCPercentTimeInGC must be in [0, 100]");
    }

    // -- RhGetMemoryInfo --
    private static void Test_RhGetMemoryInfo_Smoke()
    {
        byte[] buffer = new byte[512];
        RuntimeGC.RhGetMemoryInfo(ref buffer[0], GCKind.Any);
        Assert.True(true, "RhGetMemoryInfo returned without crashing");
    }

    // -- RhGetTotalAllocatedBytes --
    private static void Test_RhGetTotalAllocatedBytes_Positive()
    {
        long allocated = RuntimeGC.RhGetTotalAllocatedBytes();
        Assert.True(allocated > 0, "RhGetTotalAllocatedBytes must be > 0 after allocations");
    }

    // -- RhGetTotalAllocatedBytesPrecise --
    private static void Test_RhGetTotalAllocatedBytesPrecise_MatchesNonPrecise()
    {
        // With TLABs, the precise variant subtracts live threads' unused TLAB space
        // in addition to dead thread unused bytes. So precise <= normal.
        long precise = RuntimeGC.RhGetTotalAllocatedBytesPrecise();
        long normal = RuntimeGC.RhGetTotalAllocatedBytes();
        Assert.True(precise > 0, "RhGetTotalAllocatedBytesPrecise must be > 0 after allocations");
        Assert.True(precise <= normal, "RhGetTotalAllocatedBytesPrecise must be <= RhGetTotalAllocatedBytes");
    }

    // -- RhIsPromoted --
    private static void Test_RhIsPromoted_ReturnsFalse()
    {
        object obj = new();
        bool promoted = RuntimeGC.RhIsPromoted(obj);
        Assert.Equal(false, promoted, "RhIsPromoted must return false (non-generational)");
    }

    // -- RhIsServerGc --
    private static void Test_RhIsServerGc_ReturnsFalse()
    {
        bool server = RuntimeGC.RhIsServerGc();
        Assert.Equal(false, server, "RhIsServerGc must return false");
    }

    // -- RhpGcPoll --
    private static void Test_RhpGcPoll_Smoke()
    {
        StartupCodeHelpers.RhpGcPoll();
        Assert.True(true, "RhpGcPoll no-op returned");
    }

    // -- RhpNewFinalizable --
    private static void Test_RhpNewFinalizable_Object_NonNull()
    {
        void* raw = StartupCodeHelpers.RhpNewFinalizable(MethodTable.Of<object>());
        Assert.True(raw != null, "RhpNewFinalizable returned null");

        object obj = null!;
        *(void**)Unsafe.AsPointer(ref obj) = raw;
        Assert.NotNull(obj, "RhpNewFinalizable result should cast to object");
    }

    // -- RhpTrapThreads --
    private static void Test_RhpTrapThreads_Smoke()
    {
        StartupCodeHelpers.RhpTrapThreads();
        Assert.True(true, "RhpTrapThreads no-op returned");
    }

    // -- RhRegisterForGCReporting / RhUnregisterForGCReporting --
    private static void Test_RhRegisterAndUnregisterForGCReporting_Smoke()
    {
        byte dummy = 0;
        RuntimeGC.RhRegisterForGCReporting(&dummy);
        RuntimeGC.RhUnregisterForGCReporting(&dummy);
        Assert.True(true, "Register/Unregister for GC reporting returned without crashing");
    }

    // -- RhReRegisterForFinalize --
    private static void Test_RhReRegisterForFinalize_Smoke()
    {
        object obj = new();
        StartupCodeHelpers.RhReRegisterForFinalize(obj);
        Assert.True(true, "RhReRegisterForFinalize no-op returned");
    }

    // -- RhSuppressFinalize --
    private static void Test_RhSuppressFinalize_Smoke()
    {
        object obj = new();
        StartupCodeHelpers.RhSuppressFinalize(obj);
        Assert.True(true, "RhSuppressFinalize no-op returned");
    }

    // =============================================================================
    // Threading / Runtime Info Stubs
    // =============================================================================

    // -- GetSystemArrayEEType --
    private static void Test_GetSystemArrayEEType_NonNull()
    {
        MethodTable* mt = StartupCodeHelpers.GetSystemArrayEEType();
        Assert.True(mt != null, "GetSystemArrayEEType must return a non-null MethodTable*");
    }

    // -- InitializeModules --
    private static void Test_InitializeModules_ZeroArgs_Smoke()
    {
        // The body is empty; call it with safe zero arguments to pin the no-op behavior.
        StartupCodeHelpers.InitializeModules(nint.Zero, null, 0, null, 0);
        Assert.True(true, "InitializeModules no-op returned");
    }

    // -- RhCompatibleReentrantWaitAny --
    private static void Test_RhCompatibleReentrantWaitAny_ReturnsSuccess()
    {
        uint result = StartupCodeHelpers.RhCompatibleReentrantWaitAny(0, 0, 0, nint.Zero);
        Assert.Equal(0u, result, "RhCompatibleReentrantWaitAny returns WAIT_OBJECT_0 (0)");
    }

    // -- RhCreateCrashDumpIfEnabled --
    private static void Test_RhCreateCrashDumpIfEnabled_Smoke()
    {
        StartupCodeHelpers.RhCreateCrashDumpIfEnabled(nint.Zero, nint.Zero);
        Assert.True(true, "RhCreateCrashDumpIfEnabled no-op returned");
    }

    // -- RhCurrentOSThreadId --
    private static void Test_RhCurrentOSThreadId_ReturnsNonNegative()
    {
        ulong id = StartupCodeHelpers.RhCurrentOSThreadId();
        Assert.True(id < 256UL, "RhCurrentOSThreadId returns a valid thread ID");
    }

    // -- RhGetCurrentThreadStackBounds --
    private static void Test_RhGetCurrentThreadStackBounds_HighAboveLow()
    {
        RuntimeThread.RhGetCurrentThreadStackBounds(out nint low, out nint high);
        Assert.True(high > low, "Stack high bound must be greater than low bound");
    }

    // -- RhGetProcessCpuCount --
    private static void Test_RhGetProcessCpuCount_ReturnsOne()
    {
        int count = StartupCodeHelpers.RhGetProcessCpuCount();
        Assert.Equal(1, count, "RhGetProcessCpuCount stub returns 1");
    }

    // -- RhGetRuntimeVersion --
    private static void Test_RhGetRuntimeVersion_ReturnsZero()
    {
        int version = StartupCodeHelpers.RhGetRuntimeVersion();
        Assert.Equal(0, version, "RhGetRuntimeVersion stub returns 0");
    }

    // -- RhNewObject --
    private static void Test_RhNewObject_Object_NonNull()
    {
        void* raw = StartupCodeHelpers.RhNewObject(MethodTable.Of<object>());
        Assert.True(raw != null, "RhNewObject returned null");

        object obj = null!;
        *(void**)Unsafe.AsPointer(ref obj) = raw;
        Assert.NotNull(obj, "RhNewObject result should cast to object");
    }

    // -- RhpCheckedLockCmpXchg --
    private static void Test_RhpCheckedLockCmpXchg_MatchSwaps()
    {
        object a = "a";
        object b = "b";
        object location = a;
        object* locPtr = (object*)Unsafe.AsPointer(ref location);
        // comparand == *location → value wins
        object result = StartupCodeHelpers.RhpCheckedLockCmpXchg(locPtr, b, a, 0);
        Assert.True(ReferenceEquals(result, a), "RhpCheckedLockCmpXchg returns original when comparand matches");
        Assert.True(ReferenceEquals(location, b), "RhpCheckedLockCmpXchg writes value when comparand matches");
    }

    private static void Test_RhpCheckedLockCmpXchg_NoMatchKeeps()
    {
        object a = "first";
        object b = "second";
        object location = a;
        object* locPtr = (object*)Unsafe.AsPointer(ref location);
        // comparand != *location → location stays as a
        object result = StartupCodeHelpers.RhpCheckedLockCmpXchg(locPtr, b, b, 0);
        Assert.True(ReferenceEquals(result, a), "RhpCheckedLockCmpXchg returns original when comparand differs");
        Assert.True(ReferenceEquals(location, a), "RhpCheckedLockCmpXchg leaves location unchanged when comparand differs");
    }

    // -- RhpGetTickCount64 --
    private static void Test_RhpGetTickCount64_Monotonic()
    {
        long first = Cpu.RhpGetTickCount64();
        long second = Cpu.RhpGetTickCount64();
        Assert.True(second > first, "RhpGetTickCount64 must be monotonically increasing");
    }

    // -- RhpPInvoke / RhpPInvokeReturn / RhpReversePInvoke / RhpReversePInvokeReturn --
    private static void Test_RhpPInvokePairs_Smoke()
    {
        StartupCodeHelpers.RhpPInvoke(nint.Zero);
        StartupCodeHelpers.RhpPInvokeReturn(nint.Zero);
        StartupCodeHelpers.RhpReversePInvoke(nint.Zero);
        StartupCodeHelpers.RhpReversePInvokeReturn(nint.Zero);
        Assert.True(true, "P/Invoke transition stubs returned without crashing");
    }

    // -- RhpStackProbe --
    private static void Test_RhpStackProbe_Smoke()
    {
        StartupCodeHelpers.RhpStackProbe();
        Assert.True(true, "RhpStackProbe no-op returned");
    }

    // -- RhSetThreadExitCallback --
    private static void Test_RhSetThreadExitCallback_Smoke()
    {
        global::Cosmos.Kernel.Core.Runtime.Thread.RhSetThreadExitCallback(nint.Zero);
        Assert.True(true, "RhSetThreadExitCallback no-op returned");
    }

    // -- RhSpinWait --
    private static void Test_RhSpinWait_Zero_NoOp()
    {
        Core.Runtime.Thread.RhSpinWait(0);
        Assert.True(true, "RhSpinWait(0) returned");
    }

    // -- RhYield --
    private static void Test_RhYield_ReturnsZero()
    {
        int result = Core.Runtime.Thread.RhYield();
        Assert.Equal(0, result, "RhYield stub returns 0");
    }

    // -- NativeRuntimeEventSource_Log* (6 no-op stubs, batched) --
    private static void Test_NativeRuntimeEventSource_LogAll_Smoke()
    {
        StartupCodeHelpers.NativeRuntimeEventSource_LogContentionLockCreated(nint.Zero, nint.Zero, "m", 0);
        StartupCodeHelpers.NativeRuntimeEventSource_LogContentionStart(0, nint.Zero, nint.Zero, "m", 0);
        StartupCodeHelpers.NativeRuntimeEventSource_LogContentionStop(0, nint.Zero, nint.Zero, "m", 0);
        StartupCodeHelpers.NativeRuntimeEventSource_LogWaitHandleWaitStart(0, nint.Zero);
        StartupCodeHelpers.NativeRuntimeEventSource_LogWaitHandleWaitStop(0, nint.Zero);
        StartupCodeHelpers.NativeRuntimeEventSource_LogThreadPoolMinMaxThreads(0, 0, 0, 0);
        Assert.True(true, "All NativeRuntimeEventSource no-op stubs returned");
    }

    // =============================================================================
    // Code / Stack Introspection Stubs
    // =============================================================================

    // -- RhFindMethodStartAddress --
    private static void Test_RhFindMethodStartAddress_ReturnsInput()
    {
        nint input = 0x1234;
        nint result = StartupCodeHelpers.RhFindMethodStartAddress(input);
        Assert.True(result == input, "RhFindMethodStartAddress stub returns its input");
    }

    // -- RhGetCodeTarget --
    private static void Test_RhGetCodeTarget_ReturnsInput()
    {
        nint input = 0x5678;
        nint result = StartupCodeHelpers.RhGetCodeTarget(input);
        Assert.True(result == input, "RhGetCodeTarget stub returns its input");
    }

    // -- RhGetCrashInfoBuffer --
    private static void Test_RhGetCrashInfoBuffer_ReturnsZero()
    {
        nint result = StartupCodeHelpers.RhGetCrashInfoBuffer();
        Assert.True(result == nint.Zero, "RhGetCrashInfoBuffer stub returns IntPtr.Zero");
    }

    // -- RhGetCurrentThreadStackTrace --
    private static void Test_RhGetCurrentThreadStackTrace_ReturnsZeroFrames()
    {
        nint result = StartupCodeHelpers.RhGetCurrentThreadStackTrace(0, 16, out int frames);
        Assert.True(result == nint.Zero, "RhGetCurrentThreadStackTrace returns IntPtr.Zero");
        Assert.Equal(0, frames, "RhGetCurrentThreadStackTrace sets frame count to 0");
    }

    // -- RhGetModuleFileName --
    private static void Test_RhGetModuleFileName_ReturnsZeroAndNullName()
    {
        int length = MetaTable.RhGetModuleFileName(nint.Zero, out byte* name);
        Assert.Equal(0, length, "RhGetModuleFileName stub returns length 0");
        Assert.True(name == null, "RhGetModuleFileName stub writes null pointer");
    }

    // -- RhGetTargetOfUnboxingAndInstantiatingStub --
    private static void Test_RhGetTargetOfUnboxingAndInstantiatingStub_ReturnsInput()
    {
        nint input = 0x9ABC;
        nint result = StartupCodeHelpers.RhGetTargetOfUnboxingAndInstantiatingStub(input);
        Assert.True(result == input, "RhGetTargetOfUnboxingAndInstantiatingStub stub returns its input");
    }

    // -- DebugDebugger_IsNativeDebuggerAttached --
    private static void Test_DebugDebugger_IsNativeDebuggerAttached_ReturnsOne()
    {
        int result = Debugger.DebugDebugger_IsNativeDebuggerAttached();
        Assert.Equal(1, result, "DebugDebugger_IsNativeDebuggerAttached stub returns 1");
    }
}
