using System;
using System.Reflection;
using System.Reflection.Metadata;

namespace Cosmos.IL2CPU
{
    public static class ModuleExtensions
    {
        // TODO: All this methods will be available in .NET Standard 2.0
        public static FieldInfo ResolveField(this Module aThis, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new Exception("NetCore Fix Me");
        }

        public static MethodBase ResolveMethod(this Module aThis, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new Exception("NetCore Fix Me");
        }

        public static string ResolveString(this Module aThis, int metadataToken)
        {
            throw new Exception("NetCore Fix Me");
        }

        public static Type ResolveType(this Module aThis, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            throw new Exception("NetCore Fix Me");
        }
    }
}
