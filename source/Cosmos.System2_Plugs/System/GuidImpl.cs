using System;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Guid))]
    public class GuidImpl
    {
        public static Guid NewGuid()
        {
            Random rnd = new Random();
            var guid = new byte[16];
            rnd.NextBytes(guid);
            guid[6] = (byte) (0x40 | ((int) guid[6] & 0xf));
            guid[8] = (byte) (0x80 | ((int) guid[8] & 0x3f));

            return new Guid(guid);
        }
    }
}
