using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Debug.Symbols
{
    public static class ModuleExtensions
    {
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
