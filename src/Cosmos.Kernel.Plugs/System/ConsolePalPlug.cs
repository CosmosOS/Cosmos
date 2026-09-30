using System.Text;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.IO;

namespace Cosmos.Kernel.Plugs.System;

[Plug("System.ConsolePal")]
public class ConsolePalPlug
{
    [PlugMember]
    public static Encoding GetConsoleEncoding()
    {
        return Encoding.Default;
    }

    [PlugMember]
    public static Stream OpenStandardOutput()
    {
        return new ConsoleStream(FileAccess.Write);
    }

    [PlugMember]
    public static Stream OpenStandardError()
    {
        return new ConsoleStream(FileAccess.Write);
    }

    [PlugMember]
    public static Stream OpenStandardInput()
    {
        return new ConsoleStream(FileAccess.Read);
    }

    [PlugMember]
    public static void EnsureInitializedCore()
    {
        if (!KernelConsole.IsInitialized)
        {
            KernelConsole.Initialize();
        }
    }

    [PlugMember]
    public static bool IsErrorRedirectedCore()
    {
        return false;
    }

    [PlugMember]
    public static bool IsInputRedirectedCore()
    {
        return false;
    }

    [PlugMember]
    public static bool IsOutputRedirectedCore()
    {
        return false;
    }

    private static KeyboardTextReader StdInReader => field ??= new();

    /// <summary>
    /// Standard input, which reads the calling thread's console session. It
    /// is made whether or not keyboard support is compiled in: a remote
    /// session reads the network, and a local one with no keyboard reads as
    /// the end of input.
    /// </summary>
    [PlugMember]
    public static TextReader GetOrCreateReader()
    {
        if (Console.IsInputRedirected || Console.InputEncoding != Encoding.Default)
        {
            Stream stream = OpenStandardInput();
            // TODO: Once the lock keyword works, call 'TextReader.Synchronized' to get a thread-safe reader.
            return new StreamReader(stream, Console.InputEncoding, detectEncodingFromByteOrderMarks: false, 4096, leaveOpen: true);
        }

        return StdInReader;
    }
}
