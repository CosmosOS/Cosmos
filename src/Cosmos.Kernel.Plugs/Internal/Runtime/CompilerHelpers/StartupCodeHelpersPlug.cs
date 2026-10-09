using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.Core.Runtime;
using Internal.Runtime;

namespace Cosmos.Kernel.Plugs.Internal.Runtime.CompilerHelpers;

[Plug("Internal.Runtime.CompilerHelpers.StartupCodeHelpers")]
public unsafe partial class StartupCodeHelpersPlug
{
    [PlugMember]
    public static void RunModuleInitializers()
    {
        ManagedModule.RunModuleInitializers();
    }

    [PlugMember]
    internal static int GetLoadedModules(TypeManagerHandle[] outputModules)
    {
        return ManagedModule.GetLoadedModules(outputModules);
    }
}
