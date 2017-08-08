using System;

namespace Cosmos.IL2CPU.API.Attribs
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, AllowMultiple = true)]
    public class AsmLabel : Attribute
    {
        public string Label { get; }

        public AsmLabel(string aLabel)
        {
            Label = aLabel;
        }
    }
}
