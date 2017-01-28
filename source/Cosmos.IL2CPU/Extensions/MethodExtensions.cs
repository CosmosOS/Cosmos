using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Cosmos.Assembler;
using Cosmos.Debug.Symbols;

namespace Cosmos.IL2CPU.Extensions
{
    public static class MethodExtensions
    {
        public static string GetFullName(this MethodBase aMethod)
        {
            return LabelName.GetFullName(aMethod);
        }

        public static List<LocalVariableInfo> GetLocalVariables(this MethodBase aThis)
        {
            var xBody = DebugSymbolReader.GetMethodBodyBlock(aThis.Module, aThis.MetadataToken);
            return DebugSymbolReader.GetLocalVariableInfos(aThis.Module, xBody);
        }

        public static MethodBodyBlock GetMethodBody(this MethodBase aThis)
        {
            return DebugSymbolReader.GetMethodBodyBlock(aThis.Module, aThis.MetadataToken);
        }

    }
}
