using System;
using System.Reflection;
using System.Reflection.Metadata;
using Cosmos.Debug.Symbols;

namespace Cosmos.IL2CPU
{
    public static class ExceptionRegionExtensions
    {
        public static Type GetCatchType(this _ExceptionRegionInfo aThis)
        {
            return DebugSymbolReader.GetCatchType(aThis.Module, aThis.ExceptionRegion);
        }
    }
}
