using System;
using System.Reflection;

namespace Cosmos.BuildEngine
{
    static class PlugUtils
    {
        public static String ResolveFullName(MethodBase methodbase)
        {
            return methodbase.DeclaringType.FullName + "." + methodbase.Name;
        }
    }
}