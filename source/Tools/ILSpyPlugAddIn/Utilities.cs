using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Cosmos.ILSpyPlugs.Plugin
{
    public static class Utilities
    {
        public static string GetCSharpTypeName(TypeReference reference)
        {
            return reference.FullName;
        }
    }
}