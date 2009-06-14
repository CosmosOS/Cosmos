using System;

namespace Cosmos.BuildEngine
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PlugAssemblyAttribute : IgnoreWhenBuildingCosmosAttribute
    {
    }
}