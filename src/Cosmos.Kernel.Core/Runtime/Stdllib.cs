using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory;
using Internal.Runtime;

#region Things needed by ILC
namespace System
{
    namespace Runtime
    {
        internal enum InternalGCCollectionMode
        {
            Default,
            Forced,
            Optimized
        }

        internal sealed class RuntimeExportAttribute(string entry) : Attribute
        {
        }

        internal sealed class RuntimeImportAttribute : Attribute
        {
            public string DllName { get; }
            public string EntryPoint { get; }

            public RuntimeImportAttribute(string entry)
            {
                EntryPoint = entry;
            }

            public RuntimeImportAttribute(string dllName, string entry)
            {
                EntryPoint = entry;
                DllName = dllName;
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    // Calls to methods or references to fields marked with this attribute may be replaced at
    // some call sites with jit intrinsic expansions.
    // Types marked with this attribute may be specially treated by the runtime/compiler.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
    internal sealed class IntrinsicAttribute : Attribute
    {
    }
}
namespace Cosmos.Kernel.Core.Runtime
{
    // A class that the compiler looks for that has helpers to initialize the
    // process. The compiler can gracefully handle the helpers not being present,
    // but the class itself being absent is unhandled. Let's add an empty class.
    internal static unsafe partial class StartupCodeHelpers
    {
        [RuntimeExport("RhpReversePInvoke")]
        internal static void RhpReversePInvoke(IntPtr frame) { }
        [RuntimeExport("RhpReversePInvokeReturn")]
        internal static void RhpReversePInvokeReturn(IntPtr frame) { }
        [RuntimeExport("RhpPInvoke")]
        internal static void RhpPInvoke(IntPtr frame) { }
        [RuntimeExport("RhpPInvokeReturn")]
        internal static void RhpPInvokeReturn(IntPtr frame) { }

        [RuntimeExport("RhpFallbackFailFast")]
        internal static void RhpFallbackFailFast()
        {
            ExceptionHelper.FailFast("Fallback fail fast called");
        }

        [RuntimeExport("InitializeModules")]
        internal static unsafe void InitializeModules(IntPtr osModule, IntPtr* pModuleHeaders, int count, IntPtr* pClasslibFunctions, int nClasslibFunctions) { }

        // RhpThrowEx is now implemented in assembly (CPU/ExceptionHandling.s)
        // The assembly stub saves register context, creates ExInfo, then calls RhThrowEx

        /// <summary>
        /// Managed exception dispatcher called from assembly RhpThrowEx.
        /// This is the entry point for the two-pass exception handling.
        /// </summary>
        [RuntimeExport("RhThrowEx")]
        internal static void RhThrowEx(Exception ex, void* pExInfo)
        {
            // Get throw address and context from ExInfo
            nuint throwAddress = 0;
            nuint throwFp = 0;  // Frame pointer (RBP on x64, FP/x29 on ARM64)
            nuint throwSp = 0;  // Stack pointer

            if (pExInfo != null)
            {
                // ExInfo.m_pExContext points to PAL_LIMITED_CONTEXT
                void* pContext = *(void**)((byte*)pExInfo + 0x08); // OFFSETOF__ExInfo__m_pExContext
                if (pContext != null)
                {
#if ARCH_X64
                    // x64 PAL_LIMITED_CONTEXT layout:
                    // 0x00: IP (instruction pointer / return address)
                    // 0x08: Rsp
                    // 0x10: Rbp
                    throwAddress = *(nuint*)((byte*)pContext + 0x00);
                    throwSp = *(nuint*)((byte*)pContext + 0x08);
                    throwFp = *(nuint*)((byte*)pContext + 0x10);
#elif ARCH_ARM64
                    // ARM64 PAL_LIMITED_CONTEXT layout:
                    // 0x00: SP (stack pointer)
                    // 0x08: IP (instruction pointer / LR)
                    // 0x10: FP (frame pointer / x29)
                    throwSp = *(nuint*)((byte*)pContext + 0x00);
                    throwAddress = *(nuint*)((byte*)pContext + 0x08);
                    throwFp = *(nuint*)((byte*)pContext + 0x10);
#endif
                }
            }

            // Use our exception handling infrastructure
            ExceptionHelper.ThrowExceptionWithContext(ex, throwAddress, throwFp, throwSp, pExInfo);
        }

        [RuntimeExport("RhpAssignRef")]
        internal static unsafe void RhpAssignRef(void** location, void* value)
        {
            *location = value;
        }

        [RuntimeExport("RhpCheckedAssignRef")]
        internal static unsafe void RhpCheckedAssignRef(void** location, void* value)
        {
            *location = value;
        }

        [RuntimeExport("RhpCheckedXchg")]
        internal static void* InterlockedExchange(void** location1, void* value)
        {
            void* original = *location1;
            *location1 = value;
            return original;
        }


        [RuntimeExport("RhpTrapThreads")]
        internal static void RhpTrapThreads() { }

        [RuntimeExport("RhpGcPoll")]
        internal static void RhpGcPoll() { }

        [RuntimeExport("RhpStackProbe")]
        internal static void RhpStackProbe()
        {

        }

        [RuntimeExport("RhGetRuntimeVersion")]
        internal static int RhGetRuntimeVersion()
        {
            return 0;
        }

        [RuntimeExport("RhBulkMoveWithWriteBarrier")]
        internal static unsafe void RhBulkMoveWithWriteBarrier(void* dest, void* src, UIntPtr len)
        {
            memmove((byte*)dest, (byte*)src, len);
        }

        [RuntimeExport("RhpCheckedLockCmpXchg")]
        internal static unsafe object RhpCheckedLockCmpXchg(object* location, object value, object comparand, int typeHandle)
        {
            object original = *location;
            if (original == comparand)
            {
                *location = value;
            }

            return original;
        }

        [RuntimeExport("RhGetProcessCpuCount")]
        internal static int RhGetProcessCpuCount()
        {
            return 1;
        }

        [RuntimeExport("RhSuppressFinalize")]
        internal static void RhSuppressFinalize(object obj) { }

        [RuntimeExport("RhReRegisterForFinalize")]
        internal static void RhReRegisterForFinalize(object obj) { }

        /// <summary>
        /// Returns the MethodTable* for System.Array. This is used by the runtime when
        /// it needs to determine the base type of array types.
        /// </summary>
        [RuntimeExport("GetSystemArrayEEType")]
        internal static unsafe MethodTable* GetSystemArrayEEType()
        {
            return MethodTable.Of<Array>();
        }

        [RuntimeExport("RhNewObject")]
        internal static unsafe void* RhNewObject(MethodTable* pEEType)
        {
            return Memory.RhpNewFast(pEEType); // Simplified implementation
        }

        [RuntimeExport("RhpStelemRef")]
        internal static unsafe void RhpStelemRef(object?[] array, nint index, object? obj)
        {
            if (array is null)
            {
                throw new NullReferenceException();
            }

            ref object rawData = ref MemoryMarshal.GetArrayDataReference(array)!;
            ref object element = ref Unsafe.Add(ref rawData, index);

            if (obj == null)
            {
                element = null!;
                return;
            }

            void* objPtr = Unsafe.AsPointer(ref obj);  // Get address of the obj parameter
            void* actualObjPtr = *(void**)objPtr;       // Dereference to get the actual object pointer
            RhpAssignRef((void**)Unsafe.AsPointer(ref element), actualObjPtr);
        }

        [RuntimeExport("RhCurrentOSThreadId")]
        internal static ulong RhCurrentOSThreadId()
        {
            if (CosmosFeatures.SchedulerEnabled && Scheduler.SchedulerManager.IsRunning)
            {
                Scheduler.PerCpuState? cpuState = Scheduler.SchedulerManager.CurrentCpuState;
                return cpuState?.CurrentThread?.Id ?? 1;
            }

            return 1;
        }

        [RuntimeExport("RhGetCrashInfoBuffer")]
        internal static IntPtr RhGetCrashInfoBuffer()
        {
            return IntPtr.Zero;
        }

        [RuntimeExport("RhCreateCrashDumpIfEnabled")]
        internal static void RhCreateCrashDumpIfEnabled(IntPtr exceptionRecord, IntPtr contextRecord) { }

#if ARCH_ARM64
        [RuntimeExport("RhpByRefAssignRef")]
        internal static unsafe void RhpByRefAssignRef(void** location, void* value)
        {
            RhpByRefAssignRefArm64(location, value);
        }

        [DllImport("*", EntryPoint = "RhpByRefAssignRefArm64")]
        private static extern unsafe void RhpByRefAssignRefArm64(void** location, void* value);
#endif

        [RuntimeExport("RhpNewFinalizable")]
        internal static unsafe void* RhpNewFinalizable(MethodTable* pEEType)
        {
            // TODO: Should actually be Memory.RhAllocateNewObject with finalizable flag
            return Memory.RhpNewFast(pEEType); // Simplified implementation (Should set gc flag)
        }

        // RhpRethrow is now implemented in assembly (CPU/ExceptionHandling.s)

        [RuntimeExport("RhCompatibleReentrantWaitAny")]
        internal static uint RhCompatibleReentrantWaitAny(int alertable, uint timeout, uint handleCount, IntPtr pHandles)
        {
            // Single-threaded kernel: always return success immediately
            return 0x00000000; // WAIT_OBJECT_0 (SUCCESS)
        }

        [RuntimeExport("RhBuffer_BulkMoveWithWriteBarrier")]
        internal static unsafe void RhBuffer_BulkMoveWithWriteBarrier(void* dest, void* src, UIntPtr len)
        {
            memmove((byte*)dest, (byte*)src, len);
        }

        [RuntimeExport("RhFindMethodStartAddress")]
        internal static IntPtr RhFindMethodStartAddress(IntPtr ip)
        {
            return ip;
        }

        [RuntimeExport("RhGetCurrentThreadStackTrace")]
        internal static IntPtr RhGetCurrentThreadStackTrace(int skipFrames, int maxFrames, out int pFrameCount)
        {
            pFrameCount = 0;
            return IntPtr.Zero;
        }

        [RuntimeExport("RhpLdelemaRef")]
        public static unsafe ref object RhpLdelemaRef(object?[] array, nint index, MethodTable* elementType)
        {

            ref object rawData = ref MemoryMarshal.GetArrayDataReference(array)!;
            ref object element = ref Unsafe.Add(ref rawData, index);

            MethodTable* arrayElemType = array.GetMethodTable()->RelatedParameterType;

            /* This is causing issues, disabling for now.
            if (elementType != arrayElemType)
                throw new ArrayTypeMismatchException();
            */

            return ref element;
        }

        [RuntimeExport("RhGetCodeTarget")]
        internal static IntPtr RhGetCodeTarget(IntPtr pCode)
        {
            return pCode;
        }

        [RuntimeExport("RhGetTargetOfUnboxingAndInstantiatingStub")]
        internal static IntPtr RhGetTargetOfUnboxingAndInstantiatingStub(IntPtr pCode)
        {
            return pCode;
        }

        [RuntimeExport("RhSpanHelpers_MemZero")]
        internal static unsafe void RhSpanHelpers_MemZero(byte* dest, nuint len)
        {
            MemoryOp.MemSet(dest, 0, (int)len);
        }

        [RuntimeExport("RhpDbl2Lng")]
        internal static long RhpDbl2Lng(double value)
        {
            return (long)value;
        }

        [RuntimeExport("RhpDbl2Int")]
        internal static int RhpDbl2Int(double value)
        {
            return (int)value;
        }

        private static unsafe void memmove(byte* dest, byte* src, UIntPtr len)
        {
            MemoryOp.MemMove(dest, src, (int)len);
        }

        // NativeRuntimeEventSource stubs for event tracing
        [RuntimeExport("NativeRuntimeEventSource_LogContentionLockCreated")]
        internal static void NativeRuntimeEventSource_LogContentionLockCreated(IntPtr lockID, IntPtr declaringTypeID, string methodName, int lockLevel) { }

        [RuntimeExport("NativeRuntimeEventSource_LogContentionStart")]
        internal static void NativeRuntimeEventSource_LogContentionStart(int id, IntPtr lockID, IntPtr declaringTypeID, string methodName, int lockLevel) { }

        [RuntimeExport("NativeRuntimeEventSource_LogContentionStop")]
        internal static void NativeRuntimeEventSource_LogContentionStop(int id, IntPtr lockID, IntPtr declaringTypeID, string methodName, int lockLevel) { }

        [RuntimeExport("NativeRuntimeEventSource_LogWaitHandleWaitStart")]
        internal static void NativeRuntimeEventSource_LogWaitHandleWaitStart(int id, IntPtr handle) { }

        [RuntimeExport("NativeRuntimeEventSource_LogWaitHandleWaitStop")]
        internal static void NativeRuntimeEventSource_LogWaitHandleWaitStop(int id, IntPtr handle) { }

        [RuntimeExport("NativeRuntimeEventSource_LogThreadPoolMinMaxThreads")]
        internal static void NativeRuntimeEventSource_LogThreadPoolMinMaxThreads(int workerMin, int workerMax, int ioMin, int ioMax) { }

    }
}

/*              [RuntimeExport("RhGetGcCollectionCount")]
                static int RhGetGcCollectionCount(int generation) { throw null; }
                [RuntimeExport("RhGetGcTotalMemory")]
                static long RhGetGcTotalMemory(bool forceFullCollection) { throw null; }
                [RuntimeExport("RhCollect")]
                static void RhCollect(int generation, InternalGCCollectionMode mode) { }
                [RuntimeExport("RhWaitForPendingFinalizers")]
                static void RhWaitForPendingFinalizers() { }
                [RuntimeExport("EventPipeInternal_CreateProvider")]
                static IntPtr EventPipeInternal_CreateProvider(string providerName, IntPtr callback, IntPtr callbackContext) { throw null; }
                [RuntimeExport("EventPipeInternal_DeleteProvider")]
                static void EventPipeInternal_DeleteProvider(IntPtr provider) { }
                [RuntimeExport("EventPipeInternal_WriteEventData")]
                static bool EventPipeInternal_WriteEventData(IntPtr provider, int eventID, int eventVersion, int eventLevel, long keywords, IntPtr pMetadata, int metadataLength, IntPtr pData, int dataLength) { throw null; }
                [RuntimeExport("EventPipeInternal_EventActivityIdControl")]
                static bool EventPipeInternal_EventActivityIdControl(int controlCode, ref Guid activityId) { throw null; }
                [RuntimeExport("EventPipeInternal_DefineEvent")]
                static int EventPipeInternal_DefineEvent(IntPtr provider, string eventName, int eventVersion, int eventLevel, long keywords, IntPtr pMetadata, int metadataLength) { throw null; }
                [RuntimeExport("RhGetGenerationBudget")]
                static int RhGetGenerationBudget(int generation) { throw null; }
                [RuntimeExport("RhGetTotalAllocatedBytes")]
                static long RhGetTotalAllocatedBytes() { throw null; }
                [RuntimeExport("RhGetLastGCPercentTimeInGC")]
                static int RhGetLastGCPercentTimeInGC(int generation) { throw null; }
*/

#endregion
