using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Reflection;

[Plug(typeof(Module))]
internal class ModuleImpl
{
    public static string ToString(Module aThis) => "";
}
