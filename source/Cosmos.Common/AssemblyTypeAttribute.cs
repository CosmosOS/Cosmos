using System;

namespace Cosmos.Common
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyTypeAttribute : Attribute
    {
        public AssemblyTypeAttribute(AssemblyType aType)
        {
            Type = aType;
        }

        public AssemblyType Type { get; }
    }
}
