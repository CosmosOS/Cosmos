using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Cosmos.Assembler;
using Cosmos.Debug.Symbols;
using Cosmos.Debug.Symbols.Pdb;

namespace Cosmos.IL2CPU.Extensions
{
    public static class MethodExtensions
    {
        public static string GetFullName(this MethodBase aMethod)
        {
            return LabelName.Get(aMethod);
        }

        public static IList<ILLocalVariable> GetLocalVariables(this MethodBase aThis)
        {
            return DebugSymbolReader.GetLocalVariableInfos(aThis);
        }

        public static MethodBodyBlock GetMethodBody(this MethodBase aThis)
        {
            return DebugSymbolReader.GetMethodBodyBlock(aThis.Module, aThis.MetadataToken);
        }

        public static IEnumerable<_ExceptionRegionInfo> GetExceptionRegionInfos(this MethodBodyBlock aThis, Module aModule)
        {
            foreach (var x in aThis.ExceptionRegions)
            {
                yield return new _ExceptionRegionInfo(aModule, x);
            }
        }
    }
}
