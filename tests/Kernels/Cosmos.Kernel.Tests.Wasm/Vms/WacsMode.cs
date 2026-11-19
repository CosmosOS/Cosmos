// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Tests.Wasm.Vms;

/// <summary>
/// The WACS interpreters that need no runtime code generation. The
/// transpiler, which emits IL, is left out: NativeAOT cannot run it.
/// </summary>
internal enum WacsMode
{
    /// <summary>The default: one object per instruction, dispatched through virtual calls.</summary>
    Polymorphic,

    /// <summary>The default with the super-instruction rewriter, which folds instruction trees into one node.</summary>
    PolymorphicSuper,

    /// <summary>The source-generated switch over a pre-decoded bytecode stream.</summary>
    Switch,

    /// <summary>The switch with its bytecode-stream super-instruction fuser.</summary>
    SwitchSuper
}
