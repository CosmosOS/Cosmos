using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Debug.Symbols
{
    public static class MethodBodyBlockExtensions
    {
        public static IList<LocalVariableInfo> GetLocalVariablesInfo(this MethodBodyBlock aThis)
        {
            throw new Exception("NetCore Fix Me");

            //TODO: If DebugSymbolReader.GetLocalVariablesInfo(MethodBodyBlock) is not used, remove it
        }
    }
}
