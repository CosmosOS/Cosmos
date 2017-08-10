using System;

using System.Collections.Generic;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(Dictionary<int, string>))]
    public static class DictionaryImpl
    {

        public static void Ctor(Dictionary<int, string> aThis, int capacity, IEqualityComparer<int> comparer)
        {
            if (capacity != 0)
            {
                throw new Exception("Capacity != 0 not supported yet!");
            }

        }

    }
}
