using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(decimal))]
    public static class DecimalImpl
    {
        public static int GetHashCode(ref decimal aThis)
        {
            throw new NotImplementedException("Decimal.GetHashCode()");
        }
    }
}
