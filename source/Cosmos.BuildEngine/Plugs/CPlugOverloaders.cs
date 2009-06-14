using System;
using System.Reflection;

namespace Cosmos.BuildEngine
{
    public abstract class CILPlugOverloader
    {
        public abstract MethodBase ResolvePlug(String PlatformName);
    }
    public abstract class ASMPlugOverloader
    {
        public abstract CompilationResult ResolvePlug(String PlatformName);
    }
}