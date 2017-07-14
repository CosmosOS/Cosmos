using System;

namespace Cosmos.IL2CPU.API {
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyTypeAttribute : Attribute {
        public enum AssemblyType {
            Plugs,
            AssemblerPlugs
        }

        public AssemblyTypeAttribute(AssemblyType aType) {
            Type = aType;
        }

        public AssemblyType Type { get; }
    }
}
