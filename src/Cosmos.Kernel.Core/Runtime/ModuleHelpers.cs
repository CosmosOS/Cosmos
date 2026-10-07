using System.Runtime;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

internal static unsafe class ModuleHelpers
{
    internal static nint OsModule { get; private set; }

    [RuntimeExport("RhpGetModuleSection")]
    internal static void* RhpGetModuleSection(TypeManagerHandle* module, ReadyToRunSectionType sectionId, int* length)
    {
        nint section = module->AsTypeManager()->GetModuleSection(sectionId, out int len);
        *length = len;
        return (void*)section;
    }

    [RuntimeExport("RhpRegisterOsModule")]
    internal static nint RhpRegisterOsModule(nint osModule)
    {
        Serial.WriteString("[ModuleHelpers] - RhpRegisterOsModule called for OS Module 0x");
        Serial.WriteHex((nuint)osModule);
        Serial.WriteString("\n");
        //TODO: Should be saved on an array or some other struct.
        OsModule = osModule;
        return osModule;
    }

    [RuntimeExport("RhpCreateTypeManager")]
    internal static unsafe TypeManagerHandle RhpCreateTypeManager(IntPtr osModule, ReadyToRunHeader* moduleHeader, void** pClasslibFunctions, uint nClasslibFunctions)
    {
        TypeManager* tm = (TypeManager*)MemoryOp.Alloc((uint)sizeof(TypeManager));
        tm->OsHandle = osModule;
        tm->Header = moduleHeader;
        tm->m_pClasslibFunctions = pClasslibFunctions;
        tm->m_nClasslibFunctions = nClasslibFunctions;
        tm->m_pStaticsGCDataSection = tm->GetModuleSection(ReadyToRunSectionType.GCStaticRegion, out _);
        tm->m_pThreadStaticsDataSection = tm->GetModuleSection(ReadyToRunSectionType.ThreadStaticRegion, out _);

        return new TypeManagerHandle(tm);
        // TypeManager typeManager = new(osModule, moduleHeader, pClasslibFunctions, nClasslibFunctions);
        // return new TypeManagerHandle((TypeManager*)Unsafe.AsPointer(ref typeManager));
    }
    [RuntimeExport("RhpGetClasslibFunctionFromCodeAddress")]
    internal static unsafe void* RhpGetClasslibFunctionFromCodeAddress(IntPtr address, ClassLibFunctionId id)
    {
        //Requires some work;
        return (void*)IntPtr.Zero;
    }

    [RuntimeExport("RhpGetClasslibFunctionFromEEType")]
    internal static unsafe void* RhpGetClasslibFunctionFromEEType(MethodTable* pEEType, ClassLibFunctionId id)
    {
        return pEEType->TypeManager.AsTypeManager()->GetClassLibFunction(id);
    }

    [RuntimeExport("RhGetOSModuleFromPointer")]
    internal static IntPtr RhGetOSModuleFromPointer(IntPtr ptr)
    {
        return (nint)OsModule;
    }

    [RuntimeExport("RhFindBlob")]
    internal static unsafe bool RhFindBlob(TypeManagerHandle* typeManagerHandle, uint blobId, byte** ppbBlob, uint* pcbBlob)
    {
        ReadyToRunSectionType sectionId = (ReadyToRunSectionType)((uint)ReadyToRunSectionType.ReadonlyBlobRegionStart + blobId);

        TypeManager* pModule = typeManagerHandle->AsTypeManager();

        IntPtr pBlob;
        pBlob = pModule->GetModuleSection(sectionId, out int length);

        *ppbBlob = (byte*)pBlob;
        *pcbBlob = (uint)length;

        return pBlob != IntPtr.Zero;
    }

    internal static unsafe TypeManagerHandle[] CreateTypeManagers(IntPtr osModule, Span<nint> pModuleHeaders, void** pClasslibFunctions, uint nClasslibFunctions)
    {
        // Count the number of modules so we can allocate an array to hold the TypeManager objects.
        // At this stage of startup, complex collection classes will not work.
        int moduleCount = 0;
        for (int i = 0; i < pModuleHeaders.Length; i++)
        {
            // The null pointers are sentinel values and padding inserted as side-effect of
            // the section merging. (The global static constructors section used by C++ has
            // them too.)
            if (pModuleHeaders[i] != IntPtr.Zero)
            {
                moduleCount++;
            }
        }

        // We cannot use the new keyword just yet, so stackalloc the array first
        var pHandles = stackalloc TypeManagerHandle[moduleCount];
        int moduleIndex = 0;
        for (int i = 0; i < pModuleHeaders.Length; i++)
        {
            if (pModuleHeaders[i] != IntPtr.Zero)
            {
                TypeManagerHandle handle = RhpCreateTypeManager(OsModule, (ReadyToRunHeader*)pModuleHeaders[i], pClasslibFunctions, nClasslibFunctions);

                IntPtr dehydratedRegion = handle.AsTypeManager()->GetModuleSection(ReadyToRunSectionType.DehydratedData, out int length);
                if (dehydratedRegion != IntPtr.Zero)
                {
                    Serial.WriteString("[ManagedModule] - Dehydrated Data found for module ");
                    Serial.WriteNumber(moduleIndex);
                    Serial.WriteString("\n");
                }

                pHandles[moduleIndex] = handle;
                moduleIndex++;
            }
        }

        //void* ptr;

        //Memory.RhAllocateNewArray(MethodTable.Of<TypeManagerHandle[]>(), (uint)moduleCount, 0, out ptr);
        // Any potentially dehydrated MethodTables got rehydrated, we can safely use `new` now.
        var modules = new TypeManagerHandle[moduleCount];
        //var modules = Unsafe.AsRef<TypeManagerHandle[]>(ptr);
        for (int i = 0; i < modules.Length; i++)
        {
            modules[i] = pHandles[i];
        }

        return modules;
    }
    internal static unsafe TypeManagerHandle[] CreateTypeManagers(IntPtr osModule, ReadyToRunHeader** pModuleHeaders, int count, void** pClasslibFunctions, uint nClasslibFunctions)
    {
        // Count the number of modules so we can allocate an array to hold the TypeManager objects.
        // At this stage of startup, complex collection classes will not work.
        int moduleCount = 0;
        for (int i = 0; i < count; i++)
        {
            // The null pointers are sentinel values and padding inserted as side-effect of
            // the section merging. (The global static constructors section used by C++ has
            // them too.)
            if (pModuleHeaders[i] != (void*)IntPtr.Zero)
            {
                moduleCount++;
            }
        }

        // We cannot use the new keyword just yet, so stackalloc the array first
        var pHandles = stackalloc TypeManagerHandle[moduleCount];
        int moduleIndex = 0;
        for (int i = 0; i < count; i++)
        {
            if (pModuleHeaders[i] != (void*)IntPtr.Zero)
            {
                TypeManagerHandle handle = RhpCreateTypeManager(osModule, pModuleHeaders[i], pClasslibFunctions, nClasslibFunctions);

                pHandles[moduleIndex] = handle;
                moduleIndex++;
            }
        }

        // Any potentially dehydrated MethodTables got rehydrated, we can safely use `new` now.
        TypeManagerHandle[] modules = new TypeManagerHandle[moduleCount];
        for (int i = 0; i < moduleCount; i++)
        {
            modules[i] = pHandles[i];
        }

        return modules;
    }
}
