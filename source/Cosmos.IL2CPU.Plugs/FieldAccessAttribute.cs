using System;

namespace Cosmos.IL2CPU.Plugs
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FieldAccessAttribute : Attribute
    {
        public string Name;
    }
}