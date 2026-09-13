using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Handles the initialization of managed modules.
/// </summary>
// https://github.com/dotnet/runtime/blob/main/docs/design/coreclr/botr/readytorun-format.md

internal static unsafe partial class ManagedModule
{
    /// <summary>
    /// Table of logical modules.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal static TypeManagerHandle[] s_modules;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal static int s_moduleCount = 0;
    public static TypeManagerHandle[] Modules => s_modules;
    public static int ModuleCount => s_moduleCount;

    /// <summary>
    /// GC handle of an array with s_moduleCount elements, each representing and array of GC static bases of the types in the module.
    /// </summary>
    private static IntPtr s_moduleGCStaticsSpines;


    [LibraryImport("*", EntryPoint = "GetModules")]
    [SuppressGCTransition]
    internal static unsafe partial uint GetModules(out ReadyToRunHeader* modules);

    // Classlib functions array - must match ClassLibFunctionId enum order
    // This is allocated on the unmanaged heap to ensure it's never moved by the GC
    private static void** s_pClasslibFunctions = null;
    private const int ClasslibFunctionCount = 12;

    /// <summary>
    /// Returns the MethodTable* for System.Array. This is used by the runtime when
    /// it needs to determine the base type of array types.
    /// Note: Also exported via RuntimeExport in StartupCodeHelpers, but we need a
    /// direct reference for the classlib functions array.
    /// </summary>
    private static unsafe MethodTable* GetSystemArrayEEType()
    {
        return MethodTable.Of<Array>();
    }

    //This method requires no optimization and inlining to ensure the stack is not corrupted.
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static void InitializeModules()
    {
        Serial.WriteString("[ManagedModule] - Initilizing Module Handlers - Starting\n");
        uint count = GetModules(out var modulesptr);
        Serial.WriteString("[ManagedModule] - Found ");
        Serial.WriteNumber(count);
        Serial.WriteString(" modules\n");

        // Allocate classlib functions array on unmanaged heap (never moved by GC)
        // Must match ClassLibFunctionId enum (12 entries, 0-11)
        s_pClasslibFunctions = (void**)Cosmos.Kernel.Core.Memory.Heap.Heap.Alloc((uint)(ClasslibFunctionCount * sizeof(void*)));
        s_pClasslibFunctions[0] = (void*)(delegate*<ExceptionIDs, Exception>)&ExceptionHelper.GetRuntimeException;
        s_pClasslibFunctions[1] = null; // FailFast - not implemented yet
        s_pClasslibFunctions[2] = null; // ThreadEntryPoint - not implemented yet
        s_pClasslibFunctions[3] = null; // AppendExceptionStackFrame - not implemented yet
        s_pClasslibFunctions[4] = null; // unused
        s_pClasslibFunctions[5] = (void*)(delegate*<MethodTable*>)&GetSystemArrayEEType; // GetSystemArrayEEType
        s_pClasslibFunctions[6] = null; // OnFirstChance - not implemented yet
        s_pClasslibFunctions[7] = null; // OnUnhandledException - not implemented yet
        s_pClasslibFunctions[8] = null; // ObjectiveCMarshalTryGetTaggedMemory
        s_pClasslibFunctions[9] = null; // ObjectiveCMarshalGetIsTrackedReferenceCallback
        s_pClasslibFunctions[10] = null; // ObjectiveCMarshalGetOnEnteredFinalizerQueueCallback
        s_pClasslibFunctions[11] = null; // ObjectiveCMarshalGetUnhandledExceptionPropagationHandler

        var modules = ModuleHelpers.CreateTypeManagers((nint)modulesptr, new(modulesptr, (int)count), s_pClasslibFunctions, ClasslibFunctionCount);

        var ptr = Memory.RhNewArray(MethodTable.Of<object[][]>(), modules.Length);
        var gcStaticBaseSpines = Unsafe.AsRef<object[]>(ptr);
        s_moduleGCStaticsSpines = GarbageCollector.AllocateHandler((GCObject*)ptr, GCHandleType.Normal, IntPtr.Zero);

        for (int i = 0; i < modules.Length; i++)
        {
            Serial.WriteString("[ManagedModule] - Setting TypeManagerSlot for module ");
            Serial.WriteNumber(i);
            Serial.WriteString("\n");

            InitializeGlobalTablesForModule(modules[i], i, ref gcStaticBaseSpines);

            Serial.WriteString("[ManagedModule] - Running Static Constructors for all modules\n");
            RunInitializers(modules[i], ReadyToRunSectionType.EagerCctor);
        }

        s_modules = modules;
        s_moduleCount = modules.Length;

        Serial.WriteString("[ManagedModule] - Initilizing Module Handlers - Complete\n");
    }

    public static int GetLoadedModules(TypeManagerHandle[] outputModules)
    {
        if (outputModules is not null)
        {
            for (int i = 0; i < s_moduleCount && i < outputModules.Length; i++)
            {
                outputModules[i] = s_modules[i];
            }
        }

        return s_moduleCount;
    }

    private static void InitializeGlobalTablesForModule(TypeManagerHandle typeManagerHandle, int moduleIndex, ref object[] gcStaticBaseSpines)
    {
        TypeManagerSlot* section;

        TypeManager* typeManager = typeManagerHandle.AsTypeManager();

        section = (TypeManagerSlot*)typeManager->GetModuleSection(ReadyToRunSectionType.TypeManagerIndirection, out int length);
        section->TypeManager = typeManagerHandle;
        section->ModuleIndex = moduleIndex;

        IntPtr gcStaticBase = typeManager->GetModuleSection(ReadyToRunSectionType.GCStaticRegion, out length);
        if (gcStaticBase != IntPtr.Zero)
        {
            Serial.WriteString("[ManagedModule] - Initializing Statics for module ");
            Serial.WriteNumber(moduleIndex);
            Serial.WriteString("\n");

            var spine = InitializeStatics(gcStaticBase, length);
            ref object rawSpineIndexData = ref MemoryMarshal.GetArrayDataReference(gcStaticBaseSpines);
            //ref object rawSpineIndexData = ref Unsafe.As<byte, object>(ref Unsafe.As<RawArrayData>(gcStaticBaseSpines).Data);            
            // Avoid type check
            //gcStaticBaseSpines[moduleIndex] = spine;
            Unsafe.Add(ref rawSpineIndexData, moduleIndex) = spine;
        }

        IntPtr frozenSegmentRegion = typeManager->GetModuleSection(ReadyToRunSectionType.FrozenObjectRegion, out length);
        if (frozenSegmentRegion != IntPtr.Zero)
        {
            if (GarbageCollector.RegisterFrozenSegment(frozenSegmentRegion, (nuint)length, (nuint)length, (nuint)length) == IntPtr.Zero)
            {
                ExceptionHelper.FailFast("Failed to register frozen object segment for the module.");
            }

        }
    }

    public static void RunModuleInitializers()
    {
        for (int i = 0; i < s_moduleCount; i++)
        {
            Serial.WriteString("[ManagedModule] - Running Module Initializers for module ");
            Serial.WriteNumber(i);
            Serial.WriteString("\n");

            RunInitializers(s_modules[i], ReadyToRunSectionType.ModuleInitializerList);
        }
    }

    private static unsafe void RunInitializers(TypeManagerHandle typeManager, ReadyToRunSectionType section)
    {
        byte* pInitializers = (byte*)typeManager.AsTypeManager()->GetModuleSection(section, out int length);

        Serial.WriteString("[ManagedModule] - Running Initializers, found ");
        Serial.WriteNumber(length / (MethodTable.SupportsRelativePointers ? sizeof(int) : sizeof(nint)));
        Serial.WriteString(" initializers for section ");
        Serial.WriteNumber((int)section);
        Serial.WriteString("\n");

        for (byte* pCurrent = pInitializers;
            pCurrent < (pInitializers + length);
            pCurrent += MethodTable.SupportsRelativePointers ? sizeof(int) : sizeof(nint))
        {
            Serial.WriteString("[ManagedModule] - Running Initializer at address ");
            Serial.WriteHex((uint)(nint)pCurrent);
            Serial.WriteString("\n");
            var initializer = MethodTable.SupportsRelativePointers ? (delegate*<void>)ReadRelPtr32(pCurrent) : *(delegate*<void>*)pCurrent;
            initializer();
            Serial.WriteString("[ManagedModule] - Completed Initializer at address ");
            Serial.WriteHex((uint)(nint)pCurrent);
            Serial.WriteString("\n");
        }

        static void* ReadRelPtr32(void* address)
            => (byte*)address + *(int*)address;
    }


    private static unsafe object[] InitializeStatics(IntPtr gcStaticRegionStart, int length)
    {
        byte* gcStaticRegionEnd = ((byte*)gcStaticRegionStart) + length;


        //var ptr = Memory.RhNewArray(MethodTable.Of<object[]>(), length / (MethodTable.SupportsRelativePointers ? sizeof(int) : sizeof(nint)));
        //var spine = Unsafe.AsRef<object[]>(ptr);

        object[] spine = new object[length / (MethodTable.SupportsRelativePointers ? sizeof(int) : sizeof(nint))];
        ref object rawSpineData = ref Unsafe.As<byte, object>(ref Unsafe.As<RawArrayData>(spine).Data);
        System.Runtime.InteropServices.GCHandle.Alloc(spine);

        int currentBase = 0;
        for (byte* block = (byte*)gcStaticRegionStart;
            block < gcStaticRegionEnd;
            block += MethodTable.SupportsRelativePointers ? sizeof(int) : sizeof(nint))
        {

            IntPtr* pBlock = MethodTable.SupportsRelativePointers ? (IntPtr*)ReadRelPtr32(block) : *(IntPtr**)block;
            nint blockAddr = MethodTable.SupportsRelativePointers ? (nint)ReadRelPtr32(pBlock) : *pBlock;

            if ((blockAddr & GCStaticRegionConstants.Uninitialized) == GCStaticRegionConstants.Uninitialized)
            {
                object? obj = null;
                var pMT = (MethodTable*)(blockAddr & ~GCStaticRegionConstants.Mask);
                Memory.RhAllocateNewObject(pMT, GC_ALLOC_FLAGS.GC_ALLOC_PINNED_OBJECT_HEAP, &obj);

                if (obj is null)
                {
                    Serial.WriteString("Failed allocating GC static bases\n");
                    throw new OutOfMemoryException();
                }

                if ((blockAddr & GCStaticRegionConstants.HasPreInitializedData) == GCStaticRegionConstants.HasPreInitializedData)
                {
                    void* pPreInitDataAddr = MethodTable.SupportsRelativePointers ? ReadRelPtr32((int*)pBlock + 1) : (void*)*(pBlock + 1);

                    nuint rawSize = pMT->BaseSize - (nuint)(2 * sizeof(IntPtr));
                    if (pMT->HasComponentSize)
                    {
                        rawSize += (uint)Unsafe.As<RawArrayData>(obj).Length * (nuint)pMT->ComponentSize;
                    }

                    byte* destPtr = (byte*)Unsafe.AsPointer(ref Unsafe.As<RawData>(obj).Data);

                    MemoryOp.MemMove(destPtr, (byte*)pPreInitDataAddr, (int)rawSize);
                }

                // Avoid type check
                Unsafe.Add(ref rawSpineData, currentBase) = obj;

                *pBlock = *(IntPtr*)&obj;
            }

            currentBase++;
        }

        return spine;


        static void* ReadRelPtr32(void* address)
            => (byte*)address + *(int*)address;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal class RawArrayData
{
    public uint Length; // Array._numComponents padded to IntPtr
                        //#if TARGET_64BIT
    public uint Padding;
    //#endif
    public byte Data;
}

[StructLayout(LayoutKind.Sequential)]
internal class RawData
{
    public byte Data;
}
