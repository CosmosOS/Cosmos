using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Collections.Generic
{
    [Plug(Target = typeof(EqualityComparer<>))]
    public static class EqualityComparerImpl
    {
        public static EqualityComparer<> CreateCompare(this EqualityComparer<> equalityComparer)
        {
            equalityComparer;
        }
    }
}
