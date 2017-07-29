using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.IL2CPU.API
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class AsmLabelAttribute : Attribute
    {
        public string Label { get; }

        public AsmLabelAttribute(string aLabel)
        {
            Label = aLabel;
        }
    }
}
