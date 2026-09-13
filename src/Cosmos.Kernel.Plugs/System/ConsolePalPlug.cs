using System.Text;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System;
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
    [PlugMember]
    public static TextReader GetOrCreateReader()
    {
        if (KernelFeatures.Keyboard)
        {
            if (Console.IsInputRedirected || Console.InputEncoding != Encoding.Default)
            {
                Stream stream = OpenStandardInput();
                //TODO: Once lock keyword works, call 'TextReader.Syncronize' to get a thread save reader.
                return new StreamReader(stream, Console.InputEncoding, detectEncodingFromByteOrderMarks: false, 4096, leaveOpen: true);
            }

            return StdInReader;
        }
        else
        {
            return TextReader.Null;
        }
    }
}
