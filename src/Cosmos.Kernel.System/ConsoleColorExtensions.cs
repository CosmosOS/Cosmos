// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System
{
    internal static class ConsoleColorExtensions
    {
        extension(ConsoleColor color)
        {
            public static ConsoleColor Custom => (ConsoleColor)16;
        }
    }
}
