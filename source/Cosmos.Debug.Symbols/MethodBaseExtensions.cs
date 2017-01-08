using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

namespace Cosmos.Debug.Symbols
{
    public static class MethodBaseExtensions
    {
        public static IList<LocalVariable> GetLocalVariables(this MethodBase aThis)
        {
            return DebugSymbolReader.GetReader(aThis.DeclaringType.GetTypeInfo().Assembly.Location).GetLocalVariables(aThis.MetadataToken);
        }

        public static string GetLocalVariableName(this MethodBase aThis, int aIndex)
        {
            return DebugSymbolReader.GetReader(aThis.DeclaringType.GetTypeInfo().Assembly.Location).GetLocalVariableName(aThis.MetadataToken, aIndex);
        }

        public static MethodBodyBlock GetMethodBody(this MethodBase aThis)
        {
            var xReader = DebugSymbolReader.GetReader(aThis.DeclaringType.GetTypeInfo().Assembly.Location);

            return xReader.GetMethodBodyBlock(aThis.MetadataToken);
        }
    }
}
