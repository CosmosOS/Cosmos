using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using Cosmos.IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics.Contracts
{
    //[Plug(TargetName = "System.Diagnostics.Contracts.Contract")]
    [Plug(typeof(Contract))]
    public static class ContractImpl
    {
        public static void Ensures(bool condition)
        {
        }

        public static T Result<T>() { return default(T); }
    }
}
