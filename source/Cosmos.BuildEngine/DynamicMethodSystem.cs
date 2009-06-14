using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Cosmos.BuildEngine
{
    public static class DynamicMethodSystem
    {
        public abstract class CDynamicMethodProvider
        {
            public abstract bool GetEmittsMethod(MethodBase methodbase);
            public abstract bool GetIsValidCacheFor(MethodBase methodbase, String CacheIdentifier);
        }
        public abstract class CDynamicMethodCILProvider : CDynamicMethodProvider
        {
            public abstract MethodBase GetEmittedMethod(MethodBase MethodBase, out String CacheIdentifier);
        }
        public abstract class CDynamicMethodASMProvider : CDynamicMethodProvider
        {
            public abstract CompilationResult GetEmittedMethod(MethodBase MethodBase, out String CacheIdentifier);
        }

        private static List<CDynamicMethodProvider> Providers = new List<CDynamicMethodProvider>();

        public static void Initialize()
        {
            //Register Providers Below
        }
        public static CompilationResult GetASMForMethod(MethodBase methodbase, out String CacheIdentifier)
        {
            foreach (CDynamicMethodASMProvider ASMProvider in (from Provider in Providers where Provider is CDynamicMethodASMProvider select (CDynamicMethodASMProvider)Provider))
            {
                if (ASMProvider.GetEmittsMethod(methodbase))
                {
                    return ASMProvider.GetEmittedMethod(methodbase, out CacheIdentifier);
                }
            }

            CacheIdentifier = null;

            return null;
        }
        public static MethodBase GetMBForMethod(MethodBase methodbase, out String CacheIdentifier)
        {
            foreach (CDynamicMethodCILProvider CILProvider in (from Provider in Providers where Provider is CDynamicMethodCILProvider select (CDynamicMethodCILProvider)Provider))
            {
                if (CILProvider.GetEmittsMethod(methodbase))
                {
                    return CILProvider.GetEmittedMethod(methodbase, out CacheIdentifier);
                }
            }

            CacheIdentifier = null;

            return null;
        }
    }
}