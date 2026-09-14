// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.Intrinsics.X86;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL;

internal static class DebugInfo
{
    private const string Dash = " --- ";
    private const string NewLine = "\n";

    private static void PrintLine(string name, bool value)
    {
        Serial.Write(name, Dash, value, NewLine);
    }

    public static void Print()
    {
        PrintLine("X86Base", X86Base.IsSupported);
        PrintLine("Sse", Sse.IsSupported);
        PrintLine("Sse2", Sse2.IsSupported);
        PrintLine("Sse3", Sse3.IsSupported);
        PrintLine("Sse41", Sse41.IsSupported);
        PrintLine("Sse42", Sse42.IsSupported);
        PrintLine("Aes", Aes.IsSupported);
        PrintLine("Avx", Avx.IsSupported);
        PrintLine("Avx2", Avx2.IsSupported);

    }
}
