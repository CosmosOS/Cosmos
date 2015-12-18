using System;
using System.Reflection;

namespace Cosmos.IL2CPU.CustomImplementation.System
{
    public static class ObjectImplRefs
    {
        public static readonly MethodBase ObjectCtor;

        public static readonly Assembly RuntimeAssemblyDef;

        static ObjectImplRefs()
        {
            Type xType = typeof(object);
            ObjectCtor = xType.GetMethod("Ctor", new Type[] { typeof(IntPtr) });
            if (ObjectCtor == null)
            {
                throw new Exception("Implementation of ObjectCtor not found!");
            }


            xType = typeof(ObjectImpl);
            foreach (FieldInfo xField in typeof(ObjectImplRefs).GetFields())
            {
                if (xField.Name.EndsWith("Ref"))
                {
                    MethodBase xTempMethod = xType.GetMethod(
                        xField.Name.Substring(0, xField.Name.Length - "Ref".Length));
                    if (xTempMethod == null)
                    {
                        throw new Exception(
                            "Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length)
                            + "' not found on RuntimeEngine!");
                    }
                    xField.SetValue(null, xTempMethod);
                }
            }
        }
    }
}