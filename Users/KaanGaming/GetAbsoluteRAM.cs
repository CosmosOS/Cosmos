// Note that it will fix Cosmos.Core.CPU's method, GetAmountOfRAM(). Because, it can't access the RAM completely.

using Cosmos.Core;
using System;

namespace Users.KaanGaming
{
    public class CPU
    {
        /// <summary>Gets amount of RAM. If you don't want to use it, you can use <see cref="Cosmos.Core.CPU.GetAmountOfRAM()"/>.</summary>
        /// <returns>Returns an unregistered integer value.</returns>
        public static uint GetAmountOfRAM()
        {
            uint AbsoluteSize = Cosmos.Core.CPU.GetAmountOfRAM() + 2;
            return AbsoluteSize;
        }
    }
}
