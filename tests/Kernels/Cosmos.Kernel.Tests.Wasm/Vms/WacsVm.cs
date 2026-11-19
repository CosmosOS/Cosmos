// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Tests.Wasm.Modules;
using Wacs.Core;
using Wacs.Core.Runtime;

namespace Cosmos.Kernel.Tests.Wasm.Vms;

/// <summary>
/// WACS (kelnishi/WACS) in one of its interpreter modes. Exports are called
/// through <see cref="WasmRuntime.CreateInvoker"/>, which takes boxed
/// arguments: the typed <c>CreateInvokerFunc</c> family goes through
/// <see cref="Delegate.DynamicInvoke"/>.
/// </summary>
internal sealed class WacsVm : IWasmVm
{
    private readonly WacsMode _mode;
    private readonly Dictionary<(ModuleKind Module, string Export), Delegates.GenericFuncs> _invokers = [];
    private WasmRuntime? _runtime;

    public string Name
    {
        get
        {
            return _mode switch
            {
                WacsMode.Polymorphic => "WACS",
                WacsMode.PolymorphicSuper => "WACS-Super",
                WacsMode.Switch => "WACS-Switch",
                _ => "WACS-SwitchSuper",
            };
        }
    }

    public WacsVm(WacsMode mode)
    {
        _mode = mode;
    }

    public void Load(byte[] benchModule, byte[] hostModule)
    {
        WasmRuntime runtime = new();
        // Both modes must be set before InstantiateModule, which compiles
        // every function for the switch runtime. The polymorphic rewriter
        // stays off under the switch runtime: its folded nodes are opcodes
        // the switch's bytecode compiler cannot emit.
        runtime.SuperInstruction = _mode == WacsMode.PolymorphicSuper;
        runtime.UseSwitchRuntime = _mode is WacsMode.Switch or WacsMode.SwitchSuper;
        runtime.ExecContext.Attributes.UseSwitchSuperInstructions = _mode == WacsMode.SwitchSuper;
        runtime.BindHostFunction<Func<int, int, int>>(("env", "host_add"), HostAdd);

        runtime.RegisterModule("bench", runtime.InstantiateModule(BinaryModuleParser.ParseWasm(new MemoryStream(benchModule))));
        runtime.RegisterModule("host", runtime.InstantiateModule(BinaryModuleParser.ParseWasm(new MemoryStream(hostModule))));

        _invokers.Clear();
        _runtime = runtime;
    }

    public int Invoke(ModuleKind module, string export, ReadOnlySpan<int> arguments)
    {
        WasmRuntime runtime = _runtime ?? throw new InvalidOperationException($"{Name} has not loaded its modules.");
        if (!_invokers.TryGetValue((module, export), out Delegates.GenericFuncs? invoker))
        {
            string moduleName = module == ModuleKind.Host ? "host" : "bench";
            FuncAddr address = runtime.GetExportedFunction((moduleName, export));
            invoker = runtime.CreateInvoker(address, new InvokerOptions { SynchronousExecution = true });
            _invokers[(module, export)] = invoker;
        }

        object[] boxed = new object[arguments.Length];
        for (int i = 0; i < arguments.Length; i++)
        {
            boxed[i] = arguments[i];
        }

        return invoker(boxed)[0];
    }

    private static int HostAdd(int a, int b)
    {
        return a + b;
    }
}
