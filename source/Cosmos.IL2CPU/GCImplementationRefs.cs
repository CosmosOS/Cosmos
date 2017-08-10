using System;
using System.Reflection;

namespace Cosmos.IL2CPU {
    public static class GCImplementationRefs {
        public static readonly MethodBase AllocNewObjectRef;
        public static readonly MethodBase IncRefCountRef;
        public static readonly MethodBase DecRefCountRef;

        static GCImplementationRefs() {
            Type xType = null;
            if (CompilerEngine.UseGen3Kernel) {
                xType = Type.GetType("Cosmos.CPU.x86.GCImplementation, Cosmos.CPU.x86", true);
            } else {
                xType = Type.GetType("Cosmos.Core.GCImplementation, Cosmos.Core", true);
            } 
            if (xType == null) {
                throw new Exception("GCImplementation type not found!");
            }

            foreach (FieldInfo xField in typeof(GCImplementationRefs).GetTypeInfo().GetFields()) {
                if (xField.Name.EndsWith("Ref")) {
                    MethodBase xTempMethod = xType.GetTypeInfo().GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length));
                    if (xTempMethod == null) {
                        throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on RuntimeEngine!");
                    }
                    xField.SetValue(null, xTempMethod);
                }
            }
        }
    }
}