// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Tests.Wasm.Modules;

namespace Cosmos.Kernel.Tests.Wasm.Vms;

/// <summary>
/// A WebAssembly VM under test: it loads the suite's two modules once, then
/// calls their exports by name.
/// </summary>
internal interface IWasmVm
{
    /// <summary>The name the tests and the report carry.</summary>
    string Name { get; }

    /// <summary>
    /// Decodes, validates and instantiates both modules, binding the host
    /// module's <c>env.host_add</c> import to a C# function.
    /// </summary>
    void Load(byte[] benchModule, byte[] hostModule);

    /// <summary>Calls an export that takes i32 arguments and returns one i32.</summary>
    int Invoke(ModuleKind module, string export, ReadOnlySpan<int> arguments);
}
