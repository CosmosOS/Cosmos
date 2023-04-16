using Cosmos.Common.Extensions;
using Cosmos.Common;
using IL2CPU.API.Attribs;
using IL2CPU.API;

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
            return formating switch
            {
                "X" => ToHexString.ToHex(aThis, false),
                "G" => ToString(ref aThis),
                _ => throw new NotImplementedException(),
            };
        }
    }
}