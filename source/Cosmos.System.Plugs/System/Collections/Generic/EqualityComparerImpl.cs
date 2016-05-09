using Cosmos.IL2CPU.Plugs;
using System;
using System.Collections.Generic;

namespace Cosmos.System.Plugs.System.Collections.Generic
{
    [Plug(Target = typeof(EqualityComparer<>))]
    public static class EqualityComparerImpl<T>
    {
        public static EqualityComparer<T> CreateComparer()
        {
            throw new NotImplementedException();
        }
    }
}
