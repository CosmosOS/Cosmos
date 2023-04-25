using IL2CPU.API.Attribs;
using System;

namespace Cosmos.Core_Plugs.System
{
    [Plug("System.Marvin, System.Private.CoreLib")]
    public static class MarvinImpl
    {
        // todo: maybe plug this at the RandomNumberGeneratorImplementation level
        public static ulong GenerateSeed()
        {
            var random = new Random();
            var buffer = new byte[8];

            random.NextBytes(buffer);

            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}