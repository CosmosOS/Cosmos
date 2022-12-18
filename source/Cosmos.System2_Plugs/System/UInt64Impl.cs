using System;

using Cosmos.Common;
using Cosmos.Common.Extensions;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(ulong))]
    public class UInt64Impl
    {
        public static string ToString(ref ulong aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static string ToString(ref ulong aThis, string formating)
        {
            if(formating == "X")
            {
                return ToHexString.ToHex(aThis, false);
            }
            else if(formating == "G")
            {
                return ToString(ref aThis);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
