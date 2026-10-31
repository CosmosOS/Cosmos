// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Tests.Wasm.Modules;

/// <summary>Which of the two <see cref="WasmModules"/> an export lives in.</summary>
internal enum ModuleKind
{
    Bench,
    Host
}
