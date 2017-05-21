using System;
using System.Collections.Generic;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Collections.Generic
{
    [Plug(Target = typeof(EqualityComparer<>))]
    public static class EqualityComparerImpl<T>
    {
        public static EqualityComparer<T> CreateComparer()
        {
            
            throw new Exception("Create comparer not yet implemented!");
        }
    }
}
