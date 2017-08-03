using System;

namespace Cosmos.IL2CPU.API.Attribs { 
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyTypeAttrib : Attribute {
        public enum AssemblyType {
            Plugs,
            AssemblerPlugs
        }

        public AssemblyTypeAttrib(AssemblyType aType) {
            Type = aType;
        }

        public AssemblyType Type { get; }
    }
}
