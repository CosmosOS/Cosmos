using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.ILSpy;
using Mono.Cecil;

namespace Cosmos.ILSpyPlugs.Plugin
{
    public static class Utilities
    {
        public static string GetCSharpTypeName(TypeReference reference)
        {
            var xCSharp = Languages.GetLanguage("C#");

            return xCSharp.TypeToString(reference, true);
        }

        public static string GetMethodName(MethodDefinition method)
        {
            if (method.IsConstructor)
            {
                if (method.IsStatic)
                {
                    return "Cctor";
                }
                else
                {
                    return "Ctor";
                }
            }
            return method.Name;
        }
    }
}