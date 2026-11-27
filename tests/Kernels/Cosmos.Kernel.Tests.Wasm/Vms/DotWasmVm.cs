// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Tests.Wasm.Modules;
using DotWasm.Encoding;
using DotWasm.Models;
using DotWasm.Runtime;

namespace Cosmos.Kernel.Tests.Wasm.Vms;

/// <summary>
/// DotWasm (nuskey8/DotWasm): validates the module at instantiation, then
/// interprets it, one C# stack frame per wasm call.
/// </summary>
internal sealed class DotWasmVm : IWasmVm
{
    private WasmInstance? _bench;
    private WasmInstance? _host;

    public string Name => "DotWasm";

    public void Load(byte[] benchModule, byte[] hostModule)
    {
        WasmLinker linker = new(new WasmStore());
        _bench = linker.Instantiate(WasmEncoding.Decode(benchModule));
        linker.RegisterFunction("env", "host_add", new HostFunction
        {
            // An import decodes as a non-nullable function type; FuncType
            // defaults to nullable, which the linker refuses as a mismatch.
            Type = new FuncType
            {
                IsNullable = false,
                Parameters = [WasmTypes.I32, WasmTypes.I32],
                Results = [WasmTypes.I32],
            },
            Delegate = HostAdd,
        });
        _host = linker.Instantiate(WasmEncoding.Decode(hostModule));
    }

    public int Invoke(ModuleKind module, string export, ReadOnlySpan<int> arguments)
    {
        WasmInstance instance = (module == ModuleKind.Host ? _host : _bench)
            ?? throw new InvalidOperationException($"{Name} has not loaded its modules.");

        WasmValue[] values = new WasmValue[arguments.Length];
        for (int i = 0; i < arguments.Length; i++)
        {
            values[i] = WasmValue.FromI32(arguments[i]);
        }

        WasmValue[] results = new WasmValue[1];
        instance.Invoke(export, values, results);
        return results[0].I32;
    }

    private static void HostAdd(ReadOnlySpan<WasmValue> arguments, Span<WasmValue> results)
    {
        results[0] = WasmValue.FromI32(arguments[0].I32 + arguments[1].I32);
    }
}
