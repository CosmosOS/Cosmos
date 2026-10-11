// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native import for the address of the interface dispatch stub.
/// <c>get_initial_dynamic_interface_dispatch</c> is exported by
/// Runtime/InterfaceDispatch.s on both architectures, so no arch gating is
/// needed here.
/// </summary>
internal static unsafe partial class InterfaceDispatchNative
{
    /// <summary>The address of <c>RhpInitialDynamicInterfaceDispatch</c>, the stub
    /// a dispatch cell starts with: it resolves the call through
    /// <c>RhpCidResolve</c>.</summary>
    [LibraryImport("*", EntryPoint = "get_initial_dynamic_interface_dispatch")]
    [SuppressGCTransition]
    public static partial nuint GetInitialDynamicInterfaceDispatch();

    /// <summary>The cache of resolved calls <c>RhpInitialDynamicInterfaceDispatch</c>
    /// looks up before resolving one (<c>CachedInterfaceDispatch.Remember</c>
    /// fills it); 0 where the stub has none (ARM64).</summary>
    [LibraryImport("*", EntryPoint = "get_interface_dispatch_cache")]
    [SuppressGCTransition]
    public static partial nuint GetInterfaceDispatchCache();
}
