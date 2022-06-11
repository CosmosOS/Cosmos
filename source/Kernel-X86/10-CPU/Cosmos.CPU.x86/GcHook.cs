using System.Diagnostics;
using IL2CPU.API.Attribs;

namespace Cosmos.CPU.x86;

[DebuggerStepThrough]
public static class GCImplementation
{
    private static void AcquireLock()
    {
    }

    private static void ReleaseLock()
    {
    }

    [PlugMethod(PlugRequired = true)]
    public static uint AllocNewObject(uint aSize) => 0;

    /// <summary>
    ///     This function gets the pointer to the memory location of where it's stored.
    /// </summary>
    /// <param name="aObject"></param>
    public static void IncRefCount(uint aObject)
    {
    }

    /// <summary>
    ///     This function gets the pointer to the memory location of where it's stored.
    /// </summary>
    /// <param name="aObject"></param>
    public static void DecRefCount(uint aObject)
    {
    }
}
