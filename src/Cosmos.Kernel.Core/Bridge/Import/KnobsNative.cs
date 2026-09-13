using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native import for the runtime-provided knob values table.
/// Consumed by the AppContext plug.
/// </summary>
internal static unsafe partial class KnobsNative
{
    [LibraryImport("*", EntryPoint = "RhGetKnobValues")]
    [SuppressGCTransition]
    public static partial uint GetKnobValues(out byte** keys, out byte** values);
}
