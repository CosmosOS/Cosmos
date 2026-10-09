using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// This class is responsible for initializing the library and its dependencies. It is called by the runtime before any managed code is executed.
/// </summary>
internal class LibraryInitializer
{
    /// <summary>
    /// Initialize all Core Elements of Cosmos, such as heap memory, garbage collector, and managed modules. This method is called by the runtime before any managed code is executed.
    /// </summary>
    public static void InitializeLibrary()
    {
        // Initialize heap for memory allocations
        Serial.WriteString("[KERNEL]   - Initializing heap...\n");
        MemoryOp.InitializeHeap(0, 0);

        // Initialize garbage collector
        Serial.WriteString("[KERNEL]   - Initializing garbage collector...\n");
        GarbageCollector.Initialize();

        // Initialize managed modules
        Serial.WriteString("[KERNEL]   - Initializing managed modules...\n");
        ManagedModule.InitializeModules();
    }
}
